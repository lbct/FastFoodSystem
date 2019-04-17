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
    /// Interaction logic for UsersPopUp.xaml
    /// </summary>
    public partial class UsersPopUp : SystemPopUpClass
    {
        public UsersPopUp()
        {
            InitializeComponent();
        }

        public async Task Init()
        {
            users_table.ItemsSource = await App.RunAsync(() => App.Database.Users.Where(su => !su.Hide)
            .ToArray());
        }

        private void Add_user_button_Click(object sender, RoutedEventArgs e)
        {
            App.OpenSystemPopUp<NewUserPopUp>().Init(async user =>
            {
                await App.OpenSystemPopUp<UsersPopUp>().Init();
            }, () => App.OpenSystemPopUp<UsersPopUp>());
        }

        private void Cancel_button_Click(object sender, RoutedEventArgs e)
        {
            App.CloseSystemPopUp();
        }

        private void Edit_user_button_Initialized(object sender, EventArgs e)
        {
            (sender as Button).Click += new RoutedEventHandler(EditData_Click);
        }

        private void EditData_Click(object sender, RoutedEventArgs e)
        {
            App.OpenSystemPopUp<LoadPopUp>();
            var data = ((FrameworkElement)sender).DataContext as User;
            App.OpenSystemPopUp<NewUserPopUp>().Init(data, async () => 
            {
                await App.OpenSystemPopUp<UsersPopUp>().Init();
            }, () => App.OpenSystemPopUp<UsersPopUp>());
        }
    }
}
