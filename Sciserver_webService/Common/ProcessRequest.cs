using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Text;
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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


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
        public LoggedInfo ActivityInfo = null;
        public RequestMisc rm;

        public ProcessRequest(HttpRequestMessage request, string EntryPoint)
        {
            try
            {
                rm = new RequestMisc(request, EntryPoint);

                this.server_name = HttpContext.Current.Request.ServerVariables["SERVER_NAME"];
                this.windows_name = System.Environment.MachineName;

                this.dictionary = rm.GetDict(request.RequestUri.ParseQueryString());
                this.ClientIP = rm.GetClientIP(dictionary);//GetClientIP sets the value of IsDirectUserConnection as well.
                this.TaskName = rm.GetTaskName(dictionary, EntryPoint);// must be executed right after GetClientIP(ref dictionary);
                this.ActivityInfo = rm.ActivityInfo;
                
            }
            catch { throw; }
        }

        /// <summary>
        /// Returns a Json object containing relevant information to be logged.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>


        public IHttpActionResult runquery(ApiController api, string queryType, string positionType, string Task)
        {
            //api.Request.Headers.Add("TaskName", Task);
            //api.Request.Headers.Add("EntryPoint", Task);
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
            ExtraInfo.Add("SaveResult", dictionary.ContainsKey("SaveResult") ? dictionary["SaveResult"] : "false");// default is to show result on webpage instead of saving it to a file.

            HttpResponseMessage resp = new HttpResponseMessage();
            Logger log = (HttpContext.Current.ApplicationInstance as MvcApplication).Log;
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
                this.ClientIP = rm.GetClientIP(dictionary);//GetClientIP sets the value of IsDirectUserConnection as well.
            if (string.IsNullOrEmpty(this.TaskName)) 
                this.TaskName = rm.GetTaskName(dictionary, Task);// must be executed right after GetClientIP(ref dictionary);
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
                    ActivityInfo.Message = rm.GetLoggedMessage(ExtraInfo["QueryForUserDisplay"]);   //request.ToString();
                    return new ReturnSIAPresults(positionType, "VOTable", datarelease, dictionary, ActivityInfo); // this is tricky code
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
                    case "csv": 
                        ExtraInfo.Add("FormatFromUser", format); format = KeyWords.contentCSV; break;
                    case "xml": 
                    case "application/xml":
                        ExtraInfo.Add("FormatFromUser", format); format = KeyWords.contentXML; break;
                    case "votable": 
                    case "application/x-votable+xml":
                        ExtraInfo.Add("FormatFromUser", format); format = KeyWords.contentVOTable; break;
                    case "json":
                    case "application/json":
                        ExtraInfo.Add("FormatFromUser", format); format = KeyWords.contentJson; break;
                    case "fits": 
                    case "application/fits":
                        ExtraInfo.Add("FormatFromUser", format); format = KeyWords.contentFITS; break;
                    case "dataset": 
                    case "application/x-dataset":
                        ExtraInfo.Add("FormatFromUser", format); format = KeyWords.contentDataset; break;
                    case "html": 
                        ExtraInfo.Add("FormatFromUser", "html"); format = KeyWords.contentDataset; break;
                    default: 
                        ExtraInfo.Add("FormatFromUser", format); format = KeyWords.contentJson; break;
                }
            }
            catch (Exception exp)
            {
                if (IsDirectUserConnection)//in case the user did not specify a format
                {
                    format = KeyWords.contentJson;
                    string val;
                    if (!ExtraInfo.TryGetValue("FormatFromUser", out val))
                        ExtraInfo.Add("FormatFromUser", "json");
                }
                else
                {
                    format = KeyWords.contentDataset;//which is a dataset
                    string val;
                    if (!ExtraInfo.TryGetValue("FormatFromUser", out val))
                        ExtraInfo.Add("FormatFromUser", "dataset");
                }
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
            ActivityInfo.Message = rm.GetLoggedMessage(ExtraInfo["QueryForUserDisplay"]);   //request.ToString();

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
                    this.ClientIP = rm.GetClientIP(dictionary);//GetClientIP sets the value of IsDirectUserConnection as well.
                if (string.IsNullOrEmpty(this.TaskName))
                    this.TaskName = rm.GetTaskName(dictionary, Message);// must be executed right after GetClientIP(ref dictionary);
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
                ActivityInfo.Message = rm.GetLoggedMessage(ExtraInfo["QueryForUserDisplay"]);   //request.ToString();



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



    }
}

