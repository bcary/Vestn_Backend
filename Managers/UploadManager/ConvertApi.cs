using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using Microsoft.Win32;

namespace Manager
{

    public class ConvertApiException : Exception
    {
        public ConvertApiException() { }
        public ConvertApiException(string message) : base(message) { }
        public ConvertApiException(string message, Exception inner) : base(message, inner) { }

        readonly HttpStatusCode _statusCode;
        readonly string _statusDescription;
        private readonly bool _customException = false;

        public ConvertApiException(HttpStatusCode statusCode, string statusDescription)
        {
            _statusCode = statusCode;
            _statusDescription = statusDescription;
            _customException = true;
        }

        public override string ToString()
        {
            if (!_customException)
                return base.ToString();
            else
            {
                const string convertapiServerResponse = "ConvertApi server response:";
                return _statusCode != HttpStatusCode.Unused
                           ? String.Format("{0} {1}  {2}", convertapiServerResponse, (int)_statusCode, _statusDescription)
                           : String.Format("{0} {1}", convertapiServerResponse, _statusDescription);
            }

        }

    }

    public abstract class ConvertApi
    {
        internal readonly int ApiKey;

        //internal string ApiBaseUri = "http://do.convertapi.com/";
        public static string ApiBaseUri = "http://do.convertapi.com/";


        private const string UserAgent = "ConvertApi DotNet library";
        internal Dictionary<string, object> PostParameters = new Dictionary<string, object>();
        private int _httpRequestTimeOut = 320;
        private const int ApiConverterTimeOutBias = 60;


        protected ConvertApi(int apiKey)
        {
            ApiKey = apiKey;
        }

        protected ConvertApi()
        {
        }

        internal string Api { get { return GetType().Name; } }


        public int? GetCreditsLeft { get; private set; }
        public int GetCreditsCost { get; private set; }
        public int GetFileSize { get; private set; }
        public string GetOutputFileName { get; private set; }
        public string GetProcessId { get; private set; }

        public void SetTimeout(int value)
        {
            PostParameters["Timeout"] = value;
            _httpRequestTimeOut = value * ApiConverterTimeOutBias;
        }

        /*       public void SetOutputFileName(string value)
               {
                   PostParameters["OutputFileName"] = value;
               }*/

        /*        public void ConvertFile(string inFileName, Stream outStream)
                {
                    //todo
                }*/

        /*      public void ConvertFile(string inFileName,string inFileMimeType, string outFileName)
                {
                    //todo
                }*/




        internal void Convert(Dictionary<string, object> postParameters, Stream outStream, string postUri)
        {
            //string postUri = ApiBaseUri + Api;
            PostParameters["ApiKey"] = ApiKey.ToString();

            try
            {
                HttpWebResponse webResponse = FormUpload.MultipartFormDataPost(postUri, UserAgent, _httpRequestTimeOut,
                                                                               postParameters);
                Stream clientResponse = webResponse.GetResponseStream();
                if (webResponse.StatusCode == HttpStatusCode.OK)
                {
                    Helpers.CopyStream(clientResponse, outStream);

                    object creditsLeft = webResponse.Headers["CreditsLeft"];
                    if (creditsLeft != null)
                        GetCreditsLeft = System.Convert.ToInt32(creditsLeft);

                    GetFileSize = System.Convert.ToInt32(webResponse.Headers["FileSize"]);
                    GetCreditsCost = System.Convert.ToInt32(webResponse.Headers["CreditsCost"]);
                    GetOutputFileName = webResponse.Headers["OutputFileName"];
                    GetProcessId = webResponse.Headers["ProcessId"];
                }
                else
                {
                    throw new WebException("Response error!");
                }
            }
            catch (WebException e)
            {
                if (e.Response != null)
                {
                    HttpWebResponse httpWebResponse = ((HttpWebResponse)e.Response);
                    throw new ConvertApiException(httpWebResponse.StatusCode, httpWebResponse.StatusDescription);
                }
                else
                {
                    throw new ConvertApiException("Exception occurred while posting data to server.", e);
                }
            }
        }
    }

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public abstract class FileMethods : ConvertApi
    {
        protected FileMethods(int apiKey)
            : base(apiKey)
        {
        }

        protected FileMethods()
        {
        }

        public void ConvertFile(string inFileName, string outFileName)
        {
            try
            {
                using (FileStream intStream = new FileStream(inFileName, FileMode.Open))
                {
                    using (FileStream outStream = new FileStream(outFileName, FileMode.Create))
                    {
                        ConvertFile(intStream, inFileName, outStream);
                    }
                }

            }
            finally
            {
                //Deleting empty output file on exception               
                if (File.Exists(outFileName) && new FileInfo(outFileName).Length == 0) File.Delete(outFileName);
            }
        }

        public void ConvertFile(string inFileName, Stream outStream)
        {
            using (FileStream intStream = new FileStream(inFileName, FileMode.Open))
            {
                ConvertFile(intStream, inFileName, outStream);

            }
        }

        public virtual void ConvertFile(Stream inStream, string fileName, Stream outStream)
        {
            byte[] data = new byte[inStream.Length];
            inStream.Read(data, 0, data.Length);
            inStream.Close();

            /*            if (!PostParameters.ContainsKey("OutputFileName"))
                            SetOutputFileName(Path.GetFileNameWithoutExtension(fileName));*/

            PostParameters.Add("file", new FormUpload.FileParameter(data, fileName, Helpers.GetMimeType(fileName)));

            Convert(PostParameters, outStream, "dummy");
        }

        public virtual void ConvertFileByte(Byte[] data, string fileName, Stream outStream, string postUri)
        {

            PostParameters.Add("file", new FormUpload.FileParameter(data, fileName, Helpers.GetMimeType(fileName)));

            Convert(PostParameters, outStream, postUri);
        }
    }

    public abstract class OfficeEntity : FileMethods
    {

        protected OfficeEntity(int apiKey)
            : base(apiKey)
        {
        }

        protected OfficeEntity()
        {
        }



        public void SetDocumentTitle(string value)
        {
            PostParameters["DocumentTitle"] = value;
        }

        public void SetDocumentSubject(string value)
        {
            PostParameters["DocumentSubject"] = value;
        }

        public void SetDocumentAuthor(string value)
        {
            PostParameters["DocumentAuthor"] = value;
        }

        public void SetDocumentKeywords(string value)
        {
            PostParameters["DocumentKeywords"] = value;
        }

        public void SetOutputFormat(string value)
        {
            PostParameters["OutputFormat"] = value;
        }




    }

    public class Word2Pdf : OfficeEntity
    {
        public Word2Pdf(int apiKey)
            : base(apiKey)
        {
        }

        public Word2Pdf()
        {
        }

    }

    public class Excel2Pdf : OfficeEntity
    {
        public Excel2Pdf(int apiKey)
            : base(apiKey)
        {
        }

        public Excel2Pdf()
        {
        }

    }

    public class Text2Pdf : OfficeEntity
    {
        public Text2Pdf(int apiKey)
            : base(apiKey)
        {
        }

        public Text2Pdf()
        {
        }


    }

    public class RichText2Pdf : OfficeEntity
    {
        public RichText2Pdf(int apiKey)
            : base(apiKey)
        {
        }

        public RichText2Pdf()
        {
        }

    }


    public class PowerPoint2Pdf : OfficeEntity
    {
        public PowerPoint2Pdf(int apiKey)
            : base(apiKey)
        {
        }

        public PowerPoint2Pdf()
        {
        }
    }

    public class Lotus2Pdf : OfficeEntity
    {
        public Lotus2Pdf(int apiKey)
            : base(apiKey)
        {
        }

        public Lotus2Pdf()
        {
        }
    }

    public class Snp2Pdf : OfficeEntity
    {
        public Snp2Pdf(int apiKey)
            : base(apiKey)
        {
        }

        public Snp2Pdf()
        {
        }
    }

    public class Image2Pdf : OfficeEntity
    {
        public Image2Pdf(int apiKey)
            : base(apiKey)
        {
        }

        public Image2Pdf()
        {
        }
    }

    public class Xps2Pdf : OfficeEntity
    {
        public Xps2Pdf(int apiKey)
            : base(apiKey)
        {
        }

        public Xps2Pdf()
        {
        }
    }

    public class Publisher2Pdf : OfficeEntity
    {
        public Publisher2Pdf(int apiKey)
            : base(apiKey)
        {
        }

        public Publisher2Pdf()
        {
        }
    }

    public class OpenOffice2Pdf : OfficeEntity
    {
        public OpenOffice2Pdf(int apiKey)
            : base(apiKey)
        {
        }

        public OpenOffice2Pdf()
        {
        }
    }

    public abstract class SourceEntity : FileMethods
    {
        protected SourceEntity(int apiKey)
            : base(apiKey)
        {
        }

        protected SourceEntity()
        {
        }
    }



    public abstract class WebEntity : ConvertApi
    {
        protected WebEntity(int apiKey)
            : base(apiKey)
        {
        }

        protected WebEntity()
        {
        }


        public void ConvertFile(string inFileName, string outFileName)
        {
            try
            {
                string htmlContent = File.ReadAllText(inFileName);

                using (FileStream outStream = new FileStream(outFileName, FileMode.Create))
                {
                    ConvertHtml(htmlContent, outStream);
                }


            }
            finally
            {
                //Deleting empty output file on exception               
                if (File.Exists(outFileName) && new FileInfo(outFileName).Length == 0) File.Delete(outFileName);
            }
        }

        public void ConvertHtml(string html, Stream stream)
        {
            ConvertUri(html, stream);
        }


        public void ConvertUri(string uri, string outFileName)
        {
            using (FileStream outStream = new FileStream(outFileName, FileMode.Create))
            {
                ConvertUri(uri, outStream);
            }

        }

        public void ConvertUri(string uri, Stream stream)
        {

            PostParameters["CUrl"] = uri;
            Convert(PostParameters, stream, "dummy");
        }

        public void SetConversionDelay(int value)
        {
            PostParameters["ConversionDelay"] = value.ToString();
        }

        public void SetScripts(bool value)
        {
            PostParameters["Scripts"] = value.ToString();
        }

        public void SetPlugins(bool value)
        {
            PostParameters["Plugins"] = value.ToString();
        }

        public void SetAuthUsername(string value)
        {
            PostParameters["AuthUsername"] = value;
        }

        public void SetAuthPassword(string value)
        {
            PostParameters["AuthPassword"] = value;
        }

    }

    public class Web2Pdf : WebEntity
    {
        public Web2Pdf(int apiKey)
            : base(apiKey)
        {
        }

        public Web2Pdf()
        {
        }

        public void SetPrintType(bool value)
        {
            PostParameters["PrintType"] = value.ToString();
        }

        public void SetMarginLeft(string value)
        {
            PostParameters["MarginLeft"] = value;
        }

        public void SetMarginRight(string value)
        {
            PostParameters["MarginRight"] = value;
        }

        public void SetMarginTop(string value)
        {
            PostParameters["MarginTop"] = value;
        }


        public void SetMarginBottom(string value)
        {
            PostParameters["MarginBottom"] = value;
        }

        public void SetDocumentTitle(string value)
        {
            PostParameters["DocumentTitle"] = value;
        }

        public void SetPageOrientation(string value)
        {
            PostParameters["PageOrientation"] = value;
        }

        public void SetPageSize(string value)
        {
            PostParameters["PageSize"] = value;
        }

        public void SetPageWidth(string value)
        {
            PostParameters["PageWidth"] = value;
        }

        public void SetPageHeight(string value)
        {
            PostParameters["PageHeight"] = value;
        }

        public void SetCoverUrl(string value)
        {
            PostParameters["CoverUrl"] = value;
        }

        public void SetOutline(bool value)
        {
            PostParameters["Outline"] = value;
        }

        public void SetBackground(bool value)
        {
            PostParameters["Background"] = value;
        }

        public void SetPageNo(bool value)
        {
            PostParameters["PageNo"] = value;
        }

        public void SetInfoStamp(bool value)
        {
            PostParameters["InfoStamp"] = value;
        }

        public void SetLowQuality(bool value)
        {
            PostParameters["LowQuality"] = value;
        }

        public void SetFooterUrl(string value)
        {
            PostParameters["FooterUrl"] = value;
        }

        public void SetHeaderUrl(string value)
        {
            PostParameters["HeaderUrl"] = value;
        }

    }

    public class Web2Image : WebEntity
    {
        public Web2Image(int apiKey)
            : base(apiKey)
        {
        }

        public Web2Image()
        {
        }

        public void SetPageWidth(int value)
        {
            PostParameters["PageWidth"] = value;
        }

        public void SetPageHeight(int value)
        {
            PostParameters["PageHeight"] = value;
        }


    }

    public static class Helpers
    {
        internal static void CopyStream(Stream input, Stream output)
        {
            byte[] buffer = new byte[32768];
            while (true)
            {
                int read = input.Read(buffer, 0, buffer.Length);
                if (read <= 0)
                    return;
                output.Write(buffer, 0, read);
            }
        }

        public static string GetMimeType(string filePath)
        {

            const string defaultMimeType = "application/octet-stream";

            FileInfo fileInfo = new FileInfo(filePath);
            string fileExtension = fileInfo.Extension.ToLower();

            // direct mapping which is fast and ensures these extensions are found
            switch (fileExtension)
            {
                case ".htm":
                case ".html":
                    return "text/html";
                case ".js":
                    return "text/javascript"; // registry may return "application/x-javascript"
                case ".pdf":
                    return "application/pdf";
            }



            // looks for extension with a content type
            RegistryKey rkContentTypes = Registry.ClassesRoot.OpenSubKey(fileExtension);
            if (rkContentTypes != null)
            {
                object key = rkContentTypes.GetValue("Content Type");
                if (key != null)
                    return key.ToString().ToLower();
            }


            // looks for a content type with extension
            // Note : This would be problem if  multiple extensions associate with one content type.
            var typeKey = Registry.ClassesRoot.OpenSubKey(@"MIME\Database\Content Type");

            foreach (string keyname in typeKey.GetSubKeyNames())
            {
                RegistryKey curKey = typeKey.OpenSubKey(keyname);
                if (curKey != null)
                {
                    object extension = curKey.GetValue("Extension");
                    if (extension != null)
                    {
                        if (extension.ToString().ToLower() == fileExtension)
                        {
                            return keyname;
                        }
                    }
                }
            }

            return defaultMimeType;
        }
    }
}
