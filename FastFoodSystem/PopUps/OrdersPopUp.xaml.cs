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
                var orders = App.Database.Orders
                .Where(o => !o.Committed)
                .ToArray();
                return orders;
            });
        }

        private void Commit_order_button_Initialized(object sender, EventArgs e)
        {

        }

        private void Delete_order_button_Initialized(object sender, EventArgs e)
        {

        }

        private void Cancel_button_Click(object sender, RoutedEventArgs e)
        {
            App.CloseSystemPopUp();
        }
    }
}
