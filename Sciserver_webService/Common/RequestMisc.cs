using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Http;

namespace Sciserver_webService.Common
{

    
    public class RequestMisc
    {
        public bool IsDirectUserConnection = true;
        public Dictionary<String, String> dictionary = null;
        public string server_name = null;
        public string windows_name = null;
        public LoggedInfo ActivityInfo = null;

        public RequestMisc(HttpRequestMessage request, string EntryPoint)
        {
            try
            {
                //this.dictionary = request.GetQueryNameValuePairs().ToDictionary(kv => kv.Key, kv => kv.Value, StringComparer.OrdinalIgnoreCase);
                try 
                { 
                    this.dictionary = GetDict(request.RequestUri.ParseQueryString());
                }
                catch (Exception e)
                {
                    throw new ArgumentException("Check input parameters properly.\n" + e.Message);
                }

                this.server_name = HttpContext.Current.Request.ServerVariables["SERVER_NAME"];
                this.windows_name = System.Environment.MachineName;

                this.ActivityInfo = new LoggedInfo();
                this.ActivityInfo.ClientIP = GetClientIP(dictionary);//GetClientIP sets the value of IsDirectUserConnection as well.
                this.ActivityInfo.TaskName = GetTaskName(dictionary, EntryPoint);// must be executed right after GetClientIP(dictionary);
                //this.ActivityInfo.Headers = api.ControllerContext.Request.Headers;
                this.ActivityInfo.Headers = request.Headers;
                //this.ActivityInfo.Message = request.ToString() + "\n" + request.Headers.ToString();
                this.ActivityInfo.EntryPoint = EntryPoint;
                this.ActivityInfo.URI = request.RequestUri;
                //this.ActivityInfo.UrlReferrer = request..GetUrlHelper  .RequestUri;


            }
            catch { throw; }
        }


        /// <summary>
        /// Returns a Json object containing an array of relevant information to be logged.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public string GetLoggedMessage(string query)
        {
            try
            {
                string Referer = "";
                try
                {
                    Referer = ActivityInfo.Headers.Referrer.ToString();
                }
                catch { };

                string DoShowInUserHistory = "false";
                if (ActivityInfo.TaskName.ToLower().Contains("loadexplore") && (Referer.ToLower().Contains("explore") || Referer.ToLower().Contains("quicklook")))
                    DoShowInUserHistory = "true";

                foreach (string task in KeyWords.TasksInUserHistory)
                {
                    if (ActivityInfo.EntryPoint.ToLower().Contains(task.ToLower()))
                    {
                        DoShowInUserHistory = "true";
                        break;
                    }
                }

                string Message = "{ \"WebServiceEntryPoint\": \"" + ActivityInfo.EntryPoint + "\", \"TaskName\": \"" + ActivityInfo.TaskName + "\", \"DoShowInUserHistory\": \"" + DoShowInUserHistory + "\", \"SqlCommands\": \"" + query + "\", \"RequestUri\": \"" + ActivityInfo.URI + "\", \"Headers\": \"" + ActivityInfo.Headers.ToString() + "\", \"Referer\": \"" + Referer + "\"}";
                return Message;
            }
            catch { throw; }
        }


        public Uri AddTaskNameToURI(Uri uri)
        {

            if (this.IsDirectUserConnection)
            {
                if (this.dictionary.ContainsKey("TaskName"))
                    this.dictionary["TaskName"] = this.ActivityInfo.TaskName;
                else
                    this.dictionary.Add("TaskName", this.ActivityInfo.TaskName);
            }
            else
            {
                if (!this.dictionary.ContainsKey("TaskName"))
                    this.dictionary["TaskName"] = this.ActivityInfo.TaskName;
            }



            string query = "?";

            foreach (string key in dictionary.Keys)
            {
                query += key + "=" + Uri.EscapeDataString(this.dictionary[key]) + "&";
            }
            query = query.Remove(query.Length - 1);

            string URI = "";
            if (uri.Query == "")
                URI = uri.OriginalString + query;
            else
            {
                int index = uri.OriginalString.IndexOf(uri.Query, 0);
                URI = uri.OriginalString.Remove(index) + query;
            }

            return new Uri(URI);
        }


        public Dictionary<String, String> GetDict(NameValueCollection col)
        {
            Dictionary<String, String> dict = new Dictionary<string, string>();
            for (int i = 0; i < col.Count; i++)
                dict.Add(col.GetKey(i), col.GetValues(i)[0]);

            return dict;
        }

        public string GetClientIP(Dictionary<String, String> requestDir)
        {
            string clientIP = "unknown";
            try
            {
                if (!string.IsNullOrEmpty(HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"]))
                {
                    clientIP = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                    string[] addresses = clientIP.Split(',');
                    if (addresses.Length != 0)
                        clientIP = addresses[0];
                }
                else
                {
                    if (HttpContext.Current.Request.UserHostAddress != null)
                    {
                        clientIP = HttpContext.Current.Request.UserHostAddress;
                    }
                    else
                    {
                        clientIP = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
                    }
                }
                //checking whether the request came from our servers that host Skyserver website.
                foreach (string ip in KeyWords.IPClientServers)
                {
                    if (clientIP.Contains(ip))
                    {
                        this.IsDirectUserConnection = false;
                        try { clientIP = requestDir["clientIP"]; }
                        catch { clientIP = "unkownForAgent"; }
                        if (clientIP == "")
                            clientIP = "unkownForAgent";
                        break;
                    }
                }
                if (clientIP == "")
                    clientIP = "unspecified";
            }
            catch { }
            return clientIP;
        }

        public string GetTaskName(Dictionary<String, String> requestDir, string EntryPoint)
        {
            if (IsDirectUserConnection && !String.IsNullOrEmpty(EntryPoint))
                return EntryPoint;// + ".DirectQuery";
            else
            {
                string taskname = "";
                try { taskname = requestDir["TaskName"]; }
                catch { taskname = EntryPoint + ".unkownForAgent"; }
                return taskname;
            }
        }




    }





}