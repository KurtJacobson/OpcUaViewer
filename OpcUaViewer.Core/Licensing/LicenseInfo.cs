using System;

namespace OpcUaViewer.Core.Licensing;

public record LicenseInfo(
    string    Licensee,
    string    Address,
    DateTime? ValidUntil,        // null = perpetual
    DateTime  MaintenanceUntil,
    string    Notes,
    DateTime  IssuedDate)
{
    public bool IsPerpetual      => ValidUntil is null;
    public bool IsExpired        => ValidUntil.HasValue && ValidUntil.Value.Date < DateTime.Today;
    public bool MaintenanceActive => MaintenanceUntil.Date >= DateTime.Today;
}
