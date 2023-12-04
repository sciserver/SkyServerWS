using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Net.Http.Headers;
using System.Net.Http.Formatting;
using Sciserver_webService.ExceptionFilter;
using net.ivoa.VOTable;
using Sciserver_webService.Common;
using SciServer.Logging;
using System.Web;
using Newtonsoft.Json;
using System.IO;
using System.Text;
using Sciserver_webService.Models;


using System.Configuration;
using System.Data;
using Sciserver_webService.ToolsSearch;

namespace Sciserver_webService.Controllers
{
    public class DBStateController : ApiController
    {
        [ExceptionHandleAttribute]
        public IHttpActionResult get()
        {
            return new DatabaseState();
        }
    }
}
