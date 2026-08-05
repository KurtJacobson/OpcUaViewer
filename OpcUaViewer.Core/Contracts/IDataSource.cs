namespace OpcUaViewer.Core.Contracts;

/// <summary>
/// Marker interface for non-visual data-source plugins.
/// Implement domain-specific sub-interfaces (e.g. IOpcUaSource, IModbusSource)
/// and retrieve instances at runtime via DataSourceRegistry.Get&lt;T&gt;().
/// </summary>
public interface IDataSource
{
    string Name        { get; }
    string Description => "";
}
