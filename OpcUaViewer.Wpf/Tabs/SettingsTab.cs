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

    private bool _keyboardEnabled;

    public bool KeyboardEnabled
    {
        get => _keyboardEnabled;
        set => Set(ref _keyboardEnabled, value);
    }

    public MonitorTab      MonitorTab    { get; }
    public PluginService   PluginService { get; }
    public RelayCommand    SaveCommand   { get; }

    public SettingsTab(MonitorTab monitorTab, PluginService pluginService)
    {
        MonitorTab    = monitorTab;
        PluginService = pluginService;
        SaveCommand   = new RelayCommand(Save);
        Load();
    }

    public void Load()
    {
        KeyboardEnabled = AppSettings.Current.KeyboardEnabled;
    }

    private void Save()
    {
        AppSettings.Current.KeyboardEnabled = KeyboardEnabled;
        PluginService.SaveEnabledState();
        AppSettings.Save();
    }
}
