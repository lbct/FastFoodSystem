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
            productsTable.ItemsSource = await App.RunAsync(() => 
            {
                return App.Database.GetProductView();
            });
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

        private async void EditData_Click(object sender, RoutedEventArgs e)
        {
            App.ShowLoad();
            var data = ((FrameworkElement)sender).DataContext as ProductView;
            if((await App.RunAsync(() => App.Database.SimpleProducts.FirstOrDefault(sp => sp.Id == data.Id))) != null)
            {
                await App.GetSystemPopUp<NewSimpleProductPopUp>().Init(data.Id);
                App.OpenSystemPopUp<NewSimpleProductPopUp>();
            }
            else if((await App.RunAsync(() => App.Database.FoodInputs.FirstOrDefault(fi => fi.Id == data.Id))) != null)
            {
                await App.GetSystemPopUp<NewFoodInputPopUp>().Init(data.Id);
                App.OpenSystemPopUp<NewFoodInputPopUp>();
            }
            else if ((await App.RunAsync(() => App.Database.CompoundProducts.FirstOrDefault(fi => fi.Id == data.Id))) != null)
            {
                await App.GetSystemPopUp<NewCompoundProductPopUp>().Init(data.Id);
                App.OpenSystemPopUp<NewCompoundProductPopUp>();
            }
            else
            {
                await App.GetSystemPopUp<NewComboPopUp>().Init(data.Id);
                App.OpenSystemPopUp<NewComboPopUp>();
            }
        }

        private async void New_compound_button_Click(object sender, Telerik.Windows.RadRoutedEventArgs e)
        {
            App.ShowLoad();
            await App.GetSystemPopUp<NewCompoundProductPopUp>().Init();
            App.OpenSystemPopUp<NewCompoundProductPopUp>();
        }

        private async void New_simple_button_Click(object sender, Telerik.Windows.RadRoutedEventArgs e)
        {
            App.ShowLoad();
            await App.GetSystemPopUp<NewSimpleProductPopUp>().Init();
            App.OpenSystemPopUp<NewSimpleProductPopUp>();
        }

        private async void New_input_button_Click(object sender, Telerik.Windows.RadRoutedEventArgs e)
        {
            App.ShowLoad();
            await App.GetSystemPopUp<NewFoodInputPopUp>().Init();
            App.OpenSystemPopUp<NewFoodInputPopUp>();
        }

        private async void New_combo_button_Click(object sender, Telerik.Windows.RadRoutedEventArgs e)
        {
            App.ShowLoad();
            await App.GetSystemPopUp<NewComboPopUp>().Init();
            App.OpenSystemPopUp<NewComboPopUp>();
        }
    }
}
