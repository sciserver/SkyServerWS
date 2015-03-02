using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Net.Http.Headers;
using Sciserver_webService.ExceptionFilter;
using Sciserver_webService.Common;

namespace Sciserver_webService.Controllers
{
    public class getAllSIAPInfoController : ApiController
    {
        //[ExceptionHandleAttribute]
        //public sdssSIAP.SiapTable Get([FromUri] String POS, [FromUri] String SIZE)
        //{
        //    sdssSIAP.SIAP siap = new sdssSIAP.SIAP();
        //    return siap.getSiapInfo(POS, SIZE, "ALL", "*");
        //}

        [ExceptionHandleAttribute]
        public IHttpActionResult Get()
        {

            ProcessRequest request = new ProcessRequest();
            return request.runquery(this, KeyWords.SIAP, KeyWords.getSIAPInfoAll, "SIAP:getAllSIAPInfo");
        }
      
    }
}
