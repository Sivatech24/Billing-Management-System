using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MySql.Data.MySqlClient;
// Pdf export removed to avoid GDI/System.Drawing dependency in this build
using Billing_Management_System.Models;

namespace Billing_Management_System.Views
{
    public partial class PrintView : Page
    {
        private ObservableCollection<BillItem> _items =
            new ObservableCollection<BillItem>();

        private long _billId;

        public PrintView(long billId)
        {
            InitializeComponent();
            _billId = billId;
            LoadBill(_billId);
        }

        // ================= LOAD BILL =================
        private void LoadBill(long billId)
        {
            _items.Clear();

            using (var con = Billing_Management_System.DatabaseHelper.GetConnection())
            {
                con.Open();

                // -------- LOAD BILL HEADER --------
                string billQuery = "SELECT * FROM Bills WHERE Id=@Id";
                using (var cmd = new MySqlCommand(billQuery, con))
                {
                    cmd.Parameters.AddWithValue("@Id", billId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            txtInvoiceNo.Text = $"Invoice No: {reader["Id"]}";
                            txtInvoiceDate.Text = $"Date: {Convert.ToDateTime(reader["BillDate"]):dd-MM-yyyy}";
                            txtCustomerName.Text = reader["CustomerName"].ToString();
                        }
                    }
                }

                // -------- LOAD BILL ITEMS --------
                string itemQuery = "SELECT * FROM BillItems WHERE BillId=@BillId";
                using (var cmd = new MySqlCommand(itemQuery, con))
                {
                    cmd.Parameters.AddWithValue("@BillId", billId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            _items.Add(new BillItem
                            {
                                ItemName = reader["ItemName"].ToString(),
                                Qty = Convert.ToInt32(reader["Qty"]),
                                Price = Convert.ToDecimal(reader["Price"]),
                                Cgst = Convert.ToDecimal(reader["Cgst"]),
                                Sgst = Convert.ToDecimal(reader["Sgst"]),
                                Total = Convert.ToDecimal(reader["Total"])
                            });
                        }
                    }
                }
            }

            dgInvoiceItems.ItemsSource = _items;

            // -------- CALCULATE TOTAL --------
            decimal subtotal = 0;
            foreach (var item in _items)
                subtotal += item.Total;

            txtSubtotal.Text = $"Subtotal: ₹ {subtotal:F2}";
            txtTax.Text = "";
            txtGrandTotal.Text = $"Grand Total: ₹ {subtotal:F2}";
        }

        // ================= PRINT =================
        public void PrintInvoice()
        {
            PrintDialog pd = new PrintDialog();
            if (pd.ShowDialog() == true)
            {
                pd.PrintVisual(this, "Invoice Print");
            }
        }

        // ================= EXPORT IMAGE =================
        public void ExportAsImage(string filePath)
        {
            Size size = new Size(ActualWidth, ActualHeight);
            Measure(size);
            Arrange(new Rect(size));

            RenderTargetBitmap rtb = new RenderTargetBitmap(
                (int)size.Width,
                (int)size.Height,
                96,
                96,
                PixelFormats.Pbgra32);

            rtb.Render(this);

            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(rtb));

            using (FileStream fs = new FileStream(filePath, FileMode.Create))
            {
                encoder.Save(fs);
            }
        }

        // ================= EXPORT PDF =================
        // PDF export removed because it depends on GDI/System.Drawing which isn't available
        // in the current build configuration. Provide a simple fallback that exports
        // the invoice as an image and saves with .png extension when requested.
        public void ExportAsPdf(string filePath)
        {
            // If caller expects PDF, save an image with .png extension instead
            var pngPath = Path.ChangeExtension(filePath, ".png");
            ExportAsImage(pngPath);
        }
    }
}