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
    using System.Collections.Generic;
    
    public partial class Product
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Product()
        {
            this.PurchaseDetails = new HashSet<PurchaseDetail>();
            this.SaleDetails = new HashSet<SaleDetail>();
            this.Providers = new HashSet<Provider>();
        }
    
        public int Id { get; set; }
        public int CategoryTypeId { get; set; }
        public string Description { get; set; }
        public decimal SaleValue { get; set; }
        public string ImagePath { get; set; }
        public decimal SaleDiscount { get; set; }
        public bool Hide { get; set; }
        public bool HideForSales { get; set; }
    
        public virtual CategoryType CategoryType { get; set; }
        public virtual Combo Combo { get; set; }
        public virtual CompoundProduct CompoundProduct { get; set; }
        public virtual FoodInput FoodInput { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PurchaseDetail> PurchaseDetails { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SaleDetail> SaleDetails { get; set; }
        public virtual SimpleProduct SimpleProduct { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Provider> Providers { get; set; }
    }
}
