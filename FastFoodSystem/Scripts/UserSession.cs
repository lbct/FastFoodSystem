using FastFoodSystem.Database;
using FastFoodSystem.PopUps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FastFoodSystem.Scripts
{
    public static class UserSession
    {
        public static long LoginID { get; private set; }
        public static int BillConfigId { get; private set; }
        private static int dailyId = 0;
        private static int dailyOrderId;

        public static string LastUsername { get; private set; }
        public static string LastPassword { get; private set; }
        
        public static int DailyId
        {
            get
            {
                return ++dailyId;
            }
        }
        public static int DailyOrderId
        {
            get; set;
        } = 1;

        public async static Task Logout()
        {
            LoadPopUp.SetText("Cerrando sesión...");
            var endCash = await DatabaseActions.GetCurrentInBoxCashValue();
            await App.RunAsync(() => 
            {
                var currentLog = App.Database.Logins.FirstOrDefault(log => log.Id == LoginID);
                currentLog.EndDateTime = DateTime.Now;
                currentLog.EndCashValue = endCash;
                App.Database.SaveChanges();
            });
            LoadPopUp.SetText("Cargando...");
            dailyId = 0;
        }

        public static async Task<BillConfig> GetBillConfig()
        {
            return await App.RunAsync(() => App.Database.BillConfigs
            .FirstOrDefault(b => b.Id == BillConfigId));
        }

        public static async Task<bool> Login(string username, string password)
        {
            LastUsername = username;
            LastPassword = password;
            LoadPopUp.SetText("Iniciando sesión...");
            bool login = false;
            using (MD5 md5Hash = MD5.Create())
            {
                string hash = GetMd5Hash(md5Hash, password);
                var user = await App.RunAsync(() => 
                    App.Database.Users
                    .FirstOrDefault(u => u.Username.Equals(username) 
                    && u.Password.Equals(hash.ToLower().Trim()) && !u.Hide)
                );
                LoadPopUp.SetText("Cargando...");
                if (user != null)
                {
                    login = true;
                    
                    await LoadProducts();

                    var lastLogin = await App.RunAsync(() => 
                    {
                        return App.Database.Logins.OrderByDescending(l => l.Id).FirstOrDefault();
                    });
                    var lastBillConfig = await App.RunAsync(() => 
                    {
                        return App.Database.BillConfigs.OrderByDescending(b => b.Id).FirstOrDefault();
                    });
                    if(lastBillConfig == null)
                    {
                        BillConfig billConfig = new BillConfig()
                        {
                            AuthorizationCode = "",
                            CurrentBillNumber = 1,
                            DosificationCode = "",
                            LimitEmissionDate = DateTime.Now
                        };
                        await App.RunAsync(() =>
                        {
                            App.Database.BillConfigs.Add(billConfig);
                            App.Database.SaveChanges();
                        });
                        lastBillConfig = billConfig;
                    }
                    BillConfigId = lastBillConfig.Id;

                    if (lastLogin != null && lastLogin.EndDateTime == null)
                    {
                        LoginID = lastLogin.Id;
                        dailyOrderId = await App.RunAsync(() =>
                        {
                            int maxId = 0;
                            try
                            {
                                maxId = App.Database.SaleOrders.Count() <= 0 ? 0 : App.Database.SaleOrders
                                .Where(o => o.LoginId == LoginID)
                                .Max(o => o.DailyId);
                            }
                            catch { }
                            return maxId;
                        });
                    }
                    else
                    {
                        decimal startCash = 0;
                        if (lastLogin != null && lastLogin.EndCashValue != null)
                            startCash = lastLogin.EndCashValue.Value;
                        var currentLogin = new Login()
                        {
                            StartDateTime = DateTime.Now,
                            UserId = user.Id,
                            StartCashValue = startCash
                        };
                        login = await App.RunAsync(() =>
                        {
                            App.Database.Logins.Add(currentLogin);
                            App.Database.SaveChanges();
                        });
                        dailyOrderId = 0;
                        if (login)
                            LoginID = currentLogin.Id;
                    }
                }
            }
            return login;
        }

        private static async Task LoadProducts()
        {
            LoadPopUp.SetText("Obteniendo imagenes de productos...");
            var productsImages = await App.RunAsync(() =>
            {
                return App.Database.Products.Where(p => !p.Hide).Select(p => p.ImagePath).ToArray();
            });

            await App.RunAsync(() =>
            {
                int max = productsImages.Length;
                int count = 0;
                foreach (var img in productsImages)
                {
                    LoadPopUp.SetText("Cargando imágenes (" + count + "/" + max + ")");
                    ImageManager.LoadBitmap(img, 180);
                    count++;
                }
                LoadPopUp.SetText("Cargando...");
            });
        }

        public static string GetMd5Hash(MD5 md5Hash, string input)
        {

            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }
    }
}
