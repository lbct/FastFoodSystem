using FastFoodSystem.Controls;
using FastFoodSystem.Database;
using FastFoodSystem.Pages;
using FastFoodSystem.Scripts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
    /// Lógica de interacción para NewCompoundProductPopUp.xaml
    /// </summary>
    public partial class NewCompoundProductPopUp : SystemPopUpClass
    {
        private int? product_id;

        public NewCompoundProductPopUp()
        {
            InitializeComponent();
        }

        public async Task Init(int? product_id = null)
        {
            this.product_id = product_id;
            products_container.Children.Clear();
            UpdateValues();
            category_search_bar.SelectedItem = null;
            category_search_bar.SearchText = "";
            image.BorrarImagen();
            description_text.Text = "";
            sale_value.Value = 0;
            discount_value.Value = 0;
            hide_in_sales_button.IsChecked = false;

            category_search_bar.ItemsSource = await App.RunAsync(() => App.Database.CategoryTypes.ToArray());
            title.Content = "Nuevo Producto Compuesto";
            delete_button.Visibility = Visibility.Collapsed;

            if (product_id != null)
            {
                delete_button.Visibility = Visibility.Visible;

                var product = await App.RunAsync(() => App.Database.Products.FirstOrDefault(p => p.Id == product_id.Value));
                hide_in_sales_button.IsChecked = product.HideForSales;
                category_search_bar.SelectedItem = await App.RunAsync(() => App.Database.CategoryTypes.FirstOrDefault(ct => ct.Id == product.CategoryTypeId));
                if (File.Exists(product.ImagePath))
                {
                    //var imgSource = ImageFunctions.FileToImageSource(product.ImagePath);
                    image.EstablecerImagen(product.ImagePath);
                }
                description_text.Text = product.Description;
                sale_value.Value = double.Parse(product.SaleValue.ToString());
                discount_value.Value = double.Parse(product.SaleDiscount.ToString());
                title.Content = string.Format("Producto .- {0:0000}", product.Id);
                var relations = await App.RunAsync(() => App.Database.CompoundProductFoodInputs
                .Where(rel => rel.CompoundProductId == product_id.Value)
                .ToArray());
                foreach(var relation in relations)
                {
                    var relation_prod = await App.RunAsync(() => App.Database.Products.FirstOrDefault(p => p.Id == relation.FoodInputId));
                    AddProductToList(relation_prod, relation.RequiredUnits);
                }
                UpdateValues();
            }
            if (hide_in_sales_button.IsChecked == true)
                hide_in_sale_button_label.Content = "Ocultar - Activado";
            else
                hide_in_sale_button_label.Content = "Ocultar - Desactivado";
        }

        private async void Save_button_Click(object sender, RoutedEventArgs e)
        {
            App.ShowLoad();
            try
            {
                if (products_container.Children.Count <= 0)
                    throw new Exception("Debe añadir por lo menos 1 insumo a su producto.");
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

                Product product = null;
                if (product_id != null)
                    product = await App.RunAsync(() => App.Database.Products.FirstOrDefault(p => p.Id == product_id.Value));
                else
                    product = new Product();

                product.HideForSales = hide_in_sales_button.IsChecked == true;
                product.CategoryTypeId = categoryType.Id;
                product.Description = description;
                product.SaleValue = saleValue;
                product.SaleDiscount = discount;

                await App.RunAsync(() =>
                {
                    if (product_id == null)
                        App.Database.Products.Add(product);
                    App.Database.SaveChanges();
                });
                string imgPath = await ImageFunctions.GetImagePath(image, product.Id);
                if (imgPath != null)
                {
                    product.ImagePath = imgPath;
                    ImageManager.LoadBitmap(imgPath, 180, true);
                    await App.RunAsync(() => App.Database.SaveChanges());
                }
                else
                    product.ImagePath = null;

                CompoundProduct compoundProduct = null;
                if (product_id != null)
                    compoundProduct = await App.RunAsync(() => App.Database.CompoundProducts.FirstOrDefault(cp => cp.Id == product_id.Value));
                else
                    compoundProduct = new CompoundProduct() { Id = product.Id };
                await App.RunAsync(() =>
                {
                    if(product_id == null)
                        App.Database.CompoundProducts.Add(compoundProduct);
                    App.Database.SaveChanges();
                });

                var products = products_container.Children.OfType<ProductLabel>()
                    .ToDictionary(pl => pl.Product.Id, pl => (int)pl.units_value.Value.Value);

                foreach(var sel_product in products)
                {
                    CompoundProductFoodInput rel = null;
                    bool exist = false;
                    if (product_id != null)
                    {
                        rel = await App.RunAsync(() => App.Database.CompoundProductFoodInputs
                        .FirstOrDefault(r => r.CompoundProductId == product.Id && r.FoodInputId == sel_product.Key));
                        if (rel != null)
                            exist = true;
                    }
                    if (rel == null)
                        rel = new CompoundProductFoodInput();

                    rel.CompoundProductId = product.Id;
                    rel.FoodInputId = sel_product.Key;
                    rel.RequiredUnits = sel_product.Value;
                    if(!exist)
                        App.Database.CompoundProductFoodInputs.Add(rel);
                }
                if(product_id != null)
                {
                    var old_relations = await App.RunAsync(() => App.Database.CompoundProductFoodInputs
                    .Where(rel => rel.CompoundProductId == product_id.Value)
                    .ToArray());
                    var new_relations = products.Keys.ToList();
                    foreach (var relation in old_relations)
                    {
                        if (!new_relations.Exists(id => id == relation.FoodInputId))
                            App.Database.CompoundProductFoodInputs.Remove(relation);
                    }
                }

                await App.RunAsync(() => App.Database.SaveChanges());

                App.ShowMessage("Producto compuesto "+(product_id == null ? "registrado" : "guardado") +" correctamente.", true, () =>
                {
                    App.GetSystemPage<ProductsPage>().Refresh();
                });
            }
            catch (Exception ex)
            {
                App.ShowMessage(ex.Message, false, () => App.OpenSystemPopUp<NewCompoundProductPopUp>());
            }
        }

        private void Cancel_button_Click(object sender, RoutedEventArgs e)
        {
            App.OpenSystemPopUp<ConfirmPopUp>().Init("Al cancelar perderá los cambios, ¿Desea continuar?", () =>
            {
                App.CloseSystemPopUp();
            }, () =>
            {
                App.OpenSystemPopUp<NewCompoundProductPopUp>();
            });
        }

        private async void Delete_button_Click(object sender, RoutedEventArgs e)
        {
            App.ShowLoad();
            var relations = await App.RunAsync(() =>
            {
                return App.Database.CompoundProductComboes
                .Where(rel => rel.CompoundProductId == product_id.Value)
                .ToArray();
            });
            if (relations.Length > 0)
            {
                StringBuilder sb = new StringBuilder("El producto que desea eliminar está vinculado a los siguientes Combos:\n\n");
                foreach (var relation in relations)
                {
                    string comboDesc = (await App.RunAsync(() =>
                    App.Database.Products.FirstOrDefault(c => c.Id == relation.ComboId)))
                    .Description;
                    sb.Append(comboDesc);
                    sb.AppendLine();
                }
                sb.AppendLine();
                sb.Append("Para eliminar el producto, primero elimine los vinculos.");
                App.ShowMessage(sb.ToString(), false, () => App.OpenSystemPopUp<NewCompoundProductPopUp>());
            }
            else
            {
                await App.GetSystemPopUp<SecurityPopUp>().Init(async login =>
                {
                    App.ShowLoad();
                    if (login)
                    {
                        bool correct = await App.RunAsync(() => 
                        {
                            var foodInput_relations = App.Database.CompoundProductFoodInputs
                            .Where(r => r.CompoundProductId == product_id.Value)
                            .ToArray();
                            foreach(var rel in foodInput_relations)
                            {
                                App.Database.CompoundProductFoodInputs.Remove(rel);
                            }
                            App.Database.SaveChanges();
                        });
                        if (correct)
                        {
                            await App.RunAsync(() =>
                            {
                                var product = App.Database.Products.FirstOrDefault(p => p.Id == product_id.Value);
                                product.Hide = true;
                                App.Database.SaveChanges();
                            });
                            App.GetSystemPage<ProductsPage>().Refresh();
                            App.CloseSystemPopUp();
                        }
                    }
                    else
                    {
                        App.ShowMessage("Contraseña incorrecta", false, () => App.OpenSystemPopUp<NewCompoundProductPopUp>());
                    }
                }, () =>
                {
                    App.OpenSystemPopUp<NewCompoundProductPopUp>();
                });
                App.OpenSystemPopUp<SecurityPopUp>();
            }
        }

        private async void Add_product_button_Click(object sender, RoutedEventArgs e)
        {
            App.ShowLoad();
            var sel_products = products_container.Children.OfType<ProductLabel>()
                .Select(vp => vp.Product).ToList();
            var products = await App.RunAsync(() => 
            {
                return App.Database.GetFoodInputProductView()
                .Where(p => !sel_products.Exists(pr => pr.Id == p.Id))
                .ToArray();
            });
            App.OpenSystemPopUp<ProductPickerPopUp>().Init(async productView => 
            {
                var product = await App.RunAsync(() => App.Database.Products.FirstOrDefault(p => p.Id == productView.Id));
                AddProductToList(product);
                UpdateValues();
            }, () => 
            {
                App.OpenSystemPopUp<NewCompoundProductPopUp>();
            }, products.ToArray());
        }

        private void AddProductToList(Product product, int units = 1)
        {
            App.OpenSystemPopUp<NewCompoundProductPopUp>();
            var item = new ProductLabel()
            {
                Margin = new Thickness(10)
            };
            item.Init(product);
            item.units_value.Value = units;
            item.remove_button.Click += RemoveProductLabel_Click;
            item.units_value.ValueChanged += Units_value_ValueChanged;
            products_container.Children.Add(item);
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

        private void Hide_in_sales_button_Click(object sender, RoutedEventArgs e)
        {
            if (hide_in_sales_button.IsChecked == true)
                hide_in_sale_button_label.Content = "Ocultar - Activado";
            else
                hide_in_sale_button_label.Content = "Ocultar - Desactivado";
        }
    }
}
