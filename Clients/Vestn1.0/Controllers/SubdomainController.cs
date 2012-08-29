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
                    string redirectURL = "http://vestn.com";
                    return Redirect(redirectURL);
                }
                else if (user.isPublic == 0)
                {
                    string redirectURL = "http://vestn.com/#splash=404";
                    return Redirect(redirectURL);
                }
                else
                {
                    string redirectURL = "http://vestn.com/#profile=" + user.id.ToString();
                    return Redirect(redirectURL);
                }
            }
            catch (Exception ex)
            {
                string redirectURL = "http://vestn.com";
                return Redirect(redirectURL);
            }
        }
    }
}
