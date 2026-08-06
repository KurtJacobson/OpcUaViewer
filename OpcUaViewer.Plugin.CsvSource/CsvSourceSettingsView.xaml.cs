using System.Windows.Controls;
using WinForms = System.Windows.Forms;

namespace OpcUaViewer.Plugin.CsvSource;

public partial class CsvSourceSettingsView : System.Windows.Controls.UserControl
{
    public CsvSourceSettingsView() => InitializeComponent();

    private void BrowseButton_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        using var dlg = new WinForms.FolderBrowserDialog
        {
            Description            = "Select the folder containing Schroder ProductionLog CSV files",
            UseDescriptionForTitle = true,
        };

        if (DataContext is CsvMachineSource vm && !string.IsNullOrEmpty(vm.LogFolderPath))
            dlg.SelectedPath = vm.LogFolderPath;

        if (dlg.ShowDialog() == WinForms.DialogResult.OK && DataContext is CsvMachineSource vm2)
            vm2.LogFolderPath = dlg.SelectedPath;
    }
}
