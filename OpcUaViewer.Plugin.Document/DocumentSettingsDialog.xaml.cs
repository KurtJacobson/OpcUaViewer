using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using OpcUaViewer.Core.Settings;

namespace OpcUaViewer.Plugin.Document;

public partial class DocumentSettingsDialog : Window
{
    public DocumentSettingsDialog()
    {
        InitializeComponent();
        Owner = Application.Current.MainWindow;
        PdfFolderBox.Text = AppSettings.Current.PdfFolderPath;
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        => DragMove();

    private void Browse_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new OpenFolderDialog { Title = "Select PDF folder" };
        if (dlg.ShowDialog(this) == true)
            PdfFolderBox.Text = dlg.FolderName;
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        AppSettings.Current.PdfFolderPath = PdfFolderBox.Text.Trim();
        AppSettings.Save();
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
        => DialogResult = false;
}
