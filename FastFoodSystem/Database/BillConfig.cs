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
    
    public partial class BillConfig
    {
        public int Id { get; set; }
        public string AuthorizationCode { get; set; }
        public string DosificationCode { get; set; }
        public int CurrentBillNumber { get; set; }
        public System.DateTime LimitEmissionDate { get; set; }
    }
}
