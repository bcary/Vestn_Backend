using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Manager;
using Entity;
using Models;
using System.Web.Security;
using Accessor;

namespace Controllers
{
    public class HomeController : BaseController
    {
        UserManager userManager = new UserManager();
        LogAccessor logAccessor = new LogAccessor();

        public ActionResult Index()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return View();
            }
            else
            {
                return RedirectToAction("Home", "User", new { userName = User.Identity.Name });
            }
        }

        public ActionResult Splash()
        {
            return View();
        }

        public ActionResult CreateNewAccount()
        {
            return View();
        }

        public ActionResult About()
        {
            return View();
        }

        [Authorize(Users = "errorView,hpham,bcary,pbaylog,skyler,Kyle,jake")]
        public ActionResult ErrorView()
        {
            List<Log> logs = new List<Log>();
            List<int> ids = new List<int>();
            List<DateTime> eventTimes = new List<DateTime>();
            List<string> locations = new List<string>();
            List<string> exceptions = new List<string>();


            //logs = logAccessor.GetLogs();

            List<ErrorModel> errorModelList = new List<ErrorModel>();

            int i = 0;
            foreach (Log log in logs)
            {
                ErrorModel er = new ErrorModel(log);
                errorModelList.Add(er);
            }



            return View(errorModelList);
        }

        public string TestMe()
        {
            return "success";
        }
    }
}
