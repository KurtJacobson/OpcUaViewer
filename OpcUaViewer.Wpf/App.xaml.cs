using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Web.WebView2.Core;
using OpcUaViewer.Core.Contracts;
using OpcUaViewer.Core.Services;
using OpcUaViewer.Core.Settings;
using OpcUaViewer.Wpf.DataSources;
using OpcUaViewer.Wpf.Dialogs;
using OpcUaViewer.Wpf.Plugins;
using OpcUaViewer.Wpf.Tabs;
using OpcUaViewer.Wpf.ViewModels;

namespace OpcUaViewer.Wpf;

public partial class App : Application
{
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

        var opcUaDataSource = new OpcUaDataSource();
        DataSourceRegistry.Register(opcUaDataSource);

        var pluginService = new PluginService(this);
        var pluginTabs    = pluginService.LoadAll().ToList();

        foreach (var p in pluginService.Plugins)
        {
            if (string.IsNullOrEmpty(p.LoadError))
                AppLogger.Info($"Plugin loaded: {p.Name} tabs=[{p.TabList}] sources=[{p.SourceList}] panels={p.Panels.Count} — {p.FilePath}");
            else
                AppLogger.Error($"Plugin failed: {p.Name} — {p.LoadError} — {p.FilePath}");
        }

        var settingsTab = new SettingsTab(pluginService, [opcUaDataSource]);
        var allTabs     = pluginTabs.Append(settingsTab);
        var vm          = new MainViewModel(allTabs);
        var window      = new MainWindow(vm);
        MainWindow = window;
        window.Show();

        AppLogger.Info("Application started");

        // Notify all data sources that the app is ready (e.g. OpcUaDataSource auto-connects)
        foreach (var src in DataSourceRegistry.All.Values.Distinct())
            src.OnApplicationStarted();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        AppLogger.Info("Application exiting");
        foreach (var src in DataSourceRegistry.All.Values.Distinct().OfType<IDisposable>())
            src.Dispose();
        base.OnExit(e);
    }
}
