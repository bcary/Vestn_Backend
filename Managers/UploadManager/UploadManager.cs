using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing.Imaging;
using System.IO;
using System.Drawing;
using System.Web;
using System.Net;
using Manager;
using Entity;
using Google.GData.Client;
using Google.GData.Extensions;
using Google.GData.YouTube;
using Google.YouTube;
using Google.GData.Extensions.MediaRss;
using Engine;
using Accessor;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.StorageClient;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace Manager
{
    public class UploadManager
    {

        ThumbnailEngine TEngine = new ThumbnailEngine();
        BlobStorageAccessor BSAccessor = new BlobStorageAccessor();
        CloudStorageAccount storageAccount;
        private CloudBlobClient blobClient;
        private CloudBlobContainer blobContainer;

        public string TestMe()
        {
            return "success";
        }

        public string generateThumbnail(string imageURI, int entityId, string type, int displayWidth, int displayHeight)
        {
            try
            {
                storageAccount = CloudStorageAccount.Parse(RoleEnvironment.GetConfigurationSettingValue("BlobConnectionString"));
                blobClient = storageAccount.CreateCloudBlobClient();
                blobContainer = blobClient.GetContainerReference("images");

                int idValue = entityId;
                CloudBlob inputBlob = blobContainer.GetBlobReference(imageURI);
                Stream input = inputBlob.OpenRead();
                Bitmap image = TEngine.CreateThumbnail(input, displayWidth, displayHeight);
                Uri uri;
                using (MemoryStream stream = new MemoryStream())
                {
                    // Save image to stream.
                    image.Save(stream, ImageFormat.Png);//changed this to make the background transparent
                    uri = BSAccessor.uploadThumbnail(stream, false);
                }
                if (type == "User")
                {
                    UserAccessor ua = new UserAccessor();
                    User u = ua.GetEntityUser(idValue);
                    u.profilePictureThumbnail = uri.ToString();
                    ua.UpdateFromWorker(u);
                }
                if (type == "About")
                {
                    UserAccessor ua = new UserAccessor();
                    User u = ua.GetEntityUser(idValue);
                    u.aboutPictureThumbnail = uri.ToString();
                    ua.UpdateFromWorker(u);
                }
                else
                {
                    ProjectAccessor pa = new ProjectAccessor();
                    if (type == "PictureElement")
                    {
                        ProjectElement_Picture pe = (ProjectElement_Picture)pa.GetProjectElement(idValue);
                        pe.pictureThumbnailLocation = uri.ToString();
                        pa.UpdateProjectElement(pe);
                    }
                    else if (type == "PictureElement_Galleria")
                    {
                        ProjectElement_Picture pe = (ProjectElement_Picture)pa.GetProjectElement(idValue);
                        pe.pictureGalleriaThumbnailLocation = uri.ToString();
                        pa.UpdateProjectElement(pe);
                    }
                    else if (type == "DocumentElement")
                    {
                        ProjectElement_Document pe = (ProjectElement_Document)pa.GetProjectElement(idValue);
                        pe.documentThumbnailLocation = uri.ToString();
                        pa.UpdateProjectElement(pe);
                    }
                    else if (type == "ProjectPicture")
                    {
                        Project p = pa.GetProject(idValue);
                        p.coverPictureThumbnail = uri.ToString();
                        pa.UpdateProject(p);
                    }
                }
                return uri.ToString();
            }
            catch(Exception e)
            {
                LogAccessor logAccessor = new LogAccessor();
                logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), e.ToString());
                return "thumbnailGenFail";
            }
        }

        //This function will return the URL and the token that needed to upload the video
        //
        public string[] uploadVideoFile()
        {

            var youTubeRequest = GetRequest();
            var newVideo = new Video { Title = "VideoTitle", Description = "VideoDescription" };

            //newVideo.Media.Categories.Add(new MediaCategory("All")); // You can assign any category here.
            newVideo.Tags.Add(new MediaCategory("Film", YouTubeNameTable.CategorySchema));
            newVideo.Keywords = "samplekeyword";
            newVideo.YouTubeEntry.Private = false;

            //Something bombed out once in a while with this code and we not sure why...
            try
            {
                var token = youTubeRequest.CreateFormUploadToken(newVideo);
                var postUrl = token.Url;
                var tokenValue = token.Token;

                string[] UrlAndToken = new string[2];
                UrlAndToken[0] = postUrl;
                UrlAndToken[1] = tokenValue;

                return UrlAndToken;
            }
            catch
            {
                return null;
            }
            
  
        }

        // This will create a youttube request  
        public static YouTubeRequest GetRequest()
        {
            YouTubeRequest request;
            
            request = System.Web.HttpContext.Current.Session["YTRequest"] as YouTubeRequest;
          

            if (request == null)
            {

                var settings = new YouTubeRequestSettings("YoutubeAPI",
                                          ConfigurationManager.AppSettings["YouTubeAPIDeveloperKey"],
                                          ConfigurationManager.AppSettings["YouTubeUsername"],
                                          ConfigurationManager.AppSettings["YouTubePassword"]);

                request = new YouTubeRequest(settings);

                System.Web.HttpContext.Current.Session["YTRequest"] = request;

            }

            return request;

        }

        public static double GetHypotenuseAngleInDegreesFrom(double opposite, double adjacent)
        {

             double radians = Math.Atan2(opposite, adjacent); // Get Radians for Atan2
             double angle = radians*(180/Math.PI); // Change back to degrees
             return angle;
         }

        public string ExtractText(string location)
        {
            try
            {
                MemoryStream forPdf = new MemoryStream();
                WebClient client = new WebClient();
                byte[] pdfData = client.DownloadData(location);
                forPdf.Write(pdfData, 0, pdfData.Length);
                PdfReader reader = new PdfReader(pdfData);

                StringBuilder text = new StringBuilder();
                for (int page = 1; page <= reader.NumberOfPages; page++)
                {
                    iTextSharp.text.pdf.parser.ITextExtractionStrategy strategy = new iTextSharp.text.pdf.parser.SimpleTextExtractionStrategy();
                    string currentText = iTextSharp.text.pdf.parser.PdfTextExtractor.GetTextFromPage(reader, page, strategy);

                    currentText = Encoding.UTF8.GetString(ASCIIEncoding.Convert(Encoding.Default, Encoding.UTF8, Encoding.Default.GetBytes(currentText)));
                    text.Append(currentText);
                    reader.Close();
                }
                return text.ToString();
            }
            catch (Exception e)
            {
                //log error
                return "";
            }
        }

        public string stampThatShit(string location, string userName, string presetURL)
        {
            MemoryStream forPdf = new MemoryStream();
            WebClient client = new WebClient();
            byte[] pdfData = client.DownloadData(location);
            forPdf.Write(pdfData, 0, pdfData.Length);
            PdfReader reader = new PdfReader(pdfData);
            PdfStamper stamper = new PdfStamper(reader, forPdf);
            for (int i = 1; i <= reader.NumberOfPages; i++)
            {
                iTextSharp.text.Rectangle pageSize = reader.GetPageSizeWithRotation(i);
                PdfContentByte pdfPageContents = stamper.GetUnderContent(i);
                pdfPageContents.BeginText();
                BaseFont font = BaseFont.CreateFont(BaseFont.HELVETICA, Encoding.ASCII.EncodingName, false);
                pdfPageContents.SetFontAndSize(font, 25);
                pdfPageContents.SetRGBColorFill(255, 214, 169);
                float angle = (float)GetHypotenuseAngleInDegreesFrom(pageSize.Height, pageSize.Width);
                pdfPageContents.ShowTextAligned(PdfContentByte.ALIGN_CENTER, userName, pageSize.Width / 2, pageSize.Height / 2, angle);
                pdfPageContents.EndText();
            }
            stamper.FormFlattening = true;
            stamper.Close();
            MemoryStream ms = new MemoryStream();
            ms.Write(forPdf.ToArray(), 0, forPdf.ToArray().Length);
            if (presetURL != null)
            {
                return BSAccessor.uploadPDF(ms, false, presetURL).ToString();
            }
            else
            {
                return BSAccessor.uploadPDF(ms, false).ToString();
            }
        }

        public string convertDocument(string location, string type, int projectElementId, string userName, string presetDocURL)
        {
            string PDFLocation = "unassigned";
            ProjectAccessor pa = new ProjectAccessor();
            ProjectElement_Document pe = (ProjectElement_Document)pa.GetProjectElement(projectElementId);
            WebClient client = new WebClient();
            byte[] data = client.DownloadData(location);
            MemoryStream outStream = new MemoryStream();
            try
            {
                Word2Pdf convertApi = new Word2Pdf(667067036);
                //Word2Pdf convertApi = new Word2Pdf();
                convertApi.ConvertFileByte(data, location.Substring(location.Length - 6, 6), outStream, type);
                if (userName != "")
                {
                    PDFLocation = BSAccessor.uploadPDF(outStream, false).ToString();
                    PDFLocation = stampThatShit(PDFLocation, userName, presetDocURL);
                }
                else
                {
                    PDFLocation = BSAccessor.uploadPDF(outStream, false, presetDocURL).ToString();
                }
                pe.documentText = ExtractText(PDFLocation);
                pe.documentThumbnailLocation = PDFLocation;
                pa.UpdateProjectElement(pe);
            }
            catch (Exception e)
            {
                return "dummy";
            }

            outStream.Close();

            return PDFLocation;
        }
        
    }
}
