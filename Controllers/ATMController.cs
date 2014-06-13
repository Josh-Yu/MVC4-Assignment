using Assignment2Basic.Business_Rules;
using Assignment2Basic.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebMatrix.WebData;

namespace Assignment2Basic.Controllers
{
    public class ATMController : Controller
    {
        // GET: /ATM/
        [HttpGet]
        [Authorize(Roles = "Customer")]
        public ActionResult Index()
        {
            HttpContext.Session["accountNumber"] = null;
            ATM_ViewModel models = new ATM_ViewModel();
            return View(models);
        }

        [HttpPost]
        public ActionResult Index(ATM_ViewModel models)
        {
            switch (models.SelectedValue)
            {
                case "0":
                    return RedirectToAction("Transfer", "ATM");
                case "1":
                    return RedirectToAction("Deposit", "ATM");
                case "2":
                    return RedirectToAction("Withdrawal", "ATM");
                case "3":
                    return RedirectToAction("PayBills", "ATM");
                case "4":
                    return RedirectToAction("BillSchedule", "ATM"); 
                default:
                    break;
            }
            return View();

        }

        /*Deposit********************************************************/
        [HttpGet]
        [Authorize(Roles = "Customer")]
        public ActionResult Deposit()
        {
            int sessionUserID = WebSecurity.GetUserId(User.Identity.Name);
            Bank bank = new Bank();
            return View(bank.Deposit(sessionUserID));
        }

        [HttpPost]
        [Authorize(Roles = "Customer")]
        [ValidateAntiForgeryToken]
        public ActionResult Deposit(ATMDeposit_ViewModel models)
        {
            int sessionUserID = WebSecurity.GetUserId(User.Identity.Name);
            Bank bank = new Bank();

            ATMDeposit_ViewModel depositView;

            if (ModelState.IsValid)
            {
                depositView = bank.DepositPost(sessionUserID, models);
            }
            else
            {
                return Deposit();
            }

            return View(depositView);

        }


        /*Withdrawal********************************************************/
        [HttpGet]
        [Authorize(Roles = "Customer")]
        public ActionResult Withdrawal()
        {
            int sessionUserID = WebSecurity.GetUserId(User.Identity.Name);
            Bank bank = new Bank();
            return View(bank.Withdrawal(sessionUserID));
        }

        [HttpPost]
        [Authorize(Roles = "Customer")]
        [ValidateAntiForgeryToken]
        public ActionResult Withdrawal(ATMWithdraw_ViewModel models)
        {
            int sessionUserID = WebSecurity.GetUserId(User.Identity.Name);
            Bank bank = new Bank();

            ATMWithdraw_ViewModel withdrawalView;

            if (ModelState.IsValid)
            {
                withdrawalView = bank.WithdrawalPost(sessionUserID, models);
            }
            else
            {
                return Withdrawal();
            }

            return View(withdrawalView);
        }


        /*Transfer********************************************************/
        [HttpGet]
        [Authorize(Roles = "Customer")]
        public ActionResult Transfer()
        {
            int sessionUserID = WebSecurity.GetUserId(User.Identity.Name);
            Bank bank = new Bank();
            return View(bank.Transfer(sessionUserID));
        }

        [HttpPost]
        [Authorize(Roles = "Customer")]
        [ValidateAntiForgeryToken]
        public ActionResult Transfer(ATMTransfer_ViewModel models)
        {
            int sessionUserID = WebSecurity.GetUserId(User.Identity.Name);
            Bank bank = new Bank();

            ATMTransfer_ViewModel transferView;

            if (ModelState.IsValid)
            {
                transferView = bank.TransferPost(sessionUserID, models);
                HttpContext.Session["fromAccountNumber"] = null;
            }
            else
            {
                HttpContext.Session["fromAccountNumber"] = models.FromAccountNumber;
                return Transfer();
            }

            return View(models);
        }

        /*PayBills********************************************************/
        [HttpGet]
        [Authorize(Roles = "Customer")]
        public ActionResult PayBills()
        {
            int sessionUserID = WebSecurity.GetUserId(User.Identity.Name);
            Bank bank = new Bank();
            return View(bank.PayBills(sessionUserID));
        }
        
        [HttpPost]
        [Authorize(Roles = "Customer")]
        [ValidateAntiForgeryToken]
        public ActionResult PayBills(ATMPayBills_ViewModel models)
        {
            int sessionUserID = WebSecurity.GetUserId(User.Identity.Name);
            Bank bank = new Bank();

            ATMPayBills_ViewModel payBillView;

            if (ModelState.IsValid)
            {
                payBillView = bank.PayBillsPost(sessionUserID, models);
            }
            else
            {
                return PayBills();
            }

            return View(payBillView);
        }

        /*BillSchedule********************************************************/
        // This handles the view where the user initially wants to scheduled a job
        [HttpGet]
        [Authorize(Roles = "Customer")]
        public ActionResult BillSchedule()
        {
            int sessionUserID = WebSecurity.GetUserId(User.Identity.Name);
            Bank bank = new Bank();

            return View(bank.billSchedule(sessionUserID));
        }

        // This handles the view to display the list of all scheduled jobs for the user in a grid layout
        [HttpPost]
        [Authorize(Roles = "Customer")]
        [ValidateAntiForgeryToken]
        public ActionResult BillSchedule(ATMBillSchedule_ViewModel models)
        {

            int sessionUserID = WebSecurity.GetUserId(User.Identity.Name);
            Bank bank = new Bank();

            if (ModelState.IsValid)
            {
                HttpContext.Session["accountNumber"] = null;
                return View(bank.billSchedulePost(sessionUserID, models));
            }
            else
            {
                return BillSchedule();
            }
        }

        //BillScheduleUpdate
        //This handles the view where the user sees a grid of all the scheduled jobs created by them 
        [HttpGet]
        [Authorize(Roles = "Customer")]
        public ActionResult BillScheduleUpdate(string id)
        {
            int sessionUserID = WebSecurity.GetUserId(User.Identity.Name);
            Bank bank = new Bank();
            if (id == null)
            {
                return View(bank.billUpdateGet(sessionUserID, null));
            }
            else
            {
                return View(bank.billUpdateGet(sessionUserID, id));
            }
        }

        [HttpPost]
        [Authorize(Roles = "Customer")]
        [ValidateAntiForgeryToken]
        // This handles the view when a user chooses to modify the scheduled job when via the grid
        public ActionResult BillScheduleUpdate(ATMPayBillsSchedule_ViewModel models, string id)
        {
            int sessionUserID = WebSecurity.GetUserId(User.Identity.Name);
            Bank bank = new Bank();

            HttpContext.Session["accountNumber"] = models.AccountNumber;

            if (ModelState.IsValid)
            {
                if (id == null)
                {
                  return View(bank.billUpdatePost(sessionUserID, models, null));
                }
                else
                {
                models.message = "Schedule Bill Had Been Updated";
                return View(bank.billUpdatePost(sessionUserID, models, id));
                }
            }
            else
            {
                return BillScheduleUpdate(id);
            }
        }
    }
}

