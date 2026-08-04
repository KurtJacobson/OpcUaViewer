using System.Windows;
using System.Windows.Controls;
using Microsoft.Web.WebView2.Wpf;
using OpcUaViewer.Wpf.Tabs;

namespace OpcUaViewer.Wpf.Views;

public partial class DocumentView : UserControl
{
    private WebView2? _webView;

    public DocumentView()
    {
        InitializeComponent();
        Loaded   += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        _webView = new WebView2 { HorizontalAlignment = HorizontalAlignment.Stretch,
                                  VerticalAlignment   = VerticalAlignment.Stretch };
        WebViewHost.Child = _webView;

        await _webView.EnsureCoreWebView2Async();
        _webView.CoreWebView2.Settings.IsStatusBarEnabled   = false;
        _webView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;

        if (DataContext is DocumentTab vm)
        {
            vm.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName == nameof(DocumentTab.DocumentUri))
                    Navigate(vm.DocumentUri);
            };
            if (!string.IsNullOrEmpty(vm.DocumentUri))
                Navigate(vm.DocumentUri);
        }
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _webView?.Dispose();
        _webView = null;
    }

    private void Navigate(string uri)
    {
        if (_webView?.CoreWebView2 is null) return;
        if (string.IsNullOrEmpty(uri))
            _webView.CoreWebView2.Navigate("about:blank");
        else
            _webView.CoreWebView2.Navigate(uri);
    }
}
