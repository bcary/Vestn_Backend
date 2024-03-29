﻿using System;
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
using UserClientMembers.Models;
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

using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;


namespace UserClientMembers.Controllers
{
    public class UserController : BaseController
    {
        UserManager userManager = new UserManager();
        ProjectManager projectManager = new ProjectManager();
        UploadManager uploadManager = new UploadManager();
        CommunicationManager communicationManager = new CommunicationManager();
        LogAccessor logAccessor = new LogAccessor();
        AuthenticaitonEngine authenticationEngine = new AuthenticaitonEngine();
        ActivityManager activityManager = new ActivityManager();

        //User Account Creation

        public ActionResult fileexist()
        {
            return View();
        }

        [AcceptVerbs("POST", "OPTIONS")]
        [AllowCrossSiteJson]
        public string CheckFileExist(String url = null, string token = null)
        {
            if (Request.RequestType.Equals("OPTIONS", StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }
            if(String.IsNullOrEmpty(url))
            {
                return AddErrorHeader("No URL specified", 1);
            }
            if (url == "about:blank")
            {
                return AddErrorHeader("about:blank is not a valid url to check", 1);
            }
            
            int count = 0;
            bool found = false;
            while (!found && count < 40)
            {
                try
                {
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                    request.Timeout = 5000;
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    found = true;
                }
                catch (WebException ex)
                {
                    count++;
                    Thread.Sleep(500);
                    continue;
                }
            }
            if (count >= 40)
            {
                return AddErrorHeader("Timeout", 1);
            }
            else
            {
                return (AddSuccessHeader("File Exists", true));
            }
        }

        //[AcceptVerbs("POST", "OPTIONS")]
        //public string CheckFileExist(String url, string token)
        //{
        //    Response.AddHeader("Access-Control-Allow-Origin", "*");
        //    if (Request.RequestType.Equals("OPTIONS", StringComparison.InvariantCultureIgnoreCase))  //This is a preflight request
        //    {
        //        Response.AddHeader("Access-Control-Allow-Methods", "POST, PUT");
        //        Response.AddHeader("Access-Control-Allow-Headers", "X-Requested-With");
        //        Response.AddHeader("Access-Control-Allow-Headers", "X-Request");
        //        Response.AddHeader("Access-Control-Allow-Headers", "X-File-Name");
        //        Response.AddHeader("Access-Control-Allow-Headers", "Content-Type");
        //        Response.AddHeader("Access-Control-Max-Age", "86400"); //caching this policy for 1 day
        //        return null;
        //    }
        //    else
        //    {
        //        try
        //        {

        //            HttpWebRequest request = (HttpWebRequest)System.Net.WebRequest.Create(url);
        //            bool found = false;
        //            int counter = 0;
        //            while (!found)
        //            {
        //                try
        //                {
        //                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
        //                    if (response.StatusCode != HttpStatusCode.NotFound)
        //                    {
        //                        found = true;
        //                        return AddSuccessHeader("File Exists", true);
        //                    }
        //                    else
        //                    {
        //                        Thread.Sleep(500);
        //                        counter++;
        //                        if (counter > 40)
        //                        {
        //                            return AddErrorHeader("Check File Timeout. Either this URL will not exist, or the server is suuuper slow");
        //                        }
        //                    }

        //                }
        //                catch (System.Net.WebException)
        //                {
        //                    Thread.Sleep(500);
        //                    counter++;
        //                    if (counter > 40)
        //                    {
        //                        return AddErrorHeader("Check File Timeout. Either this URL will not exist, or the server is suuuper slow");
        //                    }
        //                    continue;
        //                }
        //            }
        //            return AddSuccessHeader("File Exists", true);
                    
        //        }
        //        catch (Exception ex)
        //        {
        //            logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), ex.ToString());
        //            return AddErrorHeader("Unknown error - CheckFileExist");
        //        }
        //    }
        //}


        public string TestMe()
        {
            return "success";
        }

        //User Account Creation

        //public ActionResult Register(Boolean isEducation = false)
        //{
        //    ViewBag.IsEducation = isEducation;
        //    return View();
        //}

        //[HttpPost]
        //public ActionResult Register(RegisterModel model)
        //{
        //    try
        //    {
        //        CommunicationManager communicationManager = new CommunicationManager();
        //        //if (ValidationEngine.ValidateBetaKey(model.betaKey) != ValidationEngine.Success)
        //        //{
        //        //    TempData["MessageBar"] = ValidationEngine.ValidateBetaKey(model.betaKey);
        //        //    return View(model);
        //        //}
        //        if (ValidationEngine.ValidateEmail(model.Email) != ValidationEngine.Success || ValidationEngine.IsDuplicateEmail(model.Email) == true)
        //        {
        //            TempData["MessageBar"] = "Invalid Email";
        //            return View(model);
        //        }
        //        if (!userManager.CheckDuplicateEmail(model.Email))
        //        {
        //            TempData["MessageBar"] = "A user with that email already exists in our database";
        //            return View(model);
        //        }
        //        if (ValidationEngine.ValidateUsername(model.UserName) != ValidationEngine.Success)
        //        {
        //            TempData["MessageBar"] = ValidationEngine.ValidateUsername(model.UserName);
        //            return View(model);
        //        }
        //        if (!userManager.CheckDuplicateUsername(model.UserName))
        //        {
        //            TempData["MessageBar"] = "A user with that username already exists in our database";
        //            return View(model);
        //        }
        //        if (ValidationEngine.ValidatePassword(model.Password) != ValidationEngine.Success)
        //        {
        //            TempData["MessageBar"] = ValidationEngine.ValidatePassword(model.Password);
        //            return View(model);
        //        }
        //        if (model.Password != model.ConfirmPassword)
        //        {
        //            TempData["MessageBar"] = "Password fields do not match";
        //            return View(model);
        //        }
        //        if (ModelState.IsValid)
        //        {
        //            User newUser = model.toUser();
        //            newUser.profileURL = newUser.userName;

        //            newUser = userManager.CreateUser(newUser, model.Password);

        //            userManager.ActivateUser(newUser, true);
        //            communicationManager.SendVerificationMail(userManager.GetProviderUserKey(newUser), newUser.userName, newUser.email);

        //            FormsAuthentication.SetAuthCookie(newUser.userName, false /* createPersistentCookie */);
        //        }

        //        return RedirectToAction("Profile", "User");
        //    }
        //    catch (Exception ex)
        //    {
        //        logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), ex.ToString());
        //        return View("Error");
        //    }
        //}

        //[HttpPost]
        //public string Register(string UserName, string Email, string Password, string ConfirmPassword)
        //{
        //    try
        //    {
        //        CommunicationManager communicationManager = new CommunicationManager();
        //        //if (ValidationEngine.ValidateBetaKey(model.betaKey) != ValidationEngine.Success)
        //        //{
        //        //    TempData["MessageBar"] = ValidationEngine.ValidateBetaKey(model.betaKey);
        //        //    return View(model);
        //        //}
        //        RegisterModel model = new RegisterModel { Email = Email, UserName = UserName, Password = Password, ConfirmPassword = ConfirmPassword };
        //        if (ValidationEngine.ValidateEmail(model.Email) != ValidationEngine.Success || ValidationEngine.IsDuplicateEmail(model.Email) == true)
        //        {
        //            TempData["MessageBar"] = "Invalid Email";
        //            return AddErrorHeader("Invalid Email");
        //            //return View(model);
        //        }
        //        if (!userManager.CheckDuplicateEmail(model.Email))
        //        {
        //            TempData["MessageBar"] = "A user with that email already exists in our database";
        //            return AddErrorHeader("A user with that email already exists in our database");
        //            //return View(model);
        //        }
        //        if (ValidationEngine.ValidateUsername(model.UserName) != ValidationEngine.Success)
        //        {
        //            TempData["MessageBar"] = ValidationEngine.ValidateUsername(model.UserName);
        //            return AddErrorHeader(ValidationEngine.ValidateUsername(model.UserName));
        //            //return View(model);
        //        }
        //        if (!userManager.CheckDuplicateUsername(model.UserName))
        //        {
        //            TempData["MessageBar"] = "A user with that username already exists in our database";
        //            return AddErrorHeader("A user with that username already exists in our database");

        //            //return View(model);
        //        }
        //        if (ValidationEngine.ValidatePassword(model.Password) != ValidationEngine.Success)
        //        {
        //            TempData["MessageBar"] = ValidationEngine.ValidatePassword(model.Password);
        //            return AddErrorHeader(ValidationEngine.ValidateUsername(model.Password));
        //            //return View(model);
        //        }
        //        if (model.Password != model.ConfirmPassword)
        //        {
        //            TempData["MessageBar"] = "Password fields do not match";
        //            return AddErrorHeader("Password fields do not match");

        //            //return View(model);
        //        }
        //        if (ModelState.IsValid)
        //        {
        //            User newUser = model.toUser();
        //            newUser.profileURL = newUser.userName;

        //            newUser = userManager.CreateUser(newUser, model.Password);

        //            userManager.ActivateUser(newUser, true);
        //            communicationManager.SendVerificationMail(userManager.GetProviderUserKey(newUser), newUser.userName, newUser.email);

        //            FormsAuthentication.SetAuthCookie(newUser.userName, false /* createPersistentCookie */);
        //        }
        //        return AddSuccessHeader("\"Register Successful\"");
        //        //return RedirectToAction("Profile", "User");
        //    }
        //    catch (Exception ex)
        //    {
        //        logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), ex.ToString());
        //        return AddErrorHeader("Error");
        //    }
        //}

        [AcceptVerbs("POST","OPTIONS")]
        [AllowCrossSiteJson]
        public string Register(string email, string password, string networkJoinCode = null)
        {
            if (Request != null)
            {
                if (Request.RequestType.Equals("OPTIONS", StringComparison.InvariantCultureIgnoreCase))
                {
                    return null;
                }
            }
            try
            {
                CommunicationManager communicationManager = new CommunicationManager();
                string userName = email.Substring(0, email.IndexOf('@'));
                userName = userName.Replace("+", "");
                RegisterModel model = new RegisterModel { Email = email, UserName = userName, Password = password, ConfirmPassword = password };
                if (ValidationEngine.ValidateEmail(model.Email) != ValidationEngine.Success)
                {
                    return AddErrorHeader("Invalid Email", 1);
                }
                if (!userManager.CheckDuplicateEmail(model.Email))
                {
                    return AddErrorHeader("A user with that email already exists in our database", 1);
                }
                if (ValidationEngine.ValidateUsername(model.UserName) != ValidationEngine.Success)
                {
                    return AddErrorHeader(ValidationEngine.ValidateUsername(model.UserName), 1);
                }
                if (!userManager.CheckDuplicateUsername(model.UserName))
                {
                    return AddErrorHeader("A user with that username already exists in our database", 1);
                }
                if (ValidationEngine.ValidatePassword(model.Password) != ValidationEngine.Success)
                {
                    return AddErrorHeader(ValidationEngine.ValidateUsername(model.Password), 1);
                }
                if (model.Password != model.ConfirmPassword)
                {
                    return AddErrorHeader("Password fields do not match", 1);
                }
                if (ModelState.IsValid)
                {
                    User newUser = model.toUser();
                    newUser.profileURL = newUser.userName;

                    newUser = userManager.CreateUser(newUser, model.Password);

                    userManager.ActivateUser(newUser, true);
                    //communicationManager.SendVerificationMail(userManager.GetProviderUserKey(newUser), newUser.userName, newUser.email);

                    if (networkJoinCode != null)
                    {
                        NetworkManager nm = new NetworkManager();
                        Network network = nm.GetNetworkByIdentifier(networkJoinCode);
                        if (network != null)
                        {
                            string[] emailArray = { email };
                            nm.AddNetworkUsers(network, emailArray);
                        }
                    }
                    userManager.SendVerifyEmail(email);

                    AuthenticaitonEngine authEngine = new AuthenticaitonEngine();
                    string token = authEngine.logIn(newUser.id, newUser.userName);
                    JsonModels.RegisterResponse rr = new JsonModels.RegisterResponse();
                    rr.userId = newUser.id;
                    rr.token = token;
                    return AddSuccessHeader(Serialize(rr));
                }
                else
                {
                    return AddErrorHeader("User Model Not Valid", 1);
                }
            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), ex.ToString());
                return AddErrorHeader("Something went wrong while creating this user", 1);
            }
        }

        [Authorize]
        public JsonResult AddConnection(int userId)
        {
            User currentUser = userManager.GetUser(User.Identity.Name);
            List<User> connections = userManager.GetUserConnections(currentUser);

            foreach (User user in connections)
            {
                if (userId == user.id)
                {
                    return Json(new { Error = "You are already connected to this user." });
                }
            }

            userManager.AddConnection(currentUser, userId);
            return Json(new { Status = 1 });
        }

        [HttpGet]
        public JsonResult FindUser(string query)//email profileURL lastName firstNameSpaceLastName
        {
            return Json(userManager.FindUser(query), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [HttpPost]
        public JsonResult AddUserTag(string value)
        {
            TagManager tm = new TagManager();
            string type;
            if (tm.GetAllSTagValues().Contains(value))
            {
                type = "s";
            }
            else
            {
                type = "f";
            }
            string result = tm.AddTag(value, type, userManager.GetUser(User.Identity.Name).id);
            if (result != null)
            {
                if (result == "Tag already added.")
                {
                    return Json(new { tagStatus = "Tag already added" });
                }
                else
                {
                    return Json(new { tagStatus = "Tag Added!" });
                }
            }
            else
            {
                return Json(new { Error = "Failed to add tag :(" });
            }
        }


        //currently IS sending emails.
        [Authorize]
        [HttpPost]
        public JsonResult SendVerificationEmail()
        {
            try
            {
                User user = userManager.GetUser(User.Identity.Name);
                if (user.emailVerified == 1)
                {
                    return Json(new { VerificationEmailStatus = "isVerified" });
                }
                else
                {
                    Guid guid = userManager.GetProviderUserKey(user);
                    //uncomment this for production
                    if (communicationManager.SendVerificationMail(guid, user.userName, user.email))
                    {
                        return Json(new { VerificationEmailStatus = "emailSent" });
                    }
                    else
                    {
                        return Json(new { VerificationEmailStatus = "emailNotSent" });
                    }
                }
            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), ex.ToString());
                return Json(new { Error = "An unknown error occured" });
            }
        }


        [Authorize]
        [HttpPost]
        public JsonResult MakeProfilePublic()
        {
            try
            {
                User user = userManager.GetUser(User.Identity.Name);

                if (user.isPublic == 1)
                {
                    return Json(new { MadePublicStatus = "profileAlreadyPublic" });
                }

                //ADDS TAGS
                /*
                TagManager tagManager = new TagManager();

                string lines = (Resource.freelancer_tags);
                char[] separators = { '\n', '\r' };
                var etfs = lines.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                int x = 0;
                do
                {
                    if (x == etfs.Length)
                    {
                        break;
                    }
                    if (etfs[x].Substring(0, 1) == "~")
                    {
                        sTag top = tagManager.CreateSTag(0, etfs[x].Substring(1, etfs[x].Length - 1).Trim());
                        x++;
                        if (x == etfs.Length)
                        {
                            break;
                        }
                        while (etfs[x] != "!")
                        {
                            int i = etfs[x].IndexOf("(");
                            string value = etfs[x].Substring(0, i - 2);
                            sTag mid = tagManager.CreateSTag(tagManager.GetSTag(top.value).id, value);
                            x++;
                            if (x == etfs.Length)
                            {
                                break;
                            }
                        }
                    }
                    if (x == etfs.Length)
                    {
                        break;
                    }
                    else if (etfs[x].Substring(0, 1) == "!")
                    {
                        x++;
                        if (x == etfs.Length)
                        {
                            break;
                        }
                    }
                }
                while (x < etfs.Length);
                */
                //END ADD TAGS

                user = userManager.MakePublic(user, 1);
                AnalyticsAccessor aa = new AnalyticsAccessor();
                aa.CreateAnalytic("Profile Made Public", DateTime.Now, user.userName);

                if (user.isPublic == 1)
                {
                    return Json(new { MadePublicStatus = "profileMadePublic" });
                }
                else
                {
                    if (user.emailVerified == 1)
                    {
                        return Json(new { MadePublicStatus = "profileNotMadePublic" });
                    }
                    else
                    {
                        return Json(new { MadePublicStatus = "userEmailNotVerified" });
                    }
                }
            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), ex.ToString());
                return Json(new { Error = "An unknown error occured" });
            }
        }

        [Authorize]
        [HttpPost]
        public JsonResult HidePublicProfile()
        {
            try
            {
                User user = userManager.GetUser(User.Identity.Name);

                if (user.isPublic == 0)
                {
                    return Json(new { HidePublicProfileStatus = "profileAlreadyHidden" });
                }

                user = userManager.MakePublic(user, 0);

                if (user.isPublic == 0)
                {
                    return Json(new { HidePublicProfileStatus = "profileMadeHidden" });
                }
                else
                {
                    if (user.emailVerified == 1)
                    {
                        return Json(new { HidePublicProfileStatus = "profileNotMadeHidden" });
                    }
                    else
                    {
                        return Json(new { HidePublicProfileStatus = "userEmailNotVerified" });
                    }
                }
            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), ex.ToString());
                return Json(new { Error = "An unknown error occured" });
            }
        }

        public ActionResult Verify(string id)
        {
            try
            {
                Guid guid = new Guid(id);
                User user = userManager.GetUser(guid);
                if (user != null)
                {
                    try
                    {
                        userManager.VerifyUserEmail(user, 1);
                        FormsAuthentication.SetAuthCookie(user.userName, false);
                        return RedirectToAction("Index");
                    }
                    catch (Exception) { }
                }
                else
                {
                    // Wrong GUID or user was already approved
                }

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), ex.ToString());
                return View("Error");
            }
        }

        //DEPRECIATED
        public ActionResult MakePublic(string id)
        {
            try
            {
                Guid guid = new Guid(id);
                User user = userManager.GetUser(guid);
                if (user != null)//need some kind of message to user when this fails because they are not email verified
                {
                    userManager.MakePublic(user, 1);
                }
                return RedirectToAction("Index", "Home");//fix redirect

            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), ex.ToString());
                return View("Error");
            }
        }


        //The log on function used by the new Front End.
        /// <summary>
        /// LogOn POST function. Authenticates the user, and returns a token value that must be stored by the client application and used on every subsequesnt authorized request
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns>Authenticaiton token</returns>
        ///
        [AcceptVerbs("POST", "OPTIONS")]
        [AllowCrossSiteJson]
        public string LogOn(string username, string password)
        {
            if (Request.RequestType.Equals("OPTIONS", StringComparison.InvariantCultureIgnoreCase))  //This is a preflight request
            {
                return null;
            }
            else
            {
                try
                {
                    User user = userManager.GetUser(username);
                    //MembershipUser mu = Membership.GetUser(username);
                    //mu.UnlockUser();
                    //mu.ChangePassword(mu.ResetPassword(), "Mpos104*");
                    if (user == null)
                    {
                        user = userManager.GetUserByEmail(username);
                        if (user != null)
                        {
                            username = user.userName;
                        }
                        else
                        {
                            return AddErrorHeader("The username/email does not exist in the database", 1);
                        }
                    }
                    if (userManager.ValidateUser(user, password))
                    {
                        AuthenticaitonEngine authEngine = new AuthenticaitonEngine();
                        string token = authEngine.logIn(user.id, user.userName);

                        AnalyticsAccessor aa = new AnalyticsAccessor();
                        aa.CreateAnalytic("User Login", DateTime.Now, user.userName);

                        JsonModels.LogOnModel logOnReturnObject = new JsonModels.LogOnModel();
                        logOnReturnObject.userId = user.id;
                        logOnReturnObject.firstName = (user.firstName != null) ? user.firstName : null;
                        logOnReturnObject.lastName = (user.lastName != null) ? user.lastName : null;
                        logOnReturnObject.profileURL = (user.profileURL != null) ? user.profileURL : null;
                        logOnReturnObject.token = token;
                        logOnReturnObject.emailVerified = (user.emailVerified == 1) ? true : false;

                        return AddSuccessHeader(Serialize(logOnReturnObject));
                    }
                    else
                    {
                        return AddErrorHeader("User Information Not Valid", 2);
                    }
                }
                catch (Exception ex)
                {
                    logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), ex.ToString());
                    return AddErrorHeader("Something went wrong while trying to log this user in", 1);
                }
            }
        }

        //User Account LogOn / LogOff
        /// <summary>
        /// Logs the user on and redirects to the home page
        /// </summary>
        /// <param name="string username"></param>
        /// <param name="string password"></param>
        /// <param name="boolean rememberme"></param>
        /// <returns>Home View</returns>
        [HttpPost]
        public JsonResult LogOnOld(string username, string password, Boolean rememberme)
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
                    FormsAuthentication.SetAuthCookie(username, rememberme);


                    //fixing issue with remember me checkbox.

                    FormsAuthentication.Initialize();
                    FormsAuthenticationTicket tkt = new FormsAuthenticationTicket(1, user.userName, DateTime.Now,
                      DateTime.Now.AddMinutes(30), rememberme, FormsAuthentication.FormsCookiePath);
                    HttpCookie ck = new HttpCookie(FormsAuthentication.FormsCookieName, FormsAuthentication.Encrypt(tkt));
                    ck.Path = FormsAuthentication.FormsCookiePath;

                    if (rememberme)
                        ck.Expires = DateTime.Now.AddMonths(1);

                    Response.Cookies.Add(ck);


                    //----------------------------------------

                    AnalyticsAccessor aa = new AnalyticsAccessor();
                    aa.CreateAnalytic("User Login", DateTime.Now, user.userName);


                    return Json(new { LogOnResult = "Success" });
                    // return Json(new { LogInStatus = "Login Success" });
                }
                else
                {
                    return Json(new { Error = "User Information Not Valid" });
                }


            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), ex.ToString());
                return Json(new { Error = "An unknown error occured" });
            }


        }

        public ActionResult LogOff()
        {
            FormsAuthentication.SignOut();

            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public ActionResult ChangeUserName()
        {
            return View();
        }

        //[Authorize]
        //[HttpPost]
        //public ActionResult ChangeUserName(ChangeUserNameModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        User user = userManager.GetUser(User.Identity.Name);
        //        user = userManager.ChangeUserName(user, model.NewUserName, model.Password);

        //        if (user != null)
        //        {
        //            TempData.Add("MessageBar", "Your username has been changed successfully.");
        //            FormsAuthentication.SignOut();
        //            FormsAuthentication.SetAuthCookie(user.userName, false);
        //            return RedirectToAction("Index");
        //        }
        //        else
        //        {
        //            ModelState.AddModelError("", "The password is incorrect or the new username is invalid.");
        //        }
        //    }

        //    // If we got this far, something failed, redisplay form
        //    return View(model);
        //}

        [AcceptVerbs("POST","OPTIONS")]
        [AllowCrossSiteJson]
        public string ChangePassword(string oldPassword, string newPassword, string token)
        {
            if (Request.RequestType.Equals("OPTIONS", StringComparison.InvariantCultureIgnoreCase))  //This is a preflight request
            {
                return null;
            }
            try
            {
                int authUserId = -1;
                if (token != null)
                {
                    authUserId = authenticationEngine.authenticate(token);
                }
                else
                {
                    return AddErrorHeader("A token must be passed in", 2);
                }
                if (authUserId < 0)
                {
                    return AddErrorHeader("The authenticaiton token is not valid", 2);
                }
                User user = userManager.GetUser(authUserId);
                bool changePasswordSucceeded = userManager.ChangePassword(user, oldPassword, newPassword);

                if (changePasswordSucceeded)
                {
                    return AddSuccessHeader("Password Successfully Changed", true);
                }
                else
                {
                    return AddErrorHeader("An error occurred changing the password", 1);
                }
            }
            catch (Exception ex)
            {
                return AddErrorHeader("Something went wrong changing the password", 1);
            }
        }

        [AcceptVerbs("POST","OPTIONS")]
        [AllowCrossSiteJson]
        public string ChangeProfileURL(string desiredProfileURL, string token)
        {
            if (Request.RequestType.Equals("OPTIONS", StringComparison.InvariantCultureIgnoreCase))  //This is a preflight request
            {
                return null;
            }
            try
            {
                int authUserId = -1;
                if (token != null)
                {
                    authUserId = authenticationEngine.authenticate(token);
                }
                else
                {
                    return AddErrorHeader("A token must be passed in", 2);
                }
                if (authUserId < 0)
                {
                    return AddErrorHeader("The authenticaiton token is not valid", 2);
                }
                User user = userManager.GetUser(authUserId);
                string validationResult = ValidationEngine.ValidateProfileURL(desiredProfileURL);
                if (validationResult == "success")
                {
                    user.profileURL = desiredProfileURL;
                    userManager.UpdateUser(user);
                    return AddSuccessHeader("Successfully updated profileURL", true);
                }
                else
                {
                    return AddErrorHeader(validationResult, 1);
                }

            }
            catch (Exception ex)
            {
                return AddErrorHeader("Something went wrong while updating this profileURL", 1);
            }
        }

        /*
        public JsonResult ForgotPassword(string email)
        {
            try
            {
                CommunicationManager communicationManager = new CommunicationManager();

                User user = userManager.GetUserByEmail(email);
                if (user == null)
                {
                    return Json(new { ForgotPasswordStatus = "emailNotSent" });
                }

                string resetPasswordHash = userManager.ResetPassword(user);

                communicationManager.SendForgotPasswordEmail(user.userName, user.email, resetPasswordHash);

                return Json(new { ForgotPasswordStatus = "emailSent" });
            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), ex.ToString());
                return Json(new { ForgotPasswordStatus = "Error" });
            }
        }

        public ActionResult ResetPassword(string userName, string resetPasswordHash)
        {
            try
            {
                CommunicationManager communicationManager = new CommunicationManager();

                User user = userManager.GetUser(userName);

                ChangePasswordModel changePasswordModel = new ChangePasswordModel()
                {
                    OldPassword = resetPasswordHash,
                    NewPassword = "",
                    ConfirmPassword = ""
                };

                if (userManager.ValidateUser(user, resetPasswordHash))
                {
                    FormsAuthentication.SetAuthCookie(user.userName, false);
                    return View(changePasswordModel);
                }

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), ex.ToString());
                return View("Error");
            }
        }
        */
        [AcceptVerbs("POST", "OPTIONS")]
        [AllowCrossSiteJson]
        public string ForgotPassword(string email)
        {
            if (Request.RequestType.Equals("OPTIONS", StringComparison.InvariantCultureIgnoreCase))  //This is a preflight request
            {
                return null;
            }
            try
            {
                CommunicationManager communicationManager = new CommunicationManager();

                User user = userManager.GetUserByEmail(email);
                if (user == null)
                {
                    return AddErrorHeader("Email not found", 1);
                }

                string resetPasswordHash = userManager.ResetPassword(user);
                user.forgotPasswordHash = resetPasswordHash;
                userManager.UpdateUser(user);

                communicationManager.SendForgotPassword(user.email, resetPasswordHash, user.firstName);

                return AddSuccessHeader("Email sent", true);
            }
            catch (Exception ex)
            {
                return AddErrorHeader("Error sending forgot password email", 1);
            }
        }

        [AcceptVerbs("POST", "OPTIONS")]
        [AllowCrossSiteJson]
        public string ResetPassword(string password, int userId, string hash)
        {
            if (Request.RequestType.Equals("OPTIONS", StringComparison.InvariantCultureIgnoreCase))  //This is a preflight request
            {
                return null;
            }
            try
            {
                User userForId = userManager.GetUser(userId);
                if (userForId.forgotPasswordHash == hash)
                {
                    if (userManager.ChangePassword(userForId, password))
                    {
                        AuthenticaitonEngine authEngine = new AuthenticaitonEngine();
                        string token = authEngine.logIn(userForId.id, userForId.userName);
                        JsonModels.RegisterResponse rr = new JsonModels.RegisterResponse();
                        rr.userId = userForId.id;
                        rr.token = token;
                        userForId.forgotPasswordHash = null;
                        userManager.UpdateUser(userForId);
                        return AddSuccessHeader(Serialize(rr));
                    }
                    else
                    {
                        return AddErrorHeader("Error resetting password", 1);
                    }
                }
                else
                {
                    return AddErrorHeader("Invalid hash", 1);
                }
            }
            catch (Exception ex)
            {
                return AddErrorHeader("Error resetting password", 1);
            }
        }



        //Viewing a User's Profile Using Various Methods

        [Authorize]
        public ActionResult Index()
        {
            try
            {
                User loggedOnUser = userManager.GetUser(User.Identity.Name);
                if (loggedOnUser == null)
                {
                    return RedirectToAction("Index", "Home");
                }
                return RedirectToAction("Home", new { userName = loggedOnUser.userName });
            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), ex.ToString());
                return View("Error");
            }
        }

        [Authorize]
        public ActionResult Home(string userName)
        {
            try
            {
                User user = userManager.GetUser(userName);
                if (user == null)
                {
                    return View("NotAuthenticated");
                }

                UserModel userModel = new UserModel(user);

                if (user.userName == User.Identity.Name && User.Identity.IsAuthenticated && userModel != null)
                {
                    return View(userModel);
                }
                else
                {
                    return View("NotAuthenticated");
                }
            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), ex.ToString());
                return View("Error");
            }
        }

        public ActionResult Profile(string profileURL)
        {
            if (profileURL == "")
            {
                User currentUser = userManager.GetUser(User.Identity.Name);
                return RedirectToAction("Profile", "User", new { profileURL = currentUser.profileURL });
            }


            //throw (new ArgumentNullException());
            TempData["MessageBar"] = TempData["MessageBar"];
            TempData["Popup"] = TempData["Popup"];

            try
            {
                ViewBag.DisplayPicture = false;
                ViewBag.DisplayInfo = false;

                TagManager tagManager = new TagManager();


                User user = userManager.GetUserByProfileURL(profileURL);
                if (user == null)
                {
                    try
                    {
                        string userNameLoggedIn = User.Identity.Name;
                        if (userNameLoggedIn == null || userNameLoggedIn == "")
                        {
                            return RedirectToAction("Index", "Home");
                        }
                        else
                        {
                            user = userManager.GetUser(userNameLoggedIn);
                        }
                    }
                    catch (Exception e)
                    {

                    }
                }
                else if ((User.Identity.Name != user.userName) && (user.isPublic != 1))
                {
                    //if not the owner and trying to access a user that is not public
                    return RedirectToAction("Index", "Home");
                }
                //else...
                //projectManager.moveProjectRight(user, 2);
                //userManager.UpdateUser(user);
                if (user.projectOrder == null)
                {
                    userManager.ResetProjectOrder(user);
                    userManager.UpdateUser(user);
                    foreach (Project p in user.projects)
                    {
                        projectManager.resetProjectElementOrder(p);
                        projectManager.UpdateProject(p);
                    }
                }

                ProfileModel model = new ProfileModel(user);
                List<string> tagValues = new List<string>();
                //Put user's tags on the ProfileModel
                /*
                if (user.tagIds != null && user.tagIds != "")
                {
                    List<Tag> tagList = tagManager.GetTags(user.tagIds);
                    foreach (Tag tag in tagList)
                    {
                        tagValues.Add(tag.value);
                    }
                    model.tagValues = tagValues;
                }*/

                //ViewBag.WillingToRelocate = new List<string>(Enum.GetNames(typeof(WillingToRelocateType)));

                if (user.userName == User.Identity.Name && User.Identity.IsAuthenticated)
                {
                    AnalyticsAccessor aa = new AnalyticsAccessor();
                    aa.CreateAnalytic("Profile Page Hit: Logged in", DateTime.Now, user.userName);

                    //User is going to their own profile
                    ViewBag.IsOwner = true;
                    model.connections = new List<User>();
                    if (user.connections != null)
                    {
                        foreach (string userId in user.connections.Split(','))
                        {
                            if (userId.Trim() != "")
                            {
                                int userIdInt = Convert.ToInt32(userId);
                                User connection = userManager.GetUser(userIdInt);
                                model.connections.Add(connection);
                            }
                        }
                    }

                    /*//depreciated. can't use .CompleteProfilePrompt any more. will have to deal with tags some other way
                     * if (userManager.IsProfilePartiallyComplete(user))
                    {
                        //User has already entered some extra information on their profile
                        ViewBag.CompleteProfilePrompt = false;
                    }
                    else
                    {
                        //User has not updated any further info on their profile
                        //Get list of tags for picking out which ones we initially want on our profile
                        List<string> listOfLowestLevelTags = userManager.GetAllLowestLevelTagValues();
                        ViewBag.LowestLevelTags = listOfLowestLevelTags;
                        ViewBag.CompleteProfilePrompt = true;
                    }*/
                }
                else
                {
                    AnalyticsAccessor aa = new AnalyticsAccessor();
                    aa.CreateAnalytic("Profile Page Hit: Not Logged in", DateTime.Now, user.userName);

                    //User is visiting someone else's profile
                    ViewBag.IsOwner = false;
                }


                //------------------------------------------------------------
                return View(model);
            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), ex.ToString());
                return View("Error");
            }

        }



        //Edit a User's Profile

        //Update profile method written eliquently by Skyler
        //(Called after modal box is submitted)
        [Authorize]
        [HttpPost]
        public ActionResult Profile(ProfileModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    User user = model.toUser();

                    //Will need to be uncommented when we start editing tags on the profile page
                    /*
                    foreach (string tag in model.tagValues)
                    {
                        user = userManager.addTag(user, tag);
                    }
                    */
                    user = userManager.UpdateUser(user);

                }
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), ex.ToString());
                return View("Error");
            }
        }

        [Authorize]
        public JsonResult InviteFriend(string friendName, string friendEmail)
        {
            if (ValidationEngine.ValidateEmail(friendEmail) != ValidationEngine.Success)
            {
                return Json(new { InviteFriendStatus = "emailNotSent" });
            }
            if (!userManager.CheckDuplicateEmail(friendEmail))
            {
                return Json(new { InviteFriendStatus = "emailAlreadyRegistered" });
            }
            try
            {
                //for now skip this. don't want to send emails to everyone
                communicationManager.SendInviteFriendEmail(User.Identity.Name, friendName, friendEmail);
                return Json(new { InviteFriendStatus = "emailSent" });
            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), ex.ToString());
                return Json(new { Error = "An unknown error occured" });
            }
        }

        [AcceptVerbs("POST","OPTIONS")]
        [AllowCrossSiteJson]
        public string UpdateProfileModel(IEnumerable<JsonModels.ProfileInformation> profile, string token = null)
        {
            if (Request.RequestType.Equals("OPTIONS", StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }
            try
            {
                int authUserId = -1;
                if (token != null)
                {
                    authUserId = authenticationEngine.authenticate(token);
                }
                else
                {
                    return AddErrorHeader("An authentication token must be passed in", 2);
                }
                if (authUserId < 0)
                {
                    return AddErrorHeader("You are not authenticated, please log in!", 2);
                }
                JsonModels.ProfileInformation profileFromJson = profile.FirstOrDefault();
               
                if (profileFromJson.id == authUserId.ToString())
                {
                    User originalProfile = userManager.GetUser(authUserId);
                    if (originalProfile != null)
                    {
                        //model sync
                        if (profileFromJson.description != null)
                        {
                            originalProfile.description = (profileFromJson.description == "null") ? null : profileFromJson.description;
                        }
                        if (profileFromJson.email != null)
                        {
                            originalProfile.email = (profileFromJson.email == "null") ? null : profileFromJson.email;
                        }
                        if (profileFromJson.facebookLink != null)
                        {
                            originalProfile.facebookLink = (profileFromJson.facebookLink == "null") ? null : profileFromJson.facebookLink;
                        }
                        if (profileFromJson.twitterLink != null)
                        {
                            originalProfile.twitterLink = (profileFromJson.twitterLink == "null") ? null : profileFromJson.twitterLink;
                        }
                        if (profileFromJson.linkedinLink != null)
                        {
                            originalProfile.linkedinLink = (profileFromJson.linkedinLink == "null") ? null : profileFromJson.linkedinLink;
                        }
                        if (profileFromJson.firstName != null)
                        {
                            originalProfile.firstName = (profileFromJson.firstName == "null") ? null : profileFromJson.firstName;
                        }
                        if (profileFromJson.lastName != null)
                        {
                            originalProfile.lastName = (profileFromJson.lastName == "null") ? null : profileFromJson.lastName;
                        }
                        if (profileFromJson.location != null)
                        {
                            originalProfile.location = (profileFromJson.location == "null") ? null : profileFromJson.location;
                        }
                        if (profileFromJson.major != null)
                        {
                            originalProfile.major = (profileFromJson.major == "null") ? null : profileFromJson.major;
                        }
                        if (profileFromJson.phoneNumber != null)
                        {
                            originalProfile.phoneNumber = (profileFromJson.phoneNumber == "null") ? null : profileFromJson.phoneNumber;
                        }
                        if (profileFromJson.projectOrder != null)
                        {
                            originalProfile.projectOrder = (profileFromJson.projectOrder == "null") ? null : profileFromJson.projectOrder;
                        }
                        //originalProfile.resume = (profileFromJson.resume != null) ? profileFromJson.resume : null;
                        if (profileFromJson.organization != null)
                        {
                            originalProfile.organization = (profileFromJson.organization == "null") ? null : profileFromJson.organization;
                        }
                        if (profileFromJson.tagLine != null)
                        {
                            originalProfile.tagLine = (profileFromJson.tagLine == "null") ? null : profileFromJson.tagLine;
                        }
                        if (profileFromJson.title != null)
                        {
                            originalProfile.title = (profileFromJson.title == "null") ? null : profileFromJson.title;
                        }
                        
                        userManager.UpdateUser(originalProfile);
                        JsonModels.ProfileInformation returnProfile = userManager.GetProfileJson(originalProfile);
                        activityManager.AddActivity(originalProfile.id, "Profile", "Updated", originalProfile.id);
                        return AddSuccessHeader (Serialize(returnProfile));
                    }
                    else
                    {
                        return AddErrorHeader("The user does not exist in the database", 1);
                    }
                }
                else
                {
                    return AddErrorHeader("User is not the profile owner, and thus is not authorized to edit this profile!", 3);
                }
            }
            catch(Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, "UserController - UpdateProfile", ex.StackTrace);
                return AddErrorHeader("Something went wrong while updating this Profile.", 1);
            }
        }

        [AcceptVerbs("POST", "OPTIONS")]
        [AllowCrossSiteJson]
        public string testThumbnail(string qqfile = null)
        {
            if (Request.RequestType.Equals("OPTIONS", StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }
            try
            {
                var length = Request.ContentLength;
                var bytes = new byte[length];
                Request.InputStream.Read(bytes, 0, length);
                Stream s = new MemoryStream(bytes);
                ThumbnailEngine te = new ThumbnailEngine();
                Bitmap b = te.CreateProfileThumbnail(s, 100, 100);
                MemoryStream ms = new MemoryStream();
                b.Save(ms, ImageFormat.Bmp);
                BlobStorageAccessor ba = new BlobStorageAccessor();
                string location = ba.uploadImage(ms, false).ToString();
                return location;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [AcceptVerbs("POST", "OPTIONS")]
        [AllowCrossSiteJson]
        public string UpdateProfilePicture(int userId, string token = null, string qqfile = null)
        {
            if (Request.RequestType.Equals("OPTIONS", StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }
            try
            {
                int authUserId = -1;
                if (token != null)
                {
                    authUserId = authenticationEngine.authenticate(token);
                }
                else
                {
                    return AddErrorHeader("An authentication token must be passed in", 2);
                }
                if (authUserId < 0)
                {
                    return AddErrorHeader("You are not authenticated, please log in!", 2);
                }
                User user = userManager.GetUser(userId);
                if (user == null)
                {
                    return AddErrorHeader("User not found", 1);
                }
                if (userId == authUserId)
                {
                    if (qqfile != null || Request.Files.Count == 1)
                    {
                        var length = Request.ContentLength;
                        var bytes = new byte[length];
                        Request.InputStream.Read(bytes, 0, length);
                        Stream s = new MemoryStream(bytes);
                        if (user.profilePicture != null && user.profilePictureThumbnail != null)
                        {
                            userManager.DeleteProfilePicture(user);
                        }
                        string returnPic = userManager.UploadUserPicture(user, s, "Profile");
                        activityManager.AddActivity(user.id, "Profile Picture", "Updated", user.id);
                        return AddSuccessHeader("http://vestnstaging.blob.core.windows.net/thumbnails/" + returnPic, true);
                    }
                    else
                    {
                        return AddErrorHeader("No files posted to server", 1);
                    }
                }
                else
                {
                    return AddErrorHeader("The user is not authorized to edit this profile picture", 3);
                }
                
            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, "UserController - UpdateProfilePicture",ex.StackTrace);
                return AddErrorHeader("Something went wrong while updating this profile picture", 1);
            }
        }

        [AcceptVerbs("POST", "OPTIONS")]
        [AllowCrossSiteJson]
        public string UpdateSettings(int userId, string email = null, string profileURL = null, string visibility = null, string token = null)
        {
            if (Request.RequestType.Equals("OPTIONS", StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }
            try
            {
                int authUserId = -1;
                if (token != null)
                {
                    authUserId = authenticationEngine.authenticate(token);
                }
                else
                {
                    return AddErrorHeader("An authentication token must be passed in", 2);
                }

                if (authUserId != userId)
                {
                    return AddErrorHeader("Not authorized to update another user's settings", 3);
                }
                else
                {
                    User user = userManager.GetUser(userId);
                    if (user != null)
                    {
                        JsonModels.UserSettings settings = new JsonModels.UserSettings();
                        settings.email = user.email;
                        settings.firstName = user.firstName;
                        settings.lastName = user.lastName;
                        settings.profileURL = user.profileURL;
                        if (user.isPublic == 0)
                        {
                            settings.visibility = "hidden";
                        }
                        else
                        {
                            settings.visibility = "public";
                        }

                        Boolean updateTables = false;
                        if (profileURL != null && user.profileURL != profileURL)
                        {
                            string result = ValidationEngine.ValidateProfileURL(profileURL);
                            if (result == ValidationEngine.Success)
                            {
                                settings.profileURL = profileURL;
                                user.profileURL = profileURL;
                                updateTables = true;
                            }
                            else
                            {
                                return AddErrorHeader("Not a valid profile URL", 1);
                            }
                        }
                        else if (email != null && user.email != email)
                        {
                            if (userManager.CheckDuplicateEmail(email))
                            {
                                settings.email = email;
                                user.email = email;
                                updateTables = true;
                            }
                            else
                            {
                                return AddErrorHeader("Email already in use", 1);
                            }
                        }
                        else if (visibility != null && visibility != settings.visibility)
                        {
                            updateTables = true;
                            settings.visibility = visibility;
                            if (visibility == "hidden")
                            {
                                user.isPublic = 0;
                            }
                            else
                            {
                                user.isPublic = 1;
                            }
                        }
                        if (updateTables)
                        {
                            //update user now with updated fields;
                            bool success = userManager.UpdateUserSettings(user);
                            if (success)
                            {
                                return AddSuccessHeader(Serialize(settings));
                                //email user's new and old email?
                            }
                            else
                            {
                                return AddErrorHeader("Update Unsuccessful", 1);
                            }
                        }
                        else//nothing updated but still a success
                        {
                            return AddSuccessHeader(Serialize(settings));
                        }
                    }
                    else
                    {
                        return AddErrorHeader("No user found for id " + userId, 1);
                    }
                }
            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, "UserController - UpdateSettingsModel", ex.StackTrace);
                return AddErrorHeader("Something went wrong while updating this  user SettingsModel", 1);
            }
        }

        [AcceptVerbs("POST", "OPTIONS")]
        [AllowCrossSiteJson]
        public string UpdateResume(int userId, string token = null, string qqfile = null)
        {
            if (Request.RequestType.Equals("OPTIONS", StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }
            try
            {
                int authUserId = -1;
                if (token != null)
                {
                    authUserId = authenticationEngine.authenticate(token);
                }
                else
                {
                    return AddErrorHeader("An authentication token must be passed in", 2);
                }
                if (authUserId < 0)
                {
                    return AddErrorHeader("You are not authenticated, please log in!", 2);
                }
                User user = userManager.GetUser(userId);
                if (user == null)
                {
                    return AddErrorHeader("User not found", 1);
                }
                if (userId == authUserId)
                {
                    string fileName = null;
                    if (qqfile == null)
                    {
                        fileName = Request.Files.Get(0).FileName;
                    }
                    else
                    {
                        fileName = qqfile;
                    }
                    var length = Request.ContentLength;
                    var bytes = new byte[length];
                    Request.InputStream.Read(bytes, 0, length);
                    Stream fs = new MemoryStream(bytes);
                    string[] s2 = fileName.Split('.');
                    string fileType = s2[s2.Count() - 1].ToLower();

                    string resumeUri = null;
                    if (String.Compare(fileType, "pdf", true) == 0)
                    {
                        resumeUri = userManager.UploadResumePDF(user, fs);
                        return AddSuccessHeader(resumeUri, true);
                    }
                    else if (String.Compare(fileType, "doc", true) == 0)
                    {
                        resumeUri = userManager.UploadResumeDoc(user, fs);
                        return AddSuccessHeader("http://vestnstaging.blob.core.windows.net/pdfs/" + resumeUri, true);
                    }
                    else if (String.Compare(fileType, "docx", true) == 0)
                    {
                        resumeUri = userManager.UploadResumeDocx(user, fs);
                        return AddSuccessHeader("http://vestnstaging.blob.core.windows.net/pdfs/" + resumeUri, true);
                    }
                    else if (String.Compare(fileType, "rtf", true) == 0)
                    {
                        resumeUri = userManager.UploadResumeRTF(user, fs);
                        return AddSuccessHeader("http://vestnstaging.blob.core.windows.net/pdfs/" + resumeUri, true);
                    }
                    else if (String.Compare(fileType, "txt", true) == 0)
                    {
                        resumeUri = userManager.UploadResumeTXT(user, fs);
                        return AddSuccessHeader("http://vestnstaging.blob.core.windows.net/pdfs/" + resumeUri, true);
                    }
                    else
                    {
                        return AddErrorHeader("Document Type not supported", 1);
                    }
                }
                else
                {
                    return AddErrorHeader("User is not authorized to edit this resume!", 3);
                }
            }
            catch (Exception ex)
            {
                return AddErrorHeader("Something went wrong while trying to update this user's resume", 1);
            }
        }

        [AcceptVerbs("POST", "OPTIONS")]
        [AllowCrossSiteJson]
        public string VerifyEmail(string email, string hash)
        {
            if (Request.RequestType.Equals("OPTIONS", StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }
            try
            {
                User u = userManager.GetUserByEmail(email);
                if (u != null)
                {
                    //User Manager Verify Email (email, hash)
                    if (u.verifyEmailHash == hash)
                    {
                        bool success = userManager.verifyEmail(u);
                        if (success)
                        {
                            AuthenticaitonEngine authEngine = new AuthenticaitonEngine();
                            string token = authEngine.logIn(u.id, u.userName);
                            JsonModels.RegisterResponse rr = new JsonModels.RegisterResponse();
                            rr.userId = u.id;
                            rr.token = token;
                            return AddSuccessHeader(Serialize(rr));
                        }
                        else
                        {
                            return AddErrorHeader("Error updating user",1);
                        }
                    }
                    else
                    {
                        return AddErrorHeader("Invalid verify email identifier", 1);
                    }
                }
                else
                {
                    return AddErrorHeader("Invalid Email", 1);
                }
            }
            catch (Exception ex)
            {
                return AddErrorHeader("Something went wrong while verifying this email", 1);
            }
        }


        //[Authorize]
        [AcceptVerbs("POST","OPTIONS")]
        [AllowCrossSiteJson]
        public string UpdateUser(int userId = -1, string propertyId = null, string propertyValue = null, string token = null, string qqfile = null)
        {
            if (Request.RequestType.Equals("OPTIONS", StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }
            try
            {
                int authUserId = -1;
                if (token != null)
                {
                    authUserId = authenticationEngine.authenticate(token);
                }
                else
                {
                    return AddErrorHeader("An authentication token must be passed in",2);
                }
                if (authUserId < 0)
                {
                    return AddErrorHeader("You are not authenticated, please log in!", 2);
                }
                User user = userManager.GetUser(userId);
                if (user == null)
                {
                    return AddErrorHeader("User not found", 1);
                }
                if (userId == authUserId)
                {
                    System.Reflection.PropertyInfo pi = null;
                    if (propertyId != null)
                    {
                        if (propertyId == "coverPicture")
                        {
                            propertyId = "aboutPicture";//Its called a coverPicture on the site, but an aboutPicture in the database
                        }
                        pi = user.GetType().GetProperty(propertyId);
                    }
                    else
                    {
                        AddErrorHeader("You must pass in a propertyId to set", 1);
                    }

                    if (pi == null)
                    {
                        return AddErrorHeader("Invalid propertyId", 1);
                    }
                    else
                    {
                        try
                        {
                            if (propertyValue != null)
                            {
                                propertyValue = StripNewLineAndReplaceWithLineBreaks(propertyValue);
                            }
                            else if (propertyId == "profilePicture" || propertyId == "aboutPicture" || propertyId == "resume")
                            {
                                //its OK for propertyValue to be null
                            }
                            else
                            {
                                return AddErrorHeader("You must pass in a propertyValue to set", 1);
                            }
                            bool postedFile = false;
                            if (qqfile != null || Request.Files.Count == 1)
                            {
                                postedFile = true;
                            }
                            if (postedFile)
                            {
                                if (propertyId == "aboutPicture")
                                {
                                    var length = Request.ContentLength;
                                    var bytes = new byte[length];
                                    Request.InputStream.Read(bytes, 0, length);
                                    Stream s = new MemoryStream(bytes);
                                    string returnPic = userManager.UploadUserPicture(user, s, "About");
                                    return AddSuccessHeader("http://vestnstaging.blob.core.windows.net/thumbnails/" + returnPic, true);
                                }
                                else if (propertyId == "profilePicture")
                                {
                                    var length = Request.ContentLength;
                                    var bytes = new byte[length];
                                    Request.InputStream.Read(bytes, 0, length);
                                    Stream s = new MemoryStream(bytes);
                                    if (user.profilePicture != null && user.profilePictureThumbnail != null)
                                    {
                                        userManager.DeleteProfilePicture(user);
                                    }
                                    string returnPic = userManager.UploadUserPicture(user, s, "Profile");
                                    return AddSuccessHeader("http://vestnstaging.blob.core.windows.net/thumbnails/" + returnPic, true);
                                }
                                else if (propertyId == "resume")
                                {
                                    string fileName = null;
                                    if (qqfile == null)
                                    {
                                        fileName = Request.Files.Get(0).FileName;
                                    }
                                    else
                                    {
                                        fileName = qqfile;
                                    }
                                    var length = Request.ContentLength;
                                    var bytes = new byte[length];
                                    Request.InputStream.Read(bytes, 0, length);
                                    Stream fs = new MemoryStream(bytes);
                                    string[] s2 = fileName.Split('.');
                                    string fileType = s2[s2.Count() - 1].ToLower();

                                    string resumeUri = null;
                                    if (String.Compare(fileType, "pdf", true) == 0)
                                    {
                                        resumeUri = userManager.UploadResumePDF(user, fs);
                                        return AddSuccessHeader(resumeUri, true);
                                    }
                                    else if (String.Compare(fileType, "doc", true) == 0)
                                    {
                                        resumeUri = userManager.UploadResumeDoc(user, fs);
                                        return AddSuccessHeader("http://vestnstaging.blob.core.windows.net/pdfs/" + resumeUri, true);
                                    }
                                    else if (String.Compare(fileType, "docx", true) == 0)
                                    {
                                        resumeUri = userManager.UploadResumeDocx(user, fs);
                                        return AddSuccessHeader("http://vestnstaging.blob.core.windows.net/pdfs/" + resumeUri, true);
                                    }
                                    else if (String.Compare(fileType, "rtf", true) == 0)
                                    {
                                        resumeUri = userManager.UploadResumeRTF(user, fs);
                                        return AddSuccessHeader("http://vestnstaging.blob.core.windows.net/pdfs/" + resumeUri, true);
                                    }
                                    else if (String.Compare(fileType, "txt", true) == 0)
                                    {
                                        resumeUri = userManager.UploadResumeTXT(user, fs);
                                        return AddSuccessHeader("http://vestnstaging.blob.core.windows.net/pdfs/" + resumeUri, true);
                                    }
                                    else
                                    {
                                        return AddErrorHeader("Document Type not supported", 1);
                                    }
                                }
                            }
                            switch (propertyId)
                            {
                                case "profileURL":
                                    if (user.profileURL != propertyValue)
                                    {
                                        if (ValidationEngine.ValidateProfileURL(propertyValue) == ValidationEngine.Success)
                                        {
                                            pi.SetValue(user, Convert.ChangeType(propertyValue, pi.PropertyType), null);
                                        }
                                        else
                                        {
                                            return AddErrorHeader("profileURL not valid, user not updated", 1);
                                        }
                                    }
                                    else
                                    {
                                        return AddErrorHeader("profileURL already in use, user not updated", 1);
                                    }
                                    break;
                                case "school":
                                    if (ValidationEngine.ValidateSchool(propertyValue) == ValidationEngine.Success)
                                    {
                                        pi.SetValue(user, Convert.ChangeType(propertyValue, pi.PropertyType), null);
                                    }
                                    else
                                    {
                                        return AddErrorHeader("school not valid, user not updated", 1);
                                    }
                                    break;
                                case "email":
                                    if (propertyValue != user.email)
                                    {
                                        if (ValidationEngine.ValidateEmail(propertyValue) == ValidationEngine.Success && ValidationEngine.IsDuplicateEmail(propertyValue) == false)
                                        {
                                            pi.SetValue(user, Convert.ChangeType(propertyValue, pi.PropertyType), null);
                                        }
                                        else
                                        {
                                            return AddErrorHeader("email not valid, user not updated", 1);
                                        }
                                    }
                                    else
                                    {
                                        return AddErrorHeader("email match, user not updated", 1);
                                    }
                                    break;
                                case "location":
                                    if (ValidationEngine.ValidateLocation(propertyValue) == ValidationEngine.Success)
                                    {
                                        pi.SetValue(user, Convert.ChangeType(propertyValue, pi.PropertyType), null);
                                    }
                                    else
                                    {
                                        return AddErrorHeader("location not valid, user not updated", 1);
                                    }
                                    break;
                                case "firstName":
                                    if (ValidationEngine.ValidateFirstName(propertyValue) == ValidationEngine.Success)
                                    {
                                        pi.SetValue(user, Convert.ChangeType(propertyValue, pi.PropertyType), null);
                                    }
                                    else
                                    {
                                        return AddErrorHeader("firstName not valid, user not updated", 1);
                                    }
                                    break;
                                case "lastName":
                                    if (ValidationEngine.ValidateLastName(propertyValue) == ValidationEngine.Success)
                                    {
                                        pi.SetValue(user, Convert.ChangeType(propertyValue, pi.PropertyType), null);
                                    }
                                    else
                                    {
                                        return AddErrorHeader("lastName not valid, user not updated", 1);
                                    }
                                    break;
                                case "title":
                                    if (ValidationEngine.ValidateTitle(propertyValue) == ValidationEngine.Success)
                                    {
                                        pi.SetValue(user, Convert.ChangeType(propertyValue, pi.PropertyType), null);
                                    }
                                    else
                                    {
                                        return AddErrorHeader("title not valid, user not updated", 1);
                                    }
                                    break;
                                case "major":
                                    if (ValidationEngine.ValidateMajor(propertyValue) == ValidationEngine.Success)
                                    {
                                        pi.SetValue(user, Convert.ChangeType(propertyValue, pi.PropertyType), null);
                                    }
                                    else
                                    {
                                        return AddErrorHeader("major not valid, user not updated", 1);
                                    }
                                    break;
                                case "connections":
                                    if (ValidationEngine.ValidateMajor(propertyValue) == ValidationEngine.Success)
                                    {
                                        pi.SetValue(user, Convert.ChangeType(propertyValue, pi.PropertyType), null);
                                    }
                                    else
                                    {
                                        return AddErrorHeader("connections not valid, user not updated", 1);
                                    }
                                    break;
                                case "description":
                                    if (ValidationEngine.ValidateDescription(propertyValue) == ValidationEngine.Success)
                                    {
                                        pi.SetValue(user, Convert.ChangeType(propertyValue, pi.PropertyType), null);
                                    }
                                    else
                                    {
                                        return AddErrorHeader("description not valid, user not updated", 1);
                                    }
                                    break;
                                case "tagLine":
                                    if (ValidationEngine.ValidateDescription(propertyValue) == ValidationEngine.Success)
                                    {
                                        pi.SetValue(user, Convert.ChangeType(propertyValue, pi.PropertyType), null);
                                    }
                                    else
                                    {
                                        return AddErrorHeader("tagLine not valid, user not updated", 1);
                                    }
                                    break;

                            }
                            //persist user model to DB with manager updateUser method
                            user = userManager.UpdateUser(user);
                            AnalyticsAccessor aa = new AnalyticsAccessor();
                            aa.CreateAnalytic("User Update", DateTime.Now, user.userName, "Information updated: " + pi.PropertyType.ToString());

                            return AddSuccessHeader("UserId:"+userId+" successfully updated", true);
                        }
                        catch (Exception exc)
                        {
                            logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), exc.ToString());
                            return AddErrorHeader("Something went wrong while updating this user", 1);
                        }
                    }
                }
                else
                {
                    return AddErrorHeader("User not authorized to edit this user", 3);
                }
            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), ex.ToString());
                return AddErrorHeader("Something went wrong while updating this user", 1);
            }
        }

        [Authorize]
        public JsonResult CompleteProfile(string firstName, string lastName, string location, string school, string major, bool checkbox)
        {
            try
            {

                if (!checkbox)
                {
                    return Json(new { Error = "Please accept the Vestn User Agreement before continuing." });
                }

                if (ValidationEngine.ValidateFirstName(firstName) != ValidationEngine.Success)
                {
                    return Json(new { Error = ValidationEngine.ValidateFirstName(firstName) });
                }

                if (ValidationEngine.ValidateLastName(lastName) != ValidationEngine.Success)
                {
                    return Json(new { Error = ValidationEngine.ValidateLastName(lastName) });
                }

                if (ValidationEngine.ValidateLocation(location) != ValidationEngine.Success)
                {
                    return Json(new { Error = ValidationEngine.ValidateLocation(location) });
                }

                if (ValidationEngine.ValidateSchool(school) != ValidationEngine.Success)
                {
                    return Json(new { Error = ValidationEngine.ValidateSchool(school) });
                }

                if (ValidationEngine.ValidateMajor(major) != ValidationEngine.Success)
                {
                    return Json(new { Error = ValidationEngine.ValidateMajor(major) });
                }

                User user = userManager.GetUser(User.Identity.Name);
                user.firstName = firstName;
                user.lastName = lastName;
                user.location = location;
                user.organization = school;
                user.major = major;
                user.isActive = 1;
                if (checkbox == true)
                {
                    UserAgreementAccessor uaa = new UserAgreementAccessor();
                    uaa.CreateAgreement(DateTime.Now, user.userName, "Agree", Request.ServerVariables["REMOTE_ADDR"]);
                }

                //update user with new values (this will not update tags or prof pic/resume)
                user = userManager.UpdateUser(user);

                user = userManager.UpdateUser(user);

                if (user.profilePicture != null)
                {
                    while (user.profilePictureThumbnail == null)
                    {
                        user = userManager.GetUser(user.id);
                        Thread.Sleep(200);
                    }
                }

                return Json(new { UserInformation = new { FirstName = firstName, LastName = lastName, Location = location, School = school, Major = major } });
            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), ex.ToString());
                return Json(new { Status = 0 });
            }
        }

        //Dispose

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        //This get use by both add feedback and Get help request
        public JsonResult AddFeedback(string message, string subject)
        {
            try
            {
                string name = null;
                string userEmail = null;
                try
                {
                    name = User.Identity.Name;
                    userEmail = userManager.GetUser(name).email;
                }
                catch (Exception e)
                {
                    logAccessor.CreateLog(DateTime.Now, System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), e.ToString());
                }
                FeedbackAccessor feedbackAccesor = new FeedbackAccessor();
                feedbackAccesor.CreateFeedback(name, message, subject);
                if (subject == "Error Report")
                {
                    CommunicationManager cm = new CommunicationManager();
                    cm.SendErrorEmail(name, message);
                }
                else if (subject == "Help Request")
                {
                    CommunicationManager cm = new CommunicationManager();
                    cm.SendHelpRequestEmail(userEmail, message);
                }
                else if (subject == "Feedback")
                {
                    CommunicationManager cm = new CommunicationManager();
                    cm.SendSiteFeedbackEmail(name, userEmail, message);
                }

                return Json(new { FeedbackStatus = "success" });
            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), ex.ToString());
                return Json(new { Error = "Problem submitting feedback" });
            }
        }

        public JsonResult Share(string link, string email)
        {
            try
            {

                if (ValidationEngine.ValidateEmail(email) != ValidationEngine.Success)
                {
                    return Json(new { Error = "Please enter a valid email." });
                }

                string senderName = null;
                try
                {
                    string userNombre = User.Identity.Name;
                    if (userNombre.Length > 0)
                    {
                        // user is logged in, so we'll put their name in the email
                        User user = userManager.GetUser(User.Identity.Name);
                        senderName = user.firstName;
                    }
                    else
                    {
                        // person who wants to share the profile isn't logged in
                        senderName = "Someone";
                    }
                    link = "vestn.com/v/" + link;
                }
                catch (Exception e)
                {
                    logAccessor.CreateLog(DateTime.Now, System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), e.ToString());
                }

                CommunicationManager cm = new CommunicationManager();
                if (cm.SendShareEmail(link, email, senderName))
                {
                    return Json(new { ShareStatus = "success" });
                }
                else
                {
                    return Json(new { Error = "Email failed to send." });
                }


            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), ex.ToString());
                return Json(new { Error = "Problem sharing link" });
            }
        }

        [AcceptVerbs("POST", "OPTIONS")]
        [AllowCrossSiteJson]
        public string SendUserFeedback(int userId, string feedbackMessage, string fromAlias = null)
        {
            try
            {
                User user = userManager.GetUser(userId);
                if (fromAlias == null)
                {
                    communicationManager.SendUserFeedback(user.email, user.firstName, feedbackMessage);
                }
                else
                {
                    communicationManager.SendUserFeedback(user.email, user.firstName, feedbackMessage, fromAlias);
                }
                return AddSuccessHeader("Feedback Sent to User",true);
            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, "UserController - SendUserFeedback", ex.StackTrace);
                return AddErrorHeader("Something went wrong while sending feedback to this user", 1);
            }
        }

        public JsonResult SendFeedbackToUser(string friendUsername, string message)
        {
            try
            {
                string userFirstName = null;
                string friendFirstName = null;
                string friendEmail = null;
                User user = userManager.GetUser(User.Identity.Name);
                try
                {
                    if (user == null)
                    {
                        userFirstName = "Someone";
                    }
                    else if (user.firstName == null)
                    {
                        userFirstName = user.userName;
                    }
                    else
                    {
                        userFirstName = userManager.GetUser(User.Identity.Name).firstName;
                    }

                    friendFirstName = userManager.GetUser(friendUsername).firstName;
                    friendEmail = userManager.GetUser(friendUsername).email;
                }
                catch (Exception e)
                {
                    logAccessor.CreateLog(DateTime.Now, System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), e.ToString());
                }

                CommunicationManager cm = new CommunicationManager();
                cm.SendFeedbackToUserEmail(userFirstName, friendFirstName, friendEmail, message);

                return Json(new { FeedbackStatus = "success" });
            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), ex.ToString());
                return Json(new { Error = "Problem submitting feedback" });
            }
        }

        // This will return the youtbe URL and Token values
        //[Authorize]
        [HttpPost]
        public JsonResult GetYoutubeURLandToken()
        {
            // this generate Youtube url and token value

            string[] UrlAndToken = new string[2];

            try
            {
                if (projectManager.uploadVideoFile() != null)
                {
                    UrlAndToken = projectManager.uploadVideoFile();
                    return Json(new { postURL = UrlAndToken[0], token = UrlAndToken[1] });
                }
                else
                {
                    return Json(null);
                }
            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), ex.ToString());
                return Json(new { Error = "Problem retreiving Youtube URL/Token" });
            }

        }

         //<summary>
         //This is the main end point for getting basic user information. 
         //It takes in a int[] of userIds and a string[] request objects
         //Here is an example request:
         ///User/GetUserInformation?id=1&id=2&request=firstName&request=lastName&request=tagLine
         //requestObjects can contain any of the following strinsg:
         //    firstName, lastName, title, school, description, tagLine, resume, profilePicture, profilePictureThumbnail, stats, links, experiences, references, tags, projects, todo, recentActivity, connections
         //</summary>
         //<param name="List<int> userIds"></param>
         //<param name="List<string> requestObjects"></param>
         //<returns>Json Object of UserInformation class</returns>
         
        [AcceptVerbs("POST", "OPTIONS")]
        public string  GetUserInformation(int id = -1, string profileURL = null, string[] request = null, string token = null)
        {
            Response.AddHeader("Access-Control-Allow-Origin", "*");
            if (Request.RequestType.Equals("OPTIONS", StringComparison.InvariantCultureIgnoreCase))  //This is a preflight request
            {
                Response.AddHeader("Access-Control-Allow-Methods", "POST, PUT");
                Response.AddHeader("Access-Control-Allow-Headers", "X-Requested-With");
                Response.AddHeader("Access-Control-Allow-Headers", "X-Request");
                Response.AddHeader("Access-Control-Allow-Headers", "X-File-Name");
                Response.AddHeader("Access-Control-Allow-Headers", "Content-Type");
                Response.AddHeader("Access-Control-Max-Age", "86400"); //caching this policy for 1 day
                return null;
            }
            else
            {
                //authenticate via token
                string returnVal;
                try
                {
                    if (token != null)
                    {
                        int authenticate = authenticationEngine.authenticate(token);
                        if (authenticate < 0)
                        {
                            //Response.StatusCode = 500;
                            //return AddErrorHeader("Not Authenticated");
                        }
                    }
                    bool requestAll = false;
                    if (request == null)
                    {
                        requestAll = true;
                    }
                    List<JsonModels.UserInformation> userInformationList = new List<JsonModels.UserInformation>();
                    int add = 0;
                    User u;
                    if (id < 0)
                    {
                        if (profileURL != null)
                        {
                            u = userManager.GetUserByProfileURL(profileURL);
                            if (u == null)
                            {
                                return AddErrorHeader("A user with the specified profileURL was not found", 1);
                            }
                            else
                            {
                                id = u.id;
                            }
                        }
                        else
                        {
                            return AddErrorHeader("An id or profileURL must be specified", 1);
                        }
                    }
                    else
                    {
                        u = userManager.GetUser(id);
                        if (u == null)
                        {
                            return AddErrorHeader("A user with the specified id was not found", 1);
                        }
                    }
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
                            if (u.organization != null)
                            {
                                ui.school = u.organization;
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
                        if (requestAll || request.Contains("aboutPicture"))
                        {
                            if (u.aboutPicture != null)
                            {
                                ui.aboutPicture = u.aboutPicture;
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
                        if (requestAll || request.Contains("aboutPictureThumbnail"))
                        {
                            if (u.aboutPictureThumbnail != null)
                            {
                                ui.aboutPictureThumbnail = u.aboutPictureThumbnail;
                                add = 1;
                            }
                        }
                        if (requestAll || request.Contains("stats"))
                        {
                            JsonModels.UserStats stats = userManager.getUserStats(id);
                            if (stats != null)
                            {
                                ui.stats = stats;
                                add = 1;
                            }
                        }
                        if (requestAll || request.Contains("links"))
                        {
                            JsonModels.Links links = userManager.getUserLinks(id);
                            if (links != null)
                            {
                                ui.links = links;
                                add = 1;
                            }
                        }
                        if (requestAll || request.Contains("experiences"))
                        {
                            List<JsonModels.Experience> experiences = userManager.GetUserExperiences(id);
                            if (experiences != null && experiences.Count != 0)
                            {
                                ui.experiences = experiences;
                                add = 1;
                            }
                        }
                        if (requestAll || request.Contains("references"))
                        {
                            List<JsonModels.Reference> references = userManager.GetUserReferences(id);
                            if (references != null && references.Count != 0)
                            {
                                ui.references = references;
                                add = 1;
                            }
                        }
                        if (requestAll || request.Contains("tags"))
                        {
                            List<JsonModels.UserTag> tags = userManager.GetUserTags(id);
                            if (tags != null && tags.Count != 0)
                            {
                                ui.tags = tags;
                                add = 1;
                            }
                        }
                        if (requestAll || request.Contains("projects"))
                        {
                            List<JsonModels.ProjectShell> projects = projectManager.GetProjectShells(id);
                            if (projects != null && projects.Count != 0)
                            {
                                ui.projects = projects;
                                add = 1;
                            }
                        }
                        if (requestAll || request.Contains("todo"))
                        {
                            List<JsonModels.Todo> todoList = userManager.GetTodo(id);
                            if (todoList != null && todoList.Count != 0)
                            {
                                ui.todo = todoList;
                                add = 1;
                            }
                        }
                        if (requestAll || request.Contains("recentActivity"))
                        {
                            List<JsonModels.RecentActivity> recentActivity = userManager.GetRecentActivity(id);
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
                    try
                    {
                        returnVal = Serialize(userInformationList);
                    }
                    catch (Exception ex)
                    {
                        logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), ex.ToString());
                        return AddErrorHeader(ex.Message, 1);
                    }
                }
                catch (Exception ex)
                {
                    logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), ex.ToString());
                    return AddErrorHeader("Bad Request", 1);
                }
                return AddSuccessHeader(returnVal);
            }
        }

        [AcceptVerbs("POST", "OPTIONS")]
        public string UpdateProjectOrder(string order, string token)
        {
            Response.AddHeader("Access-Control-Allow-Origin", "*");
            if (Request.RequestType.Equals("OPTIONS", StringComparison.InvariantCultureIgnoreCase))  //This is a preflight request
            {
                Response.AddHeader("Access-Control-Allow-Methods", "POST, PUT");
                Response.AddHeader("Access-Control-Allow-Headers", "X-Requested-With");
                Response.AddHeader("Access-Control-Allow-Headers", "X-Request");
                Response.AddHeader("Access-Control-Allow-Headers", "X-File-Name");
                Response.AddHeader("Access-Control-Allow-Headers", "Content-Type");
                Response.AddHeader("Access-Control-Max-Age", "86400"); //caching this policy for 1 day
                return null;
            }
            else
            {
                try
                {
                    int userId = authenticationEngine.authenticate(token);
                    if (userId < 0)
                    {
                        return AddErrorHeader("You are not authenticated, please log in!", 2);
                    }
                    User u = userManager.GetUser(userId);
                    ReorderEngine re = new ReorderEngine();
                    List<int> ListOrder = re.stringOrderToList(order);
                    List<int> currentProjectIds = new List<int>();
                    bool add = true;
                    foreach (Project p in u.projects)
                    {
                        currentProjectIds.Add(p.id);
                    }
                    foreach (int i in ListOrder)
                    {
                        if (!currentProjectIds.Contains(i))
                        {
                            add = false;
                        }
                    }
                    if (add == false)
                    {
                        //????????you cant do that
                        return AddErrorHeader("Update Failed.", 1);
                    }
                    else
                    {
                        u.projectOrder = order;
                        u = userManager.UpdateUser(u);
                    }
                    return AddSuccessHeader("Order updated", true);
                }
                catch (Exception ex)
                {
                    logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), ex.ToString());
                    return AddErrorHeader("Something went wrong while updating the Project Order", 1);
                }
            }
        }

        [AcceptVerbs("POST", "OPTIONS")]
        [AllowCrossSiteJson]
        public string GetProfile(int id = -1, string profileURL = null, IEnumerable<string> request = null, bool ignoreViewCount = false, string token = null)
        {
            if (Request.RequestType.Equals("OPTIONS", StringComparison.InvariantCultureIgnoreCase))  //This is a preflight request
            {
                return null;
            }
            else
            {
                
                //authenticate via token
                string returnVal;
                int authenticateId = -1;
                int add = 0;
                try
                {
                    if (token != null)
                    {
                        authenticateId = authenticationEngine.authenticate(token);
                        if (authenticateId < 0)
                        {
                            //Only return PUBLIC projects
                            //retreive user
                            //two lists of networks
                            //networks, admin networks <- list of network where i am an admin
                        }
                    }
                    bool requestAll = false;

                    JsonModels.ProfileInformation ui = new JsonModels.ProfileInformation();
                    User u;
                    Boolean sameUser = false;
                    Boolean adminOrMemberCheck = false;
                    if (id < 0)//???? && profileURL != "" ===>>> this endpoint supports getting by id OR profileURL. if the id is < 0, we must try to retrieve the profileURL
                    {
                        if (profileURL != null)
                        {
                            u = userManager.GetUserByProfileURL(profileURL);
                            if (u == null)
                            {
                                return AddErrorHeader("A user with the specified profileURL was not found", 1);
                            }
                            id = u.id;
                            if (id != authenticateId)//check to see if the authenticated user is an admin of the requested user's networks
                            {
                                User authenticatedUser = userManager.GetUser(authenticateId);
                                if (authenticatedUser == null)
                                {
                                    //This means somebody is attempting to hit the profile PUBLICLY, without a token
                                    if (u.isPublic == 0)
                                    {
                                        return AddErrorHeader("Not Authorized", 3);
                                    }
                                    //TODO
                                    //whatever setting needs to happen so that only public projects are returned
                                }
                                else
                                {
                                    List<int> authenticatedUserAdminandNetworkIds = new List<int>();
                                    List<int> requestedUserNetworkIds = new List<int>();
                                    foreach (Network n in u.networks)
                                    {
                                        requestedUserNetworkIds.Add(n.id);
                                    }
                                    foreach (Network n in authenticatedUser.adminNetworks)
                                    {
                                        authenticatedUserAdminandNetworkIds.Add(n.id);
                                    }
                                    foreach (Network n in authenticatedUser.networks)
                                    {
                                        authenticatedUserAdminandNetworkIds.Add(n.id);
                                    }
                                    foreach (int i in authenticatedUserAdminandNetworkIds)
                                    {
                                        foreach (int n in requestedUserNetworkIds)
                                        {
                                            if (i == n)
                                            {
                                                adminOrMemberCheck = true;
                                                break;
                                            }
                                        }
                                    }
                                    if (u.isPublic == 0 && !adminOrMemberCheck)
                                    {
                                        return AddErrorHeader("Not Authorized", 3);
                                    }
                                }
                            }
                            else
                            {
                                sameUser = true;
                            }

                            if (id == authenticateId)
                            {
                                if (request == null)
                                {
                                    requestAll = true;
                                }
                                else
                                {
                                    //requestAll is false still
                                }
                            }
                            else
                            {
                                if (request == null)
                                {
                                    requestAll = true;
                                    if (!ignoreViewCount)
                                    {
                                        u.profileViews++;
                                        userManager.UpdateUser(u);
                                    }
                            }
                                }
                            }
                        }
                        else
                        {
                            return AddErrorHeader("An id or profileURL must be specified", 1);
                        }
                    }
                    else
                    {
                        u = userManager.GetUser(id);
                        if (u == null)
                        {
                            return AddErrorHeader("A user with the specified id was not found", 1);
                        }
                        if (id != authenticateId)//check to see if the authenticated user is an admin of the requested user's networks
                        {
                            User authenticatedUser = userManager.GetUser(authenticateId);
                            if (authenticatedUser == null)
                            {
                                if (u.isPublic == 0)
                                {
                                    return AddErrorHeader("Not Authorized", 3);
                                }
                                //whatever setting needs to happen so that only public projects are returned
                            }
                            else
                            {
                                List<int> authenticatedUserAdminandNetworkIds = new List<int>();
                                List<int> requestedUserNetworkIds = new List<int>();
                                foreach (Network n in u.networks)
                                {
                                    requestedUserNetworkIds.Add(n.id);
                                }
                                foreach (Network n in authenticatedUser.adminNetworks)
                                {
                                    authenticatedUserAdminandNetworkIds.Add(n.id);
                                }
                                foreach (Network n in authenticatedUser.networks)
                                {
                                    authenticatedUserAdminandNetworkIds.Add(n.id);
                                }
                                foreach (int i in authenticatedUserAdminandNetworkIds)
                                {
                                    foreach (int n in requestedUserNetworkIds)
                                    {
                                        if (i == n)
                                        {
                                            adminOrMemberCheck = true;
                                            break;
                                        }
                                    }
                                }
                                if (u.isPublic == 0 && !adminOrMemberCheck)
                                {
                                    return AddErrorHeader("Not Authorized", 3);
                                }
                            }
                        }
                        else
                        {
                            sameUser = true;
                        }
                        if (id == authenticateId)
                        {
                            if (request == null)
                            {
                                requestAll = true;
                            }
                            else
                            {
                                //requestAll is still false
                            }
                        }
                        else
                        {
                            if (request == null)
                            {
                                requestAll = true;
                                if (!ignoreViewCount)
                                {
                                    u.profileViews++;
                                    userManager.UpdateUser(u);
                                }
                            }
                        }
                    }

                    //END OF DETECTING SETTINGS
                    List<string> projectPrivacyPrivledge = new List<string>();
                    projectPrivacyPrivledge.Add("public");
                    if (sameUser)
                    {
                        projectPrivacyPrivledge.Add("network");
                        projectPrivacyPrivledge.Add("private");
                    }
                    else if (adminOrMemberCheck)
                    {
                        projectPrivacyPrivledge.Add("network");
                    }

                    if (u != null)
                    {
                        add = 0;
                        if (requestAll || request.Contains("firstName"))
                        {
                            if (u.firstName != null)
                            {
                                ui.firstName = u.firstName;
                                add = 1;
                            }
                            else
                            {
                                ui.firstName = null;
                            }
                        }
                        if (requestAll || request.Contains("lastName"))
                        {
                            if (u.lastName != null)
                            {
                                ui.lastName = u.lastName;
                                add = 1;
                            }
                            else
                            {
                                ui.lastName = null;
                            }
                        }
                        if (requestAll || request.Contains("phoneNumber"))
                        {
                            if (u.phoneNumber != null)
                            {
                                ui.phoneNumber = u.phoneNumber;
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
                            else
                            {
                                ui.tagLine = null;
                            }
                        }
                        if (requestAll || request.Contains("email"))
                        {
                            if (u.email != null)
                            {
                                ui.email = u.email;
                                add = 1;
                            }
                            else
                            {
                                ui.email = null;
                            }
                        }
                        if (requestAll || request.Contains("location"))
                        {
                            if (u.location != null)
                            {
                                ui.location = u.location;
                                add = 1;
                            }
                            else
                            {
                                ui.location = null;
                            }
                        }
                        if (requestAll || request.Contains("title"))
                        {
                            if (u.title != null)
                            {
                                ui.title = u.title;
                                add = 1;
                            }
                            else
                            {
                                ui.title = null;
                            }
                        }
                        if (requestAll || request.Contains("organization"))
                        {
                            if (u.organization != null)
                            {
                                ui.organization = u.organization;
                                add = 1;
                            }
                            else
                            {
                                ui.organization = null;
                            }
                        }
                        if (requestAll || request.Contains("major"))
                        {
                            if (u.major != null)
                            {
                                ui.major = u.major;
                                add = 1;
                            }
                            else
                            {
                                ui.major = null;
                            }
                        }
                        if (requestAll || request.Contains("description"))
                        {
                            if (u.description != null)
                            {
                                ui.description = u.description;
                                add = 1;
                            }
                            else
                            {
                                ui.description = null;
                            }
                        }
                        if (requestAll || request.Contains("resume"))
                        {
                            if (u.resume != null)
                            {
                                ui.resume = u.resume;
                                add = 1;
                            }
                            else
                            {
                                ui.resume = null;
                            }
                        }
                        if (requestAll || request.Contains("projectOrder"))
                        {
                            if (u.projectOrder != null)
                            {
                                ui.projectOrder = u.projectOrder;
                                add = 1;
                            }
                            else
                            {
                                ui.projectOrder = null;
                            }
                        }
                        if (requestAll || request.Contains("profilePicture"))
                        {
                            if (u.profilePicture != null)
                            {
                                ui.profilePicture = u.profilePicture;
                                add = 1;
                            }
                            else
                            {
                                ui.profilePicture = null;
                            }
                        }
                        if (requestAll || request.Contains("profilePictureThumbnail"))
                        {
                            if (u.profilePictureThumbnail != null)
                            {
                                ui.profilePictureThumbnail = u.profilePictureThumbnail;
                                add = 1;
                            }
                            else
                            {
                                ui.profilePictureThumbnail = null;
                            }
                        }
                        //TODO actually calculate the stats
                        if (requestAll || request.Contains("stats"))
                        {
                            JsonModels.UserStats stats = userManager.getUserStats(id);
                            if (stats != null)
                            {
                                ui.stats = stats;
                                add = 1;
                            }
                            else
                            {
                                ui.stats = null;
                            }
                        }
                        if (requestAll || request.Contains("links"))
                        {
                            ui.facebookLink = u.facebookLink;
                            ui.twitterLink = u.twitterLink;
                            ui.linkedinLink = u.linkedinLink;
                        }
                        if (requestAll || request.Contains("experiences"))
                        {
                            List<JsonModels.Experience> experiences = userManager.GetUserExperiences(id);
                            if (experiences != null && experiences.Count != 0)
                            {
                                ui.experiences = experiences;
                                add = 1;
                            }
                            else
                            {
                                ui.experiences = null;
                            }
                        }
                        if (requestAll || request.Contains("references"))
                        {
                            List<JsonModels.Reference> references = userManager.GetUserReferences(id);
                            if (references != null && references.Count != 0)
                            {
                                ui.references = references;
                                add = 1;
                            }
                            else
                            {
                                ui.references = null;
                            }
                        }
                        if (requestAll || request.Contains("tags"))
                        {
                            List<JsonModels.UserTag> tags = userManager.GetUserTags(id);
                            if (tags != null && tags.Count != 0)
                            {
                                ui.tags = tags;
                                add = 1;
                            }
                            else
                            {
                                ui.tags = null;
                            }
                        }
                        if (requestAll || request.Contains("projects"))
                        {
                            int[] projectIds = new int[u.projects.Count]; 
                            int count = 0;
                            foreach (Project p in u.projects)
                            {
                                projectIds[count] = p.id;
                                count++;
                            }
                            List<JsonModels.CompleteProject> projects = projectManager.GetCompleteProjects(projectIds, projectPrivacyPrivledge);
                            if (projects != null && projects.Count != 0)
                            {
                                ui.projects = projects;
                                add = 1;
                            }
                            else
                            {
                                ui.projects = null;
                            }
                        }
                        if (requestAll || request.Contains("profileViews"))
                        {
                            ui.profileViews = u.profileViews;
                        }
                        if (requestAll || request.Contains("activity"))
                        {
                            List<JsonModels.Activity> activity = activityManager.GetUserActivity(id);
                            ui.activity = activity;
                        }
                        //if (requestAll || request.Contains("profileScore"))
                        //{
                        //    ui.profileScore = userManager.GetProfileScore(u);
                        //}
                        ui.id = u.id.ToString();
                    }
                    try
                    {
                        returnVal = Serialize(ui);
                    }
                    catch (Exception ex)
                    {
                        logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), ex.ToString());
                        return AddErrorHeader(ex.Message, 1);
                    }
                }
                catch (Exception ex)
                {
                    logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), ex.ToString());
                    return AddErrorHeader("Bad Request", 1);
                }
                return AddSuccessHeader(returnVal);
            }
        }

        [AcceptVerbs("POST", "OPTIONS")]
        [AllowCrossSiteJson]
        public string AddExperience(string title = null, string description = null, string startDate = null, string endDate = null, string city = null, string state = null, string company = null, string token = null)
        {
            if (Request.RequestType.Equals("OPTIONS", StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }
            try
            {
                int authUserId = -1;
                if (token != null)
                {
                    authUserId = authenticationEngine.authenticate(token);
                }
                else
                {
                    return AddErrorHeader("An authentication token must be passed in", 2);
                }
                if (authUserId < 0)
                {
                    return AddErrorHeader("You are not authenticated, please log in!", 2);
                }
                User user = userManager.GetUser(authUserId);
                DateTime startDateTime = DateTime.Now;
                if(startDate != null)
                {
                    startDateTime = DateTime.Parse(startDate);
                }
                DateTime endDateTime = DateTime.Now;
                if(endDate != null)
                {
                    endDateTime = DateTime.Parse(endDate);
                }
                JsonModels.Experience exp = userManager.AddExperience(user.id, startDateTime, endDateTime, title, description, city, state, company);
                activityManager.AddActivity(user.id, "Experience", "Added", exp.id);
                if (exp != null)
                {
                    return AddSuccessHeader(Serialize(exp));
                }
                else
                {
                    return AddErrorHeader("Something went wrong while attempting to save this experience", 1);
                }

            }
            catch (Exception e)
            {
                logAccessor.CreateLog(DateTime.Now, "userController - AddExperience", e.StackTrace);
                return AddErrorHeader("Something went wrong while adding the experience", 1);
            }
        }

        [AcceptVerbs("POST", "OPTIONS")]
        [AllowCrossSiteJson]
        public string UpdateExperienceModel(IEnumerable<JsonModels.Experience> experience, string token = null)
        {
            if (Request.RequestType.Equals("OPTIONS", StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }
            try
            {
                int authUserId = -1;
                if (token != null)
                {
                    authUserId = authenticationEngine.authenticate(token);
                }
                else
                {
                    return AddErrorHeader("An authentication token must be passed in", 2);
                }
                if (authUserId < 0)
                {
                    return AddErrorHeader("You are not authenticated, please log in!", 2);
                }
                if (experience != null)
                {
                    JsonModels.Experience experienceFromJson = experience.FirstOrDefault();
                    if (experienceFromJson != null)
                    {
                        Experience originalExperience = userManager.GetExperience(experienceFromJson.id);
                        if (originalExperience != null)
                        {
                            if (originalExperience.userId == authUserId)
                            {
                                if (experienceFromJson.city != null)
                                {
                                    originalExperience.city = (experienceFromJson.city == "null") ? null : experienceFromJson.city;
                                }
                                if (experienceFromJson.company != null)
                                {
                                    originalExperience.company = (experienceFromJson.company == "null") ? null : experienceFromJson.company;
                                }
                                if (experienceFromJson.description != null)
                                {
                                    originalExperience.description = (experienceFromJson.description == "null") ? null : experienceFromJson.description;
                                }
                                if (experienceFromJson.endDate != null)
                                {
                                    originalExperience.endDate = DateTime.Parse(experienceFromJson.endDate);
                                }
                                if (experienceFromJson.startDate != null)
                                {
                                    originalExperience.startDate = DateTime.Parse(experienceFromJson.startDate);
                                }
                                if (experienceFromJson.state != null)
                                {
                                    originalExperience.state = (experienceFromJson.state == "null") ? null : experienceFromJson.state;
                                }
                                if (experienceFromJson.title != null)
                                {
                                    originalExperience.title = (experienceFromJson.title == "null") ? null : experienceFromJson.title;
                                }
                                userManager.UpdateExperience(originalExperience);
                                activityManager.AddActivity(authUserId, "Experience", "Updated", originalExperience.id);
                                return AddSuccessHeader(Serialize(experienceFromJson));
                            }
                            else
                            {
                                return AddErrorHeader("User is not authorized to edit this experience", 3);
                            }
                        }
                        else
                        {
                            return AddErrorHeader("The experience model was not found in the database", 1);
                        }
                    }
                    else
                    {
                        return AddErrorHeader("The experience model was not valid", 1);
                    }
                }
                else
                {
                    return AddErrorHeader("An experience model was not received", 1);
                }
            }
            catch(Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, "userController - updateExperience", ex.StackTrace);
                return AddErrorHeader("Something went wrong while updating this experience element", 1);
            }
        }

        [AcceptVerbs("POST", "OPTIONS")]
        [AllowCrossSiteJson]
        public string UpdateExperience(int experienceId = -1, string propertyId = null, string propertyValue = null , string token = null)
        {
            if (Request.RequestType.Equals("OPTIONS", StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }
            try
            {
                int authUserId = -1;
                if (token != null)
                {
                    authUserId = authenticationEngine.authenticate(token);
                }
                else
                {
                    return AddErrorHeader("An authentication token must be passed in", 2);
                }
                if (authUserId < 0)
                {
                    return AddErrorHeader("You are not authenticated, please log in!", 2);
                }
                Experience exp = new Experience();
                if (experienceId > 0)
                {
                    exp = userManager.GetExperience(experienceId);
                }
                else
                {
                    return AddErrorHeader("An experienceId must be passed in", 1);
                }
                if (exp.userId != authUserId)
                {
                    return AddErrorHeader("User is not authorized to exit this experience", 3);
                }
                System.Reflection.PropertyInfo pi;
                if(propertyId == null)
                {
                    return AddErrorHeader("A propertyId must be passed in", 1);
                }
                else
                {
                    pi = exp.GetType().GetProperty(propertyId);
                }
                if(propertyValue == null)
                {
                    return AddErrorHeader("A propertyValue must be passed in", 1);
                }
                else
                {
                    if(propertyId == "startDate" || propertyId == "endDate")
                    {
                        pi.SetValue(exp, Convert.ChangeType(DateTime.Parse(propertyValue), pi.PropertyType), null);
                    }
                    else
                    {
                        pi.SetValue(exp, Convert.ChangeType(propertyValue, pi.PropertyType), null);
                    }
                    Experience exp1 = userManager.UpdateExperience(exp);
                    JsonModels.Experience returnExp = userManager.GetExperienceJson(exp1.id);
                    string ret = Serialize(returnExp);
                    return AddSuccessHeader(ret);
                }

            }
            catch(Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, "userController - updateExperience", ex.StackTrace);
                return AddErrorHeader("Something went wrong while updating this experience element", 1);
            }
        }

        [AcceptVerbs("POST", "OPTIONS")]
        [AllowCrossSiteJson]
        public string GetExperience(int experienceId = -1, string token = null)
        {
            if (Request.RequestType.Equals("OPTIONS", StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }
            try
            {
                int authUserId = -1;
                if (token != null)
                {
                    authUserId = authenticationEngine.authenticate(token);
                }
                else
                {
                    return AddErrorHeader("An authentication token must be passed in", 3);
                }
                if (authUserId < 0)
                {
                    return AddErrorHeader("You are not authenticated, please log in!", 3);
                }
                if (experienceId < 0)
                {
                    return AddErrorHeader("An experienceId must be passed in", 1);
                }
                else
                {
                    JsonModels.Experience expModelJson = userManager.GetExperienceJson(experienceId);
                    if (expModelJson == null)
                    {
                        return AddErrorHeader("The experience was not found in the database", 1);
                    }
                    else
                    {
                        return AddSuccessHeader(Serialize(expModelJson));
                    }
                }
            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, "UserController - GetExperience", ex.StackTrace);
                return AddErrorHeader("Something went wrong while getting this experience", 1);
            }
        }

        [AcceptVerbs("POST", "OPTIONS")]
        [AllowCrossSiteJson]
        public string DeleteExperience(int experienceId = -1, string token = null)
        {
            if (Request.RequestType.Equals("OPTIONS", StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }
            try
            {
                int authUserId = -1;
                if (token != null)
                {
                    authUserId = authenticationEngine.authenticate(token);
                }
                else
                {
                    return AddErrorHeader("An authentication token must be passed in", 2);
                }
                if (authUserId < 0)
                {
                    return AddErrorHeader("You are not authenticated, please log in!", 2);
                }
                if (experienceId < 0)
                {
                    return AddErrorHeader("An experienceId must be passed in", 1);
                }
                else
                {
                    Experience experience = userManager.GetExperience(experienceId);
                    if (experience == null)
                    {
                        return AddErrorHeader("An experience with the provided experienceId does not exist", 1);
                    }
                    string response = userManager.DeleteExperience(experience);
                    if (response == null)
                    {
                        return AddErrorHeader("Something went wrong while deleting this experience", 1);
                    }
                    else
                    {
                        return AddSuccessHeader(response, true);
                    }
                }
            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, "UserController - DeleteExperience", ex.StackTrace);
                return AddErrorHeader("Something went wrong while deleting this experience", 1);
            }
        }

        [AcceptVerbs("POST", "OPTIONS")]
        [AllowCrossSiteJson]
        public string AddReference(string firstName = null, string lastName = null, string email = null, string company = null, string title = null, string message = null, string videoLink = null , string videoType = null, string token = null)
        {
            if (Request.RequestType.Equals("OPTIONS", StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }
            try
            {
                int authUserId = -1;
                if (token != null)
                {
                    authUserId = authenticationEngine.authenticate(token);
                }
                else
                {
                    return AddErrorHeader("An authentication token must be passed in", 2);
                }
                if (authUserId < 0)
                {
                    return AddErrorHeader("You are not authenticated, please log in!", 2);
                }
                User user = userManager.GetUser(authUserId);
                JsonModels.Reference exp = userManager.AddReference(user.id, firstName, lastName, company, email, title, message, videoLink, videoType);
                activityManager.AddActivity(user.id, "Reference", "Added", exp.id);
                return AddSuccessHeader(Serialize(exp));
 

            }
            catch (Exception e)
            {
                logAccessor.CreateLog(DateTime.Now, "userController - AddReference", e.StackTrace);
                return AddErrorHeader("Something went wrong while adding the reference", 1);
            }
        }

        [AcceptVerbs("POST", "OPTIONS")]
        [AllowCrossSiteJson]
        public string UpdateReference(int referenceId = -1, string propertyId = null, string propertyValue = null , string token = null)
        {
            if (Request.RequestType.Equals("OPTIONS", StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }
            try
            {
                int authUserId = -1;
                if (token != null)
                {
                    authUserId = authenticationEngine.authenticate(token);
                }
                else
                {
                    return AddErrorHeader("An authentication token must be passed in", 2);
                }
                if (authUserId < 0)
                {
                    return AddErrorHeader("You are not authenticated, please log in!", 2);
                }
                Reference reference = new Reference();
                if (referenceId > 0)
                {
                    reference = userManager.GetReference(referenceId);
                    if (reference == null)
                    {
                        return AddErrorHeader("The reference with the given referenceId does not exist", 1);
                    }
                      
                }
                else
                {
                    return AddErrorHeader("A referenceId must be passed in", 1);
                }
                if (reference.userId != authUserId)
                {
                    return AddErrorHeader("User is not authorized to exit this reference", 3);
                }
                System.Reflection.PropertyInfo pi;
                if(propertyId == null)
                {
                    return AddErrorHeader("A propertyId must be passed in", 1);
                }
                else
                {
                    pi = reference.GetType().GetProperty(propertyId);
                }
                if(propertyValue == null)
                {
                    return AddErrorHeader("A propertyValue must be passed in", 1);
                }
                else
                {
                    pi.SetValue(reference, Convert.ChangeType(propertyValue, pi.PropertyType), null);
                    userManager.UpdateReference(reference);
                    return AddSuccessHeader("Experience with id: " + reference.id + " has been successfully updated", true);
                }

            }
            catch(Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, "userController - updateReference", ex.StackTrace);
                return AddErrorHeader("Something went wrong while updating this reference element", 1);
            }
        }

        [AcceptVerbs("POST", "OPTIONS")]
        [AllowCrossSiteJson]
        public string UpdateReferenceModel(IEnumerable<JsonModels.Reference> reference, string token = null)
        {
            if (Request.RequestType.Equals("OPTIONS", StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }
            try
            {
                int authUserId = -1;
                if (token != null)
                {
                    authUserId = authenticationEngine.authenticate(token);
                }
                else
                {
                    return AddErrorHeader("An authentication token must be passed in", 2);
                }
                if (authUserId < 0)
                {
                    return AddErrorHeader("You are not authenticated, please log in!", 2);
                }
                if (reference != null)
                {
                    JsonModels.Reference referenceFromJson = reference.FirstOrDefault();
                    if (referenceFromJson != null)
                    {
                        Reference originalReference = userManager.GetReference(referenceFromJson.id);
                        if (originalReference != null)
                        {
                            if (originalReference.userId == authUserId)
                            {
                                if (referenceFromJson.company != null)
                                {
                                    originalReference.company = (referenceFromJson.company == "null") ? null : referenceFromJson.company;
                                }
                                if (referenceFromJson.email != null)
                                {
                                    originalReference.email = (referenceFromJson.email == "null") ? null : referenceFromJson.email;
                                }
                                if (referenceFromJson.firstName != null)
                                {
                                    originalReference.firstName = (referenceFromJson.firstName == "null") ? null : referenceFromJson.firstName;
                                }
                                if (referenceFromJson.lastName != null)
                                {
                                    originalReference.lastName = (referenceFromJson.lastName == "null") ? null : referenceFromJson.lastName;
                                }
                                if (referenceFromJson.message != null)
                                {
                                    originalReference.message = (referenceFromJson.message == "null") ? null : referenceFromJson.message;
                                }
                                if (referenceFromJson.title != null)
                                {
                                    originalReference.title = (referenceFromJson.title == "null") ? null : referenceFromJson.title;
                                }
                                if (referenceFromJson.videoLink != null)
                                {
                                    originalReference.videoLink = (referenceFromJson.videoLink == "null") ? null : referenceFromJson.videoLink;
                                }
                                if (referenceFromJson.videoType != null)
                                {
                                    originalReference.videoType = (referenceFromJson.videoType == "null") ? null : referenceFromJson.videoType;
                                }
                                originalReference.id = referenceFromJson.id;

                                userManager.UpdateReference(originalReference);
                                activityManager.AddActivity(authUserId, "Reference", "Updated", originalReference.id);
                                return AddSuccessHeader(Serialize(referenceFromJson));
                            }
                            else
                            {
                                return AddErrorHeader("You are not authorized to edit this reference.", 1);
                            }
                        }
                        else
                        {
                            return AddErrorHeader("The reference model was not found in the database", 1);
                        }
                    }
                    else
                    {
                        return AddErrorHeader("The reference model was not valid", 1);
                    }
                }
                else
                {
                    return AddErrorHeader("An reference model was not received", 1);
                }
            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, "userController - updatereference", ex.StackTrace);
                return AddErrorHeader("Something went wrong while updating this reference model", 1);
            }
        }

        [AcceptVerbs("POST", "OPTIONS")]
        [AllowCrossSiteJson]
        public string GetReference(int referenceId = -1, string token = null)
        {
            if (Request.RequestType.Equals("OPTIONS", StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }
            try
            {
                int authUserId = -1;
                if (token != null)
                {
                    authUserId = authenticationEngine.authenticate(token);
                }
                else
                {
                    return AddErrorHeader("An authentication token must be passed in", 2);
                }
                if (authUserId < 0)
                {
                    return AddErrorHeader("You are not authenticated, please log in!", 2);
                }
                if (referenceId < 0)
                {
                    return AddErrorHeader("An referenceId must be passed in", 1);
                }
                else
                {
                    JsonModels.Reference refJson = userManager.GetReferenceJson(referenceId);
                    if (refJson == null)
                    {
                        return AddErrorHeader("The reference was not found in the database", 1);
                    }
                    else
                    {
                        return AddSuccessHeader(Serialize(refJson));
                    }
                }
            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, "UserController - GetReference", ex.StackTrace);
                return AddErrorHeader("Something went wrong while getting this reference", 1);
            }
        }

        [AcceptVerbs("POST", "OPTIONS")]
        [AllowCrossSiteJson]
        public string DeleteReference(int referenceId = -1, string token = null)
        {
            if (Request.RequestType.Equals("OPTIONS", StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }
            try
            {
                int authUserId = -1;
                if (token != null)
                {
                    authUserId = authenticationEngine.authenticate(token);
                }
                else
                {
                    return AddErrorHeader("An authentication token must be passed in", 2);
                }
                if (authUserId < 0)
                {
                    return AddErrorHeader("You are not authenticated, please log in!", 2);
                }
                if (referenceId < 0)
                {
                    return AddErrorHeader("An referenceId must be passed in", 1);
                }
                else
                {
                    Reference reference = userManager.GetReference(referenceId);
                    if (reference == null)
                    {
                        return AddErrorHeader("The reference was not found in the database", 1);
                    }
                    string response = userManager.DeleteReference(reference);
                    if (response == null)
                    {
                        return AddErrorHeader("Something went wrong while deleting this reference", 1);
                    }
                    else
                    {
                        return AddSuccessHeader(response, true);
                    }
                }
            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, "UserController - DeleteReference", ex.StackTrace);
                return AddErrorHeader("Something went wrong while deleting this reference", 1);
            }
        }

        [AcceptVerbs("POST", "OPTIONS")]
        [AllowCrossSiteJson]
        public string GetProfileScore(int userId, string token = null)
        {
            if (Request.RequestType.Equals("OPTIONS", StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }
            try
            {
                int authUserId = -1;
                if (token != null)
                {
                    authUserId = authenticationEngine.authenticate(token);
                }
                User u = userManager.GetUser(userId);
                JsonModels.ProfileScore profileScoreJson = userManager.GetProfileScore(u);
                return AddSuccessHeader(Serialize(profileScoreJson));
            }
            catch (Exception ex)
            {
                return AddErrorHeader("Something went wrong while fetching this profile score", 1);
            }
        }

        [AcceptVerbs("POST", "OPTIONS")]
        [AllowCrossSiteJson]
        public string SetUserPrivacy(string privacy, string token)
        {
            if (Request.RequestType.Equals("OPTIONS", StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }
            User user = null;
            try
            {
                int authUserId = -1;
                if (token != null)
                {
                    authUserId = authenticationEngine.authenticate(token);
                }
                else
                {
                    return AddErrorHeader("A token must be passed in", 2);
                }
                if (authUserId < 0)
                {
                    return AddErrorHeader("The authenticaiton token is not valid", 2);
                }
                user = userManager.GetUser(authUserId);
                if (privacy == "public")
                {
                    user.isPublic = 1;
                }
                else if (privacy == "network")
                {
                    user.isPublic = 0;
                }
                userManager.UpdateUser(user);
                //JsonModels.ProfileInformation returnProfile = userManager.GetProfileJson(user);
                //return AddSuccessHeader(Serialize(returnProfile));
                return AddSuccessHeader("Privacy settings changed", true);
            }
            catch (Exception e)
            {
                return AddErrorHeader("Error saving user privacy settigns", 1);
            }
        }

        [AcceptVerbs("POST", "OPTIONS")]
        [AllowCrossSiteJson]
        public string GetUserModel(int userId, string token)
        {
            if (Request.RequestType.Equals("OPTIONS", StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }
            try
            {
                int authUserId = -1;
                if (token != null)
                {
                    authUserId = authenticationEngine.authenticate(token);
                }
                else
                {
                    return AddErrorHeader("A token must be passed in", 2);
                }
                if (authUserId < 0)
                {
                    return AddErrorHeader("The authenticaiton token is not valid", 2);
                }

                if (authUserId != userId)
                {
                    return AddErrorHeader("User must be profile owner to retrieve the User Model", 1);
                }
                User user = userManager.GetUserWithNetworks(userId);
                if (user != null)
                {
                    NetworkManager networkManager = new NetworkManager();
                    JsonModels.UserInformationModel userInfoJson = new JsonModels.UserInformationModel();
                    JsonModels.UserSettings userSettingsJson = new JsonModels.UserSettings();
                    List<JsonModels.UserNetworkShell> userNetworksJson = new List<JsonModels.UserNetworkShell>();

                    List<Network> topLevelNetworks = new List<Network>();
                    List<Network> subNetworks = new List<Network>();
                    List<Network> groupNetworks = new List<Network>();

                    foreach (Network n in user.networks)
                    {
                        if (n != null)
                        {
                            if (n.GetType().Name.Contains("Network_TopNetwork"))
                            {
                                if (!topLevelNetworks.Contains(n))
                                {
                                    topLevelNetworks.Add(n);
                                }
                            }
                            else if (n.GetType().Name.Contains("Network_SubNetwork"))
                            {
                                if (!subNetworks.Contains(n))
                                {
                                    subNetworks.Add(n);
                                }
                            }
                            else if (n.GetType().Name.Contains("Network_Group"))
                            {
                                if (!groupNetworks.Contains(n))
                                {
                                    groupNetworks.Add(n);
                                }
                            }
                        }
                    }
                    foreach (Network n in user.adminNetworks)
                    {
                        if (n != null)
                        {
                            if (n.GetType().Name.Contains("Network_TopNetwork"))
                            {
                                if (!topLevelNetworks.Contains(n))
                                {
                                    topLevelNetworks.Add(n);
                                }
                            }
                            else if (n.GetType().Name.Contains("Network_SubNetwork"))
                            {
                                if (!subNetworks.Contains(n))
                                {
                                    subNetworks.Add(n);
                                }
                            }
                            else if (n.GetType().Name.Contains("Network_Group"))
                            {
                                if (!groupNetworks.Contains(n))
                                {
                                    groupNetworks.Add(n);
                                }
                            }
                        }
                    }

                    for (int i = 0; i < topLevelNetworks.Count; i++)
                    {
                        Network topNet = topLevelNetworks.ElementAt(i);
                        JsonModels.UserNetworkShell topNetShell = new JsonModels.UserNetworkShell();
                        topNetShell.name = topNet.name;
                        topNetShell.networkId = topNet.id;
                        topNetShell.networkURL = topNet.profileURL;
                        if (user.adminNetworks.Contains(topNet))
                        {
                            topNetShell.userAuthorization = "admin";
                        }
                        else
                        {
                            topNetShell.userAuthorization = "member";
                        }

                        for (int j = 0; j < subNetworks.Count; j++)
                        {
                            Network_SubNetwork subNet = (Network_SubNetwork)subNetworks.ElementAt(j);
                            JsonModels.UserNetworkShell subNetShell = new JsonModels.UserNetworkShell();
                            if (subNet.Network_TopNetwork.id == topNet.id)
                            {
                                subNetShell.name = subNet.name;
                                subNetShell.networkId = subNet.id;
                                subNetShell.networkURL = subNet.profileURL;
                                if (user.adminNetworks.Contains(subNet))
                                {
                                    subNetShell.userAuthorization = "admin";
                                }
                                else
                                {
                                    subNetShell.userAuthorization = "member";
                                }

                                for (int k = 0; k < groupNetworks.Count; k++)
                                {
                                    Network_Group groupNet = (Network_Group)groupNetworks.ElementAt(k);
                                    if (groupNet.Network_SubNetwork.id == subNet.id)
                                    {
                                        JsonModels.UserNetworkShell groupNetShell = new JsonModels.UserNetworkShell();
                                        groupNetShell.name = groupNet.name;
                                        groupNetShell.networkId = groupNet.id;
                                        groupNetShell.networkURL = groupNet.profileURL;
                                        if (user.adminNetworks.Contains(groupNet))
                                        {
                                            groupNetShell.userAuthorization = "admin";
                                        }
                                        else
                                        {
                                            groupNetShell.userAuthorization = "member";
                                        }
                                        if (subNetShell.subnetworks == null)
                                        {
                                            subNetShell.subnetworks = new List<JsonModels.UserNetworkShell>();
                                        }
                                        subNetShell.subnetworks.Add(groupNetShell);
                                    }
                                }
                                if (topNetShell.subnetworks == null)
                                {
                                    topNetShell.subnetworks = new List<JsonModels.UserNetworkShell>();
                                }
                                topNetShell.subnetworks.Add(subNetShell);

                            }
                        }

                        userNetworksJson.Add(topNetShell);

                    }
                    userSettingsJson.email = user.email;
                    userSettingsJson.profileURL = user.profileURL;
                    userSettingsJson.firstName = user.firstName;
                    userSettingsJson.lastName = user.lastName;
                    if(user.isPublic == 1)
                    {
                        userSettingsJson.visibility = "visible";
                    }
                    else
                    {
                        userSettingsJson.visibility = "hidden";
                    }

                    userInfoJson.networks = userNetworksJson;
                    userInfoJson.settings = userSettingsJson;

                    return AddSuccessHeader(Serialize(userInfoJson));
                }
                else
                {
                    return AddErrorHeader("User not found", 1);
                }
            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, "UserController - GetUserModel", ex.StackTrace);
                return AddErrorHeader("something went wrong while retrieving this user's info", 1);
            }
        }

        public void sendTestEmail(string to, string subject, string greeting, string body)
        {
            communicationManager.SendTestEmail(to, subject, greeting, body);
        }

        public ActionResult testNewRegister()
        {
            return View();
        }

        public ActionResult testLogon()
        {
            return View();
        }

    }

}
