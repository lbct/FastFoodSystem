//------------------------------------------------------------------------------
// <auto-generated>
//    Este código se generó a partir de una plantilla.
//
//    Los cambios manuales en este archivo pueden causar un comportamiento inesperado de la aplicación.
//    Los cambios manuales en este archivo se sobrescribirán si se regenera el código.
// </auto-generated>
//------------------------------------------------------------------------------

namespace FastFoodSystem.Database
{
    using System;
    using System.Collections.Generic;
    
    public partial class Login
    {
        public Login()
        {
            this.CashMovements = new HashSet<CashMovement>();
            this.Purchases = new HashSet<Purchase>();
            this.Sales = new HashSet<Sale>();
            this.SaleOrders = new HashSet<SaleOrder>();
        }
    
        public long Id { get; set; }
        public int UserId { get; set; }
        public System.DateTime StartDateTime { get; set; }
        public Nullable<System.DateTime> EndDateTime { get; set; }
        public decimal StartCashValue { get; set; }
        public Nullable<decimal> EndCashValue { get; set; }
    
        public virtual ICollection<CashMovement> CashMovements { get; set; }
        public virtual User User { get; set; }
        public virtual ICollection<Purchase> Purchases { get; set; }
        public virtual ICollection<Sale> Sales { get; set; }
        public virtual ICollection<SaleOrder> SaleOrders { get; set; }
    }
}
