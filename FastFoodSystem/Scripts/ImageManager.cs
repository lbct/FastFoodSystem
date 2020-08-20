using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Media;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FastFoodSystem.Scripts
{
    public static class ImageManager
    {
        private static Dictionary<string, ImageSource> images;
        private static int dpi = 150;

        static ImageManager()
        {
            images = new Dictionary<string, ImageSource>();
        }

        public static void LoadBitmap(string path, int width, bool update = false)
        {
            if (!string.IsNullOrEmpty(path) && File.Exists(path) && (!images.ContainsKey(path) || update))
            {
                var f = new FileInfo(path);
                var saveFile = new FileInfo(Path.Combine(f.Directory.FullName, "tmp", f.Name));
                if (!saveFile.Exists || update)
                {
                    using (FileStream sr = new FileStream(path, FileMode.Open))
                    {
                        Bitmap original = new Bitmap(sr);
                        int old_wid = original.Width;
                        int old_hgt = original.Height;
                        int new_wid = width;
                        int new_hgt = (int)(((old_hgt + 0.0) / (old_wid + 0.0)) * width);

                        using (Bitmap bm = new Bitmap(new_wid, new_hgt))
                        {
                            System.Drawing.Point[] points =
                            {
                            new System.Drawing.Point(0, 0),
                            new System.Drawing.Point(new_wid, 0),
                            new System.Drawing.Point(0, new_hgt),
                        };
                            using (Graphics gr = Graphics.FromImage(bm))
                            {
                                gr.DrawImage(original, points);
                            }
                            bm.SetResolution(dpi, dpi);

                            Directory.CreateDirectory(saveFile.Directory.FullName);
                            bm.Save(saveFile.FullName);
                            UpdateImage(path, saveFile.FullName);
                        }
                    }
                }
                else
                    UpdateImage(path, saveFile.FullName);
            }
        }

        private static void UpdateImage(string path, string savePath)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                if (images.ContainsKey(path))
                    images[path] = ImageFunctions.FileToImageSource(savePath);
                else
                    images.Add(path, ImageFunctions.FileToImageSource(savePath));
            });
        }

        public static ImageSource GetImageSource(string path)
        {
            ImageSource img = null;
            if (!string.IsNullOrEmpty(path) && images.ContainsKey(path))
                img = images[path];
            return img;
        }

        public static BitmapSource CreateBitmapSourceFromGdiBitmap(Bitmap src)
        {
            MemoryStream ms = new MemoryStream();
            ((System.Drawing.Bitmap)src).Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);
            image.StreamSource = ms;
            image.EndInit();
            return image;
        }

        /*[DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject([In] IntPtr hObject);

        public static ImageSource ImageSourceFromBitmap(Bitmap bmp)
        {
            var handle = bmp.GetHbitmap();
            try
            {
                return Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            finally { DeleteObject(handle); }
        }*/
    }
}
