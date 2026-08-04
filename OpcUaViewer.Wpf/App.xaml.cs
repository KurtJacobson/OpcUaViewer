using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Markup;
using Microsoft.Web.WebView2.Core;
using OpcUaViewer.Core.Contracts;
using OpcUaViewer.Core.Services;
using OpcUaViewer.Core.Settings;
using OpcUaViewer.Wpf.Dialogs;
using OpcUaViewer.Wpf.Tabs;
using OpcUaViewer.Wpf.ViewModels;

namespace OpcUaViewer.Wpf;

public partial class App : Application
{
    private OpcUaService? _opc;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        AppSettings.Load();
        DialogService.Current = new WpfDialogService();

        // Pre-warm the WebView2 browser process so it's ready before the Document tab is opened
        _ = CoreWebView2Environment.CreateAsync();

        _opc = new OpcUaService();

        IAppTab[] builtIn =
        [
            new MonitorTab(_opc),
            new SettingsTab(),
        ];

        var allTabs = new List<IAppTab>(builtIn);
        allTabs.AddRange(LoadPlugins(_opc));

        var monitorTab = (MonitorTab)builtIn[0];
        var vm     = new MainViewModel(allTabs);
        var window = new MainWindow(vm);
        window.Show();

        if (!string.IsNullOrWhiteSpace(AppSettings.Current.EndpointUrl))
            monitorTab.ConnectCommand.Execute(null);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _opc?.Dispose();
        base.OnExit(e);
    }

    // ── Plugin loader ─────────────────────────────────────────────────────────

    private IEnumerable<IAppTab> LoadPlugins(OpcUaService opc)
    {
        // Built-in plugins ship alongside the executable in a "plugins" subfolder
        string appPluginDir = Path.Combine(AppContext.BaseDirectory, "plugins");

        // Customer plugins live in AppData so updates don't wipe them
        string userPluginDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "OpcUaViewer", "plugins");

        var searchDirs = new[] { appPluginDir, userPluginDir };

        foreach (string dll in searchDirs
            .Where(Directory.Exists)
            .SelectMany(d => Directory.GetFiles(d, "*.dll", SearchOption.AllDirectories)))
        {
            Assembly asm;
            try { asm = Assembly.LoadFrom(dll); }
            catch { continue; }

            // Merge any ResourceDictionary embedded in the plugin
            foreach (var rd in FindPluginResources(asm))
                Resources.MergedDictionaries.Add(rd);

            // Instantiate every IAppTab the plugin exports
            foreach (var type in asm.GetExportedTypes())
            {
                if (!typeof(IAppTab).IsAssignableFrom(type) || type.IsAbstract) continue;

                IAppTab? tab = null;
                try { tab = (IAppTab?)Activator.CreateInstance(type, opc); }
                catch { }

                if (tab is null)
                {
                    try { tab = (IAppTab?)Activator.CreateInstance(type); }
                    catch { }
                }

                if (tab is not null) yield return tab;
            }
        }
    }

    private static IEnumerable<ResourceDictionary> FindPluginResources(Assembly asm)
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
