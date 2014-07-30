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

namespace Sciserver_webService.Common
{
    public class ProcessRequest
    {
        public HttpResponseMessage runquery(ApiController api, string queryType, string positionType, string casjobsMessage)
        {
            
            IEnumerable<string> values;
            if (api.ControllerContext.Request.Headers.TryGetValues(KeyWords.XAuthToken, out values))
            {
                // Keystone authentication
                string token = values.First();
                var userAccess = Keystone.Authenticate(token);

                // logging for the request
                HttpResponseMessage resp = new HttpResponseMessage();
                Dictionary<String, String> dictionary = api.Request.GetQueryNameValuePairs().ToDictionary(kv => kv.Key, kv => kv.Value, StringComparer.OrdinalIgnoreCase);

                string query = QueryTools.BuildQuery.buildQuery(queryType, dictionary, positionType);
                RunCasjobs run = new RunCasjobs();
                resp.Content = new StringContent(run.postCasjobs(query, token, casjobsMessage).Content.ReadAsStringAsync().Result);
                return resp;
            } else {
                // No authentication (anonymous) // Logg
                throw new UnauthorizedAccessException("Check the token you are using.");
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

