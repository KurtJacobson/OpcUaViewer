using System.Reflection;
using System.Windows;
using OpcUaViewer.Core.Settings;
using OpcUaViewer.Wpf.ViewModels;

namespace OpcUaViewer.Wpf;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;

        // Restore saved window placement
        var s = AppSettings.Current;
        if (s.WindowLeft >= 0 && s.WindowTop >= 0)
        {
            Left   = s.WindowLeft;
            Top    = s.WindowTop;
            Width  = s.WindowWidth;
            Height = s.WindowHeight;
        }
        if (Enum.TryParse<WindowState>(s.WindowState, out var state))
            WindowState = state;

        // Show assembly version in nav footer
        var ver = Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
            ?? "0.0.0";
        VersionText.Text = $"v{ver}";
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        base.OnClosing(e);
        var s        = AppSettings.Current;
        s.WindowState = WindowState.ToString();
        if (WindowState == WindowState.Normal)
        {
            s.WindowLeft   = (int)Left;
            s.WindowTop    = (int)Top;
            s.WindowWidth  = (int)Width;
            s.WindowHeight = (int)Height;
        }
        AppSettings.Save();
    }
}
