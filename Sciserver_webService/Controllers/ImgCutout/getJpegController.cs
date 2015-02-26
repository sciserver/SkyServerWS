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

/// deoyani@pha.jhu.edu
namespace Sciserver_webService.Controllers
{
    public class getJpegController : ApiController
    {
        //// Get The cone search results
        [ExceptionHandleAttribute]
        public HttpResponseMessage Get([FromUri] string ra = null, [FromUri] string dec = null, [FromUri] string scale = "0.396127", [FromUri] int width = 128, [FromUri] int height = 128, [FromUri] String opt = "", [FromUri]String query = "")
        {
            HttpResponseMessage resp = new HttpResponseMessage();
            Logger log = (HttpContext.Current.ApplicationInstance as MvcApplication).Log;
            string token = "";
            string userid = "unknown";
            IEnumerable<string> values;
            if (ControllerContext.Request.Headers.TryGetValues(KeyWords.XAuthToken, out values))
            {
                try
                {
                    // Keystone authentication
                    token = values.First();
                    var userAccess = Keystone.Authenticate(token);

                    Message message = log.CreateCustomMessage("Auth-SQlSearchRequest", JsonConvert.SerializeObject(Request));
                    userid = userAccess.User.Id;
                    message.UserId = userAccess.User.Id;
                    log.SendMessage(message);
                }
                catch (Exception ex)
                {

                    Message message = log.CreateCustomMessage(KeyWords.loggingMessageType, ex.Message);
                    message.UserId = userid;
                    log.SendMessage(message);
                    throw new UnauthorizedAccessException("Given token is not authorized.");
                }
            }            
            else
            {
                Message message = log.CreateCustomMessage(KeyWords.loggingMessageType, ControllerContext.Request.ToString());
                message.UserId = userid;
                log.SendMessage(message);
            }
           
           Validation valid = new Validation();

           if (ra == null || dec == null || scale == null) throw new ArgumentException("There are not enough parameters to process your request. Enter position (ra,dec) values properly.ra must be in [0,360], dec must be in [-90,90], scale must be in [0.015, 60.0]. ");
           if (valid.ValidateInput(ra, dec, scale))
           {
               ImgCutout.ImgCutout img = new ImgCutout.ImgCutout();               

               if (query == null) query = "";
               /// This part can be changed later if we change internal ImgCutout code.
               if (opt != null) opt = "C" + opt;  else opt = "C";

               resp.Content = new ByteArrayContent(img.GetJpeg(valid.getRa(), valid.getDec(), valid.getScale(), width, height, opt, query, "", ""));
               resp.Content.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
               resp.StatusCode = HttpStatusCode.OK;
               
               return resp;
           }

           throw new Exception("Request is not processed, Enter parameters properly.  ra must be in [0,360], dec must be in [-90,90], scale must be in [0.015, 60.0], height and width must be in [64,2048].");

        }

    }
    
}
