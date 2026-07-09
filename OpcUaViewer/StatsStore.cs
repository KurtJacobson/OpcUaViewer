using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpcUaViewer
{
    /// <summary>
    /// Persists cycle time history (per-job, per-product, and global) plus operating-hour
    /// baselines to a JSON file in %AppData%\OpcUaViewer\stats.json.
    /// All mutations call Save() immediately so data survives crashes.
    /// </summary>
    internal sealed class StatsStore
    {
        // ── persisted properties ──────────────────────────────────────────────

        public double BaselineTotalHours     { get; set; }
        public double BaselineProducingHours { get; set; }

        public List<double> GlobalCycleTimes      { get; set; } = new();
        public List<double> GlobalSetupTimes      { get; set; } = new();
        public List<double> GlobalPartToPartTimes { get; set; } = new();
        public List<double> GlobalBendingTimes    { get; set; } = new();

        // Key = CAM file name without extension
        public Dictionary<string, List<double>> JobCycleTimes      { get; set; } = new();
        public Dictionary<string, List<double>> JobSetupTimes      { get; set; } = new();
        public Dictionary<string, List<double>> JobPartToPartTimes { get; set; } = new();
        public Dictionary<string, List<double>> JobBendingTimes    { get; set; } = new();

        // Key = product file name without extension
        public Dictionary<string, List<double>> ProductCycleTimes      { get; set; } = new();
        public Dictionary<string, List<double>> ProductSetupTimes      { get; set; } = new();
        public Dictionary<string, List<double>> ProductPartToPartTimes { get; set; } = new();
        public Dictionary<string, List<double>> ProductBendingTimes    { get; set; } = new();

        // ── runtime-only ──────────────────────────────────────────────────────

        [JsonIgnore]
        public string FilePath { get; private set; } = "";

        [JsonIgnore]
        public IEnumerable<string> AllProductKeys =>
            ProductCycleTimes.Keys
                .Union(ProductSetupTimes.Keys)
                .Union(ProductPartToPartTimes.Keys)
                .Union(ProductBendingTimes.Keys)
                .Distinct();

        // ── limits ───────────────────────────────────────────────────────────

        private const int MaxGlobal = 1000;
        private const int MaxPerJob = 500;

        // ── factory ───────────────────────────────────────────────────────────

        public StatsStore() { }

        public static StatsStore Load()
        {
            string dir  = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "OpcUaViewer");
            string path = Path.Combine(dir, "stats.json");

            StatsStore store;
            try
            {
                if (File.Exists(path))
                {
                    var json = File.ReadAllText(path);
                    store = JsonSerializer.Deserialize<StatsStore>(json,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                        ?? new StatsStore();
                }
                else
                    store = new StatsStore();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Could not load stats: {ex.Message}");
                store = new StatsStore();
            }

            store.FilePath = path;
            return store;
        }

        // ── mutations ─────────────────────────────────────────────────────────

        public void AddCycleTime(string jobKey, string productKey, double t) =>
            AddTo(GlobalCycleTimes, JobCycleTimes, ProductCycleTimes, jobKey, productKey, t);

        public void AddSetupTime(string jobKey, string productKey, double t) =>
            AddTo(GlobalSetupTimes, JobSetupTimes, ProductSetupTimes, jobKey, productKey, t);

        public void AddPartToPartTime(string jobKey, string productKey, double t) =>
            AddTo(GlobalPartToPartTimes, JobPartToPartTimes, ProductPartToPartTimes, jobKey, productKey, t);

        public void AddBendingTime(string jobKey, string productKey, double t) =>
            AddTo(GlobalBendingTimes, JobBendingTimes, ProductBendingTimes, jobKey, productKey, t);

        private void AddTo(
            List<double> global,
            Dictionary<string, List<double>> perJob,
            Dictionary<string, List<double>> perProduct,
            string jobKey, string productKey, double value)
        {
            global.Add(value);
            if (global.Count > MaxGlobal) global.RemoveAt(0);
            AddToDict(perJob,     jobKey,     value, MaxPerJob);
            AddToDict(perProduct, productKey, value, MaxPerJob);
            Save();
        }

        private static void AddToDict(Dictionary<string, List<double>> d, string key, double value, int max)
        {
            if (string.IsNullOrEmpty(key)) return;
            if (!d.TryGetValue(key, out var list)) d[key] = list = new List<double>();
            list.Add(value);
            if (list.Count > max) list.RemoveAt(0);
        }

        public void ResetHours(double rawTotal, double rawProducing)
        {
            BaselineTotalHours     = rawTotal;
            BaselineProducingHours = rawProducing;
            Save();
        }

        // ── read helpers ──────────────────────────────────────────────────────

        public (double total, double producing) ApplyBaseline(double rawTotal, double rawProducing) =>
            (Math.Max(0, rawTotal - BaselineTotalHours),
             Math.Max(0, rawProducing - BaselineProducingHours));

        public IReadOnlyList<double> GetJobCycleTimes(string k)           => Lookup(JobCycleTimes,          k);
        public IReadOnlyList<double> GetJobSetupTimes(string k)           => Lookup(JobSetupTimes,          k);
        public IReadOnlyList<double> GetJobPartToPartTimes(string k)      => Lookup(JobPartToPartTimes,     k);
        public IReadOnlyList<double> GetJobBendingTimes(string k)         => Lookup(JobBendingTimes,        k);

        public IReadOnlyList<double> GetProductCycleTimes(string k)       => Lookup(ProductCycleTimes,      k);
        public IReadOnlyList<double> GetProductSetupTimes(string k)       => Lookup(ProductSetupTimes,      k);
        public IReadOnlyList<double> GetProductPartToPartTimes(string k)  => Lookup(ProductPartToPartTimes, k);
        public IReadOnlyList<double> GetProductBendingTimes(string k)     => Lookup(ProductBendingTimes,    k);

        private static IReadOnlyList<double> Lookup(Dictionary<string, List<double>> d, string key) =>
            !string.IsNullOrEmpty(key) && d.TryGetValue(key, out var list)
                ? list : Array.Empty<double>();

        // ── key helpers ───────────────────────────────────────────────────────

        /// <summary>Stable key derived from a CAM file path.</summary>
        public static string JobKey(string camFilePath) =>
            string.IsNullOrEmpty(camFilePath)
                ? "" : Path.GetFileNameWithoutExtension(camFilePath);

        /// <summary>Stable key derived from an OPC product ID (strips path and extension).</summary>
        public static string ProductKey(string productId) =>
            string.IsNullOrEmpty(productId)
                ? "" : Path.GetFileNameWithoutExtension(productId);

        // ── persistence ───────────────────────────────────────────────────────

        public void Save()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
                File.WriteAllText(FilePath, JsonSerializer.Serialize(this,
                    new JsonSerializerOptions { WriteIndented = true }));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save stats: {ex.Message}");
            }
        }
    }
}
