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
        public static Login CurrentLogin { get; private set; }

        public async static Task Logout()
        {
            await App.RunAsync(() => 
            {
                CurrentLogin.EndDateTime = DateTime.Now;
                App.Database.SaveChanges();
            });
            CurrentLogin = null;
        }

        public static async Task<bool> Login(string username, string password)
        {
            bool login = false;
            using (MD5 md5Hash = MD5.Create())
            {
                string hash = GetMd5Hash(md5Hash, password);
                var user = await App.RunAsync(() => 
                    App.Database.Users
                    .FirstOrDefault(u => u.Username.Equals(username) 
                    && u.Password.Equals(hash.ToLower().Trim()))
                );
                if (user != null)
                {
                    login = true;
                    var lastLogin = await App.RunAsync(() => 
                    {
                        return (from log in App.Database.Logins
                                orderby log.Id descending
                                select log).FirstOrDefault();
                                   
                    });
                    if (lastLogin != null && lastLogin.EndDateTime == null)
                        CurrentLogin = lastLogin;
                    else
                    {
                        CurrentLogin = new Login()
                        {
                            StartDateTime = DateTime.Now,
                            UserId = user.Id
                        };
                        login = await App.RunAsync(() => 
                        {
                            App.Database.Logins.Add(CurrentLogin);
                            App.Database.SaveChanges();
                        });
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
