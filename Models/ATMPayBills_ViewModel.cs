using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Assignment2Basic.Models
{
    public class ATMPayBills_ViewModel
    {
        public int CustomerID { get; set; }
        public string CustomerName { get; set; }

        public IEnumerable<SelectListItem> AccountList { get; set; }
        public IEnumerable<SelectListItem> AllAccountList { get; set; }
        public IEnumerable<SelectListItem> AllPayeeList { get; set; }
        
        public IEnumerable<PeriodType> PeriodList =
            new List<PeriodType>
            {
                new PeriodType { Name = "Once off", Id = "S"},
                new PeriodType { Name = "Monthly", Id = "M"},
                new PeriodType { Name = "Quarterly", Id = "Q"},
                new PeriodType { Name = "Annually", Id = "Y"}
            };

        [Required(ErrorMessage = "An account must be selected")]
        public int FromAccountNumber { get; set; }
       
        [Required(ErrorMessage = "A payee must be selected")]
        public int Payee { get; set; }
        
        [Range(0.01, int.MaxValue, ErrorMessage = "Amount must be greater than 0.00")]
        public decimal Amount { get; set; }
        public string Comment { get; set; }

        [Required(ErrorMessage = "Scheduled date cannot be empty")]
        //http://stackoverflow.com/questions/14640893/regular-expression-for-date-format-dd-mm-yyyy
        //[RegularExpression(@"^(((0[1-9]|[12]\d|3[01])/(0[13578]|1[02])/((19|[2-9]\d)\d{2}))|((0[1-9]|[12]\d|30)/(0[13456789]|1[012])/((19|[2-9]\d)\d{2}))|((0[1-9]|1\d|2[0-8])/02/((19|[2-9]\d)\d{2}))|(29/02/((1[6-9]|[2-9]\d)(0[48]|[2468][048]|[13579][26])|((16|[2468][048]|[3579][26])00))))$", ErrorMessage = "Schedule date must be of format dd/MM/YYYY")]
        [DataType(DataType.Date)]
        public DateTime ScheduledDate { get; set; }

        [Required(ErrorMessage = "A period must be selected")]
        public string Period { get; set; }

        public string Message { get; set; }
        public string AccountBalanceMessage { get; set; }
    }

    public class PeriodType
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}