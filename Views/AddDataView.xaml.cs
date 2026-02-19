using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;

namespace Billing_Management_System.Views
{
    /// <summary>
    /// Interaction logic for AddDataView.xaml
    /// </summary>
    public partial class AddDataView : Page
    {
        private System.Collections.ObjectModel.ObservableCollection<Billing_Management_System.Models.BillItem> _items =
            new System.Collections.ObjectModel.ObservableCollection<Billing_Management_System.Models.BillItem>();

        private long? _editingBillId;

        public AddDataView()
        {
            InitializeComponent();
            dgItems.ItemsSource = _items;
        }

        // Load an existing bill for editing
        public AddDataView(long billId) : this()
        {
            _editingBillId = billId;
            LoadBill(billId);
        }

        private void LoadBill(long billId)
        {
            _items.Clear();
            using (var con = Billing_Management_System.DatabaseHelper.GetConnection())
            {
                con.Open();

                using (var cmd = new MySql.Data.MySqlClient.MySqlCommand("SELECT * FROM bills WHERE Id=@Id", con))
                {
                    cmd.Parameters.AddWithValue("@Id", billId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            txtCustomerName.Text = reader["CustomerName"] == DBNull.Value ? string.Empty : reader["CustomerName"].ToString();
                        }
                    }
                }

                using (var cmd = new MySql.Data.MySqlClient.MySqlCommand("SELECT * FROM billitems WHERE BillId=@BillId", con))
                {
                    cmd.Parameters.AddWithValue("@BillId", billId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            _items.Add(new Billing_Management_System.Models.BillItem
                            {
                                ItemName = reader["ItemName"].ToString(),
                                Price = Convert.ToDecimal(reader["Price"]),
                                Qty = Convert.ToInt32(reader["Qty"]),
                                Cgst = Convert.ToDecimal(reader["Cgst"]),
                                Sgst = Convert.ToDecimal(reader["Sgst"]),
                                Total = Convert.ToDecimal(reader["Total"])
                            });
                        }
                    }
                }
            }
            dgItems.ItemsSource = _items;
            CalculateTotals();
        }

        private void CalculateTotals()
        {
            decimal subtotal = 0;
            foreach (var it in _items) subtotal += it.Total;
            txtSubtotal.Text = subtotal.ToString("F2");
        }

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

            _items.Add(new Billing_Management_System.Models.BillItem
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

        private void ClearItems_Click(object sender, RoutedEventArgs e)
        {
            _items.Clear();
            CalculateTotals();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var con = Billing_Management_System.DatabaseHelper.GetConnection())
                {
                    con.Open();
                    MySql.Data.MySqlClient.MySqlTransaction tx = con.BeginTransaction();
                    try
                    {
                        long billId;
                        if (_editingBillId.HasValue)
                        {
                            billId = _editingBillId.Value;
                            using (var cmd = new MySql.Data.MySqlClient.MySqlCommand("UPDATE bills SET CustomerName=@CustomerName, Amount=@Amount WHERE Id=@Id", con, tx))
                            {
                                cmd.Parameters.AddWithValue("@CustomerName", txtCustomerName.Text);
                                cmd.Parameters.AddWithValue("@Amount", decimal.Parse(txtSubtotal.Text));
                                cmd.Parameters.AddWithValue("@Id", billId);
                                cmd.ExecuteNonQuery();
                            }

                            // remove existing items then re-insert
                            using (var cmd = new MySql.Data.MySqlClient.MySqlCommand("DELETE FROM billitems WHERE BillId=@BillId", con, tx))
                            {
                                cmd.Parameters.AddWithValue("@BillId", billId);
                                cmd.ExecuteNonQuery();
                            }
                        }
                        else
                        {
                            using (var cmd = new MySql.Data.MySqlClient.MySqlCommand("INSERT INTO bills (CustomerName, Amount, BillDate) VALUES (@CustomerName, @Amount, @BillDate)", con, tx))
                            {
                                cmd.Parameters.AddWithValue("@CustomerName", txtCustomerName.Text);
                                cmd.Parameters.AddWithValue("@Amount", decimal.Parse(txtSubtotal.Text));
                                cmd.Parameters.AddWithValue("@BillDate", DateTime.Now);
                                cmd.ExecuteNonQuery();
                                billId = cmd.LastInsertedId;
                            }
                        }

                        foreach (var it in _items)
                        {
                            using (var cmd = new MySql.Data.MySqlClient.MySqlCommand("INSERT INTO billitems (BillId, ItemName, Price, Qty, Cgst, Sgst, Total) VALUES (@BillId,@ItemName,@Price,@Qty,@Cgst,@Sgst,@Total)", con, tx))
                            {
                                cmd.Parameters.AddWithValue("@BillId", billId);
                                cmd.Parameters.AddWithValue("@ItemName", it.ItemName);
                                cmd.Parameters.AddWithValue("@Price", it.Price);
                                cmd.Parameters.AddWithValue("@Qty", it.Qty);
                                cmd.Parameters.AddWithValue("@Cgst", it.Cgst);
                                cmd.Parameters.AddWithValue("@Sgst", it.Sgst);
                                cmd.Parameters.AddWithValue("@Total", it.Total);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        tx.Commit();
                        MessageBox.Show("Bill saved successfully.");
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving bill: " + ex.Message);
            }
        }
        // Note: parameterless constructor is defined earlier to initialize UI and items.
    }
}
