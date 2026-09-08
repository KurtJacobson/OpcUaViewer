using System.Windows;
using System.Windows.Input;
using OpcUaViewer.Core.Settings;

namespace OpcUaViewer.Plugin.Webcam;

public partial class WebcamSettingsDialog : Window
{
    public WebcamSettingsDialog()
    {
        InitializeComponent();
        Owner = Application.Current.MainWindow;
        CameraIndexBox.SelectedIndex = AppSettings.Current.WebcamIndex;
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        AppSettings.Current.WebcamIndex = CameraIndexBox.SelectedIndex;
        AppSettings.Save();
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
