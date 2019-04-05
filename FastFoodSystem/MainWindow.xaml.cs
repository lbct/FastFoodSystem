using FastFoodSystem.Database;
using System;
using System.Collections.Generic;
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

namespace FastFoodSystem
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public async void RefreshTable()
        {
            table.ItemsSource = await Task.Factory.StartNew(() => App.DatabaseEntities.Products.ToArray());
            MessageBox.Show("Updated");
        }

        private async void Add_item_button_Click(object sender, RoutedEventArgs e)
        {
            await Task.Factory.StartNew(() =>
            {
                CategoryType category = (from c in App.DatabaseEntities.CategoryTypes
                                         where c.Id == 1
                                         select c).First();
                Product product = new Product()
                {
                    CategoryType = category,
                    CategoryTypeId = category.Id,
                    Description = "Cocacola 500ml",
                    SaleValue = 6
                };
                App.DatabaseEntities.Products.Add(product);
                App.DatabaseEntities.SaveChanges();
                SimpleProduct simple = new SimpleProduct()
                {
                    Product = product,
                    Id = product.Id,
                    UnitCost = 5,
                    Units = 100
                };
                App.DatabaseEntities.SimpleProducts.Add(simple);
                App.DatabaseEntities.SaveChanges();
            });
            MessageBox.Show("Added");
        }

        private void Refresh_button_Click(object sender, RoutedEventArgs e)
        {
            RefreshTable();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
        }
    }
}
