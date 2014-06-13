using Assignment2Basic.Models.AdminModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Assignment2Basic.Models.AdminModels
{
    public class AccountList_ViewModel
    {
        //This list simply holds all the accounts in the system
        //List will be populated with account objects by the admin controller
        public List<CustomerAccount_View> ListAccounts = new List<CustomerAccount_View>();
    }
}