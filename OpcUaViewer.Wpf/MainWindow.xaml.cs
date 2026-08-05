using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using OpcUaViewer.Core.Contracts;
using OpcUaViewer.Core.Settings;
using OpcUaViewer.Wpf.ViewModels;

namespace OpcUaViewer.Wpf;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;

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

        var ver = Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
            ?? "0.0.0";
        VersionText.Text = $"v{ver}";
    }

    private bool _suppressSelection;

    internal void NavGroups_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_suppressSelection || e.AddedItems.Count == 0) return;
        if (e.AddedItems[0] is not TabGroupViewModel g) return;
        _suppressSelection = true;
        PinnedListBox.SelectedItem = null;
        ((MainViewModel)DataContext).SelectedGroup = g;
        _suppressSelection = false;
    }

    internal void PinnedGroups_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_suppressSelection || e.AddedItems.Count == 0) return;
        if (e.AddedItems[0] is not TabGroupViewModel g) return;
        _suppressSelection = true;
        NavListBox.SelectedItem = null;
        ((MainViewModel)DataContext).SelectedGroup = g;
        _suppressSelection = false;
    }

    internal void SubTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count == 0) return;
        if (e.AddedItems[0] is not IAppTab tab) return;
        var group = ((MainViewModel)DataContext).SelectedGroup;
        if (group is not null)
            group.SelectedTab = tab;
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        base.OnClosing(e);
        var s         = AppSettings.Current;
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
