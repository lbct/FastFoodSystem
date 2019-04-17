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
        private Action<string, string> action;
        private Action actionCancel;

        public NewOrderPopUp()
        {
            InitializeComponent();
        }

        public void Init(Action<string, string> action, Action actionCancel)
        {
            this.action = action;
            this.actionCancel = actionCancel;
            name_text.Text = "";
            observation_text.Text = "";
        }

        private void Save_button_Click(object sender, RoutedEventArgs e)
        {
            App.ShowLoad();
            try
            {
                if (string.IsNullOrEmpty(name_text.Text.Trim()))
                    throw new Exception("Debe ingresar un nombre");
                string name = name_text.Text.Trim();
                string obs = observation_text.Text.Trim();
                App.CloseSystemPopUp();
                action?.Invoke(name, obs);
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
