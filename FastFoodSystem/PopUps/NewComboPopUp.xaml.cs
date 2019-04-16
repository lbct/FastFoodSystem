using FastFoodSystem.Controls;
using FastFoodSystem.Database;
using FastFoodSystem.Pages;
using FastFoodSystem.Scripts;
using System;
using System.Collections.Generic;
using System.IO;
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
        private int? product_id;

        public NewComboPopUp()
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
            title.Content = "Nuevo Combo";
            delete_button.Visibility = Visibility.Collapsed;

            if (product_id != null)
            {
                delete_button.Visibility = Visibility.Visible;

                var product = await App.RunAsync(() => App.Database.Products.FirstOrDefault(p => p.Id == product_id.Value));
                var combo = await App.RunAsync(() => App.Database.Comboes.FirstOrDefault(p => p.Id == product_id.Value));
                hide_in_sales_button.IsChecked = product.HideForSales;
                category_search_bar.SelectedItem = await App.RunAsync(() => App.Database.CategoryTypes.FirstOrDefault(ct => ct.Id == product.CategoryTypeId));
                if (File.Exists(product.ImagePath))
                {
                    var imgSource = ImageFunctions.FileToImageSource(product.ImagePath);
                    image.EstablecerImagen(imgSource, product.ImagePath);
                }
                description_text.Text = product.Description;
                sale_value.Value = double.Parse(product.SaleValue.ToString());
                discount_value.Value = double.Parse(product.SaleDiscount.ToString());
                title.Content = string.Format("Producto .- {0:0000}", product.Id);

                var compoundRelations = await App.RunAsync(() => App.Database.CompoundProductComboes
                .Where(r => r.ComboId == combo.Id)
                .ToArray());
                var inputRelations = await App.RunAsync(() => App.Database.FoodInputComboes
                .Where(r => r.ComboId == combo.Id)
                .ToArray());
                var simpleRelations = await App.RunAsync(() => App.Database.SimpleProductComboes
                .Where(r => r.ComboId == combo.Id)
                .ToArray());
                foreach (var relation in compoundRelations)
                    AddProductToList(await App.RunAsync(() => App.Database.Products
                    .FirstOrDefault(p => p.Id == relation.CompoundProductId)),
                    relation.RequiredUnits);
                foreach (var relation in inputRelations)
                    AddProductToList(await App.RunAsync(() => App.Database.Products
                    .FirstOrDefault(p => p.Id == relation.FoodInputId)),
                    relation.RequiredUnits);
                foreach (var relation in simpleRelations)
                    AddProductToList(await App.RunAsync(() => App.Database.Products
                    .FirstOrDefault(p => p.Id == relation.SimpleProductId)),
                    relation.RequiredUnits);
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
                    await App.RunAsync(() => App.Database.SaveChanges());
                }
                else
                    product.ImagePath = null;

                Combo combo = null;
                if (product_id != null)
                    combo = await App.RunAsync(() => App.Database.Comboes.FirstOrDefault(c => c.Id == product_id.Value));
                else
                    combo = new Combo() { Id = product.Id };

                await App.RunAsync(() =>
                {
                    if(product_id == null)
                        App.Database.Comboes.Add(combo);
                    App.Database.SaveChanges();
                });
                await AddAndUpdateProductList(combo);
                if (product_id != null)
                    await RemoveOldRelations();
                await App.RunAsync(() => App.Database.SaveChanges());

                App.ShowMessage("Combo "+(product_id == null ? "registrado" : "guardado") +" correctamente.", true, () =>
                {
                    App.GetSystemPage<ProductsPage>().Refresh();
                });
            }
            catch (Exception ex)
            {
                App.ShowMessage(ex.Message, false, () => App.OpenSystemPopUp<NewComboPopUp>());
            }
        }

        private async Task RemoveOldRelations()
        {
            var products = products_container.Children.OfType<ProductLabel>()
                    .ToDictionary(pl => pl.Product.Id, pl => (int)pl.units_value.Value.Value);
            var new_relations = products.Keys.ToList();

            var old_inputs = await App.RunAsync(() => App.Database.FoodInputComboes
                    .Where(rel => rel.ComboId == product_id.Value)
                    .ToArray());
            var old_compounds = await App.RunAsync(() => App.Database.CompoundProductComboes
                    .Where(rel => rel.ComboId == product_id.Value)
                    .ToArray());
            var old_simple = await App.RunAsync(() => App.Database.SimpleProductComboes
                    .Where(rel => rel.ComboId == product_id.Value)
                    .ToArray());

            foreach (var relation in old_inputs)
            {
                if (!new_relations.Exists(id => id == relation.FoodInputId))
                    App.Database.FoodInputComboes.Remove(relation);
            }
            foreach (var relation in old_compounds)
            {
                if (!new_relations.Exists(id => id == relation.CompoundProductId))
                    App.Database.CompoundProductComboes.Remove(relation);
            }
            foreach (var relation in old_simple)
            {
                if (!new_relations.Exists(id => id == relation.SimpleProductId))
                    App.Database.SimpleProductComboes.Remove(relation);
            }
        }

        private async Task AddAndUpdateProductList(Combo combo)
        {
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
                            CompoundProductCombo rel = null;
                            bool exist = false;
                            if (product_id != null)
                            {
                                rel = App.Database.CompoundProductComboes
                                .FirstOrDefault(r => r.ComboId == combo.Id && r.CompoundProductId == compoundProduct.Id);
                                if (rel == null)
                                    rel = new CompoundProductCombo();
                                else
                                    exist = true;
                            }
                            else
                                rel = new CompoundProductCombo();
                            rel.ComboId = combo.Id;
                            rel.CompoundProductId = compoundProduct.Id;
                            rel.RequiredUnits = sel_product.Value;
                            if (!exist)
                                App.Database.CompoundProductComboes.Add(rel);
                        }
                        else
                        {
                            SimpleProductCombo rel = null;
                            bool exist = false;
                            if (product_id != null)
                            {
                                rel = App.Database.SimpleProductComboes
                                .FirstOrDefault(r => r.ComboId == combo.Id && r.SimpleProductId == simpleProd.Id);
                                if (rel == null)
                                    rel = new SimpleProductCombo();
                                else
                                    exist = true;
                            }
                            else
                                rel = new SimpleProductCombo();
                            rel.ComboId = combo.Id;
                            rel.SimpleProductId = simpleProd.Id;
                            rel.RequiredUnits = sel_product.Value;
                            if (!exist)
                                App.Database.SimpleProductComboes.Add(rel);
                        }
                    }
                    else
                    {
                        FoodInputCombo rel = null;
                        bool exist = false;
                        if (product_id != null)
                        {
                            rel = App.Database.FoodInputComboes
                            .FirstOrDefault(r => r.ComboId == combo.Id && r.FoodInputId == foodInput.Id);
                            if (rel == null)
                                rel = new FoodInputCombo();
                            else
                                exist = true;
                        }
                        else
                            rel = new FoodInputCombo();
                        rel.ComboId = combo.Id;
                        rel.FoodInputId = foodInput.Id;
                        rel.RequiredUnits = sel_product.Value;
                        if (!exist)
                            App.Database.FoodInputComboes.Add(rel);
                    }
                });
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

        private async void Delete_button_Click(object sender, RoutedEventArgs e)
        {
            App.ShowLoad();
            await App.GetSystemPopUp<SecurityPopUp>().Init(async login =>
            {
                App.ShowLoad();
                if (login)
                {
                    bool correct = await App.RunAsync(() => 
                    {
                        var input_relations = App.Database.FoodInputComboes
                        .Where(r => r.ComboId == product_id.Value)
                        .ToArray();
                        var compound_relations = App.Database.CompoundProductComboes
                        .Where(r => r.ComboId == product_id.Value)
                        .ToArray();
                        var simple_relations = App.Database.SimpleProductComboes
                        .Where(r => r.ComboId == product_id.Value)
                        .ToArray();
                        foreach (var rel in input_relations)
                            App.Database.FoodInputComboes.Remove(rel);
                        foreach (var rel in compound_relations)
                            App.Database.CompoundProductComboes.Remove(rel);
                        foreach (var rel in simple_relations)
                            App.Database.SimpleProductComboes.Remove(rel);
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
                    App.ShowMessage("Contraseña incorrecta", false, () => App.OpenSystemPopUp<NewComboPopUp>());
                }
            }, () =>
            {
                App.OpenSystemPopUp<NewComboPopUp>();
            });
            App.OpenSystemPopUp<SecurityPopUp>();
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
                .Where(p => !sel_products.Exists(sp => sp.Id == p.Id) && !exclude.Exists(id => id == p.Id) && !p.Hide)
                .ToArray();
            });
            App.OpenSystemPopUp<ProductPickerPopUp>().Init(product =>
            {
                App.OpenSystemPopUp<NewComboPopUp>();
                AddProductToList(product);
                UpdateValues();
            }, () =>
            {
                App.OpenSystemPopUp<NewComboPopUp>();
            }, products.ToArray());
        }

        private void AddProductToList(Product product, int units = 1)
        {
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
