using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace OpcUaViewer
{
    /// <summary>Represents one .p3cam order file (a "group" of products).</summary>
    internal class CamOrder
    {
        public string FilePath     { get; }
        public string FileName     { get; }
        public string OrderId      { get; }
        public string CustomerName { get; }
        public int    Quantity     { get; }
        public int    Completed    { get; }
        public string InfoText     { get; }
        public List<CamProduct> Products { get; } = new List<CamProduct>();

        public CamOrder(string filePath)
        {
            FilePath = filePath;
            FileName = Path.GetFileNameWithoutExtension(filePath);

            var doc  = XDocument.Load(filePath);
            var root = doc.Root;
            var ns   = root.Name.Namespace;

            OrderId      = (string)root.Attribute("OrderId")      ?? FileName;
            CustomerName = (string)root.Attribute("CustomerName") ?? "";
            InfoText     = (string)root.Attribute("InfoText")     ?? "";
            int.TryParse((string)root.Attribute("Quantity"),  out int qty);  Quantity  = qty;
            int.TryParse((string)root.Attribute("Completed"), out int comp); Completed = comp;

            foreach (var el in root.Elements(ns + "Product"))
                Products.Add(new CamProduct(el, ns));
        }
    }

    /// <summary>Represents a single product entry within a CamOrder.</summary>
    internal class CamProduct
    {
        public string ListId           { get; }
        public string ProductId        { get; }  // raw XML value
        public string DisplayName      { get; }  // leaf filename without extension
        public int    Quantity         { get; }  // from XML
        public int    RunQuantity      { get; set; }  // editable by operator
        public int    Completed        { get; }
        public string InfoText         { get; }
        public string OperatorHint     { get; }
        public string Parameters       { get; }
        public string MaterialId       { get; }
        public string MaterialThickness{ get; }

        public CamProduct(XElement el, XNamespace ns)
        {
            ListId      = (string)el.Attribute("ListId")       ?? "";
            ProductId   = (string)el.Attribute("ProductId")    ?? "";
            DisplayName = Path.GetFileNameWithoutExtension(Path.GetFileName(ProductId));
            InfoText    = (string)el.Attribute("InfoText")     ?? "";
            OperatorHint= (string)el.Attribute("OperatorHint") ?? "";
            int.TryParse((string)el.Attribute("Quantity"),  out int qty);  Quantity     = qty;
            int.TryParse((string)el.Attribute("Completed"), out int comp); Completed    = comp;
            RunQuantity = qty;

            var prop = el.Element(ns + "Modifications")?.Element(ns + "Property");
            Parameters        = (string)prop?.Attribute("Parameters")         ?? "";
            MaterialId        = (string)prop?.Attribute("PhysicalMaterialId") ?? "";
            MaterialThickness = (string)prop?.Attribute("MaterialThickness")  ?? "";
        }
    }
}
