using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Security;
using Entity;
using System.Diagnostics.CodeAnalysis;
using System.Data.Linq;

namespace Accessor
{ 
    public class UserAccessor
    {
        LogAccessor logAccessor = new LogAccessor();
        public string TestMe()
        {
            return "success";
        }

        public User CreateUser(User user, string password)
        {
            //Membership logic
            MembershipCreateStatus createStatus;
            Membership.CreateUser(user.userName, password, user.email, null, null, false, null, out createStatus);
            if (createStatus != MembershipCreateStatus.Success)
            {
                logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), createStatus.ToString());
                return null;
            }

            //Entity logic
            try
            {
                VestnDB db = new VestnDB();
                if (user.tagIds == null)
                {
                    user.tagIds = "";
                }
                //if (user.status == null)
                //{
                //    user.status = EmploymentStatus.LookingForInternship;
                //}
                //if (user.willingToRelocate == null)
                //{
                //    user.willingToRelocate = WillingToRelocateType.undecided;
                //}
                if (user.projects == null)
                {
                    user.projects = new List<Project>();
                }
                if (user.birthDate.Year < 1800)
                {
                    user.birthDate = new DateTime(1800, 1, 1);
                }
                if (user.graduationDate.Year < 1800)
                {
                    user.graduationDate = new DateTime(1800, 1, 1);
                }
                user.isPublic = 0;
                user.isActive = 1;
                user.emailVerified = 0;
                db.users.Add(user);
                db.SaveChanges();
            }
            catch (Exception e)
            {
                //If the entity logic failed, delete the stray MemershipUser
                logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), e.ToString());
                Membership.DeleteUser(user.userName);
                return null;
            }

            return GetUser(user.id);
        }

        public User ChangeUserName(User user, string newUserName, string currentPassword)
        {
            MembershipUser membershipUser = Membership.GetUser(user.userName);
            Membership.DeleteUser(membershipUser.UserName, true);

            MembershipCreateStatus status;
            Membership.CreateUser(newUserName, currentPassword, membershipUser.Email, null, null, true, membershipUser.ProviderUserKey, out status);

            user.userName = newUserName;
            UpdateEntityUser(user);

            return user;
        }

        public string ResetPassword(User user)
        {
            MembershipUser membershipUser = Membership.GetUser(user.userName);

            string hashPassword = GenerateRandomAlphaNumString(30);
            string tempPassword = membershipUser.ResetPassword();

            if (ChangePassword(user, tempPassword, hashPassword))
            {
                return hashPassword;
            }
            return tempPassword;
        }

        private string GenerateRandomAlphaNumString(int length)
        {
            Random r = new Random();
            

            string characters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKMNOPQRSTUVWXYZ1234567890";
            string randomString = "";
            for (int i = 0; i < length; i++)
            {
                randomString += characters.ElementAt(r.Next(characters.Length));
            }

            return randomString;
        }




        public List<User> GetAllUsers()
        {
            VestnDB db = new VestnDB();
            return db.users.Include(u => u.projects.Select(b => b.projectElements)).ToList();
        }

        public User GetUser(int id)
        {
            //Get entity user using ID
            User user = GetEntityUser(id);
            if (user == null)
            {
                return null;
            }

            //TODO do not let this go live uncommented....
            //Make sure MembershipUser exists
            MembershipUser membershipUser = Membership.GetUser(user.userName);
            if (membershipUser == null)
            {
                //MembershipCreateStatus createStatus;
                //Membership.CreateUser(user.userName, "safasfsd", user.email, null, null, false, null, out createStatus);
                return null;
            }
            return user;
        }

        public User GetUser(string userName)
        {
            if (userName == null)
            {
                return null;
            }

            //Make sure MembershipUser exists
            MembershipUser membershipUser = Membership.GetUser(userName);
            if (membershipUser == null)
            {
                return null;
            }

            //Get entity user with userName
            User user = GetEntityUser(userName);
            return user;
        }

        public User GetUserByProfileURL(string profileURL)
        {
            if (profileURL == null)
            {
                return null;
            }

            //Get entity user with userName
            User user = GetEntityUserByProfileURL(profileURL);

            //Make sure MembershipUser exists
            if (user != null)
            {
                MembershipUser membershipUser = Membership.GetUser(user.userName);
                if (membershipUser == null)
                {
                    return null;
                }

            }
            return user;
        }

        public User GetUserByEmail(string email)
        {
            string userName = Membership.GetUserNameByEmail(email);
            User user = GetEntityUser(userName);
            return user;
        }

        public User GetUser(Guid guid)
        {
            //Make sure MembershipUser exists
            MembershipUser membershipUser = Membership.GetUser(guid);
            if (membershipUser == null)
            {
                return null;
            }

            //Get entity user with userName
            User user = GetEntityUser(membershipUser.UserName);

            return user;
        }

        public bool ValidateUser(User user, string password)
        {
            if (user != null)
            {
                if (Membership.ValidateUser(user.userName, password))
                {
                    return true;
                }
            }
            return false;
        }

        public Guid GetProviderUserKey(User user)
        {
            MembershipUser membershipUser = Membership.GetUser(user.userName);
            return (Guid)membershipUser.ProviderUserKey;
        }

        public User UpdateFromWorker(User user)
        {
            try
            {
                VestnDB db = new VestnDB();
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
            }
            catch (Exception e)
            {
                logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), e.ToString());
                return null;
            }

            return user;
        }

        public User UpdateUser(User user)
        {
            if (user == null)
            {
                return null;
            }

            if (user.profileURL == null)
            {
                user.profileURL = user.userName;
            }

            User oldUser = GetUser(user.id);
            
            //throw error if we attempt to change username
            //we will probably implement this later but we are still
            //wrestling conceptually with it
            if (oldUser.userName != user.userName)
            {
                return null;
            }

            //Membership logic if we updated the email address
            //(or username in the future)
            if (oldUser.email != user.email)
            {
                MembershipUser membershipUser = Membership.GetUser(oldUser.userName);
                membershipUser.Email = user.email;
                Membership.UpdateUser(membershipUser);
            }

            if (user.graduationDate.Year < 1800)
            {
                user.graduationDate = new DateTime(1800, 1, 1);
            }
            if (user.birthDate.Year < 1800)
            {
                user.birthDate = new DateTime(1800, 1, 1);
            }

            //Entity logic for everything else (except password)
            try
            {
                VestnDB db = new VestnDB();
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
            }
            catch (Exception e)
            {
                logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), e.ToString());
                return null;
            }

            return user;
        }

        public bool ChangePassword(User user, string oldPassword, string newPassword)
        {
            // ChangePassword will throw an exception rather
            // than return false in certain failure scenarios.
            bool changePasswordSucceeded;
            try
            {
                MembershipUser currentUser = Membership.GetUser(user.userName, true /* userIsOnline */);
                changePasswordSucceeded = currentUser.ChangePassword(oldPassword, newPassword);
            }
            catch (Exception)//not logging this exception
            {
                changePasswordSucceeded = false;
            }
            return changePasswordSucceeded;
        }

        public bool ActivateUser(User user, bool toActivate)
        {
            MembershipUser membershipUser = Membership.GetUser(user.userName);
            membershipUser.IsApproved = toActivate;
            Membership.UpdateUser(membershipUser);

            membershipUser = Membership.GetUser(user.userName);
            return membershipUser.IsApproved;
        }

        public User VerifyUserEmail(User user, int toActivate)
        {
            user.emailVerified = toActivate;
            return UpdateUser(user);
        }

        public User MakePublic(User user, int toActivate)
        {
            user.isPublic = toActivate;
            return UpdateUser(user);
        }


        public User DeleteUser(User user)
        {
            Membership.DeleteUser(user.userName);

            VestnDB db = new VestnDB();
            db.users.Attach(user);
            
            //remove associated projects
            List<Project> projectList = new List<Project>();
            foreach (Project p in user.projects)
            {
                List<ProjectElement> peList = new List<ProjectElement>();
                foreach (ProjectElement pe in p.projectElements)
                {
                    peList.Add(pe);
                }
                foreach (ProjectElement pe in peList)
                {
                    db.projectElements.Remove(pe);
                }
               projectList.Add(p);
            }
            foreach (Project p in projectList)
            {
                db.projects.Remove(p);
            }
            user = db.users.Remove(user);
            db.SaveChanges();

            return user;
        }

        public bool DeleteUsers(List<int> userIds)
        {
            try
            {
                foreach (int i in userIds)
                {
                    DeleteUser(GetUser(i));
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public User GetEntityUser(int id)
        {
            VestnDB db = new VestnDB();
            User user;
            try
            {
                //user = db.users.Find(id);
                user = db.users
                    .Where(u => u.id == id)
                    .Include(u => u.projects.Select(b => b.projectElements))
                    .FirstOrDefault();
            }
            catch (InvalidOperationException e)
            {
                logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), e.ToString());
                return null;
            }
            return user;
        }

        private User GetEntityUser(string userName)
        {
            VestnDB db = new VestnDB();
            User user = db.users
                    .Where(u => u.userName == userName)
                    .Include(u => u.projects.Select(b => b.projectElements))
                    .FirstOrDefault();
            return user;
        }

        private User GetEntityUserByProfileURL(string profileURL)
        {
            VestnDB db = new VestnDB();
            User user = db.users
                    .Where(u => u.profileURL == profileURL)
                    .Include(u => u.projects.Select(b => b.projectElements))
                    .FirstOrDefault();
            return user;
        }

        private User UpdateEntityUser(User user)
        {
            //Entity logic for username update
            try
            {
                VestnDB db = new VestnDB();
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
            }
            catch (Exception e)
            {
                logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), e.ToString());
                return null;
            }
            return user;
        }

    //ASK DOUG ABOUT THIS METHOD
        /*
        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
         */

        [ExcludeFromCodeCoverage]
        #region Status Codes
        private static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // See http://go.microsoft.com/fwlink/?LinkID=177550 for
            // a full list of status codes.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "User name already exists. Please enter a different user name.";

                case MembershipCreateStatus.DuplicateEmail:
                    return "A user name for that e-mail address already exists. Please enter a different e-mail address.";

                case MembershipCreateStatus.InvalidPassword:
                    return "The password provided is invalid. Please enter a valid password value.";

                case MembershipCreateStatus.InvalidEmail:
                    return "The e-mail address provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "The password retrieval answer provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "The password retrieval question provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidUserName:
                    return "The user name provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.ProviderError:
                    return "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                case MembershipCreateStatus.UserRejected:
                    return "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                default:
                    return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
        }
        #endregion

        public User AddConnection(User u, int connection)
        {
            if (u.connections == null)
            {
                u.connections += connection;
            }
            else
            {
                u.connections = u.connections + "," + connection;
            }

            return UpdateUser(u);
        }

        public Experience AddExperience(Experience experience)
        {
            try
            {
                if (experience != null)
                {
                    VestnDB db = new VestnDB();
                    db.experience.Add(experience);
                    db.SaveChanges();
                    return experience;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception e)
            {
                logAccessor.CreateLog(DateTime.Now, "userAccessor - AddExperience", e.StackTrace);
                return null;
            }
        }
        public Experience GetExperience(int experienceId)
        {
            try
            {
                if (experienceId > 0)
                {
                    VestnDB db = new VestnDB();
                    Experience experience = (from c in db.experience where c.id == experienceId select c).FirstOrDefault();
                    return experience;
                }
                else
                {
                    return null;
                }
            }
            catch(Exception e)
            {
                logAccessor.CreateLog(DateTime.Now, "userAccessor - GetExperience", e.StackTrace);
                return null;
            }
        }
        public Experience UpdateExperience(Experience experience)
        {
            try
            {
                if (experience != null)
                {
                    VestnDB db = new VestnDB();
                    db.Entry(experience).State = EntityState.Modified;
                    db.SaveChanges();
                    return experience;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception e)
            {
                logAccessor.CreateLog(DateTime.Now, "userAccessor - UpdateExperience", e.StackTrace);
                return null;
            }
        }
        public bool DeleteExperience(Experience experience)
        {
            try
            {
                bool wasDeleted = false;
                if (experience != null)
                {
                    VestnDB db = new VestnDB();
                    db.experience.Attach(experience);
                    db.experience.Remove(experience);
                    db.SaveChanges();
                    wasDeleted = true;
                    return wasDeleted;
                }
                else
                {
                    return wasDeleted;
                }
            }
            catch (Exception e)
            {
                logAccessor.CreateLog(DateTime.Now, "userAccessor - deleteExperience", e.StackTrace);
                return false;
            }
        }

        public List<Experience> GetUserExperience(int userId)
        {
            try
            {
                VestnDB db = new VestnDB();
                List<Experience> experienceList = (from c in db.experience where c.userId == userId select c).ToList();
                return experienceList;
            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, "userAccessor - GetUserExperience", ex.StackTrace);
                return null;
            }
        }

        public Reference AddReference(Reference reference)
        {
            try
            {
                if (reference != null)
                {
                    VestnDB db = new VestnDB();
                    db.reference.Add(reference);
                    db.SaveChanges();
                    return reference;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception e)
            {
                logAccessor.CreateLog(DateTime.Now, "userAccessor - AddReference", e.StackTrace);
                return null;
            }
        }
        public Reference GetReference(int referenceId)
        {
            try
            {
                if (referenceId > 0)
                {
                    VestnDB db = new VestnDB();
                    Reference reference = (from c in db.reference where c.id == referenceId select c).FirstOrDefault();
                    return reference;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception e)
            {
                logAccessor.CreateLog(DateTime.Now, "userAccessor - GetReference", e.StackTrace);
                return null;
            }
        }
        public Reference UpdateReference(Reference reference)
        {
            try
            {
                if (reference != null)
                {
                    VestnDB db = new VestnDB();
                    db.Entry(reference).State = EntityState.Modified;
                    db.SaveChanges();
                    return reference;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception e)
            {
                logAccessor.CreateLog(DateTime.Now, "userAccessor - UpdateReference", e.StackTrace);
                return null;
            }
        }
        public bool DeleteReference(Reference reference)
        {
            try
            {
                bool wasDeleted = false;
                if (reference != null)
                {
                    VestnDB db = new VestnDB();
                    db.reference.Attach(reference);
                    db.reference.Remove(reference);
                    db.SaveChanges();
                    wasDeleted = true;
                    return wasDeleted;
                }
                else
                {
                    return wasDeleted;
                }
            }
            catch (Exception e)
            {
                logAccessor.CreateLog(DateTime.Now, "userAccessor - deleteReference", e.StackTrace);
                return false;
            }
        }

        public List<Reference> GetUserReference(int userId)
        {
            try
            {
                List<Reference> referenceList = new List<Reference>();
                VestnDB db = new VestnDB();
                referenceList = (from c in db.reference where c.userId == userId select c).ToList();
                return referenceList;
            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, "userAccessor - GetUserReference", ex.StackTrace);
                return null;
            }
        }

    }
}
