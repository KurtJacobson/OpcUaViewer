using System.Collections.Generic;
using System.Linq;
using OpcUaViewer.Core.Contracts;
using OpcUaViewer.Wpf.Infrastructure;

namespace OpcUaViewer.Wpf.Plugins;

public sealed class PluginInfo : ViewModelBase
{
    private bool _isEnabled;

    public string Name      { get; init; } = "";
    public string FilePath  { get; init; } = "";
    public bool   IsLoaded  { get; init; }
    public string LoadError { get; init; } = "";

    public IReadOnlyList<IAppTab>      Tabs        { get; init; } = [];
    public IReadOnlyList<IDataSource>  DataSources { get; init; } = [];
    public IReadOnlyList<ISettingsPanel> Panels    { get; init; } = [];

    public bool IsDataSourcePlugin => Tabs.Count == 0 && DataSources.Count > 0;

    public string Description =>
        Tabs.FirstOrDefault(t => !string.IsNullOrEmpty(t.Description))?.Description
        ?? DataSources.FirstOrDefault(s => !string.IsNullOrEmpty(s.Description))?.Description
        ?? "";

    public string TabList => Tabs.Count > 0
        ? string.Join(", ", Tabs.Select(t => t.Title))
        : "—";

    public string SourceList => DataSources.Count > 0
        ? string.Join(", ", DataSources.Select(s => s.Name))
        : "—";

    public bool CanConfigure =>
        Tabs.OfType<IConfigurableTab>().Any() ||
        DataSources.OfType<IConfigurableTab>().Any();

    public RelayCommand ConfigureCommand { get; }

    public bool IsEnabled
    {
        get => _isEnabled;
        set => Set(ref _isEnabled, value);
    }

    public PluginInfo()
    {
        ConfigureCommand = new RelayCommand(
            () => (Tabs.OfType<IConfigurableTab>().FirstOrDefault()
                   ?? (IConfigurableTab?)DataSources.OfType<IConfigurableTab>().FirstOrDefault())?.Configure(),
            () => CanConfigure && IsLoaded);
    }
}
