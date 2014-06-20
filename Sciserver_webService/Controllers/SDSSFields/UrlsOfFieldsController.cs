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
    public class UrlsOfFieldsController : ApiController
    {
        [HttpGet]
        [ActionName("UrlsOfFields")]
        public String[] UrlsOfFields([FromUri] String ra, [FromUri] String dec, [FromUri] String radius, [FromUri] String band)
        {
            Validation valid = new Validation();
            Sciserver_webService.SDSSFields.SDSSFields sdssFields = new Sciserver_webService.SDSSFields.SDSSFields();
            if (valid.ValidateInput(ra, dec, radius) && valid.ValidateInput(band))
            {
                return sdssFields.UrlOfFields(valid.getRa(), valid.getDec(), valid.getRadius(), band);
            }
            throw new Exception("There is error processing your request at this time. Check your request/input parameters and try again later!");
        }
    }
}
