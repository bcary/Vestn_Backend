using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Data.Entity;
using Manager;
using Entity;
using System.IO;
using System.Net;
using Controllers;

namespace Vestn2._0
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Profile_Default", // Route name
                "v", // URL with parameters
                new { controller = "User", action = "Profile", profileURL = "" }, // Parameter defaults
                new string[]{ "Controllers" }
            );

            routes.MapRoute(
                "Profile", // Route name
                "v/{profileURL}", // URL with parameters
                new { controller = "User", action = "Profile" }, // Parameter defaults
                new string[] { "Controllers" }
            );

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional }, // Parameter defaults
                new string[]{ "Controllers" }

            );

        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            //UNCOMMENT TO GENERATE TABLES IN ORDER TO IMPORT SCRIPTS FROM
            //SQL SERVER MANAGEMENT STUDIO INTO VESTNDB PROJECT.  DO NOT UNCOMMENT
            //AND RUN UNTIL VESTNDB DATABSE IS DELETED IN SQL MANAGEMENT STUDIO
            Database.SetInitializer<VestnDB>(new DropCreateDatabaseIfModelChanges<VestnDB>());
            //Database.SetInitializer<VestnDB>(null);
            new VestnDB().users.FirstOrDefault();

            //updatePictureElementsForImageGallery();

            InitializeConnections();

            //OPTIONAL CALL TO CREATE SAMPLE USER (MOHAMMAD WONG) ON STARTUP
            //new UserController().CreateSampleUser();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
            ControllerBuilder.Current.DefaultNamespaces.Add("UserClientMembers.Controllers");

        }

        //private void updatePictureElementsForImageGallery()
        //{
        //    UploadManager uploadManager = new UploadManager();
        //    ProjectManager projectManager = new ProjectManager();
        //    List<ProjectElement> projectElements = new VestnDB().projectElements.ToList();
        //    List<ProjectElement_Picture> pictureElements = new List<ProjectElement_Picture>();
        //    foreach (ProjectElement element in projectElements)
        //    {
        //        if (element.GetType() == typeof(ProjectElement_Picture))
        //        {
        //            pictureElements.Add((ProjectElement_Picture)element);
        //        }
        //    }
        //    foreach (ProjectElement_Picture picture in pictureElements)
        //    {
        //        if (picture.pictureGalleriaThumbnailLocation == null)
        //        {
        //            picture.pictureGalleriaThumbnailLocation = uploadManager.generateThumbnail(picture.pictureLocation, picture.id, "PictureElement_Galleria", 1000, 700);
        //            projectManager.UpdateProjectElement(picture);
        //        }
        //    }
        //}

        private void InitializeConnections()
        {
            UserManager userManager = new UserManager();

            List<User> userList = new VestnDB().users.ToList();
            foreach (User user in userList)
            {
                if (user.connections == null)
                {
                    user.connections = "";
                    userManager.UpdateUser(user);
                }
            }
        }
    }
}
