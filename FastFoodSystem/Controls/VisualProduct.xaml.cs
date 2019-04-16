﻿using FastFoodSystem.Database;
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
        public Product Product { get; private set; }

        public VisualProduct()
        {
            InitializeComponent();
        }

        public async void SetProduct(Product product)
        {
            Product = product;
            product_detail_label.Text = "(" + product.Id + ") - "
                + product.Description;
            product_sale_value.Value = double.Parse((await App.RunAsync(() => App.Database.GetProductCost(product.Id))).ToString());
            product_units.Value = await App.RunAsync(() => App.Database.GetProductUnits(product.Id));
            if (!string.IsNullOrEmpty(product.ImagePath) && File.Exists(product.ImagePath))
            {
                image.Source = new BitmapImage(new Uri(product.ImagePath));
            }
        }

        public async Task SetProductAsSale(Product product)
        {
            Product = product;
            product_detail_label.Text = "(" + product.Id + ") - "
                + product.Description;
            product_sale_value.Value = double.Parse((product.SaleValue * (1.0m - (product.SaleDiscount / 100.0m))).ToString());
            product_units.Value = await App.RunAsync(() => App.Database.GetProductUnits(product.Id)); 
            if (!string.IsNullOrEmpty(product.ImagePath) && File.Exists(product.ImagePath))
            {
                image.Source = new BitmapImage(new Uri(product.ImagePath));
            }
            if (product_units.Value.Value <= 0)
            {
                button.BorderBrush = Brushes.Red;
                product_units.Foreground = Brushes.Red;
            }
        }
    }
}
