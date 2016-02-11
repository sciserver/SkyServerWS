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
    public class ListOfFieldsController : ApiController
    {
      
        [ExceptionHandleAttribute]
        public IHttpActionResult get()
        {
            ProcessRequest request = new ProcessRequest(this.Request, "SkyserverWS.SDSSFields.ListOfFields");
            return request.runquery(this, KeyWords.SDSSFields, KeyWords.ListOfFields, "SkyserverWS.SDSSFields.ListOfFields");
        }

        [ExceptionHandleAttribute]
        public IHttpActionResult post()
        {
            ProcessRequest request = new ProcessRequest(this.Request, "SkyserverWS.SDSSFields.ListOfFields");
            return request.runquery(this, KeyWords.SDSSFields, KeyWords.ListOfFields, "SkyserverWS.SDSSFields.ListOfFields");
        }     
    }
}
