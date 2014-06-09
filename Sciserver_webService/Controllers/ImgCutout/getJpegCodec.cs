using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Net.Http.Headers;
using System.Net.Http.Formatting;
using Sciserver_webService.ExceptionFilter;
using Sciserver_webService.ImgCutout;


namespace Sciserver_webService.Controllers
{
    public class getJpegCodecController : ApiController
    {
        //// Get The cone search results
        [ExceptionHandleAttribute]

        public HttpResponseMessage Get([FromUri] string  R= null, [FromUri] string C = null, [FromUri] string F = null, [FromUri] string Z = "0")
        {

            //if (ControllerContext.Request.Headers.TryGetValues("X-Auth-Token", out values))
            //{
            //    // Keystone authentication
            //    string token = values.First();
            //    var userAccess = Keystone.Authenticate(token);
            //    return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, "Keystone authentication");
            //}
            //else
            //{
            //    // No authentication (anonymous)
            //    return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, "Anonymous");
            //}
            Validation valid = new Validation();

            if (R == null || C == null || F == null) throw new ArgumentException("There are not enough parameters to process your request. Enter position (ra,dec) values properly. ");
            if (valid.ValidateInput(R, C, F, Z))
            {
                ImgCutout.ImgCutout img = new ImgCutout.ImgCutout();
                HttpResponseMessage resp = new HttpResponseMessage();                
                resp.Content = new ByteArrayContent(img.GetJpegImg(valid.getRun(), valid.getCamcol(), valid.getField(),valid.getZoom()));
                resp.Content.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
                resp.StatusCode = HttpStatusCode.OK;
                return resp;
            }



            throw new Exception("Request is not processed, Enter parameters properly.  ra must be in [0,360], dec must be in [-90,90], scale must be in [0.015, 60.0], height and width must be in [64,2048].");

        }
    }
}
