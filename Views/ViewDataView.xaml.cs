using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using MySql.Data.MySqlClient;

namespace Billing_Management_System.Views
{
    /// <summary>
    /// Interaction logic for ViewDataView.xaml
    /// </summary>
    public partial class ViewDataView : Page
    {
        public ViewDataView()
        {
            InitializeComponent();
            LoadTables();
        }

        // ================= LOAD TABLE LIST =================
        private void LoadTables()
        {
            try
            {
                using (var con = Billing_Management_System.DatabaseHelper.GetConnection())
                {
                    con.Open();

                    string query = "SHOW TABLES";
                    using (var cmd = new MySqlCommand(query, con))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            cmbTables.Items.Add(reader[0].ToString());
                        }
                    }
                }

                if (cmbTables.Items.Count > 0)
                    cmbTables.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading tables: " + ex.Message);
            }
        }

        // ================= LOAD SELECTED TABLE DATA =================
        private void LoadTableData(string tableName)
        {
            try
            {
                using (var con = Billing_Management_System.DatabaseHelper.GetConnection())
                {
                    con.Open();

                    string query = $"SELECT * FROM {tableName}";
                    using (var adapter = new MySqlDataAdapter(query, con))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        dgData.ItemsSource = dt.DefaultView;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading data: " + ex.Message);
            }
        }

        // ================= TABLE SELECTION =================
        private void cmbTables_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbTables.SelectedItem != null)
            {
                LoadTableData(cmbTables.SelectedItem.ToString());
            }
        }

        // ================= REFRESH BUTTON =================
        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            if (cmbTables.SelectedItem != null)
            {
                LoadTableData(cmbTables.SelectedItem.ToString());
            }
        }

        // ================= EDIT SELECTED RECORD =================
        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            if (cmbTables.SelectedItem == null)
            {
                MessageBox.Show("Select a table first.");
                return;
            }

            var table = cmbTables.SelectedItem.ToString();
            if (table.ToLower() != "bills")
            {
                MessageBox.Show("Editing is only supported for the 'bills' table.");
                return;
            }

            if (dgData.SelectedItem == null)
            {
                MessageBox.Show("Select a bill row to edit.");
                return;
            }

            var row = (System.Data.DataRowView)dgData.SelectedItem;
            long id = Convert.ToInt64(row["Id"]);

            // navigate MainFrame to AddDataView for editing
            var main = Application.Current.MainWindow as MainWindow;
            if (main != null)
            {
                main.MainFrame.Navigate(new AddDataView(id));
            }
        }
    }
}
