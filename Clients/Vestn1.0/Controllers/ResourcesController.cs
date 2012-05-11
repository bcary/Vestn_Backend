using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Manager;
using Entity;

namespace UserClientMembers.Controllers
{
    public class ResourcesController : BaseController
    {
        [HttpGet]
        public ActionResult AutocompleteTags(int limit)
        {
            TagManager tm = new TagManager();
            List<sTag> stags = new List<sTag>();
            stags = tm.GetAllSTags();
            List<string> tags = new List<string>();
            foreach (sTag s in stags)
            {
                tags.Add(s.value);
            }
            return Json(tags, JsonRequestBehavior.AllowGet);
        }

    }
}
