using System;
using System.Collections.Generic;
using System.Windows;
using OpcUaViewer.Core.Contracts;
using OpcUaViewer.Core.Services;
using OpcUaViewer.Core.Settings;
using OpcUaViewer.Plugin.OpcUa.Views;

namespace OpcUaViewer.Plugin.OpcUa;

public class OpcUaDataSource : ViewModelBase, IMachineSource, ISettingsPanel, IDisposable
{
    private readonly OpcUaService _svc = new();

    private bool   _isConnected;
    private bool   _isBusy;
    private string _statusText  = "Disconnected";
    private string _endpointUrl;

    // ── IDataSource ───────────────────────────────────────────────────────────
    public string Name        => "OPC UA";
    public string Description => "OPC UA client — connects to a server and streams tag values.";

    // ── IMachineSource ────────────────────────────────────────────────────────
    public bool   IsConnected { get => _isConnected; private set => Set(ref _isConnected, value); }
    public string StatusText  { get => _statusText;  private set => Set(ref _statusText, value); }

    public event EventHandler<string>?                 StatusChanged;
    public event EventHandler<IReadOnlyList<TagInfo>>? TagsDiscovered;
    public event EventHandler<TagValueEventArgs>?      TagValueUpdated;
    public event EventHandler<string>?                 ProductIdChanged;
    public event EventHandler<string>?                 CamFileChanged;
    public event EventHandler<int>?                    MachineStateChanged;
    public event EventHandler<bool>?                   OperatorActionChanged;

    // ── OpcUa-specific ViewModel surface (for OpcUaSettingsView) ─────────────
    public bool   IsBusy      { get => _isBusy;      private set => Set(ref _isBusy, value); }
    public string EndpointUrl { get => _endpointUrl; set => Set(ref _endpointUrl, value); }

    public RelayCommand ConnectCommand    { get; }
    public RelayCommand DisconnectCommand { get; }

    // ── ISettingsPanel ────────────────────────────────────────────────────────
    public string Header => "OPC UA";
    FrameworkElement ISettingsPanel.CreateView() => new OpcUaSettingsView { DataContext = this };
    public void Save()
    {
        AppSettings.Current.EndpointUrl = EndpointUrl.Trim();
        AppSettings.Save();
    }

    // ── IDataSource ───────────────────────────────────────────────────────────
    public void OnApplicationStarted()
    {
        if (!string.IsNullOrWhiteSpace(EndpointUrl))
            ConnectCommand.Execute(null);
    }

    public OpcUaDataSource()
    {
        _endpointUrl = AppSettings.Current.EndpointUrl;

        _svc.StatusChanged       += (_, msg)   => Dispatch(() => { StatusText = msg; StatusChanged?.Invoke(this, msg); });
        _svc.TagsDiscovered      += (_, tags)  => TagsDiscovered?.Invoke(this, tags);
        _svc.TagValueUpdated     += (_, e)     => TagValueUpdated?.Invoke(this, e);
        _svc.ProductIdChanged    += (_, v)     => ProductIdChanged?.Invoke(this, v);
        _svc.CamFileChanged      += (_, v)     => CamFileChanged?.Invoke(this, v);
        _svc.MachineStateChanged += (_, v)     => MachineStateChanged?.Invoke(this, v);
        _svc.OperatorActionChanged += (_, v)   => OperatorActionChanged?.Invoke(this, v);

        ConnectCommand    = new RelayCommand(ConnectAsync,  () => !IsConnected && !IsBusy);
        DisconnectCommand = new RelayCommand(Disconnect,    () => IsConnected);
    }

    private async void ConnectAsync()
    {
        IsBusy = true;
        var cts = _svc.NewCts();
        try
        {
            AppLogger.Info($"Connecting to OPC UA: {EndpointUrl.Trim()}");
            await _svc.ConnectAsync(EndpointUrl.Trim(), cts.Token);
            IsConnected = true;
            AppLogger.Info("OPC UA connected");
        }
        catch (OpcUaConnectionException ex)
        {
            AppLogger.Error($"OPC UA connection failed: {EndpointUrl.Trim()}", ex);
            DialogService.Current.Warn(
                $"Could not connect to the OPC UA server.\n\n{ex.Message}",
                "Connection Failed");
        }
        finally { IsBusy = false; }
    }

    private void Disconnect()
    {
        _svc.Disconnect();
        IsConnected = false;
    }

    public void Dispose() => _svc.Dispose();

    private static void Dispatch(Action a)
    {
        if (Application.Current?.Dispatcher.CheckAccess() == true) a();
        else Application.Current?.Dispatcher.BeginInvoke(a);
    }
}
