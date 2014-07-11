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
    public class ConeController : ApiController
    {
        [ExceptionHandleAttribute]
        public HttpResponseMessage get(String ra = null, String dec = null, String radius = null, String limit = "50", String imgparams = "", String specparams = "", String magColorType = "",
                                       String uMag="", String iMag="", String gMag="", String rMag="", String zMag="", String uColor="",String gColor="", 
                                       String iColor="", String rColor="", String zColor="", String objType="", String score="", String objFlagsON="", String objFlagsOff="") 
        {
           
             //foreach (var parameter in Request.GetQueryNameValuePairs())
            //{
            //    var key = parameter.Key;
            //    var value = parameter.Value;
            //}            
            //Validation valid = new Validation(ra, dec, sr,limit, imaging, spectroscopy, magColorType, uMag,iMag, gMag,rMag,zMag, uColor,gColor,iColor,rColor,zColor,objType,score,objFlagsON, objFlagsOff);
            //Validation valid = new Validation(ra, dec, sr);


            IEnumerable<string> values;
            if (ControllerContext.Request.Headers.TryGetValues("X-Auth-Token", out values))
            {
                // Keystone authentication
                string token = values.First();
                var userAccess = Keystone.Authenticate(token);
                
                // logging for the request
                HttpResponseMessage resp = new HttpResponseMessage();
                Dictionary<String,String> dictionary =  Request.GetQueryNameValuePairs().ToDictionary(kv => kv.Key, kv=> kv.Value, StringComparer.OrdinalIgnoreCase);
                

                //string query = ImagingQuery.ImagingQuery.buildQuery("img", dictionary);
                string query = QueryTools.BuildQuery.buildQuery("img", dictionary,"cone");
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
