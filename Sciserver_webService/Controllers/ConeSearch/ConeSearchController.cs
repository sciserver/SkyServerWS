using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Net.Http.Headers;
using Sciserver_webService.ConeSearch;
using Sciserver_webService.ExceptionFilter;
using Sciserver_webService.casjobs;
using Sciserver_webService.Models;
using Sciserver_webService.UseCasjobs;
using Sciserver_webService.Common;

using net.ivoa.VOTable;

/// deoyani@pha.jhu.edu
namespace Sciserver_webService.Controllers
{
    public class ConeSearchController : ApiController
    {
        //[ExceptionHandleAttribute]      
        //public HttpResponseMessage Get() {
        //    ProcessRequest request = new ProcessRequest();
        //    return request.runquery(this, KeyWords.ConeSearchQuery, KeyWords.cone, "Cone Search.");            
        //}

        [ExceptionHandleAttribute]
        public VOTABLE Get([FromUri] String ra = null, [FromUri] String dec = null, [FromUri] String sr = null)
        {
            if (ra == null || dec == null || sr == null) throw new ArgumentException("There are not enough parameters to process your request.");
            StreamingCone streamingResponse = new StreamingCone();
            return streamingResponse.ConeSearch(ra, dec, sr);
        }

    }
    
}
