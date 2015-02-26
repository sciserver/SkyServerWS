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

        [HttpGet]
        [ActionName("ListOfFields")]
        [ExceptionHandleAttribute]
        public IHttpActionResult ListOfFields()
        {
            ProcessRequest request = new ProcessRequest();
            return request.runquery(this, KeyWords.SDSSFields, KeyWords.ListOfFields, "SDSSFields:ListOfFields");
        }
        //public String[] ListOfFields([FromUri] String ra, [FromUri] String dec, [FromUri] String radius)
        //{
        //    Validation valid = new Validation();
        //    if (valid.ValidateInput(ra, dec, radius))
        //    {

        //        Sciserver_webService.SDSSFields.SDSSFields sdssFields = new Sciserver_webService.SDSSFields.SDSSFields();
        //        return sdssFields.ListOfFields(valid.getRa(), valid.getDec(), valid.getRadius());
        //    }

        //    throw new Exception("There is error processing your request at this time. Check your request/input parameters and try again later!");

        //}

     
    }
}
