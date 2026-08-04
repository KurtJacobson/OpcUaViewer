using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows;
using OpcUaViewer.Core.Contracts;
using OpcUaViewer.Core.Services;
using OpcUaViewer.Core.Settings;
using OpcUaViewer.Wpf.Dialogs;
using OpcUaViewer.Wpf.Infrastructure;

namespace OpcUaViewer.Wpf.Tabs;

public class MonitorTab : ViewModelBase, IAppTab
{
    public string Title => "Monitor";
    public string Icon  => "📡";
    public int    Order => 10;

    private readonly OpcUaService _opc;
    private bool   _isConnected;
    private bool   _isBusy;
    private string _statusText  = "Disconnected";
    private string _endpointUrl;

    public bool IsConnected
    {
        get => _isConnected;
        private set => Set(ref _isConnected, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set => Set(ref _isBusy, value);
    }

    public string StatusText
    {
        get => _statusText;
        private set => Set(ref _statusText, value);
    }

    public string EndpointUrl
    {
        get => _endpointUrl;
        set => Set(ref _endpointUrl, value);
    }

    public ObservableCollection<MonitoredNodeVm> Nodes { get; } = [];

    public RelayCommand ConnectCommand    { get; }
    public RelayCommand DisconnectCommand { get; }

    // Indexed by node name for O(1) value updates
    private readonly System.Collections.Generic.Dictionary<string, MonitoredNodeVm> _nodesByName = new();

    public MonitorTab(OpcUaService opc)
    {
        _opc         = opc;
        _endpointUrl = AppSettings.Current.EndpointUrl;

        ConnectCommand    = new RelayCommand(ConnectAsync,  () => !IsConnected && !IsBusy);
        DisconnectCommand = new RelayCommand(Disconnect,    () => IsConnected);

        _opc.StatusChanged    += (_, msg) => Dispatch(() => StatusText = msg);
        _opc.NodesDiscovered  += (_, nodes) => Dispatch(() => RebuildGrid(nodes));
        _opc.NodeValueUpdated += (_, e)    => Dispatch(() => UpdateNode(e));
    }

    // ── Commands ──────────────────────────────────────────────────────────────

    private async void ConnectAsync()
    {
        IsBusy = true;
        var cts = _opc.NewCts();
        try
        {
            await _opc.ConnectAsync(EndpointUrl.Trim(), cts.Token);
            AppSettings.Current.EndpointUrl = EndpointUrl.Trim();
            AppSettings.Save();
            IsConnected = true;
        }
        catch (OpcUaConnectionException ex)
        {
            AppDialog.Show($"Could not connect to the OPC UA server.\n\n{ex.Message}",
                           "Connection Failed", AppDialogIcon.Error);
        }
        finally { IsBusy = false; }
    }

    private void Disconnect()
    {
        _opc.Disconnect();
        IsConnected = false;
        Nodes.Clear();
        _nodesByName.Clear();
        StatusText = "Disconnected";
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private void RebuildGrid(System.Collections.Generic.IReadOnlyList<MonitoredNodeInfo> nodes)
    {
        Nodes.Clear();
        _nodesByName.Clear();
        foreach (var n in nodes)
        {
            var vm = new MonitoredNodeVm { Name = n.Name, NodeId = n.NodeIdStr };
            Nodes.Add(vm);
            _nodesByName[n.Name] = vm;
        }
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
    private string _value     = "-";
    private string _statusCode = "";
    private string _timestamp = "";

    public string Name     { get; init; } = "";
    public string NodeId   { get; init; } = "";

    public string Value
    {
        get => _value;
        set => Set(ref _value, value);
    }
    public string StatusCode
    {
        get => _statusCode;
        set => Set(ref _statusCode, value);
    }
    public string Timestamp
    {
        get => _timestamp;
        set => Set(ref _timestamp, value);
    }
}
