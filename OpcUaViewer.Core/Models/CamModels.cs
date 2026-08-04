using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace OpcUaViewer.Core.Models;

public class CamOrder
{
    public string FilePath     { get; }
    public string FileName     { get; }
    public string OrderId      { get; }
    public string CustomerName { get; }
    public int    Quantity     { get; }
    public int    Completed    { get; }
    public string InfoText     { get; }
    public List<CamProduct> Products { get; } = [];

    public CamOrder(string filePath)
    {
        FilePath = filePath;
        FileName = Path.GetFileNameWithoutExtension(filePath);

        var doc  = XDocument.Load(filePath);
        var root = doc.Root!;
        var ns   = root.Name.Namespace;

        OrderId      = (string?)root.Attribute("OrderId")      ?? FileName;
        CustomerName = (string?)root.Attribute("CustomerName") ?? "";
        InfoText     = (string?)root.Attribute("InfoText")     ?? "";
        int.TryParse((string?)root.Attribute("Quantity"),  out int qty);  Quantity  = qty;
        int.TryParse((string?)root.Attribute("Completed"), out int comp); Completed = comp;

        foreach (var el in root.Elements(ns + "Product"))
            Products.Add(new CamProduct(el, ns));
    }
}

public class CamProduct
{
    public string ListId            { get; }
    public string ProductId         { get; }
    public string DisplayName       { get; }
    public int    Quantity          { get; }
    public int    RunQuantity       { get; set; }
    public int    Completed         { get; }
    public string InfoText          { get; }
    public string OperatorHint      { get; }
    public string Parameters        { get; }
    public string MaterialId        { get; }
    public string MaterialThickness { get; }

    public CamProduct(XElement el, XNamespace ns)
    {
        ListId      = (string?)el.Attribute("ListId")       ?? "";
        ProductId   = (string?)el.Attribute("ProductId")    ?? "";
        DisplayName = Path.GetFileNameWithoutExtension(Path.GetFileName(ProductId));
        InfoText    = (string?)el.Attribute("InfoText")     ?? "";
        OperatorHint= (string?)el.Attribute("OperatorHint") ?? "";
        int.TryParse((string?)el.Attribute("Quantity"),  out int qty);  Quantity     = qty;
        int.TryParse((string?)el.Attribute("Completed"), out int comp); Completed    = comp;
        RunQuantity = qty;

        var prop = el.Element(ns + "Modifications")?.Element(ns + "Property");
        Parameters        = (string?)prop?.Attribute("Parameters")         ?? "";
        MaterialId        = (string?)prop?.Attribute("PhysicalMaterialId") ?? "";
        MaterialThickness = (string?)prop?.Attribute("MaterialThickness")  ?? "";
    }
}
