using System.Collections.Generic;
using System.Linq;
using OpcUaViewer.Core.Contracts;
using OpcUaViewer.Wpf.Infrastructure;

namespace OpcUaViewer.Wpf.Plugins;

public sealed class PluginInfo : ViewModelBase
{
    private bool _isEnabled;

    public string Name     { get; init; } = "";
    public string FilePath { get; init; } = "";
    public bool   IsLoaded { get; init; }

    public IReadOnlyList<IAppTab> Tabs { get; init; } = [];

    public string Description => Tabs.FirstOrDefault(t => !string.IsNullOrEmpty(t.Description))?.Description ?? "";

    public string TabList => Tabs.Count > 0
        ? string.Join(", ", Tabs.Select(t => t.Title))
        : "—";

    public bool IsEnabled
    {
        get => _isEnabled;
        set => Set(ref _isEnabled, value);
    }
}
