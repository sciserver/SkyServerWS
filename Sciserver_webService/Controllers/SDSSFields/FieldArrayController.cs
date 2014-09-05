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

namespace Sciserver_webService.Controllers
{
    [ExceptionHandleAttribute]
    public class FieldArrayController : ApiController
    {


        [HttpGet]
        [ActionName("FieldArray")]
        [ExceptionHandleAttribute]
        public HttpResponseMessage GetFieldArray([FromUri] String ra, [FromUri] String dec, [FromUri] String radius)
        {
            ProcessRequest request = new ProcessRequest();
            return request.runquery(this, KeyWords.SDSSFields, KeyWords.FieldArray, "SDSSFields:FieldArray");
        }


        
        ///// <summary>
        ///// The follwing code works with old casjobs
        ///// </summary>
        ///// <param name="ra"></param>
        ///// <param name="dec"></param>
        ///// <param name="radius"></param>
        ///// <returns></returns>
        //[HttpGet]
        //[ActionName("FieldArray")]
        //public Field[] GetFieldArray([FromUri] String ra, [FromUri] String dec, [FromUri] String radius)
        //{
        //    Validation valid = new Validation();
        //    if (ra == null || dec == null || radius == null) throw new ArgumentException("There are not enough parameters to process your request. Specify ra, dec and radius.");
        //    if (valid.ValidateInput(ra, dec, radius))
        //    {
        //        Sciserver_webService.SDSSFields.SDSSFields sdssFields = new Sciserver_webService.SDSSFields.SDSSFields();
        //        return sdssFields.FieldArray(valid.getRa(), valid.getDec(), valid.getRadius());
        //    }
        //    throw new Exception("There is error processing your request at this time. Check your input parameters and try again later!");
        //}

        //[HttpGet]
        //[ActionName("FieldArrayRect")]
        //public Field[] GetFieldsArrayRect([FromUri] String ra, [FromUri] String dec, [FromUri] String rra, [FromUri] String ddec)
        //{
        //    Validation valid = new Validation();

        //    if (valid.ValidateInput(ra, dec, rra, ddec))
        //    {
        //        Sciserver_webService.SDSSFields.SDSSFields sdssFields = new Sciserver_webService.SDSSFields.SDSSFields();
        //        return sdssFields.FieldArrayRect(valid.getRa(), valid.getDec(), valid.getRRa(), valid.getDDec());
        //    }

        //    throw new Exception("There is error processing your request at this time. Check your input parameters and try again later!");
        //}

        //[HttpGet]
        //[ActionName("ListOfFields")]
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

        //[HttpGet]
        //[ActionName("UrlsOfFields")]
        //public String[] UrlsOfFields(String id, [FromUri] String ra, [FromUri] String dec, [FromUri] String radius, [FromUri] String band)
        //{
        //    Validation valid = new Validation();
        //    Sciserver_webService.SDSSFields.SDSSFields sdssFields = new Sciserver_webService.SDSSFields.SDSSFields();
        //    if (valid.ValidateInput(ra, dec, radius) && valid.ValidateInput(band))
        //    {
        //        return sdssFields.UrlOfFields(valid.getRa(), valid.getDec(), valid.getRadius(), band);
        //    }
        //    throw new Exception("There is error processing your request at this time. Check your request/input parameters and try again later!");

        //}
    }
}
