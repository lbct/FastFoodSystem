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
    /// Interaction logic for NewProviderPopUp.xaml
    /// </summary>
    public partial class NewProviderPopUp : SystemPopUpClass
    {
        private Action<Provider> action;
        private Action actionCancel;
        private Provider provider;

        public NewProviderPopUp()
        {
            InitializeComponent();
        }

        public void Init(Action<Provider> action, Action actionCancel = null)
        {
            delete_button.Visibility = Visibility.Collapsed;
            title.Content = "Nuevo Proveedor";
            provider = null;
            name_text.Text = "";
            phone_number_text.Text = "";
            e_mail_text.Text = "";
            description_text.Document.Blocks.Clear();
            this.action = action;
            this.actionCancel = actionCancel;
        }

        public void Init(Provider provider, Action action, Action actionCancel = null)
        {
            delete_button.Visibility = Visibility.Visible;
            title.Content = "Información Proveedor";
            this.provider = provider;
            this.action = prov => action.Invoke();
            this.actionCancel = actionCancel;
            name_text.Text = provider.FullName;
            phone_number_text.Text = provider.PhoneNumber;
            e_mail_text.Text = provider.EMail;
            description_text.Document.Blocks.Clear();
            description_text.Document.Blocks.Add(new Paragraph(new Run(provider.Description)));
        }

        private async void Save_button_Click(object sender, RoutedEventArgs e)
        {
            App.OpenSystemPopUp<LoadPopUp>();
            string name = name_text.Text.Trim();
            string phone = phone_number_text.Text.Trim();
            string e_mail = e_mail_text.Text.Trim();
            string description = new TextRange(description_text.Document.ContentStart, description_text.Document.ContentEnd)
                .Text.Trim();
            if(string.IsNullOrEmpty(name) || string.IsNullOrEmpty(phone))
            {
                App.ShowMessage("Debe llenar todos los campos.", false, () => App.OpenSystemPopUp<NewProviderPopUp>());
            }
            else
            {
                Provider new_provider;
                if (provider == null)
                    new_provider = new Provider();
                else
                    new_provider = provider;
                new_provider.Hide = false;
                new_provider.Description = description;
                new_provider.EMail = e_mail;
                new_provider.PhoneNumber = phone;
                new_provider.FullName = name;
                await App.RunAsync(() => 
                {
                    if(provider == null)
                        App.Database.Providers.Add(new_provider);
                    App.Database.SaveChanges();
                });
                App.CloseSystemPopUp();
                action?.Invoke(new_provider);
            }
        }

        private void Cancel_button_Click(object sender, RoutedEventArgs e)
        {
            App.CloseSystemPopUp();
            actionCancel?.Invoke();
        }

        private void Delete_button_Click(object sender, RoutedEventArgs e)
        {
            App.OpenSystemPopUp<ConfirmPopUp>().Init("¿Desea eliminar el proveedor?", async () => 
            {
                App.OpenSystemPopUp<LoadPopUp>();
                await App.RunAsync(() => 
                {
                    provider.Hide = true;
                    App.Database.SaveChanges();
                });
                App.CloseSystemPopUp();
                action?.Invoke(null);
            }, () => App.OpenSystemPopUp<NewProviderPopUp>());
        }
    }
}
