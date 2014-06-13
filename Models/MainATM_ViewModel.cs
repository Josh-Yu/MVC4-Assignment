using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Assignment2Basic.Models
{
    public class MainATM_ViewModel
    {
        public int CustomerID { get; set; }
        public string CustomerName { get; set; }

        public IEnumerable<SelectListItem> AccountList { get; set; }
        public IEnumerable<SelectListItem> AllAccountList { get; set; }
        public IEnumerable<SelectListItem> AllPayeeList { get; set; }

        public class PeriodType
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }


        public IEnumerable<PeriodType> PeriodList =
            new List<PeriodType>
            {
                new PeriodType { Name = "One Time", Id = 0},
                new PeriodType { Name = "Daily", Id = 1},
                new PeriodType { Name = "Monthly", Id = 2},
                new PeriodType { Name = "Annual", Id = 3}
            };

        [Required(ErrorMessage = "An account must be selected")]
        public int ToAccountNumber { get; set; }
        [Required(ErrorMessage = "An account must be selected")]
        public int FromAccountNumber { get; set; }
       
        [Required(ErrorMessage = "A payee must be selected")]
        public int Payee { get; set; }
        
        [Range(0.01, int.MaxValue, ErrorMessage = "Amount must be greater than 0.00")]
        public decimal Amount { get; set; }
        public string Comment { get; set; }
        public string ScheduledDate { get; set; }
        public string Period { get; set; }

        public string Message { get; set; }
        public string AccountBalanceMessage { get; set; }
    }


}