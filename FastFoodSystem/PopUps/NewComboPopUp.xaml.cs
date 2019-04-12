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
using Telerik.Windows.Controls;

namespace FastFoodSystem.PopUps
{
    /// <summary>
    /// Lógica de interacción para NewComboPopUp.xaml
    /// </summary>
    public partial class NewComboPopUp : SystemPopUpClass
    {
        public NewComboPopUp()
        {
            InitializeComponent();
        }

        public async void Init()
        {
            products_container.Children.Clear();
            UpdateValues();
            category_search_bar.SelectedItem = null;
            category_search_bar.SearchText = "";
            image.BorrarImagen();
            description_text.Text = "";
            sale_value.Value = 0;
            discount_value.Value = 0;

            category_search_bar.ItemsSource = await App.RunAsync(() => App.Database.CategoryTypes.ToArray());
        }

        private async void Save_button_Click(object sender, RoutedEventArgs e)
        {
            App.ShowLoad();
            try
            {
                if (products_container.Children.Count <= 0)
                    throw new Exception("Debe añadir por lo menos 1 producto a su combo.");
                if (category_search_bar.SelectedItem == null && string.IsNullOrEmpty(category_search_bar.SearchText.Trim()))
                    throw new Exception("Debe seleccionar o escribir una categoría");
                if (string.IsNullOrEmpty(description_text.Text.Trim()))
                    throw new Exception("Debe escribir una descripción para el producto");

                CategoryType categoryType = category_search_bar.SelectedItem as CategoryType;
                if (categoryType == null)
                {
                    string desc = category_search_bar.SearchText.Trim();
                    categoryType = await App.RunAsync(() =>
                    {
                        CategoryType category = App.Database.CategoryTypes.FirstOrDefault(ct => ct.Description.Trim().ToLower().Equals(desc.Trim().ToLower()));
                        if (category == null)
                        {
                            category = new CategoryType()
                            {
                                Description = desc.Trim()
                            };
                            App.Database.CategoryTypes.Add(category);
                            App.Database.SaveChanges();
                        }
                        return category;
                    });
                }
                string description = description_text.Text.Trim();
                decimal saleValue = decimal.Parse(sale_value.Value.Value.ToString());
                decimal discount = decimal.Parse(discount_value.Value.Value.ToString());

                Product product = new Product()
                {
                    CategoryTypeId = categoryType.Id,
                    Description = description,
                    SaleValue = saleValue,
                    SaleDiscount = discount
                };
                await App.RunAsync(() =>
                {
                    App.Database.Products.Add(product);
                    App.Database.SaveChanges();
                });
                string imgPath = await ImageFunctions.GetImagePath(image, product.Id);
                if (imgPath != null)
                {
                    product.ImagePath = imgPath;
                    await App.RunAsync(() => App.Database.SaveChanges());
                }
                Combo combo = new Combo()
                {
                    Id = product.Id
                };
                await App.RunAsync(() =>
                {
                    App.Database.Comboes.Add(combo);
                    App.Database.SaveChanges();
                });

                var products = products_container.Children.OfType<ProductLabel>()
                    .ToDictionary(pl => pl.Product.Id, pl => (int)pl.units_value.Value.Value);
                foreach (var sel_product in products)
                {
                    await App.RunAsync(() => 
                    {
                        var foodInput = App.Database.FoodInputs.FirstOrDefault(fi => fi.Id == sel_product.Key);
                        if (foodInput == null)
                        {
                            var simpleProd = App.Database.SimpleProducts.FirstOrDefault(sp => sp.Id == sel_product.Key);
                            if (simpleProd == null)
                            {
                                var compoundProduct = App.Database.CompoundProducts.FirstOrDefault(cp => cp.Id == sel_product.Key);
                                CompoundProductCombo rel = new CompoundProductCombo()
                                {
                                    ComboId = combo.Id,
                                    CompoundProductId = compoundProduct.Id,
                                    RequiredUnits = sel_product.Value
                                };
                                App.Database.CompoundProductComboes.Add(rel);
                            }
                            else
                            {
                                SimpleProductCombo rel = new SimpleProductCombo()
                                {
                                    ComboId = combo.Id,
                                    SimpleProductId = simpleProd.Id,
                                    RequiredUnits = sel_product.Value
                                };
                                App.Database.SimpleProductComboes.Add(rel);
                            }
                        }
                        else
                        {
                            FoodInputCombo rel = new FoodInputCombo()
                            {
                                ComboId = combo.Id,
                                FoodInputId = foodInput.Id,
                                RequiredUnits = sel_product.Value
                            };
                            App.Database.FoodInputComboes.Add(rel);
                        }
                    });
                }
                await App.RunAsync(() => App.Database.SaveChanges());

                App.ShowMessage("Combo registrado correctamente.", true, () =>
                {
                    App.GetSystemPage<ProductsPage>().Refresh();
                });
            }
            catch (Exception ex)
            {
                App.ShowMessage(ex.Message, false, () => App.OpenSystemPopUp<NewComboPopUp>());
            }
        }

        private void Cancel_button_Click(object sender, RoutedEventArgs e)
        {
            App.OpenSystemPopUp<ConfirmPopUp>().Init("Al cancelar perderá los cambios, ¿Desea continuar?", () =>
            {
                App.CloseSystemPopUp();
            }, () =>
            {
                App.OpenSystemPopUp<NewComboPopUp>();
            });
        }

        private void Delete_button_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void Add_product_button_Click(object sender, RoutedEventArgs e)
        {
            App.ShowLoad();
            var sel_products = products_container.Children.OfType<ProductLabel>()
                .Select(vp => vp.Product).ToList();
            var products = await App.RunAsync(() =>
            {
                var exclude = App.Database.Comboes
                                .Select(c => c.Id)
                                .ToList();
                return App.Database.Products
                .ToArray()
                .Where(p => !exclude.Exists(id => id == p.Id))
                .ToArray();
            });
            App.OpenSystemPopUp<ProductPickerPopUp>().Init(product =>
            {
                App.OpenSystemPopUp<NewComboPopUp>();
                var item = new ProductLabel()
                {
                    Margin = new Thickness(10)
                };
                item.Init(product);
                item.remove_button.Click += RemoveProductLabel_Click;
                item.units_value.ValueChanged += Units_value_ValueChanged;
                products_container.Children.Add(item);
                UpdateValues();
            }, () =>
            {
                App.OpenSystemPopUp<NewComboPopUp>();
            }, products.ToArray());
        }

        private void Units_value_ValueChanged(object sender, Telerik.Windows.Controls.RadRangeBaseValueChangedEventArgs e)
        {
            UpdateValues();
        }

        private void RemoveProductLabel_Click(object sender, RoutedEventArgs e)
        {
            products_container.Children.Remove((sender as RadButton).ParentOfType<ProductLabel>());
            UpdateValues();
        }

        private async void UpdateValues()
        {
            var products = products_container.Children.OfType<ProductLabel>()
                .ToDictionary(pl => pl.Product.Id, pl => (int)pl.units_value.Value);

            var minUnits = await App.RunAsync(() =>
            {
                return products.Count > 0 ? products.Min(p => App.Database.GetProductUnits(p.Key) / p.Value) : 0;
            });
            var unitCost = await App.RunAsync(() =>
            {
                return products.Count > 0 ? products.Sum(p => App.Database.GetProductCost(p.Key) * p.Value) : 0;
            });
            if (unit_cost != null && available_units_value != null)
            {
                unit_cost.Value = double.Parse(unitCost.ToString());
                available_units_value.Value = minUnits;
            }
        }
    }
}
