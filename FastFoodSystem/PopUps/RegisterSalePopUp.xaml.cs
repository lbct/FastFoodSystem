using FastFoodSystem.Database;
using FastFoodSystem.Pages;
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

                var sale = await CreateSale();
                if (sale != null)
                {
                    App.ShowMessage("Venta realizada", true, () =>
                    {
                        App.GetSystemPage<NewSalePage>().Refresh();
                    });
                }
            }
            catch(Exception ex)
            {
                App.ShowMessage(ex.Message, false, () => App.OpenSystemPopUp<RegisterSalePopUp>());
            }
        }

        private async Task<Sale> CreateSale()
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
                sale = new Sale()
                {
                    ClientId = client.Id,
                    DailyId = UserSession.DailyId,
                    DateTime = DateTime.Now,
                    LoginId = UserSession.LoginID,
                    SaleTypeId = 1
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
                            await ReduceProductUnits(detail);
                    }
                }
            }
            return correct ? sale : null;
        }

        private async Task ReduceProductUnits(SaleDetail detail)
        {
            await ReduceProductUnits(detail.ProductId, detail.Units);
        }

        private async Task ReduceProductUnits(int productId, int units)
        {
            SimpleProduct simple = await App.RunAsync(() => App.Database.SimpleProducts.FirstOrDefault(sp => sp.Id == productId));
            if (simple != null)
                await ReduceProductUnits(simple, units);
            else
            {
                var foodInput = await App.RunAsync(() => App.Database.FoodInputs.FirstOrDefault(fi => fi.Id == productId));
                if (foodInput != null)
                    await ReduceProductUnits(foodInput, units);
                else
                {
                    var compound = await App.RunAsync(() => App.Database.CompoundProducts.FirstOrDefault(p => p.Id == productId));
                    if (compound != null)
                        await ReduceProductUnits(compound, units);
                    else
                    {
                        var combo = await App.RunAsync(() => App.Database.Comboes.FirstOrDefault(p => p.Id == productId));
                        await ReduceProductUnits(combo, units);
                    }
                }
            }
        }

        private async Task ReduceProductUnits(Combo combo, int units)
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
                await ReduceProductUnits(input, units);
            }
            foreach (var relation in simpleRelations)
            {
                var simpleProd = await App.RunAsync(() => App.Database.SimpleProducts.FirstOrDefault(s => s.Id == relation.SimpleProductId));
                await ReduceProductUnits(simpleProd, units);
            }
            foreach (var relation in compoundRelations)
            {
                var comp = await App.RunAsync(() => App.Database.CompoundProducts.FirstOrDefault(c => c.Id == relation.CompoundProductId));
                await ReduceProductUnits(comp, units);
            }
        }

        private async Task ReduceProductUnits(SimpleProduct simple, int units)
        {
            simple.Units = Math.Max(simple.Units - units, 0);
            await App.RunAsync(() => App.Database.SaveChanges());
        }

        private async Task ReduceProductUnits(FoodInput foodInput, int units)
        {
            foodInput.Units = Math.Max(foodInput.Units - units, 0);
            await App.RunAsync(() => App.Database.SaveChanges());
        }

        private async Task ReduceProductUnits(CompoundProduct compound, int units)
        {
            var relations = await App.RunAsync(() => App.Database.CompoundProductFoodInputs
                        .Where(r => r.CompoundProductId == compound.Id)
                        .ToArray());
            foreach (var relation in relations)
            {
                var input = await App.RunAsync(() => App.Database.FoodInputs.FirstOrDefault(fi => fi.Id == relation.FoodInputId));
                await ReduceProductUnits(input, units * relation.RequiredUnits);
            }
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
                var sale = await CreateSale();
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
            }, () =>
            {
                App.OpenSystemPopUp<RegisterSalePopUp>();
            });
        }
    }
}
