using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Net.Http.Headers;
using Sciserver_webService.ExceptionFilter;
using System.Net;
using System.Net.Http;
using Sciserver_webService.Common;

namespace Sciserver_webService.Controllers
{
    public class getSIAPController : ApiController
    {
        [ExceptionHandleAttribute]
        //[HttpGet]
        //[ActionName("getSIAPInfo")]
        public sdssSIAP.SiapTable Get([FromUri] String POS, [FromUri] String SIZE, [FromUri] String FORMAT)
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
            if (POS == null || FORMAT == null || SIZE == null) throw new ArgumentException("There are not enough parameters to process your request.Enter values for POS, FORMAT, SIZE.");
            sdssSIAP.SIAP siap = new sdssSIAP.SIAP();
            return siap.getSiapInfo(POS, SIZE, FORMAT, "");
        }

        [ExceptionHandleAttribute]
        public HttpResponseMessage Get()
        {
            throw new ArgumentException("There are not enough parameters to process your request. \n specify POS,SIZE,FORMAT and bandpass values.");
        }
        
        //[ExceptionHandleAttribute]
        //public HttpResponseMessage getSIAPInfo()
        //{
        //    ProcessRequest request = new ProcessRequest();
        //    return request.runquery(this, KeyWords.SIAP, KeyWords.getSIAPInfo, "SIAP:getSIAPInfo");
        //}
    }    
}
