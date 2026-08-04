using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Opc.Ua;
using Opc.Ua.Client;
using Opc.Ua.Configuration;

namespace OpcUaViewer.Core.Services;

public record MonitoredNodeInfo(string Name, string NodeIdStr);

public class NodeValueEventArgs(string name, object? rawValue, string statusCode, DateTime timestamp) : EventArgs
{
    public string   Name       { get; } = name;
    public object?  RawValue   { get; } = rawValue;
    public string   StrValue   { get; } = rawValue?.ToString() ?? "";
    public string   StatusCode { get; } = statusCode;
    public DateTime Timestamp  { get; } = timestamp;
}

/// <summary>
/// Manages OPC UA connection, browsing, and subscription. All events fire on background threads;
/// subscribers are responsible for dispatching to the UI thread.
/// </summary>
public class OpcUaService : IDisposable
{
    // ── Configuration ─────────────────────────────────────────────────────────
    public string MonitoringFolderPath     { get; set; } = "4:PLC/6:Modules/6:::/6:Global PV/6:Monitoring";
    public string ProductIdNodeMatch       { get; set; } = "ProductId";
    public string CamFileNodeMatch         { get; set; } = "CAMFileInProcess";
    public string MachineStateNodeMatch    { get; set; } = "CurrentMachineState";
    public ushort ExtraNodeNamespace       { get; set; } = 6;
    public string OperatorActionNodePath   { get; set; } = "::AsGlobalPV:Monitoring.OperatorActionRequested";

    // Stats nodes (same namespace as OperatorAction)
    public string StatsAutoModeNode       { get; set; } = "::AsGlobalPV:Monitoring.AutomaticMode";
    public string StatsManualModeNode     { get; set; } = "::AsGlobalPV:Monitoring.ManualMode";
    public string StatsSetupModeNode      { get; set; } = "::AsGlobalPV:Monitoring.SetupMode";
    public string StatsTotalHoursNode     { get; set; } = "::AsGlobalPV:Monitoring.TotalOperatingHours";
    public string StatsProducingHoursNode { get; set; } = "::AsGlobalPV:Monitoring.TotalOperatingHoursProducing";
    public string StatsPartCountNode      { get; set; } = "::AsGlobalPV:Monitoring.CurrentProductCount";
    public string StatsCurrentStepNode    { get; set; } = "::AsGlobalPV:Monitoring.CurrentProductionStep";
    public string StatsBendingNowNode     { get; set; } = "::AsGlobalPV:Monitoring.BendingNow";

    // ── Events ─────────────────────────────────────────────────────────────────
    public event EventHandler<string>?                   StatusChanged;
    public event EventHandler<IReadOnlyList<MonitoredNodeInfo>>? NodesDiscovered;
    public event EventHandler<NodeValueEventArgs>?       NodeValueUpdated;
    public event EventHandler<string>?                   ProductIdChanged;
    public event EventHandler<string>?                   CamFileChanged;
    public event EventHandler<int>?                      MachineStateChanged;
    public event EventHandler<bool>?                     OperatorActionChanged;
    public event EventHandler<NodeValueEventArgs>?       StatsValueChanged;

    // ── State ──────────────────────────────────────────────────────────────────
    public bool IsConnected => _session != null;

    private ISession?    _session;
    private Subscription? _subscription;
    private CancellationTokenSource? _cts;

    private readonly Dictionary<uint, string> _handleToName = new();
    private uint? _productIdHandle, _camFileHandle, _machineStateHandle, _opActionHandle;
    private readonly HashSet<uint> _statsHandles = [];

    private IReadOnlyList<MonitoredNodeInfo> _nodes = [];

    // ── Public API ─────────────────────────────────────────────────────────────

    public async Task ConnectAsync(string endpointUrl, CancellationToken ct = default)
    {
        RaiseStatus("Connecting...");
        try
        {
            var config = BuildConfig();
            await config.Validate(ApplicationType.Client);
            config.CertificateValidator.CertificateValidation += (_, e) => e.Accept = true;

            var probeTask = Task.Run(() => CoreClientUtils.SelectEndpoint(config, endpointUrl, useSecurity: false), ct);
            await Task.WhenAny(probeTask, Task.Delay(Timeout.Infinite, ct));
            ct.ThrowIfCancellationRequested();
            var endpoint = await probeTask;

            _session = await new DefaultSessionFactory().CreateAsync(
                config,
                new ConfiguredEndpoint(null, endpoint, EndpointConfiguration.Create(config)),
                updateBeforeConnect: false,
                sessionName: "OpcUaViewer",
                sessionTimeout: 60_000,
                identity: new UserIdentity(),
                preferredLocales: null,
                ct: ct);

            RaiseStatus($"Connected — browsing {MonitoringFolderPath}…");

            await Task.Run(() =>
            {
                DiscoverNodes();
                CreateSubscription();
            }, ct);

            RaiseStatus(_nodes.Count > 0
                ? $"Connected to {endpoint.EndpointUrl} ({_nodes.Count} items)"
                : $"Connected to {endpoint.EndpointUrl} — no items at configured path");
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            RaiseStatus("Not connected");
            throw new OpcUaConnectionException(ex.InnerException?.Message ?? ex.Message, ex);
        }
    }

    public void Disconnect()
    {
        _cts?.Cancel();
        try
        {
            if (_subscription != null && _session != null)
            {
                _session.RemoveSubscription(_subscription);
                _subscription.Dispose();
            }
            _session?.Close();
            _session?.Dispose();
        }
        catch { }
        finally
        {
            _session      = null;
            _subscription = null;
            _handleToName.Clear();
            _statsHandles.Clear();
            _productIdHandle = _camFileHandle = _machineStateHandle = _opActionHandle = null;
            _nodes = [];
            RaiseStatus("Disconnected");
        }
    }

    public void Dispose() => Disconnect();

    public CancellationTokenSource NewCts()
    {
        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        return _cts;
    }

    // ── Private: discovery & subscription ─────────────────────────────────────

    private void DiscoverNodes()
    {
        List<MonitoredNodeInfo> nodes;
        try
        {
            var folderId = ResolveBrowsePath(MonitoringFolderPath);
            nodes = BrowseVariables(folderId)
                .Select(n => new MonitoredNodeInfo(n.Name, n.NodeId.ToString()))
                .ToList();
        }
        catch (Exception ex)
        {
            nodes = [];
            RaiseStatus($"Browse failed: {ex.InnerException?.Message ?? ex.Message}");
        }
        _nodes = nodes;
        NodesDiscovered?.Invoke(this, nodes);
    }

    private void CreateSubscription()
    {
        if (_session == null || _nodes.Count == 0) return;

        _subscription = new Subscription(_session.DefaultSubscription) { PublishingInterval = 500 };

        var rawNodes = BrowseVariables(ResolveBrowsePath(MonitoringFolderPath));

        for (int i = 0; i < rawNodes.Count; i++)
        {
            var node = rawNodes[i];
            var item = new MonitoredItem(_subscription.DefaultItem)
            {
                DisplayName      = node.Name,
                StartNodeId      = node.NodeId,
                AttributeId      = Attributes.Value,
                SamplingInterval = 500,
            };
            item.Notification += OnValueChanged;
            _subscription.AddItem(item);
            _handleToName[item.ClientHandle] = node.Name;

            string n = node.Name;
            if (n.Contains(ProductIdNodeMatch,    StringComparison.OrdinalIgnoreCase)) _productIdHandle     = item.ClientHandle;
            if (n.Contains(CamFileNodeMatch,      StringComparison.OrdinalIgnoreCase)) _camFileHandle       = item.ClientHandle;
            if (n.Contains(MachineStateNodeMatch, StringComparison.OrdinalIgnoreCase)) _machineStateHandle  = item.ClientHandle;
        }

        // Extra hardcoded nodes
        _opActionHandle = AddExtraItem(_subscription, OperatorActionNodePath, "OperatorActionRequested");

        AddStatsItem(_subscription, StatsAutoModeNode);
        AddStatsItem(_subscription, StatsManualModeNode);
        AddStatsItem(_subscription, StatsSetupModeNode);
        AddStatsItem(_subscription, StatsTotalHoursNode);
        AddStatsItem(_subscription, StatsProducingHoursNode);
        AddStatsItem(_subscription, StatsPartCountNode);
        AddStatsItem(_subscription, StatsCurrentStepNode);
        AddStatsItem(_subscription, StatsBendingNowNode);

        _session.AddSubscription(_subscription);
        _subscription.Create();
    }

    private uint? AddExtraItem(Subscription sub, string nodePath, string displayName)
    {
        try
        {
            var it = new MonitoredItem(sub.DefaultItem)
            {
                DisplayName      = displayName,
                StartNodeId      = new NodeId(nodePath, ExtraNodeNamespace),
                AttributeId      = Attributes.Value,
                SamplingInterval = 500,
            };
            it.Notification += OnValueChanged;
            sub.AddItem(it);
            _handleToName[it.ClientHandle] = displayName;
            return it.ClientHandle;
        }
        catch { return null; }
    }

    private void AddStatsItem(Subscription sub, string nodePath)
    {
        var h = AddExtraItem(sub, nodePath, nodePath.Split('.').Last());
        if (h.HasValue) _statsHandles.Add(h.Value);
    }

    private void OnValueChanged(MonitoredItem item, MonitoredItemNotificationEventArgs e)
    {
        foreach (var v in item.DequeueValues())
        {
            string name   = _handleToName.GetValueOrDefault(item.ClientHandle, "");
            string strVal = v.Value?.ToString() ?? "";
            var    args   = new NodeValueEventArgs(name, v.Value, v.StatusCode.ToString(),
                                v.SourceTimestamp != DateTime.MinValue ? v.SourceTimestamp : DateTime.UtcNow);

            // General node value event (for monitor grid)
            NodeValueUpdated?.Invoke(this, args);

            // Routed events
            if      (_productIdHandle    == item.ClientHandle) ProductIdChanged?.Invoke(this, strVal);
            else if (_camFileHandle      == item.ClientHandle) CamFileChanged?.Invoke(this, strVal);
            else if (_machineStateHandle == item.ClientHandle && int.TryParse(strVal, out int state))
                MachineStateChanged?.Invoke(this, state);
            else if (_opActionHandle     == item.ClientHandle)
            {
                bool waiting = v.Value is bool b ? b : strVal is "True" or "1";
                OperatorActionChanged?.Invoke(this, waiting);
            }
            else if (_statsHandles.Contains(item.ClientHandle))
                StatsValueChanged?.Invoke(this, args);
        }
    }

    // ── Private: OPC UA helpers ────────────────────────────────────────────────

    private NodeId ResolveBrowsePath(string relativePath)
    {
        var elements = relativePath.Split('/').Select(segment =>
        {
            int c = segment.IndexOf(':');
            if (c < 0) throw new FormatException($"Invalid segment '{segment}'");
            return new RelativePathElement
            {
                ReferenceTypeId = ReferenceTypeIds.HierarchicalReferences,
                IsInverse       = false,
                IncludeSubtypes = true,
                TargetName      = new QualifiedName(segment[(c + 1)..], ushort.Parse(segment[..c]))
            };
        }).ToList();

        var paths = new BrowsePathCollection
        {
            new BrowsePath
            {
                StartingNode = ObjectIds.ObjectsFolder,
                RelativePath = new RelativePath { Elements = new RelativePathElementCollection(elements) }
            }
        };

#pragma warning disable CS0618
        _session!.TranslateBrowsePathsToNodeIds(null, paths, out var results, out _);
#pragma warning restore CS0618

        if (results == null || results.Count == 0 || StatusCode.IsBad(results[0].StatusCode) || results[0].Targets.Count == 0)
            throw new Exception($"Could not resolve path '{relativePath}'");

        return ExpandedNodeId.ToNodeId(results[0].Targets[0].TargetId, _session.NamespaceUris);
    }

    private List<(string Name, NodeId NodeId)> BrowseVariables(NodeId folderId)
    {
        var found = new List<(string, NodeId)>();
#pragma warning disable CS0618
        _session!.Browse(null, null, folderId, 0u,
            BrowseDirection.Forward, ReferenceTypeIds.HierarchicalReferences, true,
            (uint)NodeClass.Variable, out _, out var refs);
#pragma warning restore CS0618

        foreach (var r in refs)
        {
            string name = !string.IsNullOrEmpty(r.DisplayName?.Text) ? r.DisplayName.Text : r.BrowseName.Name;
            found.Add((name, ExpandedNodeId.ToNodeId(r.NodeId, _session.NamespaceUris)));
        }
        return found;
    }

    private static ApplicationConfiguration BuildConfig() => new()
    {
        ApplicationName = "OPC UA Viewer",
        ApplicationType = ApplicationType.Client,
        SecurityConfiguration = new SecurityConfiguration
        {
            ApplicationCertificate     = new CertificateIdentifier { StoreType = CertificateStoreType.Directory, StorePath = "CertificateStores/UA_MachineDefault", SubjectName = "CN=OPC UA Viewer" },
            TrustedPeerCertificates    = new CertificateTrustList  { StoreType = CertificateStoreType.Directory, StorePath = "CertificateStores/UA Applications" },
            TrustedIssuerCertificates  = new CertificateTrustList  { StoreType = CertificateStoreType.Directory, StorePath = "CertificateStores/UA Certificate Authorities" },
            RejectedCertificateStore   = new CertificateTrustList  { StoreType = CertificateStoreType.Directory, StorePath = "CertificateStores/RejectedCertificates" },
            AutoAcceptUntrustedCertificates = true,
            RejectSHA1SignedCertificates    = false,
        },
        TransportQuotas     = new TransportQuotas { OperationTimeout = 15_000 },
        ClientConfiguration = new ClientConfiguration(),
        TraceConfiguration  = new TraceConfiguration { OutputFilePath = "opcua.log", TraceMasks = Utils.TraceMasks.None }
    };

    private void RaiseStatus(string msg) => StatusChanged?.Invoke(this, msg);
}

public class OpcUaConnectionException(string message, Exception? inner = null)
    : Exception(message, inner);
