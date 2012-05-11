using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
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
        public string TestMe()
        {
            return "success";
        }
        
        /// <summary>
        /// Identifies the user and creates and adds a Project Entity bound to the User.
        /// </summary>
        /// <returns>JsonResult</returns>
        [Authorize]
        [HttpPost]
        public JsonResult CreateProject()
        {
            try
            {
                User user = userManager.GetUser(User.Identity.Name);
                Project project = projectManager.CreateProject(user, new List<ProjectElement>());

                ViewData = projectIdDataDictionary(user);

                //refresh the user object with the changes
                user = userManager.GetUser(User.Identity.Name);

                return Json(new { UpdatedPartial = RenderPartialViewToString("_Projects_Owner", new ProfileModel(user)), ProjectId = project.id });
            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), ex.ToString());
                return Json( new{ Error = "Unexpected Error" });
            }
        }

        /// <summary>
        /// Adds a tag (value) to the specified project given by the projectId
        /// </summary>
        /// <param name="string value"></param>
        /// <param name="int projectId"></param>
        /// <returns>JsonResult</returns>
        [Authorize]
        [HttpPost]
        public JsonResult AddProjectTag(string value, int projectId)
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
            string result = tm.AddTag("Project", value, type, projectId);
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

        /// <summary>
        /// Deletes the specified project from the logged in User
        /// </summary>
        /// <param name="int projectId"></param>
        /// <returns>JsonResult</returns>
        [Authorize]
        [HttpPost]
        public JsonResult DeleteProject(int projectId)
        {

            User user = userManager.GetUser(User.Identity.Name);

            try
            {
                Project project = projectManager.GetProject(projectId);
                if (!projectManager.IsUserOwnerOfProject(projectId, user))
                {
                    return Json(new { Error = "Couldn't delete element at this time", UpdatedPartial = RenderPartialViewToString("_Projects_Owner", new ProfileModel(user)) });
                }

                //don't delete if it is the About "project"
                if (project.name == "About")
                {
                    return Json(new { Error = "Couldn't delete project.<br /><br />Unexpected Error", UpdatedPartial = RenderPartialViewToString("_Projects_Owner", new ProfileModel(user)) });
                }

                projectManager.DeleteProject(project);
                projectManager.deleteProjectFromOrder(user, projectId);
                userManager.UpdateUser(user);

                ViewData = projectIdDataDictionary(user);

                //refresh the user object with the changes
                user = userManager.GetUser(User.Identity.Name);

                aa.CreateAnalytic("Delete Project", DateTime.Now, user.userName, "Number of elements: " + project.projectElements.Count());

                return Json(new { Success = "Success" });
            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), ex.ToString());
                return Json(new { Error = "Couldn't delete project.<br /><br />Unexpected Error", UpdatedPartial = RenderPartialViewToString("_Projects_Owner", new ProfileModel(user)) });
            }
        }
        
        /// <summary>
        /// Updates the project cover picture of the specified project
        /// </summary>
        /// <param name="int projectId"></param>
        /// <returns>JsonResult</returns>
        [Authorize]
        [HttpPost]
        public JsonResult UpdateProjectPicture(int projectId)
        {
            try
            {
                User user = userManager.GetUser(User.Identity.Name);
                Project project;

                if (!projectManager.IsUserOwnerOfProject(projectId, user))
                {
                    return Json(new { Error = "Can't update picture at this time" });
                }

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
                            if (ValidationEngine.ValidatePicture(file) != ValidationEngine.Success)
                            {
                                return Json(new { Error = ValidationEngine.ValidatePicture(file) });
                            }

                            System.IO.Stream fs = file.InputStream;
                            

                            if (file.FileName.Contains(".jpeg") || file.FileName.Contains(".jpg") || file.FileName.Contains(".png") || file.FileName.Contains(".bmp") || file.FileName.Contains(".JPEG") || file.FileName.Contains(".JPG") || file.FileName.Contains(".PNG") || file.FileName.Contains(".BMP"))
                            {
                                project = projectManager.GetProject(projectId);

                                projectManager.AddQueuedProjectPicture(projectId, fs, file.FileName);

                                aa.CreateAnalytic("Add Media", DateTime.Now, user.userName, file.FileName);
                                return Json(new { ProjectId = projectId, PictureLocation = "http://vestnstorage.blob.core.windows.net/images/uploadSuccessful2.png" });
                            }
                            else
                            {
                                return Json(new { Error = "File type not accepted" });
                            }
                        }
                    }
                }
                else
                {
                    return Json(new { Error = "Server did not receive file post" });
                }

                //refresh the user object with the changes
                user = userManager.GetUser(User.Identity.Name);

                return Json(new { UpdatedPartial = RenderPartialViewToString("_Projects_Owner", new ProfileModel(user)) });
            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), ex.ToString());
                return Json(new { Error = "Problem saving media to cloud storage" });
            }
        }

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
                    return GetFailureMessage("Can't add experience");
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
                return AddSuccessHeaders("\"Expereince added\"");
            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), ex.ToString());
                //return Json(new { Error = "Error creating new experience" });
                return GetFailureMessage("Error adding experience");
            }
        }

        /// <summary>
        /// Adds a picture project element to the specified project
        /// </summary>
        /// <param name="int projectId"></param>
        /// <returns>JsonResult</returns>
        [Authorize]
        [HttpPost]
        public string AddPictureElement(int projectId)
        {
            try
            {
                //int newProjectElementId = -1;
                JsonModels.UploadReponse response = new JsonModels.UploadReponse();
                User user = userManager.GetUser(User.Identity.Name);
                Project project;

                if (!projectManager.IsUserOwnerOfProject(projectId, user))
                {
                    //return Json(new { Error = "Can't add picture at this time" });
                    return GetFailureMessage("Can't add picture at this time");
                }

                if (Request != null)
                {
                    if (Request.Files.Count == 0)
                    {
                        //return Json(new { Error = "No files submitted to server" });
                        return GetFailureMessage("No files submitted to server");
                    }
                    else if (Request.Files[0].ContentLength == 0)
                    {
                        //return Json(new { Error = "No files submitted to server" });
                        return GetFailureMessage("No files submitted to server");
                    }

                    foreach (string inputFileId in Request.Files)
                    {
                        HttpPostedFileBase file = Request.Files[inputFileId];
                        if (file.ContentLength > 0)
                        {
                            if (ValidationEngine.ValidatePicture(file) != ValidationEngine.Success)
                            {
                                //return Json(new { Error = ValidationEngine.ValidatePicture(file) });
                                return GetFailureMessage(ValidationEngine.ValidatePicture(file));
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
                                        return GetFailureMessage("An error occured saving the docuement.");
                                    }
                                    aa.CreateAnalytic("Add Media", DateTime.Now, user.userName, file.FileName);
                                }
                            }
                            else
                            {
                                //return Json(new { Error = "File type not accepted" });
                                return GetFailureMessage("File type not accepted");
                            }
                        }
                    }
                }
                else
                {
                    //return Json(new { Error = "Server did not receive file post" });
                    return GetFailureMessage("Server did not receive file post");
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
                    return GetFailureMessage(exception.Message);
                }
                return AddSuccessHeaders(returnVal);
                //return Json(new { UpdatedPartial = RenderPartialViewToString("_Projects_Owner", new ProfileModel(user)), ProjectElementId = newProjectElementId });
            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), ex.ToString());
                //return Json(new { Error = "Problem saving media to cloud storage" });
                return GetFailureMessage("Problem saving media to cloud storage");
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
                    return GetFailureMessage("Can't add document at this time");
                }

                Project project;

                if (Request != null)
                {
                    if (Request.Files.Count == 0)
                    {
                        //return Json(new { Error = "No files submitted to server" });
                        return GetFailureMessage("No files submitted to server");
                    }
                    else if (Request.Files[0].ContentLength == 0)
                    {
                        //return Json(new { Error = "No files submitted to server" });
                        return GetFailureMessage("No files submitted to server");
                    }

                    foreach (string inputFileId in Request.Files)
                    {
                        HttpPostedFileBase file = Request.Files[inputFileId];
                        if (file.ContentLength > 0)
                        {
                            if (ValidationEngine.ValidateDocument(file) != ValidationEngine.Success)
                            {
                                //return Json(new { Error = ValidationEngine.ValidateDocument(file) });
                                return GetFailureMessage(ValidationEngine.ValidateDocument(file));
                            }

                            System.IO.Stream fs = file.InputStream;

                            if (inputFileId == "newDocumentUpload")
                            {
                                response = projectManager.AddDocumentElement(projectId, null, fs, file.FileName, user.userName);

                                //check if this is development enviroment or LIVE
                                var account = CloudStorageAccount.Parse(RoleEnvironment.GetConfigurationSettingValue("BlobConnectionString"));

                                if (account.BlobEndpoint.IsLoopback)
                                {
                                    response.pdfURL = @"http://127.0.0.1:10000/devstoreaccount1/pdfs/" + response.pdfURL;
                                }
                                else
                                {
                                    response.pdfURL = "https://vestnstaging.blob.core.windows.net/pdfs/" + response.pdfURL;//TODO change this when it goes live to vestnstorage
                                }
                                
                                
                                //--------------------------
                                
                                if (response == null)
                                {
                                    return GetFailureMessage("File type not accepted");
                                }
                                aa.CreateAnalytic("Add Media", DateTime.Now, user.userName, file.FileName);
                            }
                        }
                    }
                }
                else
                {
                    //return Json(new { Error = "Server did not receive file post" });
                    return GetFailureMessage("Server did not receive file post");
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
                    return GetFailureMessage(exception.Message);
                }
                return AddSuccessHeaders(returnVal);
                //return Json(new { UpdatedPartial = RenderPartialViewToString("_Projects_Owner", new ProfileModel(user)), ProjectElementId = newProjectElementId });
            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), ex.ToString());
                //return Json(new { Error = "Problem saving media to cloud storage" });
                return GetFailureMessage("Problem saving media to cloud storage");
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
                    return GetFailureMessage("Can't add video at this time");
                }

                int nProjectID = Convert.ToInt32(ProjectID);

                int nStatus = Convert.ToInt32(status);

                string videoID = id;
                ViewBag.VideoID = id;

                JsonModels.UploadReponse response = projectManager.AddVideoElement(nProjectID, "Description goes here", videoID);
                AnalyticsAccessor aa = new AnalyticsAccessor();
                aa.CreateAnalytic("Add Media", DateTime.Now, user.userName, "Video file");

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

        /// <summary>
        /// Takes an existing youtube video link and adds a video project element
        /// </summary>
        /// <param name="int id"></param>
        /// <param name="string newVideoLinkUpload"></param>
        /// <returns></returns>
        [Authorize]
        public string AddVideoElementByLink(int id, string newVideoLinkUpload)
        {
            try
            {
                User user = userManager.GetUser(User.Identity.Name);

                if (!projectManager.IsUserOwnerOfProject(id, user))
                {
                    //return Json(new { Error = "Can't add video at this time" });
                    return GetFailureMessage("Can't add video at this time");
                }

                string sVideoID = projectManager.ProcessYoutubeURL(newVideoLinkUpload);

                JsonModels.UploadReponse response = projectManager.AddVideoElement(id, "Description goes here", sVideoID);
         
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
                    return GetFailureMessage("Can't add audio at this time");
                    //return Json(new { Error = "Can't add audio at this time" });
                }

                Project project;

                if (Request != null)
                {
                    if (Request.Files.Count == 0)
                    {
                        return GetFailureMessage("No files submitted to server");
                        //return Json(new { Error = "No files submitted to server" });
                    }
                    else if (Request.Files[0].ContentLength == 0)
                    {
                        return GetFailureMessage("No files submitted to server");
                        //return Json(new { Error = "No files submitted to server" });
                    }

                    foreach (string inputFileId in Request.Files)
                    {
                        HttpPostedFileBase file = Request.Files[inputFileId];
                        if (file.ContentLength > 0)
                        {
                            if (ValidationEngine.ValidateAudio(file) != ValidationEngine.Success)
                            {
                                return GetFailureMessage(ValidationEngine.ValidateAudio(file));
                                //return Json(new { Error = ValidationEngine.ValidateAudio(file) });
                            }

                            System.IO.Stream fs = file.InputStream;

                            if (inputFileId == "newAudioUpload")
                            {
                                response = projectManager.AddAudioElement(projectId, null, fs, file.FileName);
                                if (response == null)
                                {
                                    return GetFailureMessage("Invalid Project Element ID");
                                    //return Json(new { Error = "Invalid Project Element ID" });
                                }
                                aa.CreateAnalytic("Add Media", DateTime.Now, user.userName, file.FileName);
                            }
                        }
                    }
                }
                else
                {
                    return GetFailureMessage("Server did not receive file post");
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
                    return GetFailureMessage(exception.Message);
                }
                return AddSuccessHeaders(returnVal);
                //return Json(new { UpdatedPartial = RenderPartialViewToString("_Projects_Owner", new ProfileModel(user)), ProjectElementId = newProjectElementId });
            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), ex.ToString());
                //return Json(new { Error = "Problem saving media to cloud storage" });
                return GetFailureMessage("Problem saving media to cloud storage");
            }
        }

        /// <summary>
        /// Updates the passed projectModel as to update the view
        /// </summary>
        /// <param name="ProjectModel projectModel"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        public JsonResult UpdateProject(ProjectModel projectModel)
        {
            try
            {
                Project project = projectModel.toProject();

                if (ValidationEngine.ValidateTitle(project.name) != ValidationEngine.Success)
                {
                    return Json(new { Error = ValidationEngine.ValidateTitle(project.name) });
                }

                projectManager.UpdateProject(project);
                User user = userManager.GetUser(User.Identity.Name);

                ViewData = projectIdDataDictionary(user);

                //refresh the user object with the changes
                user = userManager.GetUser(User.Identity.Name);

                return Json(new { UpdatedPartial = RenderPartialViewToString("_Projects_Owner", new ProfileModel(user)) });
            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), ex.ToString());
                return Json(new { Error = "Unexpected Error" });
            }
        }

       
        // this only encompasses editing a project element
        /// <summary>
        /// encomapsses editing a project element
        /// </summary>
        /// <param name="int projectElementId"></param>
        /// <param name="string id"></param>
        /// <param name="string value"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        public JsonResult EditProjectElement(int projectElementId, string id, string value)
        {
            try
            {
                User user = userManager.GetUser(User.Identity.Name);

                if (!projectManager.IsUserOwnerOfProjectElement(projectElementId, user))
                {
                    return Json(new { Error = "Can't edit information at this time" });
                }

                //strip value of \n characters and replace with <br />
                value = StripNewLineAndReplaceWithLineBreaks(value);

                ProjectElement projectElement = projectManager.GetProjectElement(projectElementId);
                string elementType = projectElement.GetType().ToString().Substring(projectElement.GetType().ToString().LastIndexOf('.') + 1);

                if (projectElement == null)
                {
                    return Json(new { UpdateStatus = "notUpdated", ProjectElementType = elementType, UpdateType = id });
                }

                System.Reflection.PropertyInfo pi = projectElement.GetType().GetProperty(id);

                string returnText = "";
                if (pi == null)
                {
                    return Json(new { UpdateStatus = "notUpdated", ProjectElementType = elementType, UpdateType = id });
                }
                else
                {
                    try
                    {
                        //save changes in local model
                        if (id == "startDate" || id == "endDate")
                        {
                            if (value.ToLower() == "current" || value.ToLower() == "now" || value.ToLower() == "present")
                            {
                                returnText = "Present";
                                value = "11/11/1811";
                            }
                        }
                        pi.SetValue(projectElement, Convert.ChangeType(value, pi.PropertyType), null);
                        

                        if (id == "title")
                        {
                                if (ValidationEngine.ValidateTitle(projectElement.title) != ValidationEngine.Success)
                                {
                                    return Json(new { UpdateStatus = "notUpdated_TooLong", ProjectElementType = elementType, UpdateType = "Title", UpdateValue = value });
                                }
                        }

                        if (id == "name")
                        {
                            if (ValidationEngine.ValidateTitle(value) != ValidationEngine.Success)
                            {
                                return Json(new { UpdateStatus = "notUpdated_TooLong", ProjectElementType = elementType, UpdateType = "Name", UpdateValue = value });
                            }
                        }

                        if (projectElement.GetType() == typeof(ProjectElement_Document))
                        {
                            ProjectElement_Document document = (ProjectElement_Document)projectElement;
                        }
                        else if (projectElement.GetType() == typeof(ProjectElement_Experience))
                        {
                            ProjectElement_Experience experience = (ProjectElement_Experience)projectElement;
                            //experience.company;
                            //experience.startDate;
                            //experience.endDate;
                            if (ValidationEngine.ValidateCity(experience.city) != ValidationEngine.Success)
                            {
                                return Json(new { UpdateStatus = "notUpdated_TooLong", ProjectElementType = elementType, UpdateType = "City", UpdateValue = value });
                            }

                            if (ValidationEngine.ValidateState(experience.state) != ValidationEngine.Success)
                            {
                                return Json(new { UpdateStatus = "notUpdated_TooLong", ProjectElementType = elementType, UpdateType = "State", UpdateValue = value });
                            }

                            if (ValidationEngine.ValidateCompany(experience.company) != ValidationEngine.Success)
                            {
                                return Json(new { UpdateStatus = "notUpdated_TooLong", ProjectElementType = elementType, UpdateType = "Company", UpdateValue = value });
                            }

                            if (ValidationEngine.ValidateDate(experience.startDate) != ValidationEngine.Success)
                            {
                                return Json(new { UpdateStatus = "notUpdated_DateError", ProjectElementType = elementType, UpdateType = "Start date", UpdateValue = value });
                            }

                            if (ValidationEngine.ValidateDate(experience.endDate) != ValidationEngine.Success)
                            {
                                return Json(new { UpdateStatus = "notUpdated_DateError", ProjectElementType = elementType, UpdateType = "End date", UpdateValue = value });
                            }

                            if (ValidationEngine.ValidateTitle(experience.jobTitle) != ValidationEngine.Success)
                            {
                                return Json(new { UpdateStatus = "notUpdated_TooLong", ProjectElementType = elementType, UpdateType = "Job title", UpdateValue = value });
                            }
                        }
                        else if (projectElement.GetType() == typeof(ProjectElement_Information))
                        {
                            ProjectElement_Information information = (ProjectElement_Information)projectElement;
                            //shouldn't really get here i don't think
                            //this should use the User/EditProfile method
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
                        //ProjectModel model = new ProjectModel(projectElement);
                        if (returnText == "")
                        {
                            return Json(new { UpdateStatus = "updated", ProjectElementType = elementType, UpdateValue = value, UpdateType = id });
                        }
                        else
                        {
                            return Json(new { UpdateStatus = "updated", ProjectElementType = elementType, UpdateValue = returnText, UpdateType = id });
                        }
                    }
                    catch (Exception)
                    {
                        return Json(new { UpdateStatus = "notUpdated", ProjectElementType = elementType, UpdateType = id });
                    }
                }
            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), ex.ToString());
                return Json(new { Error = "An unknown error occured" });
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
        [Authorize]
        [HttpPost]
        public JsonResult EditProject(int projectId, string id, string value)
        {
            try
            {
                User user = userManager.GetUser(User.Identity.Name);

                if (!projectManager.IsUserOwnerOfProject(projectId, user))
                {
                    return Json(new { Error = "Can't edit project at this time" });
                }

                //strip value of \n characters and replace with <br />
                value = StripNewLineAndReplaceWithLineBreaks(value);

                Project project = projectManager.GetProject(projectId);

                if (project == null)
                {
                    return Json(new { UpdateStatus = "notUpdated", UpdateType = id });
                }

                System.Reflection.PropertyInfo pi = project.GetType().GetProperty(id);

                if (pi == null)
                {
                    return Json(new { UpdateStatus = "notUpdated", UpdateType = id });
                }
                else
                {
                    try
                    {
                        //save changes in local model
                        pi.SetValue(project, Convert.ChangeType(value, pi.PropertyType), null);

                        if (id == "title")
                        {
                            if (ValidationEngine.ValidateTitle(value) != ValidationEngine.Success)
                            {
                                return Json(new { UpdateStatus = "notUpdated_TooLong", UpdateType = "Title", UpdateValue = value });
                            }
                        }

                        if (id == "name")
                        {
                            if (ValidationEngine.ValidateTitle(value) != ValidationEngine.Success)
                            {
                                return Json(new { UpdateStatus = "notUpdated_TooLong", UpdateType = "Name", UpdateValue = value });
                            }
                        }
                        //persist user model to DB with manager updateUser method
                        project = projectManager.UpdateProject(project);
                        //ProjectModel model = new ProjectModel(projectElement);
                        if (project != null)
                        {
                            return Json(new { UpdateStatus = "updated", UpdateValue = value, UpdateType = id });
                        }
                        else
                        {
                            return Json(new { UpdateStatus = "notUpdated", UpdateType = id });
                        }
                    }
                    catch (Exception)
                    {
                        return Json(new { UpdateStatus = "notUpdated", UpdateType = id });
                    }
                }
            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), ex.ToString());
                return Json(new { Error = "An unknown error occured" });
            }
        }

        /// <summary>
        /// Deletes the specified project element
        /// </summary>
        /// <param name="int projectId"></param>
        /// <param name="int projectElementId"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
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

        [AllowCrossSiteJson]
        [Authorize]
        [HttpGet]
        public string UpdateArtifactOrder(int id, string order)
        {
            User u = userManager.GetUser(User.Identity.Name);
            if (projectManager.IsUserOwnerOfProject(id, u))
            {

                Project p = projectManager.GetProject(id);
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
                    return GetFailureMessage("Update Failed.");
                }
                else
                {
                    p.projectElementOrder = order;
                    p = projectManager.UpdateProject(p);
                }
                return AddSuccessHeaders("\"Order updated\"");
            }
            else
            {
                return GetFailureMessage("Nice try - please log in");
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

        /// <summary>
        /// this end point takes in an array of ints that represent projectIds and returns json representing each project
        /// an example call would be:
        /// /Project/GetProject?id=3&id=4&id=5&id=6
        /// </summary>
        /// <param name="int[] id"></param>
        /// <returns>A serialized list of Projects</returns>
        [HttpGet]
        [AllowCrossSiteJson]
        public string GetProject(int[] id)
        {
            string returnVal;
            try
            {
                List<JsonModels.CompleteProject> projects = projectManager.GetCompleteProjects(id);
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
                return GetFailureMessage("No artifacts found.");
            }
            string returnVal = "";
            try
            {
                returnVal = Serialize(artifacts);
            }
            catch (Exception exception)
            {
                return GetFailureMessage(exception.Message);
            }
            return AddSuccessHeaders(returnVal);
        }
    }
}
