using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Assignment2Basic.Models
{
    public class ATMBillSchedule_ViewModel
    {
        [Required(ErrorMessage = "An account must be selected")]
        public int AccountNumber { get; set; }
        public string CustomerName { get; set; }
        public string AccountTypes { get; set; }
        public IEnumerable<SelectListItem> accountList { get; set; }
        //public List<BillPay> scheduleBillList { get; set; }
        public List<BillingList> scheduleBillList { get; set; }
        public string Message { get; set; }
        public string PayeeName { get; set; }
        public int PayeeID { get; set; }
        public Dictionary<int, string> payeeList { get; set; }
    }

    public class BillingList
    {
        public int BillPayID { get; set; }
        public string PayeeName { get; set; }
        //public int PayeeID { get; set; }
        public double Amount { get; set; }
        //public DateTime ScheduleDate { get; set; }
        public string ScheduleDate { get; set; }
        public string Period { get; set; }
    }
}