using Billing_Management_System;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Billing_Management_System
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text?.Trim();
            string password = txtPassword.Password; // PasswordBox

            if (AuthenticateUser(username, password))
            {
                var main = new MainWindow();
                Application.Current.MainWindow = main; // ensure app main window is set
                main.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Invalid username or password.", "Login failed", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private bool AuthenticateUser(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                return false;

            string hashed = SecurityHelper.HashPassword(password);

            try
            {
                using (MySqlConnection con = DatabaseHelper.GetConnection())
                {
                    con.Open();
                    string query = "SELECT COUNT(1) FROM Users WHERE Username = @Username AND PasswordHash = @PasswordHash";
                    using (MySqlCommand cmd = new MySqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@Username", username);
                        cmd.Parameters.AddWithValue("@PasswordHash", hashed);
                        object result = cmd.ExecuteScalar();
                        if (result != null && Convert.ToInt32(result) > 0)
                            return true;
                    }
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("Database error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return false;
        }

        private void CreateAccount_Click(object sender, RoutedEventArgs e)
        {
            var register = new RegisterWindow();
            register.Owner = this;
            // Open as modal dialog so user returns to this login window after registering or cancelling
            register.ShowDialog();
            // When the dialog closes, control returns here and the login window remains open
        }
    }
}