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
using System.Windows.Threading;

namespace FastFoodSystem.PopUps
{
    /// <summary>
    /// Lógica de interacción para SecurityPopUp.xaml
    /// </summary>
    public partial class SecurityPopUp : SystemPopUpClass
    {
        private Action<bool> action = res => { };
        private Action cancelAction = () => { };
        private User currentUser;

        public SecurityPopUp()
        {
            InitializeComponent();
        }

        public async Task Init(Action<bool> action, Action cancel)
        {
            cancelAction = cancel;
            password_text.Password = "";
            this.action = action;

            currentUser = await App.RunAsync(() => 
            {
                var login = App.Database.Logins.FirstOrDefault(log => log.Id == UserSession.LoginID);
                return App.Database.Users.FirstOrDefault(u => u.Id == login.UserId);
            });

            if (!currentUser.Admin)
            {
                App.CloseSystemPopUp();
                App.ShowMessage("No tiene los permisos suficientes para realizar esta acción.", false, () => action?.Invoke(false));
            }
            await this.Dispatcher.BeginInvoke(new Action(() =>
            {
                password_text.Focus();
            }), DispatcherPriority.ApplicationIdle);
        }

        private async void okButton_Click(object sender, RoutedEventArgs e)
        {
            App.CloseSystemPopUp();
            bool login = await UserSession.Login(currentUser.Username, password_text.Password);
            if (login)
            {
                action?.Invoke(true);
            }
            else
            {
                action?.Invoke(false);
            }
            password_text.Password = "";
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            App.CloseSystemPopUp();
            cancelAction?.Invoke();
        }
    }
}
