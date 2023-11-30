using Google.Cloud.Firestore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace NotificationModule
{
    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            InitializeComponent();
        }
        public void RandomNotification(string fcmtoken, string content, string title)
        {
            WebRequest tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send");
            tRequest.Method = "post";
            //serverKey - Key from Firebase cloud messaging server  
            tRequest.Headers.Add(string.Format("Authorization: key={0}", "AAAAYwWU_3M:APA91bHq5nCd7QWrPQD2ImogA971z3rj6StpH2RFMN7Vhp-YactZqXZVkBmmu0JyNM--mD7v78exDM6svs_Y1r9M-zYfzJktJyBTloLbdK2mfsjqBKZKmouZLZeTBs4EGsMlw8TvNC0p"));
            //Sender Id - From firebase project setting  
            tRequest.Headers.Add(string.Format("Sender: id={0}", "425295413107"));
            tRequest.ContentType = "application/json";
            var payload = new object();
            payload = new
            {
                to = fcmtoken,
                priority = "high",
                content_available = true,
                notification = new
                {
                    body = content,
                    title = title,
                    badge = 1,
                },
                data = new
                {
                    type = "random",

                }

            };

            string postbody = JsonConvert.SerializeObject(payload).ToString();
            Byte[] byteArray = Encoding.UTF8.GetBytes(postbody);
            tRequest.ContentLength = byteArray.Length;
            using (Stream dataStream = tRequest.GetRequestStream())
            {
                dataStream.Write(byteArray, 0, byteArray.Length);
                using (WebResponse tResponse = tRequest.GetResponse())
                {
                    using (Stream dataStreamResponse = tResponse.GetResponseStream())
                    {
                        if (dataStreamResponse != null) using (StreamReader tReader = new StreamReader(dataStreamResponse))
                            {
                                String sResponseFromServer = tReader.ReadToEnd();
                                WriteToFile(sResponseFromServer);
                                //result.Response = sResponseFromServer;
                            }
                    }
                }
            }

        }

        private async void SendNotifications()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "servicekey.json";
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", path);
            try
            {
                WriteToFile("\nInside Send Notifications Function");
                var db = FirestoreDb.Create("fir-auth-7e9d6");
                var colref = await db.Collection("users").GetSnapshotAsync();
                foreach(var doc in colref.Documents)
                {
                    var dict = doc.ToDictionary();
                    if (dict.ContainsKey("Location"))
                    {

                    }
                    else
                    {
                        WriteToFile("\n There is a missing Location Field for user " + doc.Id);
                    }
                }
            }
            catch (Exception e)
            {
                WriteToFile(e.ToString());
            }
        }
        protected override void OnStart(string[] args)
        {
            
            WriteToFile("Service is started at " + DateTime.Now);
            SendNotifications();
        }

        protected override void OnStop()
        {
            WriteToFile("Service is stopped at " + DateTime.Now);
        }
        public void WriteToFile(string Message)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "\\Logs";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string filepath = AppDomain.CurrentDomain.BaseDirectory + "\\Logs\\ServiceLog_" + DateTime.Now.Date.ToShortDateString().Replace('/', '_') + ".txt";
            if (!File.Exists(filepath))
            {
                // Create a file to write to.   
                using (StreamWriter sw = File.CreateText(filepath))
                {
                    sw.WriteLine(Message);
                }
            }
            else
            {
                using (StreamWriter sw = File.AppendText(filepath))
                {
                    sw.WriteLine(Message);
                }
            }
        }
    }
}
