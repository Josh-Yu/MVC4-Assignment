using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Assignment2Basic.Models
{
    public class ATMTransfer_ViewModel
    {
        public int CustomerID { get; set; }
        public string CustomerName { get; set; }

        public IEnumerable<SelectListItem> AccountList { get; set; }
        public IEnumerable<SelectListItem> AllAccountList { get; set; }

        [Required(ErrorMessage = "An account must be selected")]
        public int ToAccountNumber { get; set; }
        [Required(ErrorMessage = "An account must be selected")]
        public int FromAccountNumber { get; set; }

        [Range(0.01, int.MaxValue, ErrorMessage = "Amount must be greater than 0.00")]
        public decimal Amount { get; set; }
        public string Comment { get; set; }

        public string Message { get; set; }
        public string AccountBalanceMessage { get; set; }
    }
}