using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Sciserver_webService.ExceptionFilter;
using Sciserver_webService.QueryTools;
using Sciserver_webService.Common;

namespace Sciserver_webService.Controllers
{
    public class ProximityController : ApiController
    {
        
        [ExceptionHandleAttribute]
        public IHttpActionResult post() 
        {
            ProcessRequest request = new ProcessRequest(this.Request, "SkyserverWS.ImagingQuery.ProximitySearch");
            return request.proximityQuery(this, KeyWords.imagingQuery, KeyWords.proximity, "SkyserverWS.ImagingQuery.ProximitySearch");
        }

        [ExceptionHandleAttribute]
        public IHttpActionResult get()
        {
            ProcessRequest request = new ProcessRequest(this.Request, "SkyserverWS.ImagingQuery.ProximitySearch");
            return request.proximityQuery(this, KeyWords.imagingQuery, KeyWords.proximity, "SkyserverWS.ImagingQuery.ProximitySearch");
        }



    }
}
