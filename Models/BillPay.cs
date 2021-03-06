//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Assignment2Basic.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class BillPay
    {
        public int BillPayID { get; set; }
        public int AccountNumber { get; set; }
        public int PayeeID { get; set; }
        public decimal Amount { get; set; }
        public System.DateTime ScheduleDate { get; set; }
        public string Period { get; set; }
        public System.DateTime ModifyDate { get; set; }
    
        public virtual Account Account { get; set; }
        public virtual Payee Payee { get; set; }
    }
}
