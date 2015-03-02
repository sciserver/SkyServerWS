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
        //[ExceptionHandleAttribute]
        ////[HttpGet]
        ////[ActionName("getSIAPInfo")]
        //public sdssSIAP.SiapTable Get([FromUri] String POS, [FromUri] String SIZE, [FromUri] String FORMAT)
        //{
        //    if (POS == null || FORMAT == null || SIZE == null) throw new ArgumentException("There are not enough parameters to process your request.Enter values for POS, FORMAT, SIZE.");
        //    sdssSIAP.SIAP siap = new sdssSIAP.SIAP();
        //    return siap.getSiapInfo(POS, SIZE, FORMAT, "");
        //}

        [ExceptionHandleAttribute]
        public IHttpActionResult Get()
        {

            ProcessRequest request = new ProcessRequest();
            return request.runquery(this, KeyWords.SIAP, KeyWords.getSIAP, "SIAP:getSIAP");
        }
    }    
}
