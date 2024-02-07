using System.Windows;
using System.Windows.Media;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using OpenCvSharp.WpfExtensions;

namespace WebCam;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
        private VideoCapture? _capture;
        private CascadeClassifier? _faceCascade;
        private CascadeClassifier? _eyeCascade;

        public MainWindow()
        {
            InitializeComponent();
            InitializeCamera();
            InitializeCascadeClassifiers();
        }

        private void InitializeCamera()
        {
            _capture = new VideoCapture();
            _capture.Open(0);

            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }

        private void InitializeCascadeClassifiers()
        {
            _faceCascade = new CascadeClassifier();
            _eyeCascade = new CascadeClassifier();

            if (!_faceCascade.Load(@".\haarcascade_frontalface_alt.xml"))
                MessageBox.Show("Failed to load face cascade classifier!");

            if (!_eyeCascade.Load(@".\haarcascade_eye.xml"))
                MessageBox.Show("Failed to load eye cascade classifier!");
        }

        private void CompositionTarget_Rendering(object? sender, EventArgs e)
        {
            Mat frame = new Mat();
            _capture?.Read(frame);

            if (!frame.Empty())
            {
                var grayFrame = frame.CvtColor(ColorConversionCodes.BGR2GRAY);
            var faces = _faceCascade?.DetectMultiScale(grayFrame, 1.1, 5, (HaarDetectionTypes)HaarDetectionTypes.ScaleImage, new OpenCvSharp.Size(60, 60));

            if (faces != null)
                    foreach (var faceRect in faces)
                    {
                        Cv2.Rectangle(frame, faceRect, Scalar.Red, 2);

                        var faceRoi = grayFrame[faceRect];
                        var eyes = _eyeCascade?.DetectMultiScale(faceRoi, 1.1, 3,
                            (HaarDetectionTypes)HaarDetectionTypes.ScaleImage, new OpenCvSharp.Size(20, 20));

                        if (eyes != null)
                            foreach (var eyeRect in eyes)
                            {
                                var adjustedEyeRect = eyeRect;
                                adjustedEyeRect.X += faceRect.X;
                                adjustedEyeRect.Y += faceRect.Y;

                            Cv2.Rectangle(frame, adjustedEyeRect, Scalar.Blue, 1);
                            }
                    }

                imgCamera.Source = frame.ToBitmapSource();
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _capture?.Release();
        }
    }