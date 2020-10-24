using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastFoodSystem.Database
{
    public partial class SaleOrder
    {
        public int StateIndex
        {
            get => ((int)OrderStateId) - 1;
        }

        public bool IsEnable
        {
            get => OrderStateId != 2;
        }
    }
}
