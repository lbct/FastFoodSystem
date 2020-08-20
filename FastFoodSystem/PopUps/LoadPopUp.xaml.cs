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

namespace FastFoodSystem.PopUps
{
    /// <summary>
    /// Lógica de interacción para LoadPopUp.xaml
    /// </summary>
    public partial class LoadPopUp : SystemPopUpClass
    {
        private static LoadPopUp _instance;

        public LoadPopUp()
        {
            InitializeComponent();
            _instance = this;
        }

        public static void SetText(string txt)
        {
            App.Current.Dispatcher.Invoke(() => 
            {
                if (_instance != null)
                    _instance.load_label.Content = txt;
            });
        }
    }
}
