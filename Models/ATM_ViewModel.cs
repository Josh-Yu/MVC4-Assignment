using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Assignment2Basic.Models
{
    //This class is used for the dropdownlist
    public class TransactionType
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class ATM_ViewModel
    {
        public string SelectedValue { get; set; }
        public  IEnumerable<TransactionType> ListItems =
            new List<TransactionType>
            {
                new TransactionType { Name = "Transfer", Id = 0},
                new TransactionType { Name = "Deposit", Id = 1},
                new TransactionType { Name = "Withdraw", Id = 2},
                new TransactionType { Name = "Pay My Bills", Id = 3},
                new TransactionType { Name = "Pay Bill Schedule", Id = 4}
            };

        

    }
}