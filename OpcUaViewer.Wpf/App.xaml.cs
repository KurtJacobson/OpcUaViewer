using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Web.WebView2.Core;
using OpcUaViewer.Core.Contracts;
using OpcUaViewer.Core.Services;
using OpcUaViewer.Core.Settings;
using OpcUaViewer.Wpf.Dialogs;
using OpcUaViewer.Wpf.Plugins;
using OpcUaViewer.Wpf.Tabs;
using OpcUaViewer.Wpf.ViewModels;

namespace OpcUaViewer.Wpf;

public partial class App : Application
{
    private OpcUaService? _opc;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        string logDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "OpcUaViewer", "logs");
        AppLogger.Initialize(logDir);
        AppLogger.Info("Application starting");

        DispatcherUnhandledException += (_, ex) =>
        {
            AppLogger.Error("Unhandled UI exception", ex.Exception);
            ex.Handled = true;
        };
        AppDomain.CurrentDomain.UnhandledException += (_, ex) =>
            AppLogger.Error("Unhandled exception", ex.ExceptionObject as Exception);

        AppSettings.Load();
        DialogService.Current = new WpfDialogService();

        _ = CoreWebView2Environment.CreateAsync();

        _opc = new OpcUaService();

        var pluginService = new PluginService(_opc, this);
        var pluginTabs    = pluginService.LoadAll().ToList();

        foreach (var p in pluginService.Plugins)
        {
            if (string.IsNullOrEmpty(p.LoadError))
                AppLogger.Info($"Plugin loaded: {p.Name} ({p.TabList})");
            else
                AppLogger.Error($"Plugin failed: {p.Name} — {p.LoadError}");
        }

        var monitorTab  = new MonitorTab(_opc);
        var settingsTab = new SettingsTab(monitorTab, pluginService);

        var allTabs = new IAppTab[] { monitorTab, settingsTab }.Concat(pluginTabs);
        var vm      = new MainViewModel(allTabs);
        var window  = new MainWindow(vm);
        window.Show();

        AppLogger.Info("Application started");

        if (!string.IsNullOrWhiteSpace(AppSettings.Current.EndpointUrl))
            monitorTab.ConnectCommand.Execute(null);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        AppLogger.Info("Application exiting");
        _opc?.Dispose();
        base.OnExit(e);
    }
}
