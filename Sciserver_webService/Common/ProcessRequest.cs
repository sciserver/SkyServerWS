using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Sciserver_webService.ExceptionFilter;
using Sciserver_webService.QueryTools;
using Sciserver_webService.UseCasjobs;
using Sciserver_webService.Common;
using System.IO;
using SciServer.Logging;
using System.Web;
using Sciserver_webService.ToolsSearch;


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
        public HttpResponseMessage runquery(ApiController api, string queryType, string positionType, string casjobsMessage)
        {
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

            String format = "CSV";
            try
            {
                format = dictionary["format"];
            }catch(Exception exp){
                format = "CSV";
            }

            String query = "";
            
            switch (queryType)
            {
                case "SqlSearch": query = dictionary["query"];break;
                case "RectangularSearch" :
                    RectangularSearch rs = new RectangularSearch(dictionary);
                    resp.Content = new StringContent(rs.getResult(token, casjobsMessage, format));
                    return resp;
                    break;
                case "RadialSearch":
                    RadialSearch radial = new RadialSearch(dictionary);
                    resp.Content = new StringContent(radial.getResult(token, casjobsMessage, format));
                    return resp;
                    break;
                case "ConeSearch":
                    Sciserver_webService.ConeSearch.ConeSearch cs = new Sciserver_webService.ConeSearch.ConeSearch(dictionary);
                    query = cs.getConeSearchQuery();
                    break;
                default: 
                    query = QueryTools.BuildQuery.buildQuery(queryType, dictionary, positionType);
                    break;
            }
         
            RunCasjobs run = new RunCasjobs();
            resp.Content = new StringContent(run.postCasjobs(query, token, casjobsMessage).Content.ReadAsStringAsync().Result);
            return resp;
        }
       

        /// Upload table        
        public HttpResponseMessage proximityQuery(ApiController api, string queryType, string positionType, string casjobsMessage)
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

                    //if (dictionary["radecTextarea"] != null)
                    //{
                    //    UploadDataReader up = new UploadDataReader();
                    //    query += up.UploadTo(dictionary["radecTextarea"], queryType, dictionary["nearBy"]);
                    //}
                    //else
                    //{
                        var task = api.Request.Content.ReadAsStreamAsync();
                        task.Wait();
                        Stream stream = task.Result;

                        using (UploadDataReader up = new UploadDataReader(new StreamReader(stream)))
                        {
                            query += up.UploadTo(queryType, dictionary["searchNearBy"]);
                        }
                    //}

                    HttpResponseMessage resp = new HttpResponseMessage();
                    query += QueryTools.BuildQuery.buildQuery(queryType, dictionary, positionType);
                    RunCasjobs run = new RunCasjobs();
                    resp.Content = new StringContent(run.postCasjobs(query, token, casjobsMessage).Content.ReadAsStringAsync().Result);
                    return resp;
                }
                else
                {
                    // No authentication (anonymous) // Logg
                    throw new UnauthorizedAccessException("Check the token you are using.");
                }
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
                RunCasjobs run = new RunCasjobs();
                resp.Content = new StringContent(run.postCasjobs(query, token, casjobsMessage).Content.ReadAsStringAsync().Result);
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

