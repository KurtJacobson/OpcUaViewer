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
    private readonly IOpcUaSource? _src;
    private bool   _isConnected;
    private string _statusText = "Disconnected";

    public string Title    => "Monitor";
    public string Icon     => "📡";
    public int    Order    => 10;

    public bool   IsConnected { get => _isConnected; private set => Set(ref _isConnected, value); }
    public string StatusText  { get => _statusText;  private set => Set(ref _statusText, value); }

    public ObservableCollection<MonitoredNodeVm> Nodes { get; } = [];

    private readonly Dictionary<string, MonitoredNodeVm> _nodesByName = new();

    public FrameworkElement CreateView() => new MonitorView { DataContext = this };

    public MonitorTab()
    {
        _src = DataSourceRegistry.Get<IOpcUaSource>();
        if (_src is null) return;

        StatusText  = _src.StatusText;
        IsConnected = _src.IsConnected;

        _src.StatusChanged    += (_, msg)   => Dispatch(() => StatusText = msg);
        _src.NodesDiscovered  += (_, nodes) => Dispatch(() => RebuildGrid(nodes));
        _src.NodeValueUpdated += (_, e)     => Dispatch(() => UpdateNode(e));
    }

    private void RebuildGrid(IReadOnlyList<MonitoredNodeInfo> nodes)
    {
        Nodes.Clear();
        _nodesByName.Clear();
        foreach (var n in nodes)
        {
            var vm = new MonitoredNodeVm { Name = n.Name, NodeId = n.NodeIdStr };
            Nodes.Add(vm);
            _nodesByName[n.Name] = vm;
        }
        IsConnected = true;
    }

    private void UpdateNode(NodeValueEventArgs e)
    {
        if (!_nodesByName.TryGetValue(e.Name, out var vm)) return;
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

public class MonitoredNodeVm : ViewModelBase
{
    private string _value      = "-";
    private string _statusCode = "";
    private string _timestamp  = "";

    public string Name   { get; init; } = "";
    public string NodeId { get; init; } = "";

    public string Value      { get => _value;      set => Set(ref _value, value); }
    public string StatusCode { get => _statusCode; set => Set(ref _statusCode, value); }
    public string Timestamp  { get => _timestamp;  set => Set(ref _timestamp, value); }
}
