using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using OpcUaViewer.Core.Contracts;
using OpcUaViewer.Core.Settings;
using OpcUaViewer.Wpf.Infrastructure;
using OpcUaViewer.Wpf.Plugins;
using OpcUaViewer.Wpf.Views;

namespace OpcUaViewer.Wpf.Tabs;

public class SettingsTab : ViewModelBase, IAppTab
{
    public string Title       => "Settings";
    public string Icon        => "⚙";
    public int    Order       => 90;
    public bool   PinToBottom => true;

    public FrameworkElement CreateView() => new SettingsView { DataContext = this };

    private bool _keyboardEnabled;

    public bool KeyboardEnabled
    {
        get => _keyboardEnabled;
        set => Set(ref _keyboardEnabled, value);
    }

    public MonitorTab      MonitorTab      { get; }
    public PluginService   PluginService   { get; }
    public RelayCommand    SaveCommand     { get; }
    public RelayCommand    OpenLogCommand  { get; }

    public SettingsTab(MonitorTab monitorTab, PluginService pluginService)
    {
        MonitorTab     = monitorTab;
        PluginService  = pluginService;
        SaveCommand    = new RelayCommand(Save);
        OpenLogCommand = new RelayCommand(OpenLog);
        Load();
    }

    public void Load()
    {
        KeyboardEnabled = AppSettings.Current.KeyboardEnabled;
    }

    private static void OpenLog()
    {
        string path = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "OpcUaViewer", "logs", $"{DateTime.Today:yyyy-MM-dd}.log");

        if (!File.Exists(path))
            File.WriteAllText(path, "");  // create empty file so the editor opens cleanly

        Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
    }

    private void Save()
    {
        AppSettings.Current.KeyboardEnabled = KeyboardEnabled;
        PluginService.SaveEnabledState();
        AppSettings.Save();
    }
}
