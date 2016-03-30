using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Http;
using System.Configuration;
using Newtonsoft.Json;
using System.IO;
using System.Text;

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
                this.ActivityInfo.EntryPoint = EntryPoint;
                this.ActivityInfo.ClientIP = GetClientIP(dictionary);//GetClientIP sets the value of IsDirectUserConnection as well.
                request.RequestUri = AddEntryPointToURI(request.RequestUri);
                this.ActivityInfo.TaskName = GetTaskName(dictionary, EntryPoint);// must be executed right after GetClientIP(dictionary);
                this.ActivityInfo.ShortTaskName = SetShortTaskName(this.ActivityInfo.TaskName);
                this.ActivityInfo.Headers = request.Headers;
                this.ActivityInfo.URI = request.RequestUri;
                this.ActivityInfo.DoShowInUserHistory = bool.Parse(DefineShowingInUserHistory(this.ActivityInfo.TaskName));
                this.ActivityInfo.Referrer = GetReferrer();
            }
            catch { throw; }
        }

        public string DefineShowingInUserHistory(string TaskName)
        {

            string DoShowInUserHistory = "false";

            if ((ActivityInfo.TaskName.ToLower().Contains("explore.summary") || ActivityInfo.TaskName.ToLower().Contains("quicklook.summary") || ActivityInfo.TaskName.ToLower().Contains("chart.image")) && !this.IsDirectUserConnection)
                DoShowInUserHistory = "true";

            foreach (string task in KeyWords.TasksInUserHistory)
            {
                if (ActivityInfo.EntryPoint.ToLower().Contains(task.ToLower()))
                {
                    DoShowInUserHistory = "true";
                    break;
                }
            }
            return DoShowInUserHistory;
        }

        public string SetShortTaskName(string TaskName)
        {
            string Application0 = TaskName;
            string Application = Application0.ToLower();

            if (Application.StartsWith("skyserverws"))// direct call to skyserverws 
            {
                Application = Application0;
            }
            else//use of skyserver tool
            {
                if (Application.Contains("sqlsearch") || Application.Contains("search.sql"))
                    Application = "SQL Search";
                else if (Application.Contains("f_sql"))
                    Application = "Image List SQL Search";
                else if (Application.Contains("radial"))
                    Application = "Radial Search";
                else if (Application.Contains("rectangular"))
                    Application = "Rectangular Search";
                else if (Application.Contains("searchform"))
                    Application = "Search Form";
                else if (Application.Contains("iqs"))
                    Application = "Imaging Query";
                else if (Application.Contains("irqs"))
                    Application = "Infrared Spectroscopy Query";
                else if (Application.Contains("sqs"))
                    Application = "Spectroscopy Query";
                else if (Application.Contains("crossid"))
                    Application = "Object Cross-ID";
                else if (Application.Contains(".explore"))
                    Application = "Explore tool";
                else if (Application.Contains(".quicklook"))
                    Application = "QuickLook tool";
                else if (Application.Contains("conesearch"))
                    Application = "Cone Search";
                else if (Application.Contains("history"))
                    Application = "User History";
                else if (Application.Contains("sdssfields"))
                    Application = "SDSS Fields";
                else if (Application.Contains("siap"))
                    Application = "SIAP";
                else if (Application.Contains("chart.image"))
                    Application = "Finding Chart";
                else if (Application.Contains("chart.list"))
                    Application = "Image List";
                else if (Application.Contains("chart.Navi"))
                    Application = "Navigate";
                else
                    Application = Application0;
            }
            return Application;
        }
        

        public string GetReferrer()
        {
            string Referer = "";
            try
            {
                Referer = ActivityInfo.Headers.Referrer.ToString();
            }
            catch { };
            if (string.IsNullOrEmpty(Referer))
            {
                try
                {
                    Referer = ActivityInfo.Headers.GetValues(ConfigurationManager.AppSettings["RefererHeaderName"]).First().ToString();
                }
                catch { }
            }
            return Referer;
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
                string Referer = GetReferrer();

                string DoShowInUserHistory = DefineShowingInUserHistory(ActivityInfo.TaskName);
                //string Message = "{ \"WebServiceEntryPoint\": \"" + ActivityInfo.EntryPoint + "\", \"TaskName\": \"" + ActivityInfo.TaskName + "\", \"DoShowInUserHistory\": \"" + DoShowInUserHistory + "\", \"SqlCommands\": \"" + query + "\", \"RequestUri\": \"" + ActivityInfo.URI + "\", \"Headers\": \"" + ActivityInfo.Headers.ToString() + "\", \"Referer\": \"" + Referer + "\"}";
                //string Message = "{ \"WebServiceEntryPoint\": \"" + ActivityInfo.EntryPoint + "\", \"TaskName\": \"" + ActivityInfo.TaskName + "\", \"DoShowInUserHistory\": \"" + DoShowInUserHistory + "\", \"SqlCommands\": \"" + query + "\", \"RequestUri\": \"" + ActivityInfo.URI + "\", \"Headers\": \"" + ActivityInfo.Headers.ToString() + "\", \"Referer\": \"" + Referer + "\"}";

                StringBuilder strbldr = new StringBuilder();
                StringWriter sw = new StringWriter(strbldr);
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    writer.WriteStartObject();
                    writer.WritePropertyName("EntryPoint");
                    writer.WriteValue(string.IsNullOrEmpty(ActivityInfo.EntryPoint) ? ActivityInfo.TaskName : ActivityInfo.EntryPoint);
                    writer.WritePropertyName("SqlCmd");
                    writer.WriteValue(query);
                    writer.WritePropertyName("RequestUri");
                    writer.WriteValue(ActivityInfo.URI);
                    writer.WritePropertyName("Referer");
                    writer.WriteValue(Referer);
                    writer.WritePropertyName("IsSkyserverUIrequest");
                    writer.WriteValue((!IsDirectUserConnection).ToString());
                    writer.WritePropertyName("Headers");
                    writer.WriteValue(ActivityInfo.Headers.ToString());
                }

                string Message = strbldr.ToString();
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


        public Uri GetUriFromNewKeys(Uri uri, Dictionary<string, string> dictionary)
        {
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



        public Uri AddEntryPointToURI(Uri uri)
        {
            if (string.IsNullOrEmpty(this.ActivityInfo.EntryPoint))
                return uri;
            else
            {
                if (!this.dictionary.ContainsKey("EntryPoint"))
                    this.dictionary.Add("EntryPoint", this.ActivityInfo.EntryPoint);
                else
                    this.dictionary["EntryPoint"] = this.ActivityInfo.EntryPoint;

                return GetUriFromNewKeys(uri, dictionary);
            }
        }

        public Dictionary<String, String> GetDict(NameValueCollection col)
        {
            Dictionary<String, String> dict = new Dictionary<string, string>();
            if (col.Count == 0)
            {
                throw new ArgumentException("Query parameters in request URI are missing.");
            }
            for (int i = 0; i < col.Count; i++)
                dict.Add(col.GetKey(i), col.GetValues(i)[0]);

            return dict;
        }

        public string GetClientIP(Dictionary<String, String> requestDir)
        {
            string HeaderClientIP = "";
            string UriClientIP = "";
            string clientIP = "";
            string SkyserverWSPath = (new Uri(ConfigurationManager.AppSettings["skyServerWS"])).LocalPath; 
            string SkyserverUrlHost = (new Uri(ConfigurationManager.AppSettings["skyServerUrl"])).Host;
            string RefererHost = "";
            string Referer = HttpContext.Current.Request.Headers["Referer"];
            if (!string.IsNullOrEmpty(HttpContext.Current.Request.Headers[ConfigurationManager.AppSettings["RefererHeaderName"]]))
                Referer = new Uri(HttpContext.Current.Request.Headers[ConfigurationManager.AppSettings["RefererHeaderName"]]).ToString();


            if (!string.IsNullOrEmpty(Referer))
                RefererHost = (new Uri(Referer)).Host;
            if (!string.IsNullOrEmpty(HttpContext.Current.Request.Headers[ConfigurationManager.AppSettings["RefererHeaderName"]]))
                RefererHost = (new Uri(HttpContext.Current.Request.Headers[ConfigurationManager.AppSettings["RefererHeaderName"]])).Host;

            try
            {
                HeaderClientIP = HttpContext.Current.Request.Headers[ConfigurationManager.AppSettings["IpHeaderName"]];
                UriClientIP = HttpContext.Current.Request.Params["clientIP"];
                if (RefererHost == SkyserverUrlHost && !Referer.ToLower().Contains(SkyserverWSPath.ToLower()))
                    IsDirectUserConnection = false;// request coming from skyserver
                else
                    IsDirectUserConnection = true;

                if ((!String.IsNullOrEmpty(HeaderClientIP) || !String.IsNullOrEmpty(UriClientIP)) && !IsDirectUserConnection)
                {
                    if (!String.IsNullOrEmpty(HeaderClientIP))
                        clientIP = HeaderClientIP;
                    else
                        clientIP = UriClientIP;
                }
                else
                {
                    clientIP = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                    string[] addresses = new string[] { };
                    try
                    {
                        addresses = clientIP.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    }
                    catch { }
                    if (addresses.Length > 0)
                        clientIP = addresses[0];

                    if (String.IsNullOrEmpty(clientIP))
                    {
                        clientIP = HttpContext.Current.Request.UserHostAddress;
                    }
                }
            }
            catch { }
            if (string.IsNullOrEmpty(clientIP))
                clientIP = "unknown";
            //return (clientIP).Substring(0, Math.Min(clientIP.Length, 50));
            return clientIP;
        }

        public string GetTaskName(Dictionary<String, String> requestDir, string EntryPoint)
        {
            if (IsDirectUserConnection)
                //return EntryPoint;// + ".DirectQuery";
                //return !string.IsNullOrEmpty(EntryPoint) ? EntryPoint : requestDir["TaskName"];// + ".DirectQuery";
                return requestDir["EntryPoint"];
            else
            {
                string taskname = "";
                try { taskname = requestDir["TaskName"]; }
                catch { taskname = EntryPoint; }
                //catch { taskname = EntryPoint + ".unkownForAgent"; }
                return taskname;
            }
        }




    }





}