using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Http;
using SciServer.Logging;
using Sciserver_webService.SDSSFields;
using Sciserver_webService.sdssSIAP;
using Sciserver_webService.ToolsSearch;
using Sciserver_webService.DoDatabaseQuery;
//using Sciserver_webService.SciserverLog;
using System.Web.Http.Filters;

namespace Sciserver_webService.Common
{
    public class ProcessRequest
    {
        
        /// <summary>
        ///  this is common for most of the tools from ImagingQuery to IRSpectraQuery
        /// </summary>
        /// <param name="api"></param>
        /// <param name="queryType"></param>
        /// <param name="positionType"></param>
        /// <param name="casjobsMessage"></param>
        /// <returns></returns>
        /// 

        public bool IsDirectUserConnection = true;
        public string ClientIP = null;
        public string TaskName = null;
        public string server_name = null;
        public string windows_name = null;
        public Dictionary<String, String> dictionary = null;
        public System.Net.Http.Headers.HttpRequestHeaders Headers = null;
        public LoggedInfo ActivityInfo = null;
                

        public ProcessRequest()
        {

        }


        public ProcessRequest(HttpRequestMessage request, string EntryPoint)
        {
            try
            {

                //this.dictionary = request.GetQueryNameValuePairs().ToDictionary(kv => kv.Key, kv => kv.Value, StringComparer.OrdinalIgnoreCase);
                this.dictionary = GetDict(request.RequestUri.ParseQueryString());
                this.ClientIP = GetClientIP(dictionary);//GetClientIP sets the value of IsDirectUserConnection as well.
                this.TaskName = GetTaskName(dictionary, EntryPoint);// must be executed right after GetClientIP(ref dictionary);
                this.server_name = HttpContext.Current.Request.ServerVariables["SERVER_NAME"];
                this.windows_name = System.Environment.MachineName;
                this.Headers = request.Headers;

                this.ActivityInfo = new LoggedInfo();
                this.ActivityInfo.ClientIP = this.ClientIP;
                this.ActivityInfo.TaskName = this.TaskName;
                //this.ActivityInfo.Headers = api.ControllerContext.Request.Headers;
                this.ActivityInfo.Headers = request.Headers;
                this.ActivityInfo.EntryPoint = EntryPoint;
                this.ActivityInfo.URI = request.RequestUri;
            }
            catch { throw; }
        }

        /// <summary>
        /// Returns a Json object containing relevant information to be logged.
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

        public IHttpActionResult runquery(ApiController api, string queryType, string positionType, string Task)
        {
            api.Request.Headers.Add("TaskName", Task);
            string datarelease = HttpContext.Current.Request.RequestContext.RouteData.Values["anything"] as string; /// which SDSS Data release is to be accessed

            DataSet ResultsDataSet = new DataSet();

            /// this is temporary read from the web.config
            string skyserverUrl = ConfigurationManager.AppSettings["skyServerUrl"];

            // get data release number
            string drnumber = datarelease.ToUpper().Replace("DR","");

            // This dict stores extra info needed for running and rendering the query results.
            Dictionary<string, string> ExtraInfo = new Dictionary<string,string>();
            ExtraInfo.Add("fp", "");
            ExtraInfo.Add("syntax", "");
            ExtraInfo.Add("QueryForUserDisplay", "");
            ExtraInfo.Add("query", "");

            HttpResponseMessage resp = new HttpResponseMessage();
            Logger log = (HttpContext.Current.ApplicationInstance as MvcApplication).Log;
            IEnumerable<string> values;
            string token = "";
            string userid = "unknown"; ; // before knowing whether its authenticated user or unknown.

            try
            {
                if(dictionary == null)
                    dictionary = api.Request.GetQueryNameValuePairs().ToDictionary(kv => kv.Key, kv => kv.Value, StringComparer.OrdinalIgnoreCase);
            }
            catch (Exception e)
            {
                throw new ArgumentException("Check input parameters properly.\n"+e.Message);
            }

            String format = "";        
            String query = "";

            if (string.IsNullOrEmpty(this.ClientIP))
                this.ClientIP = GetClientIP(dictionary);//GetClientIP sets the value of IsDirectUserConnection as well.
            if (string.IsNullOrEmpty(this.TaskName)) 
                this.TaskName = GetTaskName(dictionary, Task);// must be executed right after GetClientIP(ref dictionary);
            if (string.IsNullOrEmpty(this.server_name)) 
                try { this.server_name = HttpContext.Current.Request.ServerVariables["SERVER_NAME"]; }
                catch { }
            if (string.IsNullOrEmpty(this.windows_name)) 
                try { this.windows_name = System.Environment.MachineName; }
                catch { };

            dictionary.Add("skyserverUrl", skyserverUrl);
            dictionary.Add("datarelease", drnumber);
            ExtraInfo.Add("ClientIP", ClientIP);
            ExtraInfo.Add("TaskName", TaskName);
            ExtraInfo.Add("server_name", server_name);
            ExtraInfo.Add("windows_name", windows_name);

            switch (queryType)
            {

                case "UserHistory":
                    UserHistory history = new UserHistory(dictionary, ExtraInfo, HttpContext.Current.Request);
                    ResultsDataSet = history.ResultDataSet;
                    ExtraInfo["QueryForUserDisplay"] = history.query;
                    ExtraInfo["query"] = history.query;
                    //DataTable dt = new DataTable();
                    //dt.Columns.Add("query", typeof(string));
                    //dt.Rows.Add(new object[] { history.query });
                    //ResultsDataSet.Merge(dt);
                    break;

                case "SqlSearch":
                    SqlSearch sqlsearch = new SqlSearch(dictionary, ExtraInfo);
                    query = sqlsearch.query; // here the query is wrapped inside SpExecuteSQL, both logging and security checking
                    ExtraInfo["syntax"] = sqlsearch.syntax;
                    ExtraInfo["QueryForUserDisplay"] = sqlsearch.QueryForUserDisplay;
                    ExtraInfo["query"] = query;
                    break;

                case "CrossIdSearch":
                    CrossIdSearch crossId = new CrossIdSearch(dictionary, ExtraInfo, HttpContext.Current.Request, api.Request.Content);
                    query = crossId.query; // // here the query is wrapped inside SpExecuteSQL, both logging and security checking of the user query part.
                    //Format = crossId.Format;
                    ExtraInfo["QueryForUserDisplay"] = crossId.QueryForUserDisplay;
                    ExtraInfo["query"] = query;
                    break;

                case "ObjectSearch":// here, multiple queries might be run in order to resolve the object. That's why we have to run them here and get the dataset immediately (no routing to RunDBquery but to SendTables);
                    ObjectSearch objectSearch = new ObjectSearch(dictionary, ExtraInfo, HttpContext.Current.Request);
                    ResultsDataSet = objectSearch.ResultDataSet;
                    //Format = objectSearch.Format;
                    break;

                case "RectangularSearch":
                    RectangularSearch rectangular = new RectangularSearch(dictionary, ExtraInfo);
                    query = rectangular.query;
                    ExtraInfo["QueryForUserDisplay"] = rectangular.QueryForUserDisplay;
                    ExtraInfo["query"] = query;
                    break;

                case "RadialSearch":
                    RadialSearch radial = new RadialSearch(dictionary, ExtraInfo);
                    query = radial.query;
                    ExtraInfo["fp"] = radial.fp;
                    ExtraInfo["QueryForUserDisplay"] = radial.QueryForUserDisplay;
                    ExtraInfo["query"] = query;
                    break;

                case "ConeSearch":
                    try
                    {
                        format = dictionary["format"].ToLower();
                        if (format.Equals("votable")) format = "dataset";
                    }
                    catch (Exception e)
                    {
                        format = "dataset"; // or votable
                    }
                    ConeSearch.ConeSearch cs = new ConeSearch.ConeSearch(dictionary);
                    query = cs.getConeSearchQuery();
                    ExtraInfo["QueryForUserDisplay"] = query;
                    ExtraInfo["query"] = query;
                    break;

                case "SDSSFields":
                    try
                    {
                        format = dictionary["format"].ToLower();
                        if (format.Equals("votable")) format = "dataset";
                    }
                    catch (Exception e)
                    {
                        format = "dataset";
                    }
                    NewSDSSFields sf = new NewSDSSFields(dictionary, positionType);
                    query = sf.sqlQuery;
                    ExtraInfo["QueryForUserDisplay"] = query;
                    ExtraInfo["query"] = query;
                    break;

                case "SIAP" :
                    return new ReturnSIAPresults(positionType, "VOTable", datarelease, dictionary); // this is tricky code
                    break;

                default:// runs all the Imaging, Spectro and SpectroIR queries in SkyServer
                    QueryTools.BuildQuery.buildQueryMaster(queryType, dictionary, positionType);
                    query = QueryTools.BuildQuery.query;
                    query = query.Replace("'", "''");
                    query = "EXEC spExecuteSQL '" + query + "', @webserver='" + this.server_name + "', @winname='" + this.windows_name + "', @clientIP='" + this.ClientIP + "', @access='" + this.TaskName + "', @filter=0, @log=1";
                    ExtraInfo["QueryForUserDisplay"] = QueryTools.BuildQuery.QueryForUserDisplay;
                    ExtraInfo["query"] = query;
                    break;
            }

            query = Regex.Replace(query, @"\/\*(.*\n)*\*\/", "");	                                // remove all multi-line comments
            query = Regex.Replace(query, @"^[ \t\f\v]*--.*\r\n", "", RegexOptions.Multiline);		// remove all isolated single-line comments
            query = Regex.Replace(query, @"--[^\r^\n]*", "");				                        // remove all embedded single-line comments
            query = Regex.Replace(query, @"[ \t\f\v]+", " ");				                        // replace multiple whitespace with single space
            query = Regex.Replace(query, @"^[ \t\f\v]*\r\n", "", RegexOptions.Multiline);			// remove empty lines          
            try
            {
                if(format.Equals(""))
                 format = dictionary["format"].ToLower();

                switch (format)
                {
                    case "txt": 
                    case "text/plain":
                        format = KeyWords.contentCSV; ExtraInfo.Add("FormatFromUser", format); break;
                    case "csv": 
                        format = KeyWords.contentCSV; ExtraInfo.Add("FormatFromUser", format); break;
                    case "xml": 
                    case "application/xml":
                        format = KeyWords.contentXML; ExtraInfo.Add("FormatFromUser", format); break;
                    case "votable": 
                    case "application/x-votable+xml":
                        format = KeyWords.contentVOTable; ExtraInfo.Add("FormatFromUser", format); break;
                    case "json":
                    case "application/json":
                        format = KeyWords.contentJson; ExtraInfo.Add("FormatFromUser", format); break;
                    case "fits": 
                    case "application/fits":
                        format = KeyWords.contentFITS; ExtraInfo.Add("FormatFromUser", format); break;
                    case "dataset": 
                    case "application/x-dataset":
                        format = KeyWords.contentDataset; ExtraInfo.Add("FormatFromUser", format); break;
                    case "html": 
                        format = KeyWords.contentDataset; ExtraInfo.Add("FormatFromUser", "html"); break;
                    default: 
                        format = KeyWords.contentJson; ExtraInfo.Add("FormatFromUser", format); break;
                }
            }
            catch (Exception exp)
            {
                if (IsDirectUserConnection)//in case the user did not specify a format
                {
                    format = KeyWords.contentJson;
                    ExtraInfo.Add("FormatFromUser", KeyWords.contentJson);
                }
                else
                    format = KeyWords.contentDataset;//which is a dataset
            }


            //logging -----------------------------------------------------------------------------------------------------------------------
            if (ActivityInfo == null)
            {
                ActivityInfo = new LoggedInfo();
                ActivityInfo.ClientIP = ClientIP;
                ActivityInfo.TaskName = TaskName;
                ActivityInfo.Headers = api.ControllerContext.Request.Headers;
            }

            //creating the message that is being logged
            ActivityInfo.Message = GetLoggedMessage(ExtraInfo["QueryForUserDisplay"]);   //request.ToString();

            switch (queryType)
            {
                case "SqlSearch":
                    return new RunDBquery(query, format, TaskName, ExtraInfo, ActivityInfo, queryType, positionType);// queries are sent through direct database connection.
                case "ObjectSearch":
                case "UserHistory":
                    return new SendTables(ResultsDataSet, format, ActivityInfo, ExtraInfo);
                default:
                    return new RunDBquery(query, format, TaskName, ExtraInfo, ActivityInfo, queryType, positionType);
            }

        }


        /// Upload table        
        public IHttpActionResult proximityQuery(ApiController api, string queryType, string positionType, string Message)
        {
            // This dict stores extra info needed for running and rendering the query results.
            Dictionary<string, string> ExtraInfo = new Dictionary<string, string>();
            ExtraInfo.Add("fp", "");
            ExtraInfo.Add("syntax", "");
            ExtraInfo.Add("QueryForUserDisplay", "");


            try
            {
                string datarelease = HttpContext.Current.Request.RequestContext.RouteData.Values["anything"] as string; /// which SDSS Data release is to be accessed

                /// 
                HttpResponseMessage resp = new HttpResponseMessage();
                Logger log = (HttpContext.Current.ApplicationInstance as MvcApplication).Log;
                String query = "";
                try
                {
                    if (dictionary == null)
                        dictionary = api.Request.GetQueryNameValuePairs().ToDictionary(kv => kv.Key, kv => kv.Value, StringComparer.OrdinalIgnoreCase);
                }
                catch (Exception e)
                {
                    throw new ArgumentException("Check input parameters properly.\n" + e.Message);
                }


                if (string.IsNullOrEmpty(this.ClientIP))                
                    this.ClientIP = GetClientIP(dictionary);//GetClientIP sets the value of IsDirectUserConnection as well.
                if (string.IsNullOrEmpty(this.TaskName))
                    this.TaskName = GetTaskName(dictionary, Message);// must be executed right after GetClientIP(ref dictionary);
                if (string.IsNullOrEmpty(this.server_name)) 
                    try { server_name = HttpContext.Current.Request.ServerVariables["SERVER_NAME"]; }
                    catch { }
                if (string.IsNullOrEmpty(this.windows_name))
                    try { windows_name = System.Environment.MachineName; }
                    catch { };

                dictionary.Add("server_name", server_name);
                dictionary.Add("windows_name", windows_name);

                
                // get data release number
                string drnumber = datarelease.ToUpper().Replace("DR", "");

                dictionary.Add("datarelease", drnumber);

                var task = api.Request.Content.ReadAsStreamAsync();
                task.Wait();
                Stream stream = task.Result;

                bool HasFile = false;
                bool HasRaDecText = false;

                string radiusDefault = "1";// in arcminutes
                try { radiusDefault = float.Parse(dictionary["radiusDefault"]).ToString(); }
                catch { }

                try
                {
                    using (UploadDataReader up = new UploadDataReader(new StreamReader(stream), radiusDefault))
                    {
                        if (stream.Length > 0)
                        {
                            try
                            {
                                query += up.UploadTo(queryType, dictionary["searchNearBy"]);
                                HasFile = true;
                            }
                            catch { }
                        
                        }
                        else
                        {
                            try
                            {
                                query += up.UploadTo(dictionary["radecTextarea"], queryType, dictionary["searchNearBy"]);
                                if (dictionary["radecTextarea"].Length > 0)
                                    HasRaDecText = true;
                            }
                            catch { }
                        }
                        if (!HasRaDecText && !HasFile)
                        {
                            //query = "SELECT 'ERROR: Neither upload file nor list specified for Proximity search.'--";
                            throw new ArgumentException("Neither upload file nor list specified for Proximity search.");
                        }
                    }
                }
                catch { throw; }
                
                //query += QueryTools.BuildQuery.buildQuery(queryType, dictionary, positionType);

                QueryTools.BuildQuery.buildQueryMaster(queryType, dictionary, positionType);
                ExtraInfo["QueryForUserDisplay"] = query + QueryTools.BuildQuery.QueryForUserDisplay;
                query += QueryTools.BuildQuery.query;
                query = query.Replace("'", "''");
                query = "EXEC spExecuteSQL '" + query + "', @webserver='" + this.server_name + "', @winname='" + this.windows_name + "', @clientIP='" + this.ClientIP + "', @access='" + this.TaskName + "', @filter=0, @log=1";

                //RunCasjobs run = new RunCasjobs();
                //resp.Content = new StringContent(run.postCasjobs(query, token, casjobsMessage).Content.ReadAsStringAsync().Result);
                //return resp;
                String format = "";   
                try
                {
                    if (format.Equals(""))
                        format = dictionary["format"].ToLower();

                    switch (format)
                    {

                        case "csv": format = KeyWords.contentCSV; ExtraInfo.Add("FormatFromUser", format); break;
                        case "xml": format = KeyWords.contentXML; ExtraInfo.Add("FormatFromUser", format); break;
                        case "votable": format = KeyWords.contentVOTable; ExtraInfo.Add("FormatFromUser", format); break;
                        case "json": format = KeyWords.contentJson; ExtraInfo.Add("FormatFromUser", format); break;
                        case "fits": format = KeyWords.contentFITS; ExtraInfo.Add("FormatFromUser", format); break;
                        case "dataset": format = KeyWords.contentDataset; ExtraInfo.Add("FormatFromUser", format); break;
                        case "html": format = KeyWords.contentDataset; ExtraInfo.Add("FormatFromUser", "html"); break;

                        default: format = KeyWords.contentJson; ExtraInfo.Add("FormatFromUser", format); break;
                    }
                }
                catch (Exception exp)
                {
                    format = KeyWords.contentCSV;
                    ExtraInfo.Add("FormatFromUser", KeyWords.contentCSV);
                }


                //logging -----------------------------------------------------------------------------------------------------------------------
                if (ActivityInfo == null)
                {
                    ActivityInfo = new LoggedInfo();
                    ActivityInfo.ClientIP = ClientIP;
                    ActivityInfo.TaskName = TaskName;
                    ActivityInfo.Headers = api.ControllerContext.Request.Headers;
                }

                //creating the message that is being logged
                ActivityInfo.Message = GetLoggedMessage(ExtraInfo["QueryForUserDisplay"]);   //request.ToString();



                //return new RunCasjobs(query, token, casjobsMessage, format, datarelease);
                //return new RunDBquery(query, format, this.TaskName, ExtraInfo);return new RunCasjobs(query, token, this.TaskName, format, datarelease, ExtraInfo, this.ClientIP);
                return new RunDBquery(query, format, this.TaskName, ExtraInfo, ActivityInfo, queryType, positionType);
            }
            catch (Exception exp)
            {
                throw new Exception("Error while uploading coordinates to create a temporary table. " + exp.Message);
            }
        }


        /// Upload table        
        public HttpResponseMessage uploadTest(ApiController api, string queryType, string positionType, string casjobsMessage)
        {
            try
            {
                IEnumerable<string> values;
                if (api.ControllerContext.Request.Headers.TryGetValues(KeyWords.XAuthToken, out values))
                {
                // Keystone authentication
                string token = values.First();
                var userAccess = Keystone.Authenticate(token);

                Dictionary<String, String> dictionary = api.Request.GetQueryNameValuePairs().ToDictionary(kv => kv.Key, kv => kv.Value, StringComparer.OrdinalIgnoreCase);
                String query = "";

                string radiusDefault = "1";// in arcminutes
                try { radiusDefault = float.Parse(dictionary["radiusDefault"]).ToString(); }
                catch { }

                if (dictionary["radecTextarea"] != null) {
                    UploadDataReader up = new UploadDataReader(radiusDefault);
                    query += up.UploadTo(dictionary["radecTextarea"],queryType, dictionary["nearBy"]);
                }
                else
                {
                    var task = api.Request.Content.ReadAsStreamAsync();
                    task.Wait();
                    Stream stream = task.Result;
                 
                    using (UploadDataReader up = new UploadDataReader(new StreamReader(stream), radiusDefault))
                    {
                        query += up.UploadTo(queryType, dictionary["nearBy"]);
                    }
                }

                HttpResponseMessage resp = new HttpResponseMessage();
                query += QueryTools.BuildQuery.buildQuery(queryType, dictionary, positionType);
                //RunCasjobs run = new RunCasjobs();
                //resp.Content = new StringContent(run.postCasjobs(query, token, casjobsMessage).Content.ReadAsStringAsync().Result);
                return resp;                
                }
                else
                {
                    // No authentication (anonymous) // Logg
                    throw new UnauthorizedAccessException("Check the token you are using.");
                }
            }
            catch(Exception exp){
                throw new Exception("Exception while uploading data to create temp table."+exp.Message);
            }
        }



        public Uri AddTaskNameToURI(Uri uri)
        {
            string Task = this.TaskName;

            if (this.IsDirectUserConnection)
            {
                if (this.dictionary.ContainsKey("TaskName"))
                    this.dictionary["TaskName"] = this.TaskName;
                else
                    this.dictionary.Add("TaskName", this.TaskName);
            }
            else
            {
                if (!this.dictionary.ContainsKey("TaskName"))
                    this.dictionary["TaskName"] = this.TaskName;
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

