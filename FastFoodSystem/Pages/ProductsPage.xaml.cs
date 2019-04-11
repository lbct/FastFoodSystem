using FastFoodSystem.Database;
using FastFoodSystem.PopUps;
using FastFoodSystem.Scripts;
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

namespace FastFoodSystem.Pages
{
    /// <summary>
    /// Lógica de interacción para ProductsPage.xaml
    /// </summary>
    public partial class ProductsPage : SystemPageClass
    {
        public ProductsPage()
        {
            InitializeComponent();
        }

        public override async void Refresh()
        {
            App.ShowLoad();
            productsTable.ItemsSource = await App.RunAsync(() => App.Database.ProductViews.ToArray());
            App.CloseSystemPopUp();
        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            App.OpenSystemPage<MenuPage>();
        }

        private void export_to_excel_button_Click(object sender, Telerik.Windows.RadRoutedEventArgs e)
        {

        }

        private void Edit_product_button_Initialized(object sender, EventArgs e)
        {
            (sender as Button).Click += new RoutedEventHandler(EditData_Click);
        }

        private void EditData_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void New_compound_button_Click(object sender, Telerik.Windows.RadRoutedEventArgs e)
        {

        }

        private void New_simple_button_Click(object sender, Telerik.Windows.RadRoutedEventArgs e)
        {

        }

        private void New_input_button_Click(object sender, Telerik.Windows.RadRoutedEventArgs e)
        {

        }

        private void New_combo_button_Click(object sender, Telerik.Windows.RadRoutedEventArgs e)
        {

        }
    }
}
