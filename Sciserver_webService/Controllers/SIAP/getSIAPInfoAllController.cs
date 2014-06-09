using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Sciserver_webService.ExceptionFilter;

namespace Sciserver_webService.Controllers
{
    public class getSIAPInfoAllController : ApiController
    {
        [ExceptionHandleAttribute]
        public sdssSIAP.SiapTable Get([FromUri] String POS, [FromUri] String SIZE)
        {
            sdssSIAP.SIAP siap = new sdssSIAP.SIAP();
            return siap.getSiapInfo(POS, SIZE, "ALL", "*");
        }

        [ExceptionHandleAttribute]
        public HttpResponseMessage Get() {
            throw new ArgumentException("Not Enough paramters provided. Enter POS, SIZE.");
        }
    }
}
