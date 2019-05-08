using FastFoodSystem.Database;
using FastFoodSystem.Pages;
using FastFoodSystem.Scripts;
using FastFoodSystem.Scripts.Billing;
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

namespace FastFoodSystem.PopUps
{
    /// <summary>
    /// Lógica de interacción para RegisterSalePopUp.xaml
    /// </summary>
    public partial class RegisterSalePopUp : SystemPopUpClass
    {
        private SaleDetail[] saleDetails;

        public RegisterSalePopUp()
        {
            InitializeComponent();
        }

        public async Task Init(SaleDetail[] details)
        {
            saleDetails = details;
            sale_value.Value = double.Parse(details
                .Sum(d => d.UnitValue * d.Units * (1.0m - (d.DiscountValue / 100.0m))).ToString());
            client_pay_value.Value = 0;
            change_value.Value = sale_value.Value * -1;
            client_name.Text = "";
            nit_search_bar.SelectedItem = null;
            nit_search_bar.SearchText = "";
            nit_search_bar.ItemsSource = await App.RunAsync(() => App.Database.Clients.ToArray());
        }

        private async void OkButton_Click(object sender, RoutedEventArgs e)
        {
            App.ShowLoad();
            try
            {
                if (nit_search_bar.SelectedItem == null && string.IsNullOrEmpty(nit_search_bar.SearchText.Trim()))
                    throw new Exception("Debe escribir un Nit");
                if (string.IsNullOrEmpty(client_name.Text.Trim()))
                    throw new Exception("Debe escribir un Nombre");
                var billConfig = await UserSession.GetBillConfig();
                await DatabaseActions.ValidateBillInformation();
                var sale = await CreateSale(UserSession.DailyId, billConfig.CurrentBillNumber);
                
                if (sale != null)
                {
                    await App.RunAsync(() =>
                    {
                        billConfig.CurrentBillNumber++;
                        App.Database.SaveChanges();
                    });
                    App.ShowMessage("Venta realizada", true, async () =>
                    {
                        App.ShowLoad();
                        await App.GetSystemPage<BillViewerPage>().Init(sale);
                        App.GetSystemPage<NewSalePage>().Refresh();
                        App.OpenSystemPage<BillViewerPage>();
                    });
                }
            }
            catch(Exception ex)
            {
                App.ShowMessage(ex.Message, false, () => App.OpenSystemPopUp<RegisterSalePopUp>());
            }
        }

        private async Task<Sale> CreateSale(int dailyId, int billNumber)
        {
            bool correct = true;
            Sale sale = null;
            string name = client_name.Text.Trim().ToUpper();
            Client client = null;
            if (nit_search_bar.SelectedItem != null)
            {
                client = nit_search_bar.SelectedItem as Client;
                client.Name = name;
            }
            else
            {
                string nitTxt = nit_search_bar.SearchText.Trim();
                client = await App.RunAsync(() => App.Database.Clients
                .FirstOrDefault(c => c.Nit.Equals(nitTxt)));
                if (client == null)
                {
                    client = new Client()
                    {
                        Name = name,
                        Nit = nitTxt
                    };
                    App.Database.Clients.Add(client);
                }
                else
                    client.Name = name;
            }
            correct = await App.RunAsync(() =>
            {
                App.Database.SaveChanges();
            });
            if (correct)
            {
                var billConfig = await UserSession.GetBillConfig();
                var saleDateTime = DateTime.Now;

                string controlCode = await ControlCodeGenerator.GenerateAsync(
                    billConfig.CurrentBillNumber, 
                    client.Nit,
                    saleDateTime, 
                    sale_value.Value.Value, 
                    billConfig.DosificationCode, 
                    billConfig.AuthorizationCode);

                sale = new Sale()
                {
                    ClientId = client.Id,
                    DailyId = dailyId,
                    DateTime = saleDateTime,
                    LoginId = UserSession.LoginID,
                    SaleTypeId = 1,
                    BillNumber = billNumber,
                    ControlCode = controlCode
                };
                correct = await App.RunAsync(() =>
                {
                    App.Database.Sales.Add(sale);
                    App.Database.SaveChanges();
                });
                if (correct)
                {
                    foreach (var detail in saleDetails)
                    {
                        detail.SaleId = sale.Id;
                        sale.SaleDetails.Add(detail);
                        App.Database.SaleDetails.Add(detail);
                    }
                    correct = await App.RunAsync(() => { App.Database.SaveChanges(); });
                    if (correct)
                    {
                        foreach (var detail in saleDetails)
                            await DatabaseActions.ReduceProductUnits(detail);
                    }
                }
            }
            return correct ? sale : null;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            App.CloseSystemPopUp();
        }

        private void CalculatorButton_Click(object sender, RoutedEventArgs e)
        {
            var value = double.Parse(((sender as RadButton).ChildrenOfType<Telerik.Windows.Controls.Label>()).First().Content.ToString());
            if (client_pay_value.Value == null)
                client_pay_value.Value = value;
            else
                client_pay_value.Value += value;
        }

        private void Client_pay_value_ValueChanged(object sender, RadRangeBaseValueChangedEventArgs e)
        {
            if(change_value != null)
            {
                change_value.Value =  (client_pay_value.Value == null ? 0 : client_pay_value.Value.Value) - sale_value.Value.Value;
            }
        }

        private void Nit_search_bar_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(nit_search_bar.SelectedItem != null)
            {
                var client = nit_search_bar.SelectedItem as Client;
                client_name.Text = client.Name;
            }
        }

        private void Save_as_order_button_Click(object sender, RoutedEventArgs e)
        {
            App.OpenSystemPopUp<NewOrderPopUp>().Init(async (name, obs) =>
            {
                App.ShowLoad();
                if (nit_search_bar.SelectedItem == null && string.IsNullOrEmpty(nit_search_bar.SearchText.Trim()))
                {
                    nit_search_bar.SearchText = "0";
                    client_name.Text = "SIN NOMBRE";
                }
                if (string.IsNullOrEmpty(client_name.Text.Trim()))
                    client_name.Text = "SIN NOMBRE";
                try
                {
                    await DatabaseActions.ValidateBillInformation();
                    var sale = await CreateSale(0, 0);
                    if (sale != null)
                    {
                        Order order = new Order()
                        {
                            DailyId = UserSession.DailyOrderId,
                            Observation = obs,
                            OrderName = name,
                            SaleId = sale.Id,
                            Committed = false
                        };
                        sale.Hide = true;
                        bool correct = await App.RunAsync(() =>
                        {
                            App.Database.Orders.Add(order);
                            App.Database.SaveChanges();
                        });
                        App.ShowMessage("Pedido (" + order.DailyId + ") guardado a nombre de " + order.OrderName, true,
                            () => App.GetSystemPage<NewSalePage>().Refresh());
                    }
                    else
                        App.OpenSystemPopUp<RegisterSalePopUp>();
                }
                catch(Exception ex)
                {
                    App.ShowMessage(ex.Message, false, () => App.OpenSystemPopUp<RegisterSalePopUp>());
                }
            }, () =>
            {
                App.OpenSystemPopUp<RegisterSalePopUp>();
            });
        }
    }
}
