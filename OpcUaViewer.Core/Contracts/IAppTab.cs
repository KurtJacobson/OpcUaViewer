namespace OpcUaViewer.Core.Contracts;

/// <summary>
/// Contract for a navigation tab. Built-in and customer-specific tabs both implement this.
/// Customer assemblies reference only OpcUaViewer.Core, not the WPF project.
/// </summary>
public interface IAppTab
{
    string Title { get; }
    string Icon  { get; }  // Emoji or Segoe MDL2 glyph character
    int    Order { get; }  // Nav sort position; built-ins use 10, 20, 30...
}
