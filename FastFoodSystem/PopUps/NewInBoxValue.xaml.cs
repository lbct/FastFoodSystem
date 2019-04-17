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
    /// Interaction logic for NewInBoxValue.xaml
    /// </summary>
    public partial class NewInBoxValue : SystemPopUpClass
    {
        public Action<CashMovement> action;
        public Action actionCancel;

        public NewInBoxValue()
        {
            InitializeComponent();
        }

        public void Init(Action<CashMovement> action, Action actionCancel)
        {
            this.action = action;
            this.actionCancel = actionCancel;
            total_value.Value = 0;
            description_text.Text = "";
        }

        private async void OkButton_Click(object sender, RoutedEventArgs e)
        {
            App.OpenSystemPopUp<LoadPopUp>();
            CashMovement cash = new CashMovement()
            {
                DateTime = DateTime.Now,
                Description = description_text.Text,
                LoginId = UserSession.LoginID,
                Value = decimal.Parse(total_value.Value.Value.ToString()),
                Hide = false
            };
            await App.RunAsync(() => 
            {
                App.Database.CashMovements.Add(cash);
                App.Database.SaveChanges();
            });
            App.CloseSystemPopUp();
            action?.Invoke(cash);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            App.CloseSystemPopUp();
            actionCancel?.Invoke();
        }
    }
}
