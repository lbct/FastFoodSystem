﻿using FastFoodSystem.Database;
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

namespace FastFoodSystem.Pages
{
    /// <summary>
    /// Interaction logic for SalePage.xaml
    /// </summary>
    public partial class SalePage : SystemPageClass
    {
        public SalePage()
        {
            InitializeComponent();
            start_date.SelectedValue = DateTime.Now;
            end_date.SelectedValue = DateTime.Now;
            start_date.SelectionChanged += Start_date_SelectionChanged;
            end_date.SelectionChanged += End_date_SelectionChanged;
        }

        private async void End_date_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (end_date.SelectedValue.Value.Date < start_date.SelectedValue.Value.Date)
                end_date.SelectedValue = start_date.SelectedValue.Value.Date;
            App.OpenSystemPopUp<LoadPopUp>();
            await RefreshTable();
            App.CloseSystemPopUp();
        }

        private async void Start_date_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (start_date.SelectedValue.Value.Date > end_date.SelectedValue.Value.Date)
                start_date.SelectedValue = end_date.SelectedValue.Value.Date;
            App.OpenSystemPopUp<LoadPopUp>();
            await RefreshTable();
            App.CloseSystemPopUp();
        }

        public override async void Refresh()
        {
            App.OpenSystemPopUp<LoadPopUp>();
            await RefreshTable();
            App.CloseSystemPopUp();
        }

        public async Task RefreshTable()
        {
            DateTime startValue = start_date.SelectedValue.Value.Date;
            DateTime endValue = end_date.SelectedValue.Value.Date.AddDays(1);
            var items = await App.RunAsync(() =>
            {
                return App.Database.GetSaleDetail(startValue, endValue).ToArray();
            });
            saleTable.ItemsSource = items;
            double totalValueSum = 0;
            double totalCostSum = 0;
            foreach (var item in items)
            {
                totalValueSum += double.Parse(item.TotalValue.ToString());
                totalCostSum += double.Parse(item.TotalCost.ToString());
            }

            total_sale_value.Value = totalValueSum;
            total_cost_value.Value = totalCostSum;
            
        }

        private void New_sale_button_Click(object sender, Telerik.Windows.RadRoutedEventArgs e)
        {
            App.OpenSystemPage<NewSalePage>();
        }

        private void Export_to_excel_button_Click(object sender, Telerik.Windows.RadRoutedEventArgs e)
        {

        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            App.OpenSystemPage<MenuPage>();
        }

        private void Edit_sale_button_Initialized(object sender, EventArgs e)
        {
            (sender as Button).Click += new RoutedEventHandler(EditData_Click);
        }

        private async void EditData_Click(object sender, RoutedEventArgs e)
        {
            App.ShowLoad();
            var data = ((FrameworkElement)sender).DataContext as GetSaleDetail_Result;
            var sale = await App.RunAsync(() => App.Database.Sales.First(s => s.Id == data.SaleId));
            await App.GetSystemPopUp<SaleViewerPopUp>().Init(sale, () => { });
            App.OpenSystemPopUp<SaleViewerPopUp>();
        }
    }
}
