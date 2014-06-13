using Assignment2Basic.Business_Rules;
using Assignment2Basic.Models;
using Assignment2Basic.Models.AdminModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Telerik.Web.Mvc.Extensions;
using Telerik.Web.Mvc;
using Telerik.Web.Mvc.UI;
using System.Web.Routing;
using System.Diagnostics;
using Assignment2Basic.Repository;

namespace Assignment2Basic.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class AdminController : Controller
    {
        CentralRepo Repo = new CentralRepo();
        /* GET: /Admin/
         * Displays the admin page home
         */
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }
        /* Displays a grid with all the accounts in the system 
         * with various information related to the account
         */
        [HttpGet]
        public ActionResult ViewAccountDetails()
        {
            var models = new AccountList_ViewModel();

            /*LINQ statement to select * from customer and account tables.
             * Each record will create a CustomerAccount object with information from both tables
             * The query will return a IEnurable collection of CustomerAccount_View objects/types
             * The CustomerAccount_View is a POCO with properties from 2 db tables
             */
            NWBAEntities db = new NWBAEntities();
            var query = (from x in db.Accounts
                         join y in db.Customers on x.CustomerID equals y.CustomerID
                         select new CustomerAccount_View()
                         {
                             MyAccountNumber = x.AccountNumber,
                             MyAccountType = x.AccountType,
                             MyCustomerID = y.CustomerID,
                             MyCustomerName = y.CustomerName,
                             MyModifyDate = x.ModifyDate
                         });


            //Adding each account record to the viewmodel list which will passed to the view
            foreach (var i in query)
            {
                models.ListAccounts.Add(i);
            }
            return View(models);
        }

        /*Displays a grid transactions for a specific user in the system
         * Intially model (accountlist) is sent to the view then the user slects the row which
         * Passes the id back to the controller method to populate the transaction list then passes the viewmodel
         * Back to the view
         */
        [HttpGet]
        public ActionResult ViewTransactions(string id)
        {
            var models = new CustomerListTransactions_ViewModel();
            NWBAEntities db = new NWBAEntities();
            var AccountList = (from x in db.Accounts
                               select x).ToList();
            models.ListAccount = AccountList;

            //Intially, when loaded,no user has been selected
            if (string.IsNullOrEmpty(id))
            {
                models.selectedAccount = "";
            }
            models.selectedAccount = id;
            //Convert require since LINQ does not support casting inline
            int idInt = Convert.ToInt32(id);
            // Returns a list of transations that matches the account number
            var TransactionList = (from x in db.Transactions
                                   where x.AccountNumber == idInt
                                   select x).ToList();
            models.ListTransaction = TransactionList;
            return View(models);
        }

        //Displays and returns a list/model to the view with a list of 
        [HttpGet]
        public ActionResult ChangeUserDetails()
        {
            NWBAEntities db = new NWBAEntities();
            var CustomerLinqList = (from x in db.Customers
                                    select new EditProfile_ViewModel()
                                    {
                                        CustomerID = x.CustomerID,
                                        CustomerName = x.CustomerName,
                                        TFN = x.TFN,
                                        Address = x.Address,
                                        City = x.City,
                                        State = x.State,
                                        PostCode = x.PostCode,
                                        Phone = x.Phone,
                                    }).ToList();
            return View(CustomerLinqList);
        }

        /* Displays a grid of all the current schduled jobs in the system
         * Internally, it will read the billpays table for any current existing record of billpay
         */
        [HttpGet]
        public ActionResult StopScheduledPayments()
        {
            Debug.WriteLine("Inside Stop Schduled payment View Model in Admin Controller");

            //This model will contains 2 lists which are needed to populate the 2 grids for this view
            var models = new BillPayList_ViewModel(); 


            //Request a list of current active scheduled jobs(BillPayID's) from the scheduler 
            Billpay billPayObject = new Billpay();
            List<int> currentActiveBillPayJobs = billPayObject.GetAllActiveJobs();

            //LINQ statement to return billPayee_view objects inside an iquerable
            //This linq will return only billpays for current jobs that have been scheduled because of the passed in currentBillPayJobs
            //Each record will create a CustomerAccount object with information from both tables
            //The query will return a IEnurable collection of CustomerAccount_View objects/types
            //The CustomerAccount_View is a POCO with properties from 2 db tables
            using (NWBAEntities db = new NWBAEntities())
            {
                var query = (from x in db.BillPays
                             join y in db.Payees on x.PayeeID equals y.PayeeID
                             where currentActiveBillPayJobs.Contains(x.BillPayID)
                             select new BillPayPayee_View()
                             {
                                 MyBillPayID = x.BillPayID,
                                 MyAccountNumber = x.AccountNumber,
                                 MyPayeeID = x.PayeeID,
                                 MyAmount = x.Amount,
                                 MyScheduleDate = x.ScheduleDate,
                                 MyPeriod = x.Period,
                                 MyModifyDate = x.ModifyDate,
                                 MyPayeeName = y.PayeeName
                             });

                //Adding each account record to the viewmodel list which will passed to the view
                //Cheap and nasty way instead of using .toList
                foreach (var i in query)
                {
                    models.ActiveBPayJobsGrid.Add(i);
                }

                //Request a list of current Paused scheduled jobs(BillPayID's) from the scheduler 
                List<int> currentPausedBillPayJobs = billPayObject.GetAllPausedJobs();
               

                var query2 = (from x in db.BillPays
                             join y in db.Payees on x.PayeeID equals y.PayeeID
                              where currentPausedBillPayJobs.Contains(x.BillPayID)
                             select new BillPayPayee_View()
                             {
                                 MyBillPayID = x.BillPayID,
                                 MyAccountNumber = x.AccountNumber,
                                 MyPayeeID = x.PayeeID,
                                 MyAmount = x.Amount,
                                 MyScheduleDate = x.ScheduleDate,
                                 MyPeriod = x.Period,
                                 MyModifyDate = x.ModifyDate,
                                 MyPayeeName = y.PayeeName
                             });


                //Adding each account record to the viewmodel list which will passed to the view
                //Cheap and nasty way instead of using .toList
                foreach (var i in query2)
                {
                    models.PausedBPayJobsGrid.Add(i);
                }
                return View(models);
            }
        }


        /* This method is called when the form is posted when the user performs the update operation on the record.
         * Internally, a new viewmodel is created then populated with the data passed in from the view and validation is checked
         * with data annotations at the same time with the method tryupdateModel()
         * If validation passes, then record in DB is updated.
         */
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult GridUpdate(int Customerid)
        {
            //Above, id passed in represents the unique primary key for each row, more properties can be passed in if 
            // needed eg (int id,string CustomerName)


            //Creating another viewmodel so it can be used to data validation
            //need to assign it the customer id passed from the view so the object can be passed to the repositry method to add to db
            //TO BE IMPLEMENTED LATER, AT the moment, doing nothing major
            var customerObject = new EditProfile_ViewModel
            {
                CustomerID = Convert.ToInt32(Customerid)
            };

            //Perform model binding from the view to the newly created viewmodel then validate if passes data annotations
            //Reference: http://msdn.microsoft.com/en-us/library/system.web.mvc.controller.tryupdatemodel%28v=vs.108%29.aspx
            if (TryUpdateModel(customerObject))
            {
                NWBAEntities db = new NWBAEntities();
                Debug.WriteLine("Update Validation Passed");
                Customer customerRecord = (from x in db.Customers
                                           where x.CustomerID.Equals(Customerid)
                                           select x).SingleOrDefault();

                customerRecord.CustomerName = customerObject.CustomerName;
                customerRecord.TFN = customerObject.TFN;
                customerRecord.Address = customerObject.Address;
                customerRecord.City = customerObject.City;
                customerRecord.State = customerObject.State;
                customerRecord.PostCode = customerObject.PostCode;
                customerRecord.Phone = customerObject.Phone;

                db.SaveChanges();
                Debug.WriteLine("Updated Record in DB");
                return RedirectToAction("ChangeUserDetails", this.GridRouteValues());
            }

            Debug.WriteLine("Update Validation Failed");
            return RedirectToAction("ChangeUserDetails", this.GridRouteValues());
        }

           /*THis method is called when the user presses the Suspend button on a billpay record on the view they would like to suspend.
            * The Billpay ID of the associated BillPay record is passed to the method 
            * 
            */
        
        public ActionResult GridPauseJob(string id)
        {
            Debug.WriteLine("Request to suspend Job " + id);
            Billpay bill = new Billpay();
            bill.SuspendJob(id);
           
            return RedirectToAction("StopScheduledPayments", this.GridRouteValues());
        }


        /*This method is called when the user presses the Resume Button on the billpay record on the grid.
         * The associated row with the BillpayID is passed to the method which calls the billpay business 
         * object to resume the billpay job
         */
        public ActionResult GridResumeJob(string id)
        {
            Debug.WriteLine("Request to Resume Job " + id);
            Billpay bill = new Billpay();
            bill.ResumeJob(id);

            return RedirectToAction("StopScheduledPayments", this.GridRouteValues());
        }


        
        /*
         * This is POSTED when a user chooses to delete a scheduled job from the system via the view/grid
         * The post will contain the id of the coresponding record on the grid for deletion (BillpayID)
         * Calls the remove job from the business object  and passes the billpay ID
         */
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult GridDelete(string id)
        {
            Billpay bill = new Billpay();
            if (bill.removeJob(id))
            {
                //returns a http 302 - redirect and passes the grid settings such as page, grouping set by user during session
                return RedirectToAction("StopScheduledPayments", this.GridRouteValues());
            } 
            return RedirectToAction("StopScheduledPayments", this.GridRouteValues());
        }
    }
}
