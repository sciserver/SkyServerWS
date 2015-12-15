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
    public class GalacticIRController : ApiController
    {
        [ExceptionHandleAttribute]
        public IHttpActionResult get()
        {
            ProcessRequest request = new ProcessRequest(this.Request, "SkyserverWS.IRSpectroQuery.GalacticConeSearch");
            this.Request.RequestUri = request.AddTaskNameToURI(this.Request.RequestUri);
            return request.runquery(this, KeyWords.irspectroQuery, KeyWords.conelb, "SkyserverWS.IRSpectroQuery.GalacticConeSearch");
        }

        [ExceptionHandleAttribute]
        public IHttpActionResult post()
        {
            ProcessRequest request = new ProcessRequest(this.Request, "SkyserverWS.IRSpectroQuery.GalacticConeSearch");
            this.Request.RequestUri = request.AddTaskNameToURI(this.Request.RequestUri);
            return request.runquery(this, KeyWords.irspectroQuery, KeyWords.conelb, "SkyserverWS.IRSpectroQuery.GalacticConeSearch");
        }
    }
}
