using System;
using System.Data.Entity;
using System.Collections.Generic;
using System.Data;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Security;

namespace Entity
{
    public class DataBaseCleaup
    {
        public void Initialize()
        {
            dropData();
            fillTags();
            fillUsers();
        }

        public void InitializeWithMembership()
        {
            dropData();
            fillTags();
            fillUsersWithMembership();
        }

        public void dropData()
        {
            VestnDB vestnDB = new VestnDB();

            var users = from o in vestnDB.users select o;
            foreach (User u in users)
            {
                vestnDB.users.Remove(u);
            }

            var projectElements = from o in vestnDB.projectElements select o;
            foreach (ProjectElement p in projectElements)
            {
                vestnDB.projectElements.Remove(p);
            }

            var projects = from o in vestnDB.projects select o;
            foreach (Project p in projects)
            {
                vestnDB.projects.Remove(p);
            }

            vestnDB.SaveChanges();

            
        }

        private void fillUsers()
        {
            VestnDB vestnDB = new VestnDB();
            User user = new User
            {
                userName = "databaseTest",
                email = "vestnteam@gmail.com",
                firstName = "database",
                lastName = "test",
                birthDate = DateTime.Now,
                graduationDate = DateTime.Now.AddYears(1),
                tagIds = null,
                location = "Lincoln, Ne",
                school = "Univeristy of Nebraska-Lincoln",
                major= "Testing",
                phoneNumber = "555-555-5555",
                willingToRelocate = WillingToRelocateType.no,
            };

            User user2 = new User
            {
                userName = "databaseTest2",
                email = "vestnteam@gmail.com",
                firstName = "database2",
                lastName = "test2",
                birthDate = DateTime.Now,
                graduationDate = DateTime.Now.AddYears(1),
                tagIds = null,
                location = "Omaha, Ne",
                school = "Univeristy of Nebraska-Omaha",
                major = "Testing2",
                phoneNumber = "222-222-2222",
                willingToRelocate = WillingToRelocateType.yes,
            };
            vestnDB.users.Add(user);
            vestnDB.users.Add(user2);
            vestnDB.SaveChanges();

            fillProjects(user);
            fillProjects(user2);

        }

        private void fillProjects(User u)
        {
            VestnDB vestnDB = new VestnDB();
            Project project = new Project
            {
                name = "Test Project",
                description = "Test Description",
            };
            
            

            //Project Elements
            ProjectElement_Information informationElement;
            ProjectElement_Experience experienceElement;
            ProjectElement_Document documentElement;
            ProjectElement_Video videoElement;
            ProjectElement_Picture pictureElement;
            List<ProjectElement> projectElements;

            informationElement = new ProjectElement_Information
            {

                location = "Here",
                email = "vestnteam@gmail.com",
                school= "Univeristy of Nebraska- Lincoln",
                phone = "402-402-4111",
                major = "Actuarial Science",
                minor = "Business Administration",
                description = "tetetetetet"
            };
            documentElement = new ProjectElement_Document
            {
                description = "im a document",
                documentLocation = "asdfasdf"
            };
            experienceElement = new ProjectElement_Experience
            {
                jobTitle = "Owner",
                description = "I'm CEO Bitch.",
                startDate = new DateTime(1991, 6, 12),
                endDate = new DateTime(2011, 6, 12)
            };
            pictureElement = new ProjectElement_Picture
            {
                description = "im a picture",
                pictureLocation = "sadfsadfsa"
            };
            videoElement = new ProjectElement_Video
            {
                //put a test id you know in here haun
                videoId = "xxxxx",
                description = "asdfsadfasdf"
            };


            projectElements = new List<ProjectElement>();
            projectElements.Add(informationElement);
            projectElements.Add(experienceElement);
            projectElements.Add(documentElement);
            projectElements.Add(pictureElement);
            projectElements.Add(videoElement);

            //Save elements to project
            project.projectElements = projectElements;

            //Save Changes to DB
            VestnDB db = new VestnDB();
            db.projects.Add(project);
            if (u.projects == null)
            {
                u.projects = new List<Project>();
            }
            u.projects.Add(project);
            db.Entry(u).State = EntityState.Modified;
            db.SaveChanges();
        }

        
        private void fillUsersWithMembership()
        {
            User user1 = new User
            {
                userName = "databaseTest",
                email = "vestnteam@gmail.com",
                firstName = "database",
                lastName = "test",
                birthDate = DateTime.Now,
                tagIds = null,
                location = "Lincoln, Ne",
                school = "Univeristy of Nebraska-Lincoln",
                major = "Testing",
                phoneNumber = "555-555-5555",
                willingToRelocate = WillingToRelocateType.no,
            };

            User user2 = new User
            {
                userName = "databaseTest2",
                email = "vestnteam@gmail.com",
                firstName = "database2",
                lastName = "test2",
                birthDate = DateTime.Now,
                tagIds = null,
                location = "Omaha, Ne",
                school = "Univeristy of Nebraska-Omaha",
                major = "Testing2",
                phoneNumber = "222-222-2222",
                willingToRelocate = WillingToRelocateType.yes,
            };

            CreateMembershipUser(user1, "password!1");
            CreateMembershipUser(user2, "password!1");

        }

        public void CreateMembershipUser(User user, string password)
        {
            //Membership logic
            MembershipCreateStatus createStatus;
            Membership.CreateUser(user.userName, password, user.email, null, null, false, null, out createStatus);
            if (createStatus != MembershipCreateStatus.Success)
            {
            }

            //Entity logic
            try
            {
                VestnDB db = new VestnDB();
                if (user.willingToRelocate == null)
                {
                    user.willingToRelocate = WillingToRelocateType.undecided;
                }
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
                db.users.Add(user);
                db.SaveChanges();

                //fill with projects
                fillProjects(user);
            }
            catch (InvalidOperationException)
            {
                //If the entity logic failed, delete the stray MemershipUser
                Membership.DeleteUser(user.userName);
                //cant log errors here because of circular dependence
            }
        }

        private void fillTags()
        {

        }

    }
}
