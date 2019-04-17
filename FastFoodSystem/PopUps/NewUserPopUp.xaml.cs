using FastFoodSystem.Database;
using FastFoodSystem.Scripts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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
    /// Interaction logic for NewUserPopUp.xaml
    /// </summary>
    public partial class NewUserPopUp : SystemPopUpClass
    {
        private Action<User> action;
        private Action actionCancel;
        private User user_edit;

        public NewUserPopUp()
        {
            InitializeComponent();
        }

        public void Init(Action<User> action, Action actionCancel = null)
        {
            user_edit = null;
            delete_button.Visibility = Visibility.Collapsed;
            permission_combobox.IsEnabled = true;
            this.action = action;
            username_text.IsEnabled = true;
            this.actionCancel = actionCancel;
            username_text.Text = "";
            fullname_text.Text = "";
            password_text.Password = "";
            permission_combobox.SelectedItem = null;
            permission_combobox.ItemsSource = new string[]
            {
                "Local",
                "Administrador"
            };
        }

        public async void Init(User user, Action action, Action actionCancel = null)
        {
            user_edit = user;
            username_text.IsEnabled = false;
            delete_button.Visibility = Visibility.Visible;

            var login = await App.RunAsync(() => App.Database.Logins.FirstOrDefault(l => l.Id == UserSession.LoginID));

            if (login.UserId == user.Id)
            {
                delete_button.IsEnabled = false;
                permission_combobox.IsEnabled = false;
            }
            else
            {
                delete_button.IsEnabled = true;
                permission_combobox.IsEnabled = true;
            }
            this.action = u => action.Invoke();
            this.actionCancel = actionCancel;
            username_text.Text = user.Username;
            fullname_text.Text = user.FullName;
            password_text.Password = "";
            permission_combobox.ItemsSource = new string[]
            {
                "Local",
                "Administrador"
            };
            permission_combobox.SelectedIndex = user.Admin ? 1 : 0;
        }

        private async void Save_button_Click(object sender, RoutedEventArgs e)
        {
            App.OpenSystemPopUp<LoadPopUp>();
            if (!string.IsNullOrEmpty(username_text.Text.Trim())
                && !string.IsNullOrEmpty(fullname_text.Text.Trim())
                && ((!string.IsNullOrEmpty(password_text.Password) && user_edit == null) || user_edit != null)
                && permission_combobox.SelectedItem != null)
            {
                string username = username_text.Text.Trim();
                string fullname = fullname_text.Text.Trim();
                bool exist = false;
                if (user_edit == null)
                {
                    exist = await App.RunAsync(() =>
                    {
                        return App.Database.Users.Count(u => u.Username.ToLower().Equals(username.ToLower())) > 0;
                    });
                }
                if (exist)
                    App.ShowMessage("Ya se registró un usuario con el mismo nombre", false, () =>
                    App.OpenSystemPopUp<NewUserPopUp>());
                else
                {
                    User user = null;
                    if (user_edit != null)
                        user = user_edit;
                    else
                        user = new User();
                    using (MD5 md5Hash = MD5.Create())
                    {
                        user.Hide = false;
                        if(!string.IsNullOrEmpty(password_text.Text))
                            user.Password = UserSession.GetMd5Hash(md5Hash, password_text.Password);
                        user.Admin = permission_combobox.SelectedIndex == 1;
                        user.Username = username;
                        user.FullName = fullname;
                        await App.RunAsync(() => 
                        {
                            if(user_edit == null)
                                App.Database.Users.Add(user);
                            App.Database.SaveChanges();
                        });
                    }
                    App.CloseSystemPopUp();
                    action?.Invoke(user);
                }
            }
            else
                App.ShowMessage("Debe llenar todos los campos.", false, () => App.OpenSystemPopUp<NewUserPopUp>());
        }

        private void Cancel_button_Click(object sender, RoutedEventArgs e)
        {
            App.CloseSystemPopUp();
            actionCancel?.Invoke();
        }

        private void Delete_button_Click(object sender, RoutedEventArgs e)
        {
            App.OpenSystemPopUp<ConfirmPopUp>().Init("¿Desea eliminar el usuario?", async () => 
            {
                App.OpenSystemPopUp<LoadPopUp>();
                user_edit.Hide = true;
                await App.RunAsync(() => App.Database.SaveChanges());
                App.CloseSystemPopUp();
                action?.Invoke(null);
            }, () => App.OpenSystemPopUp<NewUserPopUp>());
        }
    }
}
