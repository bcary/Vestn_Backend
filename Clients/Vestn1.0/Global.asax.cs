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
using Accessor;
using UserClientMembers.Controllers;
using Engine;

namespace UserClientMembers
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class SubdomainRoute : RouteBase
    {
        public override RouteData GetRouteData(HttpContextBase httpContext)
        {
            var url = httpContext.Request.Headers["HOST"];
            var index = url.IndexOf(".");

            if (index < 0)
                return null;

            var subDomain = url.Substring(0, index);

            if (subDomain != null || subDomain != "")
            {
                var routeData = new RouteData(this, new MvcRouteHandler());
                routeData.Values.Add("controller", "Subdomain"); //Goes to the User1Controller class
                routeData.Values.Add("action", "RedirectSubdomain"); //Goes to the Index action on the User1Controller
                routeData.Values.Add("subdomain",subDomain);

                return routeData;
            }
            return null;
        }

        public override VirtualPathData GetVirtualPath(RequestContext requestContext, RouteValueDictionary values)
        {
            //Implement your formating Url formating here
            return null;
        }
    }


    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.Add(new SubdomainRoute());
            //routes.MapRoute(
            //    "Profile_Default", // Route name
            //    "v", // URL with parameters
            //    new { controller = "User", action = "Profile", profileURL = "" } // Parameter defaults
            //);

            //routes.MapRoute(
            //    "Profile", // Route name
            //    "v/{profileURL}", // URL with parameters
            //    new { controller = "User", action = "Profile" } // Parameter defaults
            //);
            
            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
                    
            );

        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            //UNCOMMENT TO GENERATE TABLES IN ORDER TO IMPORT SCRIPTS FROM
            //SQL SERVER MANAGEMENT STUDIO INTO VESTNDB PROJECT.  DO NOT UNCOMMENT
            //AND RUN UNTIL VESTNDB DATABSE IS DELETED IN SQL MANAGEMENT STUDIO
            //Database.SetInitializer<VestnDB>(new DropCreateDatabaseIfModelChanges<VestnDB>());
            Database.SetInitializer<VestnDB>(null);
            new VestnDB().users.FirstOrDefault();

            //updatePictureElementsForImageGallery();

            //InitializeConnections();
            InitializeTags();
            //UserManager um = new UserManager();
            //User u = new Entity.User();
            //u.firstName = "b";
            //UserController uc = new UserController();
            //uc.Register("b@b.com", "rrrrrr");

        

            //OPTIONAL CALL TO CREATE SAMPLE USER (MOHAMMAD WONG) ON STARTUP
            //new UserController().CreateSampleUser();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
            ControllerBuilder.Current.DefaultNamespaces.Add("UserClientMembers.Controllers");
            LogAccessor la = new LogAccessor();
            DateTime dt = DateTime.Now;
            //UserController uc = new UserController();
            //uc.Register("test@vestn.com", "test");
        }

        private void InitializeTags()
        {
            VestnDB db = new VestnDB();
            List<sTag> sTags = db.sTag.ToList();
            if (sTags.Count == 0)
            {
                FillFreeLancerTags();
            }
        }

        private void FillFreeLancerTags()
        {
            TagManager tagManager = new TagManager();
            TagAccessor tagAccessor = new TagAccessor();

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
                        sTag mid = tagManager.CreateSTag(tagAccessor.GetSTag(top.value).id, value);
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
