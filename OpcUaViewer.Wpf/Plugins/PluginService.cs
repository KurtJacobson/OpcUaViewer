using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Markup;
using OpcUaViewer.Core.Contracts;
using OpcUaViewer.Core.Services;
using OpcUaViewer.Core.Settings;

namespace OpcUaViewer.Wpf.Plugins;

public sealed class PluginService
{
    private readonly OpcUaService _opc;
    private readonly Application  _app;

    public ObservableCollection<PluginInfo> Plugins { get; } = [];

    private static IEnumerable<string> PluginDirectories =>
    [
        Path.Combine(AppContext.BaseDirectory, "plugins"),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                     "OpcUaViewer", "plugins"),
    ];

    public PluginService(OpcUaService opc, Application app)
    {
        _opc = opc;
        _app = app;
    }

    // ── Initial load at startup ───────────────────────────────────────────────

    public IEnumerable<IAppTab> LoadAll()
    {
        var disabled = AppSettings.Current.DisabledPlugins;

        foreach (string dll in DiscoverDlls())
        {
            bool enabled = !disabled.Contains(dll, StringComparer.OrdinalIgnoreCase);

            PluginInfo? info;
            if (enabled)
            {
                info = TryLoad(dll);
                if (info is null) continue;
                foreach (var tab in info.Tabs) yield return tab;
            }
            else
            {
                info = BuildUnloaded(dll);
                if (info is null) continue;
            }

            Plugins.Add(info);
        }
    }

    public void SaveEnabledState()
    {
        var disabled = AppSettings.Current.DisabledPlugins;
        disabled.Clear();
        foreach (var p in Plugins.Where(p => !p.IsEnabled))
            disabled.Add(p.FilePath);
        AppSettings.Save();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private PluginInfo? TryLoad(string dll)
    {
        Assembly asm;
        try { asm = Assembly.LoadFrom(dll); }
        catch { return null; }

        foreach (var rd in FindResources(asm))
            _app.Resources.MergedDictionaries.Add(rd);

        var tabs = new List<IAppTab>();
        foreach (var type in asm.GetExportedTypes())
        {
            if (!typeof(IAppTab).IsAssignableFrom(type) || type.IsAbstract) continue;
            IAppTab? tab = null;
            try { tab = (IAppTab?)Activator.CreateInstance(type, _opc); } catch { }
            if (tab is null)
                try { tab = (IAppTab?)Activator.CreateInstance(type); } catch { }
            if (tab is not null) tabs.Add(tab);
        }

        if (tabs.Count == 0) return null;

        string name = asm.GetName().Name ?? Path.GetFileNameWithoutExtension(dll);
        return new PluginInfo
        {
            Name      = name,
            FilePath  = dll,
            Tabs      = tabs,
            IsLoaded  = true,
            IsEnabled = true,
        };
    }

    private static PluginInfo? BuildUnloaded(string dll)
    {
        try
        {
            // Load metadata only to verify the assembly contains at least one IAppTab
            var ctx  = new System.Runtime.Loader.AssemblyLoadContext(null, isCollectible: true);
            var asm  = ctx.LoadFromAssemblyPath(dll);
            bool hasTab = asm.GetExportedTypes()
                            .Any(t => typeof(IAppTab).IsAssignableFrom(t) && !t.IsAbstract);
            ctx.Unload();

            if (!hasTab) return null;

            string name = AssemblyName.GetAssemblyName(dll).Name
                          ?? Path.GetFileNameWithoutExtension(dll);
            return new PluginInfo
            {
                Name      = name,
                FilePath  = dll,
                IsLoaded  = false,
                IsEnabled = false,
            };
        }
        catch { return null; }
    }

    private static IEnumerable<string> DiscoverDlls() =>
        PluginDirectories
            .Where(Directory.Exists)
            .SelectMany(d => Directory.GetFiles(d, "*.dll", SearchOption.AllDirectories));

    private static IEnumerable<ResourceDictionary> FindResources(Assembly asm)
    {
        foreach (string name in asm.GetManifestResourceNames())
        {
            if (!name.EndsWith(".xaml", StringComparison.OrdinalIgnoreCase)) continue;
            ResourceDictionary? rd = null;
            try
            {
                using var stream = asm.GetManifestResourceStream(name)!;
                rd = XamlReader.Load(stream) as ResourceDictionary;
            }
            catch { }
            if (rd is not null) yield return rd;
        }
    }
}
