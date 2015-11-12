using System;
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
using Sciserver_webService.UseCasjobs;
using Sciserver_webService.DoDatabaseQuery;

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
        public string ClientIP = "";
        public string TaskName = "";
        public string server_name = "";
        public string windows_name = "";


        public IHttpActionResult runquery(ApiController api, string queryType, string positionType, string casjobsMessage)
        {

            string datarelease = HttpContext.Current.Request.RequestContext.RouteData.Values["anything"] as string; /// which SDSS Data release is to be accessed

            DataSet ResultsDataSet = new DataSet();

            /// this is temporary read from the web.config
            string skyserverUrl = ConfigurationManager.AppSettings["skyServerUrl"]+datarelease;

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
            Dictionary<String, String> dictionary = null;
            IEnumerable<string> values;
            string token = "";
            string userid = "unknown"; ; // before knowing whether its authenticated user or unknown.
            if (api.ControllerContext.Request.Headers.TryGetValues(KeyWords.XAuthToken, out values))
            {
                try
                {
                    // Keystone authentication
                    token = values.First();
                    var userAccess = Keystone.Authenticate(token);

                    // logging for the request

                    Message message = log.CreateCustomMessage(KeyWords.loggingMessageType, api.ControllerContext.Request.ToString());
                    userid = userAccess.User.Id;
                    message.UserId = userAccess.User.Id;
                    log.SendMessage(message);
                    
                }catch(Exception e){

                    // No authentication (anonymous) // Logg
                    Message message = log.CreateCustomMessage(KeyWords.loggingMessageType, e.Message);
                    message.UserId = userid;
                    log.SendMessage(message);
                    throw new UnauthorizedAccessException("Given token is not authorized.");                    
                }

            } else {
                Message message = log.CreateCustomMessage(KeyWords.loggingMessageType, api.ControllerContext.Request.ToString());
                message.UserId = userid;
                log.SendMessage(message);
            }

            try
            {
                dictionary = api.Request.GetQueryNameValuePairs().ToDictionary(kv => kv.Key, kv => kv.Value, StringComparer.OrdinalIgnoreCase);
            }
            catch (Exception e)
            {
                Message message = log.CreateCustomMessage(KeyWords.loggingMessageType, "Check input parameters properly. Exception while creating dictionary of input parameters." + e.Message);
                message.UserId = userid;
                log.SendMessage(message);
                throw new ArgumentException("Check input parameters properly.");
            }

            String format = "";        
            String query = "";

            //string Format = "";

            this.ClientIP = GetClientIP(dictionary);//GetClientIP sets the value of IsDirectUserConnection as well.
            this.TaskName = GetTaskName(dictionary, casjobsMessage);// must be executed right after GetClientIP(ref dictionary);
            try { this.server_name = HttpContext.Current.Request.ServerVariables["SERVER_NAME"]; }catch { }
            try { this.windows_name = System.Environment.MachineName; }catch { };

            dictionary.Add("skyserverUrl", skyserverUrl);
            dictionary.Add("datarelease", drnumber);
            ExtraInfo.Add("ClientIP", ClientIP);
            ExtraInfo.Add("TaskName", TaskName);
            ExtraInfo.Add("server_name", server_name);
            ExtraInfo.Add("windows_name", windows_name);

            switch (queryType)
            {
                case "SqlSearch":
                    SqlSearch sqlsearch = new SqlSearch(dictionary, ExtraInfo);
                    query = sqlsearch.query; // here the query is wrapped inside SpExecuteSQL
                    ExtraInfo["syntax"] = sqlsearch.syntax;
                    ExtraInfo["QueryForUserDisplay"] = sqlsearch.QueryForUserDisplay;
                    ExtraInfo["query"] = query;
                    break;

                case "CrossIdSearch":
                    CrossIdSearch crossId = new CrossIdSearch(dictionary, ExtraInfo, HttpContext.Current.Request, api.Request.Content);
                    query = crossId.query; // here the query is not wrapped inside SpExecuteSQL
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
                    return new ReturnSIAPresults(casjobsMessage, "VOTable", datarelease, dictionary); // this is tricky code
                    break;

                default:
                    QueryTools.BuildQuery.buildQueryMaster(queryType, dictionary, positionType);
                    query = QueryTools.BuildQuery.query;
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


            //return new RunCasjobs( query, token, casjobsMessage, format, datarelease);
            //return new RunCasjobs(query, token, casjobsMessage, format, datarelease, DoReturnHtml);
            //return new RunCasjobs(query, token, casjobsMessage, format, datarelease, ExtraInfo);

            switch (queryType)
            {
                case "SqlSearch":
                    //return new RunCasjobs(query, token, this.TaskName, format, datarelease, ExtraInfo, this.ClientIP);// queries are sent to CasJobs
                    return new RunDBquery(query, format, TaskName, ExtraInfo);// queries are sent to CasJobs
                case "ObjectSearch":
                    return new SendTables(ResultsDataSet, format);
                default:
                    //return new RunCasjobs(query, token, this.TaskName, format, datarelease, ExtraInfo, this.ClientIP);// queries are sent to CasJobs
                    //return new RunDBquery(query, format);
                    return new RunDBquery(query, format, TaskName, ExtraInfo);
            }

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
                foreach (string ip in KeyWords.IPClientServers)
                {
                    if (ip.Contains(clientIP))
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

        public string GetTaskName(Dictionary<String, String> requestDir, string casjobsMessage)
        {
            if (IsDirectUserConnection)
                return casjobsMessage + ".DirectQuery";
            else
            {
                string taskname = "";
                try { taskname = requestDir["TaskName"]; }
                catch { taskname =  casjobsMessage + ".unkownForAgent"; }
                return taskname;
            }
        }



        /// Upload table        
        public IHttpActionResult proximityQuery(ApiController api, string queryType, string positionType, string casjobsMessage)
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
                Dictionary<String, String> dictionary = api.Request.GetQueryNameValuePairs().ToDictionary(kv => kv.Key, kv => kv.Value, StringComparer.OrdinalIgnoreCase);
                string token = "";   
                IEnumerable<string> values;

                string userid = "unknown"; ; // before knowing whether its authenticated user or unknown.
                if (api.ControllerContext.Request.Headers.TryGetValues(KeyWords.XAuthToken, out values))
                {
                    try
                    {
                        // Keystone authentication
                        token = values.First();
                        var userAccess = Keystone.Authenticate(token);

                        // logging for the request
                        Message message = log.CreateCustomMessage(KeyWords.loggingMessageType, api.ControllerContext.Request.ToString());
                        userid = userAccess.User.Id;
                        message.UserId = userAccess.User.Id;
                        log.SendMessage(message);
                    }
                    catch (Exception e)
                    {
                        // No authentication (anonymous) // Logg
                        Message message = log.CreateCustomMessage(KeyWords.loggingMessageType, e.Message);
                        message.UserId = userid;
                        log.SendMessage(message);
                        throw new UnauthorizedAccessException("Given token is not authorized.");
                    }

                }
                else
                {
                    Message message = log.CreateCustomMessage(KeyWords.loggingMessageType, api.ControllerContext.Request.ToString());
                    message.UserId = userid;
                    log.SendMessage(message);
                }


                this.ClientIP = GetClientIP(dictionary);//GetClientIP sets the value of IsDirectUserConnection as well.
                this.TaskName = GetTaskName(dictionary, casjobsMessage);// must be executed right after GetClientIP(ref dictionary);
                try { server_name = HttpContext.Current.Request.ServerVariables["SERVER_NAME"]; }
                catch { }
                try { windows_name = System.Environment.MachineName; }
                catch { };

                dictionary.Add("ClientIP", ClientIP);
                dictionary.Add("TaskName", TaskName);
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

                using (UploadDataReader up = new UploadDataReader(new StreamReader(stream), radiusDefault))
                {
                    if (stream.Length > 0)
                    {
                        query += up.UploadTo(queryType, dictionary["searchNearBy"]);
                        HasFile = true;
                    }
                    else 
                    {
                        try
                        {
                            query += up.UploadTo(dictionary["radecTextarea"], queryType, dictionary["searchNearBy"]);
                            if (dictionary["radecTextarea"].Length > 0)
                                HasRaDecText = true;
                        }
                        catch{}
                    }
                    if (!HasRaDecText && !HasFile)
                    {
                        query = "SELECT 'ERROR: Neither upload file nor list specified for Proximity search.'--";
                    }
                }

                
                //query += QueryTools.BuildQuery.buildQuery(queryType, dictionary, positionType);

                QueryTools.BuildQuery.buildQueryMaster(queryType, dictionary, positionType);
                ExtraInfo["QueryForUserDisplay"] = query + QueryTools.BuildQuery.QueryForUserDisplay;
                query += QueryTools.BuildQuery.query;


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

                //return new RunCasjobs(query, token, casjobsMessage, format, datarelease);
                //return new RunDBquery(query, format, this.TaskName, ExtraInfo);return new RunCasjobs(query, token, this.TaskName, format, datarelease, ExtraInfo, this.ClientIP);
                return new RunDBquery(query, format, this.TaskName, ExtraInfo);
            }
            catch (Exception exp)
            {
                throw new Exception("Exception while uploading data to create temp table." + exp.Message);
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

