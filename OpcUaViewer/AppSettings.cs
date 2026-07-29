using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpcUaViewer
{
    internal sealed class AppSettings
    {
        private static readonly string FilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "OpcUaViewer", "settings.json");

        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.Never
        };

        // ── singleton ─────────────────────────────────────────────────────────
        public static AppSettings Current { get; private set; } = new();

        public static void Load()
        {
            try
            {
                if (File.Exists(FilePath))
                    Current = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(FilePath), JsonOpts) ?? new();
            }
            catch { Current = new(); }
        }

        public static void Save()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
                File.WriteAllText(FilePath, JsonSerializer.Serialize(Current, JsonOpts));
            }
            catch { }
        }

        // ── connection / folders ──────────────────────────────────────────────
        public string EndpointUrl      { get; set; } = "opc.tcp://10.10.10.102:4840";
        public string PdfFolderPath    { get; set; } = "C:\\ProductDocs";
        public string CamFolderPath    { get; set; } = "";
        public string CamOutputPath    { get; set; } = "";
        public string CamProductsPath  { get; set; } = "";
        public string ProductPathPrefix{ get; set; } = "";

        // ── UI state ──────────────────────────────────────────────────────────
        public bool   KeyboardEnabled  { get; set; } = false;
        public string WindowState      { get; set; } = "Normal";
        public int    WindowLeft       { get; set; } = -1;
        public int    WindowTop        { get; set; } = -1;
        public int    WindowWidth      { get; set; } = 1100;
        public int    WindowHeight     { get; set; } = 660;

        // ── CSV import ────────────────────────────────────────────────────────
        public string CsvPartNameColumn     { get; set; } = "Part Name";
        public string CsvPartNameRegex      { get; set; } = "";
        public string CsvTemplateFileColumn { get; set; } = "";
        public string CsvTemplateFileRegex  { get; set; } = "";
        public string CsvQtyColumn          { get; set; } = "Quantity";
        public string CsvQtyRegex           { get; set; } = "";
        public string CsvMaterialColumn     { get; set; } = "Material";
        public string CsvMaterialRegex      { get; set; } = "";
        public string CsvThicknessColumn    { get; set; } = "Thickness";
        public string CsvThicknessRegex     { get; set; } = "";
        public string CsvLengthColumn       { get; set; } = "Length";
        public string CsvLengthRegex        { get; set; } = "";
        public string CsvWidthColumn        { get; set; } = "Width";
        public string CsvWidthRegex         { get; set; } = "";
    }
}
