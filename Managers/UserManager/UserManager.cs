using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Accessor;
using System.Web.Security;
using System.Web.Routing;
using System.Drawing.Imaging;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Diagnostics;
using Entity;
using Engine;
using System.Runtime.Serialization;

namespace Manager
{
    public class UserManager
    {

        UserAccessor userAccessor = new UserAccessor();
        BlobStorageAccessor blobStorageAccessor = new BlobStorageAccessor();
        CloudQueueClient queueClient;
        CloudQueue queue;
        CloudStorageAccount storageAccount;
        string messageQueueName = "uploadqueue";

        public string UploadUserPicture(User user, Stream stmPicture, string type)
        {
            string messageQueueName = "uploadqueue"; //queue name must be in lower case
            CloudQueueClient queueClient;
            CloudQueue queue;
            CloudStorageAccount storageAccount;
            storageAccount = CloudStorageAccount.Parse(RoleEnvironment.GetConfigurationSettingValue("BlobConnectionString"));
            queueClient = storageAccount.CreateCloudQueueClient();
            queue = queueClient.GetQueueReference(messageQueueName);
            queue.CreateIfNotExist();
            if (user == null)
            {
                return null;
            }
            else if (stmPicture == null)
            {
                return "error saving picture - userManager";//return user without updating
            }
            string photoURL = "notset";
            if (CheckImageSize(stmPicture, 10000000))
            {
                CloudQueueMessage message = null;
                CloudQueueMessage message2 = null;

                String FileNameThumb = Guid.NewGuid().ToString();
                string artifactURL = string.Format("{0}{1}", FileNameThumb, ".jpeg");
                user.profilePicture = blobStorageAccessor.uploadImage(stmPicture, false).ToString();
                user.profilePictureThumbnail = RoleEnvironment.GetConfigurationSettingValue("storageAccountUrl").ToString()+"thumbnails/" + artifactURL;

                String FileNameThumb2 = Guid.NewGuid().ToString();
                string artifactURL2 = string.Format("{0}{1}", FileNameThumb2, ".jpeg");
                user.networkPictureThumbnail = RoleEnvironment.GetConfigurationSettingValue("storageAccountUrl").ToString()+"thumbnails/" + artifactURL2;

                photoURL = artifactURL;
                message = new CloudQueueMessage(String.Format("{0},{1},{2},{3},{4},{5},{6},{7}", user.profilePicture, user.id, "thumbnail", "User", 170, 170, "", artifactURL));
                message2 = new CloudQueueMessage(String.Format("{0},{1},{2},{3},{4},{5},{6},{7}", user.profilePicture, user.id, "thumbnail", "User", 50, 50, "", artifactURL2));

                user = UpdateUser(user);

                queue.AddMessage(message);
                queue.AddMessage(message2);
                return photoURL;
            }
            else
            {
                return null;
            }
        }

        public bool verifyEmail(User user)
        {
            try
            {
                return userAccessor.VerifyEmail(user.id);
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public void SendVerifyEmail(string email)
        {
            User user = GetUserByEmail(email);
            if (user != null)
            {
                CommunicationManager cm = new CommunicationManager();
                Random r = new Random();
                string random = r.Next(100000, 999999).ToString();
                string verifyHash = random + user.id.ToString();
                if(userAccessor.SetVerifyHash(user.id, verifyHash))
                {
                    cm.SendVerifyEmail(email, verifyHash);
                }
            }
        }

        public static bool CheckImageSize(Stream stmPicture, long size)
        {
            return stmPicture.Length <= size;
        }

        public User CreateUser(User user, string password)
        {
            ProjectAccessor projectAccessor = new ProjectAccessor();
            User createdUser = userAccessor.CreateUser(user, password);
            if (createdUser == null)
            {
                return null;
            }
            return userAccessor.GetUser(createdUser.id);
        }

        public string ResetPassword(User user)
        {
            return userAccessor.ResetPassword(user);
        }

        public List<User> GetAllUsers()
        {
            return userAccessor.GetAllUsers();
        }

        public User GetUser(int id)
        {
            User user = userAccessor.GetUser(id);

            //Reorder project elements before sending back to user
            ReorderEngine reorderEngine = new ReorderEngine();
            user = reorderEngine.ReOrderProjects(user);
            return user;
        }

        public User GetUser(string userName)
        {
            User user = userAccessor.GetUser(userName);

            //Reorder project elements before sending back to user
            ReorderEngine reorderEngine = new ReorderEngine();
            user = reorderEngine.ReOrderProjects(user);

            return user;
        }

        public User GetUserWithNetworks(int userId)
        {
            User user = userAccessor.GetUserWithNetworks(userId);

            //NOT re-ordering projects, as my query to include network Collections does not, by default, return all FK relationships. Plus I dont need the projects for what this method is used for.
            //Reorder project elements before sending back to user
            //ReorderEngine reorderEngine = new ReorderEngine();
            //user = reorderEngine.ReOrderProjects(user);

            return user;
        }

        public User GetUserByProfileURL(string profileURL)
        {
            User user = userAccessor.GetUserByProfileURL(profileURL);

            //Reorder project elements before sending back to user
            ReorderEngine reorderEngine = new ReorderEngine();
            user = reorderEngine.ReOrderProjects(user);

            return user;
        }

        public User GetUser(Guid guid)
        {
            User user = userAccessor.GetUser(guid);

            //Reorder project elements before sending back to user
            ReorderEngine reorderEngine = new ReorderEngine();
            user = reorderEngine.ReOrderProjects(user);

            return user;
        }

        public User GetUserByEmail(string email)
        {
            User user = userAccessor.GetUserByEmail(email);

            //Reorder project elements before sending back to user
            if (user != null)
            {
                ReorderEngine reorderEngine = new ReorderEngine();
                user = reorderEngine.ReOrderProjects(user);
            }
            return user;
        }

        public bool ValidateUser(User user, string password)
        {
            return userAccessor.ValidateUser(user, password);
        }

        public Guid GetProviderUserKey(User user)
        {
            return userAccessor.GetProviderUserKey(user);
        }

        public User VerifyUserEmail(User user, int toActivate)
        {
            return userAccessor.VerifyUserEmail(user, toActivate);
        }

        public User MakePublic(User user, int toActivate)
        {
            if (user.emailVerified == 1)
            {
                return userAccessor.MakePublic(user, toActivate);
            }
            else
            {
                return user;
            }
        }

        public User ChangeUserName(User user, string newUserName, string currentPassword)
        {
            return userAccessor.ChangeUserName(user, newUserName, currentPassword);
        }

        public User UpdateUser(User user)
        {
            return userAccessor.UpdateUser(user);
        }

        public bool ChangePassword(User user, string oldPassword, string newPassword)
        {
            return userAccessor.ChangePassword(user, oldPassword, newPassword);
        }

        public bool ChangePassword(User user, string newPassword)
        {
            return userAccessor.ChangePassword(user, newPassword);
        }

        public bool UpdateUserSettings(User user)
        {
            try
            {
                return userAccessor.updateUserSettings(user);
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool ActivateUser(User user, bool toActivate)
        {
            return userAccessor.ActivateUser(user, toActivate);
        }

        public User DeleteUser(User user)
        {
            return userAccessor.DeleteUser(user);
        }


        public void DeleteResume(User user)
        {
            if (user.resume != null)
            {
                string[] split = user.resume.Split('.');
                if (split[split.Length - 1] == "pdf")
                {
                    blobStorageAccessor.deleteFromBlob(user.resume, "pdfs");
                }
                else
                {
                    blobStorageAccessor.deleteFromBlob(user.resume, "documents");
                }
            }
        }
        public void DeleteProfilePicture(User user)
        {
            if (user.profilePicture != null)
            {
                blobStorageAccessor.deleteFromBlob(user.profilePicture, "images");
                blobStorageAccessor.deleteFromBlob(user.profilePictureThumbnail, "thumbnails");
            }
        }

        public bool CheckDuplicateEmail(string email)//returns true if there are no matching emails for users
        {
            User u = userAccessor.GetUserByEmail(email);
            if (u == null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool CheckDuplicateUsername(string userName)//returns true if there are no matching usernames for users
        {
            User u = userAccessor.GetUser(userName);
            if (u == null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool CheckDuplicateProfileURL(string profileURL)//returns true if there are no matching usernames for users
        {
            User u = userAccessor.GetUserByProfileURL(profileURL);
            if (u == null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public User AddConnection(User u, int connection)
        {
            return userAccessor.AddConnection(u, connection);
        }

        public List<User> GetUserConnections(User u)
        {
            List<User> connectedUsers = new List<User>();
            if (u.connections != null)
            {
                if (u.connections.Length > 0)
                {
                    string[] connections = u.connections.Split(',');

                    for (int i = 0; i < connections.Length; i++)
                    {
                        if (connections[i].Trim() != "")
                        {
                            User user = userAccessor.GetUser(Convert.ToInt32(connections[i]));
                            connectedUsers.Add(user);
                        }
                    }
                }
            }

            return connectedUsers;
        }

        public User DeleteConnection(User u, int connection)
        {
            string[] temp = u.connections.Split(',');
            List<int> connections = new List<int>();
            foreach (string c in temp)
            {
                int convertedToInt = Convert.ToInt32(c);
                if (convertedToInt != connection)
                {
                    connections.Add(convertedToInt);
                }
            }
            u.connections = null;
            foreach (int i in connections)
            {
                AddConnection(u, i);
            }
            return u;
        }

        public User ResetProjectOrder(User u)
        {
            List<int> currentProjects = new List<int>();
            string newProjectOrder = null;
            foreach (Project p in u.projects)
            {
                if (p.isActive == true && p.privacy != "deleted")
                {
                    currentProjects.Add(p.id);
                }
            }
            foreach (int y in currentProjects)
            {
                newProjectOrder += y + " ";
            }
            newProjectOrder = newProjectOrder.TrimEnd().Replace(' ', ',');
            u.projectOrder = newProjectOrder;
            return u;
        }

        public string UploadResumeDoc(User user, Stream stmDoc)
        {

            if (CheckImageSize(stmDoc, 20000000))
            {
                storageAccount = CloudStorageAccount.Parse(RoleEnvironment.GetConfigurationSettingValue("BlobConnectionString"));
                queueClient = storageAccount.CreateCloudQueueClient();
                queue = queueClient.GetQueueReference(messageQueueName);
                queue.CreateIfNotExist();
                string fullName = user.firstName + " " + user.lastName;
                String FileName = Guid.NewGuid().ToString();
                string uniqueBlobName = string.Format("{0}{1}", FileName, ".pdf");
                string resumeLocation = blobStorageAccessor.uploadDOC(stmDoc, false, ".doc").ToString();
                CloudQueueMessage message = new CloudQueueMessage(String.Format("{0},{1},{2},{3},{4},{5},{6},{7}", resumeLocation, user.id, "userDocumentConversion", @"http://do.convertapi.com/Word2Pdf", 0, 0, fullName, uniqueBlobName));
                queue.AddMessage(message);
                user.resume = RoleEnvironment.GetConfigurationSettingValue("storageAccountUrl").ToString()+"pdfs/" + uniqueBlobName;
                user = UpdateUser(user);
                return uniqueBlobName;
            }
            else
                return null;
        }

        public string UploadResumeDocx(User user, Stream stmDoc)
        {

            if (CheckImageSize(stmDoc, 20000000))
            {
                storageAccount = CloudStorageAccount.Parse(RoleEnvironment.GetConfigurationSettingValue("BlobConnectionString"));
                queueClient = storageAccount.CreateCloudQueueClient();
                queue = queueClient.GetQueueReference(messageQueueName);
                queue.CreateIfNotExist();
                string fullName = user.firstName + " " + user.lastName;
                String FileName = Guid.NewGuid().ToString();
                string uniqueBlobName = string.Format("{0}{1}", FileName, ".pdf");
                string resumeLocation = blobStorageAccessor.uploadDOC(stmDoc, false, ".docx").ToString();
                CloudQueueMessage message = new CloudQueueMessage(String.Format("{0},{1},{2},{3},{4},{5},{6},{7}", resumeLocation, user.id, "userDocumentConversion", @"http://do.convertapi.com/Word2Pdf", 0, 0, fullName, uniqueBlobName));
                queue.AddMessage(message);
                user.resume = RoleEnvironment.GetConfigurationSettingValue("storageAccountUrl").ToString()+"pdfs/" + uniqueBlobName;
                user = UpdateUser(user);
                return uniqueBlobName;
            }
            else
                return null;
        }

        public string UploadResumePDF(User user, Stream s)
        {

            if (CheckImageSize(s, 20000000))
            {
                user.resume = blobStorageAccessor.uploadPDF(s, false).ToString();
                user = UpdateUser(user);
                return user.resume;
            }
            else
                return null;
        }

        public string UploadResumeRTF(User user, Stream s)
        {

            if (CheckImageSize(s, 20000000))
            {
                storageAccount = CloudStorageAccount.Parse(RoleEnvironment.GetConfigurationSettingValue("BlobConnectionString"));
                queueClient = storageAccount.CreateCloudQueueClient();
                queue = queueClient.GetQueueReference(messageQueueName);
                queue.CreateIfNotExist();
                string fullName = user.firstName + " " + user.lastName;
                String FileName = Guid.NewGuid().ToString();
                string uniqueBlobName = string.Format("{0}{1}", FileName, ".pdf");
                string resumeLocation = blobStorageAccessor.uploadUnknown(s, false, "rtf").ToString();
                CloudQueueMessage message = new CloudQueueMessage(String.Format("{0},{1},{2},{3},{4},{5},{6},{7}", resumeLocation, user.id, "userDocumentConversion", @"http://do.convertapi.com/RichText2Pdf", 0, 0, fullName, uniqueBlobName));
                queue.AddMessage(message);
                user.resume = RoleEnvironment.GetConfigurationSettingValue("storageAccountUrl").ToString()+"pdfs/" + uniqueBlobName;
                user = UpdateUser(user);
                return uniqueBlobName;
            }
            else
                return null;
        }

        public string UploadResumeTXT(User user, Stream s)
        {

            if (CheckImageSize(s, 20000000))
            {
                storageAccount = CloudStorageAccount.Parse(RoleEnvironment.GetConfigurationSettingValue("BlobConnectionString"));
                queueClient = storageAccount.CreateCloudQueueClient();
                queue = queueClient.GetQueueReference(messageQueueName);
                queue.CreateIfNotExist();
                string fullName = user.firstName + " " + user.lastName;
                String FileName = Guid.NewGuid().ToString();
                string uniqueBlobName = string.Format("{0}{1}", FileName, ".pdf");
                string resumeLocation = blobStorageAccessor.uploadUnknown(s, false, "txt").ToString();
                CloudQueueMessage message = new CloudQueueMessage(String.Format("{0},{1},{2},{3},{4},{5},{6},{7}", resumeLocation, user.id, "userDocumentConversion", @"http://do.convertapi.com/Text2Pdf", 0, 0, fullName, uniqueBlobName));
                queue.AddMessage(message);
                user.resume = RoleEnvironment.GetConfigurationSettingValue("storageAccountUrl").ToString()+"pdfs/" + uniqueBlobName;
                user = UpdateUser(user);
                return uniqueBlobName;
            }
            else
                return null;
        }

        public List<string> FindUser(string query)//email profileURL lastName firstNameSpaceLastName
        {
            User u = null;
            List<string> userExactMatch = new List<string>();
            List<string> userApproxMatch = new List<string>();
            List<User> lastMatch = new List<User>();

            if (query.Contains("@"))
            {
                u = GetUserByEmail(query.ToLower());

                if (u != null && u.isPublic == 1)
                {
                    return new List<string> {(u.firstName + ' ' + u.lastName + " - vestn.com/v/" + u.profileURL)};
                }
            }
            else if (query.Contains(" "))
            {
                string[] name = query.Split(' ');
                List<User> allUsers = GetAllUsers();
                foreach (User um in allUsers)
                {
                    if (um.lastName.ToLower() == name[1].ToLower() && um.isPublic == 1)
                    {
                        lastMatch.Add(um);
                    }
                }
                foreach (User um in lastMatch)
                {
                    if (um.isPublic == 1)
                    {
                        if (um.firstName.ToLower() == name[0].ToLower())
                        {
                            userExactMatch.Add(um.firstName + ' ' + um.lastName + " - vestn.com/v/" + um.profileURL);
                        }
                        else
                        {
                            userApproxMatch.Add(um.firstName + ' ' + um.lastName + " - vestn.com/v/" + um.profileURL);
                        }
                    }
                }
                foreach (string s in userApproxMatch)
                {
                    userExactMatch.Add(s);
                }
                return userExactMatch;
            }
            if (u == null)
            {
                u = GetUserByProfileURL(query.ToLower());
                if (u != null && u.isPublic == 1)
                {
                    return new List<string> {(u.firstName + ' ' + u.lastName + " - vestn.com/v/" + u.profileURL)};
                }
            }
            if (u == null)
            {
                List<string> lastName = new List<string>();
                foreach (User um in GetAllUsers())
                {
                    if (um.lastName.ToLower() == query.ToLower() && um.isPublic == 1)
                    {
                        lastName.Add(um.firstName + ' ' + um.lastName + " - vestn.com/v/" + um.profileURL);
                    }
                }
                if (lastName.Count > 0)
                {
                    return lastName;
                }
            }
            return new List<string> { ("No users found.") };
        }

        public bool CompleteTodo(int userId)
        {
            return userAccessor.CompleteTodo(userId);
        }

        public JsonModels.UserStats getUserStats(int ID)
        {
            JsonModels.UserStats us = new JsonModels.UserStats();
            us.props = 10;
            us.views = 43;
            us.numberOfArtifacts = 13;
            return us;
        }

        public JsonModels.Links getUserLinks(int ID)
        {
            JsonModels.Links links = new JsonModels.Links();
            string twitter = GetUser(ID).twitterLink;
            string linkedin = GetUser(ID).linkedinLink;
            string facebook = GetUser(ID).facebookLink;
            int add = 0;
            if (twitter != null)
            {
                links.twitterLink = twitter;
                add = 1;
            }
            if (facebook != null)
            {
                links.facebookLink = facebook;
                add = 1;
            }
            if (linkedin != null)
            {
                links.linkedinLink = linkedin;
                add = 1;
            }
            if (add != 1)
            {
                return null;
            }
            else
            {
                return links;
            }
        }

        public JsonModels.ProfileInformation GetProfileJson(User u)
        {
            JsonModels.ProfileInformation ui = new JsonModels.ProfileInformation();
            ActivityManager activityManager = new ActivityManager();
            ui.firstName = u.firstName;
            ui.description = u.description;
            ui.email = u.email;

            List<JsonModels.Experience> experiences = GetUserExperiences(u.id);
            if (experiences != null && experiences.Count != 0)
            {
                ui.experiences = experiences;
            }
            else
            {
                ui.experiences = null;
            }

            ui.firstName = u.firstName;
            ui.id = u.id.ToString();
            ui.lastName = u.lastName;
            ui.facebookLink = u.facebookLink;
            ui.twitterLink = u.twitterLink;
            ui.linkedinLink = u.linkedinLink;

            ui.location = u.location;
            ui.major = u.major;
            ui.phoneNumber = u.phoneNumber;
            ui.profilePicture = u.profilePicture;
            ui.profilePictureThumbnail = u.profilePictureThumbnail;
            ui.projectOrder = u.projectOrder;

            //projects
            int[] projectIds = new int[u.projects.Count];
            int count = 0;
            foreach (Project p in u.projects)
            {
                projectIds[count] = p.id;
                count++;
            }
            ProjectManager pm = new ProjectManager();
            List<JsonModels.CompleteProject> projects =  pm.GetCompleteProjects(projectIds, null);
            if (projects != null && projects.Count != 0)
            {
                ui.projects = projects;
            }
            else
            {
                ui.projects = null;
            }

            //ui.recentActivity = GetRecentActivity(u.id);
            ui.activity = activityManager.GetUserActivity(u.id);

            List<JsonModels.Reference> references = GetUserReferences(u.id);
            if (references != null && references.Count != 0)
            {
                ui.references = references;
            }
            else
            {
                ui.references = null;
            }

            ui.resume = u.resume;
            ui.organization = u.organization;
            ui.stats = getUserStats(u.id);
            ui.tagLine = u.tagLine;
            ui.title = u.title;

            return ui;
        }

        //public List<JsonModels.Reference> GetUserReferences(int ID)
        //{
        //    return new List<JsonModels.Reference> { new JsonModels.Reference { name = "me" } };
        //}

        public List<JsonModels.UserTag> GetUserTags(int ID)
        {
            TagAccessor ta = new TagAccessor();
            List<Tag> tags = ta.GetUserTags(ID);

            List<JsonModels.UserTag> userTags = new List<JsonModels.UserTag>();
            foreach (Tag t in tags)
            {
                JsonModels.UserTag ut = new JsonModels.UserTag();
                if (t.value != null)
                {
                    ut.userTagValue = t.value;
                    ut.userTagId = t.id;
                }
                userTags.Add(ut);
            }
            return userTags;
        }

        public List<JsonModels.Todo> GetTodo(int ID)
        {
            List<JsonModels.Todo> todoList = new List<JsonModels.Todo>();
            todoList.Add(new JsonModels.Todo { id = 1, item = "Add a resume" });
            todoList.Add(new JsonModels.Todo { id = 2, item = "Edit your First Vestn Project" });
            todoList.Add(new JsonModels.Todo { id = 3, item = "Add experience, references, or more projects to show what you can do" });
            return todoList;
        }

        public List<JsonModels.RecentActivity> GetRecentActivity(int ID)
        {

            List<JsonModels.RecentActivity> recentActivity = new List<JsonModels.RecentActivity>();
            UserAccessor ua = new UserAccessor();

            User owner = ua.GetUser(ID);
            List<Project> projectsToUse = new List<Project>();
            try
            {
                foreach (Project p in owner.projects)
                {
                    if (p.name != "About" && p.isActive == true)
                    {
                        projectsToUse.Add(p);
                    }
                }
                owner.projects = projectsToUse;
                Random rnd = new Random();
                
                User u = ua.GetUser(getRandomVestnId(ID));
                Project randomProject = owner.projects.OrderBy(x => rnd.Next()).Take(1).FirstOrDefault();
                recentActivity.Add(new JsonModels.RecentActivity { id = 1, type = "prop", comment = "Wonderful project! very well done",propGiverID=u.id,propGiverURL=u.profileURL, pictureURL = u.profilePictureThumbnail, propGiverName = u.firstName + " " + u.lastName, projectName = randomProject.name, projectID = randomProject.id, date = "Today" });
                
                recentActivity.Add(new JsonModels.RecentActivity { id = 2, type = "contactUpdate", date = "Yesterday" });
                
                u = ua.GetUser(getRandomVestnId(ID));
                randomProject = owner.projects.OrderBy(x => rnd.Next()).Take(1).FirstOrDefault();
                recentActivity.Add(new JsonModels.RecentActivity { id = 3, type = "prop", comment = "Amazing work! Keep it up.", propGiverID = u.id, propGiverURL = u.profileURL, pictureURL = u.profilePictureThumbnail, propGiverName = u.firstName + " " + u.lastName, projectName = randomProject.name, projectID = randomProject.id, date = "4 days ago" });
                
                randomProject = owner.projects.OrderBy(x => rnd.Next()).Take(1).FirstOrDefault();
                recentActivity.Add(new JsonModels.RecentActivity { id = 4, type = "newProject", projectName = randomProject.name, projectID = randomProject.id, date = "4 days ago" });
                
                u = ua.GetUser(getRandomVestnId(ID));
                randomProject = owner.projects.OrderBy(x => rnd.Next()).Take(1).FirstOrDefault();
                recentActivity.Add(new JsonModels.RecentActivity { id = 5, type = "prop", comment = "Really nice project. I love your work!", propGiverID = u.id, propGiverURL = u.profileURL, pictureURL = u.profilePictureThumbnail, propGiverName = u.firstName + " " + u.lastName, projectName = randomProject.name, projectID = randomProject.id, date = "1 week ago" });
                
                randomProject = owner.projects.OrderBy(x => rnd.Next()).Take(1).FirstOrDefault();
                recentActivity.Add(new JsonModels.RecentActivity { id = 6, type = "newArtifact", projectName = randomProject.name, projectID = randomProject.id, artifactCount = 3, date = "2 weeks ago" });
                
            }
            catch (Exception e)
            {
                recentActivity.Add(new JsonModels.RecentActivity { id = 1, type = "newProject", projectName = "Fake Project" });
            }
            return recentActivity;
        }

        private int getRandomVestnId(int ID)
        {
            List<int> vestnMemberIds = new List<int> { 2, 3, 88, 5, 6 };
            vestnMemberIds.Remove(ID);
            Random rnd = new Random();
            return vestnMemberIds.OrderBy(x => rnd.Next()).Take(1).FirstOrDefault();
        }

        public JsonModels.Experience AddExperience(int userId, DateTime startDate, DateTime endDate, string title = null, string description = null, string city =null, string state =null, string company = null)
        {
            try
            {
                if (userId == null)
                {
                    return null;
                }
                if (startDate == null)
                {
                    startDate = DateTime.Now;
                }
                if (endDate == null)
                {
                    endDate = DateTime.Now;
                }
                Experience experience = new Experience
                {
                    userId = userId,
                    startDate = startDate,
                    endDate = endDate,
                    title = title,
                    description = description,
                    city = city,
                    state = state,
                    company = company
                };
                Experience exp = userAccessor.AddExperience(experience);
                if (exp == null)
                {
                    return null;
                }
                JsonModels.Experience response = new JsonModels.Experience();
                response.id = experience.id;
                //response.userId = userId;
                response.startDate = startDate.ToString();
                response.endDate = endDate.ToString();
                response.description = description;
                response.city = city;
                response.state = state;
                response.company = company;
                response.title = title;
                return response;

            }
            catch (Exception e)
            {
                LogAccessor la = new LogAccessor();
                la.CreateLog(DateTime.Now, "userManager - AddExperience", e.StackTrace);
                return null;
            }
        }
        public Experience UpdateExperience(Experience experience)
        {
            return userAccessor.UpdateExperience(experience);
        }
        public JsonModels.Experience GetExperienceJson(int experienceId)
        {
            Experience experience = userAccessor.GetExperience(experienceId);
            JsonModels.Experience response = new JsonModels.Experience();
            response.id = experience.id;
            //response.userId = experience.userId;
            response.startDate = experience.startDate.ToString();
            response.endDate = experience.endDate.ToString();
            response.description = experience.description;
            response.city = experience.city;
            response.state = experience.state;
            response.company = experience.company;
            response.title = experience.title;
            return response;

        }
        public Experience GetExperience(int referenceId)
        {
            return userAccessor.GetExperience(referenceId);
        }

        public List<JsonModels.Experience> GetUserExperiences(int userId)
        {
            try
            {
                List<Experience> experiences = userAccessor.GetUserExperience(userId);
                List<JsonModels.Experience> experiencesJson = new List<JsonModels.Experience>();
                foreach (Experience e in experiences)
                {
                    JsonModels.Experience eJson = new JsonModels.Experience();
                    if (e != null)
                    {
                        eJson.id = e.id;
                        if (e.company != null)
                        {
                            eJson.company = e.company;
                        }
                        if (e.description != null)
                        {
                            eJson.description = e.description;
                        }
                        if (e.startDate != null)
                        {
                            eJson.startDate = e.startDate.ToShortDateString();
                        }
                        if (e.endDate != null)
                        {
                            eJson.endDate = e.endDate.ToShortDateString();
                        }
                        if (e.title != null)
                        {
                            eJson.title = e.title;
                        }
                        if (e.city != null)
                        {
                            eJson.city = e.city;
                        }
                        if (e.state != null)
                        {
                            eJson.state = e.state;
                        }
                        experiencesJson.Add(eJson);
                    }
                }
                return experiencesJson;
            }
            catch (Exception ex)
            {
                LogAccessor la = new LogAccessor();
                la.CreateLog(DateTime.Now, "UserManager - GetUserExperiences", ex.StackTrace);
                return null;
            }
        }
        public string DeleteExperience(Experience experience)
        {
            if (userAccessor.DeleteExperience(experience))
            {
                return "Successfully Deleted Experience";
            }
            else
            {
                return null;
            }
        }

        public JsonModels.Reference AddReference(int userId, string firstName = null, string lastName = null, string company = null, string email = null, string title = null, string message = null, string videoLink = null, string videoType = null)
        {
            try
            {
                if (userId == null)
                {
                    return null;
                }
                if (videoLink != null)
                {
                    if (videoLink != "videoLink")
                    {
                        if (videoLink.Contains("youtube"))
                        {
                            if (videoLink.Contains("http://"))
                            {
                                videoLink = videoLink.Substring(31, 11);
                                videoType = "youtube";
                            }
                            else
                            {
                                videoLink = videoLink.Substring(24, 11);
                                videoType = "youtube";
                            }
                        }
                        else if (videoLink.Contains("youtu."))
                        {
                            videoLink = videoLink.Substring(16);
                            videoType = "youtube";
                        }
                        else if (videoLink.Contains("vimeo"))
                        {
                            string[] s = videoLink.Split('/');
                            videoLink = s[s.Count() - 1];
                            videoType = "vimeo";
                        }
                    }
                }
                Reference reference = new Reference
                {
                    userId = userId,
                    firstName = firstName,
                    lastName = lastName,
                    email = email,
                    message = message,
                    title = title,
                    company = company,
                    videoLink = videoLink,
                    videoType = videoType
                };
                userAccessor.AddReference(reference);
                JsonModels.Reference response = new JsonModels.Reference();
                response.id = reference.id;
                //response.userId = userId;
                response.message = message;
                response.email = email;
                response.firstName = firstName;
                response.lastName = lastName;
                response.company = company;
                response.title = title;
                response.videoLink = videoLink;
                response.videoType = videoType;
                return response;

            }
            catch (Exception e)
            {
                LogAccessor la = new LogAccessor();
                la.CreateLog(DateTime.Now, "userManager - AddReference", e.StackTrace);
                return null;
            }
        }
        public Reference UpdateReference(Reference reference)
        {
            return userAccessor.UpdateReference(reference);
        }
        public JsonModels.Reference GetReferenceJson(int referenceId)
        {
            Reference reference = userAccessor.GetReference(referenceId);
            JsonModels.Reference response = new JsonModels.Reference();
            response.id = reference.id;
            //response.userId = reference.userId;
            response.message = reference.message;
            response.email = reference.email;
            response.firstName = reference.firstName;
            response.lastName = reference.lastName;
            response.company = reference.company;
            response.title = reference.title;
            response.videoLink = reference.videoLink;
            response.videoType = reference.videoType;
            return response;
        }
        public Reference GetReference(int referenceId)
        {
            return userAccessor.GetReference(referenceId);
        }

        public List<JsonModels.Reference> GetUserReferences(int userId)
        {
            try
            {
                List<Reference> references = userAccessor.GetUserReference(userId);
                List<JsonModels.Reference> referencesJson = new List<JsonModels.Reference>();
                foreach (Reference r in references)
                {
                    JsonModels.Reference rJson = new JsonModels.Reference();
                    if (r != null)
                    {
                        rJson.id = r.id;
                        if (r.company != null)
                        {
                            rJson.company = r.company;
                        }
                        if (r.firstName != null)
                        {
                            rJson.firstName = r.firstName;
                        }
                        if (r.lastName != null)
                        {
                            rJson.lastName = r.lastName;
                        }
                        if (r.message != null)
                        {
                            rJson.message = r.message;
                        }
                        if (r.title != null)
                        {
                            rJson.title = r.title;
                        }
                        if (r.videoLink != null)
                        {
                            rJson.videoLink = r.videoLink;
                        }
                        if (r.email != null)
                        {
                            rJson.email = r.email;
                        }
                        if (r.videoType != null)
                        {
                            rJson.videoType = r.videoType;
                        }
                        referencesJson.Add(rJson);
                    }
                }
                return referencesJson;
            }
            catch (Exception ex)
            {
                LogAccessor la = new LogAccessor();
                la.CreateLog(DateTime.Now, "UserManager - GetUserReferences", ex.StackTrace);
                return null;
            }
        }

        public string DeleteReference(Reference reference)
        {
            if (userAccessor.DeleteReference(reference))
            {
                return "successfully deleted reference";
            }
            else
            {
                return null;
            }
        }

        public JsonModels.ProfileScore GetProfileScore(User user)
        {
            JsonModels.ProfileScore profileScoreJson = new JsonModels.ProfileScore();
            if (user.profilePicture != null && user.profilePictureThumbnail != null)
            {
                profileScoreJson.profilePic = true;
            }
            else
            {
                profileScoreJson.profilePic = false;
            }
            if (user.location != null)
            {
                profileScoreJson.location = true;
            }
            else
            {
                profileScoreJson.location = false;
            }
            if (user.organization != null)
            {
                profileScoreJson.school = true;
            }
            else
            {
                profileScoreJson.school = false;
            }
            if (user.resume != null)
            {
                profileScoreJson.resume = true;
            }
            else
            {
                profileScoreJson.school = false;
            }

            int numberOfFeaturedProjects = 0;
            int numberOfArtifacts = 0;
            int numberOfReflections = 0;
            int numberOfProps = 0;
            if (user.projects != null)
            {
                ProjectManager pm = new ProjectManager();
                foreach (Project p in user.projects)
                {
                    //check if project is a featured project
                    //numberOfFeaturedProjects++;

                    List<JsonModels.Prop> props = pm.GetProjectProps(p.id);
                    if (props != null)
                    {
                        numberOfProps = numberOfProps + props.Count;
                    }

                    if (p.projectElements != null)
                    {
                        foreach (ProjectElement pe in p.projectElements)
                        {
                            if (pe != null)
                            {
                                numberOfArtifacts++;
                                if (pe.description != null)
                                {
                                    numberOfReflections++;
                                }
                            }
                        }
                    }
                }
            }

            profileScoreJson.artifacts = numberOfArtifacts;
            profileScoreJson.featuredProjects = numberOfFeaturedProjects;
            profileScoreJson.props = numberOfProps;
            profileScoreJson.reflections = numberOfReflections;

            return profileScoreJson;

        }

        
    }
}
