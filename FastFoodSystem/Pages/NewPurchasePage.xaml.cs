using FastFoodSystem.Database;
using FastFoodSystem.PopUps;
using FastFoodSystem.Scripts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
    /// Interaction logic for NewPurchasePage.xaml
    /// </summary>
    public partial class NewPurchasePage : SystemPageClass
    {
        private ObservableCollection<PurchaseDetailItem> details;

        public NewPurchasePage()
        {
            InitializeComponent();
            details = new ObservableCollection<PurchaseDetailItem>();
            details.CollectionChanged += Details_CollectionChanged;
            purchase_table.ItemsSource = details;
        }

        public override void Refresh()
        {
            current_date.SelectedValue = DateTime.Now;
            details.Clear();
            total_sale_value.Value = 0;
        }

        private async void Add_purchase_detail_button_Click(object sender, RoutedEventArgs e)
        {
            App.OpenSystemPopUp<LoadPopUp>();

            var foodInputs = await App.RunAsync(() => App.Database.FoodInputs.ToArray());
            var simples = await App.RunAsync(() => App.Database.SimpleProducts.ToArray());
            var products = new List<Product>();
            foreach (var input in foodInputs)
            {
                var product = await App.RunAsync(() => App.Database.Products.FirstOrDefault(p => p.Id == input.Id));
                if (!product.Hide)
                    products.Add(product);
            }
            foreach(var simple in simples)
            {
                var product = await App.RunAsync(() => App.Database.Products.FirstOrDefault(p => p.Id == simple.Id));
                if (!product.Hide)
                    products.Add(product);
            }
            App.GetSystemPopUp<ProductPickerPopUp>().Init(AddPurchaseDetailItem, () => { }, products.ToArray());
            App.OpenSystemPopUp<ProductPickerPopUp>();
        }

        private void Do_purchase_button_Click(object sender, RoutedEventArgs e)
        {
            if (details.Count > 0)
            {
                App.OpenSystemPopUp<ConfirmPopUp>().Init("¿Desea realizar la compra?", async () =>
                {
                    App.OpenSystemPopUp<LoadPopUp>();
                    DateTime date = current_date.SelectedValue.Value;
                    Purchase purchase = new Purchase()
                    {
                        DateTime = date,
                        LoginId = UserSession.LoginID
                    };
                    bool correct = await App.RunAsync(() =>
                    {
                        App.Database.Purchases.Add(purchase);
                        App.Database.SaveChanges();
                        var purchase_details = details.Select(d =>
                        {
                            PurchaseDetail detail = new PurchaseDetail()
                            {
                                ProductId = d.Product.Id,
                                PurchaseId = purchase.Id,
                                Units = d.Units,
                                UnitValue = d.PurchaseValue
                            };
                            return detail;
                        }).ToArray();
                        purchase.PurchaseDetails = purchase_details;
                        foreach (var purchase_detail in purchase_details)
                        {
                            App.Database.PurchaseDetails.Add(purchase_detail);
                        }
                        App.Database.SaveChanges();
                    });
                    if (correct)
                    {
                        foreach(var detail in purchase.PurchaseDetails)
                        {
                            await DatabaseActions.IncreaseProductUnits(detail);
                        }
                        App.ShowMessage("La compra fue realizada con éxito", true, () =>
                        {
                            Refresh();
                        });
                    }
                });
            }
            else
                App.ShowMessage("Debe añadir por lo menos 1 producto a la compra.", false);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if(details.Count == 0)
                App.OpenSystemPage<MenuPage>();
            else
            {
                App.OpenSystemPopUp<ConfirmPopUp>().Init("Se perderán los datos de la compra, ¿Desea continuar?", () => 
                {
                    App.OpenSystemPage<MenuPage>();
                });
            }
        }


        private void Delete_detail_button_Initialized(object sender, EventArgs e)
        {
            (sender as Button).Click += new RoutedEventHandler(EditData_Click);
        }

        private async void AddPurchaseDetailItem(Product product)
        {
            var item = details.FirstOrDefault(i => i.Product.Id == product.Id);
            if (item == null)
            {
                var newItem = new PurchaseDetailItem()
                {
                    Product = product,
                    PurchaseValue = await App.RunAsync(() => App.Database
                    .GetProductCost(product.Id)),
                    Units = 1
                };
                details.Add(newItem);
                newItem.PropertyChanged += StoreProduct_PropertyChanged;
            }
            else
            {
                item.Units++;
            }
        }

        private void StoreProduct_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            total_sale_value.Value = details.Count > 0 ? double.Parse(details.Sum(d => d.Total).ToString()) : 0;
        }

        private void Details_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            total_sale_value.Value = details.Count > 0 ? double.Parse(details.Sum(d => d.Total).ToString()) : 0;
        }

        private void EditData_Click(object sender, RoutedEventArgs e)
        {
            var data = ((FrameworkElement)sender).DataContext as PurchaseDetailItem;
            details.Remove(data);
        }
    }

    public class PurchaseDetailItem : INotifyPropertyChanged
    {
        private int _units;

        public Product Product { get; set; }

        public int Units
        {
            get { return _units; }
            set
            {
                _units = value;
                InvokePropertyChanged(new PropertyChangedEventArgs("Units"));
                InvokePropertyChanged(new PropertyChangedEventArgs("Total"));
            }
        }

        public decimal PurchaseValue { get; set; }

        public decimal Total
        {
            get
            {
                decimal val = PurchaseValue * Units;
                return val;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void InvokePropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }
    }
}
