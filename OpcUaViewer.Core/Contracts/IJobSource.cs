using System;
using System.Collections.Generic;

namespace OpcUaViewer.Core.Contracts;

/// <summary>
/// Represents a job/work-order record pushed from an ERP or defined locally.
/// </summary>
public record JobRecord(
    string    JobNumber,
    string    PartNumber,
    string    PartDescription  = "",
    string    DrawingRevision  = "",
    int       OperationSeq     = 0,
    string    OperationDesc    = "",
    string    ProductGroup     = "",
    string    Material         = "",
    string    Thickness        = "",
    int       QuantityPlanned  = 0,
    int       QuantityComplete = 0,
    string    Customer         = "",
    DateTime? DueDate          = null,
    string    Notes            = "");

/// <summary>
/// Data source that provides job/work-order data to tab plugins.
/// The first implementation will be a local JSON file; a future implementation
/// can pull from an ERP via REST, database, or file drop.
/// Retrieve via DataSourceRegistry.Get&lt;IJobSource&gt;().
/// </summary>
public interface IJobSource : IDataSource
{
    /// <summary>All currently known jobs, keyed by part number.</summary>
    IReadOnlyDictionary<string, JobRecord> Jobs { get; }

    /// <summary>Look up a job by part number. Returns null if not found.</summary>
    JobRecord? GetByPartNumber(string partNumber);

    /// <summary>Fired when the job list is refreshed (e.g. ERP push or file reload).</summary>
    event EventHandler? JobsRefreshed;
}
