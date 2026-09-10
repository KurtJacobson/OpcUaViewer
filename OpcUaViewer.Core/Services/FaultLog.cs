using System;
using System.IO;
using Microsoft.Data.Sqlite;
using OpcUaViewer.Core.Settings;

namespace OpcUaViewer.Core.Services;

/// <summary>
/// Persists machine fault events and part-formed events to a local SQLite database.
/// Designed to be the source of truth for write-back to MES/ERP when that integration lands.
/// Thread-safe for concurrent reads; writes are serialised via lock.
/// </summary>
public sealed class FaultLog : IDisposable
{
    public static readonly string DefaultDbPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "FoldControl", "events.db");

    private readonly SqliteConnection _conn;
    private readonly object           _lock = new();

    // Track the open fault so we can close it when state clears
    private long?    _openFaultId;
    private DateTime _faultStartUtc;

    public FaultLog(string? dbPath = null)
    {
        string path = dbPath ?? DefaultDbPath;
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        _conn = new SqliteConnection($"Data Source={path}");
        _conn.Open();
        EnsureSchema();
    }

    // ── Schema ─────────────────────────────────────────────────────────────────

    private void EnsureSchema()
    {
        Exec(@"
            CREATE TABLE IF NOT EXISTS FaultEvents (
                Id          INTEGER PRIMARY KEY AUTOINCREMENT,
                MachineId   TEXT    NOT NULL,
                MachineName TEXT    NOT NULL,
                SiteCode    TEXT    NOT NULL,
                FaultStart  TEXT    NOT NULL,  -- ISO-8601 UTC
                FaultCleared TEXT,             -- ISO-8601 UTC; NULL = still active
                DurationSec REAL,              -- NULL until cleared
                ErrorCode   TEXT,
                FaultReason TEXT,
                Published   INTEGER NOT NULL DEFAULT 0  -- 0 = pending push to MES
            );
            CREATE TABLE IF NOT EXISTS PartFormedEvents (
                Id          INTEGER PRIMARY KEY AUTOINCREMENT,
                MachineId   TEXT    NOT NULL,
                MachineName TEXT    NOT NULL,
                SiteCode    TEXT    NOT NULL,
                FormedAt    TEXT    NOT NULL,  -- ISO-8601 UTC
                PartNumber  TEXT    NOT NULL,
                Quantity    INTEGER NOT NULL DEFAULT 1,
                Published   INTEGER NOT NULL DEFAULT 0
            );");
    }

    // ── Fault tracking ─────────────────────────────────────────────────────────

    /// <summary>
    /// Call when the machine transitions INTO a faulted state.
    /// </summary>
    public void RecordFaultStart(string? errorCode = null, string? faultReason = null)
    {
        var s    = AppSettings.Current;
        var now  = DateTime.UtcNow;
        lock (_lock)
        {
            if (_openFaultId.HasValue) return;   // already tracking one

            _faultStartUtc = now;
            _openFaultId   = ExecScalar<long>(@"
                INSERT INTO FaultEvents (MachineId, MachineName, SiteCode, FaultStart, ErrorCode, FaultReason)
                VALUES ($mid, $mname, $site, $start, $code, $reason);
                SELECT last_insert_rowid();",
                ("$mid",   s.MachineId),
                ("$mname", s.MachineName),
                ("$site",  s.SiteCode),
                ("$start", Iso(now)),
                ("$code",  errorCode   ?? (object)DBNull.Value),
                ("$reason", faultReason ?? (object)DBNull.Value));
        }
    }

    /// <summary>
    /// Call when the machine leaves the faulted state.
    /// </summary>
    public void RecordFaultCleared()
    {
        lock (_lock)
        {
            if (!_openFaultId.HasValue) return;

            var now     = DateTime.UtcNow;
            double secs = (now - _faultStartUtc).TotalSeconds;

            Exec(@"UPDATE FaultEvents
                   SET FaultCleared = $cleared, DurationSec = $dur
                   WHERE Id = $id",
                ("$cleared", Iso(now)),
                ("$dur",     secs),
                ("$id",      _openFaultId.Value));

            _openFaultId = null;
        }
    }

    // ── Part formed ────────────────────────────────────────────────────────────

    public void RecordPartFormed(string partNumber, int quantity = 1)
    {
        var s   = AppSettings.Current;
        var now = DateTime.UtcNow;
        lock (_lock)
        {
            Exec(@"INSERT INTO PartFormedEvents (MachineId, MachineName, SiteCode, FormedAt, PartNumber, Quantity)
                   VALUES ($mid, $mname, $site, $at, $part, $qty)",
                ("$mid",   s.MachineId),
                ("$mname", s.MachineName),
                ("$site",  s.SiteCode),
                ("$at",    Iso(now)),
                ("$part",  partNumber),
                ("$qty",   quantity));
        }
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    private static string Iso(DateTime utc) => utc.ToString("yyyy-MM-ddTHH:mm:ssZ");

    private void Exec(string sql, params (string name, object value)[] parms)
    {
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = sql;
        foreach (var (name, value) in parms)
            cmd.Parameters.AddWithValue(name, value);
        cmd.ExecuteNonQuery();
    }

    private T ExecScalar<T>(string sql, params (string name, object value)[] parms)
    {
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = sql;
        foreach (var (name, value) in parms)
            cmd.Parameters.AddWithValue(name, value);
        return (T)cmd.ExecuteScalar()!;
    }

    public void Dispose() => _conn.Dispose();
}
