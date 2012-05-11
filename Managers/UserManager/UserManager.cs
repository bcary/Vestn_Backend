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

        public User UploadUserPicture(User user, Stream stmPicture, string type)
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
                return user;//return user without updating
            }

            if (CheckImageSize(stmPicture, 10000000))
            {
                CloudQueueMessage message = null;
                if (type == "Profile")
                {
                    user.profilePicture = blobStorageAccessor.uploadImage(stmPicture, false).ToString();
                    message = new CloudQueueMessage(String.Format("{0},{1},{2},{3},{4},{5}", user.profilePicture, user.id, "thumbnail", "User", "175","175"));
                }
                else if (type == "About")
                {
                    message = new CloudQueueMessage(String.Format("{0},{1},{2},{3},{4},{5}", user.aboutPicture, user.id, "thumbnail", "About", "175", "175"));
                    user.aboutPicture = blobStorageAccessor.uploadImage(stmPicture, false).ToString();
                }
                user = UpdateUser(user);
                queue.AddMessage(message);
                return user;
            }
            else
                return null;
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
            if (user.projects.Count() == 0)
            {
                projectAccessor.CreateProject(createdUser, new List<ProjectElement>());
            }
            AnalyticsAccessor aa = new AnalyticsAccessor();
            aa.CreateAnalytic("User Register", DateTime.Now, createdUser.userName);
            //createdUser.projects.FirstOrDefault().name = "About";
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
            List<User> users = userAccessor.GetAllUsers();
            bool duplicate = true;
            foreach (User u in users)
            {
                if (u.email.Trim() == email.Trim())
                {
                    duplicate = false;
                }
            }
            return duplicate;
        }

        public bool CheckDuplicateUsername(string userName)//returns true if there are no matching usernames for users
        {
            List<User> users = userAccessor.GetAllUsers();
            bool duplicate = true;
            foreach (User u in users)
            {
                if (u.userName.Trim() == userName.Trim() || u.profileURL.Trim() == userName.Trim())
                {
                    duplicate = false;
                }
            }
            return duplicate;
        }

        public bool CheckDuplicateProfileURL(User user)//returns true if there are no matching usernames for users
        {
            List<User> users = userAccessor.GetAllUsers();
            bool duplicate = true;
            foreach (User u in users)
            {
                if ((
                        u.profileURL.Trim() == user.profileURL.Trim() || 
                        u.userName.Trim() == user.profileURL.Trim()
                        ) &&
                    u.userName.Trim() != user.userName.Trim()
                    )

                {
                    duplicate = false;
                }
            }
            return duplicate;
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
                if (p.isActive == true)
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

        public User UploadResumeDoc(User user, Stream stmDoc)
        {

            if (CheckImageSize(stmDoc, 20000000))
            {
                user.resume = blobStorageAccessor.uploadDOC(stmDoc, false, ".doc").ToString();
                user = UpdateUser(user);
                return user;
            }
            else
                return null;
        }

        public User UploadResumeDocx(User user, Stream stmDoc)
        {

            if (CheckImageSize(stmDoc, 20000000))
            {
                user.resume = blobStorageAccessor.uploadDOC(stmDoc, false, ".docx").ToString();
                user = UpdateUser(user);
                return user;
            }
            else
                return null;
        }

        public User UploadResumePDF(User user, Stream s)
        {

            if (CheckImageSize(s, 20000000))
            {
                user.resume = blobStorageAccessor.uploadPDF(s, false).ToString();
                user = UpdateUser(user);
                return user;
            }
            else
                return null;
        }

        public User UploadResumeRTF(User user, Stream s)
        {

            if (CheckImageSize(s, 20000000))
            {
                user.resume = blobStorageAccessor.uploadUnknown(s, false, "rtf").ToString();
                user = UpdateUser(user);
                return user;
            }
            else
                return null;
        }

        public User UploadResumeTXT(User user, Stream s)
        {

            if (CheckImageSize(s, 20000000))
            {
                user.resume = blobStorageAccessor.uploadUnknown(s, false, "txt").ToString();
                user = UpdateUser(user);
                return user;
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

                if (u != null)
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
                    if (um.lastName.ToLower() == name[1].ToLower())
                    {
                        lastMatch.Add(um);
                    }
                }
                foreach (User um in lastMatch)
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
                foreach (string s in userApproxMatch)
                {
                    userExactMatch.Add(s);
                }
                return userExactMatch;
            }
            if (u == null)
            {
                u = GetUserByProfileURL(query.ToLower());
                if (u != null)
                {
                    return new List<string> {(u.firstName + ' ' + u.lastName + " - vestn.com/v/" + u.profileURL)};
                }
            }
            if (u == null)
            {
                List<string> lastName = new List<string>();
                foreach (User um in GetAllUsers())
                {
                    if (um.lastName.ToLower() == query.ToLower())
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

        public List<JsonModels.Experience> GetUserExperiences(int ID)
        {
            ProjectAccessor pa = new ProjectAccessor();
            List<ProjectElement_Experience> PEexperiences = pa.GetExperiences(userAccessor.GetUser(ID));
            List<JsonModels.Experience> experiences = new List<JsonModels.Experience>();
            foreach (ProjectElement_Experience pe in PEexperiences)
            {
                JsonModels.Experience e = new JsonModels.Experience();
                if (pe.company != null)
                {
                    e.company = pe.company;
                }
                if (pe.description != null)
                {
                    e.jobDescription = pe.jobDescription;
                }
                if (pe.startDate != null)
                {
                    e.startDate = pe.startDate.ToShortDateString();
                }
                if (pe.endDate != null)
                {
                    e.endDate = pe.endDate.ToShortDateString();
                }
                if (pe.title != null)
                {
                    e.jobTitle = pe.title;
                }
                experiences.Add(e);
            }                
            return experiences;
        }

        public List<JsonModels.Reference> GetUserReferences(int ID)
        {
            return new List<JsonModels.Reference> { new JsonModels.Reference { name = "me" } };
        }

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

        
    }
}
