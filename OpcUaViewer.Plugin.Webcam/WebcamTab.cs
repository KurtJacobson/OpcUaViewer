using System.Windows;
using OpcUaViewer.Core.Contracts;

namespace OpcUaViewer.Plugin.Webcam;

public class WebcamTab : IAppTab, IConfigurableTab
{
    public string Title       => "Webcam";
    public string Icon        => "📷";
    public int    Order       => 50;
    public string Description => "Live webcam feed viewer.";

    public WebcamViewModel ViewModel { get; } = new();

    public FrameworkElement CreateView() => new WebcamView { DataContext = ViewModel };

    public void Configure() => new WebcamSettingsDialog().ShowDialog();
}
