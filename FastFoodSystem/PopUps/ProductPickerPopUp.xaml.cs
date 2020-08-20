using FastFoodSystem.Controls;
using FastFoodSystem.Database;
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
using Telerik.Windows.Controls;

namespace FastFoodSystem.PopUps
{
    /// <summary>
    /// Lógica de interacción para ProductPickerPopUp.xaml
    /// </summary>
    public partial class ProductPickerPopUp : SystemPopUpClass
    {
        private Action<ProductView> action;
        private Action actionCancel;

        public ProductPickerPopUp()
        {
            InitializeComponent();
        }

        public void Init(Action<ProductView> action, Action actionCancel, ProductView[] products)
        {
            this.action = action;
            this.actionCancel = actionCancel;
            product_container.Children.Clear();
            foreach (var product in products)
            {
                VisualProduct item = new VisualProduct()
                {
                    Margin = new Thickness(10)
                };
                item.SetProduct(product);
                item.button.Click += SelectProduct_Click;
                product_container.Children.Add(item);
            }
        }

        private void SelectProduct_Click(object sender, RoutedEventArgs e)
        {
            var product = (sender as RadButton).ParentOfType<VisualProduct>().ProductView;
            App.CloseSystemPopUp();
            action?.Invoke(product);
        }

        private void Cancel_button_Click(object sender, RoutedEventArgs e)
        {
            App.CloseSystemPopUp();
            actionCancel?.Invoke();
        }
    }
}
