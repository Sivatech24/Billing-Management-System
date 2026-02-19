using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;
using MySql.Data.MySqlClient;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using System.Collections.ObjectModel;
using System.IO;
// using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Billing_Management_System.Models;

namespace Billing_Management_System.Views
{
    /// <summary>
    /// Interaction logic for PrintView.xaml
    /// </summary>
    public partial class PrintView : Page
    {
        private ObservableCollection<BillItem> _items
            = new ObservableCollection<BillItem>();

        public PrintView(long billId)
        {
            InitializeComponent();
            LoadBill(billId);
        }

        // ================= LOAD BILL =================
        private void LoadBill(long billId)
        {
            using (var con = Billing_Management_System.DatabaseHelper.GetConnection())
            {
                con.Open();

                // Load bill
                string billQuery = "SELECT * FROM Bills WHERE Id=@Id";
                using (var cmd = new MySqlCommand(billQuery, con))
                {
                    cmd.Parameters.AddWithValue("@Id", billId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            txtInvoiceNo.Text = "Invoice No: " + reader["Id"].ToString();
                            txtInvoiceDate.Text = "Date: " +
                                Convert.ToDateTime(reader["BillDate"]).ToShortDateString();
                            txtCustomerName.Text = reader["CustomerName"].ToString();
                        }
                    }
                }

                // Load items
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

            decimal subtotal = 0;
            foreach (var item in _items)
                subtotal += item.Total;

            txtSubtotal.Text = "Subtotal: ₹ " + subtotal.ToString("F2");
            txtTax.Text = "";
            txtGrandTotal.Text = "Grand Total: ₹ " + subtotal.ToString("F2");
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
            RenderTargetBitmap rtb = new RenderTargetBitmap(
                (int)ActualWidth,
                (int)ActualHeight,
                96, 96,
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
        public void ExportAsPdf(string filePath)
        {
            PdfDocument document = new PdfDocument();
            PdfPage page = document.AddPage();
            XGraphics gfx = XGraphics.FromPdfPage(page);

            XFont font = new XFont("Verdana", 12);

            gfx.DrawString("Invoice", font, XBrushes.Black,
                new XRect(0, 20, page.Width.Point, 20),
                XStringFormats.TopCenter);

            int y = 60;

            foreach (var item in _items)
            {
                gfx.DrawString(
                    $"{item.ItemName}  {item.Qty}  {item.Price}  {item.Total}",
                    font,
                    XBrushes.Black,
                    40,
                    y);

                y += 20;
            }

            document.Save(filePath);
        }
    }
}
