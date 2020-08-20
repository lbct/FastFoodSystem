using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastFoodSystem.Scripts
{
    public static class DatabaseSettings
    {
        private static string databaseName = "fast_food_db";
        private static string currentConnectionString; 

        public static string ConnectionString { get => currentConnectionString; }

        private static string GetConnectionString(string database)
        {
            currentConnectionString = ";server=localhost;user id=root;password=root;persistsecurityinfo=True;database="+database;

            return "metadata=res://*/Database.FastFoodDbModel.csdl|res://*/Database.FastFoodDbModel.ssdl|res://*/Database.FastFoodDbModel.msl;"
                + "provider=MySql.Data.MySqlClient;provider connection string=\""+currentConnectionString+"\";";
        }

        private static string GetConnectionString()
        {
            return GetConnectionString(databaseName);
        }

        public static void Configure(string dbName)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.ConnectionStrings.ConnectionStrings.Remove("DatabaseEntities");
            config.ConnectionStrings.ConnectionStrings.Add(new ConnectionStringSettings("DatabaseEntities",
                GetConnectionString(dbName), "System.Data.EntityClient"));
            
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("connectionStrings");
        }

        public static void Configure()
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.ConnectionStrings.ConnectionStrings.Remove("DatabaseEntities");
            
            config.ConnectionStrings.ConnectionStrings.Add(new ConnectionStringSettings("DatabaseEntities",
                GetConnectionString(), "System.Data.EntityClient"));
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("connectionStrings");
        }
    }
}
