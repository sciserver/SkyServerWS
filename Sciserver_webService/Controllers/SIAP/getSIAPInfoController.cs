using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Net.Http.Headers;

using Sciserver_webService.ExceptionFilter;

/// deoyani@pha.jhu.edu
namespace Sciserver_webService.Controllers
{
    public class getSIAPInfoController : ApiController
    {
        //// Get The cone search results
        [ExceptionHandleAttribute]
        //[HttpGet]
        //[ActionName("getSIAPInfo")]
        public sdssSIAP.SiapTable Get([FromUri] String POS, [FromUri] String SIZE, [FromUri] String FORMAT, [FromUri] String bandpass)
        {

            //    // Keystone authentication
            //    string token = values.First();
            //    var userAccess = Keystone.Authenticate(token);
            //    return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, "Keystone authentication");
            //}
            //else
            //{
            //    // No authentication (anonymous)
            //    return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, "Anonymous");
            //}
           if (POS == null || FORMAT == null || bandpass == null) throw new ArgumentException("There are not enough parameters to process your request.");
           sdssSIAP.SIAP siap = new sdssSIAP.SIAP();
           return siap.getSiapInfo(POS, SIZE, FORMAT, bandpass);
        }
        [ExceptionHandleAttribute]
        public HttpResponseMessage Get()
        {
            throw new ArgumentException("There are not enough parameters to process your request. \n specify POS,SIZE,FORMAT and bandpass values.");
        }
        
    }
    
}
