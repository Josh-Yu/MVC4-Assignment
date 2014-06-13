using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Assignment2Basic.Models.AdminModels
{
    //Has properties from the BillPay table and the Payee ID
    //Custom object type
    public class BillPayPayee_View
    {
        public int MyBillPayID{ get; set; }
        public int MyPayeeID { get; set; }
        public int MyAccountNumber { get; set; }
        public decimal MyAmount { get; set; }
        public DateTime MyScheduleDate { get; set; }
        public string MyPeriod { get; set; }
        public DateTime MyModifyDate { get; set; }
        public string MyPayeeName { get; set; }
    }
}