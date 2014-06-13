using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Assignment2Basic.Models
{
    public class Statement_ViewModel
    {
        public int CustomerID { get; set; }
        [Required(ErrorMessage = "An account must be selected")]
        public int AccountNumber { get; set; }
        public string AccountTypes { get; set; }
        public decimal Balance { get; set; }
        public IEnumerable<SelectListItem> accountList { get; set; }
        public List<Transaction> tranList { get; set; }
        public string retrieveMessage { get; set; }
    }
}