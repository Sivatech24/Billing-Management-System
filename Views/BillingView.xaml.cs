using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Globalization;
using Billing_Management_System.Models;
using MySql.Data.MySqlClient;

namespace Billing_Management_System.Views
{
    /// <summary>
    /// Interaction logic for BillingView.xaml
    /// </summary>
    public partial class BillingView : Page
    {
        private ObservableCollection<BillItem> _items = new ObservableCollection<BillItem>();

        public BillingView()
        {
            InitializeComponent();
            dgItems.ItemsSource = _items;
            LoadBills();
        }

        // ================= ADD ITEM =================
        private void AddItem_Click(object sender, RoutedEventArgs e)
        {
            if (!decimal.TryParse(txtPrice.Text, out decimal price) ||
                !int.TryParse(txtQty.Text, out int qty) ||
                !decimal.TryParse(txtCgst.Text, out decimal cgst) ||
                !decimal.TryParse(txtSgst.Text, out decimal sgst))
            {
                MessageBox.Show("Enter valid numeric values.");
                return;
            }

            decimal subtotal = price * qty;
            decimal cgstAmt = subtotal * (cgst / 100);
            decimal sgstAmt = subtotal * (sgst / 100);
            decimal total = subtotal + cgstAmt + sgstAmt;

            _items.Add(new BillItem
            {
                ItemName = txtItemName.Text,
                Price = price,
                Qty = qty,
                Cgst = cgst,
                Sgst = sgst,
                Total = total
            });

            CalculateTotals();
        }

        // ================= CALCULATE TOTALS =================
        private void CalculateTotals()
        {
            decimal subtotal = 0;
            decimal totalCgst = 0;
            decimal totalSgst = 0;

            foreach (var item in _items)
            {
                decimal itemSubtotal = item.Price * item.Qty;
                subtotal += itemSubtotal;
                totalCgst += itemSubtotal * (item.Cgst / 100);
                totalSgst += itemSubtotal * (item.Sgst / 100);
            }

            decimal grandTotal = subtotal + totalCgst + totalSgst;

            txtSubtotal.Text = subtotal.ToString("F2");
            txtTotalCgst.Text = totalCgst.ToString("F2");
            txtTotalSgst.Text = totalSgst.ToString("F2");
            txtGrandTotal.Text = grandTotal.ToString("F2");
        }

        // ================= SAVE BILL =================
        private void SaveBill_Click(object sender, RoutedEventArgs e)
        {
            if (_items.Count == 0)
            {
                MessageBox.Show("Add at least one item.");
                return;
            }

            try
            {
                using (var con = Billing_Management_System.DatabaseHelper.GetConnection())
                {
                    con.Open();
                    MySqlTransaction transaction = con.BeginTransaction();

                    try
                    {
                        // Insert into bills (use lowercase table name to match DB)
                        string billQuery = "INSERT INTO bills (CustomerName, Amount, BillDate) VALUES (@CustomerName, @Amount, @BillDate)";
                        long billId;

                        using (var cmd = new MySqlCommand(billQuery, con, transaction))
                        {
                            cmd.Parameters.AddWithValue("@CustomerName", txtCustomerName.Text);
                            cmd.Parameters.AddWithValue("@Amount", decimal.Parse(txtGrandTotal.Text));
                            cmd.Parameters.AddWithValue("@BillDate", DateTime.Now);

                            cmd.ExecuteNonQuery();
                            billId = cmd.LastInsertedId;
                        }

                        // Insert billitems (use lowercase table name)
                        foreach (var item in _items)
                        {
                            string itemQuery = @"INSERT INTO billitems 
                                (BillId, ItemName, Price, Qty, Cgst, Sgst, Total)
                                VALUES (@BillId, @ItemName, @Price, @Qty, @Cgst, @Sgst, @Total)";

                            using (var cmd = new MySqlCommand(itemQuery, con, transaction))
                            {
                                cmd.Parameters.AddWithValue("@BillId", billId);
                                cmd.Parameters.AddWithValue("@ItemName", item.ItemName);
                                cmd.Parameters.AddWithValue("@Price", item.Price);
                                cmd.Parameters.AddWithValue("@Qty", item.Qty);
                                cmd.Parameters.AddWithValue("@Cgst", item.Cgst);
                                cmd.Parameters.AddWithValue("@Sgst", item.Sgst);
                                cmd.Parameters.AddWithValue("@Total", item.Total);

                                cmd.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                        MessageBox.Show("Bill Saved Successfully!");
                        _items.Clear();
                        LoadBills();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        // ================= LOAD BILLS =================
        private void LoadBills_Click(object sender, RoutedEventArgs e)
        {
            LoadBills();
        }

        private void LoadBills()
        {
            try
            {
                var bills = new ObservableCollection<Bill>();

                using (var con = Billing_Management_System.DatabaseHelper.GetConnection())
                {
                    con.Open();
                    string query = "SELECT * FROM bills ORDER BY BillDate DESC";

                    using (var cmd = new MySqlCommand(query, con))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var id = reader.GetInt64("Id");

                            // handle nullable CustomerName and BillDate safely
                            var customer = reader["CustomerName"] == DBNull.Value ? string.Empty : reader["CustomerName"].ToString();
                            var amount = reader["Amount"] == DBNull.Value ? 0m : reader.GetDecimal("Amount");
                            DateTime billDate = DateTime.MinValue;
                            if (reader["BillDate"] != DBNull.Value)
                            {
                                billDate = reader.GetDateTime("BillDate");
                            }

                            bills.Add(new Bill
                            {
                                Id = id,
                                CustomerName = customer,
                                Amount = amount,
                                BillDate = billDate
                            });
                        }
                    }
                }

                dgBills.ItemsSource = bills;

                // if there are bills, auto-select the first one so Print can work immediately
                if (bills.Count > 0)
                {
                    dgBills.SelectedIndex = 0;
                }
                else
                {
                    // no bills found - notify the user so they know to create one
                    System.Diagnostics.Debug.WriteLine("No bills found in database.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading bills: " + ex.Message);
            }
        }

        // Expose the currently selected bill to external callers (MainWindow)
        public Bill SelectedBill => dgBills.SelectedItem as Bill;

        private void ClearItems_Click(object sender, RoutedEventArgs e)
        {
            _items.Clear();
            CalculateTotals();
        }

        private void GenerateDay_Click(object sender, RoutedEventArgs e) { }
        private void GenerateWeek_Click(object sender, RoutedEventArgs e) { }
        private void GenerateMonth_Click(object sender, RoutedEventArgs e) { }
    }
}
