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
using Telerik.Windows.Controls;

namespace FastFoodSystem.Pages
{
    /// <summary>
    /// Lógica de interacción para MenuPage.xaml
    /// </summary>
    public partial class MenuPage : SystemPageClass
    {
        public MenuPage()
        {
            InitializeComponent();
        }

        public override void Refresh()
        {
        }

        private void products_button_Click(object sender, RoutedEventArgs e)
        {
            App.OpenSystemPage<ProductsPage>();
        }

        private async void Providers_button_Click(object sender, RoutedEventArgs e)
        {
            App.ShowLoad();
            await App.GetSystemPopUp<ProvidersPopUp>().Init();
            App.OpenSystemPopUp<ProvidersPopUp>();
        }

        private async void Users_button_Click(object sender, RoutedEventArgs e)
        {
            App.ShowLoad();
            await App.GetSystemPopUp<UsersPopUp>().Init();
            App.OpenSystemPopUp<UsersPopUp>();
        }

        private void New_sale_button_Click(object sender, RoutedEventArgs e)
        {
            App.OpenSystemPage<NewSalePage>();
        }

        private void Sale_button_Click(object sender, RoutedEventArgs e)
        {
            App.OpenSystemPage<SalePage>();
        }

        private void New_purchase_button_Click(object sender, RoutedEventArgs e)
        {
            App.OpenSystemPage<NewPurchasePage>();
        }

        private void Purchase_button_Click(object sender, RoutedEventArgs e)
        {
            App.OpenSystemPage<PurchasePage>();
        }

        private async void In_box_button_Click(object sender, RoutedEventArgs e)
        {
            App.ShowLoad();
            await App.GetSystemPopUp<InBoxPopUp>().Init();
            App.OpenSystemPopUp<InBoxPopUp>();
        }
    }
}
