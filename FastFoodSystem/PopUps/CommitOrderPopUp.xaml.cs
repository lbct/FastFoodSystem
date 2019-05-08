using FastFoodSystem.Controls;
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
    /// Lógica de interacción para CommitOrderPopUp.xaml
    /// </summary>
    public partial class CommitOrderPopUp : SystemPopUpClass
    {
        private SaleDetail[] saleDetails;
        private Order order;
        private Sale sale;

        public CommitOrderPopUp()
        {
            InitializeComponent();
        }

        public async Task Init(Order order)
        {
            detail_container.Children.Clear();
            this.order = order;
            sale = await App.RunAsync(() => App.Database.Sales.FirstOrDefault(s => s.Id == order.SaleId));
            var client = await App.RunAsync(() => App.Database.Clients.FirstOrDefault(c => c.Id == sale.ClientId));
            saleDetails = await App.RunAsync(() => App.Database.SaleDetails.Where(d => d.SaleId == order.SaleId).ToArray());
            sale_value.Value = double.Parse(saleDetails
                .Sum(d => d.UnitValue * d.Units * (1.0m - (d.DiscountValue / 100.0m))).ToString());
            client_pay_value.Value = 0;
            change_value.Value = sale_value.Value * -1;
            client_name.Text = client.Name;
            nit_search_bar.SelectedItem = null;
            nit_search_bar.SearchText = "";
            nit_search_bar.ItemsSource = await App.RunAsync(() => App.Database.Clients.ToArray());
            nit_search_bar.SelectedItem = client;
            foreach (var detail in saleDetails)
            {
                var item = new SaleDetailItem()
                {
                    Margin = new Thickness(10),
                    ShowButtons = false
                };
                await item.SetDetail(detail);
                detail_container.Children.Add(item);
            }
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
                var client = await GetClient();
                var billInfo = await UserSession.GetBillConfig();
                sale.ClientId = client.Id;
                sale.Hide = false;
                sale.DateTime = DateTime.Now;
                sale.DailyId = UserSession.DailyId;
                sale.LoginId = UserSession.LoginID;
                sale.BillNumber = billInfo.CurrentBillNumber;

                sale.ControlCode = await ControlCodeGenerator.GenerateAsync(
                    sale.BillNumber,
                    client.Nit,
                    sale.DateTime,
                    sale_value.Value.Value,
                    billInfo.DosificationCode,
                    billInfo.AuthorizationCode);

                order.Committed = true;

                bool correct = await App.RunAsync(() => 
                {
                    billInfo.CurrentBillNumber++;
                    App.Database.SaveChanges();
                });
                if (correct)
                {
                    App.ShowMessage("Venta realizada", true, () =>
                    {
                        App.GetSystemPage<NewSalePage>().Refresh();
                    });
                }
            }
            catch (Exception ex)
            {
                App.ShowMessage(ex.Message, false, () => App.OpenSystemPopUp<CommitOrderPopUp>());
            }
        }

        private async Task<Client> GetClient()
        {
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
            return client;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            App.OpenSystemPopUp<OrdersPopUp>();
        }

        private void CalculatorButton_Click(object sender, RoutedEventArgs e)
        {
            var value = double.Parse(((sender as RadButton).ChildrenOfType<Telerik.Windows.Controls.Label>()).First().Content.ToString());
            if (client_pay_value.Value == null)
                client_pay_value.Value = value;
            else
                client_pay_value.Value += value;
        }

        private void Nit_search_bar_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (nit_search_bar.SelectedItem != null)
            {
                var client = nit_search_bar.SelectedItem as Client;
                client_name.Text = client.Name;
            }
        }

        private void Client_pay_value_ValueChanged(object sender, Telerik.Windows.Controls.RadRangeBaseValueChangedEventArgs e)
        {
            if (change_value != null)
            {
                change_value.Value = (client_pay_value.Value == null ? 0 : client_pay_value.Value.Value) - sale_value.Value.Value;
            }
        }

        private void Delete_button_Click(object sender, RoutedEventArgs e)
        {
            App.OpenSystemPopUp<ConfirmPopUp>().Init("¿Desea eliminar el pedido?", async () => 
            {
                App.ShowLoad();
                order.Committed = true;
                foreach(var detail in saleDetails)
                {
                    await IncreaseProductUnits(detail);
                }
                await App.RunAsync(() => App.Database.SaveChanges());
                await App.GetSystemPopUp<OrdersPopUp>().Init();
                App.CloseSystemPopUp();
                App.GetSystemPage<NewSalePage>().Refresh();
            }, () => 
            {
                App.OpenSystemPopUp<CommitOrderPopUp>();
            });
        }

        private async Task IncreaseProductUnits(SaleDetail detail)
        {
            await IncreaseProductUnits(detail.ProductId, detail.Units);
        }

        private async Task IncreaseProductUnits(int productId, int units)
        {
            SimpleProduct simple = await App.RunAsync(() => App.Database.SimpleProducts.FirstOrDefault(sp => sp.Id == productId));
            if (simple != null)
                await IncreaseProductUnits(simple, units);
            else
            {
                var foodInput = await App.RunAsync(() => App.Database.FoodInputs.FirstOrDefault(fi => fi.Id == productId));
                if (foodInput != null)
                    await IncreaseProductUnits(foodInput, units);
                else
                {
                    var compound = await App.RunAsync(() => App.Database.CompoundProducts.FirstOrDefault(p => p.Id == productId));
                    if (compound != null)
                        await IncreaseProductUnits(compound, units);
                    else
                    {
                        var combo = await App.RunAsync(() => App.Database.Comboes.FirstOrDefault(p => p.Id == productId));
                        await IncreaseProductUnits(combo, units);
                    }
                }
            }
        }

        private async Task IncreaseProductUnits(Combo combo, int units)
        {
            var foodInputRelations = await App.RunAsync(() => App.Database.FoodInputComboes
                        .Where(r => r.ComboId == combo.Id)
                        .ToArray());
            var simpleRelations = await App.RunAsync(() => App.Database.SimpleProductComboes
            .Where(r => r.ComboId == combo.Id)
            .ToArray());
            var compoundRelations = await App.RunAsync(() => App.Database.CompoundProductComboes
            .Where(r => r.ComboId == combo.Id)
            .ToArray());
            foreach (var relation in foodInputRelations)
            {
                var input = await App.RunAsync(() => App.Database.FoodInputs.FirstOrDefault(fi => fi.Id == relation.FoodInputId));
                await IncreaseProductUnits(input, units);
            }
            foreach (var relation in simpleRelations)
            {
                var simpleProd = await App.RunAsync(() => App.Database.SimpleProducts.FirstOrDefault(s => s.Id == relation.SimpleProductId));
                await IncreaseProductUnits(simpleProd, units);
            }
            foreach (var relation in compoundRelations)
            {
                var comp = await App.RunAsync(() => App.Database.CompoundProducts.FirstOrDefault(c => c.Id == relation.CompoundProductId));
                await IncreaseProductUnits(comp, units);
            }
        }

        private async Task IncreaseProductUnits(SimpleProduct simple, int units)
        {
            simple.Units = simple.Units + units;
            await App.RunAsync(() => App.Database.SaveChanges());
        }

        private async Task IncreaseProductUnits(FoodInput foodInput, int units)
        {
            foodInput.Units = foodInput.Units + units;
            await App.RunAsync(() => App.Database.SaveChanges());
        }

        private async Task IncreaseProductUnits(CompoundProduct compound, int units)
        {
            var relations = await App.RunAsync(() => App.Database.CompoundProductFoodInputs
                        .Where(r => r.CompoundProductId == compound.Id)
                        .ToArray());
            foreach (var relation in relations)
            {
                var input = await App.RunAsync(() => App.Database.FoodInputs.FirstOrDefault(fi => fi.Id == relation.FoodInputId));
                await IncreaseProductUnits(input, units * relation.RequiredUnits);
            }
        }
    }
}
