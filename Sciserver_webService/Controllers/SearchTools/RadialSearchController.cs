using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Net.Http.Headers;
using System.Net.Http.Formatting;
using Sciserver_webService.ExceptionFilter;
using Sciserver_webService.ToolsSearch;
using Sciserver_webService.Common;

namespace Sciserver_webService.Controllers
{
    public class RadialSearchController : ApiController
    {
        [ExceptionHandleAttribute]
        public HttpResponseMessage Get()
        {
            ProcessRequest request = new ProcessRequest();
            return request.runquery(this, KeyWords.RadialQuery, KeyWords.RadialQuery, "RadialSearch Tool.");
        }
    }
}

        ///**
        // * [FromUri] String ra = null, [FromUri] String dec = null, 
        //                  [FromUri] String sr = null, [FromUri] String uband = null,
        //                  [FromUri] String gband = null, [FromUri] String rband= null,
        //                  [FromUri] String iband = null, [FromUri] String zband = null,
        //                  [FromUri] String searchtype=null,[FromUri] String returntype=null,
        //                  [FromUri] String fp = "none", [FromUri] String limit = "10")
        // * /
 
