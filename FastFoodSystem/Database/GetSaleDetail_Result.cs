//------------------------------------------------------------------------------
// <auto-generated>
//     Este código se generó a partir de una plantilla.
//
//     Los cambios manuales en este archivo pueden causar un comportamiento inesperado de la aplicación.
//     Los cambios manuales en este archivo se sobrescribirán si se regenera el código.
// </auto-generated>
//------------------------------------------------------------------------------

namespace FastFoodSystem.Database
{
    using System;
    
    public partial class GetSaleDetail_Result
    {
        public long SaleId { get; set; }
        public long SaleDetailId { get; set; }
        public int ProductId { get; set; }
        public string SystemUser { get; set; }
        public string ClientName { get; set; }
        public string ClientNit { get; set; }
        public System.DateTime DateTime { get; set; }
        public string ProductDescription { get; set; }
        public int Units { get; set; }
        public Nullable<decimal> TotalValue { get; set; }
        public Nullable<decimal> TotalCost { get; set; }
    }
}
