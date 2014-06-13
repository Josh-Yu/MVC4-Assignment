using Assignment2Basic.Business_Rules;
using Assignment2Basic.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using WebMatrix.WebData;

/*
 * This controller will be serving the  page where the user will need to log into the system)
 */
namespace Assignment2Basic.Controllers
{
    public class LoginController : Controller
    {
        //
        // GET: /Login/
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpGet]
        [Authorize]
        public ActionResult AdminHome()
        {
            //This retrieves the userID that is associated with the username in the tables
            int SessionUserID = WebSecurity.GetUserId(User.Identity.Name);

            NWBAEntities db = new NWBAEntities();
            var query = (from x in db.Customers
                         where x.CustomerID.Equals(SessionUserID)
                         select x).Single();

            var newModel = new UserHome_ViewModel()
            {
                CustomerName = query.CustomerName
            };

            return View(newModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(Login_ViewModel models, string returnUrl)
        {
            // Lets first check if the Model is valid or not
            if (ModelState.IsValid)
            {  
                    string username = models.UserID;
                    string password = models.Password;

                    bool status = WebSecurity.Login(username, password);
                    if (status)
                    {
                        if (Roles.IsUserInRole(username,"Customer"))
                        {
                           // System.Diagnostics.Debug.WriteLine("Current user is a Customer Role");

                            return RedirectToAction("Index", "ATM");
                        }
                        else if (Roles.IsUserInRole(username,"Administrator"))
                        {
                           // System.Diagnostics.Debug.WriteLine("Current user is a Administrator Role");
                            return RedirectToAction("Index", "Admin");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "The user name or password provided is incorrect.");
                    }
            }

            // ends up here something failed, redisplay form
            return View(models);
        }

        public ActionResult LogOff()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home");
        }
    }
}
