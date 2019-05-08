using FastFoodSystem.Scripts;
using FastFoodSystem.Scripts.Billing;
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
    /// Lógica de interacción para CodeControlGenPopUp.xaml
    /// </summary>
    public partial class CodeControlGenPopUp : SystemPopUpClass
    {
        public CodeControlGenPopUp()
        {
            InitializeComponent();
        }

        public void Init()
        {
            auth_number_text.Text = "";
            bill_number_value.Value = 1;
            client_nit_text.Text = "";
            bill_date.SelectedValue = DateTime.Now;
            total_sale_value.Value = 0;
            dosification_key_text.Text = "";
        }

        private async void Generate_button_Click(object sender, RoutedEventArgs e)
        {
            App.ShowLoad();
            try
            {
                string codeControl = await ControlCodeGenerator.GenerateAsync(
                    (int)bill_number_value.Value.Value,
                    client_nit_text.Text.Trim(),
                    bill_date.SelectedDate.Value,
                    total_sale_value.Value.Value,
                    dosification_key_text.Text.Trim(),
                    auth_number_text.Text.Trim()
                    );
                App.OpenSystemPopUp<CodeControlViewer>().Init(codeControl, () =>
                {
                    App.OpenSystemPopUp<CodeControlGenPopUp>();
                });
            }
            catch(Exception ex)
            {
                App.ShowMessage(ex.Message, false, () => App.OpenSystemPopUp<CodeControlGenPopUp>());
            }
        }

        private void Cancel_button_Click(object sender, RoutedEventArgs e)
        {
            App.CloseSystemPopUp();
        }
    }
}
