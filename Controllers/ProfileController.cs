using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Assignment2Basic.Models;
using WebMatrix.WebData;

namespace Assignment2Basic.Controllers
{
    public class ProfileController : Controller
    {
        private NWBAEntities db = new NWBAEntities();

        [HttpGet]
        [Authorize(Roles = "Customer")]
        public ActionResult MyProfile()
        {
            HttpContext.Session["accountNumber"] = null;

            int SessionUserID = WebSecurity.GetUserId(User.Identity.Name);
            //NWBAEntities _db = new NWBAEntities();
            //Comprehensive Linq Syntax
            var query = (from x in db.Customers
                         where x.CustomerID.Equals(SessionUserID)
                         select x).SingleOrDefault(); //<-- Single() is required to return only one object instead of a, list containing 1 object
            //var model = query;
            // var model = new MyProfileViewModel();

            var model = new MyProfile_ViewModel()
            {
                CustomerName = query.CustomerName,
                TFN = query.TFN,
                Address = query.Address,
                City = query.City,
                State = query.State,
                PostCode = query.PostCode,
                Phone = query.Phone,
            };

            return View(model);
        }
        
        [HttpGet]
        [Authorize(Roles = "Customer")]
        public ActionResult Edit()
        {
            //This retrieves the userID that is associated with the username in the tables
            int SessionUserID = WebSecurity.GetUserId(User.Identity.Name);

            //Here im, fetching the information from the db entity. So when the view eg Form is rendered.
            //The relevant textboxes have the information from the DB prefilled. Instead of empty textboxes

            var query = (from x in db.Customers
                         where x.CustomerID.Equals(SessionUserID)
                         select x).Single(); //<-- Single() is required to return only one object instead of a, list containing 1 object
            //creating viewmodel and assinging data from the EF
            var model = new EditProfile_ViewModel()
            {
                CustomerName = query.CustomerName,
                TFN = query.TFN,
                Address = query.Address,
                City = query.City,
                State = query.State,
                PostCode = query.PostCode,
                Phone = query.Phone,
            };
            return View(model);
        }

        //Executed when the form is submited via POST
        [HttpPost]
        [Authorize(Roles = "Customer")]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(EditProfile_ViewModel models) //<=== models is a result of the concept (model binding)
        {
            if (ModelState.IsValid)
            {
                int SessionUserID = WebSecurity.GetUserId(User.Identity.Name);
                //COMMIT CHANGES TO DB
                var query = (from x in db.Customers
                             where x.CustomerID.Equals(SessionUserID)
                             select x).Single();

                query.CustomerName = models.CustomerName;
                query.TFN = models.TFN;
                query.Address = models.Address;
                query.City = models.City;
                query.State = models.State;
                query.PostCode = models.PostCode;
                query.Phone = models.Phone;

                db.SaveChanges();
                return RedirectToAction("MyProfile", "Profile");
            }
            return View(models);
        }

        //Display the change password page via get request
        [HttpGet]
        [Authorize]
        public ActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(ChangePassword_ViewModel models)
        {
            if (ModelState.IsValid)
            {
                //work around, in order to change password, need to know the old password.
                //since not using old password field, need membership provider to reset 
                //the password then use the generated password as the old password

                //Generates a password token for this specific username in the session
                //Requires a username to generate the token for
                string GeneratedPasswordToken = WebSecurity.GeneratePasswordResetToken(User.Identity.Name);
                string username = User.Identity.Name;
                //Resets the password to the new password, usually the token is sent via email to the user
                bool status = WebSecurity.ResetPassword(GeneratedPasswordToken, models.ConfirmPassword);
                if (status)
                {
                    System.Diagnostics.Trace.WriteLine("Password CHange worked");
                    ViewBag.Message = "Password Change SUCCESSFULLY";
                }
                else
                {
                    System.Diagnostics.Trace.WriteLine("Password change didnt work" + User.Identity.Name);
                    ViewBag.Message = "Password Change was UNSUCCESSFUL";
                }

                /*
                var query = (from x in db.Logins
                             where x.CustomerID.Equals(100)
                             select x).Single();

                //query.Password = models.ConfirmPassword;
                db.SaveChanges();
                 */
            }
            return View();
        }
        
        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}