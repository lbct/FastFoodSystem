using FastFoodSystem.Controls;
using FastFoodSystem.Database;
using FastFoodSystem.Pages;
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
    /// Lógica de interacción para SaleViewerPopUp.xaml
    /// </summary>
    public partial class SaleViewerPopUp : SystemPopUpClass
    {
        private Sale sale;
        private SaleDetail[] saleDetails;
        private Action action;

        public SaleViewerPopUp()
        {
            InitializeComponent();
        }

        public async Task Init(Sale sale, Action action)
        {
            this.action = action;
            this.sale = sale;
            title.Content = string.Format("Venta - {0:0000}", sale.Id);
            var client = await App.RunAsync(() => App.Database.Clients.FirstOrDefault(c => c.Id == sale.ClientId));
            client_name.Content = client.Name;
            client_nit.Content = client.Nit;
            sale_datetime.Content = sale.DateTime.ToString("dd/MM/yyyy - HH:mm");
            user_name.Content = await App.RunAsync(() => 
            {
                var login = App.Database.Logins.FirstOrDefault(l => l.Id == sale.LoginId);
                var user = App.Database.Users.FirstOrDefault(u => u.Id == login.UserId);
                return user.FullName;
            });
            saleDetails = await App.RunAsync(() => App.Database.SaleDetails
            .Where(d => d.SaleId == sale.Id)
            .ToArray());
            sale_detail_container.Children.Clear();
            double sumValue = 0;
            foreach(var detail in saleDetails)
            {
                var item = new SaleDetailItem()
                {
                    Margin = new Thickness(10),
                    ShowButtons = false
                };
                await item.SetDetail(detail);
                sale_detail_container.Children.Add(item);
                var value = detail.Units * detail.UnitValue * (1m - (detail.DiscountValue / 100m));
                sumValue += double.Parse(value.ToString());
            }
            total_value.Value = sumValue;
        }

        private void Cancel_button_Click(object sender, RoutedEventArgs e)
        {
            App.CloseSystemPopUp();
            action?.Invoke();
        }

        private async void Delete_sale_button_Click(object sender, RoutedEventArgs e)
        {
            App.ShowLoad();
            var login = await App.RunAsync(() => App.Database.Logins.FirstOrDefault(l => l.Id == UserSession.LoginID));
            var user = await App.RunAsync(() => App.Database.Users.FirstOrDefault(u => u.Id == login.UserId));
            if (user.Admin)
            {
                await App.GetSystemPopUp<SecurityPopUp>().Init(async isLogin => 
                {
                    App.ShowLoad();
                    if (isLogin)
                    {
                        sale.Hide = true;
                        foreach(var detail in saleDetails)
                            await DatabaseActions.IncreaseProductUnits(detail);
                        App.ShowMessage("Venta Eliminada", true, () =>
                        {
                            App.GetSystemPage<SalePage>().Refresh();
                            App.GetSystemPage<NewSalePage>().Refresh();
                        });
                    }
                    else
                    {
                        App.ShowMessage("Nombre de usuario o contraseña Incorrecta", false, () =>
                        App.OpenSystemPopUp<SaleViewerPopUp>());
                    }
                }, () => 
                {
                    App.OpenSystemPopUp<SaleViewerPopUp>();
                });
                App.OpenSystemPopUp<SecurityPopUp>();
            }
            else
            {
                App.ShowMessage("Debe ser Administrador para eliminar una venta.", false, () =>
                {
                    App.OpenSystemPopUp<SaleViewerPopUp>();
                });
            }
        }
    }
}
