using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Drawing.Imaging;
using AForge.Video;
using AForge.Video.DirectShow;

namespace WebCam;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private VideoCaptureDevice _videoSource;
    public MainWindow()
    {
        InitializeComponent();
        InitializeWebCam();
    }

    private void InitializeWebCam()
    {
        FilterInfoCollection videoDevice = new(FilterCategory.VideoInputDevice);
        if (videoDevice.Count > 0)
        {
            _videoSource = new VideoCaptureDevice(videoDevice[0].MonikerString);
            _videoSource.NewFrame += VideoSource_NewFrame;
        }
        else
            MessageBox.Show("Video devices not found", "Info", MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    private void VideoSource_NewFrame(object sender, NewFrameEventArgs eventArgs)
    {
        try
        {
            BitmapImage bi = ConvertBitmapToBitmapImage(eventArgs.Frame);
            bi.Freeze();

            Dispatcher.Invoke(() =>
            {
                ImageBox.Source = bi;
            });
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private BitmapImage ConvertBitmapToBitmapImage(Bitmap bitmap)
    {
        using (MemoryStream memoryStream = new())
        {
            bitmap.Save(memoryStream, ImageFormat.Bmp);
            memoryStream.Position = 0;

            BitmapImage bitmapImage = new();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = memoryStream;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();
            return bitmapImage;
        }
    }

    private void StartButton(object sender, RoutedEventArgs e) => _videoSource.Start();

    private void StopButton(object sender, RoutedEventArgs e)
    {
        base.OnClosed(e);
        if (_videoSource.IsRunning)
        {
            _videoSource.SignalToStop();
            _videoSource.WaitForStop();
            ImageBox.Source = null;
        }
    }
}