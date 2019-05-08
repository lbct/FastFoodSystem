using FastFoodSystem.Database;
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

namespace FastFoodSystem.Controls
{
    /// <summary>
    /// Lógica de interacción para BillSaleDetailItem.xaml
    /// </summary>
    public partial class BillSaleDetailItem : UserControl
    {
        public BillSaleDetailItem()
        {
            InitializeComponent();
        }

        public async Task SetSaleDetail(SaleDetail detail)
        {
            var product = await App.RunAsync(() => App.Database.Products
            .FirstOrDefault(p => p.Id == detail.ProductId));
            product_name_label.Content = product.Description;
            units_label.Content = detail.Units;
            unit_price_label.Content = Math.Round(detail.UnitValue, 2);
            total_label.Content = Math.Round(detail.UnitValue * detail.Units, 2);
        }
    }
}
