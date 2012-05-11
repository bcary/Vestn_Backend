using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Manager;
using Entity;

namespace Controllers
{
    public class ResourcesController : BaseController
    {
        /// <summary>
        /// gets a list of tags for users to pick from
        /// limit is not currently used but could be added to limit the number of tags returned
        /// </summary>
        /// <param name="int limit"></param>
        /// <returns>returns a json object with a list of tag string values</returns>
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
