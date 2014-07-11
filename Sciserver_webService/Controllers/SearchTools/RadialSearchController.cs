using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Net.Http.Headers;
using System.Net.Http.Formatting;
using Sciserver_webService.ExceptionFilter;
using Sciserver_webService.UseCasjobs;

namespace Sciserver_webService.Controllers
{
    public class RadialSearchController : ApiController
    {
        [ExceptionHandleAttribute]
        public HttpResponseMessage Get([FromUri] String ra = null, [FromUri] String dec = null, 
                          [FromUri] String sr = null, [FromUri] String uband = null,
                          [FromUri] String gband = null, [FromUri] String rband= null,
                          [FromUri] String iband = null, [FromUri] String zband = null,
                          [FromUri] String searchtype=null,[FromUri] String returntype=null)
        {
            try{

                RadialSearch rs = new RadialSearch();
                HttpResponseMessage resp = new HttpResponseMessage();
                resp.Content = new StringContent( rs.getData(ra, dec, sr, uband, gband, rband, iband, zband, searchtype, returntype));
                resp.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                resp.StatusCode = HttpStatusCode.OK;
               
               return resp;
 
            }
            catch(Exception e){
                throw new Exception(""+e.Message);
            }
        }
    }
}
