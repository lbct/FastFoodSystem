using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace FastFoodSystem.Scripts
{
    public class PageContainer : Grid
    {
        private SystemPageClass currentPage;
        private SystemPopUpClass currentPopUp;

        private Border main;
        private Border second;

        public PageContainer() : base()
        {
            main = new Border();
            second = new Border();
            second.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#7F000000"));
            second.Visibility = System.Windows.Visibility.Collapsed;
            Children.Add(main);
            Children.Add(second);
        }

        public void SetPopUp(SystemPopUpClass popUp)
        {
            currentPopUp = popUp;
            second.Child = popUp;
        }

        public void SetPage(SystemPageClass page)
        {
            currentPage = page;
            main.Child = page;
        }

        public T GetPage<T>() where T : SystemPageClass
        {
            return currentPage as T;
        }

        public T GetPopUp<T>() where T : SystemPopUpClass
        {
            return currentPopUp as T;
        }

        public void ShowPopUp()
        {
            if (currentPage != null)
                currentPage.IsEnabled = false;
            if (currentPopUp != null)
                second.Visibility = System.Windows.Visibility.Visible;
        }

        public void HidePopUp()
        {
            if (currentPage != null)
                currentPage.IsEnabled = true;
            if (currentPopUp != null)
                second.Visibility = System.Windows.Visibility.Collapsed;
        }
    }
}
