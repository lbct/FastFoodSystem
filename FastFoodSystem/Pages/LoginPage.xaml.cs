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
using System.Windows.Threading;
using Telerik.Windows.Controls;

namespace FastFoodSystem.Pages
{
    /// <summary>
    /// Lógica de interacción para LoginPage.xaml
    /// </summary>
    public partial class LoginPage : SystemPageClass
    {
        public LoginPage()
        {
            InitializeComponent();
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                usernameText.Focus();
            }), DispatcherPriority.ApplicationIdle);

            databse_name_combo.ItemsSource = CompanyInformation.DatabaseConfigs;
            databse_name_combo.SelectedItem = CompanyInformation.SelectedConfig;
            App.DatabaseForeceUpdate();
            App.MainWin.Title = CompanyInformation.SelectedConfig.VisualName;

            if(App.MainWin.AuthWindowMessage != null)
            {
                databse_name_combo.Visibility = Visibility.Collapsed;
                login_panel.Visibility = Visibility.Collapsed;
                FastLogin(App.MainWin.AuthWindowMessage.Username
                    , App.MainWin.AuthWindowMessage.Password);
            }
        }

        private async void FastLogin(string username, string password)
        {
            App.ShowLoad();
            var login = await UserSession.Login(username, password);
            if (login)
                OpenMainMenu();
        }

        public override void Refresh()
        {
            usernameText.Text = "";
            passwordText.Password = "";
        }

        private async void loginButton_Click(object sender, RoutedEventArgs e)
        {
            App.ShowLoad();
            var login = await UserSession.Login(usernameText.Text, passwordText.Password);
            if (login)
                App.ShowMessage("Sesión iniciada correctamente.", true, OpenMainMenu);
            else
                App.ShowMessage("Nombre de usuario o contraseña incorrecta.", false);
        }

        private async void OpenMainMenu()
        {
            App.ShowLoad();
            var login = await App.RunAsync(() => App.Database.Logins.FirstOrDefault(l => l.Id == UserSession.LoginID));
            var user = await App.RunAsync(() => App.Database.Users.FirstOrDefault(u => u.Id == login.UserId));
            App.CloseSystemPopUp();
            if (user.Admin)
                App.OpenSystemPage<MenuPage>();
            else
                App.OpenSystemPage<NewSalePage>();
        }

        private void databse_name_combo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(databse_name_combo.SelectedItem != null)
            {
                var config = databse_name_combo.SelectedItem as DatabaseConfig;
                CompanyInformation.SelectedConfig = config;
                DatabaseSettings.Configure(CompanyInformation.SelectedConfig.Database);
                App.DatabaseForeceUpdate();
                App.MainWin.Title = CompanyInformation.SelectedConfig.VisualName;
            }
        }
    }
}
