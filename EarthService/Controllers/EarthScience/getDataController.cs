using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Sciserver_webService.ExceptionFilter;
using Sciserver_webService.Common;

namespace Sciserver_webService.Controllers.EarthScience
{
    public class getDataController : ApiController
    {
        [ExceptionHandleAttribute]
        public HttpResponseMessage get()
        {
            ProcessRequest request = new ProcessRequest();
            return request.runquery(this, "EarthSciTest", "GetData", "EarthSciTest Search Tool.");
        }        
    }
}
