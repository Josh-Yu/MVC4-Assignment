using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Assignment2Basic.Models.AdminModels
{
    //This is the viewmodel that will have the list of users in the system and the associated transactions list for the user
    //when the form is reposted back to the view
    public class CustomerListTransactions_ViewModel
    {
        //This property is set when the form is posted intially with the row id
        public string selectedAccount;
        //List of all accounts in the system for the user to choose
        public List<Account> ListAccount = new List<Account>();

        //List of transactions for the user to choose
        public List<Transaction> ListTransaction = new List<Transaction>();
    }
}