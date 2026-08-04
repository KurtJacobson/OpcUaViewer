using System.Windows;
using System.Windows.Controls;
using Microsoft.Web.WebView2.Wpf;
using OpcUaViewer.Wpf.Tabs;

namespace OpcUaViewer.Wpf.Views;

public partial class DocumentView : UserControl
{
    private WebView2? _webView;
    private bool      _initialized;
    private string    _displayedUri = "\0"; // sentinel — ensures first nav always runs

    public DocumentView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (_initialized) return;

        _webView = new WebView2 { HorizontalAlignment = HorizontalAlignment.Stretch,
                                  VerticalAlignment   = VerticalAlignment.Stretch };
        WebViewHost.Child = _webView;

        await _webView.EnsureCoreWebView2Async();
        _initialized = true;

        _webView.CoreWebView2.Settings.IsStatusBarEnabled            = false;
        _webView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;

        // Dispose WebView2 when the host window closes, not on tab switch
        if (Window.GetWindow(this) is { } win)
            win.Closed += (_, _) => { _webView?.Dispose(); _webView = null; };

        if (DataContext is DocumentTab vm)
        {
            vm.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName == nameof(DocumentTab.DocumentUri))
                    Navigate(vm.DocumentUri);
            };
            Navigate(vm.DocumentUri);
        }
    }

    private void Navigate(string uri)
    {
        if (_webView?.CoreWebView2 is null) return;
        if (uri == _displayedUri) return;   // skip redundant navigations
        _displayedUri = uri;

        if (string.IsNullOrEmpty(uri))
            _webView.CoreWebView2.NavigateToString(PlaceholderHtml);
        else
            _webView.CoreWebView2.Navigate(uri);
    }

    private const string PlaceholderHtml = """
        <!DOCTYPE html>
        <html>
        <head>
        <meta charset="utf-8"/>
        <style>
          * { margin: 0; padding: 0; box-sizing: border-box; }
          body {
            background: #181818;
            color: #505050;
            font-family: 'Segoe UI', sans-serif;
            display: flex;
            flex-direction: column;
            align-items: center;
            justify-content: center;
            height: 100vh;
            user-select: none;
          }
          .icon { font-size: 64px; margin-bottom: 20px; opacity: 0.25; }
          .msg  { font-size: 15px; opacity: 0.5; }
        </style>
        </head>
        <body>
          <div class="icon">📄</div>
          <div class="msg">No document loaded — waiting for a product ID</div>
        </body>
        </html>
        """;

}
