using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using OpcUaViewer.Core.Contracts;
using OpcUaViewer.Core.Services;
using OpcUaViewer.Core.Settings;

namespace OpcUaViewer.Plugin.CsvSource;

/// <summary>
/// IMachineSource backed by the Schroder daily ProductionLog CSV file.
/// Watches the file with FileSystemWatcher and tails new rows in real time.
/// File format (semicolon-delimited):
///   DateTime ; Status ; Duration(s) ; User ; PartNumber ; Mode
/// </summary>
public class CsvMachineSource : ViewModelBase, IMachineSource, ICycleSource, ISettingsPanel, IDisposable
{
    // ── IDataSource ───────────────────────────────────────────────────────────
    public string Name        => "CSV Source";
    public string Description => "Reads production data from Schroder CSV log files (for machines without full OPC UA support).";

    // ── IMachineSource ────────────────────────────────────────────────────────
    public bool   IsConnected { get => _isConnected; private set => Set(ref _isConnected, value); }
    public string StatusText  { get => _statusText;  private set => Set(ref _statusText, value); }

    public event EventHandler<string>?                    StatusChanged;
    public event EventHandler<IReadOnlyList<TagInfo>>?    TagsDiscovered;
    public event EventHandler<TagValueEventArgs>?         TagValueUpdated;
    public event EventHandler<string>?                    ProductIdChanged;
    public event EventHandler<string>?                    CamFileChanged;
    public event EventHandler<int>?                       MachineStateChanged;
    public event EventHandler<bool>?                      OperatorActionChanged;
    public event EventHandler<CycleCompletedEventArgs>?   CycleCompleted;

    // ── ISettingsPanel ────────────────────────────────────────────────────────
    public string Header => "CSV Source";
    System.Windows.FrameworkElement ISettingsPanel.CreateView() => new CsvSourceSettingsView { DataContext = this };
    public void Save()
    {
        AppSettings.Current.CsvLogFolderPath   = LogFolderPath.Trim();
        AppSettings.Current.CsvHistoryDaysBack  = HistoryDaysBack;
        AppSettings.Save();
        RestartWatcher();
    }

    // ── ViewModel surface ─────────────────────────────────────────────────────
    private bool   _isConnected;
    private string _statusText      = "Not watching";
    private string _logFolderPath;
    private int    _historyDaysBack;

    public string LogFolderPath
    {
        get => _logFolderPath;
        set => Set(ref _logFolderPath, value);
    }

    public int HistoryDaysBack
    {
        get => _historyDaysBack;
        set => Set(ref _historyDaysBack, Math.Max(0, Math.Min(365, value)));
    }

    // ── State ─────────────────────────────────────────────────────────────────
    private FileSystemWatcher? _watcher;
    private string?            _currentFile;
    private long               _readPosition;
    private string             _lastProductId = "";
    private readonly object    _lock          = new();
    private System.Threading.Timer? _pollTimer;

    private static readonly string ImportLogPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "OpcUaViewer", "csv_import_log.json");

    public CsvMachineSource()
    {
        _logFolderPath   = AppSettings.Current.CsvLogFolderPath;
        _historyDaysBack = AppSettings.Current.CsvHistoryDaysBack;
    }

    public void OnApplicationStarted()
    {
        ImportHistory();
        RestartWatcher();
    }

    // ── Historical import ─────────────────────────────────────────────────────

    private void ImportHistory()
    {
        string folder = LogFolderPath.Trim();
        if (string.IsNullOrEmpty(folder) || !Directory.Exists(folder) || HistoryDaysBack <= 0) return;

        var imported = LoadImportLog();
        var store    = StatsStore.Load();
        var records  = new List<(string JobKey, string ProductKey, double Seconds)>();
        var newFiles = new List<string>();

        for (int d = HistoryDaysBack; d >= 1; d--)
        {
            var date     = DateTime.Today.AddDays(-d);
            string fname = $"{date:yyyy-MM-dd}_ProductionLog.csv";
            string path  = Path.Combine(folder, fname);

            if (!File.Exists(path) || imported.Contains(fname)) continue;

            try
            {
                using var fs     = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var reader = new StreamReader(fs);
                string jobKey    = date.ToString("yyyy-MM-dd");
                string? line;

                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    var parts = line.Split(';');
                    if (parts.Length < 5) continue;

                    string status  = parts[1].Trim().Trim('"');
                    string durStr  = parts[2].Trim().Trim('"');
                    string partNum = parts[4].Trim().Trim('"');

                    if (!status.Equals("Completed", StringComparison.OrdinalIgnoreCase)) continue;
                    if (string.IsNullOrWhiteSpace(partNum)) continue;
                    if (!double.TryParse(durStr,
                            System.Globalization.NumberStyles.Any,
                            System.Globalization.CultureInfo.InvariantCulture,
                            out double seconds)) continue;

                    string productKey = StatsStore.ProductKey(partNum);
                    records.Add((jobKey, productKey, seconds));
                }

                newFiles.Add(fname);
                AppLogger.Info($"CsvMachineSource: queued history from {fname} ({records.Count} records so far)");
            }
            catch (Exception ex)
            {
                AppLogger.Error($"CsvMachineSource: error reading history file {fname}", ex);
            }
        }

        if (records.Count > 0)
        {
            store.BulkImportCycles(records);
            AppLogger.Info($"CsvMachineSource: imported {records.Count} historical cycles from {newFiles.Count} file(s)");
        }

        if (newFiles.Count > 0)
            SaveImportLog(imported, newFiles);
    }

    private static HashSet<string> LoadImportLog()
    {
        try
        {
            if (File.Exists(ImportLogPath))
            {
                var list = System.Text.Json.JsonSerializer.Deserialize<List<string>>(
                    File.ReadAllText(ImportLogPath));
                return new HashSet<string>(list ?? [], StringComparer.OrdinalIgnoreCase);
            }
        }
        catch { }
        return [];
    }

    private static void SaveImportLog(HashSet<string> existing, IEnumerable<string> add)
    {
        try
        {
            foreach (var f in add) existing.Add(f);
            Directory.CreateDirectory(Path.GetDirectoryName(ImportLogPath)!);
            File.WriteAllText(ImportLogPath,
                System.Text.Json.JsonSerializer.Serialize(existing.ToList()));
        }
        catch { }
    }

    // ── Watcher lifecycle ─────────────────────────────────────────────────────

    private void RestartWatcher()
    {
        lock (_lock)
        {
            _watcher?.Dispose();
            _watcher       = null;
            _pollTimer?.Dispose();
            _pollTimer     = null;
            _currentFile   = null;
            _readPosition  = 0;
            _lastProductId = "";
        }

        string folder = LogFolderPath.Trim();
        if (string.IsNullOrEmpty(folder))
        {
            SetStatus(false, "No log folder configured");
            return;
        }
        if (!Directory.Exists(folder))
        {
            SetStatus(false, $"Folder not found: {folder}");
            return;
        }

        AttachToTodaysFile(folder);

        var watcher = new FileSystemWatcher(folder, "*.csv")
        {
            NotifyFilter          = NotifyFilters.LastWrite | NotifyFilters.FileName,
            EnableRaisingEvents   = true,
        };
        watcher.Changed += (_, e) => OnFileChanged(e.FullPath);
        watcher.Created += (_, e) => OnFileChanged(e.FullPath);
        _watcher = watcher;

        // Poll every 30 s in case FileSystemWatcher misses events (network share)
        _pollTimer = new System.Threading.Timer(_ => PollCurrentFile(), null,
            TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));
    }

    private void AttachToTodaysFile(string folder)
    {
        string expected = Path.Combine(folder,
            $"{DateTime.Today:yyyy-MM-dd}_ProductionLog.csv");

        lock (_lock)
        {
            if (!File.Exists(expected))
            {
                SetStatus(false, $"Waiting for today's log: {Path.GetFileName(expected)}");
                return;
            }

            _currentFile  = expected;
            _readPosition = 0;
            // Skip to end — don't replay old completions from today on startup
            try
            {
                using var f = new FileStream(expected, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                _readPosition = f.Length;
            }
            catch { }
        }

        SetStatus(true, $"Watching {Path.GetFileName(expected)}");
    }

    private void OnFileChanged(string path)
    {
        if (!path.EndsWith("_ProductionLog.csv", StringComparison.OrdinalIgnoreCase)) return;

        lock (_lock)
        {
            // If the date rolled over, attach to the new file from the top
            if (_currentFile == null ||
                !string.Equals(_currentFile, path, StringComparison.OrdinalIgnoreCase))
            {
                _currentFile  = path;
                _readPosition = 0;
                Dispatch(() => SetStatus(true, $"Watching {Path.GetFileName(path)}"));
            }
        }

        ReadNewRows(path);
    }

    private void PollCurrentFile()
    {
        string? file;
        lock (_lock) { file = _currentFile; }
        if (file != null) ReadNewRows(file);

        // Also check if today's file appeared (after midnight rollover)
        string folder = LogFolderPath.Trim();
        if (!Directory.Exists(folder)) return;
        string todaysFile = Path.Combine(folder,
            $"{DateTime.Today:yyyy-MM-dd}_ProductionLog.csv");
        if (file == null && File.Exists(todaysFile))
            AttachToTodaysFile(folder);
    }

    // ── CSV parsing ───────────────────────────────────────────────────────────

    private void ReadNewRows(string path)
    {
        try
        {
            using var fs     = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            long currentLen  = fs.Length;

            long startPos;
            lock (_lock)
            {
                if (_readPosition > currentLen) _readPosition = 0; // file was truncated/replaced
                startPos      = _readPosition;
                _readPosition = currentLen;
            }

            if (startPos >= currentLen) return;

            fs.Seek(startPos, SeekOrigin.Begin);
            using var reader = new StreamReader(fs);

            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                ParseRow(line);
            }
        }
        catch (Exception ex)
        {
            AppLogger.Error("CsvMachineSource: error reading log", ex);
        }
    }

    private void ParseRow(string line)
    {
        // Format: DateTime;Status;Duration(s);User;PartNumber;Mode;
        var parts = line.Split(';');
        if (parts.Length < 5) return;

        string status   = parts[1].Trim().Trim('"');
        string duration = parts[2].Trim().Trim('"');
        string partNum  = parts[4].Trim().Trim('"');

        if (!DateTime.TryParse(parts[0].Trim().Trim('"'), out var timestamp))
            timestamp = DateTime.Now;

        if (status.Equals("Completed", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(partNum)) return;

            // Fire explicit cycle event with exact duration from CSV
            if (double.TryParse(duration,
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out double cycleSeconds))
            {
                string pn = partNum;
                Dispatch(() => CycleCompleted?.Invoke(this, new CycleCompletedEventArgs(pn, cycleSeconds)));
            }

            // Only fire ProductIdChanged when the part actually changes (for Document/Groups tabs)
            if (partNum != _lastProductId)
            {
                _lastProductId = partNum;
                string captured = partNum;
                Dispatch(() => ProductIdChanged?.Invoke(this, captured));
            }
        }
        else if (status.Equals("Pause", StringComparison.OrdinalIgnoreCase))
        {
            if (!string.IsNullOrEmpty(_lastProductId))
            {
                _lastProductId = "";
                Dispatch(() => ProductIdChanged?.Invoke(this, ""));
            }
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private void SetStatus(bool connected, string text)
    {
        IsConnected = connected;
        StatusText  = text;
        StatusChanged?.Invoke(this, text);
    }

    public void Dispose()
    {
        _watcher?.Dispose();
        _pollTimer?.Dispose();
    }

    private static void Dispatch(Action a)
    {
        if (System.Windows.Application.Current?.Dispatcher.CheckAccess() == true) a();
        else System.Windows.Application.Current?.Dispatcher.BeginInvoke(a);
    }
}
