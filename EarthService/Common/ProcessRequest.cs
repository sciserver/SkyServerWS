using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Sciserver_webService.ExceptionFilter;
//using Sciserver_webService.QueryTools;
using Sciserver_webService.UseCasjobs;
using Sciserver_webService.Common;
using System.IO;
using SciServer.Logging;
using System.Web;
//using Sciserver_webService.ToolsSearch;
using System.Text.RegularExpressions;
using Sciserver_webService.EarthSciData;

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
            string test = KeyWords.XAuthToken;
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

            String format = "csv";
            try
            {
                format = dictionary["format"].ToLower();
            }catch(Exception exp){
                format = "csv";
            }

            String query = "";
            
            switch (queryType)
            {
                case "SqlSearch": query = dictionary["cmd"];                     
                    break;
               

                case "EarthSciTest":
                    //EarthSciTest es = new EarthSciTest();
                    query = dictionary["Query"];   
                    break;

                default: 
                    //query = QueryTools.BuildQuery.buildQuery(queryType, dictionary, positionType);
                    break;
            }

            query = Regex.Replace(query, @"\/\*(.*\n)*\*\/", "");	// remove all multi-line comments
            query = Regex.Replace(query, @"^[ \t\f\v]*--.*\r\n", "", RegexOptions.Multiline);		// remove all isolated single-line comments
            query = Regex.Replace(query, @"--[^\r^\n]*", "");				// remove all embedded single-line comments
            query = Regex.Replace(query, @"[ \t\f\v]+", " ");				// replace multiple whitespace with single space
            query = Regex.Replace(query, @"^[ \t\f\v]*\r\n", "", RegexOptions.Multiline);			// remove empty lines
            
            OutputFormat o = new OutputFormat();
                return o.getResults(resp, query, token, casjobsMessage, format);
            
        }       

    }
}

