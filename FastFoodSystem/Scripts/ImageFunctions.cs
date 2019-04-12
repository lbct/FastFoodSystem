using FastFoodSystem.Controls;
using FastFoodSystem.PopUps;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FastFoodSystem.Scripts
{
    class ImageFunctions
    {
        public static string UltimaImagenSeleccionada { get; private set; }
        public static ImageSource SeleccionarImagen()
        {
            ImageSource imagen = null;
            SystemPopUpClass pop = null;
            if (App.IsPopUpOpen())
                pop = App.GetCurrentPopUp();
            App.OpenSystemPopUp<LoadPopUp>();
            OpenFileDialog op = new OpenFileDialog();
            op.Title = "Seleccione una Imagen";
            op.Filter = "Todas las imágenes soportadas|*.jpg;*.jpeg;*.png;*.gif|" +
              "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" +
              "PNG (*.png)|*.png|" +
              "GIF (*.gif)|*.gif";
            if (op.ShowDialog() == true)
            {
                byte[] bytes = File.ReadAllBytes(op.FileName);
                imagen = ByteToImageSource(bytes);//new BitmapImage(new Uri(op.FileName));
                UltimaImagenSeleccionada = op.FileName;
            }
            App.CloseSystemPopUp();
            if(pop != null)
                App.OpenSystemPopUp(pop);
            return imagen;
        }

        public static void AbrirImagen(ImageSource imagen)
        {
            File.WriteAllBytes("imagen.jpg", ImageSourceToBytes(new JpegBitmapEncoder(), imagen));
            Process.Start("imagen.jpg");
        }

        public static async Task<string> GetImagePath(EditableImage image, int id)
        {
            var image_bytes = image.DarImagen();
            string dir = @"C:\" + Assembly.GetEntryAssembly().GetName().Name + @"\Images";
            Directory.CreateDirectory(dir);
            string path = System.IO.Path.Combine(dir, "product_" + id);
            Task<bool> saveImage = new Task<bool>(() =>
            {
                bool saved = false;
                if (image_bytes != null)
                {
                    File.WriteAllBytes(path, image_bytes);
                    saved = true;
                }
                return saved;
            });
            saveImage.Start();
            bool save = await saveImage;
            return save ? path : null;
        }

        public static void GuardarImagen(ImageSource imagen, string dir)
        {
            File.WriteAllBytes(dir, ImageSourceToBytes(new JpegBitmapEncoder(), imagen));
        }

        public static Bitmap BitmapSourceToBitmap(BitmapSource srs)
        {
            int width = srs.PixelWidth;
            int height = srs.PixelHeight;
            int stride = width * ((srs.Format.BitsPerPixel + 7) / 8);
            IntPtr ptr = IntPtr.Zero;
            try
            {
                ptr = Marshal.AllocHGlobal(height * stride);
                srs.CopyPixels(new Int32Rect(0, 0, width, height), ptr, height * stride, stride);
                using (var btm = new System.Drawing.Bitmap(width, height, stride, System.Drawing.Imaging.PixelFormat.Format1bppIndexed, ptr))
                {
                    // Clone the bitmap so that we can dispose it and
                    // release the unmanaged memory at ptr
                    return new Bitmap(btm);
                }
            }
            finally
            {
                if (ptr != IntPtr.Zero)
                    Marshal.FreeHGlobal(ptr);
            }
        }

        public static byte[] ImageSourceToBytes(BitmapEncoder encoder, ImageSource imageSource)
        {
            byte[] bytes = null;
            var bitmapSource = imageSource as BitmapSource;
            if (bitmapSource != null)
            {
                encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                using (var stream = new MemoryStream())
                {
                    encoder.Save(stream);
                    bytes = stream.ToArray();
                }
            }
            return bytes;
        }

        public static ImageSource FileToImageSource(string path)
        {
            ImageSource im = null;
            if (File.Exists(path))
            {
                var imageData = File.ReadAllBytes(path);
                var imageSource = ByteToImageSource(imageData);
                im = imageSource;
            }
            return im;
        }

        public static ImageSource ByteToImageSource(byte[] imageData)
        {
            ImageSource imagen = null;
            try
            {
                BitmapImage biImg = new BitmapImage();
                MemoryStream ms = new MemoryStream(imageData);
                biImg.BeginInit();
                biImg.StreamSource = ms;
                biImg.EndInit();
                imagen = biImg as ImageSource;
            }
            catch (Exception)
            {

            }
            return imagen;
        }
    }
}
