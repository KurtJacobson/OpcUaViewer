using System.Windows;
using System.Windows.Controls;
using OpcUaViewer.Wpf.Tabs;

namespace OpcUaViewer.Wpf.Views;

public partial class SettingsView : UserControl
{
    public SettingsView() => InitializeComponent();

    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.Property == DataContextProperty && e.NewValue is SettingsTab vm)
        {
            foreach (var panel in vm.SettingsPanels)
                SettingsTabs.Items.Add(new TabItem
                {
                    Header  = panel.Header,
                    Style   = (Style)FindResource("SettingsTabItemStyle"),
                    Content = new ScrollViewer
                    {
                        VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                        Content = new Border { Margin = new Thickness(32, 24, 32, 24), Child = panel.View }
                    }
                });
        }
    }

    private void CopyError_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: string error } && !string.IsNullOrEmpty(error))
            Clipboard.SetText(error);
    }
}
