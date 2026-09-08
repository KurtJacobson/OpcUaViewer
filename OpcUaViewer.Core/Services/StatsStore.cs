using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpcUaViewer.Core.Services;

public sealed class StatsStore
{
    public double BaselineTotalHours     { get; set; }
    public double BaselineProducingHours { get; set; }
    public int    TotalPartCount         { get; set; }
    public int    TotalBendCount         { get; set; }

    public List<double> GlobalCycleTimes      { get; set; } = [];
    public List<double> GlobalSetupTimes      { get; set; } = [];
    public List<double> GlobalPartToPartTimes { get; set; } = [];
    public List<double> GlobalBendingTimes    { get; set; } = [];

    public Dictionary<string, List<double>> JobCycleTimes      { get; set; } = [];
    public Dictionary<string, List<double>> JobSetupTimes      { get; set; } = [];
    public Dictionary<string, List<double>> JobPartToPartTimes { get; set; } = [];
    public Dictionary<string, List<double>> JobBendingTimes    { get; set; } = [];

    public Dictionary<string, List<double>> ProductCycleTimes      { get; set; } = [];
    public Dictionary<string, List<double>> ProductSetupTimes      { get; set; } = [];
    public Dictionary<string, List<double>> ProductPartToPartTimes { get; set; } = [];
    public Dictionary<string, List<double>> ProductBendingTimes    { get; set; } = [];

    [JsonIgnore] public string FilePath { get; private set; } = "";

    [JsonIgnore]
    public IEnumerable<string> AllProductKeys =>
        ProductCycleTimes.Keys
            .Union(ProductSetupTimes.Keys)
            .Union(ProductPartToPartTimes.Keys)
            .Union(ProductBendingTimes.Keys)
            .Distinct();

    private const int MaxGlobal = 1000;
    private const int MaxPerKey = 500;

    public static StatsStore Load()
    {
        string dir  = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FoldControl");
        string path = Path.Combine(dir, "stats.json");
        StatsStore store;
        try
        {
            store = File.Exists(path)
                ? JsonSerializer.Deserialize<StatsStore>(File.ReadAllText(path),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new()
                : new();
        }
        catch { store = new(); }
        store.FilePath = path;
        return store;
    }

    public void AddCycleTime(string jobKey, string productKey, double t)
    {
        TotalPartCount++;
        AddTo(GlobalCycleTimes, JobCycleTimes, ProductCycleTimes, jobKey, productKey, t);
    }

    /// <summary>Fired after BulkImportCycles completes so subscribers can reload the store.</summary>
    public static event EventHandler? BulkImportCompleted;

    /// <summary>Import many cycle records at once, saving only once at the end.</summary>
    public void BulkImportCycles(IEnumerable<(string JobKey, string ProductKey, double Seconds)> records)
    {
        foreach (var (job, product, seconds) in records)
        {
            TotalPartCount++;
            GlobalCycleTimes.Add(seconds);
            if (GlobalCycleTimes.Count > MaxGlobal) GlobalCycleTimes.RemoveAt(0);
            AddToDict(JobCycleTimes,     job,     seconds);
            AddToDict(ProductCycleTimes, product, seconds);
        }
        Save();
        BulkImportCompleted?.Invoke(null, EventArgs.Empty);
    }

    public void IncrementBendCount() { TotalBendCount++; Save(); }
    public void ResetPartCount()     { TotalPartCount = 0; Save(); }
    public void ResetBendCount()     { TotalBendCount = 0; Save(); }

    public void AddSetupTime(string j, string p, double t)     => AddTo(GlobalSetupTimes,      JobSetupTimes,      ProductSetupTimes,      j, p, t);
    public void AddPartToPartTime(string j, string p, double t) => AddTo(GlobalPartToPartTimes, JobPartToPartTimes, ProductPartToPartTimes, j, p, t);
    public void AddBendingTime(string j, string p, double t)   => AddTo(GlobalBendingTimes,    JobBendingTimes,    ProductBendingTimes,    j, p, t);

    private void AddTo(List<double> global, Dictionary<string, List<double>> perJob, Dictionary<string, List<double>> perProduct,
        string jobKey, string productKey, double value)
    {
        global.Add(value);
        if (global.Count > MaxGlobal) global.RemoveAt(0);
        AddToDict(perJob,     jobKey,     value);
        AddToDict(perProduct, productKey, value);
        Save();
    }

    private void AddToDict(Dictionary<string, List<double>> d, string key, double value)
    {
        if (string.IsNullOrEmpty(key)) return;
        if (!d.TryGetValue(key, out var list)) d[key] = list = [];
        list.Add(value);
        if (list.Count > MaxPerKey) list.RemoveAt(0);
    }

    public void ResetHours(double rawTotal, double rawProducing)
    {
        BaselineTotalHours     = rawTotal;
        BaselineProducingHours = rawProducing;
        Save();
    }

    public (double total, double producing) ApplyBaseline(double rawTotal, double rawProducing) =>
        (Math.Max(0, rawTotal - BaselineTotalHours), Math.Max(0, rawProducing - BaselineProducingHours));

    public IReadOnlyList<double> GetJobCycleTimes(string k)          => Lookup(JobCycleTimes,          k);
    public IReadOnlyList<double> GetJobSetupTimes(string k)          => Lookup(JobSetupTimes,          k);
    public IReadOnlyList<double> GetJobPartToPartTimes(string k)     => Lookup(JobPartToPartTimes,     k);
    public IReadOnlyList<double> GetJobBendingTimes(string k)        => Lookup(JobBendingTimes,        k);
    public IReadOnlyList<double> GetProductCycleTimes(string k)      => Lookup(ProductCycleTimes,      k);
    public IReadOnlyList<double> GetProductSetupTimes(string k)      => Lookup(ProductSetupTimes,      k);
    public IReadOnlyList<double> GetProductPartToPartTimes(string k) => Lookup(ProductPartToPartTimes, k);
    public IReadOnlyList<double> GetProductBendingTimes(string k)    => Lookup(ProductBendingTimes,    k);

    private static IReadOnlyList<double> Lookup(Dictionary<string, List<double>> d, string key) =>
        !string.IsNullOrEmpty(key) && d.TryGetValue(key, out var list) ? list : Array.Empty<double>();

    public static string JobKey(string camFilePath)  => string.IsNullOrEmpty(camFilePath)  ? "" : Path.GetFileNameWithoutExtension(camFilePath);
    public static string ProductKey(string productId) => string.IsNullOrEmpty(productId)   ? "" : Path.GetFileNameWithoutExtension(productId);

    public void Save()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
            File.WriteAllText(FilePath, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
        }
        catch { }
    }
}
