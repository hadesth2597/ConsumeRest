using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Datalayer;

namespace Taller.Controllers
{
    public class LoginController : Controller
    {
        private escuelaEntities db = new escuelaEntities();
        // GET: Login
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Validate(string email, string name)
        {
            users us = db.users.Where(x => x.us_email == email && x.us_name == name).FirstOrDefault();
            if (us == null)
            {
                return Json(0, JsonRequestBehavior.AllowGet);
            }
            else
            {
                Session["name"] = name;
                Session["is_logged"] = 1;
                return Json(1, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}