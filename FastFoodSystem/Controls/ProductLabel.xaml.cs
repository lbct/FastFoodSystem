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
using Telerik.Windows.Controls;

namespace FastFoodSystem.Controls
{
    /// <summary>
    /// Lógica de interacción para ProductLabel.xaml
    /// </summary>
    public partial class ProductLabel : UserControl
    {
        public Product Product { get; private set; }

        public ProductLabel()
        {
            InitializeComponent();
        }

        public void Init(Product product)
        {
            Product = product;
            product_id.Content = product.Id;
            product_desc.Content = product.Description;
        }
    }
}
