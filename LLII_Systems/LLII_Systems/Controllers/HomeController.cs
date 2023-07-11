using LLII_Systems.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LLII_Systems.Helpers;
using System.Web.Security;

namespace LLII_Systems.Controllers
{
    [RoutePrefix("")]
    public class HomeController : Controller
    {
        [Route("")]
        [Authorize]
        public ActionResult Index()
        {
            if (MySession.Current.Username == null)
            {
                FormsAuthentication.SignOut();
                Session.Abandon();
                TempData["errorMessage"] = "Try again";
                return RedirectToAction("", "login");
            }
            else
            {
                //MedExpress_EmployeePortal.Controllers.ProfileController PF = new MedExpress_EmployeePortal.Controllers.ProfileController();
                //PF.GenerateProfile(model);
             
                return View();
            }

         
        
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        
    }
}