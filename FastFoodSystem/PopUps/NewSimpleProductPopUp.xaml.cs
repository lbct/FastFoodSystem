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

namespace FastFoodSystem.PopUps
{
    /// <summary>
    /// Lógica de interacción para NewSimpleProductPopUp.xaml
    /// </summary>
    public partial class NewSimpleProductPopUp : SystemPopUpClass
    {
        private int? product_id;

        public NewSimpleProductPopUp()
        {
            InitializeComponent();
        }

        public async Task Init(int? product_id = null)
        {
            this.product_id = product_id;
            category_search_bar.SelectedItem = null;
            category_search_bar.SearchText = "";
            image.BorrarImagen();
            description_text.Text = "";
            sale_value.Value = 0;
            discount_value.Value = 0;
            unit_cost_value.Value = 0;
            total_units_value.Value = 0;
            title.Content = "Nuevo Producto Simple";
            delete_button.Visibility = Visibility.Collapsed;
            hide_in_sales_button.IsChecked = false;

            category_search_bar.ItemsSource = await App.RunAsync(() => App.Database.CategoryTypes.ToArray());
            if(product_id != null)
            {
                delete_button.Visibility = Visibility.Visible;
                var product = await App.RunAsync(() => App.Database.Products.FirstOrDefault(p => p.Id == product_id.Value));
                var simpleProduct = await App.RunAsync(() => App.Database.SimpleProducts.FirstOrDefault(p => p.Id == product_id.Value));
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
                unit_cost_value.Value = double.Parse(simpleProduct.UnitCost.ToString());
                total_units_value.Value = simpleProduct.Units;
                title.Content = string.Format("Producto .- {0:0000}", product.Id);
            }
            if (hide_in_sales_button.IsChecked == true)
                hide_in_sale_button_label.Content = "Ocultar - Activado";
            else
                hide_in_sale_button_label.Content = "Ocultar - Desactivado";
        }

        private void Cancel_button_Click(object sender, RoutedEventArgs e)
        {
            App.OpenSystemPopUp<ConfirmPopUp>().Init("Al cancelar perderá los cambios, ¿Desea continuar?", () =>
            {
                App.CloseSystemPopUp();
            }, () =>
            {
                App.OpenSystemPopUp<NewSimpleProductPopUp>();
            });
        }

        private async void Save_button_Click(object sender, RoutedEventArgs e)
        {
            App.ShowLoad();
            try
            {
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
                decimal unitCost = decimal.Parse(unit_cost_value.Value.Value.ToString());
                int units = (int)total_units_value.Value.Value;

                Product product = null;
                if (product_id != null)
                    product = await App.RunAsync(() => App.Database.Products.FirstOrDefault(p => p.Id == product_id.Value));
                else
                    product = new Product();

                product.CategoryTypeId = categoryType.Id;
                product.Description = description;
                product.SaleValue = saleValue;
                product.SaleDiscount = discount;
                product.HideForSales = hide_in_sales_button.IsChecked == true;

                await App.RunAsync(() =>
                {
                    if(product_id == null)
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

                SimpleProduct simple = null;
                if (product_id != null)
                    simple = await App.RunAsync(() => App.Database.SimpleProducts.FirstOrDefault(sp => sp.Id == product_id.Value));
                else
                    simple = new SimpleProduct();
                simple.Id = product.Id;
                simple.UnitCost = unitCost;
                simple.Units = units;

                await App.RunAsync(() =>
                {
                    if(product_id == null)
                        App.Database.SimpleProducts.Add(simple);
                    App.Database.SaveChanges();
                });
                App.ShowMessage("Producto simple "+(product_id == null ? "registrado":"guardado")+" correctamente.", true, () =>
                {
                    App.GetSystemPage<ProductsPage>().Refresh();
                });
            }
            catch (Exception ex)
            {
                App.ShowMessage(ex.Message, false, () => App.OpenSystemPopUp<NewSimpleProductPopUp>());
            }
        }

        private async void Delete_button_Click(object sender, RoutedEventArgs e)
        {
            App.ShowLoad();
            var relations = await App.RunAsync(() => 
            {
                return App.Database.SimpleProductComboes
                .Where(rel => rel.SimpleProductId == product_id.Value)
                .ToArray();
            });
            if(relations.Length > 0)
            {
                StringBuilder sb = new StringBuilder("El producto que desea eliminar está vinculado a los siguientes Combos:\n\n");
                foreach(var relation in relations)
                {
                    string comboDesc = (await App.RunAsync(() =>
                    App.Database.Products.FirstOrDefault(c => c.Id == relation.ComboId)))
                    .Description;
                    sb.Append(comboDesc);
                    sb.AppendLine();
                }
                sb.AppendLine();
                sb.Append("Para eliminar el producto, primero elimine los vinculos.");
                App.ShowMessage(sb.ToString(), false, () => App.OpenSystemPopUp<NewSimpleProductPopUp>());
            }
            else
            {
                await App.GetSystemPopUp<SecurityPopUp>().Init(async login => 
                {
                    App.ShowLoad();
                    if (login)
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
                    else
                    {
                        App.ShowMessage("Contraseña incorrecta", false, () => App.OpenSystemPopUp<NewSimpleProductPopUp>());
                    }
                }, () => 
                {
                    App.OpenSystemPopUp<NewSimpleProductPopUp>();
                });
                App.OpenSystemPopUp<SecurityPopUp>();
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
