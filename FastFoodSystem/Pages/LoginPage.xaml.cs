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
        }

        public void Init()
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

        private void OpenMainMenu()
        {
            App.OpenSystemPage<MenuPage>();
        }
    }
}
