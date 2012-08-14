using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Manager;
using Entity;

namespace UserClientMembers.Controllers
{
    public class SubdomainController : Controller
    {
        //
        // GET: /Subdomain/

        public ActionResult Index()
        {
            return View();
        }

        public RedirectResult RedirectSubdomain(string subdomain)
        {
            //check if subdomain is user or network
            //TODO check if is a network
            try
            {
                UserManager userManager = new UserManager();
                User user = userManager.GetUserByProfileURL(subdomain);
                if (user == null)
                {
                    string redirectURL = "http://50.17.232.163";
                    return Redirect(redirectURL);
                }
                else
                {
                    string redirectURL = "http://50.17.232.163/#profile=" + user.id.ToString();
                    return Redirect(redirectURL);
                }
            }
            catch (Exception ex)
            {
                return Redirect("http://50.17.232.163");
            }
        }
    }
}
