using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Billing_Management_System
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            try
            {
                Billing_Management_System.DatabaseHelper.InitializeDatabase();
            }
            catch (Exception ex)
            {
                // write to console to help debugging connection issues in development
                System.Diagnostics.Debug.WriteLine("Database initialization failed: " + ex.Message);
            }
        }
    }
}
