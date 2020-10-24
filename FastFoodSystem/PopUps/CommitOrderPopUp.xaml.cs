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
        private SaleOrder order;

        public CommitOrderPopUp()
        {
            InitializeComponent();
        }

        public async Task Init(SaleOrder order)
        {
            okButton.IsEnabled = order.OrderStateId != 2;
            detail_container.Children.Clear();
            this.order = order;
            var saleOrderDetails = await App.RunAsync(() => App.Database.SaleOrderDetails.Where(d => d.SaleOrderId == order.Id).ToArray());

            sale_value.Value = double.Parse(saleOrderDetails
                .Sum(d => d.UnitValue * d.Units * (1.0m - (d.DiscountValue / 100.0m))).ToString());
            client_pay_value.Value = 0;
            change_value.Value = sale_value.Value * -1;
            foreach (var detail in saleOrderDetails)
            {
                var item = new SaleDetailItem()
                {
                    Margin = new Thickness(10),
                    ShowButtons = false
                };
                await item.SetOrderDetail(detail);
                detail_container.Children.Add(item);
            }
        }

        private async void OkButton_Click(object sender, RoutedEventArgs e)
        {
            App.ShowLoad();
            try
            {
                Sale sale = new Sale()
                {
                    BillNumber = 1,
                    ClientId = 1,
                    ControlCode = "0",
                    DailyId = UserSession.DailyId,
                    DateTime = DateTime.Now,
                    LoginId = UserSession.LoginID,
                    SaleTypeId = 1
                };
                bool correct = await App.RunAsync(() =>
                {
                    App.Database.Sales.Add(sale);
                    App.Database.SaveChanges();
                });
                if (correct)
                {
                    await App.RunAsync(() => 
                    {
                        var state = App.Database.OrderStates.First(s => s.Id == 2);
                        order.OrderState = state;
                        order.OrderStateId = state.Id;
                        App.Database.SaveChanges(); 
                    });
                    await AddSaleDetails(sale);
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

        private async Task AddSaleDetails(Sale sale)
        {
            var details = await App.RunAsync(() => App.Database.SaleOrderDetails.Where(d => d.SaleOrderId == order.Id).ToArray());
            foreach(var detail in details)
            {
                SaleDetail saleDetail = new SaleDetail()
                {
                    DiscountValue = detail.DiscountValue,
                    ProductId = detail.ProductId,
                    SaleId = sale.Id,
                    UnitCost = detail.UnitCost,
                    Units = detail.Units,
                    UnitValue = detail.UnitValue
                };
                await App.RunAsync(() =>
                {
                    App.Database.SaleDetails.Add(saleDetail);
                    App.Database.SaveChanges();
                });
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            App.GetSystemPage<NewSalePage>().RefreshOrders();
        }

        private void CalculatorButton_Click(object sender, RoutedEventArgs e)
        {
            var value = double.Parse(((sender as RadButton).ChildrenOfType<Telerik.Windows.Controls.Label>()).First().Content.ToString());
            if (client_pay_value.Value == null)
                client_pay_value.Value = value;
            else
                client_pay_value.Value += value;
        }

        private void Client_pay_value_ValueChanged(object sender, Telerik.Windows.Controls.RadRangeBaseValueChangedEventArgs e)
        {
            if (change_value != null)
            {
                change_value.Value = (client_pay_value.Value == null ? 0 : client_pay_value.Value.Value) - sale_value.Value.Value;
            }
        }

        /*private void Delete_button_Click(object sender, RoutedEventArgs e)
        {
            App.OpenSystemPopUp<ConfirmPopUp>().Init("¿Desea eliminar el pedido?", async () => 
            {
                App.ShowLoad();
                order.Hide = true;
                var details = await App.RunAsync(() =>
                {
                    return App.Database.SaleOrderDetails.Where(d => d.SaleOrderId == order.Id).ToArray();
                });
                foreach(var detail in details)
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
        }*/

        private async Task IncreaseProductUnits(SaleOrderDetail detail)
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

        /*private async void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (!App.GetSystemPage<NewSalePage>().DetailContainerInUse)
            {
                App.ShowLoad();
                order.Hide = true;
                var details = await App.RunAsync(() =>
                {
                    return App.Database.SaleOrderDetails.Where(d => d.SaleOrderId == order.Id).ToArray();
                });
                foreach (var detail in details)
                {
                    await IncreaseProductUnits(detail);
                }
                await App.RunAsync(() => App.Database.SaveChanges());
                await App.GetSystemPopUp<OrdersPopUp>().Init();
                await App.GetSystemPage<NewSalePage>().RefreshAll();
                await App.GetSystemPage<NewSalePage>().SetSaleOrder(order);
                App.CloseSystemPopUp();
            }
            else
                App.ShowMessage("No puede editar el pedido porque la Lista está con productos", false, () =>
                {
                    App.OpenSystemPopUp<CommitOrderPopUp>();
                });
        }*/
    }
}
