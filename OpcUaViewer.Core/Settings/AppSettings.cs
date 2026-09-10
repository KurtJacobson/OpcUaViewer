using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpcUaViewer.Core.Settings;

public sealed class AppSettings
{
    private static readonly string FilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "FoldControl", "settings.json");

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.Never
    };

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

    // Machine / site identity
    public string MachineId   { get; set; } = "";   // unique ID for this machine instance
    public string MachineName { get; set; } = "";   // human label, e.g. "Folder 1"
    public string SiteCode    { get; set; } = "";   // short plant/site code, e.g. "CHI"
    public string PlantCode   { get; set; } = "";   // ERP plant code if different from SiteCode

    // Connection / folders
    public string EndpointUrl       { get; set; } = "opc.tcp://10.10.10.102:4840";
    public string PdfFolderPath     { get; set; } = "C:\\ProductDocs";
    public string CsvLogFolderPath   { get; set; } = "";
    public int    CsvHistoryDaysBack { get; set; } = 7;
    public string CamFolderPath     { get; set; } = "";
    public string CamOutputPath     { get; set; } = "";
    public string CamProductsPath   { get; set; } = "";
    public string ProductPathPrefix  { get; set; } = "";

    // UI state
    public bool   KeyboardEnabled   { get; set; } = false;
    public string WindowState       { get; set; } = "Normal";
    public int    WindowLeft        { get; set; } = -1;
    public int    WindowTop         { get; set; } = -1;
    public int    WindowWidth       { get; set; } = 1200;
    public int    WindowHeight      { get; set; } = 700;

    // Plugins
    public List<string> DisabledPlugins { get; set; } = [];

    // Webcam
    public int WebcamIndex { get; set; } = 0;

    // Stats — machine state int values from CurrentMachineState OPC tag
    public int MachineStateAuto    { get; set; } = 1;
    public int MachineStateManual  { get; set; } = 2;
    public int MachineStateSetup   { get; set; } = 3;
    public int MachineStateBending { get; set; } = 4;
    public int MachineStateFaulted { get; set; } = 5;

    // Stats — tag name substrings used to extract hours/bends from TagValueUpdated
    public string TotalHoursTagMatch    { get; set; } = "TotalOperatingHours";
    public string ProducingHoursTagMatch { get; set; } = "ProducingHours";
    public string TotalBendsTagMatch    { get; set; } = "TotalBends";

    // CSV import
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
