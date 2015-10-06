using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using Sciserver_webService.ExceptionFilter;
using Sciserver_webService.casjobs;
using Sciserver_webService.SDSSFields;
using net.ivoa.VOTable;

using System.Net;
using System.Net.Http;
using Sciserver_webService.Common;

namespace Sciserver_webService.Controllers
{
    [ExceptionHandleAttribute]
    public class UrlsOfFieldsController : ApiController
    {
       
        [ExceptionHandleAttribute]
        public IHttpActionResult get()
        {
            ProcessRequest request = new ProcessRequest();
            return request.runquery(this, KeyWords.SDSSFields, KeyWords.UrlsOfFields, "SkyserverWS.SDSSFields.UrlsOfFields");
        }

        [ExceptionHandleAttribute]
        public IHttpActionResult post()
        {
            ProcessRequest request = new ProcessRequest();
            return request.runquery(this, KeyWords.SDSSFields, KeyWords.UrlsOfFields, "SkyserverWS.SDSSFields.UrlsOfFields");
        }
        ///*** input parameters are [FromUri] String ra, [FromUri] String dec, [FromUri] String radius, [FromUri] String band
    }
}
