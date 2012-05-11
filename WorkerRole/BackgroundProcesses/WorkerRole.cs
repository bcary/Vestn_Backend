using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.StorageClient;
using System.Drawing.Imaging;
using System.IO;
using System.Drawing;
using Manager;
using Accessor;

namespace BackgroundProcesses
{
    public class WorkerRole : RoleEntryPoint
    {
        private const string messageQueueName = "uploadqueue"; //queue name must be in lower case
        private CloudQueueClient queueClient;
        private CloudQueue queue;

        public override void Run()
        {   
            Trace.WriteLine("BackgroundProcesses entry point called", "Information");
            Trace.WriteLine(Thread.CurrentThread.Name, "Information");
            try
            {
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(RoleEnvironment.GetConfigurationSettingValue("BlobConnectionString"));
                Trace.WriteLine("storage account configured", "Information");
                queueClient = storageAccount.CreateCloudQueueClient();
                queue = queueClient.GetQueueReference(messageQueueName);
                queue.CreateIfNotExist();
                queue.Clear();
            }
            catch (Exception e)
            {
                LogAccessor logAccessor = new LogAccessor();
                logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), e.ToString());  
            }
            while (true)
            {
                try
                {
                    CloudQueueMessage msg = queue.GetMessage();

                    if (msg != null)
                    {
                        Trace.WriteLine("Queue Message Recieved", "Information");
                        // parse message retrieved from queue
                        string userFullName = "";
                        string presetDocURL = "";
                        var messageParts = msg.AsString.Split(new char[] { ',' });
                        var mediaURI = messageParts[0];
                        int ID = Int32.Parse(messageParts[1]);
                        var operation = messageParts[2];
                        var type = messageParts[3];
                        int displayWidth = Int32.Parse(messageParts[4]);
                        int displayHeight = Int32.Parse(messageParts[5]);
                        try
                        {
                            userFullName = messageParts[6];
                            presetDocURL = messageParts[7];
                        }
                        catch (Exception)
                        {

                        }
                        if (operation.Equals("thumbnail"))
                        {
                            UploadManager uploadManager = new UploadManager();

                            string thumbnailURI = uploadManager.generateThumbnail(mediaURI, ID, type, displayWidth, displayHeight);
                        }
                        else if (operation.Equals("documentConversion"))
                        {
                            UploadManager uploadManager = new UploadManager();
                            uploadManager.convertDocument(mediaURI, type, ID, userFullName, presetDocURL);
                        }
                        queue.DeleteMessage(msg);
                    }

                    Thread.Sleep(500);
                    //Trace.WriteLine("Working - thread sleep", "Information");
                }
                catch (Exception e)
                {
                    LogAccessor logAccessor = new LogAccessor();
                    logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), e.ToString());
                }
            }
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections 
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.


            return base.OnStart();
        }

     }
}
