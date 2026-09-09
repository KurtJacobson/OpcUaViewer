using System.Windows;

namespace OpcUaViewer.Core.Contracts;

/// <summary>
/// Implement on a data source or tab to inject a section into the main Settings tab.
/// The header and view are rendered in order of plugin load.
/// </summary>
public interface ISettingsPanel
{
    string Header { get; }
    FrameworkElement CreateView();

    /// <summary>Called when the user clicks Save Settings.</summary>
    void Save() {}
}
