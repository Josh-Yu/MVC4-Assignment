using Assignment2Basic.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebMatrix.WebData;

namespace Assignment2Basic.Controllers
{
    [InitializeSimpleMembership]
    public class HomeController : Controller
    {
        //
        // GET: /Home/
        public ActionResult Index()
        {
            HttpContext.Session["accountNumber"] = null;

            //ONLY NEED TO DO ONCE- Simply adding users to the membership database
            //WebSecurity.CreateUserAndAccount("user1", "abc");
            // WebSecurity.CreateUserAndAccount("lucas.ang", "abc123");
            //WebSecurity.CreateUserAndAccount("admin", "abc123");

            // System.Web.Security.Roles.CreateRole("Administrator");
            //System.Web.Security.Roles.CreateRole("Customer");
            // System.Web.Security.Roles.AddUserToRole("lucas.ang", "Customer");

            //System.Web.Security.Roles.AddUserToRole("admin", "Administrator");
            return View();
        }
        [Authorize(Roles = "Admin")] //TESTING ROLES
        public ActionResult About()
        {
            ViewBag.Message = "National Wealth Bank Group was based in Victoria Australia. HeartQuarters are in the heart" +
            " of Melbourne. NWAB is an organization with over 8,000,000 customers and 20,000 staffs which operate globally. " +
            "We have always understood the customers need and outcome a financial solutions that meet their needs." +
            " We provide customers with quality of services, reasonable fees and charges and guidance and advice.";

            return View();
        }


        public ActionResult Contact()
        { return View(); }


    }
}
