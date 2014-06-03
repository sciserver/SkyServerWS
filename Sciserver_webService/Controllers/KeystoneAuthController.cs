using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Sciserver_webService.Controllers
{
    public class KeystoneAuthController : ApiController
    {
        // GET api/keystoneauth
        public HttpResponseMessage Get()
        {
            IEnumerable<string> values;
            if (ControllerContext.Request.Headers.TryGetValues("X-Auth-Token", out values))
            {
                // Keystone authentication
                string token = values.First();
                var userAccess = Keystone.Authenticate(token);
                return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, "Keystone authentication");
            }
            else
            {
                // No authentication (anonymous)
                return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, "Anonymous");
            }
        }
    }
}
