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
    /// Lógica de interacción para SessionSalesPopUp.xaml
    /// </summary>
    public partial class SessionSalesPopUp : SystemPopUpClass
    {
        public SessionSalesPopUp()
        {
            InitializeComponent();
        }

        public async Task Init()
        {
            var itemsSource = await App.RunAsync(() =>
            {
                var sales = App.Database.Sales
                .Where(s => !s.Hide && s.LoginId == UserSession.LoginID)
                .OrderByDescending(s => s.DailyId)
                .ToArray();
                var items = new List<SessionSaleItem>();
                foreach (var sale in sales)
                {
                    var client = App.Database.Clients.FirstOrDefault(c => c.Id == sale.ClientId);
                    var item = new SessionSaleItem()
                    {
                        Sale = sale,
                        Client = client,
                        TotalValue = sale.SaleDetails.Sum(sd => sd.Units * sd.UnitValue)
                    };
                    items.Add(item);
                }
                return items;
            });
            sales_table.ItemsSource = itemsSource;
            total_sale_value.Value = double.Parse(itemsSource.Sum(i => i.TotalValue).ToString());
        }

        private void View_sale_button_Initialized(object sender, EventArgs e)
        {
            (sender as Button).Click += new RoutedEventHandler(EditData_Click);
        }

        private async void EditData_Click(object sender, RoutedEventArgs e)
        {
            App.ShowLoad();
            var data = ((FrameworkElement)sender).DataContext as SessionSaleItem;
            await App.GetSystemPopUp<SaleViewerPopUp>().Init(data.Sale, () => App.OpenSystemPopUp<SessionSalesPopUp>());
            App.OpenSystemPopUp<SaleViewerPopUp>();
        }

        private void Cancel_button_Click(object sender, RoutedEventArgs e)
        {
            App.CloseSystemPopUp();
        }
    }

    public class SessionSaleItem
    {
        public Sale Sale { get; set; }
        public Client Client { get; set; }
        public decimal TotalValue { get; set; }
    }
}
