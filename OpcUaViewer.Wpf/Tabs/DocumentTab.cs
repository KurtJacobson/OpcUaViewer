using OpcUaViewer.Core.Contracts;
using OpcUaViewer.Wpf.Infrastructure;

namespace OpcUaViewer.Wpf.Tabs;

/// <summary>Document viewer tab — renders PDF/HTML files via WebView2.</summary>
public class DocumentTab : ViewModelBase, IAppTab
{
    public string Title => "Document";
    public string Icon  => "📄";
    public int    Order => 30;

    private string _documentUri = "";
    private string _statusText  = "No document loaded";

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

    public void LoadDocument(string uri)
    {
        DocumentUri = uri;
        StatusText  = System.IO.Path.GetFileName(uri);
    }

    public void ClearDocument()
    {
        DocumentUri = "";
        StatusText  = "No document loaded";
    }
}
