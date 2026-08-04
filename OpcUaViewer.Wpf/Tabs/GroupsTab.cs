using System.Collections.ObjectModel;
using System.IO;
using OpcUaViewer.Core.Contracts;
using OpcUaViewer.Core.Models;
using OpcUaViewer.Core.Settings;
using OpcUaViewer.Wpf.Infrastructure;

namespace OpcUaViewer.Wpf.Tabs;

/// <summary>Product groups (orders) and their product lists — the main editing panel.</summary>
public class GroupsTab : ViewModelBase, IAppTab
{
    public string Title => "Groups";
    public string Icon  => "📋";
    public int    Order => 20;

    private CamOrderVm? _selectedOrder;
    private CamProductVm? _selectedProduct;

    public ObservableCollection<CamOrderVm> Orders { get; } = [];

    public CamOrderVm? SelectedOrder
    {
        get => _selectedOrder;
        set
        {
            Set(ref _selectedOrder, value);
            Notify(nameof(Products));
        }
    }

    public CamProductVm? SelectedProduct
    {
        get => _selectedProduct;
        set => Set(ref _selectedProduct, value);
    }

    public ObservableCollection<CamProductVm>? Products => SelectedOrder?.Products;

    public RelayCommand ReloadCommand     { get; }
    public RelayCommand ImportCsvCommand  { get; }

    public GroupsTab()
    {
        ReloadCommand    = new RelayCommand(LoadOrders);
        ImportCsvCommand = new RelayCommand(ImportCsv);
        LoadOrders();
    }

    public void LoadOrders()
    {
        Orders.Clear();
        string camFolder = AppSettings.Current.CamFolderPath;
        if (!Directory.Exists(camFolder)) return;

        foreach (var file in Directory.GetFiles(camFolder, "*.p3cam"))
        {
            try { Orders.Add(new CamOrderVm(new CamOrder(file))); }
            catch { }
        }

        SelectedOrder = Orders.Count > 0 ? Orders[0] : null;
    }

    private void ImportCsv()
    {
        // CSV import dialog will be wired here
    }
}

public class CamOrderVm : ViewModelBase
{
    public CamOrder Model { get; }

    public string OrderId      => Model.OrderId;
    public string CustomerName => Model.CustomerName;
    public int    Quantity     => Model.Quantity;
    public int    Completed    => Model.Completed;
    public string FileName     => Model.FileName;

    public ObservableCollection<CamProductVm> Products { get; }

    public CamOrderVm(CamOrder order)
    {
        Model    = order;
        Products = new ObservableCollection<CamProductVm>(
            order.Products.ConvertAll(p => new CamProductVm(p)));
    }
}

public class CamProductVm : ViewModelBase
{
    public CamProduct Model { get; }

    public string ListId      => Model.ListId;
    public string DisplayName => Model.DisplayName;
    public string MaterialId  => Model.MaterialId;

    private string _length = "";
    private string _width  = "";
    private string _thickness = "";
    private int    _runQty;
    private int    _ordQty;
    private string _hint = "";

    public string Length    { get => _length;    set => Set(ref _length,    value); }
    public string Width     { get => _width;     set => Set(ref _width,     value); }
    public string Thickness { get => _thickness; set => Set(ref _thickness, value); }
    public int    RunQty    { get => _runQty;    set => Set(ref _runQty,    value); }
    public int    OrdQty    { get => _ordQty;    set => Set(ref _ordQty,    value); }
    public string Hint      { get => _hint;      set => Set(ref _hint,      value); }

    public CamProductVm(CamProduct product)
    {
        Model     = product;
        _runQty   = product.RunQuantity;
        _ordQty   = product.Quantity;
        _hint     = product.OperatorHint;
        ParseDimensions(product.Parameters);
        ParseThickness(product.MaterialThickness);
    }

    private void ParseDimensions(string parameters)
    {
        foreach (var part in parameters.Split(';'))
        {
            var kv = part.Split('=');
            if (kv.Length != 2) continue;
            string key = kv[0].Trim();
            string val = kv[1].Trim();
            if (key is "L" or "Length")  _length = MmToIn(val);
            if (key is "W" or "Width")   _width  = MmToIn(val);
        }
    }

    private void ParseThickness(string mmStr) => _thickness = MmToIn(mmStr);

    private static string MmToIn(string mmStr)
    {
        if (double.TryParse(mmStr, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out double mm))
            return (mm / 25.4).ToString("0.000", System.Globalization.CultureInfo.InvariantCulture);
        return "";
    }
}
