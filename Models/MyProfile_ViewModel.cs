using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Assignment2Basic.Models
{
    /*THis viewModel is maped to entity framework customer. THis will DTO (Data Transfer Object) to the
     MyProfile page just to display the user information in the system*/
    public class MyProfile_ViewModel
    {
        public string CustomerName { get; set; }
        public string TFN { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostCode { get; set; }
        public string Phone { get; set; }
    }
}