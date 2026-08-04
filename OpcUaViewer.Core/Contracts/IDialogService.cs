namespace OpcUaViewer.Core.Contracts;

public interface IDialogService
{
    void Show(string message, string title = "Information");
    void Warn(string message, string title = "Warning");
    bool Confirm(string message, string title = "Confirm");
}

public static class DialogService
{
    public static IDialogService Current { get; set; } = new NullDialogService();
}

/// <summary>Fallback used before the WPF dialog service is registered.</summary>
file sealed class NullDialogService : IDialogService
{
    public void Show(string message, string title = "Information") { }
    public void Warn(string message, string title = "Warning") { }
    public bool Confirm(string message, string title = "Confirm") => false;
}
