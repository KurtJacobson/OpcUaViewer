using System;
using System.IO;

namespace OpcUaViewer.Core.Services;

public static class AppLogger
{
    private static readonly object _lock = new();
    private static string? _logDir;

    public static void Initialize(string logDir)
    {
        _logDir = logDir;
        Directory.CreateDirectory(logDir);
        PurgeOldLogs(logDir, keepDays: 30);
    }

    public static void Info(string message)  => Write("INFO ", message);
    public static void Warn(string message)  => Write("WARN ", message);
    public static void Error(string message, Exception? ex = null)
    {
        Write("ERROR", message);
        if (ex is not null) Write("ERROR", ex.ToString());
    }

    private static void Write(string level, string message)
    {
        if (_logDir is null) return;
        string path = Path.Combine(_logDir, $"{DateTime.Today:yyyy-MM-dd}.log");
        string line = $"{DateTime.Now:HH:mm:ss.fff} [{level}] {message}";
        lock (_lock)
        {
            try { File.AppendAllText(path, line + Environment.NewLine); }
            catch { /* never throw from logger */ }
        }
    }

    private static void PurgeOldLogs(string dir, int keepDays)
    {
        var cutoff = DateTime.Today.AddDays(-keepDays);
        foreach (var file in Directory.GetFiles(dir, "????-??-??.log"))
        {
            if (DateTime.TryParse(Path.GetFileNameWithoutExtension(file), out var d) && d < cutoff)
            {
                try { File.Delete(file); } catch { }
            }
        }
    }
}
