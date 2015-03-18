using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Sciserver_webService.ExceptionFilter;
using Sciserver_webService.QueryTools;
using Sciserver_webService.UseCasjobs;
using Sciserver_webService.Common;

namespace Sciserver_webService.Controllers
{
    public class ConeIRController : ApiController
    {      
       [ExceptionHandleAttribute]
        public IHttpActionResult get()
        {
            ProcessRequest request = new ProcessRequest();
            return request.runquery(this, KeyWords.irspectroQuery, KeyWords.cone, "IRSpectroQuery:ConeSearch");
        }

       [ExceptionHandleAttribute]
       public IHttpActionResult post()
       {
           ProcessRequest request = new ProcessRequest();
           return request.runquery(this, KeyWords.irspectroQuery, KeyWords.cone, "IRSpectroQuery:ConeSearch");
       }
    }
}
