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
    /// Lógica de interacción para OrdersPopUp.xaml
    /// </summary>
    public partial class OrdersPopUp : SystemPopUpClass
    {
        public OrdersPopUp()
        {
            InitializeComponent();
        }
        
        public async Task Init()
        {
            orders_table.ItemsSource = await App.RunAsync(() => 
            {
                return App.Database.SaleOrders
                .Where(o => !o.Hide)
                .ToArray();
            });
        }

        private void Cancel_button_Click(object sender, RoutedEventArgs e)
        {
            App.CloseSystemPopUp();
        }

        private void View_order_button_Initialized(object sender, EventArgs e)
        {
            (sender as Button).Click += new RoutedEventHandler(EditData_Click);
        }

        private async void EditData_Click(object sender, RoutedEventArgs e)
        {
            App.ShowLoad();
            var data = ((FrameworkElement)sender).DataContext as SaleOrder;
            
            await App.GetSystemPopUp<CommitOrderPopUp>().Init(data);
            App.OpenSystemPopUp<CommitOrderPopUp>();
        }
    }
}
