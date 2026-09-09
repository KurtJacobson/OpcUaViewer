namespace OpcUaViewer.Core.Contracts;

/// <summary>
/// Optional interface for tabs that expose a settings dialog.
/// The plugin card shows a Configure button when any tab in the plugin implements this.
/// </summary>
public interface IConfigurableTab
{
    void Configure();
}
