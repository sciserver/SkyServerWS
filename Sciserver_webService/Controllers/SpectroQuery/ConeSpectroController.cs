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
    public class ConeSpectroController : ApiController
    {      
        [ExceptionHandleAttribute]
        public IHttpActionResult get()
        {
            ProcessRequest request = new ProcessRequest(this.Request, "SkyserverWS.SpectroQuery.ConeSearch");
            return request.runquery(this, KeyWords.spectroQuery, KeyWords.cone, "SkyserverWS.SpectroQuery.ConeSearch");
        }

        [ExceptionHandleAttribute]
        public IHttpActionResult post()
        {
            ProcessRequest request = new ProcessRequest(this.Request, "SkyserverWS.SpectroQuery.ConeSearch");
            return request.runquery(this, KeyWords.spectroQuery, KeyWords.cone, "SkyserverWS.SpectroQuery.ConeSearch");
        }
    }
}
