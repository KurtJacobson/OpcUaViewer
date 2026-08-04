using OpcUaViewer.Core.Contracts;
using OpcUaViewer.Core.Settings;
using OpcUaViewer.Wpf.Infrastructure;
using OpcUaViewer.Wpf.Plugins;

namespace OpcUaViewer.Wpf.Tabs;

public class SettingsTab : ViewModelBase, IAppTab
{
    public string Title       => "Settings";
    public string Icon        => "⚙";
    public int    Order       => 90;
    public bool   PinToBottom => true;

    private string _endpointUrl     = "";
    private bool   _keyboardEnabled;

    public string EndpointUrl
    {
        get => _endpointUrl;
        set => Set(ref _endpointUrl, value);
    }
    public bool KeyboardEnabled
    {
        get => _keyboardEnabled;
        set => Set(ref _keyboardEnabled, value);
    }

    public PluginService PluginService { get; }
    public RelayCommand  SaveCommand   { get; }

    public SettingsTab(PluginService pluginService)
    {
        PluginService = pluginService;
        SaveCommand   = new RelayCommand(Save);
        Load();
    }

    public void Load()
    {
        var s = AppSettings.Current;
        EndpointUrl     = s.EndpointUrl;
        KeyboardEnabled = s.KeyboardEnabled;
    }

    private void Save()
    {
        var s = AppSettings.Current;
        s.EndpointUrl     = EndpointUrl.Trim();
        s.KeyboardEnabled = KeyboardEnabled;
        PluginService.SaveEnabledState();
        AppSettings.Save();
    }
}
