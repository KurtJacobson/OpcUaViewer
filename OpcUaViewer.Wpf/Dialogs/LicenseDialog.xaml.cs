using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using OpcUaViewer.Core.Licensing;
using OpcUaViewer.Core.Services;

namespace OpcUaViewer.Wpf.Dialogs;

public partial class LicenseDialog : Window
{
    private string? _selectedPath;

    public LicenseDialog(string reason)
    {
        InitializeComponent();
        Owner = Application.Current.MainWindow;

        MessageText.Text     = reason;
        ExpectedPathText.Text = LicenseValidator.DefaultLicensePath;
    }

    public static void ShowIfNeeded(string reason)
    {
        new LicenseDialog(reason).ShowDialog();
    }

    private void Browse_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new OpenFileDialog
        {
            Title  = "Select License File",
            Filter = "License files (*.lic)|*.lic|All files (*.*)|*.*",
        };
        if (dlg.ShowDialog() != true) return;

        _selectedPath       = dlg.FileName;
        LicensePathBox.Text = dlg.FileName;

        // Preview-validate the selected file without installing it yet
        try
        {
            var info = LicenseValidator.Load(_selectedPath);
            SetStatus($"Valid license for {info.Licensee}.", ok: true);
            InstallButton.IsEnabled = true;
        }
        catch (LicenseException ex)
        {
            SetStatus($"Invalid license: {ex.Message}", ok: false);
            InstallButton.IsEnabled = false;
        }
    }

    private void Install_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedPath is null) return;

        try
        {
            string dest = LicenseValidator.DefaultLicensePath;

            // Copy to license folder if it's not already there
            if (!string.Equals(_selectedPath, dest, System.StringComparison.OrdinalIgnoreCase))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(dest)!);
                File.Copy(_selectedPath, dest, overwrite: true);
            }

            LicenseState.Current = LicenseValidator.Load(dest);
            AppLogger.Info($"License installed — {LicenseState.Current.Licensee}");
            Close();
        }
        catch (LicenseException ex)
        {
            SetStatus($"Could not install: {ex.Message}", ok: false);
        }
        catch (System.Exception ex)
        {
            SetStatus($"Could not copy file: {ex.Message}", ok: false);
        }
    }

    private void Skip_Click(object sender, RoutedEventArgs e) => Close();

    private void TitleBar_Drag(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed) DragMove();
    }

    private void SetStatus(string message, bool ok)
    {
        StatusText.Text       = message;
        StatusText.Foreground = ok
            ? (TryFindResource("SuccessBrush") as Brush ?? Brushes.Green)
            : (TryFindResource("DangerBrush")  as Brush ?? Brushes.Red);
        StatusText.Visibility = Visibility.Visible;
    }
}
