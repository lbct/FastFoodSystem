using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace FastFoodSystem.Database
{
    public partial class OrderState
    {
        public override string ToString()
        {
            return Name;
        }

        public Brush GetColor()
        {
            switch (Id)
            {
                case 1:
                    return Brushes.Yellow;
                case 2:
                    return Brushes.LightGreen;
                case 3:
                    return Brushes.OrangeRed;
                default:
                    return Brushes.WhiteSmoke;
            }
        }
    }
}
