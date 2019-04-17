using FastFoodSystem.Database;
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

namespace FastFoodSystem.PopUps
{
    /// <summary>
    /// Interaction logic for ProvidersPopUp.xaml
    /// </summary>
    public partial class ProvidersPopUp : SystemPopUpClass
    {
        private string searchText;

        public ProvidersPopUp()
        {
            searchText = "";
            InitializeComponent();
        }

        public async Task Init()
        {
            provider_table.ItemsSource = await App.RunAsync(() => 
            {
                var providers = App.Database.Providers.Where(p => !p.Hide).ToArray();
                providers = providers.Where(p => 
                {
                    bool firstCondition = searchText.Contains(p.FullName.ToLower())
                            || p.FullName.ToLower().Contains(searchText)
                            || searchText.Contains(p.PhoneNumber)
                            || p.PhoneNumber.Contains(searchText)
                            || searchText.Contains(p.EMail)
                            || p.EMail.Contains(searchText)
                            || searchText.Contains(p.Description.ToLower())
                            || p.Description.ToLower().Contains(searchText);
                    /*bool secondCondition = false;
                    try
                    {
                        var products = p.Products.Select(prod =>
                        prod.Description.ToLower());
                        string productsString = string.Join("", products);
                        secondCondition = productsString.Contains(searchText)
                        || searchText.Contains(productsString);
                    }
                    catch { }*/
                    return firstCondition;// || secondCondition;
                }).ToArray();
                return providers;
            });
        }

        private void Edit_provider_button_Initialized(object sender, EventArgs e)
        {
            (sender as Button).Click += new RoutedEventHandler(EditData_Click);
        }

        private void EditData_Click(object sender, RoutedEventArgs e)
        {
            var data = ((FrameworkElement)sender).DataContext as Provider;
            App.OpenSystemPopUp<NewProviderPopUp>().Init(data, async () => 
            {
                await App.OpenSystemPopUp<ProvidersPopUp>().Init();
            }, () => App.OpenSystemPopUp<ProvidersPopUp>());
        }

        private void Add_provider_button_Click(object sender, RoutedEventArgs e)
        {
            App.OpenSystemPopUp<NewProviderPopUp>().Init(async provider => 
            {
                await App.OpenSystemPopUp<ProvidersPopUp>().Init();
            }, () => App.OpenSystemPopUp<ProvidersPopUp>());
        }

        private void Cancel_button_Click(object sender, RoutedEventArgs e)
        {
            App.CloseSystemPopUp();
        }

        private async void Search_button_Click(object sender, RoutedEventArgs e)
        {
            searchText = search_text.Text.Trim().ToLower();
            await Init();
        }

        private void Search_text_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(search_text.Text))
                cancel_search_button.Visibility = Visibility.Visible;
            else
                cancel_search_button.Visibility = Visibility.Collapsed;
        }

        private async void Cancel_search_button_Click(object sender, RoutedEventArgs e)
        {
            searchText = "";
            search_text.Text = "";
            await Init();
        }
    }
}
