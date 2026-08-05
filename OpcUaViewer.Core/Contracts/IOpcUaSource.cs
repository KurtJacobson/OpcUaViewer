using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;

namespace OpcUaViewer.Core.Contracts;

/// <summary>
/// Data-source contract for an OPC UA backend plugin.
/// Exposes both the raw event surface (for consumer plugins) and the
/// ViewModel surface (IsConnected, commands) needed by the settings panel.
/// </summary>
public interface IOpcUaSource : IDataSource, INotifyPropertyChanged
{
    bool   IsConnected { get; }
    bool   IsBusy      { get; }
    string StatusText  { get; }
    string EndpointUrl { get; set; }

    RelayCommand ConnectCommand    { get; }
    RelayCommand DisconnectCommand { get; }

    event EventHandler<string>?                           StatusChanged;
    event EventHandler<IReadOnlyList<MonitoredNodeInfo>>? NodesDiscovered;
    event EventHandler<NodeValueEventArgs>?               NodeValueUpdated;
    event EventHandler<string>?                           ProductIdChanged;
    event EventHandler<string>?                           CamFileChanged;
    event EventHandler<int>?                              MachineStateChanged;
    event EventHandler<bool>?                             OperatorActionChanged;
    event EventHandler<NodeValueEventArgs>?               StatsValueChanged;
}
