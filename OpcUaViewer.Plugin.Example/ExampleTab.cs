using System;
using System.Windows;
using System.Windows.Threading;
using OpcUaViewer.Core.Contracts;
using OpcUaViewer.Core.Services;

namespace OpcUaViewer.Plugin.Example;

public class ExampleTab : ViewModelBase, IAppTab
{
    public string Title       => "Example";
    public string Icon        => "🔌";
    public int    Order       => 80;
    public string Description => "Reference plugin demonstrating the machine data source plugin API.";

    public FrameworkElement CreateView() => new ExampleView { DataContext = this };

    private string _statusText   = "Not connected";
    private string _productId    = "—";
    private string _program      = "—";
    private string _machineState = "—";
    private string _clock        = "";

    public string StatusText   { get => _statusText;   private set => Set(ref _statusText,   value); }
    public string ProductId    { get => _productId;    private set => Set(ref _productId,    value); }
    public string Program      { get => _program;      private set => Set(ref _program,      value); }
    public string MachineState { get => _machineState; private set => Set(ref _machineState, value); }
    public string Clock        { get => _clock;        private set => Set(ref _clock,        value); }

    public ExampleTab()
    {
        var src = DataSourceRegistry.Get<IMachineSource>();
        if (src is not null)
        {
            src.StatusChanged       += (_, msg) => Dispatch(() => StatusText   = msg);
            src.ProductIdChanged    += (_, v)   => Dispatch(() => ProductId    = v);
            src.ProgramChanged      += (_, v)   => Dispatch(() => Program      = v);
            src.MachineStateChanged += (_, v)   => Dispatch(() => MachineState = v.ToString());
        }

        var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        timer.Tick += (_, _) => Clock = DateTime.Now.ToString("HH:mm:ss");
        timer.Start();
        Clock = DateTime.Now.ToString("HH:mm:ss");
    }

    private static void Dispatch(Action a)
    {
        if (Application.Current?.Dispatcher.CheckAccess() == true) a();
        else Application.Current?.Dispatcher.BeginInvoke(a);
    }
}
