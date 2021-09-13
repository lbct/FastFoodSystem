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
using Telerik.Windows.Controls;

namespace FastFoodSystem.PopUps
{
    /// <summary>
    /// Lógica de interacción para NewOrderPopUp.xaml
    /// </summary>
    public partial class NewOrderPopUp : SystemPopUpClass
    {
        private Action<int, string, string, string, OrderState> action;
        private Action actionCancel;

        public NewOrderPopUp()
        {
            InitializeComponent();
        }

        public async void Init(Action<int, string, string, string, OrderState> action, Action actionCancel, int orderNumber, string name = "", string obs = "", string phone = "", OrderState orderState = null)
        {
            this.action = action;
            this.actionCancel = actionCancel;
            name_text.Text = name;
            observation_text.Text = obs;
            phone_text.Text = phone;
            order_number.Value = orderNumber;
            var items = await App.RunAsync(() => App.Database.OrderStates.ToArray());
            var cbItems = new List<RadComboBoxItem>();
            foreach(var item in items)
            {
                var cbItem = new RadComboBoxItem()
                {
                    Content = item,
                    Background = item.GetColor()
                };
                cbItems.Add(cbItem);
            }

            order_state_combo.ItemsSource = cbItems;
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
                if (order_number.Value == null)
                    throw new Exception("Debe especificar un número de orden");
                string name = name_text.Text.Trim();
                string obs = observation_text.Text.Trim();
                string phone = phone_text.Text.Trim();
                var state = (order_state_combo.SelectedItem as RadComboBoxItem).Content as OrderState;
                App.CloseSystemPopUp();
                action?.Invoke((int)order_number.Value.Value, name, obs, phone, state);
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
