using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using Sciserver_webService.ExceptionFilter;
using Sciserver_webService.casjobs;

using net.ivoa.VOTable;
namespace Sciserver_webService.Controllers
{
    public class SDSSFieldsController : ApiController
    {
        [ExceptionHandleAttribute]
        public VOTABLE GetFieldArray([FromUri] String ra, [FromUri] String dec, [FromUri] String sr)
        {

        }
    }
}
