using System;

namespace OpcUaViewer.Core.Licensing;

public record LicenseInfo(
    string    Licensee,
    string    Address,
    DateTime? ValidUntil,        // null = perpetual
    DateTime  MaintenanceUntil,
    string    Notes,
    DateTime  IssuedDate,
    string[]  Options)
{
    public bool IsPerpetual       => ValidUntil is null;
    public bool IsExpired         => ValidUntil.HasValue && ValidUntil.Value.Date < DateTime.Today;
    public bool MaintenanceActive => MaintenanceUntil.Date >= DateTime.Today;

    public bool HasOption(string key) =>
        Array.Exists(Options, o => o.Equals(key.Trim(), StringComparison.OrdinalIgnoreCase));
}
