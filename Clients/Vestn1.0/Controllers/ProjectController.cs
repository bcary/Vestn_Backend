﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Drawing;
using System.Drawing.Imaging;
using Manager;
using Entity;
using UserClientMembers.Models;
using System.Web.Security;
using System.Web.Routing;
using System.Net;
using System.IO;
using Accessor;
using Engine;
using System.Text;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;


namespace UserClientMembers.Controllers
{
    public class ProjectController : BaseController
    {
        UserManager userManager = new UserManager();
        ProjectManager projectManager = new ProjectManager();
        AnalyticsAccessor aa = new AnalyticsAccessor();
        LogAccessor logAccessor = new LogAccessor();
        AuthenticaitonEngine authenticationEngine = new AuthenticaitonEngine();
        ActivityManager activityManager = new ActivityManager();
        public string TestMe()
        {
            return "success";
        }
        
        /// <summary>
        /// Identifies the user and creates and adds a Project Entity bound to the User.
        /// </summary>
        /// <returns>JsonResult</returns>
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
                        return AddErrorHeader("You are not authenticated, please log in!", 2);
                    }

                    User user = userManager.GetUser(userId);

                    Project project = projectManager.CreateProject(user, new List<ProjectElement>());

                    project.name = name;
                    project.description = description;
                    project.privacy = "public";
                    project.dateModified = DateTime.Now;
                    projectManager.UpdateProject(project);

                    //refresh the user object with the changes
                    user = userManager.GetUser(userId);
                    //User try2 = userManager.GetUserWithNetworks(userId);
                    JsonModels.CompleteProject response = new JsonModels.CompleteProject();

                    response.title = project.name;
                    response.description = project.description;
                    response.privacy = project.privacy;
                    response.id = project.id;
                    response.artifacts = null;
                    response.projectTags = null;
                    response.artifactOrder = "";
                    string returnVal;
                    try
                    {
                        returnVal = Serialize(response);
                    }
                    catch (Exception exception)
                    {
                        return AddErrorHeader(exception.Message, 1);
                    }
                    activityManager.AddActivity(user.id, "Project", "Added", response.id);
                    aa.CreateAnalytic("Add_Project", DateTime.Now, user.userName);
                    return AddSuccessHeader(returnVal);
                }
                catch (Exception ex)
                {
                    logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), ex.ToString());
                    return AddErrorHeader("Something went wrong while creating this project", 1);
                }
            }
        }

        /// <summary>
        /// Adds a tag (value) to the specified project given by the projectId
        /// </summary>
        /// <param name="string value"></param>
        /// <param name="int projectId"></param>
        /// <returns>JsonResult</returns>
        [AcceptVerbs("POST", "OPTIONS")]
        [AllowCrossSiteJson]
        public string AddProjectTag(string value, int projectId, string token = null)
        {
            if (Request.RequestType.Equals("OPTIONS", StringComparison.InvariantCultureIgnoreCase))  //This is a preflight request
            {
                return null;
            }
            else
            {
                try
                {
                    int userId;
                    if (token != null)
                    {
                        userId = authenticationEngine.authenticate(token);
                    }
                    else
                    {
                        return AddErrorHeader("An authentication token must be passed in", 2);
                    }

                    if (userId < 0)
                    {
                        return AddErrorHeader("User not authenticated, please log in!", 2);
                    }
                    User user = userManager.GetUser(userId);

                    Project project = projectManager.GetProject(projectId);
                    if (!projectManager.IsUserOwnerOfProject(projectId, user))
                    {
                        return AddErrorHeader("User not authorized to add a tag to this project", 3);
                    }
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
                    string result = tm.AddTag(value, type, projectId);
                    if (result != null)
                    {
                        if (result == "Tag already added.")
                        {
                            return AddErrorHeader("This tag already exists", 1);
                        }
                        else
                        {
                            activityManager.AddActivity(user.id, "Project Tag", "Added", project.id);
                            return AddSuccessHeader("Tag Added Successfully" , true);
                        }
                    }
                    else
                    {
                        return AddErrorHeader("Tag could not be added to Project", 1);
                    }
                }
                catch (Exception ex)
                {
                    logAccessor.CreateLog(DateTime.Now,"ProjectController - AddProjectTag", ex.StackTrace);
                    return AddErrorHeader("Something went wrong while adding this Project Tag", 1);
                }
            }
        }

        /// <summary>
        /// Deletes a tag (value) to the specified project given by the projectId
        /// </summary>
        /// <param name="string value"></param>
        /// <param name="int projectId"></param>
        /// <returns>JsonResult</returns>
        [AcceptVerbs("POST", "OPTIONS")]
        [AllowCrossSiteJson]
        public string DeleteProjectTag(string value = null, int projectId = -1, string token = null)
        {
            if (Request.RequestType.Equals("OPTIONS", StringComparison.InvariantCultureIgnoreCase))  //This is a preflight request
            {
                return null;
            }
            else
            {
                try
                {
                    int userId;
                    if (token != null)
                    {
                        userId = authenticationEngine.authenticate(token);
                    }
                    else
                    {
                        return AddErrorHeader("An authentication token must be passed in", 2);
                    }

                    if (userId < 0)
                    {
                        return AddErrorHeader("User not authenticated, please log in!", 2);
                    }
                    User user = userManager.GetUser(userId);

                    Project project = projectManager.GetProject(projectId);
                    if (!projectManager.IsUserOwnerOfProject(projectId, user))
                    {
                        return AddErrorHeader("User not authorized to delete this project", 3);
                    }
                    TagManager tm = new TagManager();
                    bool result = false;
                    sTag stag = tm.GetSTag(value);
                    if (stag != null)
                    {
                        result = tm.RemoveProjectLink(stag.id, projectId, "s");
                    }
                    else
                    {
                        fTag ftag = tm.GetFTag(value);
                        if (ftag != null)
                        {
                            result = tm.RemoveProjectLink(ftag.id, projectId, "f");
                        }
                        else
                        {
                            return AddErrorHeader("Tag does not exist in database", 1);
                        }
                    }
                    if (result == true)
                    {
                        return AddSuccessHeader("Tag Removed from Project: " + projectId , true);
                    }
                    else
                    {
                        return AddErrorHeader("Tag could not be removed from the Project", 1);
                    }
                }
                catch (Exception ex)
                {
                    logAccessor.CreateLog(DateTime.Now, "ProjectController - AddProjectTag", ex.StackTrace);
                    return AddErrorHeader("Something went wrong while removing this Project Tag", 1);
                }
            }
        }

        /// <summary>
        /// Deletes the specified project from the logged in User
        /// </summary>
        /// <param name="int projectId"></param>
        /// <returns>JsonResult</returns>
        [AcceptVerbs("POST", "OPTIONS")]
        [AllowCrossSiteJson]
        public string DeleteProject(int projectId = -1, string token = null)
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
                        return AddErrorHeader("User not authenticated, please log in!", 2);
                    }
                    User user = userManager.GetUser(userId);

                    Project project = projectManager.GetProject(projectId);
                    if (!projectManager.IsUserOwnerOfProject(projectId, user))
                    {
                        return AddErrorHeader("User not authorized to delete this project", 3);
                    }

                    if (project.name == "About")
                    {
                        return AddErrorHeader("Cannot Delete About Project", 1);
                    }

                    projectManager.DeleteProject(project);
                    projectManager.deleteProjectFromOrder(user, projectId);
                    userManager.UpdateUser(user);

                    //refresh the user object with the changes
                    user = userManager.GetUser(userId);

                    aa.CreateAnalytic("Delete Project", DateTime.Now, user.userName, "Number of elements: " + project.projectElements.Count());

                    return AddSuccessHeader("Successfully deleted project", true);
                }
                catch (Exception ex)
                {
                    logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), ex.ToString());
                    return AddErrorHeader("Something went wrong while deleting this project.", 1);
                }
            }
        }
        
        ///// <summary>
        ///// Updates the project cover picture of the specified project
        ///// </summary>
        ///// <param name="int projectId"></param>
        ///// <returns>JsonResult</returns>
        //[Authorize]
        //[HttpPost]
        //public JsonResult UpdateProjectPicture(int projectId)
        //{
        //    try
        //    {
        //        User user = userManager.GetUser(User.Identity.Name);
        //        Project project;

        //        if (!projectManager.IsUserOwnerOfProject(projectId, user))
        //        {
        //            return Json(new { Error = "Can't update picture at this time" });
        //        }

        //        if (Request != null)
        //        {
        //            if (Request.Files.Count == 0)
        //            {
        //                return Json(new { Error = "No files submitted to server" });
        //            }
        //            else if (Request.Files[0].ContentLength == 0)
        //            {
        //                return Json(new { Error = "No files submitted to server" });
        //            }

        //            foreach (string inputFileId in Request.Files)
        //            {
        //                HttpPostedFileBase file = Request.Files[inputFileId];
        //                if (file.ContentLength > 0)
        //                {
        //                    if (ValidationEngine.ValidatePicture(file) != ValidationEngine.Success)
        //                    {
        //                        return Json(new { Error = ValidationEngine.ValidatePicture(file) });
        //                    }

        //                    System.IO.Stream fs = file.InputStream;
                            

        //                    if (file.FileName.Contains(".jpeg") || file.FileName.Contains(".jpg") || file.FileName.Contains(".png") || file.FileName.Contains(".bmp") || file.FileName.Contains(".JPEG") || file.FileName.Contains(".JPG") || file.FileName.Contains(".PNG") || file.FileName.Contains(".BMP"))
        //                    {
        //                        project = projectManager.GetProject(projectId);

        //                        projectManager.AddQueuedProjectPicture(projectId, fs, file.FileName);

        //                        aa.CreateAnalytic("Add Media", DateTime.Now, user.userName, file.FileName);
        //                        return Json(new { ProjectId = projectId, PictureLocation = "http://vestnstorage.blob.core.windows.net/images/uploadSuccessful2.png" });
        //                    }
        //                    else
        //                    {
        //                        return Json(new { Error = "File type not accepted" });
        //                    }
        //                }
        //            }
        //        }
        //        else
        //        {
        //            return Json(new { Error = "Server did not receive file post" });
        //        }

        //        //refresh the user object with the changes
        //        user = userManager.GetUser(User.Identity.Name);

        //        return Json(new { UpdatedPartial = RenderPartialViewToString("_Projects_Owner", new ProfileModel(user)) });
        //    }
        //    catch (Exception ex)
        //    {
        //        logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), ex.ToString());
        //        return Json(new { Error = "Problem saving media to cloud storage" });
        //    }
        //}

        /// <summary>
        /// Adds an experience project element to the specified project (will always be added to the first project, i.e. the About Project)
        /// </summary>
        /// <param name="int projectId"></param>
        /// <returns>Json ActionResult</returns>
        [Authorize]
        [HttpPost]
        public string AddExperienceElement(int projectId)
        {
            try
            {
                int newProjectElementId = -1;
                User user = userManager.GetUser(User.Identity.Name);
                Project project;

                if (!projectManager.IsUserOwnerOfProject(projectId, user))
                {
                    //return Json(new { Error = "Can't add element" });
                    return AddErrorHeader("Can't add experience", 1);
                }

                ProjectElement_Experience experience = new ProjectElement_Experience
                {
                    jobTitle = "Job Title",
                    company = "New Company",
                    jobDescription = "Job Description goes here",
                    startDate = DateTime.Now,
                    endDate = DateTime.Now,
                    description = "Description of overall experience and what I learned goes here",
                    city = "City",
                    state = "State"
                };

                newProjectElementId = projectManager.AddExperienceElement(projectId, experience);
                aa.CreateAnalytic("Add Media", DateTime.Now, user.userName);

                //refresh the user object with the changes
                user = userManager.GetUser(User.Identity.Name);

                //return Json(new { UpdatedPartial = RenderPartialViewToString("_Projects_Owner", new ProfileModel(user)), ProjectElementId = newProjectElementId });
                return AddSuccessHeader("\"Expereince added\"");
            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), ex.ToString());
                //return Json(new { Error = "Error creating new experience" });
                return AddErrorHeader("Error adding experience", 1);
            }
        }

        /// <summary>
        /// Adds a picture project element to the specified project
        /// </summary>
        /// <param name="int projectId"></param>
        /// <returns>JsonResult</returns>
        [AcceptVerbs("POST", "OPTIONS")]
        public string AddPictureElement(int projectId, string token)
        {
            int userId = authenticationEngine.authenticate(token);
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
                    //int newProjectElementId = -1;
                    JsonModels.UploadReponse response = new JsonModels.UploadReponse();
                    User user = userManager.GetUser(userId);

                    if (!projectManager.IsUserOwnerOfProject(projectId, user))
                    {
                        //return Json(new { Error = "Can't add picture at this time" });
                        return AddErrorHeader("You are not authorized to add this picture", 3);
                    }

                    if (Request != null)
                    {
                        if (Request.Files.Count == 0)
                        {
                            //return Json(new { Error = "No files submitted to server" });
                            return AddErrorHeader("No files submitted to server", 1);
                        }
                        else if (Request.Files[0].ContentLength == 0)
                        {
                            //return Json(new { Error = "No files submitted to server" });
                            return AddErrorHeader("No files submitted to server", 1);
                        }

                        foreach (string inputFileId in Request.Files)
                        {
                            HttpPostedFileBase file = Request.Files[inputFileId];
                            if (file.ContentLength > 0)
                            {
                                if (ValidationEngine.ValidatePicture(file) != ValidationEngine.Success)
                                {
                                    //return Json(new { Error = ValidationEngine.ValidatePicture(file) });
                                    return AddErrorHeader(ValidationEngine.ValidatePicture(file), 1);
                                }

                                System.IO.Stream fs = file.InputStream;
                                if (file.FileName.Contains(".jpeg") || file.FileName.Contains(".jpg") || file.FileName.Contains(".png") || file.FileName.Contains(".bmp") || file.FileName.Contains(".JPEG") || file.FileName.Contains(".JPG") || file.FileName.Contains(".PNG") || file.FileName.Contains(".BMP"))
                                {
                                    if (inputFileId == "newPictureUpload")
                                    {

                                        response = projectManager.UploadPictureElement(projectId, fs, file.FileName);
                                        if (response == null)
                                        {
                                            //return Json(new { Error = "An error occured saving the docuement." });
                                            return AddErrorHeader("An error occured saving the docuement.", 1);
                                        }
                                        aa.CreateAnalytic("Add Media", DateTime.Now, user.userName, file.FileName);
                                    }
                                }
                                else
                                {
                                    //return Json(new { Error = "File type not accepted" });
                                    return AddErrorHeader("File type not accepted", 1);
                                }
                            }
                        }
                    }
                    else
                    {
                        //return Json(new { Error = "Server did not receive file post" });
                        return AddErrorHeader("Server did not receive file post", 1);
                    }

                    //refresh the user object with the changes
                    user = userManager.GetUser(userId);
                    string returnVal;
                    try
                    {
                        returnVal = Serialize(response);
                    }
                    catch (Exception exception)
                    {
                        return AddErrorHeader(exception.Message, 1);
                    }
                    return AddSuccessHeader(returnVal);
                    //return Json(new { UpdatedPartial = RenderPartialViewToString("_Projects_Owner", new ProfileModel(user)), ProjectElementId = newProjectElementId });
                }
                catch (Exception ex)
                {
                    logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), ex.ToString());
                    //return Json(new { Error = "Problem saving media to cloud storage" });
                    return AddErrorHeader("Problem saving media to cloud storage", 1);
                }
            }
        }

        /// <summary>
        /// Adds a document project element to the specified project
        /// </summary>
        /// <param name="int projectId"></param>
        /// <returns>JsonResult</returns>
        [Authorize]
        [HttpPost]
        public string AddDocumentElement(int projectId)
        {
            try
            {
                //int newProjectElementId = -1;
                JsonModels.UploadReponse response = new JsonModels.UploadReponse();
                User user = userManager.GetUser(User.Identity.Name);

                if (!projectManager.IsUserOwnerOfProject(projectId, user))
                {
                    //return Json(new { Error = "Can't add document at this time" });
                    return AddErrorHeader("Can't add document at this time", 1);
                }

                Project project;

                if (Request != null)
                {
                    if (Request.Files.Count == 0)
                    {
                        //return Json(new { Error = "No files submitted to server" });
                        return AddErrorHeader("No files submitted to server", 1);
                    }
                    else if (Request.Files[0].ContentLength == 0)
                    {
                        //return Json(new { Error = "No files submitted to server" });
                        return AddErrorHeader("No files submitted to server", 1);
                    }

                    foreach (string inputFileId in Request.Files)
                    {
                        HttpPostedFileBase file = Request.Files[inputFileId];
                        if (file.ContentLength > 0)
                        {
                            if (ValidationEngine.ValidateDocument(file) != ValidationEngine.Success)
                            {
                                //return Json(new { Error = ValidationEngine.ValidateDocument(file) });
                                return AddErrorHeader(ValidationEngine.ValidateDocument(file), 1);
                            }

                            System.IO.Stream fs = file.InputStream;

                            if (inputFileId == "newDocumentUpload")
                            {
                                response = projectManager.AddDocumentElement(projectId, null, fs, file.FileName, user.userName);

                                //check if this is development enviroment or LIVE
                                var account = CloudStorageAccount.Parse(RoleEnvironment.GetConfigurationSettingValue("BlobConnectionString"));

                                if (account.BlobEndpoint.IsLoopback)
                                {
                                    response.artifactURL = @"http://127.0.0.1:10000/devstoreaccount1/pdfs/" + response.artifactURL;
                                }
                                else
                                {
                                    response.artifactURL = RoleEnvironment.GetConfigurationSettingValue("storageAccountUrl").ToString()+"pdfs/" + response.artifactURL;//TODO change this when it goes live to vestnstorage
                                }
                                
                                
                                //--------------------------
                                
                                if (response == null)
                                {
                                    return AddErrorHeader("File type not accepted", 1);
                                }
                                aa.CreateAnalytic("Add Media", DateTime.Now, user.userName, file.FileName);
                            }
                        }
                    }
                }
                else
                {
                    //return Json(new { Error = "Server did not receive file post" });
                    return AddErrorHeader("Server did not receive file post", 1);
                }

                //refresh the user object with the changes
                user = userManager.GetUser(User.Identity.Name);
                string returnVal;
                try
                {
                    returnVal = Serialize(response);
                }
                catch (Exception exception)
                {
                    return AddErrorHeader(exception.Message, 1);
                }
                return AddSuccessHeader(returnVal);
                //return Json(new { UpdatedPartial = RenderPartialViewToString("_Projects_Owner", new ProfileModel(user)), ProjectElementId = newProjectElementId });
            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), ex.ToString());
                //return Json(new { Error = "Problem saving media to cloud storage" });
                return AddErrorHeader("Problem saving media to cloud storage", 1);
            }
        }

        /// <summary>
        /// Adds a picture project element to the specified project
        /// </summary>
        /// <param name="int projectId"></param>
        /// <returns>JsonResult</returns>
        [AcceptVerbs("POST", "OPTIONS")]
        [AllowCrossSiteJson]
        public string AddArtifact_Media(int projectId, string token, string qqfile=null)
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
                        return AddErrorHeader("You are not authenticated, please log in!", 2);
                    }
                    //int newProjectElementId = -1;
                    JsonModels.UploadReponse response = new JsonModels.UploadReponse();
                    User user = userManager.GetUser(userId);
                    string artifactType = "null";
                    if (!projectManager.IsUserOwnerOfProject(projectId, user))
                    {
                        //return Json(new { Error = "Can't add picture at this time" });
                        return AddErrorHeader("You are not authorized to add this picture", 3);
                    }
                    if (Request != null)
                    {
                        if (qqfile == null && Request.Files.Count == 0)
                        {
                            return AddErrorHeader("No files submitted to server", 1);
                        }

                        var length = Request.ContentLength;
                        var bytes = new byte[length];
                        Stream s;
                        if (qqfile == "System.Web.HttpPostedFileWrapper")
                        {
                            qqfile = Request.Files[0].FileName;
                            s = Request.Files[0].InputStream;
                        }
                        else
                        {
                            Request.InputStream.Read(bytes, 0, length);
                            s = new MemoryStream(bytes);
                        }
                        //logAccessor.CreateLog(DateTime.Now, "Upload Artifact Media - 700", "request.files[0].filename:"+Request.Files[0].FileName);
                        //logAccessor.CreateLog(DateTime.Now, "Upload Artifact Media - 701", "request.files[0].contenttype:" + Request.Files[0].ContentType);

                        if (qqfile.Contains(".jpeg") || qqfile.Contains(".jpg") || qqfile.Contains(".png") || qqfile.Contains(".bmp") || qqfile.Contains(".JPEG") || qqfile.Contains(".JPG") || qqfile.Contains(".PNG") || qqfile.Contains(".BMP"))
                        {
                            response = projectManager.UploadPictureElement(projectId, s, qqfile);
                            if (response == null)
                            {
                                return AddErrorHeader("An error occured saving the docuement.", 1);
                            }
                            aa.CreateAnalytic("Add_Media", DateTime.Now, user.userName, "Picture");
                            artifactType = "picture";
                        }
                        else if (qqfile.Contains(".PDF") || qqfile.Contains(".pdf") || qqfile.Contains(".doc") || qqfile.Contains(".docx") || qqfile.Contains(".ppt") || qqfile.Contains(".pptx") || qqfile.Contains(".xls") || qqfile.Contains(".xlsx") || qqfile.Contains(".txt") || qqfile.Contains(".rtf") || qqfile.Contains(".DOC") || qqfile.Contains(".DOCX") || qqfile.Contains(".PPT") || qqfile.Contains(".PPTX") || qqfile.Contains(".XLS") || qqfile.Contains(".XLSX") || qqfile.Contains(".TXT") || qqfile.Contains(".RTF"))
                        {
                            response = projectManager.AddDocumentElement(projectId, null, s, qqfile, user.userName);

                            //check if this is development enviroment or LIVE
                            var account = CloudStorageAccount.Parse(RoleEnvironment.GetConfigurationSettingValue("BlobConnectionString"));

                            if (response == null)
                            {
                                return AddErrorHeader("File type not accepted", 1);
                            }
                            aa.CreateAnalytic("Add_Media", DateTime.Now, user.userName, "Document");
                            artifactType = "document";
                        }
                        else
                        {
                            return AddErrorHeader("You did not upload an accepted picture or document type: (jpeg, jpg, png, bmp, doc, docx, ppt, pptx, xls, xlsx, txt, rtf", 1);
                        }
                    }
                    else
                    {
                        return AddErrorHeader("Server did not receive file post", 1);
                    }
                    //refresh the user object with the changes
                    user = userManager.GetUser(userId);
                    //build the artifact response

                    JsonModels.Artifact artifactResponse = new JsonModels.Artifact();
                    artifactResponse.id = response.id;
                    if(artifactType == "picture")
                    {
                        artifactResponse.artifactLocation = RoleEnvironment.GetConfigurationSettingValue("storageAccountUrl").ToString()+"thumbnails/" + response.artifactURL;
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

                    string realReturnVal;
                    try
                    {
                        realReturnVal = Serialize(artifactResponse);
                    }
                    catch (Exception exception)
                    {
                        return AddErrorHeader(exception.Message, 1);
                    }
                    activityManager.AddActivity(user.id, "Artifact", "Added", artifactResponse.id);
                    return AddSuccessHeader(realReturnVal);
                }
                catch (Exception ex)
                {
                    logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), ex.ToString());
                    return AddErrorHeader("Problem saving media to cloud storage", 1);
                }
            }
        }

        /// <summary>
        /// Adds a video project element to the specified project
        /// </summary>
        /// <param name="string ProjectID"></param>
        /// <param name="string status"></param>
        /// <param name="string id"></param>
        /// <returns></returns>
        [Authorize]
        public string AddVideoElement(string ProjectID, string status, string id)
        {
            try
            {
                User user = userManager.GetUser(User.Identity.Name);

                if (!projectManager.IsUserOwnerOfProject(Int32.Parse(ProjectID), user))
                {
                    //return Json(new { Error = "Can't add video at this time" });
                    return AddErrorHeader("Can't add video at this time", 1);
                }

                int nProjectID = Convert.ToInt32(ProjectID);

                int nStatus = Convert.ToInt32(status);

                string videoID = id;
                ViewBag.VideoID = id;

                JsonModels.Artifact response = projectManager.AddVideoElement(nProjectID, "Description goes here", videoID, "unknown");
                AnalyticsAccessor aa = new AnalyticsAccessor();
                aa.CreateAnalytic("Add Media", DateTime.Now, user.userName, "Video file");

                string returnVal;
                try
                {
                    returnVal = Serialize(response);
                }
                catch (Exception exception)
                {
                    return AddErrorHeader(exception.Message, 1);
                }
                return AddSuccessHeader(returnVal);
            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), ex.ToString());
                return AddErrorHeader("Error occured uploading your video", 1);
            }
        }

        /// <summary>
        /// Takes an existing youtube video link and adds a video project element
        /// </summary>
        /// <param name="int id"></param>
        /// <param name="string newVideoLinkUpload"></param>
        /// <returns></returns>
        [AcceptVerbs("POST","OPTIONS")]
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
                        return AddErrorHeader("User authentication token not recieved.", 2);
                    }
                    if (userId < 0)
                    {
                        return AddErrorHeader("User is not authenticated, please log in!", 2);
                    }
                    User user = userManager.GetUser(userId);
                    if (projectId < 0)
                    {
                        return AddErrorHeader("A projectId was not recieved", 1);
                    }
                    if (!projectManager.IsUserOwnerOfProject(projectId, user))
                    {
                        return AddErrorHeader("User is not authorized to complete this action", 3);
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
                        return AddErrorHeader("A videoLink was not recieved", 1);
                    }
                    
                    JsonModels.Artifact response = projectManager.AddVideoElement(projectId, null, videoLink, vType);

                    aa.CreateAnalytic("Add_Media", DateTime.Now, user.userName, "Video");
                    string returnVal;
                    try
                    {
                        returnVal = Serialize(response);
                    }
                    catch (Exception exception)
                    {
                        return AddErrorHeader(exception.Message, 1);
                    }
                    activityManager.AddActivity(user.id, "Artifact", "Added", response.id);
                    return AddSuccessHeader(returnVal);
                }
                catch (Exception ex)
                {
                    logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), ex.ToString());
                    return AddErrorHeader("Error occured uploading your video", 1);
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
                        return AddErrorHeader("A token must be passed in", 2);
                    }
                    if (userId < 0)
                    {
                        return AddErrorHeader("User is not authenticated, please log in!", 2);
                    }
                    User user = userManager.GetUser(userId);
                    if (projectId > 0)
                    {
                        if (!projectManager.IsUserOwnerOfProject(projectId, user))
                        {
                            return AddErrorHeader("User is not authorized to complete this action", 3);
                        }
                    }
                    else
                    {
                        return AddErrorHeader("A projectId must be passed in", 1);
                    }
                    JsonModels.Artifact response = new JsonModels.Artifact();
                    if (code != null && type != null)
                    {
                        response = projectManager.AddCodeElement(projectId, code, type);
                        aa.CreateAnalytic("Add_Media", DateTime.Now, user.userName, "Code Sample");
                    }
                    else
                    {
                        return AddErrorHeader("code and type must be passed in", 1);
                    }
                    string returnVal;
                    try
                    {
                        returnVal = Serialize(response);
                    }
                    catch (Exception exception)
                    {
                        return AddErrorHeader(exception.Message, 1);
                    }
                    activityManager.AddActivity(user.id, "Artifact", "Added", response.id); 

                    return AddSuccessHeader(returnVal);
                }
                catch (Exception ex)
                {
                    logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), ex.ToString());
                    return AddErrorHeader("Error occured uploading your video", 1);
                }
            }
        }

        /// <summary>
        /// Adds an audio project element to the specified project
        /// </summary>
        /// <param name="int projectId"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        public string AddAudioElement(int projectId)
        {
            try
            {
                JsonModels.UploadReponse response = new JsonModels.UploadReponse();
                User user = userManager.GetUser(User.Identity.Name);

                if (!projectManager.IsUserOwnerOfProject(projectId, user))
                {
                    return AddErrorHeader("Can't add audio at this time", 1);
                    //return Json(new { Error = "Can't add audio at this time" });
                }

                Project project;

                if (Request != null)
                {
                    if (Request.Files.Count == 0)
                    {
                        return AddErrorHeader("No files submitted to server", 1);
                        //return Json(new { Error = "No files submitted to server" });
                    }
                    else if (Request.Files[0].ContentLength == 0)
                    {
                        return AddErrorHeader("No files submitted to server", 1);
                        //return Json(new { Error = "No files submitted to server" });
                    }

                    foreach (string inputFileId in Request.Files)
                    {
                        HttpPostedFileBase file = Request.Files[inputFileId];
                        if (file.ContentLength > 0)
                        {
                            if (ValidationEngine.ValidateAudio(file) != ValidationEngine.Success)
                            {
                                return AddErrorHeader(ValidationEngine.ValidateAudio(file), 1);
                                //return Json(new { Error = ValidationEngine.ValidateAudio(file) });
                            }

                            System.IO.Stream fs = file.InputStream;

                            if (inputFileId == "newAudioUpload")
                            {
                                response = projectManager.AddAudioElement(projectId, null, fs, file.FileName);
                                if (response == null)
                                {
                                    return AddErrorHeader("Invalid Project Element ID", 1);
                                    //return Json(new { Error = "Invalid Project Element ID" });
                                }
                                aa.CreateAnalytic("Add Media", DateTime.Now, user.userName, file.FileName);
                            }
                        }
                    }
                }
                else
                {
                    return AddErrorHeader("Server did not receive file post", 1);
                    //return Json(new { Error = "Server did not receive file post" });
                }

                //refresh the user object with the changes
                user = userManager.GetUser(User.Identity.Name);
                string returnVal;
                try
                {
                    returnVal = Serialize(response);
                }
                catch (Exception exception)
                {
                    return AddErrorHeader(exception.Message, 1);
                }
                return AddSuccessHeader(returnVal);
                //return Json(new { UpdatedPartial = RenderPartialViewToString("_Projects_Owner", new ProfileModel(user)), ProjectElementId = newProjectElementId });
            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), ex.ToString());
                //return Json(new { Error = "Problem saving media to cloud storage" });
                return AddErrorHeader("Problem saving media to cloud storage", 1);
            }
        }

        /// <summary>
        /// Updates the passed projectModel as to update the view
        /// </summary>
        /// <param name="ProjectModel projectModel"></param>
        /// <returns></returns>
        //[Authorize]
        //[HttpPost]
        //public JsonResult UpdateProject(ProjectModel projectModel)
        //{
        //    try
        //    {
        //        Project project = projectModel.toProject();

        //        if (ValidationEngine.ValidateTitle(project.name) != ValidationEngine.Success)
        //        {
        //            return Json(new { Error = ValidationEngine.ValidateTitle(project.name) });
        //        }

        //        projectManager.UpdateProject(project);
        //        User user = userManager.GetUser(User.Identity.Name);

        //        ViewData = projectIdDataDictionary(user);

        //        //refresh the user object with the changes
        //        user = userManager.GetUser(User.Identity.Name);

        //        return Json(new { UpdatedPartial = RenderPartialViewToString("_Projects_Owner", new ProfileModel(user)) });
        //    }
        //    catch (Exception ex)
        //    {
        //        logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), ex.ToString());
        //        return Json(new { Error = "Unexpected Error" });
        //    }
        //}

       
        // this only encompasses editing a project element
        /// <summary>
        /// encomapsses editing a project element
        /// </summary>
        /// <param name="int projectElementId"></param>
        /// <param name="string id"></param>
        /// <param name="string value"></param>
        /// <returns></returns>
        [AcceptVerbs("POST","OPTIONS")]
        [AllowCrossSiteJson]
        public string UpdateArtifact(int artifactId, string propertyId, string propertyValue, string token)
        {
            if (Request.RequestType.Equals("OPTIONS", StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }
            try
            {
                int userId = authenticationEngine.authenticate(token);
                if (userId < 0)
                {
                    return AddErrorHeader("You are not authenticated, please log in!", 2);
                }
                User user = userManager.GetUser(userId);
                Project p = projectManager.IsUserOwnerOfProjectElement(artifactId, user);
                if (p == null)
                {
                    return AddErrorHeader("User not authorized to edit this artifact", 3);
                }
                if (propertyValue != null)
                {
                    //strip value of \n characters and replace with <br />
                    propertyValue = StripNewLineAndReplaceWithLineBreaks(propertyValue);
                }
                else
                {
                    return AddErrorHeader("You must pass in a propertyValue to set", 1);
                }

                ProjectElement projectElement = projectManager.GetProjectElement(artifactId);
                string elementType = projectElement.GetType().ToString().Substring(projectElement.GetType().ToString().LastIndexOf('.') + 1);

                if (projectElement == null)
                {
                    return AddErrorHeader("Artifact not found", 1);
                }

                System.Reflection.PropertyInfo pi = projectElement.GetType().GetProperty(propertyId);

                if (pi == null)
                {
                    return AddErrorHeader("Invalid propertyId", 1);
                }
                else
                {
                    try
                    {
                        //save changes in local model
                        pi.SetValue(projectElement, Convert.ChangeType(propertyValue, pi.PropertyType), null);
                        
                        if (propertyId == "title")
                        {
                                if (ValidationEngine.ValidateTitle(projectElement.title) != ValidationEngine.Success)
                                {
                                    return AddErrorHeader("Title exceeded 100 character limit, artifact not updated", 1);
                                }
                        }

                        if (propertyId == "name")
                        {
                            if (ValidationEngine.ValidateTitle(propertyValue) != ValidationEngine.Success)
                            {
                                return AddErrorHeader("Name exceeded 100 character limit, artifact not updated", 1);
                            }
                        }

                        //TODO validate description

                        if (projectElement.GetType() == typeof(ProjectElement_Document))
                        {
                            ProjectElement_Document document = (ProjectElement_Document)projectElement;
                        }
                        else if (projectElement.GetType() == typeof(ProjectElement_Picture))
                        {
                            ProjectElement_Picture picture = (ProjectElement_Picture)projectElement;
                        }
                        else if (projectElement.GetType() == typeof(ProjectElement_Video))
                        {
                            ProjectElement_Video video = (ProjectElement_Video)projectElement;
                        }
                        //persist user model to DB with manager updateUser method
                        projectElement = projectManager.UpdateProjectElement(projectElement);
                        return AddSuccessHeader("Artifact with id:"+artifactId +" successfully updated",true);
                    }
                    catch (Exception exc)
                    {
                        logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), exc.ToString());
                        return AddErrorHeader("Something went wrong while updating this artifact.", 1);
                    }
                }
            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), ex.ToString());
                return AddErrorHeader("Something went wrong while updating this artifact.", 1);
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
                    return AddErrorHeader("An authentication token must be passed in", 2);
                }
                if (userId < 0)
                {
                    return AddErrorHeader("You are not authenticated, please log in!", 2);
                }
                User authUser = userManager.GetUser(userId);
                JsonModels.Artifact artifactFromJson = artifact.FirstOrDefault();
                ProjectElement originalElement = projectManager.GetProjectElement(artifactFromJson.id);
                if (originalElement == null)
                {
                    return AddErrorHeader("The artifact model does not exist in the database", 1);
                }
                else
                {
                    Project p = projectManager.IsUserOwnerOfProjectElement(artifactFromJson.id, authUser);
                    if (p != null)
                    {
                        if (artifactFromJson.description != null)
                        {
                            originalElement.description = (artifactFromJson.description == "null") ? null : artifactFromJson.description;
                        }
                        if (artifactFromJson.title != null)
                        {
                            originalElement.title = (artifactFromJson.title == "null") ? null : artifactFromJson.title;
                        }
                        projectManager.UpdateProjectElement(originalElement);

                        p.dateModified = DateTime.Now;
                        projectManager.UpdateProject(p);
                        activityManager.AddActivity(userId, "Artifact", "Updated", originalElement.id);
                        return AddSuccessHeader(Serialize(projectManager.GetArtifactJson(originalElement)));
                    }
                    else
                    {
                        return AddErrorHeader("User is not authorized to edit this Artifact", 3);
                    }
                }
            }
            catch (Exception ex)
            {
                return AddErrorHeader("Something went wrong while updating this artifact", 1);
            }
        }



        [AcceptVerbs("POST","OPTIONS")]
        [AllowCrossSiteJson]
        public string UpdateProjectModel(IEnumerable<JsonModels.CompleteProject> project, string token = null)
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
                    return AddErrorHeader("An authentication token must be passed in", 2);
                }
                if (userId < 0)
                {
                    return AddErrorHeader("You are not authenticated, please log in!", 2);
                }
                User authUser = userManager.GetUser(userId);
                JsonModels.CompleteProject projectFromJson = project.FirstOrDefault();
                Project originalProject = projectManager.GetProject(projectFromJson.id);
                if (projectFromJson == null)
                {
                    return AddErrorHeader("The project JSON model did not bind correctly", 1);
                }
                else
                {
                    if (originalProject == null)
                    {
                        return AddErrorHeader("The project model does not exist in the database", 1);
                    }
                    if (projectManager.IsUserOwnerOfProject(projectFromJson.id, authUser))
                    {
                        if (projectFromJson.description != null)
                        {
                            originalProject.description = (projectFromJson.description == "null") ? null : projectFromJson.description;
                        }
                        if (projectFromJson.title != null)
                        {
                            originalProject.name = (projectFromJson.title == "null") ? null : projectFromJson.title;
                        }
                        if (projectFromJson.artifactOrder != null)
                        {
                            originalProject.projectElementOrder = (projectFromJson.artifactOrder == "null") ? null : projectFromJson.artifactOrder;
                        }

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
                                originalProject.isActive = true;
                                originalProject.privacy = projectFromJson.privacy;
                            }
                        }

                        originalProject.dateModified = DateTime.Now;

                        projectManager.UpdateProject(originalProject);
                        activityManager.AddActivity(userId, "Project", "Updated", originalProject.id);
                        return AddSuccessHeader(Serialize(projectManager.GetProjectJson(originalProject)));
                    }
                    else
                    {
                        return AddErrorHeader("User is not authorized to edit this project", 3);
                    }
                }
            }
            catch (Exception ex)
            {
                return AddErrorHeader("Something went wrong while updating this Project.", 1);
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
                    return AddErrorHeader("An authentication token must be passed in", 2);
                }
                if (userId < 0)
                {
                    return AddErrorHeader("You are not authenticated, please log in!", 2);
                }
                User user = userManager.GetUser(userId);
                Project project;
                if (projectId > 0)
                {
                    project = projectManager.GetProject(projectId);
                }
                else
                {
                    return AddErrorHeader("Invalid projectId", 1);
                }
                if (project == null)
                {
                    return AddErrorHeader("Project not found", 1);
                }
                if (!projectManager.IsUserOwnerOfProject(projectId, user))
                {
                    return AddErrorHeader("User not authorized to update this project!", 3);
                }
                else
                {
                    if (qqfile != null || Request.Files.Count == 1)
                    {
                        var length = Request.ContentLength;
                        var bytes = new byte[length];
                        Stream s;
                        if (qqfile == "System.Web.HttpPostedFileWrapper")
                        {
                            qqfile = Request.Files[0].FileName;
                            s = Request.Files[0].InputStream;
                        }
                        else
                        {
                            Request.InputStream.Read(bytes, 0, length);
                            s = new MemoryStream(bytes);
                        }
                        JsonModels.UploadReponse response = new JsonModels.UploadReponse();
                        response = projectManager.UploadPictureElement(projectId, s, "coverPicture", true);
                        if (response == null)
                        {
                            return AddErrorHeader("An error occured saving the docuement.", 1);
                        }
                        else
                        {
                            activityManager.AddActivity(user.id, "Project Cover Picture", "Updated", projectId);
                            return AddSuccessHeader(RoleEnvironment.GetConfigurationSettingValue("storageAccountUrl").ToString()+"thumbnails/" + response.artifactURL, true);
                        }
                    }
                    else
                    {
                        return AddErrorHeader("No files were posted to the server", 1);
                    }
                }
            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, "ProjectController - UpdateCoverPicture", ex.StackTrace);
                return AddErrorHeader("Something went wrond while updating this project's cover picture", 1);
            }
        }

        //edit entire project, not just an element
        /// <summary>
        /// Edit an entire project, not just an element
        /// </summary>
        /// <param name="int projectId"></param>
        /// <param name="string id"></param>
        /// <param name="string value"></param>
        /// <returns></returns>
        [AcceptVerbs("POST","OPTIONS")]
        [AllowCrossSiteJson]
        public string UpdateProject(int projectId = -1, string propertyId = null, string propertyValue = null, string token = "notset", string qqfile = null)
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
                    return AddErrorHeader("An authentication token must be passed in", 2);
                }
                if (userId < 0)
                {
                    return AddErrorHeader("You are not authenticated, please log in!", 2);
                }
                User user = userManager.GetUser(userId);
                Project project;
                if (projectId > 0)
                {
                    project = projectManager.GetProject(projectId);
                }
                else
                {
                    return AddErrorHeader("Invalid projectId", 1);
                }
                if (!projectManager.IsUserOwnerOfProject(projectId, user))
                {
                    return AddErrorHeader("User not authorized to update this project!", 3);
                }

                if (project == null)
                {
                    return AddErrorHeader("Project not found", 1);
                }

                System.Reflection.PropertyInfo pi = null;
                if (propertyId != null)
                {
                    pi = project.GetType().GetProperty(propertyId);
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
                        if (qqfile != null || Request.Files.Count != 0)
                        {
                            if (propertyId == "coverPicture")
                            {
                                var length = Request.ContentLength;
                                var bytes = new byte[length];
                                Request.InputStream.Read(bytes, 0, length);
                                Stream s = new MemoryStream(bytes);
                                JsonModels.UploadReponse response = new JsonModels.UploadReponse();
                                response = projectManager.UploadPictureElement(projectId, s, "coverPicture", true);
                                if (response == null)
                                {
                                    return AddErrorHeader("An error occured saving the docuement.", 1);
                                }
                                else
                                {
                                    return AddSuccessHeader(RoleEnvironment.GetConfigurationSettingValue("storageAccountUrl").ToString()+"thumbnails/" + response.artifactURL, true);
                                }
                            }
                        }
                        if (propertyValue != null)
                        {
                            //strip value of \n characters and replace with <br />
                            propertyValue = StripNewLineAndReplaceWithLineBreaks(propertyValue);
                        }
                        else
                        {
                            return AddErrorHeader("propertyValue not set", 1);
                        }
                        if (propertyId == "title")
                        {
                            if (ValidationEngine.ValidateTitle(propertyValue) != ValidationEngine.Success)
                            {
                                return AddErrorHeader("Title exceeded 100 character limit, project not updated", 1);
                            }
                        }
                        if (propertyId == "name")
                        {
                            if (ValidationEngine.ValidateTitle(propertyValue) != ValidationEngine.Success)
                            {
                                return AddErrorHeader("Name exceeded 100 character limit, project not updated", 1);
                            }
                        }
                        if (propertyId == "privacy")
                        {
                            //TODO - ensure what is added to the DB is of the Privacy enumeration
                        }
                        //TODO validate description
                        if (propertyId != "coverPicture")
                        {
                            pi.SetValue(project, Convert.ChangeType(propertyValue, pi.PropertyType), null);
                            //persist user model to DB with manager updateUser method
                            project = projectManager.UpdateProject(project);
                        }
                        if (project != null)
                        {
                            return AddSuccessHeader("Project with id:" + projectId + " successfully updated", true);
                        }
                        else
                        {
                            return AddErrorHeader("Update Failed", 1);
                        }
                    }
                    catch (Exception exc)
                    {

                        logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), exc.ToString());
                        return AddErrorHeader("Something went wrong while updating this project", 1);
                    }
                }
            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), ex.ToString());
                return AddErrorHeader("Something went wrong while updating this project", 1);
            }
        }

        /// <summary>
        /// Deletes the specified project element
        /// </summary>
        /// <param name="int projectId"></param>
        /// <param name="int projectElementId"></param>
        /// <returns></returns>
        [AcceptVerbs("POST", "OPTIONS")]
        public JsonResult DeleteProjectElement(int projectId, int projectElementId)
        {

            User user = userManager.GetUser(User.Identity.Name);

            try
            {
                if (!projectManager.IsUserOwnerOfProject(projectId, user))
                {
                    return Json(new { Error = "Couldn't delete project element.<br /><br />Unexpected Error", UpdatedPartial = RenderPartialViewToString("_Projects_Owner", new ProfileModel(user)) });
                }

                Project p = projectManager.GetProject(projectId);
                ProjectElement e = projectManager.GetProjectElement(projectElementId);
                p.projectElements.RemoveAll(pr => pr.id == e.id);
                //projectManager.deleteProjectElementFromOrder(p, e.id);
                DeleteElementOrder(p, e);
                p = projectManager.UpdateProject(p);

                projectManager.DeleteProjectElement(e);
                //refresh the user object with the changes
                user = userManager.GetUser(User.Identity.Name);
                return Json(new { Success = "Success" });
            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), ex.ToString());
                return Json(new { Error = "Couldn't delete project element.<br /><br />Unexpected Error", UpdatedPartial = RenderPartialViewToString("_Projects_Owner", new ProfileModel(user)) });
            }
        }

        [AcceptVerbs("POST", "OPTIONS")]
        [AllowCrossSiteJson]
        public string DeleteArtifact(int projectId = -1, int artifactId = -1, string token = null)
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
                        return AddErrorHeader("You are not authenticated, please log in!", 2);
                    }
                    User user = userManager.GetUser(userId);
                    if (!projectManager.IsUserOwnerOfProject(projectId, user))
                    {
                        return AddErrorHeader("You are not authorized to delete this artifact, :(", 3);
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
                    return AddSuccessHeader("This artifact was successfully removed from the specified project", true);
                }
                catch (Exception ex)
                {
                    logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), ex.ToString());
                    return AddErrorHeader("Something went wrong while deleting this artifact, whoops!", 1);
                }
            }
        }

        /// <summary>
        /// deletes a project element from the order or project elements within a project
        /// </summary>
        /// <param name="Project p"></param>
        /// <param name="ProjectElement element"></param>
        private void DeleteElementOrder(Project p, ProjectElement element)
        {
            projectManager.deleteProjectElementFromOrder(p, element.id);
        }

        /// <summary>
        /// returns rhe Profile Model of a specified user
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        private List<ProjectModel> ReturnProjectsModelFromUser(User user)
        {
            return new ProfileModel(user).projects;
        }

        /// <summary>
        /// Returns all project ids in the form of a viewDataDictionary of the specified user
        /// </summary>
        /// <param name="User user"></param>
        /// <returns></returns>
        private ViewDataDictionary projectIdDataDictionary(User user)
        {
            ViewDataDictionary data = new ViewDataDictionary();
            for (int i = 0; i < user.projects.Count; i++)
            {
                ViewDataDictionary newDictionary = new ViewDataDictionary();
                newDictionary.Add("projectId", user.projects.ElementAt(i).id);
                data.Add(i.ToString(), newDictionary);
            }
            return data;
        }

        /// <summary>
        /// Moves the project to the previous spot in the order
        /// </summary>
        /// <param name="int projectId"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        public JsonResult MoveProjectPrevious(int projectId)
        {
            try
            {
                User u = userManager.GetUser(User.Identity.Name);
                int result = projectManager.moveProjectPrevious(u, projectId);
                if (result != -1)
                {
                    userManager.UpdateUser(u);
                    return Json(new { ReOrderedId = result });
                }
                else
                {
                    return Json(new { Error = "Couldn't re-order projects." });
                }
            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), ex.ToString());
                return Json(new { Error = "Couldn't re-order projects."});
            }
        }

        /// <summary>
        /// Moves the specified project next in the project order
        /// </summary>
        /// <param name="int projectId"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        public JsonResult MoveProjectNext(int projectId)
        {
            try
            {
                User u = userManager.GetUser(User.Identity.Name);
                int result = projectManager.moveProjectNext(u, projectId);
                if (result != -1)
                {
                    userManager.UpdateUser(u);
                    return Json(new { ReOrderedId = result });
                }
                else
                {
                    return Json(new { Error = "Couldn't re-order projects." });
                }
            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), ex.ToString());
                return Json(new { Error = "Couldn't re-order projects." });
            }
        }

        [AcceptVerbs("POST", "OPTIONS")]
        public string UpdateArtifactOrder(int projectId, string order, string token)
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
                    if (projectManager.IsUserOwnerOfProject(projectId, u))
                    {
                        Project p = projectManager.GetProject(projectId);
                        ReorderEngine re = new ReorderEngine();
                        List<int> ListOrder = re.stringOrderToList(order);
                        List<int> currentArtifactIds = new List<int>();
                        bool add = true;
                        foreach (ProjectElement pe in p.projectElements)
                        {
                            currentArtifactIds.Add(pe.id);
                        }
                        foreach (int i in ListOrder)
                        {
                            if (!currentArtifactIds.Contains(i))
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
                            p.projectElementOrder = order;
                            p = projectManager.UpdateProject(p);
                        }
                        return AddSuccessHeader("Order updated", true);
                    }
                    else
                    {
                        return AddErrorHeader("User not authorized to update Artifact order", 3);
                    }
                }
                catch(Exception ex)
                {
                    logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), ex.ToString());
                    return AddErrorHeader("Something went wrong while updating the Artifact Order", 1);
                }
            }
        }


        /// <summary>
        /// Moves the project element to the previous spot in the project element order
        /// </summary>
        /// <param name="int projectId"></param>
        /// <param name="int projectElementId"></param>
        /// <param name="string projectElementType"></param>
        /// <returns></returns>
        //[Authorize]
        //[HttpPost]
        //public JsonResult MoveProjectElementPrevious(int projectId, int projectElementId, string projectElementType)
        //{
        //    try
        //    {
        //        Project p = projectManager.GetProject(projectId);
        //        int result = projectManager.moveProjectElementPrevious(p, projectElementId, projectElementType);
        //        if (result != -1)
        //        {
        //            projectManager.UpdateProject(p);
        //            return Json(new { ReOrderedId = result });
        //        }
        //        else
        //        {
        //            return Json(new { Error = "Couldn't re-order documents." });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), ex.ToString());
        //        return Json(new { Error = "Couldn't re-order documents." });
        //    }
        //}

        ///// <summary>
        ///// Moves the specified project element to the next spot in the project element order
        ///// </summary>
        ///// <param name="int projectId"></param>
        ///// <param name="int projectElementId"></param>
        ///// <param name="string projectElementType"></param>
        ///// <returns></returns>
        //[Authorize]
        //[HttpPost]
        //public JsonResult MoveProjectElementNext(int projectId, int projectElementId, string projectElementType)
        //{
        //    try
        //    {
        //        Project p = projectManager.GetProject(projectId);
        //        int result = projectManager.moveProjectElementNext(p, projectElementId, projectElementType);
        //        if (result != -1)
        //        {
        //            projectManager.UpdateProject(p);
        //            return Json(new { ReOrderedId = result });
        //        }
        //        else
        //        {
        //            return Json(new { Error = "Couldn't re-order documents." });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), ex.ToString());
        //        return Json(new { Error = "Couldn't re-order documents." });
        //    }
        //}

        /// <summary>
        /// this end point takes in an array of ints that represent projectelementids and returns json representing each artifact
        /// an example call would be:
        /// /Project/GetArtifact?id=3&id=4&id=5&id=6
        /// </summary>
        /// <param name="int[] id"></param>
        /// <returns>A serialized list of Artifacts</returns>
        [HttpGet]
        [AllowCrossSiteJson]
        public string GetArtifact(int[] id)
        {
            string returnVal;
            try
            {
                List<JsonModels.Artifact> artifacts = projectManager.GetArtifacts(id);
                if (artifacts != null)
                {
                    try
                    {
                        returnVal = Serialize(artifacts);
                    }
                    catch (Exception exception)
                    {
                        return AddErrorHeader(exception.Message, 1);
                    }
                }
                else
                {
                    return AddErrorHeader("No Information Found", 1);
                }
            }
            catch (Exception e)
            {
                return AddErrorHeader("Bad Request", 1);
            }
            return AddSuccessHeader(returnVal);
        }

        /// <summary>
        /// this end point takes in an array of ints that represent projectIds and returns json representing each project
        /// an example call would be:
        /// /Project/GetProject?id=3&id=4&id=5&id=6
        /// </summary>
        /// <param name="int[] id"></param>
        /// <returns>A serialized list of Projects</returns>
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
                        //return AddErrorHeader("You are not authenticated, please log in!");
                    }
                }
                string returnVal;
                try
                {
                    List<JsonModels.CompleteProject> projects = projectManager.GetCompleteProjects(projectId, null);
                    if (projects != null)
                    {
                        try
                        {
                            returnVal = Serialize(projects);
                        }
                        catch (Exception exception)
                        {
                            return AddErrorHeader(exception.Message, 1);
                        }
                    }
                    else
                    {
                        return AddErrorHeader("No Information Found", 1);
                    }
                }
                catch (Exception e)
                {
                    return AddErrorHeader("Bad Request", 1);
                }
                return AddSuccessHeader(returnVal);
            }
        }

        [AcceptVerbs("OPTIONS", "POST")]
        [AllowCrossSiteJson]
        public string AddProp(int projectId, string message = null, string token = null)
        {
            if (Request.RequestType.Equals("OPTIONS", StringComparison.InvariantCultureIgnoreCase))  //This is a preflight request
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
                    return AddErrorHeader("An authentication token must be passed in", 2);
                }
                if (userId < 0)
                {
                    return AddErrorHeader("You are not authenticated, please log in!", 2);
                }
                User authUser = userManager.GetUser(userId);
                if(projectManager.IsUserOwnerOfProject(projectId, authUser))
                {
                    return AddErrorHeader("Users cannot give props on their own projects", 1);
                }
                //Project x = projectManager.GetProject(projectId);
            
                //activityManager.AddActivity(x.UserId, "Prop", "Received", projectId);

                JsonModels.Prop propJson = projectManager.AddProp(projectId, authUser.id, authUser.firstName, authUser.lastName, message);
                activityManager.AddActivity(userId, "Prop", "Gave", projectId);
                return AddSuccessHeader(Serialize(propJson));

            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, "ProjectController - AddProp", ex.ToString());
                return AddErrorHeader("Something went wrong while updating the Artifact Order", 1);
            }
        }

        [AcceptVerbs("OPTIONS", "POST")]
        [AllowCrossSiteJson]
        public string DeleteProp(int propId, string token)
        {
            if (Request.RequestType.Equals("OPTIONS", StringComparison.InvariantCultureIgnoreCase))  //This is a preflight request
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
                    return AddErrorHeader("An authentication token must be passed in", 2);
                }
                if (userId < 0)
                {
                    return AddErrorHeader("You are not authenticated, please log in!", 2);
                }
                User authUser = userManager.GetUser(userId);
                Prop prop = projectManager.GetProp(propId);
                if (authUser.id == prop.userId || projectManager.IsUserOwnerOfProject(prop.projectId, authUser))
                {
                    bool success = projectManager.DeleteProp(propId);
                    if (success)
                    {
                        return AddSuccessHeader("Prop successfully deleted");
                    }
                    else
                    {
                        return AddErrorHeader("The prop could not be deleted", 1);
                    }
                }
                else
                {
                    return AddErrorHeader("Your are not able to delete a prop unless you are the prop giver or receiver", 3);
                }
            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, "ProjectController - DeleteProp", ex.ToString());
                return AddErrorHeader("Something went wrong while updating the Artifact Order", 1);
            }
        }

        /// <summary>
        /// Searches a specific user's projects for a given query
        /// it looks at project name, description, documentText
        /// </summary>
        /// <param name="int id"></param>
        /// <param name="string query"></param>
        /// <returns>?????</returns>
        [HttpGet]
        [Authorize]
        [AllowCrossSiteJson]
        public string SearchProjects(int id, string query)
        {
            User u = userManager.GetUser(id);
            List<ProjectElement> elements = projectManager.SearchProjects(u, query.ToLower());
            List<JsonModels.Artifact> artifacts = projectManager.GetArtifacts(elements);
            if (artifacts.Count == 0)
            {
                return AddErrorHeader("No artifacts found.", 1);
            }
            string returnVal = "";
            try
            {
                returnVal = Serialize(artifacts);
            }
            catch (Exception exception)
            {
                return AddErrorHeader(exception.Message, 1);
            }
            return AddSuccessHeader(returnVal);
        }

        //[AcceptVerbs("POST", "OPTIONS")]
        //[AllowCrossSiteJson]
        //public string testThumbnail(int width, int height, string qqfile = null)
        //{
        //    if (Request.RequestType.Equals("OPTIONS", StringComparison.InvariantCultureIgnoreCase))  //This is a preflight request
        //    {
        //        return null;
        //    }
        //    var length = Request.ContentLength;
        //    var bytes = new byte[length];
        //    Request.InputStream.Read(bytes, 0, length);
        //    Stream s = new MemoryStream(bytes);

        //    ThumbnailEngine te = new ThumbnailEngine();
        //    BlobStorageAccessor BSAccessor = new BlobStorageAccessor();
        //    string FileNameThumb = Guid.NewGuid().ToString();
        //    string presetURL = string.Format("{0}{1}", FileNameThumb, ".jpeg");

        //    Bitmap image = te.CreateThumbnail(s, width, height);
        //    Uri uri = null;
        //    using (MemoryStream stream = new MemoryStream())
        //    {
        //        // Save image to stream.
        //        image.Save(stream, ImageFormat.Png);//changed this to make the background transparent
        //        uri = BSAccessor.uploadThumbnail(stream, false, presetURL);
        //    }
        //    return uri.ToString();


        //}
    }
}
