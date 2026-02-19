using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Billing_Management_System.Views
{
    //internal class Bill
    //{
    //}
    public class Bill
    {
        public long Id { get; set; }
        public string CustomerName { get; set; }
        public decimal Amount { get; set; }
        public DateTime BillDate { get; set; }
    }
}
