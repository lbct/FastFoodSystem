using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastFoodSystem.Database
{
    public partial class DatabaseEntities
    {
        public ProductView[] GetProductView()
        {
            return this.Database.SqlQuery<ProductView>("SELECT * from ProductView;").ToArray();
        }

        public decimal GetProductCost(int productId)
        {
            return this.Database.SqlQuery<decimal>(
                "SELECT GetProductCost("+productId+");"
                ).First();
        }

        public int GetProductUnits(int productId)
        {
            return this.Database.SqlQuery<int>(
                "SELECT GetProductUnits("+ productId + ");"
                ).First();
        }

        public string GetProductType(int productId)
        {
            return this.Database.SqlQuery<string>(
                "SELECT GetProductType("+productId+");"
                ).First();
        }

        public virtual IEnumerable<GetCashMovement_Result> GetCashMovement(long login_id)
        {
            return Database.SqlQuery<GetCashMovement_Result>("call GetCashMovement('" + login_id + "');");
        }

        public virtual IEnumerable<GetPurchaseDetail_Result> GetPurchaseDetail(System.DateTime start_date, System.DateTime end_date)
        {
            return Database.SqlQuery<GetPurchaseDetail_Result>("call GetPurchaseDetail('" 
                + start_date.ToString("yyyy-MM-dd") + "', '" + end_date.ToString("yyyy-MM-dd") + "');");
        }

        public virtual IEnumerable<GetPurchaseDetailByLogin_Result> GetPurchaseDetailByLogin(long login_id)
        {
            return Database.SqlQuery<GetPurchaseDetailByLogin_Result>("call GetPurchaseDetailByLogin('" + login_id + "');");
        }

        public virtual IEnumerable<GetSaleDetail_Result> GetSaleDetail(System.DateTime start_date, System.DateTime end_date)
        {
            return Database.SqlQuery<GetSaleDetail_Result>("call GetSaleDetail('"
                + start_date.ToString("yyyy-MM-dd") + "', '" + end_date.ToString("yyyy-MM-dd") + "');");
        }

        public virtual IEnumerable<GetSaleDetailByLogin_Result> GetSaleDetailByLogin(long login_id)
        {
            return Database.SqlQuery<GetSaleDetailByLogin_Result>("call GetSaleDetailByLogin('" + login_id + "');");
        }
    }
}
