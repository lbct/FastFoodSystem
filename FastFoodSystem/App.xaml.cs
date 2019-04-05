using FastFoodSystem.Database;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace FastFoodSystem
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static DatabaseEntities databaseEntities { get; private set; }

        public App()
        {
           this.InitializeComponent();
            databaseEntities = new DatabaseEntities();
            MessageBox.Show(databaseEntities.Database.Connection.State.ToString());
        }
    }
}
