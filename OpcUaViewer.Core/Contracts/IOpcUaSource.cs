using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace OpcUaViewer.Core.Contracts;

/// <summary>
/// Protocol-agnostic data source for machine monitoring.
/// Implement this interface to feed data into Groups, Document, and Monitor plugins
/// from any industrial protocol (OPC UA, Modbus, MQTT, etc.).
/// </summary>
public interface IMachineSource : IDataSource, INotifyPropertyChanged
{
    bool   IsConnected { get; }
    string StatusText  { get; }

    event EventHandler<string>?                 StatusChanged;
    event EventHandler<IReadOnlyList<TagInfo>>? TagsDiscovered;
    event EventHandler<TagValueEventArgs>?      TagValueUpdated;
    event EventHandler<string>?                 ProductIdChanged;
    event EventHandler<string>?                 ProgramChanged;
    event EventHandler<int>?                    MachineStateChanged;
    event EventHandler<bool>?                   OperatorActionChanged;
}
