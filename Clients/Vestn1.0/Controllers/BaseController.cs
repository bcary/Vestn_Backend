using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.Routing;
using System.Net;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using Entity;
using Accessor;

namespace UserClientMembers.Controllers
{
    public abstract class BaseController : Controller
    {

        //this local method will replace all '\n' in a value with '<br />'. This is done so content can be entered by users with \n (common textarea input) but displayed back as <br />
        protected string StripNewLineAndReplaceWithLineBreaks(string value)
        {
            return value.Replace("\n", "<br />");
        }

        protected string AddSuccessHeader(string preResponse, bool addResponseMessageQuotes = false)
        {
            if (addResponseMessageQuotes)
            {
                preResponse = "{\"Response\":" + "\"" + preResponse + "\"";
            }
            else
            {
                preResponse = "{\"Response\":" + preResponse;
            }
            preResponse = preResponse + ",\"Success\": true,\"Message\": \"Success\"}";
            return preResponse;
        }

        protected string Serialize(object o)
        {
            string returnVal;
            System.Runtime.Serialization.Json.DataContractJsonSerializer serializer = new System.Runtime.Serialization.Json.DataContractJsonSerializer(o.GetType());
            using (MemoryStream ms = new MemoryStream())
            {
                serializer.WriteObject(ms, o);
                returnVal = Encoding.Default.GetString(ms.ToArray());

                if (returnVal == "[]")
                {
                    throw new Exception("No Information Found");
                }
            }
            return returnVal;
        }

        protected string AddErrorHeader(string message, int code)
        {
            AnalyticsAccessor aa = new AnalyticsAccessor();
            aa.CreateAnalytic("Error_Returned", DateTime.Now, "user", message);
            return "{\"Error\":{\"Code\":"  + @code.ToString() + ",\"Message\": \"" + @message + "\"},\"Success\": false,\"Reponse\":null}";
        }

        protected string RenderPartialViewToString()
        {
            return RenderPartialViewToString(null, null);
        }

        protected string RenderPartialViewToString(string viewName)
        {
            return RenderPartialViewToString(viewName, null);
        }

        protected string RenderPartialViewToString(object model)
        {
            return RenderPartialViewToString(null, model);
        }

        protected string RenderPartialViewToString(string viewName, object model)
        {
            if (string.IsNullOrEmpty(viewName))
                viewName = ControllerContext.RouteData.GetRequiredString("action");

            ViewData.Model = model;

            using (StringWriter sw = new StringWriter())
            {
                ViewEngineResult viewResult = ViewEngines.Engines.FindPartialView(ControllerContext, viewName);
                ViewContext viewContext = new ViewContext(ControllerContext, viewResult.View, ViewData, TempData, sw);
                viewResult.View.Render(viewContext, sw);

                return sw.GetStringBuilder().ToString();
            }
        }

    }

    public class AllowCrossSiteJsonAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            HttpResponseBase res = filterContext.RequestContext.HttpContext.Response;

            //string toParse = new StreamReader(filterContext.RequestContext.HttpContext.Request.InputStream).ReadToEnd();
            //string inputText = filterContext.RequestContext.HttpContext.Request.InputStream.ToString();
            //JavaScriptSerializer serializer = new JavaScriptSerializer();
            ////serializer.Deserialize(
            //// filterContext.RequestContext.HttpContext.Request.InputStream.


            //string[] stringParams = toParse.Split('&');
            //foreach (string parameter in stringParams)
            //{
            //    string[] kvPair = parameter.Split('=');
            //    string key = kvPair[0];
            //    string value = kvPair[1];
            //    var a = serializer.Deserialize<JsonModels.Artifact>(value);
            //    Type type = filterContext.ActionParameters[key].GetType();
            //    if (type != null)
            //    {
            //        if (type == typeof(int))
            //        {
            //            filterContext.ActionParameters[key] = Int32.Parse(value);
            //        }
            //        else if (type == typeof(string))
            //        {
            //            filterContext.ActionParameters[key] = value;
            //        }
            //        else if (type == typeof(bool))
            //        {
            //            filterContext.ActionParameters[key] = bool.Parse(value);
            //        }
            //    }

            //}

            //List<string> mylist = new List<string>();
            //mylist.Add("test");

            res.AddHeader("Access-Control-Allow-Origin", "*");
            if (filterContext.RequestContext.HttpContext.Request.RequestType.Equals("OPTIONS", StringComparison.InvariantCultureIgnoreCase))
            {
                res.AddHeader("Access-Control-Allow-Methods", "POST, PUT");
                res.AddHeader("Access-Control-Allow-Headers", "X-Requested-With");
                res.AddHeader("Access-Control-Allow-Headers", "X-Request");
                res.AddHeader("Access-Control-Allow-Headers", "X-File-Name");
                res.AddHeader("Access-Control-Allow-Headers", "X-Mime-Type");
                res.AddHeader("Access-Control-Allow-Headers", "Content-Type");
                //res.AddHeader("Access-Control-Allow-Headers", "Accept");
                res.AddHeader("Access-Control-Max-Age", "86400"); //caching this policy for 1 day
            }
            base.OnActionExecuting(filterContext);
        }
    }

}
