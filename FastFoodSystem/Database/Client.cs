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
    
    public partial class Client
    {
        public Client()
        {
            this.Sales = new HashSet<Sale>();
        }
    
        public int Id { get; set; }
        public string Nit { get; set; }
        public string Name { get; set; }
    
        public virtual ICollection<Sale> Sales { get; set; }
    }
}
