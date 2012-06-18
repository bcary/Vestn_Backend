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

        //User Account Creation

        public ActionResult fileexist()
        {
            return View();
        }
        [AcceptVerbs("POST", "OPTIONS")]
        public string CheckFileExist(String url, string token)
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
                    //int userId = authenticationEngine.authenticate(token);
                    //if (userId < 0)
                    //{
                    //    return GetFailureMessage("You are not authenticated, please log in!");
                    //}
                    HttpWebRequest request = (HttpWebRequest)System.Net.WebRequest.Create(url);
                    bool found = false;
                    int counter = 0;
                    while (!found)
                    {
                        try
                        {
                            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                            if (response.StatusCode != HttpStatusCode.NotFound)
                            {
                                found = true;
                                Thread.Sleep(300);
                            }

                        }
                        catch (System.Net.WebException)
                        {
                            Thread.Sleep(500);
                            counter++;
                            if (counter > 40)
                            {
                                return GetFailureMessage("Check File Timeout. Either this URL will not exist, or the server is suuuper slow");
                            }
                        }
                    }
                    return AddSuccessHeaders("File Exists", true);
                }
                catch (Exception ex)
                {
                    logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), ex.ToString());
                    return GetFailureMessage("Unknown error - CheckFileExist");
                }
            }
        }


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
        //            return GetFailureMessage("Invalid Email");
        //            //return View(model);
        //        }
        //        if (!userManager.CheckDuplicateEmail(model.Email))
        //        {
        //            TempData["MessageBar"] = "A user with that email already exists in our database";
        //            return GetFailureMessage("A user with that email already exists in our database");
        //            //return View(model);
        //        }
        //        if (ValidationEngine.ValidateUsername(model.UserName) != ValidationEngine.Success)
        //        {
        //            TempData["MessageBar"] = ValidationEngine.ValidateUsername(model.UserName);
        //            return GetFailureMessage(ValidationEngine.ValidateUsername(model.UserName));
        //            //return View(model);
        //        }
        //        if (!userManager.CheckDuplicateUsername(model.UserName))
        //        {
        //            TempData["MessageBar"] = "A user with that username already exists in our database";
        //            return GetFailureMessage("A user with that username already exists in our database");

        //            //return View(model);
        //        }
        //        if (ValidationEngine.ValidatePassword(model.Password) != ValidationEngine.Success)
        //        {
        //            TempData["MessageBar"] = ValidationEngine.ValidatePassword(model.Password);
        //            return GetFailureMessage(ValidationEngine.ValidateUsername(model.Password));
        //            //return View(model);
        //        }
        //        if (model.Password != model.ConfirmPassword)
        //        {
        //            TempData["MessageBar"] = "Password fields do not match";
        //            return GetFailureMessage("Password fields do not match");

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
        //        return AddSuccessHeaders("\"Register Successful\"");
        //        //return RedirectToAction("Profile", "User");
        //    }
        //    catch (Exception ex)
        //    {
        //        logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), ex.ToString());
        //        return GetFailureMessage("Error");
        //    }
        //}

        [HttpPost]
        public string Register(string Email, string Password)
        {
            try
            {
                CommunicationManager communicationManager = new CommunicationManager();
                //if (ValidationEngine.ValidateBetaKey(model.betaKey) != ValidationEngine.Success)
                //{
                //    TempData["MessageBar"] = ValidationEngine.ValidateBetaKey(model.betaKey);
                //    return View(model);
                //}
                string userName = Email.Substring(0, Email.IndexOf('@'));
                userName = userName.Replace("+", "");
                RegisterModel model = new RegisterModel { Email = Email, UserName = userName, Password = Password, ConfirmPassword = Password };
                if (ValidationEngine.ValidateEmail(model.Email) != ValidationEngine.Success || ValidationEngine.IsDuplicateEmail(model.Email) == true)
                {
                    TempData["MessageBar"] = "Invalid Email";
                    return GetFailureMessage("Invalid Email");
                    //return View(model);
                }
                if (!userManager.CheckDuplicateEmail(model.Email))
                {
                    TempData["MessageBar"] = "A user with that email already exists in our database";
                    return GetFailureMessage("A user with that email already exists in our database");
                    //return View(model);
                }
                if (ValidationEngine.ValidateUsername(model.UserName) != ValidationEngine.Success)
                {
                    TempData["MessageBar"] = ValidationEngine.ValidateUsername(model.UserName);
                    return GetFailureMessage(ValidationEngine.ValidateUsername(model.UserName));
                    //return View(model);
                }
                if (!userManager.CheckDuplicateUsername(model.UserName))
                {
                    TempData["MessageBar"] = "A user with that username already exists in our database";
                    return GetFailureMessage("A user with that username already exists in our database");

                    //return View(model);
                }
                if (ValidationEngine.ValidatePassword(model.Password) != ValidationEngine.Success)
                {
                    TempData["MessageBar"] = ValidationEngine.ValidatePassword(model.Password);
                    return GetFailureMessage(ValidationEngine.ValidateUsername(model.Password));
                    //return View(model);
                }
                if (model.Password != model.ConfirmPassword)
                {
                    TempData["MessageBar"] = "Password fields do not match";
                    return GetFailureMessage("Password fields do not match");

                    //return View(model);
                }
                if (ModelState.IsValid)
                {
                    User newUser = model.toUser();
                    newUser.profileURL = newUser.userName;

                    newUser = userManager.CreateUser(newUser, model.Password);

                    userManager.ActivateUser(newUser, true);
                    communicationManager.SendVerificationMail(userManager.GetProviderUserKey(newUser), newUser.userName, newUser.email);

                    FormsAuthentication.SetAuthCookie(newUser.userName, false /* createPersistentCookie */);
                    JsonModels.LogOnModel logOnModel = new JsonModels.LogOnModel { id = newUser.id };
                    return AddSuccessHeaders(Serialize(logOnModel));
                }
                else
                {
                    return GetFailureMessage("Error creating account.");
                }

                //return RedirectToAction("Profile", "User");
            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), ex.ToString());
                return GetFailureMessage("Error");
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
            string result = tm.AddTag("User", value, type, userManager.GetUser(User.Identity.Name).id);
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
                    Boolean rememberme = false;
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


        [Authorize]
        public ActionResult ChangePassword()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public ActionResult ChangePassword(ChangePasswordModel model)
        {
            if (ModelState.IsValid)
            {
                User user = userManager.GetUser(User.Identity.Name);
                bool changePasswordSucceeded = userManager.ChangePassword(user, model.OldPassword, model.NewPassword);

                if (changePasswordSucceeded)
                {
                    TempData.Add("Popup", "Your password has been changed successfully.");
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData.Add("MessageBar", "Your password change has failed.");
                    return RedirectToAction("Index");
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

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

                ViewBag.WillingToRelocate = new List<string>(Enum.GetNames(typeof(WillingToRelocateType)));

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

        //[Authorize]
        [AcceptVerbs("POST","OPTIONS")]
        [AllowCrossSiteJson]
        public string UpdateUser(int userId, string propertyId, string propertyValue, string token)
        {
            if (Request.RequestType.Equals("OPTIONS", StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }
            try
            {
                int authUserId = authenticationEngine.authenticate(token);
                if (authUserId < 0)
                {
                    return GetFailureMessage("You are not authenticated, please log in!");
                }
                User user = userManager.GetUser(userId);
                //uncomment this when authentication works
                if (userId == authUserId)
                {
                    if (user == null)
                    {
                        return GetFailureMessage("User not found");
                    }

                    System.Reflection.PropertyInfo pi = null;
                    if (propertyId != null)
                    {
                        pi = user.GetType().GetProperty(propertyId);
                    }
                    else
                    {
                        GetFailureMessage("You must pass in a propertyId to set");
                    }

                    if (pi == null)
                    {
                        return GetFailureMessage("Invalid propertyId");
                    }
                    else
                    {
                        try
                        {
                            if (propertyValue != null)
                            {
                                propertyValue = StripNewLineAndReplaceWithLineBreaks(propertyValue);
                            }
                            else
                            {
                                return GetFailureMessage("You must pass in a propertyValue to set");
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
                                            return GetFailureMessage("profileURL not valid, user not updated");
                                        }
                                    }
                                    else
                                    {
                                        return GetFailureMessage("profileURL already in use, user not updated");
                                    }
                                    break;
                                case "school":
                                    if (ValidationEngine.ValidateSchool(propertyValue) == ValidationEngine.Success)
                                    {
                                        pi.SetValue(user, Convert.ChangeType(propertyValue, pi.PropertyType), null);
                                    }
                                    else
                                    {
                                        return GetFailureMessage("school not valid, user not updated");
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
                                            return GetFailureMessage("email not valid, user not updated");
                                        }
                                    }
                                    else
                                    {
                                        return GetFailureMessage("email match, user not updated");
                                    }
                                    break;
                                case "location":
                                    if (ValidationEngine.ValidateLocation(propertyValue) == ValidationEngine.Success)
                                    {
                                        pi.SetValue(user, Convert.ChangeType(propertyValue, pi.PropertyType), null);
                                    }
                                    else
                                    {
                                        return GetFailureMessage("location not valid, user not updated");
                                    }
                                    break;
                                case "firstName":
                                    if (ValidationEngine.ValidateFirstName(propertyValue) == ValidationEngine.Success)
                                    {
                                        pi.SetValue(user, Convert.ChangeType(propertyValue, pi.PropertyType), null);
                                    }
                                    else
                                    {
                                        return GetFailureMessage("firstName not valid, user not updated");
                                    }
                                    break;
                                case "lastName":
                                    if (ValidationEngine.ValidateLastName(propertyValue) == ValidationEngine.Success)
                                    {
                                        pi.SetValue(user, Convert.ChangeType(propertyValue, pi.PropertyType), null);
                                    }
                                    else
                                    {
                                        return GetFailureMessage("lastName not valid, user not updated");
                                    }
                                    break;
                                case "title":
                                    if (ValidationEngine.ValidateTitle(propertyValue) == ValidationEngine.Success)
                                    {
                                        pi.SetValue(user, Convert.ChangeType(propertyValue, pi.PropertyType), null);
                                    }
                                    else
                                    {
                                        return GetFailureMessage("title not valid, user not updated");
                                    }
                                    break;
                                case "major":
                                    if (ValidationEngine.ValidateMajor(propertyValue) == ValidationEngine.Success)
                                    {
                                        pi.SetValue(user, Convert.ChangeType(propertyValue, pi.PropertyType), null);
                                    }
                                    else
                                    {
                                        return GetFailureMessage("major not valid, user not updated");
                                    }
                                    break;
                                case "connections":
                                    if (ValidationEngine.ValidateMajor(propertyValue) == ValidationEngine.Success)
                                    {
                                        pi.SetValue(user, Convert.ChangeType(propertyValue, pi.PropertyType), null);
                                    }
                                    else
                                    {
                                        return GetFailureMessage("connections not valid, user not updated");
                                    }
                                    break;
                                case "description":
                                    if (ValidationEngine.ValidateDescription(propertyValue) == ValidationEngine.Success)
                                    {
                                        pi.SetValue(user, Convert.ChangeType(propertyValue, pi.PropertyType), null);
                                    }
                                    else
                                    {
                                        return GetFailureMessage("description not valid, user not updated");
                                    }
                                    break;
                                case "tagLine":
                                    if (ValidationEngine.ValidateDescription(propertyValue) == ValidationEngine.Success)
                                    {
                                        pi.SetValue(user, Convert.ChangeType(propertyValue, pi.PropertyType), null);
                                    }
                                    else
                                    {
                                        return GetFailureMessage("tagLine not valid, user not updated");
                                    }
                                    break;

                            }
                            //persist user model to DB with manager updateUser method
                            user = userManager.UpdateUser(user);
                            AnalyticsAccessor aa = new AnalyticsAccessor();
                            aa.CreateAnalytic("User Update", DateTime.Now, user.userName, "Information updated: " + pi.PropertyType.ToString());

                            return AddSuccessHeaders("UserId:"+userId+" successfully updated", true);
                        }
                        catch (Exception exc)
                        {
                            logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), exc.ToString());
                            return GetFailureMessage("Something went wrong while updating this user");
                        }
                    }
                }
                else
                {
                    return GetFailureMessage("User not authorized to edit this user");
                }
            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), ex.ToString());
                return GetFailureMessage("Something went wrong while updating this user");
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
                user.school = school;
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


        /*
        //User Managing Their AccessKeys
        [Authorize]
        public ActionResult AccessKeys()
        {
            try
            {
                User user = userManager.GetUser(User.Identity.Name);
                List<AccessKey> accessKeyList = userManager.GetAllAccessKeys(user);

                List<AccessKeysModel> modelList = new List<AccessKeysModel>();
                foreach (AccessKey accessKey in accessKeyList)
                {
                    modelList.Add(new AccessKeysModel(accessKey));
                }

                return View(modelList);
            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), ex.ToString());
                return View("Error");
            }
        }

        [Authorize]
        public ActionResult AccessKeyCreate()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public ActionResult AccessKeyCreate(AccessKeysModel model)
        {
            try
            {
                User loggedInUser = userManager.GetUser(User.Identity.Name);
                AccessKey accessKey = model.toAccessKey();

                userManager.CreateAccessKey(loggedInUser, accessKey);

                return RedirectToAction("AccessKeys");
            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), ex.ToString());
                return View("Error");
            }
        }


        [Authorize]
        public ActionResult AccessKeyUpdate(int id)
        {
            try
            {
                AccessKey newAccessKey = userManager.GetAccessKey(id);
                AccessKeysModel model = new AccessKeysModel(newAccessKey);
                return View(model);
            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), ex.ToString());
                return View("Error");
            }
        }

        [Authorize]
        [HttpPost]
        public ActionResult AccessKeyUpdate(AccessKeysModel model)
        {
            try
            {
                AccessKey newAccessKey = model.toAccessKey();
                userManager.UpdateAccessKey(newAccessKey);

                return RedirectToAction("AccessKeys");
            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), ex.ToString());
                return View("Error");
            }
        }

        [Authorize]
        public ActionResult AccessKeyDelete(int id)
        {
            try
            {
                AccessKey accessKey = userManager.GetAccessKey(id);
                AccessKeysModel accessKeysModel = new AccessKeysModel(accessKey);
                return View(accessKeysModel);
            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), ex.ToString());
                return View("Error");
            }
        }

        [Authorize]
        [HttpPost, ActionName("AccessKeyDelete")]
        public ActionResult AccessKeyDeleteConfirmed(int id)
        {
            try
            {
                User user = userManager.GetUser(User.Identity.Name);
                AccessKey accessKey = userManager.GetAccessKey(id);
                accessKey = userManager.DeleteAccessKey(user, accessKey);
                return RedirectToAction("AccessKeys");
            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), ex.ToString());
                return View("Error");
            }
        }
        */


        //Dispose

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        //Sample user creation

        //public ActionResult CreateSampleUser()
        //{
        //    string password = "wongwong";
        //    User user = createMohammadWong(password);
        //    LogOnModel model = new LogOnModel()
        //    {
        //        UserName = user.userName,
        //        Password = password,
        //        RememberMe = false
        //    };
        //    return LogOn(model, null);
        //}

        //private User createMohammadWong(string password)
        //{
        //    UserManager userManager = new UserManager();
        //    ProjectManager projectManager = new ProjectManager();
        //    TagManager tagManager = new TagManager();


        //    User user = new User()
        //    {
        //        userName = "mwong",
        //        birthDate = DateTime.Now,
        //        email = "m.wong@gmail.com",
        //        firstName = "Mohammad",
        //        lastName = "Wong",
        //        location = "Cupertino",
        //        phoneNumber = "8675309",
        //        school = "Peking University",
        //        major = "English/Psychology",
        //        willingToRelocate = WillingToRelocateType.yes
        //    };

        //    //delete any existing mwongs
        //    User userToDelete = userManager.GetUser(user.userName);
        //    if (userToDelete != null)
        //    {
        //        userManager.DeleteUser(userToDelete);
        //    }

        //    user = userManager.CreateUser(user, password);
        //    userManager.ActivateUser(user, true);

        //    //profile picture
        //    string fileLocation = @"http://newsletter.thebikeboutique.com/200910-TBB-Gobal-Newsletter-Issue-14/images/wong2.jpg";
        //    WebClient client = new WebClient();
        //    byte[] data = client.DownloadData(fileLocation);
        //    Stream stream = new MemoryStream(data);
        //    userManager.UploadUserPicture(user, stream, "Profile");



        //    //tags
        //    Tag[] tagList = tagManager.GetAllLowestLevel().ToArray();
        //    Tag randomTag1 = tagList[DateTime.Now.Millisecond % tagList.Length];
        //    Tag randomTag2 = tagList[DateTime.Now.Second % tagList.Length];
        //    userManager.addTag(user, randomTag1.value);
        //    userManager.addTag(user, randomTag2.value);

        //    //projects
        //    List<ProjectElement> projectElements = new List<ProjectElement>();

        //    ProjectElement_Experience projectElement_Experience = new ProjectElement_Experience()
        //    {
        //        jobTitle = "Poet",
        //        description = "Poetry writer",
        //        startDate = new DateTime(2010, 3, 14),
        //        endDate = new DateTime(2010, 9, 11)
        //    };

        //    ProjectElement_Information projectElement_Information = new ProjectElement_Information()
        //    {
        //        description = "I wrote and continue to write soulful stanzas.",
        //        email = user.email,
        //        location = "Paris (on the sidewalks and stuff)",
        //        major = "French Women",
        //        minor = "Art",
        //        phone = user.phoneNumber,
        //        school = "University of Paris"
        //    };
        //    ProjectElement_Video projectElement_Video = new ProjectElement_Video
        //    {
        //        //put a test id you know in here haun
        //        videoId = "xxxxx",
        //        description = "asdfsadfasdf"
        //    };

        //    projectElements.Add(projectElement_Information);
        //    projectElements.Add(projectElement_Experience);
        //    projectElements.Add(projectElement_Video);

        //    //about project
        //    /*
        //    Project aboutProject = userManager.GetUser(user.userName).projects.First();
        //    List<ProjectElement> aboutProjectElements = new List<ProjectElement>();
        //    aboutProjectElements.Add(projectElement_Information);
        //    aboutProjectElements.Add(projectElement_Experience);
        //    aboutProject.projectElements = aboutProjectElements;
        //    projectManager.UpdateProject(aboutProject);
        //    */

        //    projectManager.CreateProject(user, projectElements);
        //    //projectManager.CreateProject(user, projectElements);
        //    //projectManager.CreateProject(user, projectElements);
        //    //projectManager.CreateProject(user, projectElements);

        //    //accessKeys
        //    AccessKey accessKey = new AccessKey()
        //    {
        //        accessString = "public",
        //        infoVisible = true,
        //        pictureVisible = true,
        //        projectVisible = new List<bool>() { true, true, true, true }
        //    };

        //    userManager.CreateAccessKey(user, accessKey);

        //    return userManager.GetUser(user.id);
        //}

        [Authorize]
        [HttpPost]
        public JsonResult UpdateProfilePicture()
        {
            try
            {
                User user = userManager.GetUser(User.Identity.Name);

                if (Request != null)
                {
                    if (Request.Files.Count == 0)
                    {
                        return Json(new { Error = "No files submitted to server" });
                    }
                    else if (Request.Files[0].ContentLength == 0)
                    {
                        return Json(new { Error = "No files submitted to server" });
                    }

                    foreach (string inputFileId in Request.Files)
                    {
                        HttpPostedFileBase file = Request.Files[inputFileId];
                        if (file.ContentLength > 0)
                        {

                            System.IO.Stream fs = file.InputStream;

                            if (ValidationEngine.ValidatePicture(file) != ValidationEngine.Success)
                            {
                                return Json(new { Error = ValidationEngine.ValidatePicture(file) });
                            }

                            if (user.profilePicture != null && user.profilePictureThumbnail != null)
                            {
                                userManager.DeleteProfilePicture(user);
                            }
                            user = userManager.UploadUserPicture(user, fs, "Profile");
                        }
                    }
                }
                else
                {
                    return Json(new { Error = "Server did not receive file post" });
                }

                return Json(new { PictureLocation = "http://vestnstorage.blob.core.windows.net/images/uploadSuccessful2.png" });

                //return Json(new { Status = "success", responseText = RedirectToAction("Profile", "User", new ProfileModel(user)) });
            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), ex.ToString());
                return Json(new { Error = "Problem saving to cloud" });
            }
        }



        [Authorize]
        public JsonResult UpdateResume()
        {
            try
            {
                User user = userManager.GetUser(User.Identity.Name);

                if (Request != null)
                {
                    if (Request.Files.Count == 0)
                    {
                        return Json(new { Error = "No files submitted to server" });
                    }
                    else if (Request.Files[0].ContentLength == 0)
                    {
                        return Json(new { Error = "No files submitted to server" });
                    }

                    foreach (string inputFileId in Request.Files)
                    {
                        HttpPostedFileBase file = Request.Files[inputFileId];
                        if (file.ContentLength > 0)
                        {
                            System.IO.Stream fs = file.InputStream;
                            string s = file.FileName;
                            string[] s2 = s.Split('.');
                            string fileType = s2[s2.Count() - 1].ToLower();

                            if (ValidationEngine.ValidateDocument(file) != ValidationEngine.Success)
                            {
                                return Json(new { Error = ValidationEngine.ValidateDocument(file) });
                            }

                            if (fileType == "pdf")
                            {
                                user = userManager.UploadResumePDF(user, fs);
                            }
                            else if (fileType == "doc")
                            {
                                user = userManager.UploadResumeDoc(user, fs);
                            }
                            else if (fileType == "docx")
                            {
                                user = userManager.UploadResumeDocx(user, fs);
                            }
                            else if (fileType == "rtf")
                            {
                                user = userManager.UploadResumeRTF(user, fs);
                            }
                            else if (fileType == "txt")
                            {
                                user = userManager.UploadResumeTXT(user, fs);
                            }
                            else
                            {
                                return Json(new { Error = "File type not supported for resumes." });
                            }
                        }
                    }
                }
                else
                {
                    return Json(new { Error = "Server did not receive file post" });
                }

                return Json(new { UpdatedPartial = RenderPartialViewToString("_UserResume", new ProfileModel(user)) });

                //return Json(new { Status = "success", responseText = RedirectToAction("Profile", "User", new ProfileModel(user)) });
            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), ex.ToString());
                return Json(new { Error = "Problem saving to cloud" });
            }
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
        [AcceptVerbs("POST", "OPTIONS")]
        public string GetUserInformation(int[] id, string[] request, string token)
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
                int authenticate = authenticationEngine.authenticate(token);
                if (authenticate < 0)
                {
                    Response.StatusCode = 500;
                    return GetFailureMessage("Not Authenticated");
                }
                

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
                            if (requestAll || request.Contains("experiences"))
                            {
                                List<JsonModels.Experience> experiences = userManager.GetUserExperiences(ID);
                                if (experiences != null && experiences.Count != 0)
                                {
                                    ui.experiences = experiences;
                                    add = 1;
                                }
                            }
                            if (requestAll || request.Contains("references"))
                            {
                                List<JsonModels.Reference> references = userManager.GetUserReferences(ID);
                                if (references != null && references.Count != 0)
                                {
                                    ui.references = references;
                                    add = 1;
                                }
                            }
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
                    catch (Exception ex)
                    {
                        logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), ex.ToString());
                        return GetFailureMessage(ex.Message);
                    }
                }
                catch (Exception ex)
                {
                    logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), ex.ToString());
                    return GetFailureMessage("Bad Request");
                }
                return AddSuccessHeaders(returnVal);
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
                        return GetFailureMessage("You are not authenticated, please log in!");
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
                        return GetFailureMessage("Update Failed.");
                    }
                    else
                    {
                        u.projectOrder = order;
                        u = userManager.UpdateUser(u);
                    }
                    return AddSuccessHeaders("Order updated", true);
                }
                catch (Exception ex)
                {
                    logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), ex.ToString());
                    return GetFailureMessage("Something went wrong while updating the Project Order");
                }
            }
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
