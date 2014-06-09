using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using Sciserver_webService.ExceptionFilter;
using Sciserver_webService.casjobs;
using Sciserver_webService.SDSSFields;
using net.ivoa.VOTable;
namespace Sciserver_webService.Controllers
{
    [ExceptionHandleAttribute]
    public class FieldArrayRectController : ApiController
    {
        
        [HttpGet]
        [ActionName("FieldArrayRect")]
        public Field[] GetFieldsArrayRect([FromUri] String ra, [FromUri] String dec, [FromUri] String dra, [FromUri] String ddec)
        {
            Validation valid = new Validation();

            if (valid.ValidateInput(ra, dec, dra, ddec))
            {
                Sciserver_webService.SDSSFields.SDSSFields sdssFields = new Sciserver_webService.SDSSFields.SDSSFields();
                return sdssFields.FieldArrayRect(valid.getRa(), valid.getDec(), valid.getRRa(), valid.getDDec());
            }

            throw new Exception("There is error processing your request at this time. Check your input parameters and try again later!");
        }       
    }
}
