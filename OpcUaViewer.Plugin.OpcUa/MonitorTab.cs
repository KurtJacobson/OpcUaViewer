using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using OpcUaViewer.Core.Contracts;
using OpcUaViewer.Core.Services;
using OpcUaViewer.Plugin.OpcUa.Views;

namespace OpcUaViewer.Plugin.OpcUa;

public class MonitorTab : ViewModelBase, IAppTab
{
    private readonly IMachineSource? _src;
    private bool   _isConnected;
    private string _statusText = "Disconnected";

    public string Title    => "Monitor";
    public string Icon     => "📡";
    public int    Order    => 10;

    public bool   IsConnected { get => _isConnected; private set => Set(ref _isConnected, value); }
    public string StatusText  { get => _statusText;  private set => Set(ref _statusText, value); }

    public ObservableCollection<MonitoredTagVm> Tags { get; } = [];

    private readonly Dictionary<string, MonitoredTagVm> _tagsByName = new();

    public FrameworkElement CreateView() => new MonitorView { DataContext = this };

    public MonitorTab()
    {
        _src = DataSourceRegistry.Get<IMachineSource>();
        if (_src is null) return;

        StatusText  = _src.StatusText;
        IsConnected = _src.IsConnected;

        _src.StatusChanged   += (_, msg)  => Dispatch(() => StatusText = msg);
        _src.TagsDiscovered  += (_, tags) => Dispatch(() => RebuildGrid(tags));
        _src.TagValueUpdated += (_, e)    => Dispatch(() => UpdateTag(e));
    }

    private void RebuildGrid(IReadOnlyList<TagInfo> tags)
    {
        Tags.Clear();
        _tagsByName.Clear();
        foreach (var t in tags)
        {
            var vm = new MonitoredTagVm { Name = t.Name, Address = t.Address };
            Tags.Add(vm);
            _tagsByName[t.Name] = vm;
        }
        IsConnected = true;
    }

    private void UpdateTag(TagValueEventArgs e)
    {
        if (!_tagsByName.TryGetValue(e.Name, out var vm)) return;
        vm.Value      = e.StrValue;
        vm.StatusCode = e.StatusCode;
        vm.Timestamp  = e.Timestamp.ToLocalTime().ToString("HH:mm:ss.fff");
    }

    private static void Dispatch(Action a)
    {
        if (Application.Current?.Dispatcher.CheckAccess() == true) a();
        else Application.Current?.Dispatcher.BeginInvoke(a);
    }
}

public class MonitoredTagVm : ViewModelBase
{
    private string _value      = "-";
    private string _statusCode = "";
    private string _timestamp  = "";

    public string Name    { get; init; } = "";
    public string Address { get; init; } = "";

    public string Value      { get => _value;      set => Set(ref _value, value); }
    public string StatusCode { get => _statusCode; set => Set(ref _statusCode, value); }
    public string Timestamp  { get => _timestamp;  set => Set(ref _timestamp, value); }
}
