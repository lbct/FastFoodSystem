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

namespace FastFoodSystem
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

        }

        public async void RefreshTable()
        {
            var tableData = await Task.Factory.StartNew(() =>
            {
                var data = (from t in App.databaseEntities.Tables select t).ToArray();
                return data;
            });
            table.ItemsSource = tableData;
        }

        private async void Add_item_button_Click(object sender, RoutedEventArgs e)
        {
            var input = new InputBox();
            input.ShowDialog();
            string name = input.text.Text;
            input = new InputBox();
            input.ShowDialog();
            string type = input.text.Text;
            await Task.Factory.StartNew(() => 
            {
                Database.Table table = new Database.Table()
                {
                    Name = name,
                    Type = type
                };
                App.databaseEntities.Tables.Add(table);
                App.databaseEntities.SaveChanges();
            });
            MessageBox.Show("Saved "+App.databaseEntities.Database.Connection.State);
        }

        private void Refresh_button_Click(object sender, RoutedEventArgs e)
        {
            RefreshTable();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
        }
    }
}
