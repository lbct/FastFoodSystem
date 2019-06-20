using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;
using Telerik.Windows.Documents.Spreadsheet.FormatProviders;
using Telerik.Windows.Documents.Spreadsheet.FormatProviders.OpenXml.Xlsx;
using Telerik.Windows.Documents.Spreadsheet.Model;
using Telerik.Windows.Documents.Spreadsheet.PropertySystem;
using Telerik.Windows.Documents.Spreadsheet.Theming;
using Telerik.Windows.Documents.Spreadsheet.Utilities;

namespace FastFoodSystem.Scripts
{
    static class ExcelExporter
    {
        private static Workbook workbook;
        private static string nombreEmpresa;
        private static Dictionary<Estilos, string> nombresEstilos;
        private static string dirArchivo;

        private enum Estilos
        {
            FooterExcel,
            Opcional,
            Headers,
            ExpensePeriodStyle,
            CompanyStyle
        }

        public static void Export(RadGridView datos, RadDataPager control, string[] formatos, string nombre, string detalle)
        {
            App.ShowLoad();
            SaveFileDialog save = new SaveFileDialog();
            save.Title = "Guardar Documento Excel";
            save.Filter = "Documento Excel|*.xlsx";
            string archivo = "";
            if (save.ShowDialog() == true)
                archivo = save.FileName;
            if (!archivo.Equals(""))
                Exportar(archivo, datos, control, formatos, nombre, detalle);
            App.CloseSystemPopUp();
        }

        public static void Export<T>(T[] data, string[] formatos, string nombre, string detalle)
        {
            App.ShowLoad();
            SaveFileDialog save = new SaveFileDialog();
            save.Title = "Guardar Documento Excel";
            save.Filter = "Documento Excel|*.xlsx";
            string archivo = "";
            if (save.ShowDialog() == true)
                archivo = save.FileName;
            if (!archivo.Equals(""))
                Exportar(archivo, data, formatos, nombre, detalle);
            App.CloseSystemPopUp();
        }

        private static void Exportar<T>(string dir, T[] data, string[] formatos, string nombre, string detalle)
        {
            nombreEmpresa = nombre;
            dirArchivo = dir;
            workbook = new Workbook();
            Set_DefaultStyle();
            AgregarEstilos();
            var ws = workbook.Worksheets.Add();
            RadGridView tabla = Convert(data);//DarCopiaLimpia(datos, control);
            Worksheet worksheet = tabla.ExportToWorkbook().Worksheets[0];
            CellRange celdas = worksheet.UsedCellRange;
            WorksheetFragment worksheetFragment = worksheet.Cells[celdas].Copy();
            int numeroFila = 5;
            ws.Cells[numeroFila, 0].Paste(worksheetFragment, PasteOptions.All);

            ColumnSelection columnSelection = ws.Columns[0, tabla.Columns.Count - 1];
            for (int c = 0; c < tabla.Columns.Count; c++)
            {
                if (c < formatos.Length && !string.IsNullOrWhiteSpace(formatos[c]))
                {
                    CellRange cells = new CellRange(numeroFila, c, numeroFila + tabla.Items.Count, c);
                    ws.Cells[cells].SetFormat(new CellValueFormat(formatos[c]));
                }
            }
            columnSelection.AutoFitWidth();
            for (int i = 0; i < tabla.Columns.Count; i++)
            {
                ColumnSelection column = ws.Columns[i];
                column.SetWidth(new ColumnWidth(column.GetWidth().Value.Value + 50, true));
            }
            //columnSelection.SetWidth(new ColumnWidth(columnSelection.GetWidth().Value.Value + 50, true));
            RowSelection rowSelection = ws.Rows[numeroFila, numeroFila + tabla.Items.Count];
            rowSelection.SetHeight(new RowHeight(30, true));
            try
            {
                ws.Filter.FilterRange = new CellRange(numeroFila, 0, numeroFila, tabla.Columns.Count - 1);
                RealizarOperaciones(tabla.Columns.Count - 1, tabla.Items.Count, detalle);
                MessageBox.Show("Archivo excel exportado.");
            }
            catch (Exception e)
            {
                ErrorAlExportar(e);
            }
        }

        private static void Exportar(string dir, RadGridView datos, RadDataPager control, string[] formatos, string nombre, string detalle)
        {
            nombreEmpresa = nombre;
            dirArchivo = dir;
            workbook = new Workbook();
            Set_DefaultStyle();
            AgregarEstilos();
            var ws = workbook.Worksheets.Add();
            RadGridView tabla = DarCopiaLimpia(datos, control);
            Worksheet worksheet = tabla.ExportToWorkbook().Worksheets[0];
            CellRange celdas = worksheet.UsedCellRange;
            WorksheetFragment worksheetFragment = worksheet.Cells[celdas].Copy();
            int numeroFila = 5;
            ws.Cells[numeroFila, 0].Paste(worksheetFragment, PasteOptions.All);

            ColumnSelection columnSelection = ws.Columns[0, tabla.Columns.Count - 1];
            for (int c = 0; c < tabla.Columns.Count; c++)
            {
                if (c < formatos.Length && !string.IsNullOrWhiteSpace(formatos[c]))
                {
                    CellRange cells = new CellRange(numeroFila, c, numeroFila + tabla.Items.Count, c);
                    ws.Cells[cells].SetFormat(new CellValueFormat(formatos[c]));
                }
            }
            columnSelection.AutoFitWidth();
            for(int i = 0; i < tabla.Columns.Count; i++)
            {
                ColumnSelection column = ws.Columns[i];
                column.SetWidth(new ColumnWidth(column.GetWidth().Value.Value + 50, true));
            }
            //columnSelection.SetWidth(new ColumnWidth(columnSelection.GetWidth().Value.Value + 50, true));
            RowSelection rowSelection = ws.Rows[numeroFila, numeroFila + tabla.Items.Count];
            rowSelection.SetHeight(new RowHeight(30, true));
            try
            {
                ws.Filter.FilterRange = new CellRange(numeroFila, 0, numeroFila, tabla.Columns.Count - 1);
                RealizarOperaciones(tabla.Columns.Count - 1, tabla.Items.Count, detalle);
                MessageBox.Show("Archivo excel exportado.");
            }
            catch(Exception e)
            {
                ErrorAlExportar(e);
            }
        }

        public static string DarFormatoUnidad(string unidad)
        {
            return "#.##,#0 [$"+unidad+"-400A];-#.##,#0 [$"+unidad+"-400A]";
        }

        public static string DarFormatoFecha()
        {
            return "dd/mm/yyyy hh:mm:ss";
        }

        private static void ErrorAlExportar(Exception e)
        {
            MessageBox.Show(e.Message);
            File.Delete(dirArchivo);
        }

        private static void RealizarOperaciones(int numeroColumnas, int numeroFilas, string detalle)
        {
            Set_Company_Title(numeroColumnas);
            Set_Details(numeroColumnas, detalle);
            Set_Header_Styles(numeroColumnas);
            Items_Color_Styles(numeroColumnas, numeroFilas);
            //Funciones_Excel(numeroColumnas, numeroFilas);
            ExportarArchivo(dirArchivo);
        }

        private static void ExportarArchivo(string dir)
        {
            //Dar espacio a lo escrito
            //Worksheet ws;
            //ws = workbook.Worksheets[0];
            //ws.Columns[ws.UsedCellRange].AutoFitWidth();
            IWorkbookFormatProvider formatProvider = new XlsxFormatProvider();
            using (FileStream output = new FileStream(dir, FileMode.Create))
            {
                formatProvider.Export(workbook, output);
            }
        }

        private static RadGridView Convert<T>(T[] data)
        {
            RadGridView rgv = new RadGridView();
            rgv.AutoGenerateColumns = true;
            rgv.ItemsSource = data;
            return rgv;
        }
        
        private static RadGridView DarCopiaLimpia(RadGridView tabla, RadDataPager control)
        {
            RadGridView rgv = new RadGridView();
            if (control != null)
            {
                //var datos = (tabla.ItemsSource as IEnumerable);
                List<object> lista = new List<object>();
                control.MoveToFirstPage();
                foreach (var obj in tabla.Items)
                    lista.Add(obj);
                while (control.CanMoveToNextPage)
                {
                    control.MoveToNextPage();
                    foreach (var obj in tabla.Items)
                        lista.Add(obj);
                }
                control.MoveToFirstPage();
                foreach (GridViewColumn column in tabla.Columns)
                {
                    if ((column is GridViewDataColumn) && column.IsVisible)
                    {
                        try
                        {
                            GridViewDataColumn columnCopy = new GridViewDataColumn()
                            {
                                Header = (column as GridViewDataColumn).Header,
                                DataMemberBinding = new Binding((column as GridViewDataColumn).DataMemberBinding.Path.Path)
                            };
                            rgv.Columns.Add(columnCopy);
                        }
                        catch (NullReferenceException) { }
                    }
                }
                rgv.FilterDescriptors.Add(tabla.FilterDescriptors);
                rgv.AutoGenerateColumns = false;
                rgv.ItemsSource = lista;
            }
            else
                rgv = tabla;
            return rgv;
        }
        
        private static void Set_DefaultStyle()
        {
            var estilo = workbook.Styles["Normal"];
            estilo.ForeColor = new ThemableColor(Colors.Red);
            estilo.FontFamily = new ThemableFontFamily(ThemeFontType.Minor);
            estilo.FontSize = UnitHelper.PointToDip(10);
            estilo.HorizontalAlignment = RadHorizontalAlignment.Center;
            CellBorder border = new CellBorder(CellBorderStyle.Thin, new ThemableColor(Colors.Chartreuse));
            estilo.RightBorder = border;
            estilo.TopBorder = border;
            estilo.LeftBorder = border;
            estilo.BottomBorder = border;
        }

        private static void Set_Company_Title(int numeroColumnas)
        {
            var ws = workbook.Worksheets[0];
            CellSelection departmentNameCells = ws.Cells[0, 0, 0, numeroColumnas];
            departmentNameCells.Merge();
            departmentNameCells.SetValue(nombreEmpresa);
            ws.Cells[0, 0, 0, 4].SetStyleName(DarEstilo(Estilos.CompanyStyle));
        }

        private static void Set_Details(int numeroColumnas, string detalle)
        {
            var ws = workbook.Worksheets[0];
            //Reporte de expensas inserción de texto
            CellSelection reporte_celdas = ws.Cells[2, 0, 2, numeroColumnas];
            reporte_celdas.Merge();
            reporte_celdas.SetValue(detalle);
            //Sección de la fecha
            CellSelection Fecha = ws.Cells[3, 0, 3, numeroColumnas];
            Fecha.Merge();
            Fecha.SetValue(DateTime.Now);
            Fecha.SetFormat(new CellValueFormat("[$-F800]dddd, mmmm dd, yyyy"));
            //Estilo para reporte de expensas y fecha
            ws.Cells[1, 0, 4, numeroColumnas].SetStyleName(DarEstilo(Estilos.ExpensePeriodStyle));
        }

        private static void Set_Header_Styles(int numeroColumnas)
        {
            var ws = workbook.Worksheets[0];
            ws.Cells[5, 0, 5, numeroColumnas].SetStyleName(DarEstilo(Estilos.Headers));
        }

        private static void Items_Color_Styles(int numeroColumnas, int numeroFilas)
        {
            var ws = workbook.Worksheets[0];
            ws.Cells[6, 0, numeroFilas + 5, numeroColumnas].SetStyleName(DarEstilo(Estilos.Opcional));
        }

        /*private static void Funciones_Excel(int numeroColumnas, int numeroFilas)
        {
            var ws = workbook.Worksheets[0];
            ws.Cells[numeroFilas + 5, 0].SetValue("Total: ");
            ws.Cells[numeroFilas + 5, 0, numeroFilas + 5, numeroColumnas + 3].SetStyleName(DarEstilo(Estilos.FooterExcel));
        }*/

        private static void AgregarEstilos()
        {
            nombresEstilos = new Dictionary<Estilos, string>();
            CellStyle estilo;
            //Definiendo Footer_Excel
            estilo = NuevoEstilo(Estilos.FooterExcel);
            estilo.FontFamily = new ThemableFontFamily("Arial");
            //estilo.Fill = PatternFill.CreateSolidFill(new ThemableColor(Color.FromRgb(129, 142, 30)));
            //estilo.ForeColor = new ThemableColor(Colors.White);
            estilo.FontSize = UnitHelper.PointToDip(11);
            //Definiendo Opcional
            estilo = NuevoEstilo(Estilos.Opcional);
            //estilo.Fill = PatternFill.CreateSolidFill(new ThemableColor(GreenPalette.Palette.MainColor));
            //estilo.ForeColor = new ThemableColor(GreenPalette.Palette.MarkerColor);
            estilo.FontFamily = new ThemableFontFamily("Arial");
            estilo.HorizontalAlignment = RadHorizontalAlignment.Left;
            //Definiendo Headers
            estilo = NuevoEstilo(Estilos.Headers);
            //estilo.BottomBorder = new CellBorder(CellBorderStyle.Thick, new ThemableColor(Color.FromRgb(166, 183, 39)));
            //estilo.Fill = PatternFill.CreateSolidFill(new ThemableColor(GreenPalette.Palette.AlternativeColor));
            estilo.FontSize = UnitHelper.PointToDip(11);
            estilo.FontFamily = new ThemableFontFamily("Arial");
            estilo.HorizontalAlignment = RadHorizontalAlignment.Left;
            //estilo.ForeColor = new ThemableColor(GreenPalette.Palette.MarkerColor);
            //Definiendo ExpensePeriodStyle
            estilo = NuevoEstilo(Estilos.ExpensePeriodStyle);
            estilo.FontFamily = new ThemableFontFamily("Arial");
            estilo.FontSize = UnitHelper.PointToDip(14);
            estilo.HorizontalAlignment = RadHorizontalAlignment.Left;
            //estilo.ForeColor = new ThemableColor(GreenPalette.Palette.MarkerColor);
            //estilo.Fill = PatternFill.CreateSolidFill(new ThemableColor(GreenPalette.Palette.MainColor));
            //Definiendo CompanyStyle
            estilo = NuevoEstilo(Estilos.CompanyStyle);
            estilo.FontFamily = new ThemableFontFamily(ThemeFontType.Major);
            estilo.FontSize = UnitHelper.PointToDip(32);
            estilo.HorizontalAlignment = RadHorizontalAlignment.Center;
            //estilo.ForeColor = new ThemableColor(GreenPalette.Palette.MarkerColor);
            estilo.FontFamily = new ThemableFontFamily("Arial");
            //estilo.Fill = PatternFill.CreateSolidFill(new ThemableColor(GreenPalette.Palette.AlternativeColor));
        }

        private static CellStyle NuevoEstilo(Estilos tipo)
        {
            string nombre = tipo.ToString();
            CellStyle estilo = workbook.Styles.Add(nombre);
            nombresEstilos.Add(tipo, nombre);
            return estilo;
        }

        private static string DarEstilo(Estilos tipo)
        {
            return nombresEstilos[tipo];
        }
    }
}
