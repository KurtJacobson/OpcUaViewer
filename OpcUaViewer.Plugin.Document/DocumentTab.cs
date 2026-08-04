using System;
using System.IO;
using System.Windows;
using OpcUaViewer.Core.Contracts;
using OpcUaViewer.Core.Services;
using OpcUaViewer.Core.Settings;

namespace OpcUaViewer.Plugin.Document;

public class DocumentTab : ViewModelBase, IAppTab, IConfigurableTab
{
    public string Title       => "Document";
    public string Icon        => "📄";
    public int    Order       => 30;
    public string Description => "Displays product PDF documentation based on the active OPC UA product ID.";

    public FrameworkElement CreateView() => new DocumentView { DataContext = this };

    private string _documentUri = "";
    private string _statusText  = "No document loaded";
    private string _lastLoadedProductId = "";

    public string DocumentUri
    {
        get => _documentUri;
        set => Set(ref _documentUri, value);
    }

    public string StatusText
    {
        get => _statusText;
        set => Set(ref _statusText, value);
    }

    public void Configure() => new DocumentSettingsDialog().ShowDialog();

    public DocumentTab(OpcUaService opc)
    {
        opc.ProductIdChanged += (_, productId) => Dispatch(() => OpenProductPdf(productId));
    }

    public void LoadDocument(string uri)
    {
        DocumentUri = uri;
        StatusText  = Path.GetFileName(uri);
    }

    public void ClearDocument(string message = "No document loaded")
    {
        DocumentUri = "";
        StatusText  = message;
    }

    private void OpenProductPdf(string productId)
    {
        if (string.IsNullOrWhiteSpace(productId))
        {
            if (!string.IsNullOrEmpty(_lastLoadedProductId))
                ClearDocument("Waiting for a product ID...");
            _lastLoadedProductId = "";
            return;
        }

        if (productId == _lastLoadedProductId) return;
        _lastLoadedProductId = productId;

        string folder = AppSettings.Current.PdfFolderPath;
        if (string.IsNullOrEmpty(folder) || !Directory.Exists(folder))
        {
            ClearDocument($"PDF folder not found: {folder}");
            return;
        }

        string fileName = SanitizeFileName(productId) + ".pdf";
        string fullPath = Path.Combine(folder, fileName);

        if (!File.Exists(fullPath))
        {
            ClearDocument($"No PDF found for '{productId}'");
            return;
        }

        DocumentUri = new Uri(fullPath).AbsoluteUri;
        StatusText  = $"Showing: {fileName}";
    }

    private static string SanitizeFileName(string name)
    {
        foreach (char c in Path.GetInvalidFileNameChars())
            name = name.Replace(c, '_');
        return name;
    }

    private static void Dispatch(Action a)
    {
        if (Application.Current?.Dispatcher.CheckAccess() == true) a();
        else Application.Current?.Dispatcher.BeginInvoke(a);
    }
}
