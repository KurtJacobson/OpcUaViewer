using System;
using System.Windows.Threading;
using OpcUaViewer.Core.Contracts;
using OpcUaViewer.Core.Services;

namespace OpcUaViewer.Plugin.Example;

public class ExampleTab : ViewModelBase, IAppTab
{
    public string Title => "Example";
    public string Icon  => "🔌";
    public int    Order => 90;

    private string _statusText   = "Not connected";
    private string _productId    = "—";
    private string _camFile      = "—";
    private string _machineState = "—";
    private string _clock        = "";

    public string StatusText   { get => _statusText;   private set => Set(ref _statusText,   value); }
    public string ProductId    { get => _productId;    private set => Set(ref _productId,    value); }
    public string CamFile      { get => _camFile;      private set => Set(ref _camFile,      value); }
    public string MachineState { get => _machineState; private set => Set(ref _machineState, value); }
    public string Clock        { get => _clock;        private set => Set(ref _clock,        value); }

    public ExampleTab(OpcUaService opc)
    {
        opc.StatusChanged       += (_, msg) => Dispatch(() => StatusText   = msg);
        opc.ProductIdChanged    += (_, v)   => Dispatch(() => ProductId    = v);
        opc.CamFileChanged      += (_, v)   => Dispatch(() => CamFile      = v);
        opc.MachineStateChanged += (_, v)   => Dispatch(() => MachineState = v.ToString());

        var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        timer.Tick += (_, _) => Clock = DateTime.Now.ToString("HH:mm:ss");
        timer.Start();
        Clock = DateTime.Now.ToString("HH:mm:ss");
    }

    private static void Dispatch(Action a)
    {
        if (System.Windows.Application.Current?.Dispatcher.CheckAccess() == true) a();
        else System.Windows.Application.Current?.Dispatcher.BeginInvoke(a);
    }
}
