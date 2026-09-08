using System;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using OpcUaViewer.Core.Licensing;
using OpcUaViewer.Wpf.Dialogs;

namespace OpcUaViewer.Wpf.Views;

public partial class AboutView : System.Windows.Controls.UserControl
{
    public AboutView()
    {
        InitializeComponent();
        Loaded += (_, _) => Populate();
    }

    private void Populate()
    {
        var ver = Assembly.GetEntryAssembly()?.GetName().Version;
        AppNameText.Text    = "OPC UA Viewer";
        AppVersionText.Text = ver is null ? "" : $"v{ver.Major}.{ver.Minor}.{ver.Build}";

        var lic = LicenseState.Current;
        if (lic is null)
        {
            LicenseBorder.Visibility        = Visibility.Collapsed;
            NoLicenseBorder.Visibility      = Visibility.Visible;
            InstallLicenseButton.Visibility = Visibility.Visible;
            return;
        }

        LicenseeText.Text = lic.Licensee;
        AddressText.Text  = lic.Address.Trim();
        AddressText.Visibility = string.IsNullOrWhiteSpace(lic.Address)
            ? Visibility.Collapsed : Visibility.Visible;

        ValidUntilText.Text = lic.IsPerpetual ? "Perpetual" : lic.ValidUntil!.Value.ToString("yyyy-MM-dd");
        SetBadge(ValidBadge, ValidBadgeText,
            ok:    !lic.IsExpired,
            okLabel:  "ACTIVE",
            badLabel: "EXPIRED");

        MaintenanceText.Text = lic.MaintenanceUntil.ToString("yyyy-MM-dd");
        SetBadge(MaintBadge, MaintBadgeText,
            ok:    lic.MaintenanceActive,
            okLabel:  "ACTIVE",
            badLabel: "EXPIRED");

        IssuedText.Text = lic.IssuedDate.ToString("yyyy-MM-dd");

        if (!string.IsNullOrWhiteSpace(lic.Notes))
        {
            NotesText.Text             = lic.Notes.Trim();
            NotesLabel.Visibility      = Visibility.Visible;
            NotesText.Visibility       = Visibility.Visible;
            NotesText.Margin           = new Thickness(0, 10, 0, 0);
            NotesLabel.Margin          = new Thickness(0, 10, 0, 0);
        }
    }

    private void InstallLicense_Click(object sender, RoutedEventArgs e)
    {
        LicenseDialog.ShowIfNeeded("No valid license is currently installed.");
        // Refresh display if the user successfully installed one
        if (LicenseState.Current is not null)
        {
            NoLicenseBorder.Visibility      = Visibility.Collapsed;
            InstallLicenseButton.Visibility = Visibility.Collapsed;
            LicenseBorder.Visibility        = Visibility.Visible;
            Populate();
        }
    }

    private void SetBadge(System.Windows.Controls.Border badge,
                          System.Windows.Controls.TextBlock label,
                          bool ok, string okLabel, string badLabel)
    {
        var successBrush = TryFindResource("SuccessBrush") as Brush ?? Brushes.Green;
        var warnBrush    = TryFindResource("DangerBrush")  as Brush ?? Brushes.Red;

        badge.Background = new SolidColorBrush(
            ok ? Color.FromArgb(0x22, 0x64, 0xC8, 0x64)
               : Color.FromArgb(0x22, 0xC8, 0x32, 0x32));
        label.Foreground = ok ? successBrush : warnBrush;
        label.Text       = ok ? okLabel : badLabel;
    }
}
