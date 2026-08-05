using System;
using System.Collections.Generic;
using OpcUaViewer.Core.Contracts;

namespace OpcUaViewer.Core.Services;

public static class DataSourceRegistry
{
    private static readonly Dictionary<Type, IDataSource> _sources = [];

    /// <summary>
    /// Register a data source under every IDataSource-derived interface it implements.
    /// Call this before instantiating tab plugins so their constructors can call Get&lt;T&gt;().
    /// </summary>
    public static void Register(IDataSource source)
    {
        foreach (var iface in source.GetType().GetInterfaces())
        {
            if (typeof(IDataSource).IsAssignableFrom(iface) && iface != typeof(IDataSource))
                _sources[iface] = source;
        }
        _sources[source.GetType()] = source;
    }

    /// <summary>Retrieve the registered instance for interface T, or null if none registered.</summary>
    public static T? Get<T>() where T : class, IDataSource
        => _sources.TryGetValue(typeof(T), out var s) ? s as T : null;

    public static IReadOnlyDictionary<Type, IDataSource> All => _sources;
}
