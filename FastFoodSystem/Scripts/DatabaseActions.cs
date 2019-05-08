using FastFoodSystem.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastFoodSystem.Scripts
{
    public static class DatabaseActions
    {
        public static async Task<decimal> GetCurrentInBoxCashValue()
        {
            decimal saleData = 0;
            decimal purchaseData = 0;
            decimal cashData = 0;
            try
            {
                var sales = await App.RunAsync(() => App.Database.GetSaleDetailByLogin(UserSession.LoginID).ToArray());
                if (sales.Length > 0)
                    saleData = sales.Sum(s => s.TotalValue).Value;
            }
            catch { }
            try
            {
                var purchases = await App.RunAsync(() => App.Database.GetPurchaseDetailByLogin(UserSession.LoginID).ToArray());
                if (purchases.Length > 0)
                    purchaseData = purchases.Sum(p => p.TotalValue).Value;
            }
            catch { }
            try
            {
                var cashes = await App.RunAsync(() => App.Database.GetCashMovement(UserSession.LoginID).ToArray());
                if (cashes.Length > 0)
                    cashData = cashes.Sum(c => c.Value);
            }
            catch { }
            var login = await App.RunAsync(() => App.Database.Logins.FirstOrDefault(l => l.Id == UserSession.LoginID).StartCashValue);
            return login + saleData + cashData - purchaseData;
        }

        public static async Task IncreaseProductUnits(PurchaseDetail detail)
        {
            await IncreaseProductUnits(detail.ProductId, detail.Units);
        }

        public static async Task ValidateBillInformation()
        {
            var billConfig = await UserSession.GetBillConfig();
            if (string.IsNullOrEmpty(CompanyInformation.CompanyName.Trim()))
                throw new Exception("Debe añadir el nombre de su empresa");
            if (string.IsNullOrEmpty(CompanyInformation.EconomicActivity.Trim()))
                throw new Exception("Debe añadir una actividad económica");
            if (CompanyInformation.CompanyNit.Trim().Length < 5 || !CompanyInformation.CompanyNit.Trim().All(char.IsDigit))
                throw new Exception("Debe añadir un NIT válido");
            if (billConfig.AuthorizationCode.Trim().Length < 5 || !billConfig.AuthorizationCode.Trim().All(char.IsDigit))
                throw new Exception("Debe añadir un número de autorización válido");
            if (billConfig.DosificationCode.Trim().Length != 64)
                throw new Exception("Debe añadir una llave de dosificación válida");
            if (billConfig.LimitEmissionDate < DateTime.Now.Date)
                throw new Exception("Debe añadir una Fecha límite de emisión válida");
        }

        public static async Task IncreaseProductUnits(SaleDetail detail)
        {
            await IncreaseProductUnits(detail.ProductId, detail.Units);
        }

        public static async Task ReduceProductUnits(SaleDetail detail)
        {
            await ReduceProductUnits(detail.ProductId, detail.Units);
        }

        public static async Task ReduceProductUnits(PurchaseDetail detail)
        {
            await ReduceProductUnits(detail.ProductId, detail.Units);
        }

        private static async Task ReduceProductUnits(int productId, int units)
        {
            SimpleProduct simple = await App.RunAsync(() => App.Database.SimpleProducts.FirstOrDefault(sp => sp.Id == productId));
            if (simple != null)
                await ReduceProductUnits(simple, units);
            else
            {
                var foodInput = await App.RunAsync(() => App.Database.FoodInputs.FirstOrDefault(fi => fi.Id == productId));
                if (foodInput != null)
                    await ReduceProductUnits(foodInput, units);
                else
                {
                    var compound = await App.RunAsync(() => App.Database.CompoundProducts.FirstOrDefault(p => p.Id == productId));
                    if (compound != null)
                        await ReduceProductUnits(compound, units);
                    else
                    {
                        var combo = await App.RunAsync(() => App.Database.Comboes.FirstOrDefault(p => p.Id == productId));
                        await ReduceProductUnits(combo, units);
                    }
                }
            }
        }

        private static async Task ReduceProductUnits(Combo combo, int units)
        {
            var foodInputRelations = await App.RunAsync(() => App.Database.FoodInputComboes
                        .Where(r => r.ComboId == combo.Id)
                        .ToArray());
            var simpleRelations = await App.RunAsync(() => App.Database.SimpleProductComboes
            .Where(r => r.ComboId == combo.Id)
            .ToArray());
            var compoundRelations = await App.RunAsync(() => App.Database.CompoundProductComboes
            .Where(r => r.ComboId == combo.Id)
            .ToArray());
            foreach (var relation in foodInputRelations)
            {
                var input = await App.RunAsync(() => App.Database.FoodInputs.FirstOrDefault(fi => fi.Id == relation.FoodInputId));
                await ReduceProductUnits(input, units);
            }
            foreach (var relation in simpleRelations)
            {
                var simpleProd = await App.RunAsync(() => App.Database.SimpleProducts.FirstOrDefault(s => s.Id == relation.SimpleProductId));
                await ReduceProductUnits(simpleProd, units);
            }
            foreach (var relation in compoundRelations)
            {
                var comp = await App.RunAsync(() => App.Database.CompoundProducts.FirstOrDefault(c => c.Id == relation.CompoundProductId));
                await ReduceProductUnits(comp, units);
            }
        }

        private static async Task ReduceProductUnits(SimpleProduct simple, int units)
        {
            simple.Units = simple.Units - units;
            await App.RunAsync(() => App.Database.SaveChanges());
        }

        private static async Task ReduceProductUnits(FoodInput foodInput, int units)
        {
            foodInput.Units = foodInput.Units - units;
            await App.RunAsync(() => App.Database.SaveChanges());
        }

        private static async Task ReduceProductUnits(CompoundProduct compound, int units)
        {
            var relations = await App.RunAsync(() => App.Database.CompoundProductFoodInputs
                        .Where(r => r.CompoundProductId == compound.Id)
                        .ToArray());
            foreach (var relation in relations)
            {
                var input = await App.RunAsync(() => App.Database.FoodInputs.FirstOrDefault(fi => fi.Id == relation.FoodInputId));
                await ReduceProductUnits(input, units * relation.RequiredUnits);
            }
        }

        private static async Task IncreaseProductUnits(int productId, int units)
        {
            SimpleProduct simple = await App.RunAsync(() => App.Database.SimpleProducts.FirstOrDefault(sp => sp.Id == productId));
            if (simple != null)
                await IncreaseProductUnits(simple, units);
            else
            {
                var foodInput = await App.RunAsync(() => App.Database.FoodInputs.FirstOrDefault(fi => fi.Id == productId));
                if (foodInput != null)
                    await IncreaseProductUnits(foodInput, units);
                else
                {
                    var compound = await App.RunAsync(() => App.Database.CompoundProducts.FirstOrDefault(p => p.Id == productId));
                    if (compound != null)
                        await IncreaseProductUnits(compound, units);
                    else
                    {
                        var combo = await App.RunAsync(() => App.Database.Comboes.FirstOrDefault(p => p.Id == productId));
                        await IncreaseProductUnits(combo, units);
                    }
                }
            }
        }

        private static async Task IncreaseProductUnits(Combo combo, int units)
        {
            var foodInputRelations = await App.RunAsync(() => App.Database.FoodInputComboes
                        .Where(r => r.ComboId == combo.Id)
                        .ToArray());
            var simpleRelations = await App.RunAsync(() => App.Database.SimpleProductComboes
            .Where(r => r.ComboId == combo.Id)
            .ToArray());
            var compoundRelations = await App.RunAsync(() => App.Database.CompoundProductComboes
            .Where(r => r.ComboId == combo.Id)
            .ToArray());
            foreach (var relation in foodInputRelations)
            {
                var input = await App.RunAsync(() => App.Database.FoodInputs.FirstOrDefault(fi => fi.Id == relation.FoodInputId));
                await IncreaseProductUnits(input, units);
            }
            foreach (var relation in simpleRelations)
            {
                var simpleProd = await App.RunAsync(() => App.Database.SimpleProducts.FirstOrDefault(s => s.Id == relation.SimpleProductId));
                await IncreaseProductUnits(simpleProd, units);
            }
            foreach (var relation in compoundRelations)
            {
                var comp = await App.RunAsync(() => App.Database.CompoundProducts.FirstOrDefault(c => c.Id == relation.CompoundProductId));
                await IncreaseProductUnits(comp, units);
            }
        }

        private static async Task IncreaseProductUnits(SimpleProduct simple, int units)
        {
            simple.Units = simple.Units + units;
            await App.RunAsync(() => App.Database.SaveChanges());
        }

        private static async Task IncreaseProductUnits(FoodInput foodInput, int units)
        {
            foodInput.Units = foodInput.Units + units;
            await App.RunAsync(() => App.Database.SaveChanges());
        }

        private static async Task IncreaseProductUnits(CompoundProduct compound, int units)
        {
            var relations = await App.RunAsync(() => App.Database.CompoundProductFoodInputs
                        .Where(r => r.CompoundProductId == compound.Id)
                        .ToArray());
            foreach (var relation in relations)
            {
                var input = await App.RunAsync(() => App.Database.FoodInputs.FirstOrDefault(fi => fi.Id == relation.FoodInputId));
                await IncreaseProductUnits(input, units * relation.RequiredUnits);
            }
        }
    }

    public static class PixelUnitFactor
    {
        public const double Px = 1.0;
        public const double Inch = 96.0;
        public const double Cm = 37.7952755905512;
        public const double Pt = 1.33333333333333;

        public static double CmToPx(double cm)
        {
            return cm * Cm;
        }

        public static double PxToCm(double px)
        {
            return px / Cm;
        }
    }

    public static class PaperSize
    {
        public static double BillWidth
        {
            get
            {
                return PixelUnitFactor.CmToPx(7.5);
            }
        }
    }
}
