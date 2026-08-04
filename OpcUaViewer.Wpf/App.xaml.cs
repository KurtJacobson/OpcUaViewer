using System.Windows;
using OpcUaViewer.Core.Settings;
using OpcUaViewer.Wpf.Tabs;
using OpcUaViewer.Wpf.ViewModels;

namespace OpcUaViewer.Wpf;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        AppSettings.Load();

        // Register built-in tabs in order.
        // To add a customer-specific tab: add it here (or auto-discover from a Plugins/ folder).
        var tabs = new OpcUaViewer.Core.Contracts.IAppTab[]
        {
            new MonitorTab(),
            new GroupsTab(),
            new DocumentTab(),
            new SettingsTab(),
        };

        var vm     = new MainViewModel(tabs);
        var window = new MainWindow(vm);
        window.Show();
    }
}
