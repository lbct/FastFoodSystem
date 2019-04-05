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
        public static DatabaseEntities DatabaseEntities { get; private set; }

        public App()
        {
           this.InitializeComponent();
            DatabaseEntities = new DatabaseEntities();
        }
    }
}
