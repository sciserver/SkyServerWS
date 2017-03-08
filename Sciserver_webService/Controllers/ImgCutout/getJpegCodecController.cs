using System;
using System.Linq;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Net.Http.Headers;
using System.Net.Http.Formatting;
using Sciserver_webService.ExceptionFilter;
using Sciserver_webService.ImgCutout;
using SciServer.Logging;
using Sciserver_webService.Common;
using System.Web;
using Newtonsoft.Json;
using System.IO;
using System.Text;

namespace Sciserver_webService.Controllers
{
    public class getJpegCodecController : ApiController
    {
        //// Get The cone search results
        [ExceptionHandleAttribute]

        public HttpResponseMessage Get([FromUri] string  R= null, [FromUri] string C = null, [FromUri] string F = null, [FromUri] string Z = "0", [FromUri]String token = "")
        {

            RequestMisc rm = new RequestMisc(this.Request, "SkyserverWS.ImgCutout.getJpegCodec");
            this.Request.RequestUri = rm.AddTaskNameToURI(this.Request.RequestUri);
            LoggedInfo ActivityInfo = rm.ActivityInfo;

            HttpResponseMessage resp = new HttpResponseMessage();
           
            Validation valid = new Validation();

            if (R == null || C == null || F == null) throw new ArgumentException("There are not enough parameters to process your request. Enter position (ra,dec) values properly. ");
            if (valid.ValidateInput(R, C, F, Z))
            {
                ImgCutout.ImgCutout img = new ImgCutout.ImgCutout();            
                resp.Content = new ByteArrayContent(img.GetJpegImg(valid.getRun(), valid.getCamcol(), valid.getField(),valid.getZoom(), token));
                resp.Content.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
                if (img.hasOutOfFooprintError && img.hasGenericError )
                    resp.StatusCode = HttpStatusCode.OK;
                else
                    if(img.hasOutOfFooprintError)
                        resp.StatusCode = HttpStatusCode.NotFound;
                    else if(img.hasGenericError)
                        resp.StatusCode = HttpStatusCode.InternalServerError;
                    else
                        resp.StatusCode = HttpStatusCode.InternalServerError;

                //logging
                SciserverLogging logger = new SciserverLogging();
                ActivityInfo.Message = rm.GetLoggedMessage("");
                ActivityInfo.DoShowInUserHistory = false;
                logger.LogActivity(ActivityInfo, "SkyserverMessage");

                return resp;
            }

            throw new Exception("Request is not processed, Enter parameters properly.  ra must be in [0,360], dec must be in [-90,90], scale must be in [0.015, 60.0], height and width must be in [64,2048].");
        }
    }
}
