using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml.Linq;
using Microsoft.Win32;
using OpcUaViewer.Core.Contracts;
using OpcUaViewer.Core.Models;
using OpcUaViewer.Core.Services;
using OpcUaViewer.Core.Settings;

namespace OpcUaViewer.Plugin.Groups;

public enum RowHighlight { Normal, Active, Dim }

public class GroupsTab : ViewModelBase, IAppTab
{
    public string Title => "Groups";
    public string Icon  => "📋";
    public int    Order => 20;

    private readonly OpcUaService _opc;

    // ── OPC UA state ──────────────────────────────────────────────────────────
    private string _activeCamFile   = "";
    private string _activeProductId = "";
    private int    _machineState    = -1;

    // ── Selection ─────────────────────────────────────────────────────────────
    private CamOrderVm?   _selectedOrder;
    private CamProductVm? _selectedProduct;

    // ── Edit mode ─────────────────────────────────────────────────────────────
    private bool   _isEditMode;
    private string _editingFilePath = "";

    private string _editFileName  = "";
    private string _editOrderId   = "";
    private string _editCustomer  = "";
    private string _editQty       = "1";
    private string _editCompleted = "0";
    private string _editInfoText  = "";

    // ── Collections ───────────────────────────────────────────────────────────
    public ObservableCollection<CamOrderVm>   Orders   { get; } = [];
    public ObservableCollection<CamProductVm> Products { get; } = [];

    // ── Properties ────────────────────────────────────────────────────────────

    public CamOrderVm? SelectedOrder
    {
        get => _selectedOrder;
        set
        {
            if (!Set(ref _selectedOrder, value)) return;
            if (value != null && !_isEditMode)
            {
                LoadProductsFrom(value);
                EditFileName  = value.FileName;
                EditOrderId   = value.OrderId;
                EditCustomer  = value.CustomerName;
                EditQty       = value.Quantity.ToString();
                EditCompleted = value.Completed.ToString();
                EditInfoText  = value.InfoText;
            }
            Notify(nameof(HasSelection));
        }
    }

    public CamProductVm? SelectedProduct
    {
        get => _selectedProduct;
        set => Set(ref _selectedProduct, value);
    }

    public bool IsEditMode
    {
        get => _isEditMode;
        private set { Set(ref _isEditMode, value); Notify(nameof(IsViewMode)); Notify(nameof(ShowEditButtons)); Notify(nameof(ShowNormalButtons)); }
    }

    public bool IsViewMode       => !_isEditMode;
    public bool ShowEditButtons  => _isEditMode;
    public bool ShowNormalButtons => !_isEditMode;
    public bool HasSelection     => _selectedOrder != null;
    public bool IsRunningLocked  => _machineState == 3 && !string.IsNullOrEmpty(_activeCamFile);

    public string EditFileName  { get => _editFileName;  set => Set(ref _editFileName,  value); }
    public string EditOrderId   { get => _editOrderId;   set => Set(ref _editOrderId,   value); }
    public string EditCustomer  { get => _editCustomer;  set => Set(ref _editCustomer,  value); }
    public string EditQty       { get => _editQty;       set => Set(ref _editQty,       value); }
    public string EditCompleted { get => _editCompleted; set => Set(ref _editCompleted, value); }
    public string EditInfoText  { get => _editInfoText;  set => Set(ref _editInfoText,  value); }

    // ── Commands ──────────────────────────────────────────────────────────────
    public RelayCommand ReloadCommand         { get; }
    public RelayCommand ImportCsvCommand      { get; }
    public RelayCommand NewGroupCommand       { get; }
    public RelayCommand EditGroupCommand      { get; }
    public RelayCommand DeleteGroupCommand    { get; }
    public RelayCommand SaveGroupCommand      { get; }
    public RelayCommand CancelEditCommand     { get; }
    public RelayCommand AddProductsCommand    { get; }
    public RelayCommand RemoveProductsCommand { get; }
    public RelayCommand RunGroupCommand       { get; }
    public RelayCommand CancelGroupCommand    { get; }

    public GroupsTab(OpcUaService opc)
    {
        _opc = opc;

        ReloadCommand         = new RelayCommand(LoadOrders);
        ImportCsvCommand      = new RelayCommand(ImportCsv);
        NewGroupCommand       = new RelayCommand(() => EnterEditMode(null),  () => !IsRunningLocked);
        EditGroupCommand      = new RelayCommand(EditSelected,               () => HasSelection && !IsRunningLocked);
        DeleteGroupCommand    = new RelayCommand(DeleteSelected,             () => HasSelection && !IsRunningLocked);
        SaveGroupCommand      = new RelayCommand(SaveGroup);
        CancelEditCommand     = new RelayCommand(() => ExitEditMode(true));
        AddProductsCommand    = new RelayCommand(AddProducts);
        RemoveProductsCommand = new RelayCommand(RemoveProducts, () => SelectedProduct != null);
        RunGroupCommand       = new RelayCommand(RunGroup,    () => HasSelection && !IsRunningLocked);
        CancelGroupCommand    = new RelayCommand(CancelGroup, () => HasSelection || IsRunningLocked);

        _opc.CamFileChanged      += (_, f) => Dispatch(() => OnCamFileChanged(f));
        _opc.MachineStateChanged += (_, s) => Dispatch(() => OnMachineStateChanged(s));
        _opc.ProductIdChanged    += (_, p) => Dispatch(() => OnProductIdChanged(p));

        LoadOrders();
    }

    // ── Load ──────────────────────────────────────────────────────────────────

    public void LoadOrders(string? preserveFilePath = null)
    {
        string camFolder = AppSettings.Current.CamFolderPath;
        Orders.Clear();
        if (!Directory.Exists(camFolder)) return;

        foreach (var file in Directory.GetFiles(camFolder, "*.p3cam"))
        {
            try { Orders.Add(new CamOrderVm(new CamOrder(file))); }
            catch { }
        }

        CamOrderVm? toSelect = null;
        if (preserveFilePath != null)
            toSelect = Orders.FirstOrDefault(o => string.Equals(o.FilePath, preserveFilePath, StringComparison.OrdinalIgnoreCase));
        SelectedOrder = toSelect ?? Orders.FirstOrDefault();
    }

    private void LoadOrders() => LoadOrders(null);

    private void LoadProductsFrom(CamOrderVm order)
    {
        Products.Clear();
        foreach (var p in order.Model.Products)
            Products.Add(new CamProductVm(p));
        RefreshHighlighting();
    }

    // ── Edit mode ─────────────────────────────────────────────────────────────

    private void EnterEditMode(CamOrderVm? existing)
    {
        IsEditMode       = true;
        _editingFilePath = existing?.FilePath ?? "";

        if (existing != null)
        {
            EditFileName  = existing.FileName;
            EditOrderId   = existing.OrderId;
            EditCustomer  = existing.CustomerName;
            EditQty       = existing.Quantity.ToString();
            EditCompleted = existing.Completed.ToString();
            EditInfoText  = existing.InfoText;
        }
        else
        {
            var (fn, oid) = NextOrderDefaults();
            EditFileName  = fn;
            EditOrderId   = oid;
            EditCustomer  = "";
            EditQty       = "1";
            EditCompleted = "0";
            EditInfoText  = "";
            Products.Clear();
        }
    }

    private void ExitEditMode(bool reload)
    {
        string saved     = _editingFilePath;
        IsEditMode       = false;
        _editingFilePath = "";
        if (reload) LoadOrders(saved);
    }

    private void EditSelected()
    {
        if (SelectedOrder == null) return;
        EnterEditMode(SelectedOrder);
    }

    // ── Save ──────────────────────────────────────────────────────────────────

    private void SaveGroup()
    {
        string fileName  = EditFileName.Trim();
        string camFolder = AppSettings.Current.CamFolderPath;

        if (string.IsNullOrEmpty(fileName))  { Warn("Please enter a file name."); return; }
        if (!Directory.Exists(camFolder))     { Warn("CAM folder not configured. Set it in Settings."); return; }
        if (!int.TryParse(EditQty,       out int qty))       qty       = 1;
        if (!int.TryParse(EditCompleted, out int completed)) completed = 0;

        XNamespace ns   = "http://eu.schroedergroup.de/xml-schemas/P3CAMSchema.xsd";
        var        root = new XElement(ns + "POS3000CAMData",
            new XAttribute("InfoText",     EditInfoText.Trim()),
            new XAttribute("OrderId",      EditOrderId.Trim()),
            new XAttribute("CustomerName", EditCustomer.Trim()),
            new XAttribute("Quantity",     qty),
            new XAttribute("Completed",    completed));

        foreach (var p in Products)
        {
            var lwParts = new System.Collections.Generic.List<string>();
            if (!string.IsNullOrEmpty(p.Length)) lwParts.Add($"Length={InToMm(p.Length)}");
            if (!string.IsNullOrEmpty(p.Width))  lwParts.Add($"Width={InToMm(p.Width)}");

            var product = new XElement(ns + "Product",
                new XAttribute("ListId",       p.ListId),
                new XAttribute("ProductId",    p.ProductId),
                new XAttribute("Quantity",     p.OrdQty),
                new XAttribute("Completed",    0),
                new XAttribute("LoadError",    0),
                new XAttribute("InfoText",     ""),
                new XAttribute("DetailsHtml",  ""),
                new XAttribute("OperatorHint", p.Hint),
                new XAttribute("UserData",     ""));

            string parameters = string.Join(";", lwParts);
            string thickness  = InToMm(p.Thickness);
            if (!string.IsNullOrEmpty(parameters) || !string.IsNullOrEmpty(p.MaterialId) || !string.IsNullOrEmpty(thickness))
            {
                product.Add(new XElement(ns + "Modifications",
                    new XElement(ns + "Property",
                        new XAttribute("Parameters",         parameters),
                        new XAttribute("PhysicalMaterialId", p.MaterialId),
                        new XAttribute("MaterialThickness",  thickness))));
            }
            root.Add(product);
        }

        if (!fileName.EndsWith(".p3cam", StringComparison.OrdinalIgnoreCase))
            fileName += ".p3cam";

        string savePath;
        if (!string.IsNullOrEmpty(_editingFilePath) &&
            string.Equals(Path.GetFileNameWithoutExtension(_editingFilePath),
                          Path.GetFileNameWithoutExtension(fileName), StringComparison.OrdinalIgnoreCase))
        {
            savePath = _editingFilePath;
        }
        else
        {
            savePath = Path.Combine(camFolder, fileName);
            if (File.Exists(savePath) && savePath != _editingFilePath)
            {
                if (!DialogService.Current.Confirm($"'{fileName}' already exists. Overwrite?", "Save"))
                    return;
            }
        }

        try
        {
            new XDocument(new XDeclaration("1.0", "utf-8", null), root).Save(savePath);
            ExitEditMode(false);
            LoadOrders(savePath);
        }
        catch (Exception ex) { Warn("Failed to save:\n\n" + ex.Message); }
    }

    // ── Delete ────────────────────────────────────────────────────────────────

    private void DeleteSelected()
    {
        if (SelectedOrder == null) return;
        var order = SelectedOrder;
        if (!DialogService.Current.Confirm($"Delete '{order.FileName}'?\n\nThis cannot be undone.", "Delete Group"))
            return;
        try
        {
            File.Delete(order.FilePath);
            LoadOrders();
        }
        catch (Exception ex) { Warn("Failed to delete:\n\n" + ex.Message); }
    }

    // ── Add / Remove products ─────────────────────────────────────────────────

    private void AddProducts()
    {
        string prodFolder = AppSettings.Current.CamProductsPath;
        if (!Directory.Exists(prodFolder)) { Warn("Please configure the CAM Products Folder in Settings first."); return; }

        var dlg = new OpenFileDialog
        {
            Title            = "Select product .zip files",
            InitialDirectory = prodFolder,
            Filter           = "CAM Product files (*.zip)|*.zip",
            Multiselect      = true
        };
        if (dlg.ShowDialog() != true) return;

        string prefix = AppSettings.Current.ProductPathPrefix;
        if (!string.IsNullOrEmpty(prefix) && !prefix.EndsWith('\\')) prefix += '\\';

        foreach (string path in dlg.FileNames)
        {
            string relative = Path.GetRelativePath(prodFolder, path);
            string listId   = Path.GetFileNameWithoutExtension(path);
            string prodId   = prefix + relative;
            if (Products.Any(p => p.ListId == listId)) continue;
            Products.Add(new CamProductVm(listId, prodId));
        }
    }

    private void RemoveProducts()
    {
        if (SelectedProduct != null)
            Products.Remove(SelectedProduct);
    }

    // ── Run / Cancel group ────────────────────────────────────────────────────

    private void RunGroup()
    {
        if (SelectedOrder == null) return;
        string outputBase = AppSettings.Current.CamOutputPath;
        if (string.IsNullOrEmpty(outputBase)) { Warn("Please set the CAM Output Directory in Settings first."); return; }

        try
        {
            string inDir = Path.Combine(outputBase, "in");
            Directory.CreateDirectory(inDir);
            string dest = Path.Combine(inDir, Path.GetFileName(SelectedOrder.FilePath));
            File.Copy(SelectedOrder.FilePath, dest, overwrite: true);
            DialogService.Current.Show($"Sent '{Path.GetFileName(SelectedOrder.FilePath)}' to:\n{inDir}", "Run Group");
        }
        catch (Exception ex) { Warn("Failed to copy CAM file:\n\n" + ex.Message); }
    }

    private void CancelGroup()
    {
        string outputBase = AppSettings.Current.CamOutputPath;
        if (string.IsNullOrEmpty(outputBase)) { Warn("Please set the CAM Output Directory in Settings first."); return; }

        if (IsRunningLocked)
        {
            string processingPath = Path.Combine(outputBase, "processing", Path.GetFileName(_activeCamFile));
            if (!File.Exists(processingPath)) { DialogService.Current.Show("File not found in processing folder.", "Cancel Group"); return; }
            if (!DialogService.Current.Confirm("Delete from processing folder? This will interrupt the current run.", "Cancel Group")) return;
            try
            {
                string cancelDir = Path.Combine(outputBase, "canceled");
                Directory.CreateDirectory(cancelDir);
                File.Move(processingPath, Path.Combine(cancelDir, Path.GetFileName(_activeCamFile)), overwrite: true);
            }
            catch (Exception ex) { Warn(ex.Message); }
            return;
        }

        if (SelectedOrder == null) return;
        string inPath = Path.Combine(outputBase, "in", Path.GetFileName(SelectedOrder.FilePath));
        if (!File.Exists(inPath)) { DialogService.Current.Show("File is not currently in the 'in' folder.", "Cancel Group"); return; }

        try
        {
            string cancelDir = Path.Combine(outputBase, "canceled");
            Directory.CreateDirectory(cancelDir);
            File.Move(inPath, Path.Combine(cancelDir, Path.GetFileName(SelectedOrder.FilePath)), overwrite: true);
            DialogService.Current.Show("Moved to canceled folder.", "Cancel Group");
        }
        catch (Exception ex) { Warn(ex.Message); }
    }

    // ── CSV Import ────────────────────────────────────────────────────────────

    private void ImportCsv()
    {
        string prodFolder = AppSettings.Current.CamProductsPath;
        if (!Directory.Exists(prodFolder))
        {
            DialogService.Current.Warn("Please configure the CAM Products Folder in Settings first.", "Import CSV");
            return;
        }

        var dlg = new CsvImportDialog { Owner = Application.Current.MainWindow };
        if (dlg.ShowDialog() != true) return;

        var rows = CsvService.ParseCsv(
            dlg.CsvFilePath,
            AppSettings.Current.CsvPartNameColumn,     AppSettings.Current.CsvPartNameRegex,
            AppSettings.Current.CsvTemplateFileColumn, AppSettings.Current.CsvTemplateFileRegex,
            AppSettings.Current.CsvQtyColumn,          AppSettings.Current.CsvQtyRegex,
            AppSettings.Current.CsvMaterialColumn,     AppSettings.Current.CsvMaterialRegex,
            AppSettings.Current.CsvThicknessColumn,    AppSettings.Current.CsvThicknessRegex,
            AppSettings.Current.CsvLengthColumn,       AppSettings.Current.CsvLengthRegex,
            AppSettings.Current.CsvWidthColumn,        AppSettings.Current.CsvWidthRegex);

        if (rows.Count == 0)
        {
            DialogService.Current.Warn("No rows could be imported. Check your column mapping.", "Import CSV");
            return;
        }

        string prefix = AppSettings.Current.ProductPathPrefix;
        if (!string.IsNullOrEmpty(prefix) && !prefix.EndsWith('\\')) prefix += '\\';

        EnterEditMode(null);
        var (_, oid) = NextOrderDefaults();
        EditFileName = dlg.GroupName;
        EditOrderId  = oid;
        Products.Clear();

        foreach (var row in rows)
        {
            string listId  = row.ListId;
            string rawProd = string.IsNullOrEmpty(row.ProductIdRaw) ? row.ListId : row.ProductIdRaw;
            string prodId  = prefix + rawProd;

            var vm = new CamProductVm(listId, prodId)
            {
                MaterialId = row.Material,
                Thickness  = row.Thickness,
                Length     = row.Length,
                Width      = row.Width,
            };
            if (int.TryParse(row.Qty, out int qty)) vm.OrdQty = qty;
            Products.Add(vm);
        }
    }

    // ── OPC UA reactions ──────────────────────────────────────────────────────

    private void OnCamFileChanged(string camFile)
    {
        _activeCamFile = camFile;
        RefreshHighlighting();
        NotifyRunning();
    }

    private void OnMachineStateChanged(int state)
    {
        _machineState = state;
        RefreshHighlighting();
        NotifyRunning();
    }

    private void OnProductIdChanged(string productId)
    {
        _activeProductId = productId;
        RefreshProductHighlights();
    }

    private void RefreshHighlighting()
    {
        bool locked = IsRunningLocked;
        foreach (var o in Orders)
        {
            if (!locked) { o.Highlight = RowHighlight.Normal; continue; }
            bool match = string.Equals(
                Path.GetFileNameWithoutExtension(o.FileName),
                Path.GetFileNameWithoutExtension(_activeCamFile),
                StringComparison.OrdinalIgnoreCase);
            o.Highlight = match ? RowHighlight.Active : RowHighlight.Dim;
            if (match && SelectedOrder != o && !_isEditMode) SelectedOrder = o;
        }
        RefreshProductHighlights();
    }

    private void RefreshProductHighlights()
    {
        bool hasActive = !string.IsNullOrEmpty(_activeProductId) && IsRunningLocked;
        bool anyMatch  = false;

        foreach (var p in Products)
        {
            if (!hasActive) { p.Highlight = RowHighlight.Normal; continue; }
            bool match = p.ProductId.Contains(_activeProductId, StringComparison.OrdinalIgnoreCase)
                      || _activeProductId.Contains(p.ListId,    StringComparison.OrdinalIgnoreCase);
            p.Highlight = match ? RowHighlight.Active : RowHighlight.Dim;
            if (match) anyMatch = true;
        }

        if (!anyMatch)
            foreach (var p in Products)
                p.Highlight = RowHighlight.Normal;
    }

    private void NotifyRunning()
    {
        Notify(nameof(IsRunningLocked));
        Notify(nameof(ShowNormalButtons));
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private (string fileName, string orderId) NextOrderDefaults()
    {
        int max = 0;
        foreach (var o in Orders)
        {
            var m = System.Text.RegularExpressions.Regex.Match(o.FileName, @"- Order (\d+)\s*$", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (m.Success && int.TryParse(m.Groups[1].Value, out int n)) max = Math.Max(max, n);
            m = System.Text.RegularExpressions.Regex.Match(o.OrderId, @"^O(\d+)$", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (m.Success && int.TryParse(m.Groups[1].Value, out int n2)) max = Math.Max(max, n2);
        }
        int next = max + 1;
        return ($"{DateTime.Today:yyyy-MM-dd} - Order {next:D3}", $"O{next:D3}");
    }

    private static string InToMm(string inStr)
    {
        if (double.TryParse(inStr, NumberStyles.Any, CultureInfo.InvariantCulture, out double inches))
            return (inches * 25.4).ToString("0.####", CultureInfo.InvariantCulture);
        return "";
    }

    private static void Warn(string msg) => DialogService.Current.Warn(msg, "Groups");

    private static void Dispatch(Action a)
    {
        if (Application.Current?.Dispatcher.CheckAccess() == true) a();
        else Application.Current?.Dispatcher.BeginInvoke(a);
    }
}

// ── View models ───────────────────────────────────────────────────────────────

public class CamOrderVm : ViewModelBase
{
    private RowHighlight _highlight;

    public CamOrder Model { get; }

    public string FilePath     => Model.FilePath;
    public string FileName     => Model.FileName;
    public string OrderId      => Model.OrderId;
    public string CustomerName => Model.CustomerName;
    public string InfoText     => Model.InfoText;
    public int    Quantity     => Model.Quantity;
    public int    Completed    => Model.Completed;

    public RowHighlight Highlight { get => _highlight; set => Set(ref _highlight, value); }

    public CamOrderVm(CamOrder order) => Model = order;
}

public class CamProductVm : ViewModelBase
{
    private string _length = "", _width = "", _thickness = "", _hint = "";
    private int    _runQty, _ordQty;
    private RowHighlight _highlight;

    public string ListId      { get; set; } = "";
    public string ProductId   { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string MaterialId  { get; set; } = "";

    public string Length    { get => _length;    set => Set(ref _length,    value); }
    public string Width     { get => _width;     set => Set(ref _width,     value); }
    public string Thickness { get => _thickness; set => Set(ref _thickness, value); }
    public string Hint      { get => _hint;      set => Set(ref _hint,      value); }
    public int    OrdQty    { get => _ordQty;    set => Set(ref _ordQty,    value); }
    public int    RunQty    { get => _runQty;    set => Set(ref _runQty,    value); }

    public RowHighlight Highlight { get => _highlight; set => Set(ref _highlight, value); }

    public CamProductVm(CamProduct product)
    {
        ListId      = product.ListId;
        ProductId   = product.ProductId;
        DisplayName = product.DisplayName;
        MaterialId  = product.MaterialId;
        OrdQty      = product.Quantity;
        RunQty      = product.RunQuantity;
        Hint        = product.OperatorHint;
        ParseDimensions(product.Parameters);
        Thickness   = MmToIn(product.MaterialThickness);
    }

    public CamProductVm(string listId, string productId)
    {
        ListId      = listId;
        ProductId   = productId;
        DisplayName = Path.GetFileNameWithoutExtension(productId);
        OrdQty      = 1;
        RunQty      = 1;
    }

    private void ParseDimensions(string parameters)
    {
        foreach (var part in parameters.Split(';'))
        {
            int eq = part.IndexOf('=');
            if (eq < 0) continue;
            string key = part[..eq].Trim();
            string val = part[(eq + 1)..].Trim();
            if (key is "L" or "Length") Length = MmToIn(val);
            if (key is "W" or "Width")  Width  = MmToIn(val);
        }
    }

    private static string MmToIn(string mmStr)
    {
        if (double.TryParse(mmStr, NumberStyles.Any, CultureInfo.InvariantCulture, out double mm))
            return (mm / 25.4).ToString("0.000", CultureInfo.InvariantCulture);
        return "";
    }
}
