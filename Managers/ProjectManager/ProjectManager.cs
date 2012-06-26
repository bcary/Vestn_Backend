using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Accessor;
using Entity;
using System.IO;
using System.Net;
using Manager;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Diagnostics;
using Engine;
using System.Collections.Specialized;

namespace Manager
{
    public class ProjectManager
    {
        LogAccessor logAccessor = new LogAccessor();
        ProjectAccessor pa = new ProjectAccessor();
        private CloudQueueClient queueClient;
        private CloudQueue queue;
        CloudStorageAccount storageAccount;
        private const string messageQueueName = "uploadqueue";
        ReorderEngine reorderEngine = new ReorderEngine();
        

        public string TestMe()
        {
            return "success";
        }

        public Project CreateProject(User u, List<ProjectElement> projectElements)
        {
            if (u.projects != null)
            {
                int activeSum = 0;
                foreach(Project p in u.projects){
                    activeSum += Convert.ToInt32(p.isActive);
                }
            }
            Project project = pa.CreateProject(u, projectElements);
            AnalyticsAccessor aa = new AnalyticsAccessor();
            aa.CreateAnalytic("Project Created", DateTime.Now, u.userName);
            return project;
        }

        public int AddProjectElement(Project project, ProjectElement pe)
        {
            return pa.AddProjectElement(project, pe);
        }

        public Project UpdateProject(Project project)
        {
            return pa.UpdateProject(project);
        }

        public Project GetProject(int id)
        {
            Project p = pa.GetProject(id);
            if (p.isActive == true)
            {
                return p;
            }
            else
            {
                return null;
            }
        }

        public ProjectElement GetProjectElement(int id)
        {
            return pa.GetProjectElement(id);
        }

        public ProjectElement UpdateProjectElement(ProjectElement pe)
        {
            return pa.UpdateProjectElement(pe);
        }

        public Project DeleteProject(Project p)
        {
            //delete all user media?

            p.isActive = false;
            return pa.UpdateProject(p);
        }

        public ProjectElement DeleteProjectElement(ProjectElement pe)
        {
            return pa.DeleteProjectElement(pe);
            //add logic here to actually delete the pictures from blob storage -- need to ask brian how to do that jtaylor
        }

        public ProjectElement DeleteProjectElementById(int id)
        {
            return DeleteProjectElement(pa.GetProjectElement(id));
        }

        private string GetTitle(string fileName)
        {
            return fileName.Substring(0, fileName.IndexOf('.'));
        }

        public int AddExperienceElement(int projectId, ProjectElement_Experience experience)//might be obsolete
        {
            Project p = pa.GetProject(projectId);
            return pa.AddProjectElement(p, experience);

        }

        public JsonModels.Artifact AddVideoElement(int projectId, string description, string id)
        {
            Project p = pa.GetProject(projectId);
            ProjectElement_Video pe = new ProjectElement_Video
            {
                title = "New Video",
                description = description,
                videoId = id
            };
            int pID = pa.AddProjectElement(p, pe);
            return new JsonModels.Artifact { id = pID, artifactLocation = pe.videoId, title = pe.title, fileLocation = pe.videoId, type = "video", description = "This is a video artifact!" };
        }

        public JsonModels.UploadReponse UploadPictureElement(int projectId, Stream pictureStream, string fileName, bool isCoverPicture = false)
        {
            try
            {
                BlobStorageAccessor blobStorageAccessor = new BlobStorageAccessor();
                UploadManager uploadManager = new UploadManager();
                ProjectAccessor projectAccessor = new ProjectAccessor();

                //initiate queue message
                storageAccount = CloudStorageAccount.Parse(RoleEnvironment.GetConfigurationSettingValue("BlobConnectionString"));
                queueClient = storageAccount.CreateCloudQueueClient();
                queue = queueClient.GetQueueReference(messageQueueName);
                queue.CreateIfNotExist();

                string imageURI = blobStorageAccessor.uploadImage(pictureStream, false).ToString();
                Project p = pa.GetProject(projectId);
                if (isCoverPicture)
                {
                    string FileNameThumb1 = Guid.NewGuid().ToString();
                    string artifactURL1 = string.Format("{0}{1}", FileNameThumb1, ".jpeg");
                    CloudQueueMessage message3 = new CloudQueueMessage(String.Format("{0},{1},{2},{3},{4},{5},{6},{7}", imageURI, p.id, "thumbnail", "ProjectPicture", 635, 397, "", artifactURL1));
                    queue.AddMessage(message3);
                    p.coverPictureThumbnail = "https://vestnstaging.blob.core.windows.net/thumbnails/" + artifactURL1;
                    p.coverPicture = imageURI;
                    Project newP = projectAccessor.UpdateProject(p);
                    return new JsonModels.UploadReponse { id = p.id, fileURL = imageURI, name = fileName, galeriaURL = "noGalleryURL", artifactURL = artifactURL1, description = "default description" };
                }
                else
                {
                    string FileNameThumb = Guid.NewGuid().ToString();
                    string artifactURL = string.Format("{0}{1}", FileNameThumb, ".jpeg");
                    ProjectElement_Picture pe = new ProjectElement_Picture
                    {
                        title = GetTitle(fileName),
                        pictureLocation = imageURI,
                        pictureThumbnailLocation = "https://vestnstaging.blob.core.windows.net/thumbnails/" + artifactURL
                    };
                    int projectElementId = pa.AddProjectElement(p, pe);
                    if (projectElementId == -1)
                    {
                        logAccessor.CreateLog(DateTime.Now, "ProjectManager - UploadPictureElement", "problem saving project element - 165 ProjectManager");
                        return null;
                    }


                    //string FileNameGaleria = Guid.NewGuid().ToString();
                    //string galleryURL = string.Format("{0}{1}", FileNameGaleria, ".jpeg");

                    CloudQueueMessage message = new CloudQueueMessage(String.Format("{0},{1},{2},{3},{4},{5},{6},{7}", imageURI, projectElementId, "thumbnail", "PictureElement", 635, 397, "", artifactURL));
                    //CloudQueueMessage message2 = new CloudQueueMessage(String.Format("{0},{1},{2},{3},{4},{5},{6},{7}", imageURI, projectElementId, "thumbnail", "PictureElement_Galleria", 1000, 750, "", galleryURL));
                    queue.AddMessage(message);
                    //queue.AddMessage(message2);
                    return new JsonModels.UploadReponse { id = projectElementId, fileURL = imageURI, name = fileName, galeriaURL = "galleryURL", artifactURL = artifactURL, description = "default description" };
                }
            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, "ProjectManager - UploadPictureElement", ex.StackTrace);
                return null;
            }
        }

        public JsonModels.UploadReponse AddAudioElement(int projectId, string description, Stream fileStream, string fileName)
        {
            Project p = pa.GetProject(projectId);
            BlobStorageAccessor ba = new BlobStorageAccessor();
            string location = null;
            string[] s2 = fileName.Split('.');
            string extention = s2[s2.Count() - 1].ToLower();
            if (extention.ToLower() == "mp3")
            {
                location = ba.uploadMP3(fileStream, false).ToString();
            }
            else if (extention.ToLower() == "ogg")
            {
                location = ba.uploadOGG(fileStream, false).ToString();
            }
            else if (extention.ToLower() == "wav")
            {
                location = ba.uploadWAV(fileStream, false).ToString();
            }
            else if (extention.ToLower() == "m4a")
            {
                location = ba.uploadM4A(fileStream, false).ToString();
            }
            else
            {
                //upload unknown file type. do this or display message
                Uri result = ba.uploadUnknown(fileStream, false, extention);
                if (result == null)
                {
                    return null;
                }
                else
                {
                    location = result.ToString();
                }
            }
            ProjectElement_Audio pe = new ProjectElement_Audio
            {
                description = description,
                audioLocation = location,
                title = fileName,
            };
            int pID = pa.AddProjectElement(p, pe);
            return new JsonModels.UploadReponse { id = pID, fileURL = location, name = fileName };

        }

        public JsonModels.UploadReponse AddDocumentElement(int projectId, string description, Stream fileStream, string fileName, string userName)
        {
            Project p = pa.GetProject(projectId);
            BlobStorageAccessor ba = new BlobStorageAccessor();
            string location = null;
            string[] s2 = fileName.Split('.');
            string extention = s2[s2.Count() - 1].ToLower();

            UserAccessor ua = new UserAccessor();
            User u = ua.GetUser(userName);
            string fullName = u.firstName +" " +  u.lastName;

            storageAccount = CloudStorageAccount.Parse(RoleEnvironment.GetConfigurationSettingValue("BlobConnectionString"));
            queueClient = storageAccount.CreateCloudQueueClient();
            queue = queueClient.GetQueueReference(messageQueueName);
            queue.CreateIfNotExist();
            string documentText = "";

            //string PDFLocation = "notset";
            if (extention == "pdf")
            {
                location = ba.uploadPDF(fileStream, false).ToString();
                UploadManager um = new UploadManager();
                location = um.stampThatShit(location, fullName, null);
                documentText = um.ExtractText(location);
            }
            else if (extention == "doc" || extention == "docx")
            {
                location = ba.uploadDOC(fileStream, false, "."+extention).ToString();
            }
            else if (extention == "ppt" || extention == "pptx")
            {
                location = ba.uploadUnknown(fileStream, false, extention).ToString();
            }
            else if( extention == "xls" || extention == "xlsx")
            {
                location = ba.uploadUnknown(fileStream,false, extention).ToString();
            }
            else if (extention == "rtf")
            {
                location = ba.uploadUnknown(fileStream, false, extention).ToString();
            }
            else if (extention == "txt")
            {
                location = ba.uploadUnknown(fileStream, false, extention).ToString();
            }
            else
            {
                //upload unknown file type. do this or display message
                Uri result = ba.uploadUnknown(fileStream, false, extention);
                if (result == null)
                {
                    return null;
                }
                else
                {
                    location = result.ToString();
                }
            }
            String FileName = Guid.NewGuid().ToString();
            string uniqueBlobName = string.Format("{0}{1}", FileName, ".pdf");
            ProjectElement_Document pe = new ProjectElement_Document
            {
                description = description,
                documentLocation = location,
                title = fileName,
                documentText = documentText,
                documentThumbnailLocation = "https://vestnstaging.blob.core.windows.net/pdfs/" + uniqueBlobName
            };
            int projectElementId = pa.AddProjectElement(p, pe);
            if (extention == "doc" || extention == "docx")
            {
                CloudQueueMessage message = new CloudQueueMessage(String.Format("{0},{1},{2},{3},{4},{5},{6},{7}", location, projectElementId, "documentConversion", @"http://do.convertapi.com/Word2Pdf", 0, 0, fullName, uniqueBlobName));
                queue.AddMessage(message);
            }
            else if (extention == "ppt" || extention == "pptx")
            {
                CloudQueueMessage message = new CloudQueueMessage(String.Format("{0},{1},{2},{3},{4},{5},{6},{7}", location, projectElementId, "documentConversion", @"http://do.convertapi.com/PowerPoint2Pdf", 0, 0, "", uniqueBlobName));
                queue.AddMessage(message);
            }
            else if( extention == "xls" || extention == "xlsx")
            {
                CloudQueueMessage message = new CloudQueueMessage(String.Format("{0},{1},{2},{3},{4},{5},{6},{7}", location, projectElementId, "documentConversion", @"http://do.convertapi.com/Excel2Pdf", 0, 0, fullName, uniqueBlobName));
                queue.AddMessage(message);
            }
            else if (extention == "rtf")
            {
                CloudQueueMessage message = new CloudQueueMessage(String.Format("{0},{1},{2},{3},{4},{5},{6},{7}", location, projectElementId, "documentConversion", @"http://do.convertapi.com/RichText2Pdf", 0, 0, fullName, uniqueBlobName));
                queue.AddMessage(message);
            }
            else if (extention == "txt")
            {
                CloudQueueMessage message = new CloudQueueMessage(String.Format("{0},{1},{2},{3},{4},{5},{6},{7}", location, projectElementId, "documentConversion", @"http://do.convertapi.com/Text2Pdf", 0, 0, fullName, uniqueBlobName));
                queue.AddMessage(message);
            }
            if (extention == "pdf")
            {
                return new JsonModels.UploadReponse { id = projectElementId, fileURL = location, name = fileName };
            }
            return new JsonModels.UploadReponse { id = projectElementId, fileURL = location, artifactURL = uniqueBlobName, name = fileName };
        }

        public JsonModels.Artifact AddCodeElement(int projectId, string code, string type)
        {
            Project p = pa.GetProject(projectId);
            ProjectElement_Code pe = new ProjectElement_Code
            {
                code = code,
                type = "code",
                description = "New Artifact Description",
                fileLocation = type,
                title = "New Code Sample"
            };
            int projectElementId = pa.AddProjectElement(p, pe);
            return new JsonModels.Artifact { id = pe.id, artifactLocation = code, description = pe.description, title = pe.title, type = pe.type, fileLocation = type };
        }

        //This method is use by uploadVideo page when the user upload the video
        public string[] uploadVideoFile()
        {

            UploadManager uploadManager = new UploadManager();

            string[] UrlAndToken = new string[2];

            if (uploadManager.uploadVideoFile() != null)
            {
                UrlAndToken = uploadManager.uploadVideoFile();
                return UrlAndToken;
            }
            else
            {
                return null;

            }

        }

        //This method will process the youtube URL the user pass in
        //It will get the videoID from the URL, validate if the videoID is valid by sending a API request
        public string ProcessYoutubeURL(string sYoutubeURL)
        {
            try
            {
                string sVideoID = sYoutubeURL.Substring(16);

                string url = "http://gdata.youtube.com/feeds/api/videos/" + sVideoID;

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                int _statuscode;

                try
                {
                    _statuscode = Convert.ToInt32(((HttpWebResponse)request.GetResponse()).StatusCode);
                }
                catch (WebException ex)
                {
                    logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), ex.ToString());
                    _statuscode = 0;
                    //throw (ex);
                }

                if (_statuscode == 200)
                {
                    return sVideoID;
                }
                else
                {
                    return "false";
                }
            }
            catch (Exception e)
            {
                logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), e.ToString());
                return "false";
            }
        }

        public bool IsUserOwnerOfProject(int projectId, User user)
        {
            List<int> projectIds = new List<int>();
            foreach (Project p in user.projects)
            {
                projectIds.Add(p.id);
            }
            if (projectIds.Contains(projectId))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool IsUserOwnerOfProjectElement(int projectElementId, User user)
        {
            List<int> projectElementIds = new List<int>();
            foreach (Project p in user.projects)
            {
                foreach (ProjectElement pe in p.projectElements)
                {
                    if (pe != null)
                    {
                        projectElementIds.Add(pe.id);
                    }
                }
            }
            if (projectElementIds.Contains(projectElementId))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public int moveProjectPrevious(User u, int id)
        {
            string newOrder = SwapPrevious(reorderEngine.stringOrderToList(u.projectOrder), id);
            if (newOrder != null)
            {
                u.projectOrder = newOrder;
                return id;
            }
            else
            {
                return -1;
            }
        }

        public int moveProjectNext(User u, int id)
        {
            string newOrder = SwapNext(reorderEngine.stringOrderToList(u.projectOrder), id);
            if (newOrder != null)
            {
                u.projectOrder = newOrder;
                return id;
            }
            else
            {
                return -1;
            }
        }

        public int deleteProjectFromOrder(User u, int id)
        {
            List<int> orderList = reorderEngine.stringOrderToList(u.projectOrder);
            string newProjectOrder = null;
            orderList.Remove(id);
            foreach (int y in orderList)
            {
                newProjectOrder += y + " ";
            }
            newProjectOrder = newProjectOrder.TrimEnd().Replace(' ', ',');
            u.projectOrder = newProjectOrder;
            return id;
        }

        //public int moveProjectElementPrevious(Project p, int id, string type)
        //{
        //    string result;
        //    if (type == "pe-document-selector")
        //    {
        //        result = SwapPrevious(stringOrderToList(p.projectElementOrderDocument), id);
        //        if (result != null)
        //        {
        //            p.projectElementOrderDocument = result;
        //        }
        //        else
        //        {
        //            return -1;
        //        }
        //    }
        //    else if (type == "pe-picture-selector")
        //    {
        //        result = SwapPrevious(stringOrderToList(p.projectElementOrderPicture), id);
        //        if (result != null)
        //        {
        //            p.projectElementOrderPicture = result;
        //        }
        //        else
        //        {
        //            return -1;
        //        }
        //    }
        //    else if (type == "pe-experience-selector")
        //    {
        //        result = SwapPrevious(stringOrderToList(p.projectElementOrderExperience), id);
        //        if (result != null)
        //        {
        //            p.projectElementOrderExperience = result;
        //        }
        //        else
        //        {
        //            return -1;
        //        }
        //    }
        //    else if (type == "pe-video-selector")
        //    {
        //        result = SwapPrevious(stringOrderToList(p.projectElementOrderVideo), id);
        //        if (result != null)
        //        {
        //            p.projectElementOrderVideo = result;
        //        }
        //        else
        //        {
        //            return -1;
        //        }
        //    }
        //    else if (type == "pe-audio-selector")
        //    {
        //        result = SwapPrevious(stringOrderToList(p.projectElementOrderAudio), id);
        //        if (result != null)
        //        {
        //            p.projectElementOrderAudio = result;
        //        }
        //        else
        //        {
        //            return -1;
        //        }
        //    }
        //    return id;
        //}

        //public int moveProjectElementNext(Project p, int id, string type)
        //{
        //    string result;
        //    if (type == "pe-document-selector")
        //    {
        //        result = SwapNext(stringOrderToList(p.projectElementOrderDocument), id);
        //        if (result != null)
        //        {
        //            p.projectElementOrderDocument = result;
        //        }
        //        else
        //        {
        //            return -1;
        //        }
        //    }
        //    else if (type == "pe-picture-selector")
        //    {
        //        result = SwapNext(stringOrderToList(p.projectElementOrderPicture), id);
        //        if (result != null)
        //        {
        //            p.projectElementOrderPicture = result;
        //        }
        //        else
        //        {
        //            return -1;
        //        }
        //    }
        //    else if (type == "pe-experience-selector")
        //    {
        //        result = SwapNext(stringOrderToList(p.projectElementOrderExperience), id);
        //        if (result != null)
        //        {
        //            p.projectElementOrderExperience = result;
        //        }
        //        else
        //        {
        //            return -1;
        //        }
        //    }
        //    else if (type == "pe-video-selector")
        //    {
        //        result = SwapNext(stringOrderToList(p.projectElementOrderVideo), id);
        //        if (result != null)
        //        {
        //            p.projectElementOrderVideo = result;
        //        }
        //        else
        //        {
        //            return -1;
        //        }
        //    }
        //    else if (type == "pe-audio-selector")
        //    {
        //        result = SwapNext(stringOrderToList(p.projectElementOrderAudio), id);
        //        if (result != null)
        //        {
        //            p.projectElementOrderAudio = result;
        //        }
        //        else
        //        {
        //            return -1;
        //        }
        //    }
        //    return id;
        //}

        //public int deleteProjectElementFromOrder(Project p, int id, string type)
        //{
        //    //List<int> orderList = new List<int>();
        //    if (type == "Document")
        //    {
        //        p.projectElementOrderDocument = RemoveFromElementFromList(stringOrderToList(p.projectElementOrderDocument), id);
        //    }
        //    else if (type == "Picture")
        //    {
        //        p.projectElementOrderPicture = RemoveFromElementFromList(stringOrderToList(p.projectElementOrderPicture), id);
        //    }
        //    else if (type == "Experience")
        //    {
        //        p.projectElementOrderExperience = RemoveFromElementFromList(stringOrderToList(p.projectElementOrderExperience), id);
        //    }
        //    else if (type == "Video")
        //    {
        //        p.projectElementOrderVideo = RemoveFromElementFromList(stringOrderToList(p.projectElementOrderVideo), id);
        //    }
        //    else if (type == "Audio")
        //    {
        //        p.projectElementOrderAudio = RemoveFromElementFromList(stringOrderToList(p.projectElementOrderAudio), id);
        //    }
        //    return id;
        //}

        public int deleteProjectElementFromOrder(Project p, int id)
        {
            p.projectElementOrder = RemoveFromElementFromList(reorderEngine.stringOrderToList(p.projectElementOrder), id);
            return id;
        }

        private string RemoveFromElementFromList(List<int> orderList, int id)
        {
            string newProjectOrder = null;
            orderList.Remove(id);
            foreach (int y in orderList)
            {
                newProjectOrder += y + " ";
            }
            if (newProjectOrder != null)
            {
                newProjectOrder = newProjectOrder.TrimEnd().Replace(' ', ',');
            }
            return newProjectOrder;
        }

        private string SwapNext(List<int> orderList, int id)
        {
            int temp = -1;
            string newProjectOrder = "";
            try
            {
                for (int i = orderList.Count() - 1; i >= 0; i--)
                {
                    if (orderList[i] == id)
                    {
                        temp = orderList[i + 1];
                        orderList[i + 1] = orderList[i];
                        orderList[i] = temp;
                    }
                }
            }
            catch (Exception e)
            {
                return null;
            }
            foreach (int y in orderList)
            {
                newProjectOrder += y + " ";
            }
            newProjectOrder = newProjectOrder.TrimEnd().Replace(' ', ',');
            return newProjectOrder;
        }

        private string SwapPrevious(List<int> orderList, int id)
        {
            int temp = -1;
            string newProjectOrder = "";
            try
            {
                for (int i = 1; i < orderList.Count(); i++)
                {
                    if (orderList[i] == id)
                    {
                        temp = orderList[i - 1];
                        orderList[i - 1] = orderList[i];
                        orderList[i] = temp;
                    }
                }
            }
            catch (Exception e)
            {
                return null;
            }
            foreach (int y in orderList)
            {
                newProjectOrder += y + " ";
            }
            newProjectOrder = newProjectOrder.TrimEnd().Replace(' ', ',');
            return newProjectOrder;
        }

        public Project resetProjectElementOrder(Project p)
        {
            string newProjectOrder =null;
            foreach (ProjectElement pe in p.projectElements)
            {
                newProjectOrder += pe.id + " ";
            }
            if (newProjectOrder != null)
            {
                p.projectElementOrder = newProjectOrder.TrimEnd().Replace(' ', ',');
            }
            else
            {
                p.projectElementOrder = null;
            }
            return p;
        }

        public List<JsonModels.ProjectShell> GetProjectShells(int ID)
        {
            try
            {
                UserAccessor userAccessor = new UserAccessor();
                List<Project> projects = reorderEngine.ReOrderProjects(userAccessor.GetUser(ID)).projects;
                List<JsonModels.ProjectShell> shells = new List<JsonModels.ProjectShell>();
                foreach (Project p in projects)
                {
                    if (p.isActive == true && p.name != "About")
                    {
                        JsonModels.ProjectShell ps = new JsonModels.ProjectShell();
                        ps.id = p.id;
                        if (p.name != null)
                        {
                            ps.name = p.name;
                        }
                        if (p.description != null)
                        {
                            ps.description = p.description;
                        }
                        if (p.tagIds != "" && p.tagIds != null)
                        {
                            ps.projectTags = GetProjectTags(p.id);
                        }
                        List<JsonModels.ArtifactShell> artifactShells = new List<JsonModels.ArtifactShell>();
                        foreach (ProjectElement pe in p.projectElements)
                        {
                            if (pe == null)
                            {
                                continue;
                            }
                            JsonModels.ArtifactShell aShell = new JsonModels.ArtifactShell();
                            aShell.id = pe.id;
                            if (pe.title != null)
                            {
                                aShell.title = pe.title;
                            }
                            artifactShells.Add(aShell);
                        }
                        ps.artifacts = artifactShells;
                        shells.Add(ps);
                    }
                }
                return shells;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private List<JsonModels.ProjectTag> GetProjectTags(int p)//doesnt yet return only unique tags
        {
            TagAccessor ta = new TagAccessor();
            List<JsonModels.ProjectTag> projectTags = new List<JsonModels.ProjectTag>();
            List<Tag> tags = ta.GetProjectTags(p);
            foreach (Tag t in tags)
            {
                JsonModels.ProjectTag pt = new JsonModels.ProjectTag();
                if (t.value != null)
                {
                    pt.projectTagValue = t.value;
                    pt.projectTagId = t.id;
                }
                projectTags.Add(pt);
            }
            return projectTags;
        }

        public List<JsonModels.Artifact> GetArtifacts(int[] id)//returns artifacts for documents, pictures, videos, and audio
        {
            List<JsonModels.Artifact> artifacts = new List<JsonModels.Artifact>();
            ProjectAccessor pa = new ProjectAccessor();
            int add = 0;
            foreach (int i in id)
            {
                add = 0;
                JsonModels.Artifact a = new JsonModels.Artifact();
                ProjectElement pe = pa.GetProjectElement(i);
                if (pe != null)
                {
                    a.id = pe.id;
                    if (pe.GetType() == typeof(ProjectElement_Document))
                    {
                        a.type = "document";
                        ProjectElement_Document ped = (ProjectElement_Document)pe;
                        if (ped.documentThumbnailLocation != null)
                        {
                            a.artifactLocation = ped.documentThumbnailLocation;
                            
                        }
                        if (ped.documentThumbnailLocation == null && ped.documentLocation != null)
                        {
                            a.artifactLocation = ped.documentLocation;
                            
                        }
                        if (ped.documentLocation != null)
                        {
                            a.fileLocation = ped.documentLocation;
                        }
                        a.creationDate = "?/?/????";
                        if (ped.description != null)
                        {
                            a.description = ped.description;
                        }
                        if (ped.title != null)
                        {
                            a.title = ped.title;
                        }
                        add = 1;
                    }
                    else if (pe.GetType() == typeof(ProjectElement_Picture))
                    {
                        a.type = "picture";
                        ProjectElement_Picture pep = (ProjectElement_Picture)pe;
                        if (pep.pictureThumbnailLocation != null)
                        {
                            a.artifactLocation = pep.pictureThumbnailLocation;
                        }
                        if (pep.pictureLocation != null)
                        {
                            a.fileLocation = pep.pictureLocation;
                        }
                        a.creationDate = "?/?/????";
                        if (pep.description != null)
                        {
                            a.description = pep.description;
                        }
                        if (pep.title != null)
                        {
                            a.title = pep.title;
                        }
                        add = 1;
                    }
                    else if (pe.GetType() == typeof(ProjectElement_Video))
                    {
                        a.type = "video";
                        ProjectElement_Video pev = (ProjectElement_Video)pe;
                        if (pev.videoId != null)
                        {
                            a.artifactLocation = pev.videoId;
                        }
                        if (pev.description != null)
                        {
                            a.description = pev.description;
                        }
                        a.creationDate = "?/?/????";
                        if (pev.title != null)
                        {
                            a.title = pev.title;
                        }
                        add = 1;
                    }
                    else if (pe.GetType() == typeof(ProjectElement_Audio))
                    {
                        a.type = "audio";
                        ProjectElement_Audio pea = (ProjectElement_Audio)pe;
                        if (pea.audioLocation != null)
                        {
                            a.artifactLocation = pea.audioLocation;
                        }
                        if (pea.description != null)
                        {
                            a.description = pea.description;
                        }
                        a.creationDate = "?/?/????";
                        if (pea.title != null)
                        {
                            a.title = pea.title;
                        }
                        add = 1;
                    }
                    else if (pe.GetType() == typeof(ProjectElement_Code))
                    {
                        a.type = "code";
                        ProjectElement_Code pec = (ProjectElement_Code)pe;
                        if (pec.code != null)
                        {
                            a.artifactLocation = pec.code;
                        }
                        if (pec.description != null)
                        {
                            a.description = pec.description;
                        }
                        a.creationDate = "?/?/????";
                        if (pec.title != null)
                        {
                            a.title = pec.title;
                        }
                        if (pec.fileLocation != null)
                        {
                            a.fileLocation = pec.fileLocation;
                        }
                        add = 1;
                    }
                }
                if (add == 1)
                {
                    artifacts.Add(a);
                }
            }
            if (artifacts.Count != 0)
            {
                return artifacts;
            }
            else
            {
                return null;
            }
        }

        public List<JsonModels.Artifact> GetArtifacts(List<ProjectElement> elements)
        {
            List<JsonModels.Artifact> artifacts = new List<JsonModels.Artifact>();
            ProjectAccessor pa = new ProjectAccessor();
            int add = 0;
            foreach (ProjectElement pe in elements)
            {
                add = 0;
                JsonModels.Artifact a = new JsonModels.Artifact();
                if (pe != null)
                {
                    a.id = pe.id;
                    if (pe.GetType() == typeof(ProjectElement_Document))
                    {
                        a.type = "document";
                        ProjectElement_Document ped = (ProjectElement_Document)pe;
                        if (ped.documentThumbnailLocation != null)
                        {
                            a.artifactLocation = ped.documentThumbnailLocation;
                        }
                        a.creationDate = "?/?/????";
                        if (ped.description != null)
                        {
                            a.description = ped.description;
                        }
                        if (ped.title != null)
                        {
                            a.title = ped.title;
                        }
                        add = 1;
                    }
                    else if (pe.GetType() == typeof(ProjectElement_Picture))
                    {
                        a.type = "picture";
                        ProjectElement_Picture pep = (ProjectElement_Picture)pe;
                        if (pep.pictureLocation != null)
                        {
                            a.artifactLocation = pep.pictureLocation;
                        }
                        if (pep.pictureGalleriaThumbnailLocation != null)
                        {
                            a.thumbnailLocation = pep.pictureGalleriaThumbnailLocation;
                        }
                        a.creationDate = "?/?/????";
                        if (pep.description != null)
                        {
                            a.description = pep.description;
                        }
                        if (pep.title != null)
                        {
                            a.title = pep.title;
                        }
                        add = 1;
                    }
                    else if (pe.GetType() == typeof(ProjectElement_Video))
                    {
                        a.type = "video";
                        ProjectElement_Video pev = (ProjectElement_Video)pe;
                        if (pev.videoId != null)
                        {
                            a.artifactLocation = pev.videoId;
                        }
                        if (pev.description != null)
                        {
                            a.description = pev.description;
                        }
                        a.creationDate = "?/?/????";
                        if (pev.title != null)
                        {
                            a.title = pev.title;
                        }
                        add = 1;
                    }
                    else if (pe.GetType() == typeof(ProjectElement_Audio))
                    {
                        a.type = "audio";
                        ProjectElement_Audio pea = (ProjectElement_Audio)pe;
                        if (pea.audioLocation != null)
                        {
                            a.artifactLocation = pea.audioLocation;
                        }
                        if (pea.description != null)
                        {
                            a.description = pea.description;
                        }
                        a.creationDate = "?/?/????";
                        if (pea.title != null)
                        {
                            a.title = pea.title;
                        }
                        add = 1;
                    }
                    else if (pe.GetType() == typeof(ProjectElement_Experience))
                    {
                        a.type = "experience";
                        ProjectElement_Experience pee = (ProjectElement_Experience)pe;
                        if (pee.city != null)
                        {
                            a.city = pee.city;
                        }
                        if (pee.description != null)
                        {
                            a.description = pee.description;
                        }
                        if (pee.jobTitle != null)
                        {
                            a.title = pee.jobTitle;
                        }
                        if (pee.company != null)
                        {
                            a.company = pee.company;
                        }
                        if (pee.endDate != null)
                        {
                            a.endDate = pee.endDate.ToShortDateString();
                        }
                        if (pee.startDate != null)
                        {
                            a.startDate = pee.startDate.ToShortDateString();
                        }
                        if (pee.state != null)
                        {
                            a.state = pee.state;
                        }
                        add = 1;
                    }
                }
                if (add == 1)
                {
                    artifacts.Add(a);
                }
            }
            if (artifacts.Count != 0)
            {
                return artifacts;
            }
            else
            {
                return null;
            }

        }

        public List<JsonModels.CompleteProject> GetCompleteProjects(int[] id)
        {
            List<JsonModels.CompleteProject> projects = new List<JsonModels.CompleteProject>();
            ProjectAccessor pa = new ProjectAccessor();
            foreach (int i in id)
            {
                JsonModels.CompleteProject cp = new JsonModels.CompleteProject();
                ReorderEngine re = new ReorderEngine();
                Project p = pa.GetProject(i);
                if (p != null)
                {
                    if (p.isActive == true)
                    {
                        cp.id = p.id;
                        if (p.name != null)
                        {
                            cp.name = p.name;
                        }
                        if (p.description != null)
                        {
                            cp.description = p.description;
                        }
                        if (p.tagIds != "" && p.tagIds != null)
                        {
                            cp.projectTags = GetProjectTags(p.id);
                        }

                        //we have to reset everyone's project order or when you add a new element this breaks - removes all old elements
                        List<int> peIds = new List<int>();
                        //just reset order if its null - duh!
                        if (p.projectElementOrder == null | p.projectElementOrder == "")
                        {
                            p = resetProjectElementOrder(p);
                        }
                        if (p.projectElementOrder != null)
                        {
                            List<int> IDS = re.stringOrderToList(p.projectElementOrder);
                            int[] ids = IDS.ToArray();

                            cp.artifacts = GetArtifacts(ids);
                            cp.elementOrder = p.projectElementOrder;
                        }
                        else
                        {
                            //get in order
                        }
                        if (p.coverPicture != null)
                        {
                            cp.coverPicture = p.coverPicture;
                        }
                        if (p.coverPictureThumbnail != null)
                        {
                            cp.coverPictureThumbnail = p.coverPictureThumbnail;
                        }
                        projects.Add(cp);
                    }
                }
            }
            if (projects.Count != 0)
            {
                return projects;
            }
            else
            {
                return null;
            }
        }

        public List<ProjectElement> SearchProjects(User u, string query)
        {
            List<ProjectElement> matchElements = new List<ProjectElement>();
            string documentText = "";
            bool add = false;
            foreach (Project p in u.projects)
            {
                if (p.isActive == true)
                {
                    foreach (ProjectElement pe in p.projectElements)
                    {
                        add = false;
                        if (pe.GetType() == typeof(ProjectElement_Document))
                        {
                            ProjectElement_Document ped = (ProjectElement_Document)pe;
                            if (ped.description != null)
                            {
                                if (ped.description.ToLower().Contains(query))
                                {
                                    add = true;
                                }
                            }
                            if (ped.documentText != null)
                            {
                                if (ped.documentText.ToLower().Contains(query))
                                {
                                    add = true;
                                }
                            }
                            if (ped.title != null)
                            {
                                if (ped.title.ToLower().Contains(query))
                                {
                                    add = true;
                                }
                            }
                            if (add == true)
                            {
                                matchElements.Add(ped);
                            }
                        }
                        else
                        {
                            if (pe.description != null)
                            {
                                if (pe.description.ToLower().Contains(query))
                                {
                                    add = true;
                                }
                            }
                            if (pe.title != null)
                            {
                                if (pe.title.ToLower().Contains(query))
                                {
                                    add = true;
                                }
                            }
                            if (add == true)
                            {
                                matchElements.Add(pe);
                            }
                            
                        }
                    }
                }
            }
            return matchElements;
        }
    }
}
