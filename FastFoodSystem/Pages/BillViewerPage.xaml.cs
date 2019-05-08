using FastFoodSystem.Controls;
using FastFoodSystem.Database;
using FastFoodSystem.Scripts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Printing;
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
    /// Lógica de interacción para BillViewerPage.xaml
    /// </summary>
    public partial class BillViewerPage : SystemPageClass
    {
        public BillViewerPage()
        {
            InitializeComponent();
        }

        public async Task Init(Sale sale)
        {
            var billConfig = await UserSession.GetBillConfig();
            var client = await App.RunAsync(() => App.Database.Clients
            .FirstOrDefault(c => c.Id == sale.ClientId));
            company_name_label.Content = CompanyInformation.CompanyName;
            company_nit_label.Content = CompanyInformation.CompanyNit;
            bill_number_label.Content = sale.BillNumber;
            auth_number_label.Content = billConfig.AuthorizationCode;
            sale_date_label.Content = sale.DateTime.ToString("dd/MM/yyyy");
            client_name_label.Content = client.Name;
            client_nit_label.Content = client.Nit;
            bill_detail_container.Children.Clear();
            var details = await App.RunAsync(() => App.Database.SaleDetails
            .Where(d => d.SaleId == sale.Id)
            .ToArray());
            foreach(var detail in details)
            {
                var item = new BillSaleDetailItem();
                await item.SetSaleDetail(detail);
                bill_detail_container.Children.Add(item);
            }
            var sub_total = Math.Round(details.Sum(d => d.Units * d.UnitValue), 2);
            var discount = Math.Round(details.Sum(d => d.Units * d.UnitValue * (d.DiscountValue / 100.0m)), 2);
            var total = sub_total - discount;
            total_without_discount_label.Content = sub_total;
            total_discount.Content = discount;
            total_sale_label.Content = total;
            int decimals = (int)((total - ((int)total)) * 100m);
            total_sale_literal_label.Content = ToLiteral((int)total) + " " + decimals + "/100";
            code_control_label.Content = sale.ControlCode;
            limit_date_emission_label.Content = billConfig.LimitEmissionDate.ToString("dd/MM/yyyy");
            code_control_qr.Text = sale.ControlCode;
            consumer_law_legend_text.Text = CompanyInformation.ConsumerLawLegend;
        }

        private static string ToLiteral(int value)
        {
            string number = "";
            if (value < 10)
            {

                switch (value)
                {
                    case 1: number = "UNO"; break;
                    case 2: number = "DOS"; break;
                    case 3: number = "TRES"; break;
                    case 4: number = "CUATRO"; break;
                    case 5: number = "CINCO"; break;
                    case 6: number = "SEIS"; break;
                    case 7: number = "SIETE"; break;
                    case 8: number = "OCHO"; break;
                    case 9: number = "NUEVE"; break;
                }
            }
            else if (value < 100)
            {
                int last = value % 10;
                value -= last;
                switch (value)
                {
                    case 10:
                        if (last == 0)
                            number = "DIEZ";
                        else if (last == 1)
                            number = "ONCE";
                        else if (last == 2)
                            number = "DOCE";
                        else if (last == 3)
                            number = "TRECE";
                        else if (last == 4)
                            number = "CATORCE";
                        else if (last == 5)
                            number = "QUINCE";
                        else if (last >= 6)
                        {
                            number = "DIECI" + ToLiteral(last);
                        }
                        break;
                    case 20:
                        if (last == 0)
                            number = "VEINTE";
                        else
                            number = "VEINTI" + ToLiteral(last);
                        break;
                    case 30:
                        number = "TREINTA";
                        if (last > 0)
                            number += "I" + ToLiteral(last);
                        break;
                    case 40:
                        number = "CUARENTA";
                        if (last > 0)
                            number += "I" + ToLiteral(last);
                        break;
                    case 50:
                        number = "CINCUENTA";
                        if (last > 0)
                            number += "I" + ToLiteral(last);
                        break;
                    case 60:
                        number = "SESENTA";
                        if (last > 0)
                            number += "I" + ToLiteral(last);
                        break;
                    case 70:
                        number = "SETENTA";
                        if (last > 0)
                            number += "I" + ToLiteral(last);
                        break;
                    case 80:
                        number = "OCHENTA";
                        if (last > 0)
                            number += "I" + ToLiteral(last);
                        break;
                    case 90:
                        number = "NOVENTA";
                        if (last > 0)
                            number += "I" + ToLiteral(last);
                        break;
                }
            }
            else if (value < 1000)
            {
                int last = value % 100;
                value -= last;
                int first = value / 100;
                if (first != 5 && first != 7 && first != 9)
                {
                    if (first > 1)
                        number += ToLiteral(first);
                    number += "CIEN";
                }
                else
                {
                    switch (first)
                    {
                        case 5: number = "QUINIEN"; break;
                        case 7: number = "SETECIEN"; break;
                        case 9: number = "NOVECIEN"; break;
                    }
                }
                number += (first > 1 || last > 0 ? "TO" : "")
                    + ((first > 1) ? "S" : "")
                    + (last > 0 ? " " + ToLiteral(last) : "");
            }
            else if (value < 10000)
            {
                int last = value % 1000;
                value -= last;
                int first = value / 1000;
                if (first > 1)
                    number += ToLiteral(first) + " ";
                number += "MIL" + (last > 0 ? " " + ToLiteral(last) : "");
            }
            else
                number = value.ToString();
            return number;
        }

        public override void Refresh()
        {
        }

        private void Back_button_Click(object sender, RoutedEventArgs e)
        {
            App.OpenSystemPage<BillConfigPage>();
        }

        private void Print_button_Click(object sender, RoutedEventArgs e)
        {
            App.ShowLoad();
            PrintDialog pd = new PrintDialog();
            if ((pd.ShowDialog() == true))

            {
                PrintCapabilities capabilities = pd.PrintQueue.GetPrintCapabilities(pd.PrintTicket);
                //Size pageSize = new Size(pd.PrintableAreaWidth, pd.PrintableAreaHeight);
                //Size visibleSize = new Size(capabilities.PageImageableArea.ExtentWidth, capabilities.PageImageableArea.ExtentHeight);
                pd.PrintTicket.PageMediaSize = new PageMediaSize(bill_container.ActualWidth, bill_container.ActualHeight);
                pd.PrintTicket.PageOrientation = System.Printing.PageOrientation.Portrait;
                pd.PrintVisual(GetImage(bill_container, bill_container.ActualWidth, bill_container.ActualHeight), code_control_label.Content.ToString());
            }
            App.CloseSystemPopUp();
        }

        public DrawingVisual GetImage(UIElement source, double ancho, double alto)
        {
            double actualHeight = alto;
            double actualWidth = ancho;

            if (actualHeight > 0 && actualWidth > 0)
            {
                //RenderTargetBitmap renderTarget = new RenderTargetBitmap((int)actualWidth, (int)actualHeight, 96, 96, PixelFormats.Pbgra32);
                VisualBrush sourceBrush = new VisualBrush(source);

                DrawingVisual drawingVisual = new DrawingVisual();
                DrawingContext drawingContext = drawingVisual.RenderOpen();
                drawingContext.DrawRectangle(sourceBrush, null, new Rect(0, 0, actualWidth, actualHeight));
                drawingContext.Close();

                //renderTarget.Render(drawingVisual);
                return drawingVisual;
            }
            else
                return null;
        }
    }
}
