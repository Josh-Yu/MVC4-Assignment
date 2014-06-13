using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.Entity;
using Assignment2Basic.Models;
using WebMatrix.WebData;
using Assignment2Basic.Business_Rules;

/*
 * This controller will be serving the view the transactions in the system 
 */
namespace Assignment2Basic.Controllers
{
    [Authorize(Roles = "Customer")]
    public class StatementController : Controller
    {
        public ActionResult StatementView()
        {
            int SessionUserID = WebSecurity.GetUserId(User.Identity.Name);

            Bank bank = new Bank();

            return View(bank.getStatement(SessionUserID));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult StatementView(Statement_ViewModel models)
        {
            int SessionUserID = WebSecurity.GetUserId(User.Identity.Name);

            Bank bank = new Bank();

            if (ModelState.IsValid)
            {
                HttpContext.Session["accountNumber"] = models.AccountNumber;
                return View(bank.postStatement(SessionUserID, models));
            }
            else
            {
                HttpContext.Session["accountNumber"] = null;
                return StatementView();
            }
        }
    }
}