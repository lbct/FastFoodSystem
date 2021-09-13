using FastFoodSystem.Database;
using FastFoodSystem.Pages;
using FastFoodSystem.PopUps;
using FastFoodSystem.Scripts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Printing;
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

namespace FastFoodSystem.Controls
{
    /// <summary>
    /// Lógica de interacción para OrdersTab.xaml
    /// </summary>
    public partial class OrdersTab : UserControl
    {
        private bool loading;
        private bool visualLoaded;
        private List<OrderState> orderStateList;

        public OrdersTab()
        {
            InitializeComponent();
            visualLoaded = false;
            
            selected_date.SelectedValue = DateTime.Now;
            selected_date.SelectedDate = DateTime.Now;
            selected_date.SelectionChanged += selected_date_SelectionChanged;
        }

        public async void Init()
        {
            if (!loading)
            {
                loading = true;

                orderStateList = await App.RunAsync(() => App.Database.OrderStates.ToList());
                
                var options = new string[] { "Todos", orderStateList[0].Name, $"{orderStateList[2].Name} y {orderStateList[0].Name}", orderStateList[2].Name, orderStateList[1].Name };
                filter_combo.ItemsSource = options;
                filter_combo.SelectedIndex = 2;

                if (!visualLoaded)
                {
                    visualLoaded = true;
                    App.ShowLoad();
                }
                List<SaleOrder> orders = new List<SaleOrder>();
                if (filter_combo.SelectedIndex == 0 || filter_combo.SelectedIndex == 1 || filter_combo.SelectedIndex == 2)
                {
                    var pending = await App.RunAsync(() =>
                    {
                        return App.Database.SaleOrders
                        .Where(o => !o.Hide && o.OrderStateId == 1)
                        .OrderBy(o => o.DateTime)
                        .ToArray();
                    });
                    if (pending != null && pending.Length > 0)
                    {
                        foreach (var order in pending)
                        {
                            if(order.DateTime.Date == selected_date.SelectedDate)
                                orders.Add(order);
                        }
                    }
                }
                if (filter_combo.SelectedIndex == 0 || filter_combo.SelectedIndex == 2 || filter_combo.SelectedIndex == 3)
                {
                    var payed = await App.RunAsync(() =>
                    {
                        return App.Database.SaleOrders
                        .Where(o => !o.Hide && o.OrderStateId == 3)
                        .OrderBy(o => o.DateTime)
                        .ToArray();
                    });
                    if(payed != null && payed.Length > 0)
                    {
                        foreach (var order in payed)
                        {
                            if (order.DateTime.Date == selected_date.SelectedDate)
                                orders.Add(order);
                        }
                    }
                }
                if (filter_combo.SelectedIndex == 0 || filter_combo.SelectedIndex == 4)
                {
                    var realized = await App.RunAsync(() =>
                    {
                        return App.Database.SaleOrders
                        .Where(o => !o.Hide && o.OrderStateId == 2)
                        .OrderBy(o => o.DateTime)
                        .ToArray();
                    });
                    if(realized != null && realized.Length > 0)
                    {
                        foreach (var order in realized)
                        {
                            if (order.DateTime.Date == selected_date.SelectedDate)
                                orders.Add(order);
                        }
                    }
                }
                orders_table.ItemsSource = orders;
                App.CloseSystemPopUp();
                loading = false;
            }
        }

        private void View_order_button_Initialized(object sender, EventArgs e)
        {
            (sender as Button).Click += new RoutedEventHandler(EditData_Click);
        }

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


        private async void EditData_Click(object sender, RoutedEventArgs e)
        {
            /*App.ShowLoad();
            var data = ((FrameworkElement)sender).DataContext as SaleOrder;

            await App.GetSystemPopUp<CommitOrderPopUp>().Init(data);
            App.OpenSystemPopUp<CommitOrderPopUp>();*/
            var order = ((FrameworkElement)sender).DataContext as SaleOrder;
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
        }

        private void filter_combo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Init();
        }

        private async void RadComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var order = ((FrameworkElement)sender).DataContext as SaleOrder;
            var combo = sender as Telerik.Windows.Controls.RadComboBox;
            int selectedIndex = combo.SelectedIndex;
            if (order != null && selectedIndex != (((int)order.OrderStateId) - 1))
            {
                combo.Background = orderStateList[combo.SelectedIndex].GetColor();
                if (selectedIndex == 1)
                {
                    App.ShowLoad();
                    await App.GetSystemPopUp<CommitOrderPopUp>().Init(order);
                    App.OpenSystemPopUp<CommitOrderPopUp>();
                }
                else
                {
                    await App.RunAsync(() =>
                    {
                        var order_db = App.Database.SaleOrders.First(o => o.Id == order.Id);
                        var state = App.Database.OrderStates.First(s => s.Id == (selectedIndex + 1));
                        order_db.OrderState = state;
                        order_db.OrderStateId = state.Id;
                        App.Database.SaveChanges();
                    });
                }
            }
        }

        private async void view_confirm_order_button_Click(object sender, RoutedEventArgs e)
        {
            var order = ((FrameworkElement)sender).DataContext as SaleOrder;
            App.ShowLoad();
            await App.GetSystemPopUp<CommitOrderPopUp>().Init(order);
            App.OpenSystemPopUp<CommitOrderPopUp>();
        }

        private void selected_date_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Init();
        }

        public async void PrintOrder(SaleOrder order, CommandTemplate cmd)
        {
            App.ShowLoad();
            
            cmd.cmd_comany_name.Content = CompanyInformation.SelectedConfig.VisualName;
            cmd.cmd_order_date.Content = order.DateTime.ToString("dd/MM/yyyy HH:mm");
            cmd.cmd_order_name.Text = order.OrderName;
            cmd.cmd_order_number.Content = string.Format("{0:0000}", order.DailyId);
            cmd.cmd_order_obs.Text = order.Observation;
            cmd.cmd_order_phone.Text = order.PhoneNumber;

            var details = await App.RunAsync(() => App.Database.SaleOrderDetails.Where(d => d.SaleOrderId == order.Id).ToArray());
            cmd.cmd_detail_container.Children.Clear();
            decimal total = 0;
            foreach (var detail in details)
            {
                BillSaleDetailItem item = new BillSaleDetailItem();
                await item.SetSaleOrderDetail(detail);
                cmd.cmd_detail_container.Children.Add(item);
                total += detail.UnitValue * detail.Units;
            }
            cmd.cmd_total_order.Content = Math.Round(total, 2);

            PrintDialog pd = new PrintDialog();
            if ((pd.ShowDialog() == true))

            {
                PrintCapabilities capabilities = pd.PrintQueue.GetPrintCapabilities(pd.PrintTicket);
                pd.PrintTicket.PageMediaSize = new PageMediaSize(cmd.cmd_container.ActualWidth, cmd.cmd_container.ActualHeight);
                pd.PrintTicket.PageOrientation = System.Printing.PageOrientation.Portrait;
                pd.PrintVisual(GetImage(cmd.cmd_container, cmd.cmd_container.ActualWidth, cmd.cmd_container.ActualHeight), order.Id.ToString());
            }
            App.CloseSystemPopUp();
        }

        public DrawingVisual GetImage(UIElement source, double ancho, double alto)
        {
            double actualHeight = alto;
            double actualWidth = ancho;

            if (actualHeight > 0 && actualWidth > 0)
            {
                VisualBrush sourceBrush = new VisualBrush(source);

                DrawingVisual drawingVisual = new DrawingVisual();
                DrawingContext drawingContext = drawingVisual.RenderOpen();
                drawingContext.DrawRectangle(sourceBrush, null, new Rect(0, 0, actualWidth, actualHeight));
                drawingContext.Close();
                return drawingVisual;
            }
            else
                return null;
        }

        private void print_order_button_Click(object sender, RoutedEventArgs e)
        {
            var order = ((FrameworkElement)sender).DataContext as SaleOrder;
            PrintOrder(order, cmd);
        }

        private void RadComboBox_Initialized(object sender, EventArgs e)
        {
            var combo = sender as RadComboBox;
            List<RadComboBoxItem> list = new List<RadComboBoxItem>();
            foreach(var item in orderStateList)
            {
                RadComboBoxItem cbItem = new RadComboBoxItem()
                {
                    Content = item,
                    Background = item.GetColor()
                };
                list.Add(cbItem);
            }
            combo.ItemsSource = list;
            //combo.Background = orderStateList[(int)order.OrderStateId - 1].GetColor();
        }

        private void RadComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            var combo = sender as RadComboBox;
            var order = ((FrameworkElement)sender).DataContext as SaleOrder;
            combo.Background = orderStateList[(int)order.OrderStateId - 1].GetColor();
        }
    }
}
