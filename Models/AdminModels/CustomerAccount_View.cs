using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace Assignment2Basic.Models.AdminModels
{   //This represents a view type that the grid view is suppose to display. This contains a model of variables from 2 different tables.
    //This view model will be created in the fly and populated by linq
    public class CustomerAccount_View
    {   [DisplayName("Account Number")]
        public int MyAccountNumber { get; set; }

        [DisplayName("Acc Type)")]
        public string MyAccountType { get; set; }

        [DisplayName("Customer ID")]
        public int MyCustomerID { get; set; }

        [DisplayName("Full Name")]
        public string MyCustomerName { get; set; }

        [DisplayName("Last Modified Date")]
        public DateTime MyModifyDate { get; set; }
        
        //Can add more information if needed, very flexible. Just add the properties and linq statement


    }
}