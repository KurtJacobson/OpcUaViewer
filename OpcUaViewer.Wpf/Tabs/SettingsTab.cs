using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using OpcUaViewer.Core.Contracts;
using OpcUaViewer.Core.Services;
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

        var panels = new List<SettingsPanelVm>();
        foreach (var p in pluginService.Plugins.SelectMany(pl => pl.Panels))
        {
            try   { panels.Add(new SettingsPanelVm(p.Header, p.CreateView())); }
            catch (Exception ex) { AppLogger.Error($"Settings panel '{p.Header}' CreateView failed", ex); }
        }
        SettingsPanels = panels;
        AppLogger.Info($"SettingsPanels built: {panels.Count} panel(s)" +
            (panels.Count > 0 ? $" [{string.Join(", ", panels.Select(p => p.Header))}]" : ""));

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
