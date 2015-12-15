using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Net.Http.Headers;
using Sciserver_webService.ExceptionFilter;
using Sciserver_webService.Common;

/// deoyani@pha.jhu.edu
namespace Sciserver_webService.Controllers
{
    public class getSIAPInfoController : ApiController
    {
        ////// Get The cone search results
        //[ExceptionHandleAttribute]
        //public sdssSIAP.SiapTable Get([FromUri] String POS, [FromUri] String SIZE, [FromUri] String FORMAT, [FromUri] String bandpass)
        //{
        //    if (POS == null || FORMAT == null || bandpass == null) throw new ArgumentException("There are not enough parameters to process your request.");
        //   sdssSIAP.SIAP siap = new sdssSIAP.SIAP();
        //   return siap.getSiapInfo(POS, SIZE, FORMAT, bandpass);
        //}

        [ExceptionHandleAttribute]
        public IHttpActionResult Get()
        {
            ProcessRequest request = new ProcessRequest(this.Request, "SkyserverWS.SIAP.getSIAPInfo");
            this.Request.RequestUri = request.AddTaskNameToURI(this.Request.RequestUri);
            return request.runquery(this, KeyWords.SIAP, KeyWords.getSIAPInfo, "SkyserverWS.SIAP.getSIAPInfo");
        }
        
    }
    
}
