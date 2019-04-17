﻿using FastFoodSystem.Controls;
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
using Telerik.Windows.Controls;

namespace FastFoodSystem.Pages
{
    /// <summary>
    /// Lógica de interacción para NewSalePage.xaml
    /// </summary>
    public partial class NewSalePage : SystemPageClass
    {
        public NewSalePage()
        {
            InitializeComponent();
        }

        public override async void Refresh()
        {
            App.ShowLoad();
            var login = await App.RunAsync(() => App.Database.Logins.FirstOrDefault(l => l.Id == UserSession.LoginID));
            var user = await App.RunAsync(() => App.Database.Users.FirstOrDefault(u => u.Id == login.UserId));
            if (user.Admin)
                backButton.Visibility = Visibility.Visible;
            else
                backButton.Visibility = Visibility.Collapsed;
            category_container.Items.Clear();
            detail_container.Children.Clear();
            detail_grid.Visibility = Visibility.Hidden;
            var categories = await App.RunAsync(() => App.Database.CategoryTypes.ToArray());
            foreach(var category in categories)
            {
                var tab = await GetNewCategoryItem(category);
                if(tab != null)
                    category_container.Items.Add(tab);
            }
            if (category_container.Items.Count > 0)
                category_container.SelectedIndex = 0;

            var orders = await App.RunAsync(() => App.Database.Orders
            .Where(o => !o.Committed)
            .Count());
            if (orders <= 0)
                order_alert_container.Visibility = Visibility.Collapsed;
            else
            {
                order_alert_container.Visibility = Visibility.Visible;
                order_count.Content = orders;
            }
            App.CloseSystemPopUp();
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
                        DiscountValue = item.Product.SaleDiscount,
                        ProductId = item.Product.Id,
                        UnitCost = await App.RunAsync(() => App.Database.GetProductCost(item.Product.Id)),
                        Units = (int)item.units_value.Value,
                        UnitValue = item.Product.SaleValue
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

        private async Task<RadTabItem> GetNewCategoryItem(CategoryType type)
        {
            var products = await App.RunAsync(() => App.Database.Products
            .Where(p => (p.CategoryTypeId == type.Id || type.Id == 1) && !p.HideForSales && !p.Hide).ToArray());
            WrapPanel content = new WrapPanel()
            {
                HorizontalAlignment = HorizontalAlignment.Center
            };
            foreach(var product in products)
            {
                var item = new VisualProduct()
                {
                    Margin = new Thickness(5)
                };
                await item.SetProductAsSale(product);
                item.button.Click += ProductButton_Click;
                content.Children.Add(item);
            }

            RadTabItem tab = new RadTabItem()
            {
                Header = type.Id == 1 ? "Todo" : type.Description,
                Content = new ScrollViewer()
                {
                    HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                    Content = content
                }
            };
            return products.Length == 0 ? null : tab;
        }

        private void ProductButton_Click(object sender, RoutedEventArgs e)
        {
            var product = (sender as RadButton).ParentOfType<VisualProduct>().Product;
            var sel_item = detail_container.Children.OfType<SaleDetailItem>()
                .FirstOrDefault(it => it.Product.Id == product.Id);
            if (sel_item == null)
            {
                var item = new SaleDetailItem()
                {
                    Margin = new Thickness(10)
                };
                item.SetProduct(product);
                item.remove_button.Click += RemoveDetailSaleButton_Click;
                item.remove_unit_button.Click += RemoveSaleUnitButton_Click;
                detail_container.Children.Add(item);
                detail_grid.Visibility = Visibility.Visible;
            }
            else
                sel_item.AddUnit();
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
    }
}
