using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;
using Entity;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.StorageClient;

namespace Accessor
{
    public class BlobStorageAccessor
    {
        private const string messageImageBlobName = "fullsizeimage";
        private const string messageResumeBlobName = "resume";
        private const string testBlobName = "testimages";
        private CloudBlobClient blobClient;
        private CloudBlobContainer blobContainer;

        public Uri Save(bool test, Stream s, string blobName, string fileExtention, string fileContentType)
        {
            CloudStorageAccount storageAccount;
            if (test)
            {
                storageAccount = CloudStorageAccount.FromConfigurationSetting("BlobConnectionString");
            }
            else
            {
                storageAccount = CloudStorageAccount.Parse(RoleEnvironment.GetConfigurationSettingValue("BlobConnectionString"));
            }
            //storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=vestnstorage;AccountKey=OCZoZBQegbepYCxWNoOLYLkSBg461Bo0cac2F6Q9hpOwiOykYZmgwb5EsGgJWUfPuC3kalOuNk1Dp0C0HXsRaQ==");
            blobClient = storageAccount.CreateCloudBlobClient();
            blobContainer = blobClient.GetContainerReference(blobName);
            blobContainer.CreateIfNotExist();
            var permissions = blobContainer.GetPermissions();
            permissions.PublicAccess = BlobContainerPublicAccessType.Container;
            blobContainer.SetPermissions(permissions);
            return AddBlob(fileExtention, fileContentType, s, blobName);
        }

        public Uri Save(bool test, Stream s, string blobName, string fileExtention, string fileContentType, string presetURL)
        {
            CloudStorageAccount storageAccount;
            if (test)
            {
                storageAccount = CloudStorageAccount.FromConfigurationSetting("BlobConnectionString");
            }
            else
            {
                storageAccount = CloudStorageAccount.Parse(RoleEnvironment.GetConfigurationSettingValue("BlobConnectionString"));
            }
            //storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=vestnstorage;AccountKey=OCZoZBQegbepYCxWNoOLYLkSBg461Bo0cac2F6Q9hpOwiOykYZmgwb5EsGgJWUfPuC3kalOuNk1Dp0C0HXsRaQ==");
            blobClient = storageAccount.CreateCloudBlobClient();
            blobContainer = blobClient.GetContainerReference(blobName);
            blobContainer.CreateIfNotExist();
            var permissions = blobContainer.GetPermissions();
            permissions.PublicAccess = BlobContainerPublicAccessType.Container;
            blobContainer.SetPermissions(permissions);
            return AddBlob(fileExtention, fileContentType, s, blobName, presetURL);
        }


        public Uri uploadImage(Stream stream, bool test)
        {
            return Save(test, stream, "images", ".jpeg", "image/jpeg");
        }
        public Uri uploadThumbnail(Stream stream, bool test, string presetURL)
        {
            return Save(test, stream, "thumbnails", ".jpeg", "image/jpeg", presetURL);
        }
        public Uri uploadPDF(Stream stream, bool test)
        {
            return Save(test, stream, "pdfs", ".pdf", "application/pdf");
        }
        public Uri uploadPDF(Stream stream, bool test, string presetURL)
        {
            return Save(test, stream, "pdfs", ".pdf", "application/pdf", presetURL);
        }
        public Uri uploadDOC(Stream stream, bool test, string extention)//needs period
        {
            return Save(test, stream, "documents", extention, "application/msword");
        }
        public Uri uploadMP3(Stream stream, bool test)
        {
            return Save(test, stream, "audio", ".mp3", "audio/mpeg");
        }
        public Uri uploadOGG(Stream stream, bool test)
        {
            return Save(test, stream, "audio", ".ogg", "audio/ogg");
        }
        public Uri uploadWAV(Stream stream, bool test)
        {
            return Save(test, stream, "audio", ".wav", "audio/wav");
        }
        public Uri uploadM4A(Stream stream, bool test)
        {
            return Save(test, stream, "audio", ".m4a", "audio/mpeg");
        }
        public Uri uploadUnknown(Stream stream, bool test, string extention)
        {
            if (checkExtention(extention))
            {
                return Save(test, stream, "documents", "." + extention, "text/enriched");
            }
            else
            {
                return null;
            }
        }

        public bool checkExtention(string extention)
        {
            bool result = false;
            extention = extention.ToLower();
            if (extention == "png" || extention == "jpeg" || extention == "jpg" || extention == "bmp" || extention == "xls" || extention == "xlsx" || extention == "ppt" || extention == "pptx" || extention == "pps" || extention == "rtf" || extention == "txt" || extention == "csv" || extention == "tsv")
            {
                result = true;
            }
            return result;
        }

        public Uri AddBlob(string fileExtension, string fileContentType, Stream fileContent, string location)
        {
            String FileName = Guid.NewGuid().ToString();
            string uniqueBlobName = string.Format("{0}{1}", FileName, fileExtension);
            CloudBlob blob = blobContainer.GetBlobReference(uniqueBlobName);
            blob.Properties.ContentType = fileContentType;
            fileContent.Seek(0, SeekOrigin.Begin);
            blob.UploadFromStream(fileContent);
            Uri uri = blob.Uri;
            fileContent.Close();
            return uri;
        }

        public Uri AddBlob(string fileExtension, string fileContentType, Stream fileContent, string location, string presetURL)
        {
            CloudBlob blob = blobContainer.GetBlobReference(presetURL);
            blob.Properties.ContentType = fileContentType;
            fileContent.Seek(0, SeekOrigin.Begin);
            blob.UploadFromStream(fileContent);
            Uri uri = blob.Uri;
            fileContent.Close();
            return uri;
        }
        //not tested
        public Uri AddBlobByteArray(string fileExtension, string fileContentType, byte[] fileContent, string location)
        {
            String FileName = Guid.NewGuid().ToString();
            string uniqueBlobName = string.Format(location + "/{0}{1}", FileName, fileExtension);
            CloudBlob blob = blobContainer.GetBlobReference(uniqueBlobName);
            blob.Properties.ContentType = fileContentType;
            blob.UploadByteArray(fileContent);
            Uri uri = blob.Uri;
            return uri;
        }
        public void deleteFromBlob(string uri, string blobname)
        {
            CloudStorageAccount storageAccount;
            storageAccount = CloudStorageAccount.Parse(RoleEnvironment.GetConfigurationSettingValue("BlobConnectionString"));
            blobClient = storageAccount.CreateCloudBlobClient();
            blobContainer = blobClient.GetContainerReference(blobname);
            var blob = blobContainer.GetBlobReference(uri);
            blob.DeleteIfExists();
        }
    }
}
