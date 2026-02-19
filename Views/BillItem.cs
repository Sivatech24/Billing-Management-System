using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Billing_Management_System.Models
{
    //internal class BillItem
    //{
    //}
    public class BillItem
    {
        public long Id { get; set; }
        public long BillId { get; set; }
        public string ItemName { get; set; }
        public decimal Price { get; set; }
        public int Qty { get; set; }
        public decimal Cgst { get; set; }
        public decimal Sgst { get; set; }
        public decimal Total { get; set; }
    }
}
