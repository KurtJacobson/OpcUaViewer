using System;

namespace OpcUaViewer.Core.Licensing;

public record LicenseOption(string Key, DateTime? ExpiresOn)
{
    public bool IsActive => ExpiresOn is null || ExpiresOn.Value.Date >= DateTime.Today;
}

public record LicenseInfo(
    int             LicVersion,
    string          Licensee,
    string          Address,
    DateTime?       ValidUntil,        // null = perpetual
    DateTime        MaintenanceUntil,
    string          Notes,
    DateTime        IssuedDate,
    LicenseOption[] Options)
{
    public bool IsPerpetual       => ValidUntil is null;
    public bool IsExpired         => ValidUntil.HasValue && ValidUntil.Value.Date < DateTime.Today;
    public bool MaintenanceActive => MaintenanceUntil.Date >= DateTime.Today;

    public bool HasOption(string key) =>
        Array.Exists(Options, o =>
            o.Key.Equals(key.Trim(), StringComparison.OrdinalIgnoreCase) && o.IsActive);
}
