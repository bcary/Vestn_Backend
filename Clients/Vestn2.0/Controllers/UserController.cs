using System;
using System.Globalization;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using Manager;
using Entity;
using Models;
using System.Web.Security;
using System.Web.Routing;
using System.Net;
using System.IO;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using Microsoft.WindowsAzure.ServiceRuntime;
using Accessor;
using Engine;
using System.Runtime.Serialization;


namespace Controllers
{
    public class UserController : BaseController
    {

        UserManager userManager = new UserManager();
        ProjectManager projectManager = new ProjectManager();
        UploadManager uploadManager = new UploadManager();
        CommunicationManager communicationManager = new CommunicationManager();
        LogAccessor logAccessor = new LogAccessor();
        AuthenticaitonEngine authenticationEngine = new AuthenticaitonEngine();

        //User Account Creation

        public ActionResult fileexist()
        {
            return View();
        }

        public JsonResult CheckFileExist(String url)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)System.Net.WebRequest.Create(url);
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    return Json(new { status = response.StatusCode });
                }
            }
            catch (System.Net.WebException)
            {
                return Json(new { status = "File not exist" });
            }
        }

        /// <summary>
        /// LogOn POST function. Authenticates the user, and returns a token value that must be stored by the client application and used on every subsequesnt authorized request
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns>Authenticaiton token</returns>
        [AcceptVerbs("POST", "OPTIONS")]
        public JsonResult LogOn(string username, string password)
        {
            Response.AddHeader("Access-Control-Allow-Origin", "*");
            if (Request.RequestType.Equals("OPTIONS", StringComparison.InvariantCultureIgnoreCase))  //This is a preflight request
            {
                Response.AddHeader("Access-Control-Allow-Methods", "POST, PUT");
                Response.AddHeader("Access-Control-Allow-Headers", "X-Requested-With");
                Response.AddHeader("Access-Control-Allow-Headers", "X-Request");
                Response.AddHeader("Access-Control-Max-Age", "86400"); //caching this policy for 1 day
                return null;
            }
            else
            {
                try
                {
                    User user = userManager.GetUser(username);
                    if (user == null)
                    {
                        user = userManager.GetUserByEmail(username);
                        if (user != null)
                        {
                            username = user.userName;
                        }
                    }
                    if (userManager.ValidateUser(user, password))
                    {

                        AuthenticaitonEngine authEngine = new AuthenticaitonEngine();
                        string token = authEngine.logIn(user.id, user.userName);

                        AnalyticsAccessor aa = new AnalyticsAccessor();
                        aa.CreateAnalytic("User Login", DateTime.Now, user.userName);

                        return Json(new { id = user.id, Success = true, key = token });
                    }
                    else
                    {
                        //return GetFailureMessage("User Information Not Valid");
                        return Json(new { Error = "User Information Not Valid" });
                    }
                }
                catch (Exception ex)
                {
                    logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), ex.ToString());
                    return Json(new { Error = "An unknown error occured" });
                    //return GetFailureMessage("Error");
                }
            }
        }

        /// <summary>
        /// This is the main end point for getting basic user information. 
        /// It takes in a int[] of userIds and a string[] request objects
        /// Here is an example request:
        /// /User/GetUserInformation?id=1&id=2&request=firstName&request=lastName&request=tagLine
        /// requestObjects can contain any of the following strinsg:
        ///     firstName, lastName, title, school, description, tagLine, resume, profilePicture, profilePictureThumbnail, stats, links, experiences, references, tags, projects, todo, recentActivity, connections
        /// </summary>
        /// <param name="List<int> userIds"></param>
        /// <param name="List<string> requestObjects"></param>
        /// <returns>Json Object of UserInformation class</returns>
        /// 
        [AllowCrossSiteJson]
        [HttpGet]
        public string GetUserInformation(int[] id, string[] request, string token)
        {
            //authenticate via token
            int authenticate = authenticationEngine.authenticate(token);
            if (authenticate < 0)
            {
                Response.StatusCode = 500;
                return "Not Authenticated";
            }
            string returnVal;
            try
            {
                bool requestAll = false;
                if (request == null || request.Contains("all"))
                {
                    requestAll = true;
                }

                List<JsonModels.UserInformation> userInformationList = new List<JsonModels.UserInformation>();
                int add = 0;
                foreach (int ID in id)
                {
                    User u = userManager.GetUser(ID);
                    if (u != null)
                    {
                        add = 0;
                        //TODO add company
                        JsonModels.UserInformation ui = new JsonModels.UserInformation();
                        if (requestAll || request.Contains("firstName"))
                        {
                            if (u.firstName != null)
                            {
                                ui.firstName = u.firstName;
                                add = 1;
                            }
                        }
                        if (requestAll || request.Contains("lastName"))
                        {
                            if (u.lastName != null)
                            {
                                ui.lastName = u.lastName;
                                add = 1;
                            }
                        }
                        if (requestAll || request.Contains("connections"))
                        {
                            if (u.connections != null)
                            {
                                ui.connections = u.connections;
                                add = 1;
                            }
                        }
                        if (requestAll || request.Contains("tagLine"))
                        {
                            if (u.tagLine != null)
                            {
                                ui.tagLine = u.tagLine;
                                add = 1;
                            }
                        }
                        if (requestAll || request.Contains("title"))
                        {
                            if (u.title != null)
                            {
                                ui.title = u.title;
                                add = 1;
                            }
                        }
                        if (requestAll || request.Contains("school"))
                        {
                            if (u.school != null)
                            {
                                ui.school = u.school;
                                add = 1;
                            }
                        }
                        if (requestAll || request.Contains("description"))
                        {
                            if (u.description != null)
                            {
                                ui.description = u.description;
                                add = 1;
                            }
                        }
                        if (requestAll || request.Contains("resume"))
                        {
                            if (u.resume != null)
                            {
                                ui.resume = u.resume;
                                add = 1;
                            }
                        }
                        if (requestAll || request.Contains("profilePicture"))
                        {
                            if (u.profilePicture != null)
                            {
                                ui.profilePicture = u.profilePicture;
                                add = 1;
                            }
                        }
                        if (requestAll || request.Contains("profilePictureThumbnail"))
                        {
                            if (u.profilePictureThumbnail != null)
                            {
                                ui.profilePictureThumbnail = u.profilePictureThumbnail;
                                add = 1;
                            }
                        }
                        if (requestAll || request.Contains("stats"))
                        {
                            JsonModels.UserStats stats = userManager.getUserStats(ID);
                            if (stats != null)
                            {
                                ui.stats = stats;
                                add = 1;
                            }
                        }
                        if (requestAll || request.Contains("links"))
                        {
                            JsonModels.Links links = userManager.getUserLinks(ID);
                            if (links != null)
                            {
                                ui.links = links;
                                add = 1;
                            }
                        }
                        //if (requestAll || request.Contains("experiences"))
                        //{
                        //    List<JsonModels.Experience> experiences = userManager.GetUserExperiences(ID);
                        //    if (experiences != null && experiences.Count != 0)
                        //    {
                        //        ui.experiences = experiences;
                        //        add = 1;
                        //    }
                        //}
                        //if (requestAll || request.Contains("references"))
                        //{
                        //    List<JsonModels.Reference> references = userManager.GetUserReferences(ID);
                        //    if (references != null && references.Count != 0)
                        //    {
                        //        ui.references = references;
                        //        add = 1;
                        //    }
                        //}
                        if (requestAll || request.Contains("tags"))
                        {
                            List<JsonModels.UserTag> tags = userManager.GetUserTags(ID);
                            if (tags != null && tags.Count != 0)
                            {
                                ui.tags = tags;
                                add = 1;
                            }
                        }
                        if (requestAll || request.Contains("projects"))
                        {
                            List<JsonModels.ProjectShell> projects = projectManager.GetProjectShells(ID);
                            if (projects != null && projects.Count != 0)
                            {
                                ui.projects = projects;
                                add = 1;
                            }
                        }
                        if (requestAll || request.Contains("todo"))
                        {
                            List<JsonModels.Todo> todoList = userManager.GetTodo(ID);
                            if (todoList != null && todoList.Count != 0)
                            {
                                ui.todo = todoList;
                                add = 1;
                            }
                        }
                        if (requestAll || request.Contains("recentActivity"))
                        {
                            List<JsonModels.RecentActivity> recentActivity = userManager.GetRecentActivity(ID);
                            if (recentActivity != null && recentActivity.Count != 0)
                            {
                                ui.recentActivity = recentActivity;
                                add = 1;
                            }
                        }
                        if (add == 1)
                        {
                            userInformationList.Add(ui);
                        }
                    }
                }

                try
                {
                    returnVal = Serialize(userInformationList);
                }
                catch (Exception exception)
                {
                    return GetFailureMessage(exception.Message);
                }
            }
            catch (Exception e)
            {
                return GetFailureMessage("Bad Request");
            }
            return AddSuccessHeaders(returnVal);
        }


    }
}

