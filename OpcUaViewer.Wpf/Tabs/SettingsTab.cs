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
    private bool _fullScreen;

    public bool KeyboardEnabled
    {
        get => _keyboardEnabled;
        set => Set(ref _keyboardEnabled, value);
    }

    public bool FullScreen
    {
        get => _fullScreen;
        set => Set(ref _fullScreen, value);
    }

    public PluginService                  PluginService  { get; }
    public IReadOnlyList<SettingsPanelVm> SettingsPanels { get; }

    public RelayCommand SaveCommand     { get; }
    public RelayCommand OpenLogCommand  { get; }

    private bool _loading;
    private List<ISettingsPanel> _allPanels = [];

    public SettingsTab(PluginService pluginService, IEnumerable<ISettingsPanel>? builtInPanels = null)
    {
        PluginService  = pluginService;
        SaveCommand    = new RelayCommand(Save);
        OpenLogCommand = new RelayCommand(OpenLog);

        var rawPanels = (builtInPanels ?? []).Concat(pluginService.Plugins.SelectMany(pl => pl.Panels)).ToList();
        _allPanels = rawPanels;

        var panels = new List<SettingsPanelVm>();
        foreach (var p in rawPanels)
        {
            try   { panels.Add(new SettingsPanelVm(p.Header, p.CreateView())); }
            catch (Exception ex) { AppLogger.Error($"Settings panel '{p.Header}' CreateView failed", ex); }
        }
        SettingsPanels = panels;

        PropertyChanged += (_, e) =>
        {
            if (_loading) return;
            if (e.PropertyName is nameof(FullScreen) or nameof(KeyboardEnabled))
                SaveGeneral();
        };
        AppLogger.Info($"SettingsPanels built: {panels.Count} panel(s)" +
            (panels.Count > 0 ? $" [{string.Join(", ", panels.Select(p => p.Header))}]" : ""));

        Load();
    }

    public void Load()
    {
        _loading = true;
        KeyboardEnabled = AppSettings.Current.KeyboardEnabled;
        FullScreen      = AppSettings.Current.WindowState == nameof(System.Windows.WindowState.Maximized);
        _loading = false;
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

    private void SaveGeneral()
    {
        AppSettings.Current.KeyboardEnabled = KeyboardEnabled;

        var targetState = FullScreen
            ? System.Windows.WindowState.Maximized
            : System.Windows.WindowState.Normal;
        AppSettings.Current.WindowState = targetState.ToString();

        try
        {
            if (System.Windows.Application.Current?.MainWindow is { } w)
            {
                if (FullScreen)
                {
                    w.WindowStyle = System.Windows.WindowStyle.None;
                    w.ResizeMode  = System.Windows.ResizeMode.NoResize;
                    w.WindowState = System.Windows.WindowState.Maximized;
                }
                else
                {
                    w.WindowState = System.Windows.WindowState.Normal;
                    w.WindowStyle = System.Windows.WindowStyle.SingleBorderWindow;
                    w.ResizeMode  = System.Windows.ResizeMode.CanResize;
                }
            }
        }
        catch (Exception ex) { AppLogger.Error("Could not apply window state", ex); }

        AppSettings.Save();
    }

    private void Save()
    {
        SaveGeneral();
        foreach (var panel in _allPanels)
            panel.Save();
        PluginService.SaveEnabledState();
    }
}
