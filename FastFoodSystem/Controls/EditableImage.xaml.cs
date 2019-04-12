using FastFoodSystem.PopUps;
using FastFoodSystem.Scripts;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace FastFoodSystem.Controls
{
    /// <summary>
    /// Lógica de interacción para EditableImage.xaml
    /// </summary>
    public partial class EditableImage : UserControl
    {
        private Button botonEditar;
        private Button botonEliminar;
        private ImageSource imagenPorDefecto;
        private bool ocultarBotonEliminarImagen;

        private int id_producto;
        public string DireccionImagen { get; private set; }

        public EditableImage()
        {
            InitializeComponent();
            ocultarBotonEliminarImagen = true;
            imagenPorDefecto = imagenProducto.Source;
            id_producto = 0;
        }

        public void InicializarParaProducto(int id_producto)
        {
            this.id_producto = id_producto;
        }

        private void botonEliminar_Click(object sender, RoutedEventArgs e)
        {
            SystemPopUpClass currentPop = null;
            if(App.IsPopUpOpen())
                currentPop = App.GetCurrentPopUp();
            var pop = App.OpenSystemPopUp<ConfirmPopUp>();
            pop.Init("Se eliminará la imagen seleccionada, ¿Desea realizar la acción?", () => 
            {
                BorrarImagen();
                if(currentPop != null)
                    App.OpenSystemPopUp(currentPop);
            }, () => 
            {
                if (currentPop != null)
                    App.OpenSystemPopUp(currentPop);
            });
        }

        private void botonEditar_Click(object sender, RoutedEventArgs e)
        {
            ImageSource imagen = ImageFunctions.SeleccionarImagen();
            if (imagen != null)
            {
                EstablecerImagen(imagen, ImageFunctions.UltimaImagenSeleccionada);
            }
        }

        private void botonTomarFoto_Click(object sender, RoutedEventArgs e)
        {
            SystemPopUpClass currentPop = null;
            if (App.IsPopUpOpen())
                currentPop = App.GetCurrentPopUp();
            string temp_location = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "temp_picture_cam");
            var pop = new WebCamPopUp();
            pop.Owner = App.MainWin;
            pop.SetValues(() => 
            {
                EstablecerImagen(ImageFunctions.ByteToImageSource(File.ReadAllBytes(temp_location)), temp_location);
                if (currentPop != null)
                    App.OpenSystemPopUp(currentPop);
            }, () => 
            {
                if (currentPop != null)
                    App.OpenSystemPopUp(currentPop);
            }, temp_location);
        }

        public void EstablecerImagen(ImageSource imagen, string direccion)
        {
            try
            {
                botonEliminar.Visibility = Visibility.Visible;
            }
            catch (NullReferenceException)
            {
                ocultarBotonEliminarImagen = false;
            }
            imagenProducto.Source = imagen;
            imagenProducto.Width = imagenProducto.Height = double.NaN;
            txtImagen.Visibility = Visibility.Hidden;
            DireccionImagen = direccion;
        }

        public int DarHashImagen()
        {
            int hash = 0;
            if (!string.IsNullOrWhiteSpace(DireccionImagen) && File.Exists(DireccionImagen))
            {
                byte[] img = File.ReadAllBytes(DireccionImagen);
                hash = JsonConvert.SerializeObject(img).GetHashCode();
            }
            return hash;
        }

        public int DarHashImagenProducto()
        {
            int hash = 0;
            if (botonEliminar.Visibility == Visibility.Visible)
            {
                byte[] img = ImageFunctions.ImageSourceToBytes(new JpegBitmapEncoder(), imagenProducto.Source);
                hash = JsonConvert.SerializeObject(img).GetHashCode();
            }
            return hash;
        }

        public ImageSource DarImageSource()
        {
            ImageSource img = null;
            if (botonEliminar.Visibility == Visibility.Visible)
                img = imagenProducto.Source;
            return img;
        }

        public byte[] DarImagen()
        {
            byte[] imagen = null;
            if (!string.IsNullOrWhiteSpace(DireccionImagen))
            {
                imagen = File.ReadAllBytes(DireccionImagen);
            }
            return imagen;
        }

        public void BorrarImagen()
        {
            try
            {
                imagenProducto.Source = imagenPorDefecto;
                imagenProducto.Height = imagenProducto.Width = 50;
                botonEliminar.Visibility = Visibility.Collapsed;
                txtImagen.Visibility = Visibility.Visible;
                DireccionImagen = "";
            }
            catch (Exception) { }
        }

        private void botonEliminar_Initialized(object sender, EventArgs e)
        {
            botonEliminar = (sender as Button);
            botonEliminar.Click += new RoutedEventHandler(botonEliminar_Click);
            if (ocultarBotonEliminarImagen)
                botonEliminar.Visibility = Visibility.Collapsed;
            else
                botonEliminar.Visibility = Visibility.Visible;
        }

        private void botonEditar_Initialized(object sender, EventArgs e)
        {
            botonEditar = (sender as Button);
            botonEditar.Click += new RoutedEventHandler(botonEditar_Click);
        }

        private void botonTomarFoto_Initialized(object sender, EventArgs e)
        {
            var botonFoto = (sender as Button);
            botonFoto.Click += new RoutedEventHandler(botonTomarFoto_Click);
        }
    }
}
