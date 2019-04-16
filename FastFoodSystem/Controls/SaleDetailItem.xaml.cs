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
    /// Lógica de interacción para SaleDetailItem.xaml
    /// </summary>
    public partial class SaleDetailItem : UserControl
    {
        public Product Product { get; private set; }

        public SaleDetailItem()
        {
            InitializeComponent();
        }

        public void SetProduct(Product product)
        {
            Product = product;
            product_description.Content = product.Description;
            units_value.Value = 1;
            sale_unit_value.Value = double.Parse((product.SaleValue * (1.0m - (product.SaleDiscount / 100.0m))).ToString());
            total_value.Value = double.Parse((product.SaleValue * (1.0m - (product.SaleDiscount / 100.0m))).ToString());
        }

        public void AddUnit()
        {
            units_value.Value++;
            total_value.Value = units_value.Value.Value * sale_unit_value.Value.Value;
        }

        public bool RemoveUnit()
        {
            units_value.Value--;
            total_value.Value = units_value.Value.Value * sale_unit_value.Value.Value;
            return units_value.Value.Value <= 0;
        }
    }
}
