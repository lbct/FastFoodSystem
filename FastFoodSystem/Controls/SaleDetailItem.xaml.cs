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
        public ProductView ProductView { get; private set; }

        public bool ShowButtons
        {
            get { return buttons_container.Visibility == Visibility.Visible; }
            set
            {
                if (value)
                    buttons_container.Visibility = Visibility.Visible;
                else
                    buttons_container.Visibility = Visibility.Collapsed;
            }
        }

        public SaleDetailItem()
        {
            InitializeComponent();
        }

        public void SetProduct(ProductView product, int units = 1)
        {
            ProductView = product;
            product_description.Content = product.Description;
            units_value.Value = units;
            sale_unit_value.Value = double.Parse((product.UnitSaleValue * (1.0m - (product.SaleDiscount / 100.0m))).ToString());
            total_value.Value = units * double.Parse((product.UnitSaleValue * (1.0m - (product.SaleDiscount / 100.0m))).ToString());
        }

        public async Task SetDetail(SaleDetail detail)
        {
            ProductView = await App.RunAsync(() => App.Database.GetProductView(detail.ProductId));
            product_description.Content = ProductView.Description;
            units_value.Value = detail.Units;
            sale_unit_value.Value = double.Parse((detail.UnitValue * (1.0m - (detail.DiscountValue / 100.0m))).ToString());
            total_value.Value = double.Parse((detail.UnitValue * (1.0m - (detail.DiscountValue / 100.0m))).ToString()) * detail.Units;
        }

        public async Task SetOrderDetail(SaleOrderDetail detail)
        {
            ProductView = await App.RunAsync(() => App.Database.GetProductView(detail.ProductId));
            //Product = await App.RunAsync(() => App.Database.Products.FirstOrDefault(p => p.Id == detail.ProductId));
            product_description.Content = ProductView.Description;
            units_value.Value = detail.Units;
            sale_unit_value.Value = double.Parse((detail.UnitValue * (1.0m - (detail.DiscountValue / 100.0m))).ToString());
            total_value.Value = double.Parse((detail.UnitValue * (1.0m - (detail.DiscountValue / 100.0m))).ToString()) * detail.Units;
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
