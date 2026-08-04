using System.Windows;
using System.Windows.Input;

namespace OpcUaViewer.Wpf.Dialogs;

public enum AppDialogIcon    { Info, Warning, Error }
public enum AppDialogButtons { Ok, YesNo }

public partial class AppDialog : Window
{

    public bool Result { get; private set; }

    public AppDialog(string title, string message,
                     AppDialogButtons buttons = AppDialogButtons.Ok,
                     AppDialogIcon    icon    = AppDialogIcon.Info)
    {
        InitializeComponent();
        TitleText.Text   = title;
        MessageText.Text = message;
        IconText.Text    = icon switch
        {
            AppDialogIcon.Warning => "⚠",
            AppDialogIcon.Error   => "✖",
            _                     => "ℹ",
        };
        IconText.Foreground = icon switch
        {
            AppDialogIcon.Warning => FindResource("AccentBrush") as System.Windows.Media.Brush,
            AppDialogIcon.Error   => System.Windows.Media.Brushes.IndianRed,
            _                     => FindResource("TextSubBrush") as System.Windows.Media.Brush,
        };

        if (buttons == AppDialogButtons.Ok)
        {
            YesButton.Visibility = Visibility.Collapsed;
            NoButton.Visibility  = Visibility.Collapsed;
        }
        else
        {
            OkButton.Visibility = Visibility.Collapsed;
        }
    }

    // ── Static helpers ────────────────────────────────────────────────────────

    public static void Show(string message, string title = "Information",
                            AppDialogIcon icon = AppDialogIcon.Info)
        => Open(title, message, AppDialogButtons.Ok, icon);

    public static void Warn(string message, string title = "Warning")
        => Open(title, message, AppDialogButtons.Ok, AppDialogIcon.Warning);

    public static bool Confirm(string message, string title = "Confirm",
                               AppDialogIcon icon = AppDialogIcon.Warning)
    {
        var dlg = Create(title, message, AppDialogButtons.YesNo, icon);
        dlg.ShowDialog();
        return dlg.Result;
    }

    // ── Internals ─────────────────────────────────────────────────────────────

    private static void Open(string title, string message,
                              AppDialogButtons buttons, AppDialogIcon icon)
        => Create(title, message, buttons, icon).ShowDialog();

    private static AppDialog Create(string title, string message,
                                     AppDialogButtons buttons, AppDialogIcon icon)
        => new(title, message, buttons, icon) { Owner = Application.Current.MainWindow };

    private void Yes_Click(object sender, RoutedEventArgs e) { Result = true;  Close(); }
    private void No_Click (object sender, RoutedEventArgs e) { Result = false; Close(); }
    private void Ok_Click (object sender, RoutedEventArgs e) { Result = true;  Close(); }

    private void TitleBar_Drag(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed) DragMove();
    }
}
