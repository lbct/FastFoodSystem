using FastFoodSystem.Database;
using FastFoodSystem.Scripts;
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
    /// Lógica de interacción para VisualProduct.xaml
    /// </summary>
    public partial class VisualProduct : UserControl
    {
        public ProductView ProductView { get; set; }
        private ImageSource originalImageSource;

        public VisualProduct()
        {
            InitializeComponent();
            originalImageSource = image.Source;
        }

        public void SetProduct(ProductView product)
        {
            ProductView = product;
            product_detail_label.Text = "(" + product.Id + ") - "
                + product.Description;
            product_sale_value.Value = double.Parse(product.UnitCost + ""); //double.Parse((await App.RunAsync(() => App.Database.GetProductCost(product.Id))).ToString());
            product_units.Value = product.AvailableUnits;
            SetImage(product.ImagePath);
        }

        public void SetProductAsSale(ProductView product)
        {
            ProductView = product;
            product_detail_label.Text = "(" + product.Id + ") - "
                + product.Description;
            product_sale_value.Value = double.Parse((product.UnitSaleValue * (1.0m - (product.SaleDiscount / 100.0m))).ToString());
            product_units.Value = product.AvailableUnits; //await App.RunAsync(() => App.Database.GetProductUnits(product.Id));
            SetImage(product.ImagePath);
            if (product_units.Value.Value <= 0)
            {
                button.BorderBrush = Brushes.Red;
                product_units.Foreground = Brushes.Red;
            }
            else
            {
                button.BorderBrush = Brushes.Black;
                product_units.Foreground = Brushes.Black;
            }
        }

        private void SetImage(string path)
        {
            //ImageManager.LoadBitmap(path, 10/*(int)Width*/);
            var img = ImageManager.GetImageSource(path);
            if (img != null)
                image.Source = img;
            else
                image.Source = originalImageSource;
        }
    }
}
