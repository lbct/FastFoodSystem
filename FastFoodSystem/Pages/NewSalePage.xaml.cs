using FastFoodSystem.Controls;
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
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Telerik.Windows.Controls;

namespace FastFoodSystem.Pages
{
    /// <summary>
    /// Lógica de interacción para NewSalePage.xaml
    /// </summary>
    public partial class NewSalePage : SystemPageClass
    {
        private SaleOrder order;
        private Dictionary<int, WrapPanel> item_containers;

        public NewSalePage()
        {
            InitializeComponent();
            item_containers = new Dictionary<int, WrapPanel>();
        }

        public override async void Refresh()
        {
            App.ShowLoad();
            await RefreshAll();
            App.CloseSystemPopUp();
        }

        public async Task RefreshAll()
        {
            var login = await App.RunAsync(() => App.Database.Logins.FirstOrDefault(l => l.Id == UserSession.LoginID));
            var user = await App.RunAsync(() => App.Database.Users.FirstOrDefault(u => u.Id == login.UserId));
            if (user.Admin)
                backButton.Visibility = Visibility.Visible;
            else
                backButton.Visibility = Visibility.Collapsed;
            if(item_containers.Count <= 0)
                category_container.Items.Clear();
            detail_container.Children.Clear();

            detail_grid.Visibility = Visibility.Hidden;
            var categories = await App.RunAsync(() => App.Database.CategoryTypes.ToArray());
            var products = await App.RunAsync(() => App.Database.GetProductView());
            foreach (var category in categories)
            {
                var tab = GetNewCategoryItem(products, category);
                if (tab != null)
                    category_container.Items.Add(tab);
            }
            if (category_container.Items.Count > 0)
                category_container.SelectedIndex = 0;

            var orders = await App.RunAsync(() => App.Database.SaleOrders
            .Where(o => !o.Hide)
            .Count());
            if (orders <= 0)
                order_alert_container.Visibility = Visibility.Collapsed;
            else
            {
                order_alert_container.Visibility = Visibility.Visible;
                order_count.Content = orders;
            }
            if (order == null)
            {
                order_row.Height = new GridLength(0);
                order_buttons_container.Visibility = Visibility.Collapsed;
                sale_buttons_container.Visibility = Visibility.Visible;
            }
            else
            {
                await SetSaleOrder(order);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            App.OpenSystemPage<MenuPage>();
        }

        private async void Register_button_Click(object sender, RoutedEventArgs e)
        {
            var items = detail_container.Children.OfType<SaleDetailItem>().ToArray();
            if (items.Length > 0)
            {
                App.ShowLoad();
                var details = new List<SaleDetail>();
                foreach(var item in items)
                {
                    SaleDetail detail = new SaleDetail()
                    {
                        DiscountValue = item.ProductView.SaleDiscount,
                        ProductId = item.ProductView.Id,
                        UnitCost = item.ProductView.UnitCost.Value,
                        Units = (int)item.units_value.Value,
                        UnitValue = item.ProductView.UnitSaleValue
                    };
                    details.Add(detail);
                }
                await App.GetSystemPopUp<RegisterSalePopUp>().Init(details.ToArray());
                App.OpenSystemPopUp<RegisterSalePopUp>();
            }
            else
            {
                App.ShowMessage("Debe añadir por lo menos un detalle a la venta.", false);
            }
        }

        private RadTabItem GetNewCategoryItem(ProductView[] all_products, CategoryType type)
        {
            RadTabItem tab = null;
            var products = all_products.Where(p => (p.CategoryTypeId == type.Id || type.Id == 1) && !p.HideForSales).ToList();
                
                /*await App.RunAsync(() => App.Database.Products
            .Where(p => (p.CategoryTypeId == type.Id || type.Id == 1) && !p.HideForSales && !p.Hide).ToList());*/

            if (item_containers.ContainsKey(type.Id))
            {
                var container = item_containers[type.Id];
                var items = container.Children.OfType<VisualProduct>().ToList();
                foreach (var product in products)
                {
                    var item = items.FirstOrDefault(i => i.ProductView.Id == product.Id);
                    if(item != null)
                        item.SetProductAsSale(product);
                    else
                    {
                        item = new VisualProduct()
                        {
                            Margin = new Thickness(5)
                        };
                        item.SetProductAsSale(product);
                        item.button.Click += ProductButton_Click;
                        container.Children.Add(item);
                    }
                }
                if(products.Count != container.Children.Count)
                {
                    foreach(var item in items)
                    {
                        if (!products.Exists(p => p.Id == item.ProductView.Id))
                            container.Children.Remove(item);
                    }
                }
            }
            else
            {
                WrapPanel content = new WrapPanel()
                {
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                foreach (var product in products)
                {
                    var item = new VisualProduct()
                    {
                        Margin = new Thickness(5)
                    };
                    item.SetProductAsSale(product);
                    item.button.Click += ProductButton_Click;
                    content.Children.Add(item);
                }
                if(products.Count > 0)
                    item_containers.Add(type.Id, content);
                tab = new RadTabItem()
                {
                    Header = type.Id == 1 ? "Todo" : type.Description,
                    Content = new ScrollViewer()
                    {
                        HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                        Content = content
                    }
                };
            }
            return products.Count == 0 ? null : tab;
        }

        private void ProductButton_Click(object sender, RoutedEventArgs e)
        {
            var productView = (sender as RadButton).ParentOfType<VisualProduct>().ProductView;
            //var product = await App.RunAsync(())
            var sel_item = detail_container.Children.OfType<SaleDetailItem>()
                .FirstOrDefault(it => it.ProductView.Id == productView.Id);
            if (sel_item == null)
            {
                var item = new SaleDetailItem()
                {
                    Margin = new Thickness(10)
                };
                item.SetProduct(productView);
                item.remove_button.Click += RemoveDetailSaleButton_Click;
                item.remove_unit_button.Click += RemoveSaleUnitButton_Click;
                detail_container.Children.Add(item);
                detail_grid.Visibility = Visibility.Visible;
            }
            else
                sel_item.AddUnit();
            UpdateTotalValue();
        }

        public bool DetailContainerInUse
        {
            get
            {
                return detail_container.Children.Count > 0 || order != null;
            }
        }

        public async Task SetSaleOrder(SaleOrder order)
        {
            order_row.Height = new GridLength(48);
            sale_buttons_container.Visibility = Visibility.Collapsed;
            order_buttons_container.Visibility = Visibility.Visible;
            order_number_label.Content = order.DailyId;
            this.order = order;
            var details = await App.RunAsync(() =>
            {
                return App.Database.SaleOrderDetails.Where(d => d.SaleOrderId == order.Id).ToArray();
            });
            detail_container.Children.Clear();
            foreach (var detail in details)
            {
                var product = await App.RunAsync(() => App.Database.GetProductView(detail.ProductId)); /*await App.RunAsync(() =>
                App.Database.Products.FirstOrDefault(p => p.Id == detail.ProductId));*/
                var item = new SaleDetailItem()
                {
                    Margin = new Thickness(10)
                };
                item.SetProduct(product, detail.Units);
                item.remove_button.Click += RemoveDetailSaleButton_Click;
                item.remove_unit_button.Click += RemoveSaleUnitButton_Click;
                detail_container.Children.Add(item);
            }
            detail_grid.Visibility = Visibility.Visible;
            UpdateTotalValue();
        }

        private void RemoveSaleUnitButton_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as RadButton).ParentOfType<SaleDetailItem>();
            if (item.RemoveUnit())
            {
                detail_container.Children.Remove(item);
                if (detail_container.Children.Count <= 0)
                    detail_grid.Visibility = Visibility.Hidden;
            }
            UpdateTotalValue();
        }

        private void RemoveDetailSaleButton_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as RadButton).ParentOfType<SaleDetailItem>();
            detail_container.Children.Remove(item);
            if (detail_container.Children.Count <= 0)
                detail_grid.Visibility = Visibility.Hidden;
            UpdateTotalValue();
        }

        private void UpdateTotalValue()
        {
            if (detail_container.Children.Count > 0)
            {
                total_value.Value = detail_container.Children.OfType<SaleDetailItem>()
                    .Sum(d => d.total_value.Value.Value);
            }
            else
                total_value.Value = 0;
        }

        private async void Orders_button_Click(object sender, RoutedEventArgs e)
        {
            App.ShowLoad();
            await App.GetSystemPopUp<OrdersPopUp>().Init();
            App.OpenSystemPopUp<OrdersPopUp>();
        }

        private async void View_sales_button_Click(object sender, RoutedEventArgs e)
        {
            App.ShowLoad();
            await App.GetSystemPopUp<SessionSalesPopUp>().Init();
            App.OpenSystemPopUp<SessionSalesPopUp>();
        }

        private async void Save_order_button_Click(object sender, RoutedEventArgs e)
        {
            if(order != null)
            {
                var items = detail_container.Children.OfType<SaleDetailItem>().ToArray();
                if (items.Length > 0)
                {
                    App.ShowLoad();
                    var details = await App.RunAsync(() =>
                    {
                        return App.Database.SaleOrderDetails.Where(d => d.SaleOrderId == order.Id).ToArray();
                    });
                    foreach (var detail in details)
                    {
                        await App.RunAsync(() =>
                        {
                            App.Database.SaleOrderDetails.Remove(detail);
                        });
                    }
                    await App.RunAsync(() => App.Database.SaveChanges());
                    
                    foreach (var item in items)
                    {
                        SaleOrderDetail detail = new SaleOrderDetail()
                        {
                            DiscountValue = item.ProductView.SaleDiscount,
                            ProductId = item.ProductView.Id,
                            UnitCost = item.ProductView.UnitCost.Value,//await App.RunAsync(() => App.Database.GetProductCost(item.Product.Id)),
                            Units = (int)item.units_value.Value,
                            UnitValue = item.ProductView.UnitSaleValue,
                            SaleOrderId = order.Id
                        };
                        App.Database.SaleOrderDetails.Add(detail);
                        await DatabaseActions.ReduceProductUnits(detail);
                    }
                    await App.RunAsync(() => App.Database.SaveChanges());

                    App.OpenSystemPopUp<NewOrderPopUp>().Init(async (name, obs) => 
                    {
                        App.ShowLoad();
                        order.OrderName = name;
                        order.Observation = obs;
                        order.Hide = false;
                        await App.RunAsync(() => App.Database.SaveChanges());
                        order = null;
                        await RefreshAll();
                        App.CloseSystemPopUp();
                    }, () => { }, order.OrderName, order.Observation);
                }
                else
                {
                    App.ShowMessage("Debe añadir por lo menos un detalle al pedido.", false);
                }
            }
        }

        private void Delete_order_button_Click(object sender, RoutedEventArgs e)
        {
            if(order != null)
            {
                App.OpenSystemPopUp<ConfirmPopUp>().Init("¿Desea eliminar el pedido?", () => 
                {
                    order = null;
                    Refresh();
                });
            }
        }

        private async void Discard_order_button_Click(object sender, RoutedEventArgs e)
        {
            if(order != null)
            {
                App.ShowLoad();
                order.Hide = false;
                await App.RunAsync(() => App.Database.SaveChanges());
                App.CloseSystemPopUp();
                order = null;
                Refresh();
            }
        }
    }
}
