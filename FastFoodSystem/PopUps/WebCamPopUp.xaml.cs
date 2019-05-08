using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WebEye.Controls.Wpf;

namespace FastFoodSystem.PopUps
{
    /// <summary>
    /// Lógica de interacción para WebCamPopUp.xaml
    /// </summary>
    public partial class WebCamPopUp : Window
    {
        private WebCameraId cameraId;
        private string saveLocation;
        private Action action;
        private Action cancelAction;

        public WebCamPopUp()
        {
            InitializeComponent();
            var allCameras = camera.GetVideoCaptureDevices();
            if (allCameras != null && allCameras.Count() > 0)
                cameraId = allCameras.First();
            else
                cameraId = null;
        }

        public void SetValues(Action action, Action cancel, string location)
        {
            this.cancelAction = cancel;
            App.OpenSystemPopUp<LoadPopUp>();
            if (cameraId != null)
            {
                this.action = action;
                saveLocation = location;
                ShowDialog();
            }
            else
            {
                App.ShowMessage("No existen cámaras conectadas.", false);
                App.CloseSystemPopUp();
                cancelAction?.Invoke();
                Close();
            }
        }

        private void take_picture_button_Click(object sender, RoutedEventArgs e)
        {
            EncoderParameters encoderParameters = new EncoderParameters(1);
            encoderParameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 100L);
            
            camera.GetCurrentImage().Save(saveLocation, GetEncoder(ImageFormat.Jpeg), encoderParameters);
            if(camera.IsCapturing)
                camera.StopCapture();
            App.CloseSystemPopUp();
            action.Invoke();
            Close();
        }

        public static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();

            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }

            return null;
        }

        private void cancel_button_Click(object sender, RoutedEventArgs e)
        {
            if(camera.IsCapturing)
                camera.StopCapture();
            App.CloseSystemPopUp();
            cancelAction?.Invoke();
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (cameraId != null)
                camera.StartCapture(cameraId);
        }
    }
}
