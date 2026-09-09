using OpcUaViewer.Core.Contracts;

namespace OpcUaViewer.Wpf.Dialogs;

/// <summary>Registers AppDialog as the IDialogService implementation for the WPF host.</summary>
internal sealed class WpfDialogService : IDialogService
{
    public void Show(string message, string title = "Information")
        => AppDialog.Show(message, title);

    public void Warn(string message, string title = "Warning")
        => AppDialog.Warn(message, title);

    public bool Confirm(string message, string title = "Confirm")
        => AppDialog.Confirm(message, title);
}
