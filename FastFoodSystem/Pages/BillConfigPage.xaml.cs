using FastFoodSystem.Database;
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

namespace FastFoodSystem.Pages
{
    /// <summary>
    /// Lógica de interacción para BillConfigPage.xaml
    /// </summary>
    public partial class BillConfigPage : SystemPageClass
    {
        private BillConfig billConfig;

        public BillConfigPage()
        {
            InitializeComponent();
        }

        public override async void Refresh()
        {
            App.ShowLoad();
            billConfig = await UserSession.GetBillConfig();
            company_name_text.Text = CompanyInformation.CompanyName;
            direction_text.Document.Blocks.Clear();
            direction_text.Document.Blocks.Add(new Paragraph(new Run(CompanyInformation.Direction)));
            economic_activity_text.Text = CompanyInformation.EconomicActivity;
            consumer_law_legend_text.Document.Blocks.Clear();
            consumer_law_legend_text.Document.Blocks.Add(new Paragraph(new Run(CompanyInformation.ConsumerLawLegend)));
            phone_number_text.Text = CompanyInformation.PhoneNumber;
            e_mail_text.Text = CompanyInformation.EMail;
            company_nit_text.Text = CompanyInformation.CompanyNit;

            auth_number_text.Text = billConfig.AuthorizationCode;
            dosification_key_text.Text = billConfig.DosificationCode;
            start_bill_number_value.Value = billConfig.CurrentBillNumber;
            bill_limit_emission_date.SelectedValue = billConfig.LimitEmissionDate;
            App.CloseSystemPopUp();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            App.OpenSystemPopUp<ConfirmPopUp>().Init("No se guardarán las modificaciones, ¿Desea continuar?", () => 
            {
                App.OpenSystemPage<MenuPage>();
            });
        }

        private async void Save_button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                App.ShowLoad();
                if (string.IsNullOrEmpty(company_name_text.Text.Trim()))
                    throw new Exception("Debe añadir el nombre de su empresa");
                if (string.IsNullOrEmpty(economic_activity_text.Text.Trim()))
                    throw new Exception("Debe añadir una actividad económica");
                if (company_nit_text.Text.Trim().Length < 5 || !company_nit_text.Text.Trim().All(char.IsDigit))
                    throw new Exception("Debe añadir un NIT válido");
                if (auth_number_text.Text.Trim().Length < 5 || !auth_number_text.Text.Trim().All(char.IsDigit))
                    throw new Exception("Debe añadir un número de autorización válido");
                if (dosification_key_text.Text.Trim().Length != 64)
                    throw new Exception("Debe añadir una llave de dosificación válida");
                if (bill_limit_emission_date.SelectedDate == null || bill_limit_emission_date.SelectedDate.Value < DateTime.Now.Date)
                    throw new Exception("Debe añadir una Fecha límite de emisión válida");
                CompanyInformation.CompanyName = company_name_text.Text.Trim();
                CompanyInformation.Direction = new TextRange(
                    direction_text.Document.ContentStart,
                    direction_text.Document.ContentEnd)
                    .Text.Trim();
                CompanyInformation.EconomicActivity = economic_activity_text.Text.Trim();
                CompanyInformation.ConsumerLawLegend = new TextRange(
                    consumer_law_legend_text.Document.ContentStart,
                    consumer_law_legend_text.Document.ContentEnd)
                    .Text.Trim();
                CompanyInformation.PhoneNumber = phone_number_text.Text.Trim();
                CompanyInformation.EMail = e_mail_text.Text.Trim();
                CompanyInformation.CompanyNit = company_nit_text.Text.Trim();

                billConfig.AuthorizationCode = auth_number_text.Text.Trim();
                billConfig.CurrentBillNumber = (int)start_bill_number_value.Value.Value;
                billConfig.DosificationCode = dosification_key_text.Text.Trim();
                billConfig.LimitEmissionDate = bill_limit_emission_date.SelectedDate.Value;
                await CompanyInformation.Save();
                await App.RunAsync(() => App.Database.SaveChanges());
                App.ShowMessage("Información guardada correctamente");
            }
            catch(Exception ex)
            {
                App.ShowMessage(ex.Message, false);
            }
        }

        private void Control_code_gen_button_Click(object sender, RoutedEventArgs e)
        {
            App.OpenSystemPopUp<CodeControlGenPopUp>().Init();
        }

        private void Bill_viewer_button_Click(object sender, RoutedEventArgs e)
        {
            App.OpenSystemPage<BillViewerPage>();
        }
    }
}
