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
    /// Interaction logic for InBoxPopUp.xaml
    /// </summary>
    public partial class InBoxPopUp : SystemPopUpClass
    {
        private GetPurchaseDetailByLogin_Result[] PurchaseData;
        private GetSaleDetailByLogin_Result[] SaleData;
        private GetCashMovement_Result[] CashData;

        public InBoxPopUp()
        {
            InitializeComponent();
        }

        public async Task Init()
        {
            delete_value_button.Visibility = Visibility.Collapsed;
            var login = await App.RunAsync(() => App.Database.Logins.FirstOrDefault(l => l.Id == UserSession.LoginID));
            title.Content = "Caja Chica - " + login.StartDateTime.ToString("dd/MM/yyyy HH:mm");
            List<InBoxItem> items = new List<InBoxItem>();

            SaleData = await App.RunAsync(() => App.Database.GetSaleDetailByLogin(UserSession.LoginID).ToArray());
            PurchaseData = await App.RunAsync(() => App.Database.GetPurchaseDetailByLogin(UserSession.LoginID).ToArray());
            CashData = await App.RunAsync(() => App.Database.GetCashMovement(UserSession.LoginID).ToArray());

            items.AddRange(await GetItemsFromPurchase());
            items.AddRange(await GetItemsFromSale());
            items.AddRange(await GetItemsFromValues());
            items = items.OrderByDescending(i => i.DateTime).ToList();
            items.Insert(0, new InBoxItem()
            {
                DateTime = login.StartDateTime,
                Detail = "Inicio Caja",
                Type = InBoxType.Inicio,
                Value = double.Parse(login.StartCashValue.ToString()),
                Color = Brushes.Black
            });
            total_in_box_value.Value = items.Sum(v => v.Value);
            in_box_table.ItemsSource = items;
        }

        private async Task<InBoxItem[]> GetItemsFromValues()
        {
            InBoxItem[] items = null;
            items = await App.RunAsync(() =>
            {
                return CashData.Select(c =>
                {
                    InBoxItem item = new InBoxItem()
                    {
                        DateTime = c.DateTime,
                        Type = InBoxType.Valor,
                        Detail = c.Description,
                        Value = double.Parse(c.Value.ToString()),
                        Color = c.Value < 0 ? Brushes.IndianRed : Brushes.DarkGreen,
                        Id = c.Id
                    };
                    return item;
                }).ToArray();
            });
            return items == null ? new InBoxItem[0] : items;
        }

        private async Task<InBoxItem[]> GetItemsFromSale()
        {
            InBoxItem[] items = null;
            items = await App.RunAsync(() =>
            {
                return SaleData.Select(s =>
                {
                    InBoxItem item = new InBoxItem()
                    {
                        DateTime = s.DateTime,
                        Type = InBoxType.Venta,
                        Detail = s.ProductDescription,
                        Value = double.Parse(s.TotalValue.ToString()),
                        Color = Brushes.DarkGreen
                    };
                    return item;
                }).ToArray();
            });
            return items == null ? new InBoxItem[0] : items;
        }

        private async Task<InBoxItem[]> GetItemsFromPurchase()
        {
            InBoxItem[] items = null;
            items = await App.RunAsync(() => 
            {
                return PurchaseData.Select(p => 
                {
                    InBoxItem item = new InBoxItem()
                    {
                        DateTime = p.DateTime,
                        Type = InBoxType.Compra,
                        Detail = p.ProductDescription,
                        Value = -double.Parse(p.TotalValue.ToString()),
                        Color = Brushes.IndianRed
                    };
                    return item;
                }).ToArray();
            });
            return items == null ? new InBoxItem[0] : items;
        }

        private void Add_value_button_Click(object sender, RoutedEventArgs e)
        {
            App.OpenSystemPopUp<NewInBoxValue>().Init(async cash => 
            {
                App.OpenSystemPopUp<LoadPopUp>();
                await App.GetSystemPopUp<InBoxPopUp>().Init();
                App.OpenSystemPopUp<InBoxPopUp>();
            }, () => App.OpenSystemPopUp<InBoxPopUp>());
        }

        private void Cancel_button_Click(object sender, RoutedEventArgs e)
        {
            App.CloseSystemPopUp();
        }

        private void In_box_table_SelectionChanged(object sender, Telerik.Windows.Controls.SelectionChangeEventArgs e)
        {
            if (in_box_table.SelectedItem != null)
            {
                if ((in_box_table.SelectedItem as InBoxItem).Type == InBoxType.Valor)
                    delete_value_button.Visibility = Visibility.Visible;
                else
                    delete_value_button.Visibility = Visibility.Collapsed;
            }
            else
            {
                delete_value_button.Visibility = Visibility.Collapsed;
            }
        }

        private async void Delete_value_button_Click(object sender, RoutedEventArgs e)
        {
            App.OpenSystemPopUp<LoadPopUp>();
            var item = in_box_table.SelectedItem as InBoxItem;
            int id = item.Id;
            await App.RunAsync(() =>
            {
                var val = App.Database.CashMovements.FirstOrDefault(i => i.Id == id);
                App.Database.CashMovements.Remove(val);
                App.Database.SaveChanges();
            });
            await App.GetSystemPopUp<InBoxPopUp>().Init();
            App.OpenSystemPopUp<InBoxPopUp>();
        }
    }

    public class InBoxItem
    {
        public int Id { get; set; }
        public DateTime DateTime { get; set; }
        public InBoxType Type { get; set; }
        public string Detail { get; set; }
        public double Value { get; set; }
        public Brush Color { get; set; }
    }

    public enum InBoxType
    {
        Venta,
        Compra,
        Valor,
        Inicio
    }
}
