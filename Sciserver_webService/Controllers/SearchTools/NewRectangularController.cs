using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Net.Http.Headers;
using System.Net.Http.Formatting;
using Sciserver_webService.SearchTools;
using Sciserver_webService.ExceptionFilter;

namespace Sciserver_webService.Controller
{
    public class NewRectangularController : ApiController
    {
        [ExceptionHandleAttribute]
        public HttpResponseMessage get([FromUri] String ra = null, [FromUri] String dec = null, [FromUri] String ra2 = null,
                            [FromUri] String dec2 = null, [FromUri] String uband = null, [FromUri] String gband = null,
                            [FromUri] String rband = null, [FromUri] String iband = null, [FromUri] String zband = null,
                            [FromUri] String searchtype = null, [FromUri] String returntype = "json")
        {
            IEnumerable<string> values;
            if (ControllerContext.Request.Headers.TryGetValues("X-Auth-Token", out values))
            {
                // Keystone authentication
                string token = values.First();
                var userAccess = Keystone.Authenticate(token);
                
                // logging for the request
                HttpResponseMessage resp = new HttpResponseMessage();
                NewRectangular rs = new NewRectangular(ra, dec, ra2, dec2, uband, gband, rband, iband, zband, searchtype, returntype, token);

                //resp.Content = new StringContent(rs.getJSONstring());
                //resp.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                //HttpResponseMessage resp = rs.getResponse();
                
                resp.Content = new StringContent(rs.getResponse().Content.ReadAsStringAsync().Result);
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
