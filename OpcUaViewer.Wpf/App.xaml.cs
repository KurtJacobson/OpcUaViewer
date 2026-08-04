using System.Windows;
using Microsoft.Web.WebView2.Core;
using OpcUaViewer.Core.Services;
using OpcUaViewer.Core.Settings;
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

        // Pre-warm the WebView2 browser process so it's ready before the Document tab is opened
        _ = CoreWebView2Environment.CreateAsync();

        _opc = new OpcUaService();

        var monitorTab  = new MonitorTab(_opc);
        var groupsTab   = new GroupsTab(_opc);
        var documentTab = new DocumentTab(_opc);
        var settingsTab = new SettingsTab();

        // To add a customer-specific tab:
        //   1. Implement IAppTab + ViewModelBase in a customer assembly (reference OpcUaViewer.Core)
        //   2. Add a DataTemplate for it in MainWindow.xaml (or merge a ResourceDictionary)
        //   3. Register it here: var customerTab = new AcmeTab(_opc);
        var tabs = new OpcUaViewer.Core.Contracts.IAppTab[]
        {
            monitorTab,
            groupsTab,
            documentTab,
            settingsTab,
        };

        var vm     = new MainViewModel(tabs);
        var window = new MainWindow(vm);
        window.Show();

        // Auto-connect on startup if endpoint is configured
        if (!string.IsNullOrWhiteSpace(AppSettings.Current.EndpointUrl))
            monitorTab.ConnectCommand.Execute(null);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _opc?.Dispose();
        base.OnExit(e);
    }
}
