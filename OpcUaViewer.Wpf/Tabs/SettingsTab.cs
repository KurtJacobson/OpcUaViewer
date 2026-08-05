using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using OpcUaViewer.Core.Contracts;
using OpcUaViewer.Core.Settings;
using OpcUaViewer.Wpf.Infrastructure;
using OpcUaViewer.Wpf.Plugins;
using OpcUaViewer.Wpf.Views;

namespace OpcUaViewer.Wpf.Tabs;

public record SettingsPanelVm(string Header, FrameworkElement View);

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

    public PluginService                  PluginService  { get; }
    public IReadOnlyList<SettingsPanelVm> SettingsPanels { get; }

    public RelayCommand SaveCommand     { get; }
    public RelayCommand OpenLogCommand  { get; }

    public SettingsTab(PluginService pluginService)
    {
        PluginService  = pluginService;
        SaveCommand    = new RelayCommand(Save);
        OpenLogCommand = new RelayCommand(OpenLog);

        SettingsPanels = pluginService.Plugins
            .SelectMany(p => p.Panels)
            .Select(p => new SettingsPanelVm(p.Header, p.CreateView()))
            .ToList();

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
            File.WriteAllText(path, "");

        Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
    }

    private void Save()
    {
        AppSettings.Current.KeyboardEnabled = KeyboardEnabled;
        foreach (var panel in PluginService.Plugins.SelectMany(p => p.Panels))
            panel.Save();
        PluginService.SaveEnabledState();
        AppSettings.Save();
    }
}
