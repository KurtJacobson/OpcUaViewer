using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using OpcUaViewer.Core.Settings;

namespace OpcUaViewer.Plugin.Groups;

public partial class GroupsSettingsDialog : Window
{
    public GroupsSettingsDialog()
    {
        InitializeComponent();
        Owner = Application.Current.MainWindow;

        var s = AppSettings.Current;
        CamFolderBox.Text      = s.CamFolderPath;
        CamOutputBox.Text      = s.CamOutputPath;
        CamProductsBox.Text    = s.CamProductsPath;
        ProductPrefixBox.Text  = s.ProductPathPrefix;
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        => DragMove();

    private void Browse_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new OpenFolderDialog { Title = "Select folder" };
        if (dlg.ShowDialog(this) != true) return;

        string tag = (sender as Button)?.Tag as string ?? "";
        switch (tag)
        {
            case "CamFolder":    CamFolderBox.Text   = dlg.FolderName; break;
            case "CamOutput":    CamOutputBox.Text   = dlg.FolderName; break;
            case "CamProducts":  CamProductsBox.Text = dlg.FolderName; break;
        }
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        var s = AppSettings.Current;
        s.CamFolderPath     = CamFolderBox.Text.Trim();
        s.CamOutputPath     = CamOutputBox.Text.Trim();
        s.CamProductsPath   = CamProductsBox.Text.Trim();
        s.ProductPathPrefix = ProductPrefixBox.Text.Trim();
        AppSettings.Save();
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
        => DialogResult = false;
}
