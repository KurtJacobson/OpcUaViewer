using System.Windows;
using System.Windows.Controls;

namespace OpcUaViewer.Wpf.Views;

public partial class SettingsView : UserControl
{
    public SettingsView() => InitializeComponent();

    private void CopyError_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: string error } && !string.IsNullOrEmpty(error))
            Clipboard.SetText(error);
    }
}
