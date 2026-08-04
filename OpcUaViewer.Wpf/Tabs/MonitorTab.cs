using System.Collections.ObjectModel;
using OpcUaViewer.Core.Contracts;
using OpcUaViewer.Wpf.Infrastructure;

namespace OpcUaViewer.Wpf.Tabs;

/// <summary>OPC UA status monitor tab — shows live node values from the machine.</summary>
public class MonitorTab : ViewModelBase, IAppTab
{
    public string Title => "Monitor";
    public string Icon  => "📡";
    public int    Order => 10;

    private bool   _isConnected;
    private string _statusText  = "Disconnected";
    private string _endpointUrl = "";

    public bool IsConnected
    {
        get => _isConnected;
        set { Set(ref _isConnected, value); Notify(nameof(StatusText)); }
    }

    public string StatusText
    {
        get => _statusText;
        set => Set(ref _statusText, value);
    }

    public string EndpointUrl
    {
        get => _endpointUrl;
        set => Set(ref _endpointUrl, value);
    }

    public ObservableCollection<MonitoredNode> Nodes { get; } = [];

    public RelayCommand ConnectCommand    { get; }
    public RelayCommand DisconnectCommand { get; }

    public MonitorTab()
    {
        EndpointUrl = OpcUaViewer.Core.Settings.AppSettings.Current.EndpointUrl;

        ConnectCommand    = new RelayCommand(Connect,    () => !IsConnected);
        DisconnectCommand = new RelayCommand(Disconnect, () => IsConnected);
    }

    private void Connect()
    {
        // OPC UA connection will be wired here
        StatusText = $"Connecting to {EndpointUrl}…";
    }

    private void Disconnect()
    {
        IsConnected = false;
        StatusText  = "Disconnected";
        Nodes.Clear();
    }
}

public class MonitoredNode : ViewModelBase
{
    private string _value = "";

    public string NodeId    { get; init; } = "";
    public string Name      { get; init; } = "";
    public string DataType  { get; init; } = "";

    public string Value
    {
        get => _value;
        set => Set(ref _value, value);
    }
}
