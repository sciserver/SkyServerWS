using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Sciserver_webService.ExceptionFilter;
using Sciserver_webService.QueryTools;
using Sciserver_webService.UseCasjobs;

namespace Sciserver_webService.Controllers
{
    public class ProximityIRController : ApiController
    {
        //public HttpResponseMessage get(StreamContent content, String imaging, String spectroscopy, String magColorType,
        //                               String uMag, String iMag, String gMag, String rMag, String zMag, String uColor, String gColor,
        //                               String iColor, String rColor, String zColor, String objType, String score, String objFlags)

        [ExceptionHandleAttribute]
        public HttpResponseMessage get(String inputText,String limit = "50", String imgparams = "", String specparams = "", String magColorType = "",
                                    String uMag = "", String iMag = "", String gMag = "", String rMag = "", String zMag = "", String uColor = "", String gColor = "",
                                    String iColor = "", String rColor = "", String zColor = "", String objType = "", String score = "", String flagsOnList = "", String flagsOffList = "")
        {
            IEnumerable<string> values;
            if (ControllerContext.Request.Headers.TryGetValues("X-Auth-Token", out values))
            {
                // Keystone authentication
                string token = values.First();
                var userAccess = Keystone.Authenticate(token);

                // logging for the request
                HttpResponseMessage resp = new HttpResponseMessage();
                Dictionary<String, String> dictionary = Request.GetQueryNameValuePairs().ToDictionary(kv => kv.Key, kv => kv.Value, StringComparer.OrdinalIgnoreCase);

                //string query = ImagingQuery.ImagingQuery.buildQuery("img", dictionary);
                string query = QueryTools.BuildQuery.buildQuery("img", dictionary, "proximity");
                RunCasjobs run = new RunCasjobs();
                resp.Content = new StringContent(run.postCasjobs(query, token).Content.ReadAsStringAsync().Result);
                return resp;
            }
            else
            {
                // No authentication (anonymous) // Logg
                //return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, "Anonymous");                
                throw new UnauthorizedAccessException("Check the token you are using");
            }
        }
    }
}
