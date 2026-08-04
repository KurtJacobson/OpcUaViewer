using System.Linq;
using System.Windows;
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
        AppSettings.Load();
        DialogService.Current = new WpfDialogService();

        _ = CoreWebView2Environment.CreateAsync();

        _opc = new OpcUaService();

        var pluginService = new PluginService(_opc, this);
        var pluginTabs    = pluginService.LoadAll().ToList();

        var monitorTab  = new MonitorTab(_opc);
        var settingsTab = new SettingsTab(pluginService);

        var allTabs = new IAppTab[] { monitorTab, settingsTab }.Concat(pluginTabs);
        var vm      = new MainViewModel(allTabs);
        var window  = new MainWindow(vm);
        window.Show();

        if (!string.IsNullOrWhiteSpace(AppSettings.Current.EndpointUrl))
            monitorTab.ConnectCommand.Execute(null);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _opc?.Dispose();
        base.OnExit(e);
    }
}
