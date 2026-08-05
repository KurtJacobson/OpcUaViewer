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
        var dlls     = DiscoverDlls().ToList();

        // Pass 1: data-source plugins — must be registered before tab plugins instantiate
        foreach (string dll in dlls)
        {
            bool enabled = !disabled.Contains(dll, StringComparer.OrdinalIgnoreCase);
            if (!enabled) continue;

            var info = TryLoadDataSources(dll);
            if (info is null) continue;

            foreach (var ds in info.DataSources)
                DataSourceRegistry.Register(ds);

            Plugins.Add(info);
        }

        // Pass 2: tab plugins
        foreach (string dll in dlls)
        {
            // Skip DLLs already processed as data-source plugins
            if (Plugins.Any(p => string.Equals(p.FilePath, dll, StringComparison.OrdinalIgnoreCase)))
                continue;

            bool enabled = !disabled.Contains(dll, StringComparer.OrdinalIgnoreCase);

            PluginInfo? info;
            if (enabled)
            {
                info = TryLoadTabs(dll);
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

    // ── Pass-1: data source loader ────────────────────────────────────────────

    private static PluginInfo? TryLoadDataSources(string dll)
    {
        string shortName = Path.GetFileNameWithoutExtension(dll);

        Assembly asm;
        try { asm = Assembly.LoadFrom(dll); }
        catch { return null; }  // will be caught in pass 2 if it also has tabs

        try
        {
            var dsTypes = asm.GetExportedTypes()
                             .Where(t => typeof(IDataSource).IsAssignableFrom(t) && !t.IsAbstract)
                             .ToList();

            if (dsTypes.Count == 0) return null;

            // Bail if this DLL also has tabs — it will be handled in pass 2 together
            bool hasTabs = asm.GetExportedTypes()
                              .Any(t => typeof(IAppTab).IsAssignableFrom(t) && !t.IsAbstract);
            if (hasTabs) return null;

            var sources = new List<IDataSource>();
            foreach (var type in dsTypes)
            {
                IDataSource? ds = null;
                try { ds = (IDataSource?)Activator.CreateInstance(type); } catch { }
                if (ds is not null) sources.Add(ds);
            }

            if (sources.Count == 0) return null;

            string name = asm.GetName().Name ?? shortName;
            return new PluginInfo
            {
                Name        = name,
                FilePath    = dll,
                DataSources = sources,
                IsLoaded    = true,
                IsEnabled   = true,
            };
        }
        catch { return null; }
    }

    // ── Pass-2: tab loader ────────────────────────────────────────────────────

    private PluginInfo? TryLoadTabs(string dll)
    {
        string shortName = Path.GetFileNameWithoutExtension(dll);

        Assembly asm;
        try { asm = Assembly.LoadFrom(dll); }
        catch (Exception ex) { return Failed(dll, shortName, ex); }

        try
        {
            var tabTypes = asm.GetExportedTypes()
                             .Where(t => typeof(IAppTab).IsAssignableFrom(t) && !t.IsAbstract)
                             .ToList();

            if (tabTypes.Count == 0) return null;  // not a plugin DLL, skip silently

            foreach (var rd in FindResources(asm))
                _app.Resources.MergedDictionaries.Add(rd);

            var tabs    = new List<IAppTab>();
            var sources = new List<IDataSource>();

            // Instantiate any co-located data sources first and register them
            var dsTypes = asm.GetExportedTypes()
                             .Where(t => typeof(IDataSource).IsAssignableFrom(t) && !t.IsAbstract)
                             .ToList();
            foreach (var type in dsTypes)
            {
                IDataSource? ds = null;
                try { ds = (IDataSource?)Activator.CreateInstance(type); } catch { }
                if (ds is not null)
                {
                    sources.Add(ds);
                    DataSourceRegistry.Register(ds);
                }
            }

            foreach (var type in tabTypes)
            {
                IAppTab? tab = null;
                try { tab = (IAppTab?)Activator.CreateInstance(type, _opc); } catch { }
                if (tab is null)
                    try { tab = (IAppTab?)Activator.CreateInstance(type); } catch { }
                if (tab is not null) tabs.Add(tab);
            }

            if (tabs.Count == 0) return null;

            string name = asm.GetName().Name ?? shortName;
            return new PluginInfo
            {
                Name        = name,
                FilePath    = dll,
                Tabs        = tabs,
                DataSources = sources,
                IsLoaded    = true,
                IsEnabled   = true,
            };
        }
        catch (Exception ex) { return Failed(dll, shortName, ex); }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static PluginInfo Failed(string dll, string name, Exception ex) =>
        Failed(dll, name, ex.GetBaseException().Message);

    private static PluginInfo Failed(string dll, string name, string message) => new()
    {
        Name      = name,
        FilePath  = dll,
        IsLoaded  = false,
        IsEnabled = false,
        LoadError = message,
    };

    private static PluginInfo? BuildUnloaded(string dll)
    {
        try
        {
            var ctx  = new System.Runtime.Loader.AssemblyLoadContext(null, isCollectible: true);
            var asm  = ctx.LoadFromAssemblyPath(dll);
            bool hasPlugin = asm.GetExportedTypes()
                               .Any(t => (typeof(IAppTab).IsAssignableFrom(t) ||
                                          typeof(IDataSource).IsAssignableFrom(t))
                                         && !t.IsAbstract);
            ctx.Unload();

            if (!hasPlugin) return null;

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
