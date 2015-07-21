using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
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
        public IHttpActionResult runquery(ApiController api, string queryType, string positionType, string casjobsMessage)
        {
            string datarelease = HttpContext.Current.Request.RequestContext.RouteData.Values["anything"] as string; /// which SDSS Data release is to be accessed
            
            /// this is temporary read from the web.config
            string skyserverUrl = ConfigurationManager.AppSettings["skyServerUrl"]+datarelease;

            // get data release number
            string drnumber = datarelease.ToUpper().Replace("DR","");


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
            dictionary.Add("skyserverUrl", skyserverUrl);
            dictionary.Add("datarelease", drnumber);

            switch (queryType)
            {
                case "SqlSearch": query = dictionary["cmd"];                     
                    break;

                case "RectangularSearch" :
                    RectangularSearch rs = new RectangularSearch(dictionary);                  
                    query = rs.query;
                    break;

                case "RadialSearch":
                    RadialSearch radial = new RadialSearch(dictionary);                  
                    query = radial.query;                    
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
                    break;

                case "SIAP" :
                    return new ReturnSIAPresults(casjobsMessage, "VOTable", datarelease, dictionary); // this is tricky code
                    break;

                default: 
                    query = QueryTools.BuildQuery.buildQuery(queryType, dictionary, positionType);
                    break;
            }

            query = Regex.Replace(query, @"\/\*(.*\n)*\*\/", "");	                                // remove all multi-line comments
            query = Regex.Replace(query, @"^[ \t\f\v]*--.*\r\n", "", RegexOptions.Multiline);		// remove all isolated single-line comments
            query = Regex.Replace(query, @"--[^\r^\n]*", "");				                        // remove all embedded single-line comments
            query = Regex.Replace(query, @"[ \t\f\v]+", " ");				                        // replace multiple whitespace with single space
            query = Regex.Replace(query, @"^[ \t\f\v]*\r\n", "", RegexOptions.Multiline);			// remove empty lines          
            if (queryType == "SqlSearch")
            {
                query = "EXEC spExecuteSQL '" + query + "','500000'";// parsing the query against harmful sql commands
            }
            try
            {
                if(format.Equals(""))
                 format = dictionary["format"].ToLower();

                switch (format)
                {

                    case "csv": format = KeyWords.contentCSV; break;
                    case "xml": format = KeyWords.contentXML; break;
                    case "votable": format = KeyWords.contentVOTable; break;
                    case "json": format = KeyWords.contentJson; break;
                    case "fits": format = KeyWords.contentFITS; break;
                    case "dataset": format = KeyWords.contentDataset; break;
                    
                    default: format = KeyWords.contentJson; break;
                }
            }
            catch (Exception exp)
            {
                format = KeyWords.contentCSV;
            }            
           
            return new RunCasjobs( query, token, casjobsMessage, format, datarelease);
        }
       

        /// Upload table        
        public IHttpActionResult proximityQuery(ApiController api, string queryType, string positionType, string casjobsMessage)
        {
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

                // get data release number
                string drnumber = datarelease.ToUpper().Replace("DR", "");

                dictionary.Add("datarelease", drnumber);

                var task = api.Request.Content.ReadAsStreamAsync();
                task.Wait();
                Stream stream = task.Result;

                using (UploadDataReader up = new UploadDataReader(new StreamReader(stream)))
                {
                    query += up.UploadTo(queryType, dictionary["searchNearBy"]);
                }

                
                query += QueryTools.BuildQuery.buildQuery(queryType, dictionary, positionType);
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

                        case "csv": format = KeyWords.contentCSV; break;
                        case "xml": format = KeyWords.contentXML; break;
                        case "votable": format = KeyWords.contentVOTable; break;
                        case "json": format = KeyWords.contentJson; break;
                        case "fits": format = KeyWords.contentFITS; break;
                        case "dataset": format = KeyWords.contentDataset; break;

                        default: format = KeyWords.contentJson; break;
                    }
                }
                catch (Exception exp)
                {
                    format = KeyWords.contentCSV;
                }            

                return new RunCasjobs(query, token, casjobsMessage, format, datarelease);
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

                if (dictionary["radecTextarea"] != null) {
                    UploadDataReader up = new UploadDataReader();
                    query += up.UploadTo(dictionary["radecTextarea"],queryType, dictionary["nearBy"]);
                }
                else
                {
                    var task = api.Request.Content.ReadAsStreamAsync();
                    task.Wait();
                    Stream stream = task.Result;
                 
                    using (UploadDataReader up = new UploadDataReader(new StreamReader(stream)))
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

