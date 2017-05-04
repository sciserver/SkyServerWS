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

/// deoyani@jhu.edu
namespace Sciserver_webService.Controllers
{
    public class getJpegController : ApiController
    {
        //// Get The cone search results
        [ExceptionHandleAttribute]
        public HttpResponseMessage Get([FromUri] string ra = null, [FromUri] string dec = null, [FromUri] string scale = "0.396127",
            [FromUri] int width = 128, [FromUri] int height = 128, [FromUri] String opt = "", [FromUri]String query = "", [FromUri]String clientIP = "", [FromUri]String token = "", [FromUri]String TaskName = "")
        {
            RequestMisc rm = new RequestMisc(this.Request, "SkyserverWS.ImgCutout.getJpeg");
            this.Request.RequestUri = rm.AddTaskNameToURI(this.Request.RequestUri);
            LoggedInfo ActivityInfo = rm.ActivityInfo;
            string ClientIP = ActivityInfo.ClientIP;
            
            HttpResponseMessage resp = new HttpResponseMessage();

            Validation valid = new Validation();

            if (ra == null || dec == null || scale == null)
                throw new ArgumentException("There are not enough parameters to process your request. Enter position (ra,dec) values properly.ra must be in [0,360], dec must be in [-90,90], scale must be in [0.015, 60.0]. ");
            if (valid.ValidateInput(ra, dec, scale))
            {
                ImgCutout.ImgCutout img = new ImgCutout.ImgCutout();

                if (query == null) query = "";
                /// This part can be changed later if we change internal ImgCutout code.
                if (opt != null) opt = "C" + opt; else opt = "C";

                resp.Content = new ByteArrayContent(img.GetJpeg(valid.getRa(), valid.getDec(), valid.getScale(), width, height, opt, query, "", "", token, ClientIP));
                resp.Content.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
                if (String.IsNullOrEmpty(img.errorMessage_Generic) && String.IsNullOrEmpty(img.errorMessage_OutOfFootprint))
                    resp.StatusCode = HttpStatusCode.OK;
                else
                {
                    if (!String.IsNullOrEmpty(img.errorMessage_Generic))
                    {
                        resp.StatusCode = HttpStatusCode.InternalServerError;
                        resp.ReasonPhrase = "INTERNAL SERVER ERROR. " + img.errorMessage_Generic;
                    }
                    else if (!String.IsNullOrEmpty(img.errorMessage_OutOfFootprint))
                    {
                        resp.StatusCode = HttpStatusCode.NotFound;
                        resp.ReasonPhrase = "NOT FOUND. " + img.errorMessage_OutOfFootprint;
                    }
                    else
                        resp.StatusCode = HttpStatusCode.InternalServerError;
                }
                //logging
                SciserverLogging logger = new SciserverLogging();
                ActivityInfo.Message = rm.GetLoggedMessage(query);
                logger.LogActivity(ActivityInfo, "SkyserverMessage");

                return resp;
            }

            throw new Exception("Request is not processed, Enter parameters properly.  ra must be in [0,360], dec must be in [-90,90], scale must be in [0.015, 60.0], height and width must be in [64,2048].");

        }

    }

}
