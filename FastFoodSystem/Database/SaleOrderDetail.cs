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
    
    public partial class SaleOrderDetail
    {
        public long Id { get; set; }
        public long SaleOrderId { get; set; }
        public int ProductId { get; set; }
        public int Units { get; set; }
        public decimal UnitValue { get; set; }
        public decimal UnitCost { get; set; }
        public decimal DiscountValue { get; set; }
    
        public virtual SaleOrder SaleOrder { get; set; }
    }
}
