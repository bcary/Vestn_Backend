using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Manager;
using Entity;
using Models;
using System.Web.Security;
using System.Web.Routing;
using System.Net;
using System.IO;
using Accessor;
using Engine;
using System.Text;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;


namespace Controllers
{
    public class ProjectController : BaseController
    {
        UserManager userManager = new UserManager();
        ProjectManager projectManager = new ProjectManager();
        AnalyticsAccessor aa = new AnalyticsAccessor();
        LogAccessor logAccessor = new LogAccessor();
        AuthenticaitonEngine authenticationEngine = new AuthenticaitonEngine();

        [AcceptVerbs("POST", "OPTIONS")]
        [AllowCrossSiteJson]
        public string AddProject(string token, string name = null, string description = null)
        {
            if (Request.RequestType.Equals("OPTIONS", StringComparison.InvariantCultureIgnoreCase))  //This is a preflight request
            {
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

                    User user = userManager.GetUser(userId);

                    Project project = projectManager.CreateProject(user, new List<ProjectElement>());

                    project.name = name;
                    project.description = description;
                    project.privacy = "private";
                    projectManager.UpdateProject(project);

                    //refresh the user object with the changes
                    user = userManager.GetUser(userId);
                    JsonModels.CompleteProject response = new JsonModels.CompleteProject();
                    if (name == null)
                    {
                        response.name = "New Project";
                    }
                    else
                    {
                        response.name = name;
                    }
                    if (description == null)
                    {
                        response.description = "New Project";
                    }
                    else
                    {
                        response.description = description;
                    }
                    response.id = project.id;
                    response.artifacts = null;
                    response.projectTags = null;
                    response.projectElementOrder = "";
                    string returnVal;
                    try
                    {
                        returnVal = Serialize(response);
                    }
                    catch (Exception exception)
                    {
                        return GetFailureMessage(exception.Message);
                    }
                    return AddSuccessHeaders(returnVal);
                }
                catch (Exception ex)
                {
                    logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), ex.ToString());
                    return GetFailureMessage("Something went wrong while creating this project");
                }
            }
        }

        [AcceptVerbs("POST", "OPTIONS")]
        [AllowCrossSiteJson]
        public string DeleteProject(int projectId, string token)
        {
            if (Request.RequestType.Equals("OPTIONS", StringComparison.InvariantCultureIgnoreCase))  //This is a preflight request
            {
                return null;
            }
            else
            {
                try
                {
                    int userId = authenticationEngine.authenticate(token);
                    if (userId < 0)
                    {
                        return GetFailureMessage("User not authenticated, please log in!");
                    }
                    User user = userManager.GetUser(userId);

                    Project project = projectManager.GetProject(projectId);
                    if (!projectManager.IsUserOwnerOfProject(projectId, user))
                    {
                        return GetFailureMessage("User not authorized to delete this project");
                    }

                    if (project.name == "About")
                    {
                        return GetFailureMessage("Cannot Delete About Project");
                    }

                    projectManager.DeleteProject(project);
                    projectManager.deleteProjectFromOrder(user, projectId);
                    userManager.UpdateUser(user);

                    //refresh the user object with the changes
                    user = userManager.GetUser(userId);

                    aa.CreateAnalytic("Delete Project", DateTime.Now, user.userName, "Number of elements: " + project.projectElements.Count());

                    return AddSuccessHeaders("Successfully deleted project", true);
                }
                catch (Exception ex)
                {
                    logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), ex.ToString());
                    return GetFailureMessage("Something went wrong while deleting this project.");
                }
            }
        }

        [AcceptVerbs("POST", "OPTIONS")]
        [AllowCrossSiteJson]
        public string AddArtifact_Media(int projectId, string token, string qqfile = null)
        {
            if (Request.RequestType.Equals("OPTIONS", StringComparison.InvariantCultureIgnoreCase))  //This is a preflight request
            {
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
                    //int newProjectElementId = -1;
                    JsonModels.UploadReponse response = new JsonModels.UploadReponse();
                    User user = userManager.GetUser(userId);
                    string artifactType = "null";
                    if (!projectManager.IsUserOwnerOfProject(projectId, user))
                    {
                        //return Json(new { Error = "Can't add picture at this time" });
                        return GetFailureMessage("You are not authorized to add this picture");
                    }
                    if (Request != null)
                    {
                        if (qqfile == null && Request.Files.Count == 0)
                        {
                            return GetFailureMessage("No files submitted to server");
                        }

                        var length = Request.ContentLength;
                        var bytes = new byte[length];
                        Request.InputStream.Read(bytes, 0, length);
                        Stream s = new MemoryStream(bytes);

                        if (qqfile.Contains(".jpeg") || qqfile.Contains(".jpg") || qqfile.Contains(".png") || qqfile.Contains(".bmp") || qqfile.Contains(".JPEG") || qqfile.Contains(".JPG") || qqfile.Contains(".PNG") || qqfile.Contains(".BMP"))
                        {
                            response = projectManager.UploadPictureElement(projectId, s, qqfile);
                            if (response == null)
                            {
                                return GetFailureMessage("An error occured saving the docuement.");
                            }
                            aa.CreateAnalytic("Add Media", DateTime.Now, user.userName, qqfile);
                            artifactType = "picture";
                        }
                        else if (qqfile.Contains(".PDF") || qqfile.Contains(".pdf") || qqfile.Contains(".doc") || qqfile.Contains(".docx") || qqfile.Contains(".ppt") || qqfile.Contains(".pptx") || qqfile.Contains(".xls") || qqfile.Contains(".xlsx") || qqfile.Contains(".txt") || qqfile.Contains(".rtf") || qqfile.Contains(".DOC") || qqfile.Contains(".DOCX") || qqfile.Contains(".PPT") || qqfile.Contains(".PPTX") || qqfile.Contains(".XLS") || qqfile.Contains(".XLSX") || qqfile.Contains(".TXT") || qqfile.Contains(".RTF"))
                        {
                            response = projectManager.AddDocumentElement(projectId, null, s, qqfile, user.userName);

                            //check if this is development enviroment or LIVE
                            var account = CloudStorageAccount.Parse(RoleEnvironment.GetConfigurationSettingValue("BlobConnectionString"));
                            //if (account.BlobEndpoint.IsLoopback)
                            //{
                            //    response.artifactURL = @"http://127.0.0.1:10000/devstoreaccount1/pdfs/" + response.artifactURL;
                            //}
                            //else
                            //{
                            //    response.artifactURL = "https://vestnstaging.blob.core.windows.net/pdfs/" + response.artifactURL;//TODO change this when it goes live to vestnstorage
                            //}
                            //--------------------------
                            if (response == null)
                            {
                                return GetFailureMessage("File type not accepted");
                            }
                            aa.CreateAnalytic("Add Media", DateTime.Now, user.userName, qqfile);
                            artifactType = "document";
                        }
                        else
                        {
                            return GetFailureMessage("You did not upload an accepted picture or document type: (jpeg, jpg, png, bmp, doc, docx, ppt, pptx, xls, xlsx, txt, rtf");
                        }
                    }
                    else
                    {
                        return GetFailureMessage("Server did not receive file post");
                    }
                    //refresh the user object with the changes
                    user = userManager.GetUser(userId);
                    //build the artifact response

                    JsonModels.Artifact artifactResponse = new JsonModels.Artifact();
                    artifactResponse.id = response.id;
                    if (artifactType == "picture")
                    {
                        artifactResponse.artifactLocation = "https://vestnstaging.blob.core.windows.net/thumbnails/" + response.artifactURL;
                        artifactResponse.fileLocation = response.fileURL;
                    }
                    else if (artifactType == "document")
                    {
                        artifactResponse.artifactLocation = response.artifactURL;
                        artifactResponse.fileLocation = response.fileURL;
                    }

                    artifactResponse.title = response.name;
                    artifactResponse.type = artifactType;
                    artifactResponse.creationDate = DateTime.Now.ToString();
                    artifactResponse.description = "This is an artifact!";

                    string realReturnVal;
                    try
                    {
                        realReturnVal = Serialize(artifactResponse);
                    }
                    catch (Exception exception)
                    {
                        return GetFailureMessage(exception.Message);
                    }
                    return AddSuccessHeaders(realReturnVal);
                }
                catch (Exception ex)
                {
                    logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), ex.ToString());
                    return GetFailureMessage("Problem saving media to cloud storage");
                }
            }
        }

        [AcceptVerbs("POST", "OPTIONS")]
        [AllowCrossSiteJson]
        public string AddArtifact_Video(int projectId = -1, string videoLink = null, string token = null)
        {
            if (Request.RequestType.Equals("OPTIONS", StringComparison.InvariantCultureIgnoreCase))  //This is a preflight request
            {
                return null;
            }
            else
            {
                try
                {
                    int userId = -1;
                    if (token != null)
                    {
                        userId = authenticationEngine.authenticate(token);
                    }
                    else
                    {
                        return GetFailureMessage("User authentication token not recieved.");
                    }
                    if (userId < 0)
                    {
                        return GetFailureMessage("User is not authenticated, please log in!");
                    }
                    User user = userManager.GetUser(userId);
                    if (projectId < 0)
                    {
                        return GetFailureMessage("A projectId was not recieved");
                    }
                    if (!projectManager.IsUserOwnerOfProject(projectId, user))
                    {
                        return GetFailureMessage("User is not authorized to complete this action");
                    }
                    string vType;
                    if (videoLink != null)
                    {
                        vType = "unknown";
                        if (videoLink.Contains("youtube"))
                        {
                            if (videoLink.Contains("http://"))
                            {
                                videoLink = videoLink.Substring(31, 11);
                                vType = "youtube";
                            }
                            else
                            {
                                videoLink = videoLink.Substring(24, 11);
                                vType = "youtube";
                            }
                        }
                        else if (videoLink.Contains("youtu."))
                        {
                            videoLink = videoLink.Substring(16);
                            vType = "youtube";
                        }
                        else if (videoLink.Contains("vimeo"))
                        {
                            string[] s = videoLink.Split('/');
                            videoLink = s[s.Count() - 1];
                            vType = "vimeo";
                        }
                    }
                    else
                    {
                        return GetFailureMessage("A videoLink was not recieved");
                    }

                    JsonModels.Artifact response = projectManager.AddVideoElement(projectId, "Video Description", videoLink, vType);

                    aa.CreateAnalytic("Add Media", DateTime.Now, user.userName, "Video link");
                    string returnVal;
                    try
                    {
                        returnVal = Serialize(response);
                    }
                    catch (Exception exception)
                    {
                        return GetFailureMessage(exception.Message);
                    }
                    return AddSuccessHeaders(returnVal);
                }
                catch (Exception ex)
                {
                    logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), ex.ToString());
                    return GetFailureMessage("Error occured uploading your video");
                }
            }
        }

        [AcceptVerbs("POST", "OPTIONS")]
        [AllowCrossSiteJson]
        public string AddArtifact_Code(int projectId = -1, string code = null, string type = null, string token = null)
        {
            if (Request.RequestType.Equals("OPTIONS", StringComparison.InvariantCultureIgnoreCase))  //This is a preflight request
            {
                return null;
            }
            else
            {
                try
                {
                    int userId = -1;
                    if (token != null)
                    {
                        userId = authenticationEngine.authenticate(token);
                    }
                    else
                    {
                        return GetFailureMessage("A token must be passed in");
                    }
                    if (userId < 0)
                    {
                        return GetFailureMessage("User is not authenticated, please log in!");
                    }
                    User user = userManager.GetUser(userId);
                    if (projectId != null)
                    {
                        if (!projectManager.IsUserOwnerOfProject(projectId, user))
                        {
                            //return Json(new { Error = "Can't add video at this time" });
                            return GetFailureMessage("User is not authorized to complete this action");
                        }
                    }
                    else
                    {
                        return GetFailureMessage("A projectId must be passed in");
                    }
                    JsonModels.Artifact response = new JsonModels.Artifact();
                    if (code != null && type != null)
                    {
                        response = projectManager.AddCodeElement(projectId, code, type);
                        aa.CreateAnalytic("Add Media", DateTime.Now, user.userName, "Code Sample");
                    }
                    else
                    {
                        return GetFailureMessage("code and type must be passed in");
                    }
                    string returnVal;
                    try
                    {
                        returnVal = Serialize(response);
                    }
                    catch (Exception exception)
                    {
                        return GetFailureMessage(exception.Message);
                    }
                    return AddSuccessHeaders(returnVal);
                }
                catch (Exception ex)
                {
                    logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), ex.ToString());
                    return GetFailureMessage("Error occured uploading your video");
                }
            }
        }

        [AcceptVerbs("POST", "OPTIONS")]
        [AllowCrossSiteJson]
        public string UpdateProjectModel(IEnumerable<Project> project, string token = null)
        {
            if (Request.RequestType.Equals("OPTIONS", StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }
            try
            {
                int userId = -1;
                if (token != null)
                {
                    userId = authenticationEngine.authenticate(token);
                }
                else
                {
                    return GetFailureMessage("An authentication token must be passed in");
                }
                if (userId < 0)
                {
                    return GetFailureMessage("You are not authenticated, please log in!");
                }
                User authUser = userManager.GetUser(userId);
                Project projectFromJson = project.FirstOrDefault();
                Project originalProject = projectManager.GetProject(projectFromJson.id);
                if (projectFromJson == null)
                {
                    return GetFailureMessage("The project JSON model did not bind correctly");
                }
                else
                {
                    if (originalProject == null)
                    {
                        return GetFailureMessage("The project model does not exist in the database");
                    }
                    if (projectManager.IsUserOwnerOfProject(projectFromJson.id, authUser))
                    {
                        originalProject.description = (projectFromJson.description != null) ? projectFromJson.description : null;
                        originalProject.name = (projectFromJson.name != null) ? projectFromJson.name : null;
                        originalProject.projectElementOrder = (projectFromJson.projectElementOrder != null) ? projectFromJson.projectElementOrder : null;

                        if (projectFromJson.privacy != null)
                        {
                            if (projectFromJson.privacy.ToLower() == "deleted")
                            {
                                originalProject.privacy = "deleted";
                                originalProject.isActive = false;
                                projectManager.deleteProjectFromOrder(authUser, originalProject.id);
                            }
                            else
                            {
                                originalProject.privacy = projectFromJson.privacy;
                            }
                        }

                        originalProject.dateModified = DateTime.Now;

                        projectManager.UpdateProject(originalProject);
                        return AddSuccessHeaders(Serialize(projectManager.GetProjectJson(originalProject)));
                    }
                    else
                    {
                        return GetFailureMessage("User is not authorized to edit this project");
                    }
                }
            }
            catch (Exception ex)
            {
                return GetFailureMessage("Something went wrong while updating this Project.");
            }
        }

        [AcceptVerbs("POST", "OPTIONS")]
        [AllowCrossSiteJson]
        public string UpdateArtifactModel(IEnumerable<JsonModels.Artifact> artifact, string token = null)
        {
            if (Request.RequestType.Equals("OPTIONS", StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }
            try
            {
                int userId = -1;
                if (token != null)
                {
                    userId = authenticationEngine.authenticate(token);
                }
                else
                {
                    return GetFailureMessage("An authentication token must be passed in");
                }
                if (userId < 0)
                {
                    return GetFailureMessage("You are not authenticated, please log in!");
                }
                User authUser = userManager.GetUser(userId);
                JsonModels.Artifact artifactFromJson = artifact.FirstOrDefault();
                ProjectElement originalElement = projectManager.GetProjectElement(artifactFromJson.id);
                if (originalElement == null)
                {
                    return GetFailureMessage("The artifact model does not exist in the database");
                }
                else
                {
                    Project p = projectManager.IsUserOwnerOfProjectElement(artifactFromJson.id, authUser);
                    if (p != null)
                    {
                        originalElement.description = (artifactFromJson.description != null) ? artifactFromJson.description : null;
                        originalElement.title = (artifactFromJson.title != null) ? artifactFromJson.title : null;
                        projectManager.UpdateProjectElement(originalElement);

                        p.dateModified = DateTime.Now;
                        projectManager.UpdateProject(p);

                        return AddSuccessHeaders(Serialize(projectManager.GetArtifactJson(originalElement)));
                    }
                    else
                    {
                        return GetFailureMessage("User is not authorized to edit this Artifact");
                    }
                }
            }
            catch (Exception ex)
            {
                return GetFailureMessage("Something went wrong while updating this artifact");
            }
        }

        [AcceptVerbs("POST", "OPTIONS")]
        [AllowCrossSiteJson]
        public string UpdateCoverPicture(int projectId, string token = null, string qqfile = null)
        {
            if (Request.RequestType.Equals("OPTIONS", StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }
            try
            {
                int userId = -1;
                if (token != null)
                {
                    userId = authenticationEngine.authenticate(token);
                }
                else
                {
                    return GetFailureMessage("An authentication token must be passed in");
                }
                if (userId < 0)
                {
                    return GetFailureMessage("You are not authenticated, please log in!");
                }
                User user = userManager.GetUser(userId);
                Project project;
                if (projectId > 0)
                {
                    project = projectManager.GetProject(projectId);
                }
                else
                {
                    return GetFailureMessage("Invalid projectId");
                }
                if (project == null)
                {
                    return GetFailureMessage("Project not found");
                }
                if (!projectManager.IsUserOwnerOfProject(projectId, user))
                {
                    return GetFailureMessage("User not authorized to update this project!");
                }
                else
                {
                    if (qqfile != null || Request.Files.Count == 1)
                    {
                        var length = Request.ContentLength;
                        var bytes = new byte[length];
                        Request.InputStream.Read(bytes, 0, length);
                        Stream s = new MemoryStream(bytes);
                        JsonModels.UploadReponse response = new JsonModels.UploadReponse();
                        response = projectManager.UploadPictureElement(projectId, s, "coverPicture", true);
                        if (response == null)
                        {
                            return GetFailureMessage("An error occured saving the docuement.");
                        }
                        else
                        {
                            return AddSuccessHeaders("http://vestnstaging.blob.core.windows.net/thumbnails/" + response.artifactURL, true);
                        }
                    }
                    else
                    {
                        return GetFailureMessage("No files were posted to the server");
                    }
                }
            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, "ProjectController - UpdateCoverPicture", ex.StackTrace);
                return GetFailureMessage("Something went wrond while updating this project's cover picture");
            }
        }

        private void DeleteElementOrder(Project p, ProjectElement element)
        {
            projectManager.deleteProjectElementFromOrder(p, element.id);
        }

        [AcceptVerbs("POST", "OPTIONS")]
        [AllowCrossSiteJson]
        public string DeleteArtifact(int projectId, int artifactId, string token)
        {
            if (Request.RequestType.Equals("OPTIONS", StringComparison.InvariantCultureIgnoreCase))  //This is a preflight request
            {
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
                    User user = userManager.GetUser(userId);
                    if (!projectManager.IsUserOwnerOfProject(projectId, user))
                    {
                        return GetFailureMessage("You are not authorized to delete this artifact, :(");
                    }

                    Project p = projectManager.GetProject(projectId);
                    p.dateModified = DateTime.Now;
                    ProjectElement e = projectManager.GetProjectElement(artifactId);
                    p.projectElements.RemoveAll(pr => pr.id == e.id);
                    DeleteElementOrder(p, e);
                    p = projectManager.UpdateProject(p);
                    projectManager.DeleteProjectElement(e);
                    //refresh the user object with the changes
                    user = userManager.GetUser(userId);
                    return AddSuccessHeaders("This artifact was successfully removed from the specified project", true);
                }
                catch (Exception ex)
                {
                    logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), ex.ToString());
                    return GetFailureMessage("Something went wrong while deleting this artifact, whoops!");
                }
            }
        }

        [AcceptVerbs("POST", "OPTIONS")]
        [AllowCrossSiteJson]
        public string GetProject(int[] projectId, string token = null)
        {
            if (Request.RequestType.Equals("OPTIONS", StringComparison.InvariantCultureIgnoreCase))  //This is a preflight request
            {
                return null;
            }
            else
            {
                if (token != null)
                {
                    int userId = authenticationEngine.authenticate(token);
                    if (userId < 0)
                    {
                        // ALLOW NON AUTHENTICATED REQUESTS
                        //return GetFailureMessage("You are not authenticated, please log in!");
                    }
                }
                string returnVal;
                try
                {
                    List<JsonModels.CompleteProject> projects = projectManager.GetCompleteProjects(projectId);
                    if (projects != null)
                    {
                        try
                        {
                            returnVal = Serialize(projects);
                        }
                        catch (Exception exception)
                        {
                            return GetFailureMessage(exception.Message);
                        }
                    }
                    else
                    {
                        return GetFailureMessage("No Information Found");
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
}
