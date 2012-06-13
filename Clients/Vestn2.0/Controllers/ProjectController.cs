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

        /// <summary>
        /// Adds a picture project element to the specified project
        /// </summary>
        /// <param name="int projectId"></param>
        /// <returns>JsonResult</returns>
        [AcceptVerbs("POST", "OPTIONS")]
        public string AddArtifact_Media(int projectId, string token)
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
                        return GetFailureMessage("You are not authorized to add this picture");
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
                                else if (file.FileName.Contains(".doc") || file.FileName.Contains(".docx") || file.FileName.Contains(".ppt") || file.FileName.Contains(".pptx") || file.FileName.Contains(".xls") || file.FileName.Contains(".xlsx") || file.FileName.Contains(".txt") || file.FileName.Contains(".rtf") || file.FileName.Contains(".DOC") || file.FileName.Contains(".DOCX") || file.FileName.Contains(".PPT") || file.FileName.Contains(".PPTX") || file.FileName.Contains(".XLS") || file.FileName.Contains(".XLSX") || file.FileName.Contains(".TXT") || file.FileName.Contains(".RTF"))
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
                                        response.artifactURL = "https://vestnstaging.blob.core.windows.net/pdfs/" + response.artifactURL;//TODO change this when it goes live to vestnstorage
                                    }


                                    //--------------------------

                                    if (response == null)
                                    {
                                        return GetFailureMessage("File type not accepted");
                                    }
                                    aa.CreateAnalytic("Add Media", DateTime.Now, user.userName, file.FileName);
                                }
                                else
                                {
                                    //return Json(new { Error = "File type not accepted" });
                                    return GetFailureMessage("You did not upload an accepted picture or document type: (jpeg, jpg, png, bmp, doc, docx, ppt, pptx, xls, xlsx, txt,rtf");
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
                    user = userManager.GetUser(userId);
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
        }



    }
}
