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
    /// Lógica de interacción para ConfirmPopUp.xaml
    /// </summary>
    public partial class ConfirmPopUp : SystemPopUpClass
    {
        private Action action;
        private Action cancel;

        public ConfirmPopUp()
        {
            InitializeComponent();
        }

        public void Init(string msg, Action action, Action cancel = null)
        {
            textMessage.Inlines.Clear();
            textMessage.Inlines.Add(new Run(msg));
            this.action = action;
            this.cancel = cancel;
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            App.CloseSystemPopUp();
            action?.Invoke();
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            App.CloseSystemPopUp();
            cancel?.Invoke();
        }
    }
}
