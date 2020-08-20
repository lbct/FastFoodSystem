﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FastFoodSystem.Scripts
{
    public static class CompanyInformation
    {
        public static string CompanyName { get; set; }
        public static string Direction { get; set; }
        public static string EconomicActivity { get; set; }
        public static string ConsumerLawLegend { get; set; }
        public static string PhoneNumber { get; set; }
        public static string EMail { get; set; }
        public static string CompanyNit { get; set; }

        public static DatabaseConfig[] DatabaseConfigs { get; private set; }
        public static DatabaseConfig SelectedConfig { get; set; }

        public static void ReadDatabaseConfigs()
        {
            string dir = @"C:\" + Assembly.GetEntryAssembly().GetName().Name + @"\SystemInfo";
            string path = Path.Combine(dir, "Config.json");
            try
            {
                DatabaseConfigs = JsonConvert.DeserializeObject<DatabaseConfig[]>(File.ReadAllText(path));
                if (DatabaseConfigs.Length <= 0)
                {
                    DatabaseConfigs = new DatabaseConfig[]
                    {
                        new DatabaseConfig()
                        {
                            Database = "fast_food_db",
                            VisualName = "Big Roll",
                            Code = "BR"
                        }
                    };
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
                DatabaseConfig defaultConfig = new DatabaseConfig()
                {
                    Database = "fast_food_db",
                    VisualName = "Big Roll",
                    Code = "BR"
                };
                DatabaseConfigs = new DatabaseConfig[] { defaultConfig };
                File.WriteAllText(path, JsonConvert.SerializeObject(DatabaseConfigs));
            }
            SelectedConfig = DatabaseConfigs.First();
        }

        public static async Task Init()
        {
            await Task.Factory.StartNew(() =>
            {
                string dir = @"C:\" + Assembly.GetEntryAssembly().GetName().Name + @"\SystemInfo";

                string fileName = "Data.bin";
                string fullPath = Path.Combine(dir, fileName);
                if (File.Exists(fullPath))
                {
                    using (Stream st = new FileStream(fullPath, FileMode.Open))
                    {
                        BinaryFormatter bin = new BinaryFormatter();
                        List<string> list = (List<string>)bin.Deserialize(st);
                        CompanyName = list[0];
                        Direction = list[1];
                        EconomicActivity = list[2];
                        ConsumerLawLegend = list[3];
                        PhoneNumber = list[4];
                        EMail = list[5];
                        CompanyNit = list[6];
                    }
                }
                else
                {
                    CompanyName = 
                    Direction = 
                    EconomicActivity = 
                    ConsumerLawLegend = 
                    PhoneNumber =
                    EMail =
                    CompanyNit = "";
                }
            });
        }

        public static async Task Save()
        {
            await Task.Factory.StartNew(() =>
            {
                string dir = @"C:\" + Assembly.GetEntryAssembly().GetName().Name + @"\SystemInfo";
                string fileName = "Data.bin";
                string fullPath = Path.Combine(dir, fileName);
                Directory.CreateDirectory(dir);
                using (Stream st = new FileStream(fullPath, FileMode.Create))
                {
                    BinaryFormatter bin = new BinaryFormatter();
                    List<string> list = new List<string>() {
                        CompanyName,
                        Direction,
                        EconomicActivity,
                        ConsumerLawLegend,
                        PhoneNumber,
                        EMail,
                        CompanyNit,
                    };
                    bin.Serialize(st, list);
                }
            });
        }
    }
}
