namespace FastFoodSystem.Database
{
    using System;
    using System.Collections.Generic;

    public class ProductView
    {
        public int Id { get; set; }
        public string ProductType { get; set; }
        public int CategoryTypeId { get; set; }
        public string CategoryDescription { get; set; }
        public string Description { get; set; }
        public decimal UnitSaleValue { get; set; }
        public Nullable<decimal> UnitCost { get; set; }
        public Nullable<int> AvailableUnits { get; set; }
        public decimal SaleDiscount { get; set; }
        public string ImagePath { get; set; }
        public bool HideForSales { get; set; }
    }
}
