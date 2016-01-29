using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Net.Http.Headers;
using System.Net.Http.Formatting;
using Sciserver_webService.ExceptionFilter;
using net.ivoa.VOTable;
using Sciserver_webService.Common;
using SciServer.Logging;
using System.Web;
using Newtonsoft.Json;
using System.IO;
using System.Text;

// this is  web service version of Rectangular Search tool on the Skyserver web site.
// It takes two corners of search area in terms of ra and dec and returns result with SDSS objectids
//@deoyani@pha.jhu.edu
namespace Sciserver_webService.Controllers
{
    public class RectangularSearchController : ApiController
    {
        ///// <summary>
        ///// This is using new REST casjobs
        ///// </summary>
        ///// <returns></returns>        
        //[ExceptionHandleAttribute]
        //public IHttpActionResult Get([FromUri] String min_ra = null, [FromUri] String min_dec = null, [FromUri] String max_ra = null,
        //                    [FromUri] String max_dec = null, [FromUri] String uband = null, [FromUri] String gband = null,
        //                    [FromUri] String rband = null, [FromUri] String iband = null, [FromUri] String zband = null,
        //                      [FromUri] String whichway = null,[FromUri] String whichquery =null, [FromUri] String format = "json", [FromUri] String limit = "10" )
        //{

        //    ProcessRequest request = new ProcessRequest();
        //    return request.runquery(this, KeyWords.RectangularQuery, KeyWords.RectangularQuery, "RectangularSearch Tool.");
        //}
        [ExceptionHandleAttribute]
        public IHttpActionResult Get()
        {
            ProcessRequest request = new ProcessRequest(this.Request, "SkyserverWS.SearchTools.RectangularSearch");
            this.Request.RequestUri = request.AddTaskNameToURI(this.Request.RequestUri);
            return request.runquery(this, KeyWords.RectangularQuery, KeyWords.RectangularQuery, "SkyserverWS.SearchTools.RectangularSearch");
        }

        [ExceptionHandleAttribute]
        public IHttpActionResult Post()
        {
            ProcessRequest request = new ProcessRequest(this.Request, "SkyserverWS.SearchTools.RectangularSearch");
            this.Request.RequestUri = request.AddTaskNameToURI(this.Request.RequestUri);
            return request.runquery(this, KeyWords.RectangularQuery, KeyWords.RectangularQuery, "SkyserverWS.SearchTools.RectangularSearch");
        }
    }
}


