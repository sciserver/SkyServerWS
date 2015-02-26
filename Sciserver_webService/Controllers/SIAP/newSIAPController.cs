using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Sciserver_webService.ExceptionFilter;
using Sciserver_webService.Common;

namespace Sciserver_webService.Controllers
{
    public class newSIAPController : ApiController
    {
        [ExceptionHandleAttribute]
        public IHttpActionResult Get() {

            ProcessRequest request = new ProcessRequest();
            return request.runquery(this, KeyWords.SIAP, KeyWords.getSIAP, "SIAP:newSIAP");
        }
    }
}
