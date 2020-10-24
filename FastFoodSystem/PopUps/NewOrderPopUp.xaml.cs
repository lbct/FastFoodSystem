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
    /// Lógica de interacción para NewOrderPopUp.xaml
    /// </summary>
    public partial class NewOrderPopUp : SystemPopUpClass
    {
        private Action<string, string, string, OrderState> action;
        private Action actionCancel;

        public NewOrderPopUp()
        {
            InitializeComponent();
        }

        public async void Init(Action<string, string, string, OrderState> action, Action actionCancel, string name = "", string obs = "", string phone = "", OrderState orderState = null)
        {
            this.action = action;
            this.actionCancel = actionCancel;
            name_text.Text = name;
            observation_text.Text = obs;
            phone_text.Text = phone;
            var items = await App.RunAsync(() => App.Database.OrderStates.Where(o => o.Id != 2).ToArray());
            order_state_combo.ItemsSource = items;
            order_state_combo.SelectedItem = orderState != null ? items.First(i => i.Id == orderState.Id) : null;
        }

        private void Save_button_Click(object sender, RoutedEventArgs e)
        {
            App.ShowLoad();
            try
            {
                if (string.IsNullOrEmpty(name_text.Text.Trim()))
                    throw new Exception("Debe ingresar un nombre");
                if (string.IsNullOrEmpty(phone_text.Text.Trim()))
                    throw new Exception("Debe ingresar un teléfono");
                if (order_state_combo.SelectedItem == null)
                    throw new Exception("Debe seleccionar un estado");
                string name = name_text.Text.Trim();
                string obs = observation_text.Text.Trim();
                string phone = phone_text.Text.Trim();
                var state = order_state_combo.SelectedItem as OrderState;
                App.CloseSystemPopUp();
                action?.Invoke(name, obs, phone, state);
            }
            catch(Exception ex)
            {
                App.ShowMessage(ex.Message, false, () => App.OpenSystemPopUp<NewOrderPopUp>());
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            App.CloseSystemPopUp();
            actionCancel?.Invoke();
        }
    }
}
