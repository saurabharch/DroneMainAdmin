using DroneMainAdmin.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DroneMainAdmin.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
      //  [Authorize]
        public ActionResult Index()
        {
            if(User.Identity.IsAuthenticated)
            {
                ViewBag.Message = "Check notification";
            }
            else
            {
                ViewBag.Message = "Welcome Guest";
            }
            return View();
        }
        public ActionResult GetTeams()
        {
            using (Entities dc = new Entities())
            {
                var team = dc.Users.OrderBy(a => a.FirstName).ToList();
                return Json(new { data = team }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}