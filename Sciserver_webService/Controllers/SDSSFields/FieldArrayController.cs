using System;
using System.Web.Http;
using Sciserver_webService.Common;
using Sciserver_webService.ExceptionFilter;

namespace Sciserver_webService.Controllers
{
    [ExceptionHandleAttribute]
    public class FieldArrayController : ApiController
    {
      
        [ExceptionHandleAttribute]
        public IHttpActionResult get()
        {
            ProcessRequest request = new ProcessRequest(this.Request, "SkyserverWS.SDSSFields.FieldArray");
            return request.runquery(this, KeyWords.SDSSFields, KeyWords.FieldArray, "SkyserverWS.SDSSFields.FieldArray");
        }

        [ExceptionHandleAttribute]
        public IHttpActionResult post()
        {
            ProcessRequest request = new ProcessRequest(this.Request, "SkyserverWS.SDSSFields.FieldArray");
            return request.runquery(this, KeyWords.SDSSFields, KeyWords.FieldArray, "SkyserverWS.SDSSFields.FieldArray");
        }   
    }
}
