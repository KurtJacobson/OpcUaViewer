using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace OpcUaViewer.Core.Licensing;

public static class LicenseValidator
{
    // Public key matching the private key held by the license tool (developer only).
    private const string PublicKeyPem = @"-----BEGIN PUBLIC KEY-----
MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAyAItVPNU4kV1U2kR5TBK
3FA97dlx6RYBXGFSImnlEOZkuxjEfcUg27dL5JuLJFrfnjmsgdXzT7InsA6uXhFg
Mgdwvy2fBZEMI02A2RjFtUMpOVGBnb7NQDEkUnvN9DyRcfQUoRxS8aQWKW6qhIiH
TVgY4SehSf6r5QOxQ+em69EaYe69r0cPmXs2qQ4/aYZqd9C3u8G4Mfi/wo66+LEy
a70ipssNHPdyjiB1JgOWADZzeKFNO89Mn5oAmnt9f7kNnZsRs/CBptrtNE2iQ7G3
BLvNykmtR3gi/aLMvls3beLLscVdzKen5931PQJSEwDJ7dr6gHjY2OCHsECxguyQ
0wIDAQAB
-----END PUBLIC KEY-----";

    private const string BeginLicense   = "-----BEGIN FOLD CONTROL LICENSE-----";
    private const string EndLicense     = "-----END FOLD CONTROL LICENSE-----";
    private const string BeginSignature = "-----BEGIN LICENSE SIGNATURE-----";
    private const string EndSignature   = "-----END LICENSE SIGNATURE-----";

    public static readonly string DefaultLicensePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
        "MetalForming LLC", "Fold Control", "license.lic");

    /// <summary>
    /// Loads and validates a license file. Returns the LicenseInfo on success.
    /// Throws <see cref="LicenseException"/> if the file is missing, tampered, or expired.
    /// </summary>
    public static LicenseInfo Load(string? path = null)
    {
        path ??= DefaultLicensePath;
        if (!File.Exists(path))
            throw new LicenseException("License file not found.");

        string text = File.ReadAllText(path, Encoding.UTF8);

        string block = ExtractBlock(text, BeginLicense, EndLicense)
            ?? throw new LicenseException("License file is missing the license block.");
        string sigBlock = ExtractBlock(text, BeginSignature, EndSignature)
            ?? throw new LicenseException("License file is missing the signature block.");

        // Verify RSA signature over the exact bytes of the license block content
        byte[] blockBytes = Encoding.UTF8.GetBytes(block);
        byte[] sigBytes;
        try   { sigBytes = Convert.FromBase64String(sigBlock.Replace("\r", "").Replace("\n", "")); }
        catch { throw new LicenseException("License signature is malformed."); }

        using var rsa = RSA.Create();
        rsa.ImportFromPem(PublicKeyPem);

        if (!rsa.VerifyData(blockBytes, sigBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1))
            throw new LicenseException("License signature is invalid. The file may have been tampered with.");

        // Parse key-value fields (continuation lines indented with whitespace)
        var fields = ParseFields(block);

        string  licensee  = Require(fields, "Licensee");
        string  address   = fields.GetValueOrDefault("Address", "");
        string  notes     = fields.GetValueOrDefault("Notes", "");
        string  validStr  = Require(fields, "Valid Until");
        string  maintStr  = Require(fields, "Maintenance Until");
        string  issued    = Require(fields, "Issued");
        string[] options  = fields.TryGetValue("Options", out var optStr)
            ? optStr.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            : [];

        DateTime? validUntil = validStr.Equals("Perpetual", StringComparison.OrdinalIgnoreCase)
            ? null
            : ParseDate(validStr, "Valid Until");

        DateTime maintenanceUntil = ParseDate(maintStr, "Maintenance Until");
        DateTime issuedDate       = ParseDate(issued,   "Issued");

        var info = new LicenseInfo(licensee, address, validUntil, maintenanceUntil, notes, issuedDate, options);

        if (info.IsExpired)
            throw new LicenseException($"This license expired on {info.ValidUntil!.Value:yyyy-MM-dd}.");

        return info;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    // Returns the content between the BEGIN/END markers (exclusive), or null if not found.
    private static string? ExtractBlock(string text, string begin, string end)
    {
        int s = text.IndexOf(begin, StringComparison.Ordinal);
        int e = text.IndexOf(end,   StringComparison.Ordinal);
        if (s < 0 || e < 0 || e <= s) return null;
        s += begin.Length;
        // Normalise to LF-only so the signed bytes are consistent across platforms
        return text[s..e].Replace("\r\n", "\n").TrimStart('\n').TrimEnd('\n', ' ');
    }

    // Parses "Key:  Value\n    continuation" lines into a dictionary.
    // Multi-line values (address, notes) are joined with "\n".
    private static Dictionary<string, string> ParseFields(string block)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        string? currentKey = null;
        var sb = new StringBuilder();

        foreach (var raw in block.Split('\n'))
        {
            string line = raw.TrimEnd();
            if (string.IsNullOrEmpty(line)) continue;

            // Continuation line: starts with whitespace
            if (currentKey != null && line.Length > 0 && (line[0] == ' ' || line[0] == '\t'))
            {
                sb.Append('\n').Append(line.Trim());
                continue;
            }

            // Save previous field
            if (currentKey != null)
                result[currentKey] = sb.ToString();

            // New key: value line
            int colon = line.IndexOf(':');
            if (colon < 0) { currentKey = null; continue; }

            currentKey = line[..colon].Trim();
            sb.Clear();
            sb.Append(line[(colon + 1)..].Trim());
        }

        if (currentKey != null)
            result[currentKey] = sb.ToString();

        return result;
    }

    private static string Require(Dictionary<string, string> f, string key) =>
        f.TryGetValue(key, out var v) && !string.IsNullOrWhiteSpace(v)
            ? v
            : throw new LicenseException($"License is missing required field: {key}");

    private static DateTime ParseDate(string s, string field) =>
        DateTime.TryParse(s.Trim(), out var d)
            ? d.Date
            : throw new LicenseException($"License field '{field}' contains an invalid date: {s}");

    // ── File writer (used by the license tool) ────────────────────────────────

    /// <summary>
    /// Builds and signs a license file, returning the complete text content.
    /// </summary>
    public static string CreateLicenseText(
        string    licensee,
        string    address,
        DateTime? validUntil,
        DateTime  maintenanceUntil,
        DateTime  issuedDate,
        string    notes,
        string    options,
        string    privateKeyPem)
    {
        // Build the human-readable block
        var lines = new List<string>();
        lines.Add($"Licensee:          {licensee}");

        if (!string.IsNullOrWhiteSpace(address))
        {
            var addrLines = address.Replace("\r\n", "\n").Split('\n');
            lines.Add($"Address:           {addrLines[0].Trim()}");
            for (int i = 1; i < addrLines.Length; i++)
                if (!string.IsNullOrWhiteSpace(addrLines[i]))
                    lines.Add($"                   {addrLines[i].Trim()}");
        }

        lines.Add($"Valid Until:       {(validUntil.HasValue ? validUntil.Value.ToString("yyyy-MM-dd") : "Perpetual")}");
        lines.Add($"Maintenance Until: {maintenanceUntil:yyyy-MM-dd}");
        lines.Add($"Issued:            {issuedDate:yyyy-MM-dd}");

        if (!string.IsNullOrWhiteSpace(notes))
        {
            var noteLines = notes.Replace("\r\n", "\n").Split('\n');
            lines.Add($"Notes:             {noteLines[0].Trim()}");
            for (int i = 1; i < noteLines.Length; i++)
                if (!string.IsNullOrWhiteSpace(noteLines[i]))
                    lines.Add($"                   {noteLines[i].Trim()}");
        }

        if (!string.IsNullOrWhiteSpace(options))
            lines.Add($"Options:           {options.Trim()}");

        string block = string.Join("\n", lines);

        // Sign the block bytes
        using var rsa = RSA.Create();
        rsa.ImportFromPem(privateKeyPem);

        byte[] blockBytes = Encoding.UTF8.GetBytes(block);
        byte[] sigBytes   = rsa.SignData(blockBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        string sig        = Convert.ToBase64String(sigBytes, Base64FormattingOptions.InsertLineBreaks);

        return $"{BeginLicense}\n{block}\n{EndLicense}\n\n{BeginSignature}\n{sig}\n{EndSignature}\n";
    }
}

public class LicenseException(string message) : Exception(message);
