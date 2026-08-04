using OpcUaViewer.Core.Contracts;
using OpcUaViewer.Core.Settings;
using OpcUaViewer.Wpf.Infrastructure;
using OpcUaViewer.Wpf.Plugins;

namespace OpcUaViewer.Wpf.Tabs;

public class SettingsTab : ViewModelBase, IAppTab
{
    public string Title => "Settings";
    public string Icon  => "⚙";
    public int    Order => 90;

    private string _endpointUrl      = "";
    private string _camFolderPath    = "";
    private string _camOutputPath    = "";
    private string _camProductsPath  = "";
    private string _pdfFolderPath    = "";
    private string _productPathPrefix = "";
    private bool   _keyboardEnabled;

    public string EndpointUrl
    {
        get => _endpointUrl;
        set => Set(ref _endpointUrl, value);
    }
    public string CamFolderPath
    {
        get => _camFolderPath;
        set => Set(ref _camFolderPath, value);
    }
    public string CamOutputPath
    {
        get => _camOutputPath;
        set => Set(ref _camOutputPath, value);
    }
    public string CamProductsPath
    {
        get => _camProductsPath;
        set => Set(ref _camProductsPath, value);
    }
    public string PdfFolderPath
    {
        get => _pdfFolderPath;
        set => Set(ref _pdfFolderPath, value);
    }
    public string ProductPathPrefix
    {
        get => _productPathPrefix;
        set => Set(ref _productPathPrefix, value);
    }
    public bool KeyboardEnabled
    {
        get => _keyboardEnabled;
        set => Set(ref _keyboardEnabled, value);
    }

    public PluginService PluginService { get; }
    public RelayCommand  SaveCommand   { get; }

    public SettingsTab(PluginService pluginService)
    {
        PluginService = pluginService;
        SaveCommand   = new RelayCommand(Save);
        Load();
    }

    public void Load()
    {
        var s = AppSettings.Current;
        EndpointUrl      = s.EndpointUrl;
        CamFolderPath    = s.CamFolderPath;
        CamOutputPath    = s.CamOutputPath;
        CamProductsPath  = s.CamProductsPath;
        PdfFolderPath    = s.PdfFolderPath;
        ProductPathPrefix = s.ProductPathPrefix;
        KeyboardEnabled  = s.KeyboardEnabled;
    }

    private void Save()
    {
        var s = AppSettings.Current;
        s.EndpointUrl      = EndpointUrl.Trim();
        s.CamFolderPath    = CamFolderPath.Trim();
        s.CamOutputPath    = CamOutputPath.Trim();
        s.CamProductsPath  = CamProductsPath.Trim();
        s.PdfFolderPath    = PdfFolderPath.Trim();
        s.ProductPathPrefix = ProductPathPrefix.Trim();
        s.KeyboardEnabled  = KeyboardEnabled;
        PluginService.SaveEnabledState();
        AppSettings.Save();
    }
}
