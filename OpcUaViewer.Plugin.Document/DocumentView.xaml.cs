using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;

namespace OpcUaViewer.Plugin.Document;

public partial class DocumentView : UserControl
{
    private WebView2? _webView;
    private bool      _initialized;
    private string    _displayedUri     = "\0";
    private string    _displayedMessage = "\0";

    public DocumentView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (_initialized) return;

        bool webView2Available;
        try { webView2Available = CoreWebView2Environment.GetAvailableBrowserVersionString() is not null; }
        catch { webView2Available = false; }

        if (!webView2Available)
        {
            NoWebView2Panel.Visibility = Visibility.Visible;
            return;
        }

        _webView = new WebView2 { HorizontalAlignment = HorizontalAlignment.Stretch,
                                  VerticalAlignment   = VerticalAlignment.Stretch };
        WebViewHost.Child = _webView;

        await _webView.EnsureCoreWebView2Async();
        _initialized = true;

        _webView.CoreWebView2.Settings.IsStatusBarEnabled            = false;
        _webView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;

        if (Window.GetWindow(this) is { } win)
            win.Closed += (_, _) => { _webView?.Dispose(); _webView = null; };

        if (DataContext is DocumentTab vm)
        {
            vm.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName == nameof(DocumentTab.DocumentUri) ||
                    args.PropertyName == nameof(DocumentTab.StatusText))
                    Navigate(vm.DocumentUri, vm.StatusText);
            };
            Navigate(vm.DocumentUri, vm.StatusText);
        }
    }

    private void Navigate(string uri, string statusText)
    {
        if (_webView?.CoreWebView2 is null) return;
        if (uri == _displayedUri && statusText == _displayedMessage) return;
        _displayedUri     = uri;
        _displayedMessage = statusText;

        if (string.IsNullOrEmpty(uri))
            _webView.CoreWebView2.NavigateToString(LoadPlaceholder(statusText));
        else
            _webView.CoreWebView2.Navigate(uri);
    }

    private void WebView2Link_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        Process.Start(new ProcessStartInfo(e.Uri.ToString()) { UseShellExecute = true });
        e.Handled = true;
    }

    private static string LoadPlaceholder(string message)
    {
        string path = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "Assets", "no-document.html");

        if (!File.Exists(path))
            return $"<body style='background:#181818;color:#606060;font-family:Segoe UI;display:flex;align-items:center;justify-content:center;height:100vh'>{message}</body>";

        return File.ReadAllText(path).Replace("{{MESSAGE}}", message);
    }
}
