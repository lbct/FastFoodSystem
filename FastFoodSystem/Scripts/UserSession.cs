using FastFoodSystem.Database;
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
        
        public static int DailyId
        {
            get
            {
                return ++dailyId;
            }
        }
        public static int DailyOrderId
        {
            get
            {
                return ++dailyOrderId;
            }
        }

        public async static Task Logout()
        {
            var endCash = await DatabaseActions.GetCurrentInBoxCashValue();
            await App.RunAsync(() => 
            {
                var currentLog = App.Database.Logins.FirstOrDefault(log => log.Id == LoginID);
                currentLog.EndDateTime = DateTime.Now;
                currentLog.EndCashValue = endCash;
                App.Database.SaveChanges();
            });
            dailyId = 0;
        }

        public static async Task<BillConfig> GetBillConfig()
        {
            return await App.RunAsync(() => App.Database.BillConfigs
            .FirstOrDefault(b => b.Id == BillConfigId));
        }

        public static async Task<bool> Login(string username, string password)
        {
            dailyOrderId = await App.RunAsync(() => 
            {
                return App.Database.SaleOrders.Count() <= 0 ? 0 : App.Database.SaleOrders.Max(o => o.DailyId);
            });
            bool login = false;
            using (MD5 md5Hash = MD5.Create())
            {
                string hash = GetMd5Hash(md5Hash, password);
                var user = await App.RunAsync(() => 
                    App.Database.Users
                    .FirstOrDefault(u => u.Username.Equals(username) 
                    && u.Password.Equals(hash.ToLower().Trim()) && !u.Hide)
                );
                if (user != null)
                {
                    login = true;
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
                        LoginID = lastLogin.Id;
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
                        if (login)
                            LoginID = currentLogin.Id;
                    }
                }
            }
            return login;
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
