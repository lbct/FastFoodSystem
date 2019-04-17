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
    /// Lógica de interacción para PurchaseViewerPopUp.xaml
    /// </summary>
    public partial class PurchaseViewerPopUp : SystemPopUpClass
    {
        private Purchase purchase;
        private PurchaseDetail[] purchaseDetails;
        private Action action;

        public PurchaseViewerPopUp()
        {
            InitializeComponent();
        }

        public async Task Init(Purchase purchase, Action action)
        {
            this.action = action;
            this.purchase = purchase;
            title.Content = string.Format("Compra - {0:0000}", purchase.Id);
            sale_datetime.Content = purchase.DateTime.ToString("dd/MM/yyyy - HH:mm");
            user_name.Content = await App.RunAsync(() =>
            {
                var login = App.Database.Logins.FirstOrDefault(l => l.Id == purchase.LoginId);
                var user = App.Database.Users.FirstOrDefault(u => u.Id == login.UserId);
                return user.FullName;
            });
            purchaseDetails = await App.RunAsync(() => App.Database.PurchaseDetails
            .Where(d => d.PurchaseId == purchase.Id)
            .ToArray());
            sale_detail_container.Children.Clear();
            double sumValue = 0;
            foreach (var detail in purchaseDetails)
            {
                var item = new Controls.PurchaseDetailItem()
                {
                    Margin = new Thickness(10)
                };
                await item.SetDetail(detail);
                sale_detail_container.Children.Add(item);
                var value = detail.Units * detail.UnitValue;
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
                        purchase.Hide = true;
                        foreach (var detail in purchaseDetails)
                            await DatabaseActions.ReduceProductUnits(detail);
                        App.ShowMessage("Compra Eliminada", true, () =>
                        {
                            App.GetSystemPage<PurchasePage>().Refresh();
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
                App.ShowMessage("Debe ser Administrador para eliminar una compra.", false, () =>
                {
                    App.OpenSystemPopUp<SaleViewerPopUp>();
                });
            }
        }
    }
}
