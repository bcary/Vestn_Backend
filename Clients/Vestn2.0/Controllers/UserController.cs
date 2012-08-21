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

        [AcceptVerbs("POST", "OPTIONS")]
        [AllowCrossSiteJson]
        public string CheckFileExist(String url = null, string token = null)
        {
            if (Request.RequestType.Equals("OPTIONS", StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }
            if (String.IsNullOrEmpty(url))
            {
                return AddErrorHeader("No URL specified");
            }
            if (url == "about:blank")
            {
                return AddErrorHeader("about:blank is not a valid url to check");
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
                return AddErrorHeader("Timeout");
            }
            else
            {
                return (AddSuccessHeader("File Exists", true));
            }
        }

        [AcceptVerbs("POST", "OPTIONS")]
        [AllowCrossSiteJson]
        public string Register(string email, string password)
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
                    return AddErrorHeader("Invalid Email");
                }
                if (!userManager.CheckDuplicateEmail(model.Email))
                {
                    return AddErrorHeader("A user with that email already exists in our database");
                }
                if (ValidationEngine.ValidateUsername(model.UserName) != ValidationEngine.Success)
                {
                    return AddErrorHeader(ValidationEngine.ValidateUsername(model.UserName));
                }
                if (!userManager.CheckDuplicateUsername(model.UserName))
                {
                    return AddErrorHeader("A user with that username already exists in our database");
                }
                if (ValidationEngine.ValidatePassword(model.Password) != ValidationEngine.Success)
                {
                    return AddErrorHeader(ValidationEngine.ValidateUsername(model.Password));
                }
                if (model.Password != model.ConfirmPassword)
                {
                    return AddErrorHeader("Password fields do not match");
                }
                if (ModelState.IsValid)
                {
                    User newUser = model.toUser();
                    newUser.profileURL = newUser.userName;

                    newUser = userManager.CreateUser(newUser, model.Password);

                    userManager.ActivateUser(newUser, true);
                    //communicationManager.SendVerificationMail(userManager.GetProviderUserKey(newUser), newUser.userName, newUser.email);

                    AuthenticaitonEngine authEngine = new AuthenticaitonEngine();
                    string token = authEngine.logIn(newUser.id, newUser.userName);
                    JsonModels.RegisterResponse rr = new JsonModels.RegisterResponse();
                    rr.userId = newUser.id;
                    rr.token = token;
                    return AddSuccessHeader(Serialize(rr));
                }
                else
                {
                    return AddErrorHeader("User Model Not Valid");
                }
            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), ex.ToString());
                return AddErrorHeader("Something went wrong while creating this user");
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
                    //mu.ChangePassword(mu.ResetPassword(), "vestn2227");
                    if (user == null)
                    {
                        user = userManager.GetUserByEmail(username);
                        if (user != null)
                        {
                            username = user.userName;
                        }
                        else
                        {
                            return AddErrorHeader("The username/email does not exist in the database");
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

                        return AddSuccessHeader(Serialize(logOnReturnObject));
                    }
                    else
                    {
                        return AddErrorHeader("User Information Not Valid");
                    }
                }
                catch (Exception ex)
                {
                    logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), ex.ToString());
                    return AddErrorHeader("Something went wrong while trying to log this user in");
                }
            }
        }

        [AcceptVerbs("POST", "OPTIONS")]
        [AllowCrossSiteJson]
        public string UpdateProfileModel(JsonModels.ProfileInformation profile, string token = null)
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
                    return AddErrorHeader("An authentication token must be passed in");
                }
                if (authUserId < 0)
                {
                    return AddErrorHeader("You are not authenticated, please log in!");
                }
                JsonModels.ProfileInformation profileFromJson = profile;

                if (profileFromJson.id == authUserId.ToString())
                {
                    User originalProfile = userManager.GetUser(authUserId);
                    if (originalProfile != null)
                    {
                        //model sync
                        originalProfile.description = (profileFromJson.description != null) ? profileFromJson.description : null;
                        originalProfile.email = (profileFromJson.email != null) ? profileFromJson.email : null;
                        //if (profileFromJson.links != null)
                        //{
                        //    originalProfile.facebookLink = (profileFromJson.links.facebookLink != null) ? profileFromJson.links.facebookLink : null;
                        //    originalProfile.twitterLink = (profileFromJson.links.twitterLink != null) ? profileFromJson.links.twitterLink : null;
                        //    originalProfile.linkedinLink = (profileFromJson.links.linkedinLink != null) ? profileFromJson.links.linkedinLink : null;
                        //}

                        originalProfile.firstName = (profileFromJson.firstName != null) ? profileFromJson.firstName : null;
                        originalProfile.lastName = (profileFromJson.lastName != null) ? profileFromJson.lastName : null;
                        originalProfile.location = (profileFromJson.location != null) ? profileFromJson.location : null;
                        originalProfile.major = (profileFromJson.major != null) ? profileFromJson.major : null;
                        originalProfile.phoneNumber = (profileFromJson.phoneNumber != null) ? profileFromJson.phoneNumber : null;
                        originalProfile.projectOrder = (profileFromJson.projectOrder != null) ? profileFromJson.projectOrder : null;
                        originalProfile.resume = (profileFromJson.resume != null) ? profileFromJson.resume : null;
                        originalProfile.organization = (profileFromJson.organization != null) ? profileFromJson.organization : null;
                        originalProfile.tagLine = (profileFromJson.tagLine != null) ? profileFromJson.tagLine : null;
                        originalProfile.title = (profileFromJson.title != null) ? profileFromJson.title : null;

                        userManager.UpdateUser(originalProfile);
                        JsonModels.ProfileInformation returnProfile = userManager.GetProfileJson(originalProfile);
                        return AddSuccessHeader(Serialize(returnProfile));
                    }
                    else
                    {
                        return AddErrorHeader("The user does not exist in the database");
                    }
                }
                else
                {
                    return AddErrorHeader("User is not the profile owner, and thus is not authorized to edit this profile!");
                }
            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, "UserController - UpdateProfile", ex.StackTrace);
                return AddErrorHeader("Something went wrong while updating this Profile.");
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
                    return AddErrorHeader("An authentication token must be passed in");
                }
                if (authUserId < 0)
                {
                    return AddErrorHeader("You are not authenticated, please log in!");
                }
                User user = userManager.GetUser(userId);
                if (user == null)
                {
                    return AddErrorHeader("User not found");
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
                        return AddSuccessHeader("http://vestnstaging.blob.core.windows.net/thumbnails/" + returnPic, true);
                    }
                    else
                    {
                        return AddErrorHeader("No files posted to server");
                    }
                }
                else
                {
                    return AddErrorHeader("The user is not authorized to edit this profile picture");
                }

            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, "UserController - UpdateProfilePicture", ex.StackTrace);
                return AddErrorHeader("Something went wrong while updating this profile picture");
            }
        }

        //[AcceptVerbs("POST", "OPTIONS")]
        //[AllowCrossSiteJson]
        //public string GetProfile(int id = -1, string profileURL = null, string[] request = null, string token = null)
        //{
        //    if (Request.RequestType.Equals("OPTIONS", StringComparison.InvariantCultureIgnoreCase))  //This is a preflight request
        //    {
        //        return null;
        //    }
        //    else
        //    {

        //        //authenticate via token
        //        string returnVal;
        //        int authenticateId = -1;
        //        try
        //        {
        //            if (token != null)
        //            {
        //                int authenticate = authenticationEngine.authenticate(token);
        //                if (authenticate < 0)
        //                {
        //                    //Only return PUBLIC projects
        //                }
        //            }
        //            bool requestAll = false;
        //            if (request == null)
        //            {
        //                requestAll = true;
        //            }
        //            //List<JsonModels.ProfileInformation> userInformationList = new List<JsonModels.ProfileInformation>();
        //            JsonModels.ProfileInformation ui = new JsonModels.ProfileInformation();
        //            int add = 0;
        //            User u;
        //            if (id < 0)
        //            {
        //                if (profileURL != null)
        //                {
        //                    u = userManager.GetUserByProfileURL(profileURL);
        //                    if (u == null)
        //                    {
        //                        return AddErrorHeader("A user with the specified profileURL was not found");
        //                    }
        //                    else
        //                    {
        //                        id = u.id;
        //                        if (id == authenticateId)
        //                        {
        //                            //TODO request is coming from owner, return EVERYTHING
        //                        }
        //                        else
        //                        {
        //                            //TODO need to check if request came from another in the same network or not
        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    return AddErrorHeader("An id or profileURL must be specified");
        //                }
        //            }
        //            else
        //            {
        //                u = userManager.GetUser(id);
        //                if (u == null)
        //                {
        //                    return AddErrorHeader("A user with the specified id was not found");
        //                }
        //                if (id == authenticateId)
        //                {
        //                    //TODO request is coming from owner, return EVERYTHING
        //                }
        //                else
        //                {
        //                    //TODO need to check if request came from another in the same network or not
        //                }
        //            }
        //            if (u != null)
        //            {
        //                add = 0;
        //                //TODO add company
        //                if (requestAll || request.Contains("firstName"))
        //                {
        //                    if (u.firstName != null)
        //                    {
        //                        ui.firstName = u.firstName;
        //                        add = 1;
        //                    }
        //                    else
        //                    {
        //                        ui.firstName = null;
        //                    }
        //                }
        //                if (requestAll || request.Contains("lastName"))
        //                {
        //                    if (u.lastName != null)
        //                    {
        //                        ui.lastName = u.lastName;
        //                        add = 1;
        //                    }
        //                    else
        //                    {
        //                        ui.lastName = null;
        //                    }
        //                }
        //                if (requestAll || request.Contains("phoneNumber"))
        //                {
        //                    if (u.phoneNumber != null)
        //                    {
        //                        ui.phoneNumber = u.phoneNumber;
        //                        add = 1;
        //                    }
        //                }
        //                if (requestAll || request.Contains("tagLine"))
        //                {
        //                    if (u.tagLine != null)
        //                    {
        //                        ui.tagLine = u.tagLine;
        //                        add = 1;
        //                    }
        //                    else
        //                    {
        //                        ui.tagLine = null;
        //                    }
        //                }
        //                if (requestAll || request.Contains("email"))
        //                {
        //                    if (u.email != null)
        //                    {
        //                        ui.email = u.email;
        //                        add = 1;
        //                    }
        //                    else
        //                    {
        //                        ui.email = null;
        //                    }
        //                }
        //                if (requestAll || request.Contains("location"))
        //                {
        //                    if (u.location != null)
        //                    {
        //                        ui.location = u.location;
        //                        add = 1;
        //                    }
        //                    else
        //                    {
        //                        ui.location = null;
        //                    }
        //                }
        //                if (requestAll || request.Contains("title"))
        //                {
        //                    if (u.title != null)
        //                    {
        //                        ui.title = u.title;
        //                        add = 1;
        //                    }
        //                    else
        //                    {
        //                        ui.title = null;
        //                    }
        //                }
        //                if (requestAll || request.Contains("school"))
        //                {
        //                    if (u.organization != null)
        //                    {
        //                        ui.organization = u.organization;
        //                        add = 1;
        //                    }
        //                    else
        //                    {
        //                        ui.organization = null;
        //                    }
        //                }
        //                if (requestAll || request.Contains("major"))
        //                {
        //                    if (u.major != null)
        //                    {
        //                        ui.major = u.major;
        //                        add = 1;
        //                    }
        //                    else
        //                    {
        //                        ui.major = null;
        //                    }
        //                }
        //                if (requestAll || request.Contains("description"))
        //                {
        //                    if (u.description != null)
        //                    {
        //                        ui.description = u.description;
        //                        add = 1;
        //                    }
        //                    else
        //                    {
        //                        ui.description = null;
        //                    }
        //                }
        //                if (requestAll || request.Contains("resume"))
        //                {
        //                    if (u.resume != null)
        //                    {
        //                        ui.resume = u.resume;
        //                        add = 1;
        //                    }
        //                    else
        //                    {
        //                        ui.resume = null;
        //                    }
        //                }
        //                if (requestAll || request.Contains("projectOrder"))
        //                {
        //                    if (u.projectOrder != null)
        //                    {
        //                        ui.projectOrder = u.projectOrder;
        //                        add = 1;
        //                    }
        //                    else
        //                    {
        //                        ui.projectOrder = null;
        //                    }
        //                }
        //                if (requestAll || request.Contains("profilePicture"))
        //                {
        //                    if (u.profilePicture != null)
        //                    {
        //                        ui.profilePicture = u.profilePicture;
        //                        add = 1;
        //                    }
        //                    else
        //                    {
        //                        ui.profilePicture = null;
        //                    }
        //                }
        //                if (requestAll || request.Contains("profilePictureThumbnail"))
        //                {
        //                    if (u.profilePictureThumbnail != null)
        //                    {
        //                        ui.profilePictureThumbnail = u.profilePictureThumbnail;
        //                        add = 1;
        //                    }
        //                    else
        //                    {
        //                        ui.profilePictureThumbnail = null;
        //                    }
        //                }
        //                //TODO actually calculate the stats
        //                if (requestAll || request.Contains("stats"))
        //                {
        //                    JsonModels.UserStats stats = userManager.getUserStats(id);
        //                    if (stats != null)
        //                    {
        //                        ui.stats = stats;
        //                        add = 1;
        //                    }
        //                    else
        //                    {
        //                        ui.stats = null;
        //                    }
        //                }
        //                if (requestAll || request.Contains("links"))
        //                {
        //                    //JsonModels.Links links = userManager.getUserLinks(id);
        //                    //if (links != null)
        //                    //{
        //                    //    ui.links = links;
        //                    //    add = 1;
        //                    //}
        //                    //else
        //                    //{
        //                    //    ui.links = null;
        //                    //}
        //                }
        //                if (requestAll || request.Contains("experiences"))
        //                {
        //                    List<JsonModels.Experience> experiences = userManager.GetUserExperiences(id);
        //                    if (experiences != null && experiences.Count != 0)
        //                    {
        //                        ui.experiences = experiences;
        //                        add = 1;
        //                    }
        //                    else
        //                    {
        //                        ui.experiences = null;
        //                    }
        //                }
        //                if (requestAll || request.Contains("references"))
        //                {
        //                    List<JsonModels.Reference> references = userManager.GetUserReferences(id);
        //                    if (references != null && references.Count != 0)
        //                    {
        //                        ui.references = references;
        //                        add = 1;
        //                    }
        //                    else
        //                    {
        //                        ui.references = null;
        //                    }
        //                }
        //                if (requestAll || request.Contains("tags"))
        //                {
        //                    List<JsonModels.UserTag> tags = userManager.GetUserTags(id);
        //                    if (tags != null && tags.Count != 0)
        //                    {
        //                        ui.tags = tags;
        //                        add = 1;
        //                    }
        //                    else
        //                    {
        //                        ui.tags = null;
        //                    }
        //                }
        //                if (requestAll || request.Contains("projects"))
        //                {
        //                    int[] projectIds = new int[u.projects.Count];
        //                    int count = 0;
        //                    foreach (Project p in u.projects)
        //                    {
        //                        projectIds[count] = p.id;
        //                        count++;
        //                    }
        //                    List<JsonModels.CompleteProject> projects = projectManager.GetCompleteProjects(projectIds);
        //                    if (projects != null && projects.Count != 0)
        //                    {
        //                        ui.projects = projects;
        //                        add = 1;
        //                    }
        //                    else
        //                    {
        //                        ui.projects = null;
        //                    }
        //                }
        //                ////if (requestAll || request.Contains("todo"))
        //                ////{
        //                ////    List<JsonModels.Todo> todoList = userManager.GetTodo(id);
        //                ////    if (todoList != null && todoList.Count != 0)
        //                ////    {
        //                ////        ui.todo = todoList;
        //                ////        add = 1;
        //                ////    }
        //                ////}
        //                //if (requestAll || request.Contains("recentActivity"))
        //                //{
        //                //    List<JsonModels.RecentActivity> recentActivity = userManager.GetRecentActivity(id);
        //                //    if (recentActivity != null && recentActivity.Count != 0)
        //                //    {
        //                //        ui.recentActivity = recentActivity;
        //                //        add = 1;
        //                //    }
        //                //    else
        //                //    {
        //                //        ui.recentActivity = null;
        //                //    }
        //                //}
        //                ui.id = u.id.ToString();
        //            }
        //            try
        //            {
        //                returnVal = Serialize(ui);
        //            }
        //            catch (Exception ex)
        //            {
        //                logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), ex.ToString());
        //                return AddErrorHeader(ex.Message);
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), ex.ToString());
        //            return AddErrorHeader("Bad Request");
        //        }
        //        return AddSuccessHeader(returnVal);
        //    }
        //}

        [AcceptVerbs("POST", "OPTIONS")]
        [AllowCrossSiteJson]
        public string AddExperience(string title = "Job Title", string description = "Job Description", string startDate = null, string endDate = null, string city = "City", string state = "State", string company = "Company Name", string token = null)
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
                    return AddErrorHeader("An authentication token must be passed in");
                }
                if (authUserId < 0)
                {
                    return AddErrorHeader("You are not authenticated, please log in!");
                }
                User user = userManager.GetUser(authUserId);
                DateTime startDateTime = DateTime.Now;
                if (startDate != null)
                {
                    startDateTime = DateTime.Parse(startDate);
                }
                DateTime endDateTime = DateTime.Now;
                if (endDate != null)
                {
                    endDateTime = DateTime.Parse(endDate);
                }
                JsonModels.Experience exp = userManager.AddExperience(user.id, startDateTime, endDateTime, title, description, city, state, company);
                if (exp != null)
                {
                    return Serialize(exp);
                }
                else
                {
                    return AddErrorHeader("Something went wrong while attempting to save this experience");
                }

            }
            catch (Exception e)
            {
                logAccessor.CreateLog(DateTime.Now, "userController - AddExperience", e.StackTrace);
                return AddErrorHeader("Something went wrong while adding the experience");
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
                    return AddErrorHeader("An authentication token must be passed in");
                }
                if (authUserId < 0)
                {
                    return AddErrorHeader("You are not authenticated, please log in!");
                }
                if (experience != null)
                {
                    JsonModels.Experience experienceFromJson = experience.FirstOrDefault();
                    if (experienceFromJson != null)
                    {
                        Experience originalExperience = userManager.GetExperience(experienceFromJson.id);
                        if (originalExperience != null)
                        {
                            originalExperience.city = experienceFromJson.city;
                            originalExperience.company = experienceFromJson.company;
                            originalExperience.description = experienceFromJson.description;
                            originalExperience.endDate = DateTime.Parse(experienceFromJson.endDate);
                            originalExperience.startDate = DateTime.Parse(experienceFromJson.startDate);
                            originalExperience.state = experienceFromJson.state;
                            originalExperience.title = experienceFromJson.title;
                            userManager.UpdateExperience(originalExperience);
                            return AddSuccessHeader(Serialize(experienceFromJson));
                        }
                        else
                        {
                            return AddErrorHeader("The experience model was not found in the database");
                        }
                    }
                    else
                    {
                        return AddErrorHeader("The experience model was not valid");
                    }
                }
                else
                {
                    return AddErrorHeader("An experience model was not received");
                }
            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, "userController - updateExperience", ex.StackTrace);
                return AddErrorHeader("Something went wrong while updating this experience element");
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
                    return AddErrorHeader("An authentication token must be passed in");
                }
                if (authUserId < 0)
                {
                    return AddErrorHeader("You are not authenticated, please log in!");
                }
                if (experienceId < 0)
                {
                    return AddErrorHeader("An experienceId must be passed in");
                }
                else
                {
                    return Serialize(userManager.GetExperienceJson(experienceId));
                }
            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, "UserController - GetExperience", ex.StackTrace);
                return AddErrorHeader("Something went wrong while getting this experience");
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
                    return AddErrorHeader("An authentication token must be passed in");
                }
                if (authUserId < 0)
                {
                    return AddErrorHeader("You are not authenticated, please log in!");
                }
                if (experienceId < 0)
                {
                    return AddErrorHeader("An experienceId must be passed in");
                }
                else
                {
                    Experience experience = userManager.GetExperience(experienceId);
                    if (experience == null)
                    {
                        return AddErrorHeader("An experience with the provided experienceId does not exist");
                    }
                    string response = userManager.DeleteExperience(experience);
                    if (response == null)
                    {
                        return AddErrorHeader("Something went wrong while deleting this experience");
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
                return AddErrorHeader("Something went wrong while deleting this experience");
            }
        }

        [AcceptVerbs("POST", "OPTIONS")]
        [AllowCrossSiteJson]
        public string AddReference(string firstName = "first name", string lastName = "last name", string email = "email", string company = "company", string title = "title", string message = "message", string videoLink = "videoLink", string videoType = "videoType", string token = null)
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
                    return AddErrorHeader("An authentication token must be passed in");
                }
                if (authUserId < 0)
                {
                    return AddErrorHeader("You are not authenticated, please log in!");
                }
                User user = userManager.GetUser(authUserId);
                JsonModels.Reference exp = userManager.AddReference(user.id, firstName, lastName, company, email, title, message, videoLink, videoType);
                return Serialize(exp);

            }
            catch (Exception e)
            {
                logAccessor.CreateLog(DateTime.Now, "userController - AddReference", e.StackTrace);
                return AddErrorHeader("Something went wrong while adding the experience");
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
                    return AddErrorHeader("An authentication token must be passed in");
                }
                if (authUserId < 0)
                {
                    return AddErrorHeader("You are not authenticated, please log in!");
                }
                if (reference != null)
                {
                    JsonModels.Reference referenceFromJson = reference.FirstOrDefault();
                    if (referenceFromJson != null)
                    {
                        Reference originalReference = userManager.GetReference(referenceFromJson.id);
                        if (originalReference != null)
                        {
                            originalReference.company = referenceFromJson.company;
                            originalReference.email = referenceFromJson.email;
                            originalReference.firstName = referenceFromJson.firstName;
                            originalReference.id = referenceFromJson.id;
                            originalReference.lastName = referenceFromJson.lastName;
                            originalReference.message = referenceFromJson.message;
                            originalReference.title = referenceFromJson.title;
                            //originalReference.userId = referenceFromJson.userId;
                            originalReference.videoLink = referenceFromJson.videoLink;
                            originalReference.videoType = referenceFromJson.videoType;

                            userManager.UpdateReference(originalReference);

                            return AddSuccessHeader(Serialize(referenceFromJson));
                        }
                        else
                        {
                            return AddErrorHeader("The reference model was not found in the database");
                        }
                    }
                    else
                    {
                        return AddErrorHeader("The reference model was not valid");
                    }
                }
                else
                {
                    return AddErrorHeader("An reference model was not received");
                }
            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, "userController - updatereference", ex.StackTrace);
                return AddErrorHeader("Something went wrong while updating this reference model");
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
                    return AddErrorHeader("An authentication token must be passed in");
                }
                if (authUserId < 0)
                {
                    return AddErrorHeader("You are not authenticated, please log in!");
                }
                if (referenceId < 0)
                {
                    return AddErrorHeader("An experienceId must be passed in");
                }
                else
                {
                    return Serialize(userManager.GetReferenceJson(referenceId));
                }
            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, "UserController - GetReference", ex.StackTrace);
                return AddErrorHeader("Something went wrong while getting this reference");
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
                    return AddErrorHeader("An authentication token must be passed in");
                }
                if (authUserId < 0)
                {
                    return AddErrorHeader("You are not authenticated, please log in!");
                }
                if (referenceId < 0)
                {
                    return AddErrorHeader("An experienceId must be passed in");
                }
                else
                {
                    Reference reference = userManager.GetReference(referenceId);
                    string response = userManager.DeleteReference(reference);
                    if (response == null)
                    {
                        return AddErrorHeader("Something went wrong while deleting this reference");
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
                return AddErrorHeader("Something went wrong while deleting this reference");
            }
        }

    }
}

