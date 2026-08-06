using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using OpcUaViewer.Core.Contracts;
using OpcUaViewer.Core.Services;
using OpcUaViewer.Core.Settings;

namespace OpcUaViewer.Plugin.Stats;

public class StatsTab : ViewModelBase, IAppTab
{
    public string Title       => "Stats";
    public string Icon        => "📊";
    public int    Order       => 15;
    public string Description => "Production statistics: machine modes, cycle times, and operating hours.";

    public FrameworkElement CreateView() => new StatsView { DataContext = this };

    // ── Mode badges ───────────────────────────────────────────────────────────
    private bool _isAuto, _isManual, _isSetup, _isBending, _isOperatorWait;
    public bool IsAuto        { get => _isAuto;         private set => Set(ref _isAuto, value); }
    public bool IsManual      { get => _isManual;       private set => Set(ref _isManual, value); }
    public bool IsSetup       { get => _isSetup;        private set => Set(ref _isSetup, value); }
    public bool IsBending     { get => _isBending;      private set => Set(ref _isBending, value); }
    public bool IsOperatorWait { get => _isOperatorWait; private set => Set(ref _isOperatorWait, value); }

    // ── Live timers ───────────────────────────────────────────────────────────
    private string _liveSetup = "—", _livePart = "—";
    public string LiveSetup { get => _liveSetup; private set => Set(ref _liveSetup, value); }
    public string LivePart  { get => _livePart;  private set => Set(ref _livePart, value); }

    // ── Metric cards ──────────────────────────────────────────────────────────
    private string _totalHours = "—", _producingHours = "—", _efficiency = "—";
    private string _totalParts = "—", _totalBends = "—";
    public string TotalHours     { get => _totalHours;     private set => Set(ref _totalHours, value); }
    public string ProducingHours { get => _producingHours; private set => Set(ref _producingHours, value); }
    public string Efficiency     { get => _efficiency;     private set => Set(ref _efficiency, value); }
    public string TotalParts     { get => _totalParts;     private set => Set(ref _totalParts, value); }
    public string TotalBends     { get => _totalBends;     private set => Set(ref _totalBends, value); }

    // ── Parts grid ────────────────────────────────────────────────────────────
    public ObservableCollection<PartStatsVm> Parts { get; } = [];

    private string _activeProductKey = "";
    public string ActiveProductKey
    {
        get => _activeProductKey;
        private set
        {
            Set(ref _activeProductKey, value);
            foreach (var p in Parts) p.IsActive = p.Key == value;
        }
    }

    // ── Commands ──────────────────────────────────────────────────────────────
    public RelayCommand ResetHoursCommand { get; }
    public RelayCommand ResetPartsCommand { get; }
    public RelayCommand ResetBendsCommand { get; }

    // ── Internal state ────────────────────────────────────────────────────────
    private StatsStore _store;
    private string   _currentJobKey     = "";
    private string   _currentProductKey = "";
    private DateTime _productStartTime  = DateTime.MinValue;
    private DateTime _setupStartTime    = DateTime.MinValue;
    private bool     _inSetupPhase;
    private bool     _useExplicitCycles; // true when source implements ICycleSource
    private double   _rawTotalSecs, _rawProducingSecs;

    // ── Construction ──────────────────────────────────────────────────────────

    public StatsTab()
    {
        _store = StatsStore.Load();

        ResetHoursCommand = new RelayCommand(ResetHours);
        ResetPartsCommand = new RelayCommand(ResetParts);
        ResetBendsCommand = new RelayCommand(ResetBends);

        TotalParts = _store.TotalPartCount > 0 ? _store.TotalPartCount.ToString("N0") : "—";
        TotalBends = _store.TotalBendCount > 0 ? _store.TotalBendCount.ToString("N0") : "—";
        RefreshGrid();

        var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
        timer.Tick += (_, _) => TickLiveTimers();
        timer.Start();

        var src = DataSourceRegistry.Get<IMachineSource>();
        if (src is null) return;

        src.MachineStateChanged   += (_, s) => Dispatch(() => OnMachineStateChanged(s));
        src.OperatorActionChanged += (_, w) => Dispatch(() => IsOperatorWait = w);
        src.CamFileChanged        += (_, f) => Dispatch(() => OnCamFileChanged(f));
        src.ProductIdChanged      += (_, p) => Dispatch(() => OnProductIdChanged(p));
        src.TagValueUpdated       += (_, e) => Dispatch(() => OnTagValueUpdated(e));

        if (src is ICycleSource cycSrc)
        {
            _useExplicitCycles = true;
            cycSrc.CycleCompleted += (_, e) => Dispatch(() => OnCycleCompleted(e));
        }

        StatsStore.BulkImportCompleted += (_, _) => Dispatch(ReloadStore);
    }

    // ── Event handlers ────────────────────────────────────────────────────────

    private void OnMachineStateChanged(int state)
    {
        var s = AppSettings.Current;
        IsAuto    = state == s.MachineStateAuto;
        IsManual  = state == s.MachineStateManual;
        IsSetup   = state == s.MachineStateSetup;
        IsBending = state == s.MachineStateBending;

        if (IsSetup && !_inSetupPhase)
        {
            _inSetupPhase   = true;
            _setupStartTime = DateTime.UtcNow;
        }
        else if (!IsSetup && _inSetupPhase)
        {
            _inSetupPhase = false;
        }
    }

    private void OnCamFileChanged(string file)
    {
        _currentJobKey     = StatsStore.JobKey(file);
        _currentProductKey = "";
        ActiveProductKey   = "";
        RefreshGrid();
    }

    private void OnCycleCompleted(CycleCompletedEventArgs e)
    {
        string productKey = StatsStore.ProductKey(e.PartNumber);
        _store.AddCycleTime(_currentJobKey, productKey, e.CycleSeconds);
        TotalParts = _store.TotalPartCount.ToString("N0");
        RefreshGridRow(productKey);
    }

    private void OnProductIdChanged(string productId)
    {
        var newKey = StatsStore.ProductKey(productId);

        // When source provides explicit cycle events, skip elapsed-time recording here
        if (!_useExplicitCycles &&
            !string.IsNullOrEmpty(_currentProductKey) && _productStartTime != DateTime.MinValue)
        {
            double t = (DateTime.UtcNow - _productStartTime).TotalSeconds;
            _store.AddCycleTime(_currentJobKey, _currentProductKey, t);
            TotalParts = _store.TotalPartCount.ToString("N0");
            RefreshGridRow(_currentProductKey);
        }

        // Record setup time when transitioning to a new product
        if (_inSetupPhase && _setupStartTime != DateTime.MinValue && !string.IsNullOrEmpty(newKey))
        {
            double st = (DateTime.UtcNow - _setupStartTime).TotalSeconds;
            if (st > 1)
            {
                _store.AddSetupTime(_currentJobKey, newKey, st);
                RefreshGridRow(newKey);
            }
            _setupStartTime = DateTime.MinValue;
            _inSetupPhase   = false;
        }

        _currentProductKey = newKey;
        _productStartTime  = string.IsNullOrEmpty(newKey) ? DateTime.MinValue : DateTime.UtcNow;
        ActiveProductKey   = newKey;
    }

    private void ReloadStore()
    {
        _store = StatsStore.Load();
        TotalParts = _store.TotalPartCount > 0 ? _store.TotalPartCount.ToString("N0") : "—";
        TotalBends = _store.TotalBendCount > 0 ? _store.TotalBendCount.ToString("N0") : "—";
        RefreshGrid();
    }

    private void OnTagValueUpdated(TagValueEventArgs e)
    {
        var s = AppSettings.Current;
        string n = e.Name;

        if (!string.IsNullOrEmpty(s.TotalHoursTagMatch) &&
            n.Contains(s.TotalHoursTagMatch, StringComparison.OrdinalIgnoreCase) &&
            double.TryParse(e.StrValue, out double total))
        {
            _rawTotalSecs = total * 3600;
            RefreshHours();
        }
        else if (!string.IsNullOrEmpty(s.ProducingHoursTagMatch) &&
                 n.Contains(s.ProducingHoursTagMatch, StringComparison.OrdinalIgnoreCase) &&
                 double.TryParse(e.StrValue, out double prod))
        {
            _rawProducingSecs = prod * 3600;
            RefreshHours();
        }
        else if (!string.IsNullOrEmpty(s.TotalBendsTagMatch) &&
                 n.Contains(s.TotalBendsTagMatch, StringComparison.OrdinalIgnoreCase) &&
                 int.TryParse(e.StrValue, out int bends))
        {
            TotalBends = bends.ToString("N0");
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private void RefreshHours()
    {
        var (t, p) = _store.ApplyBaseline(_rawTotalSecs, _rawProducingSecs);
        TotalHours     = FormatHours(t / 3600.0);
        ProducingHours = FormatHours(p / 3600.0);
        Efficiency     = t > 0 ? $"{p / t * 100.0:F1}%" : "—";
    }

    private void ResetHours()
    {
        if (!DialogService.Current.Confirm(
            "Zero out the hour and efficiency displays?\n\nThe machine's actual counters are not affected.\nThis offset is saved and applied on every restart.",
            "Reset Operating Hours")) return;
        _store.ResetHours(_rawTotalSecs, _rawProducingSecs);
        TotalHours     = FormatHours(0);
        ProducingHours = FormatHours(0);
        Efficiency     = "0.0%";
    }

    private void ResetParts()
    {
        if (!DialogService.Current.Confirm("Reset total part count to zero?", "Reset Parts")) return;
        _store.ResetPartCount();
        TotalParts = "0";
    }

    private void ResetBends()
    {
        if (!DialogService.Current.Confirm("Reset total bend count to zero?", "Reset Bends")) return;
        _store.ResetBendCount();
        TotalBends = "0";
    }

    private void TickLiveTimers()
    {
        LiveSetup = _inSetupPhase && _setupStartTime != DateTime.MinValue
            ? "Setup: " + FormatSeconds((DateTime.UtcNow - _setupStartTime).TotalSeconds)
            : "—";

        LivePart = !_inSetupPhase && _productStartTime != DateTime.MinValue && !string.IsNullOrEmpty(_currentProductKey)
            ? "Part: " + FormatSeconds((DateTime.UtcNow - _productStartTime).TotalSeconds)
            : "—";
    }

    private void RefreshGrid()
    {
        Parts.Clear();
        foreach (var key in _store.AllProductKeys.OrderBy(k => k))
            Parts.Add(BuildPartVm(key));
    }

    private void RefreshGridRow(string key)
    {
        var idx = -1;
        for (int i = 0; i < Parts.Count; i++)
            if (Parts[i].Key == key) { idx = i; break; }

        var vm = BuildPartVm(key);
        if (idx >= 0) Parts[idx] = vm;
        else          Parts.Add(vm);
    }

    private PartStatsVm BuildPartVm(string key)
    {
        var cycles = _store.GetProductCycleTimes(key);
        var setups = _store.GetProductSetupTimes(key);
        var bends  = _store.GetProductBendingTimes(key);
        return new PartStatsVm
        {
            Key       = key,
            Part      = key,
            IsActive  = key == _activeProductKey,
            Count     = cycles.Count > 0 ? cycles.Count.ToString()         : "—",
            LastCycle = cycles.Count > 0 ? FormatSeconds(cycles[^1])       : "—",
            AvgCycle  = cycles.Count > 0 ? FormatSeconds(cycles.Average()) : "—",
            MinCycle  = cycles.Count > 0 ? FormatSeconds(cycles.Min())     : "—",
            MaxCycle  = cycles.Count > 0 ? FormatSeconds(cycles.Max())     : "—",
            LastSetup = setups.Count > 0 ? FormatSeconds(setups[^1])       : "—",
            AvgSetup  = setups.Count > 0 ? FormatSeconds(setups.Average()) : "—",
            AvgBend   = bends.Count  > 0 ? FormatSeconds(bends.Average())  : "—",
        };
    }

    private static string FormatHours(double h) =>
        h < 0 ? "—" : $"{(int)h}h {(int)((h - (int)h) * 60):D2}m";

    private static string FormatSeconds(double s) =>
        s >= 60 ? $"{(int)(s / 60)}m {(int)(s % 60):D2}s" : $"{s:F1}s";

    private static void Dispatch(Action a)
    {
        if (Application.Current?.Dispatcher.CheckAccess() == true) a();
        else Application.Current?.Dispatcher.BeginInvoke(a);
    }
}

public class PartStatsVm : ViewModelBase
{
    public string Key  { get; init; } = "";
    public string Part { get; init; } = "";

    private bool _isActive;
    public bool IsActive { get => _isActive; set => Set(ref _isActive, value); }

    public string Count     { get; init; } = "—";
    public string LastCycle { get; init; } = "—";
    public string AvgCycle  { get; init; } = "—";
    public string MinCycle  { get; init; } = "—";
    public string MaxCycle  { get; init; } = "—";
    public string LastSetup { get; init; } = "—";
    public string AvgSetup  { get; init; } = "—";
    public string AvgBend   { get; init; } = "—";
}
