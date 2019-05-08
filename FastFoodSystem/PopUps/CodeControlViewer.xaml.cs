using FastFoodSystem.Scripts;
using System;
using System.Collections.Generic;
using System.Drawing;
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

namespace FastFoodSystem.PopUps
{
    /// <summary>
    /// Lógica de interacción para CodeControlViewer.xaml
    /// </summary>
    public partial class CodeControlViewer : SystemPopUpClass
    {
        private Action action;
        //private Renderer renderer = new Renderer(15);

        public CodeControlViewer()
        {
            InitializeComponent();
        }

        public void Init(string controlCode, Action action)
        {
            this.action = action;
            qrcode.Text = controlCode;
            qr_code_text.Text = controlCode;
        }

        private void Close_button_Click(object sender, RoutedEventArgs e)
        {
            App.CloseSystemPopUp();
            action?.Invoke();
        }

        private void Qr_code_text_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(qrcode != null)
                qrcode.Text = qr_code_text.Text;
        }
    }
}
