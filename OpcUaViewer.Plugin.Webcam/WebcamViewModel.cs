using System;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using OpenCvSharp;
using OpcUaViewer.Core.Contracts;
using OpcUaViewer.Core.Services;
using OpcUaViewer.Core.Settings;

namespace OpcUaViewer.Plugin.Webcam;

public class WebcamViewModel : ViewModelBase, IDisposable
{
    private BitmapSource? _frame;
    private bool          _isRunning;
    private string        _statusText = "Stopped";

    private Thread?  _captureThread;
    private volatile bool _running;

    public BitmapSource? Frame      { get => _frame;      private set => Set(ref _frame, value); }
    public bool          IsRunning  { get => _isRunning;  private set => Set(ref _isRunning, value); }
    public string        StatusText { get => _statusText; private set => Set(ref _statusText, value); }

    public RelayCommand StartCommand { get; }
    public RelayCommand StopCommand  { get; }

    public WebcamViewModel()
    {
        StartCommand = new RelayCommand(Start, () => !IsRunning);
        StopCommand  = new RelayCommand(Stop,  () => IsRunning);
    }

    public void Start()
    {
        if (_running) return;
        _running = true;
        IsRunning = true;
        StatusText = "Starting…";

        _captureThread = new Thread(CaptureLoop) { IsBackground = true, Name = "WebcamCapture" };
        _captureThread.Start();
    }

    public void Stop()
    {
        _running = false;
        IsRunning = false;
        StatusText = "Stopped";
        Frame = null;
    }

    private void CaptureLoop()
    {
        var idx = AppSettings.Current.WebcamIndex;
        using var cap = new VideoCapture(idx);

        if (!cap.IsOpened())
        {
            AppLogger.Error($"Webcam: failed to open camera index {idx}");
            Application.Current?.Dispatcher.BeginInvoke(() =>
            {
                StatusText = $"Failed to open camera {idx}";
                IsRunning  = false;
                _running   = false;
            });
            return;
        }

        Application.Current?.Dispatcher.BeginInvoke(() =>
            StatusText = $"Camera {idx} — {(int)cap.Fps} fps  {cap.FrameWidth}×{cap.FrameHeight}");

        using var mat = new Mat();

        while (_running)
        {
            if (!cap.Read(mat) || mat.Empty())
            {
                Thread.Sleep(100);
                continue;
            }

            var bs = MatToBitmapSource(mat);
            bs.Freeze();
            Application.Current?.Dispatcher.BeginInvoke(() => Frame = bs);

            Thread.Sleep(33); // ~30 fps
        }
    }

    public void Dispose() => Stop();

    private static BitmapSource MatToBitmapSource(Mat mat)
    {
        using var bgra = new Mat();
        Cv2.CvtColor(mat, bgra, ColorConversionCodes.BGR2BGRA);
        return BitmapSource.Create(
            bgra.Width, bgra.Height,
            96, 96,
            PixelFormats.Bgra32,
            null,
            bgra.Data,
            (int)(bgra.Step() * bgra.Height),
            (int)bgra.Step());
    }
}
