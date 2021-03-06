﻿using System;
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
    public class ProximitySpectroController : ApiController
    {
        [ExceptionHandleAttribute]
        [HttpPost]
        public IHttpActionResult post()
        {
            ProcessRequest request = new ProcessRequest(this.Request, "SkyserverWS.SpectroQuery.ProximitySearch");
            return request.proximityQuery(this, KeyWords.spectroQuery, KeyWords.proximity, "SkyserverWS.SpectroQuery.ProximitySearch");
        }

        [ExceptionHandleAttribute]
        //[HttpPost]
        public IHttpActionResult get()
        {
            ProcessRequest request = new ProcessRequest(this.Request, "SkyserverWS.SpectroQuery.ProximitySearch");
            return request.proximityQuery(this, KeyWords.spectroQuery, KeyWords.proximity, "SkyserverWS.SpectroQuery.ProximitySearch");
        }
    }
}
