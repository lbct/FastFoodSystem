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
    /// Lógica de interacción para ErrorPopUp.xaml
    /// </summary>
    public partial class ErrorPopUp : SystemPopUpClass
    {
        private Action exitAction;

        public ErrorPopUp()
        {
            InitializeComponent();
        }

        public void SetMsg(string text, Action action = null)
        {
            exitAction = action;
            textMessage.Inlines.Clear();
            textMessage.Inlines.Add(new Run(text));
        }

        private void ok_button_Click(object sender, RoutedEventArgs e)
        {
            App.CloseSystemPopUp();
            exitAction?.Invoke();
        }
    }
}
