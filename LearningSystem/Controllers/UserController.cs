using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LearningSystem.Models;
using LearningSystem.ViewModel;
using RestSharp;

namespace LearningSystem.Controllers
{
    public class UserController : Controller
    {
        private dbEntities _db = new dbEntities();

        [HttpGet]
        public ActionResult Index()
        {
            var userId = System.Web.HttpContext.Current.User.Identity.Name;
            if (userId == null)
                return View("../Auth/Login");

            int userIdInt = Int32.Parse(userId);

            var data = _db.zooms.Where(x => x.user_id == userIdInt && x.is_active == 1).FirstOrDefault();
            return View(data);
        }

        public ActionResult authZoom()
        {

            return new RedirectResult("https://zoom.us/oauth/authorize?response_type=code&client_id=xxx&redirect_uri=https://localhost:44378");
        }

        public ActionResult unauthZoom()
        {
            var userId = Int32.Parse(System.Web.HttpContext.Current.User.Identity.Name);
            var data = _db.zooms.Where(x => x.user_id == userId && x.is_active == 1).FirstOrDefault();
            if (data != null)
            {
                data.is_active = 0;
                _db.SaveChanges();
            }

            return View();
        }


        public ActionResult Success()
        {
            return View("Success");
        }


    }
}