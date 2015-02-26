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
using System.Web.Http;
using Sciserver_webService.Common;
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
using System.IO;
using SciServer.Logging;
using System.Web;
using Sciserver_webService.ToolsSearch;
using System.Text.RegularExpressions;
using Sciserver_webService.SIAP;
using System.Data;
using System.Threading.Tasks;

namespace Sciserver_webService.Controllers
{
    [ExceptionHandleAttribute]
    public class FieldArrayController : ApiController
    {
        [HttpGet]
        [ActionName("FieldArray")]
        [ExceptionHandleAttribute]
        public IHttpActionResult GetFieldArray([FromUri] String ra, [FromUri] String dec, [FromUri] String radius)
        {
            ProcessRequest request = new ProcessRequest();
            return request.runquery(this, KeyWords.SDSSFields, KeyWords.FieldArray, "SDSSFields:FieldArray");
        }        
    }
}
