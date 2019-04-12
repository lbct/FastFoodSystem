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
    /// Lógica de interacción para NewFoodInputPopUp.xaml
    /// </summary>
    public partial class NewFoodInputPopUp : SystemPopUpClass
    {
        private int? product_id;

        public NewFoodInputPopUp()
        {
            InitializeComponent();
        }

        public async Task Init(int? product_id = null)
        {
            this.product_id = product_id;
            unit_type_combo_box.SelectedItem = null;
            category_search_bar.SelectedItem = null;
            category_search_bar.SearchText = "";
            image.BorrarImagen();
            description_text.Text = "";
            sale_value.Value = 0;
            discount_value.Value = 0;
            unit_value.Value = 1;
            unit_cost_value.Value = 0;
            total_units_value.Value = 0;

            unit_type_combo_box.ItemsSource = await App.RunAsync(() => App.Database.UnitTypes.ToArray());
            category_search_bar.ItemsSource = await App.RunAsync(() => App.Database.CategoryTypes.ToArray());
            title.Content = "Nuevo Insumo";
            delete_button.Visibility = Visibility.Collapsed;

            if (product_id != null)
            {
                delete_button.Visibility = Visibility.Visible;
                
                var product = await App.RunAsync(() => App.Database.Products.FirstOrDefault(p => p.Id == product_id.Value));
                var foodInput = await App.RunAsync(() => App.Database.FoodInputs.FirstOrDefault(p => p.Id == product_id.Value));

                unit_type_combo_box.SelectedItem = await App.RunAsync(() => App.Database.UnitTypes.FirstOrDefault(ut => ut.Id == foodInput.UnitTypeId));
                category_search_bar.SelectedItem = await App.RunAsync(() => App.Database.CategoryTypes.FirstOrDefault(ct => ct.Id == product.CategoryTypeId));
                if (File.Exists(product.ImagePath))
                {
                    var imgSource = ImageFunctions.FileToImageSource(product.ImagePath);
                    image.EstablecerImagen(imgSource, product.ImagePath);
                }
                description_text.Text = product.Description;
                sale_value.Value = double.Parse(product.SaleValue.ToString());
                discount_value.Value = double.Parse(product.SaleDiscount.ToString());
                unit_value.Value = double.Parse(foodInput.UnitValue.ToString());
                unit_cost_value.Value = double.Parse(foodInput.UnitCost.ToString());
                total_units_value.Value = foodInput.Units;
                title.Content = string.Format("Producto .- {0:0000}", product.Id);
            }
        }

        private void UpdateCustomUnits()
        {
            if (unit_type_combo_box.SelectedItem != null)
            {
                var unit_type = (unit_type_combo_box.SelectedItem as UnitType);
                unit_value.CustomUnit = unit_type.ShortName;
                total_units_value.CustomUnit = "Unid (" + ((int)(unit_value.Value.Value * total_units_value.Value.Value)) + " " + unit_type.ShortName + ")";
            }
            else if(unit_value != null && total_units_value != null)
            {
                unit_value.CustomUnit = "Unid";
                total_units_value.CustomUnit = "Unid";
            }
        }

        private void Unit_type_combo_box_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateCustomUnits();
        }

        private void Unit_value_ValueChanged(object sender, Telerik.Windows.Controls.RadRangeBaseValueChangedEventArgs e)
        {
            UpdateCustomUnits();
        }

        private async void Delete_button_Click(object sender, RoutedEventArgs e)
        {
            App.ShowLoad();
            var combo_relations = await App.RunAsync(() =>
            {
                return App.Database.FoodInputComboes
                .Where(rel => rel.FoodInputId == product_id.Value)
                .ToArray();
            });
            var compound_relations = await App.RunAsync(() =>
            {
                return App.Database.CompoundProductFoodInputs
                .Where(rel => rel.FoodInputId == product_id.Value)
                .ToArray();
            });
            if (combo_relations.Length > 0 || compound_relations.Length > 0)
            {
                StringBuilder sb = new StringBuilder("El producto que desea eliminar está vinculado a los siguientes Productos:\n\n");
                foreach (var relation in combo_relations)
                {
                    string comboDesc = (await App.RunAsync(() =>
                    App.Database.Products.FirstOrDefault(c => c.Id == relation.ComboId)))
                    .Description;
                    sb.Append(comboDesc);
                    sb.AppendLine();
                }
                foreach (var relation in compound_relations)
                {
                    string desc = (await App.RunAsync(() =>
                    App.Database.Products.FirstOrDefault(c => c.Id == relation.CompoundProductId)))
                    .Description;
                    sb.Append(desc);
                    sb.AppendLine();
                }
                sb.AppendLine();
                sb.Append("Para eliminar el producto, primero elimine los vinculos.");
                App.ShowMessage(sb.ToString(), false, () => App.OpenSystemPopUp<NewFoodInputPopUp>());
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
                        App.ShowMessage("Contraseña incorrecta", false, () => App.OpenSystemPopUp<NewFoodInputPopUp>());
                    }
                }, () =>
                {
                    App.OpenSystemPopUp<NewFoodInputPopUp>();
                });
                App.OpenSystemPopUp<SecurityPopUp>();
            }
        }

        private void Cancel_button_Click(object sender, RoutedEventArgs e)
        {
            App.OpenSystemPopUp<ConfirmPopUp>().Init("Al cancelar perderá los cambios, ¿Desea continuar?", () => 
            {
                App.CloseSystemPopUp();
            }, () => 
            {
                App.OpenSystemPopUp<NewFoodInputPopUp>();
            });
        }

        private async void Save_button_Click(object sender, RoutedEventArgs e)
        {
            App.ShowLoad();
            try
            {
                if (unit_type_combo_box.SelectedItem == null)
                    throw new Exception("Debe seleccionar una unidad de medida");
                if (category_search_bar.SelectedItem == null && string.IsNullOrEmpty(category_search_bar.SearchText.Trim()))
                    throw new Exception("Debe seleccionar o escribir una categoría");
                if (string.IsNullOrEmpty(description_text.Text.Trim()))
                    throw new Exception("Debe escribir una descripción para el producto");

                UnitType unitType = unit_type_combo_box.SelectedItem as UnitType;
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
                int unitValue = (int)unit_value.Value.Value;
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

                FoodInput input = null;
                if (product_id != null)
                    input = await App.RunAsync(() => App.Database.FoodInputs.FirstOrDefault(sp => sp.Id == product_id.Value));
                else
                    input = new FoodInput();
                input.Id = product.Id;
                input.UnitCost = unitCost;
                input.Units = units;
                input.UnitValue = unitValue;
                input.UnitTypeId = unitType.Id;

                await App.RunAsync(() =>
                {
                    if (product_id == null)
                        App.Database.FoodInputs.Add(input);
                    App.Database.SaveChanges();
                });

                App.ShowMessage("Insumo "+(product_id == null ? "registrado" : "guardado") +" correctamente.", true, () => 
                {
                    App.GetSystemPage<ProductsPage>().Refresh();
                });
            }
            catch(Exception ex)
            {
                App.ShowMessage(ex.Message, false, () => App.OpenSystemPopUp<NewFoodInputPopUp>());
            }
        }

        private void Total_units_value_ValueChanged(object sender, Telerik.Windows.Controls.RadRangeBaseValueChangedEventArgs e)
        {
            UpdateCustomUnits();
        }
    }
}
