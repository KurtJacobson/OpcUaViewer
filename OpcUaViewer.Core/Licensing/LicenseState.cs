namespace OpcUaViewer.Core.Licensing;

/// <summary>
/// Holds the validated license for the current session.
/// Set once during startup after <see cref="LicenseValidator.Load"/> succeeds.
/// </summary>
public static class LicenseState
{
    public static LicenseInfo? Current { get; set; }
}
