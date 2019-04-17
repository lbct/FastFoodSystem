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
    /// Lógica de interacción para PurchaseDetailItem.xaml
    /// </summary>
    public partial class PurchaseDetailItem : UserControl
    {
        public PurchaseDetailItem()
        {
            InitializeComponent();
        }

        public async Task SetDetail(PurchaseDetail detail)
        {
            var product = await App.RunAsync(() => App.Database.Products.FirstOrDefault(p => p.Id == detail.ProductId));
            product_description.Content = product.Description;
            units_value.Value = detail.Units;
            purchase_unit_value.Value = double.Parse(detail.UnitValue.ToString());
            total_value.Value = double.Parse((detail.UnitValue * detail.Units).ToString());
        }
    }
}
