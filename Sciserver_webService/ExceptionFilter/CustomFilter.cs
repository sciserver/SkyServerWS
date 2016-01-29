using System;
using System.Web;
using System.Web.Http.Filters;
using System.Net.Http;
using System.Net;
using System.Web.Http;
using Sciserver_webService.Common;
using System.Text;
using Newtonsoft.Json;
using System.IO;

namespace Sciserver_webService.ExceptionFilter
{
 

    public class ExceptionHandleAttribute : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext context)
        {

            ProcessRequest pr = new ProcessRequest(context.Request, "");
            SciserverLogging Logger = new SciserverLogging();


            System.Text.Encoding tCode = System.Text.Encoding.UTF8;
            String responseType = "application/json";

            StringBuilder strbldr = new StringBuilder();
            StringWriter sw = new StringWriter(strbldr);
            String reasonPhrase = "";
            String errorMessage = "";

            HttpStatusCode errorCode = HttpStatusCode.InternalServerError;

            if (context.Exception is AuthException)
            {
                errorCode = HttpStatusCode.Unauthorized;
                reasonPhrase = errorCode.ToString();
            }
            if (context.Exception is UnauthorizedAccessException)
            {
                errorCode = HttpStatusCode.Unauthorized;
                reasonPhrase = errorCode.ToString();
            }
            else if (context.Exception is NotImplementedException)
            {
                errorCode = HttpStatusCode.NotImplemented;
                reasonPhrase = errorCode.ToString(); 
                //reasonPhrase = "This method is not implemented";
            }
            else if (context.Exception is NotSupportedException)
            {
                errorCode = HttpStatusCode.BadRequest; // ???
                reasonPhrase = errorCode.ToString();
                //reasonPhrase = "The action is not supported by web service";
            }
            else if (context.Exception is HttpUnhandledException)
            {
                errorCode = HttpStatusCode.ExpectationFailed;
                reasonPhrase = errorCode.ToString();
                //reasonPhrase = "This error is not handled";

            }
            else if (context.Exception is ArgumentException)
            {
                errorCode = HttpStatusCode.BadRequest;
                reasonPhrase = errorCode.ToString(); 
                //reasonPhrase = "Input parameters are not proper";
            }
            else if (context.Exception is NotFoundException)
            {
                errorCode = HttpStatusCode.NotFound;
                reasonPhrase = errorCode.ToString();
            }
            else if (context.Exception is Exception)
            {
                errorCode = HttpStatusCode.InternalServerError;
                reasonPhrase = errorCode.ToString();
                //reasonPhrase = "An internal error occured";
            }
            else
            {
                errorCode = HttpStatusCode.InternalServerError;
                reasonPhrase = errorCode.ToString();
                //reasonPhrase = "An internal error occured";
            }

            errorMessage = context.Exception.Message + ((context.Exception.InnerException != null) ? (": " + context.Exception.InnerException.Message) : "");

            //// There should be some logging code here
            LoggedInfo ActivityInfo = new LoggedInfo();
            ActivityInfo.Exception = context.Exception;
            ActivityInfo.ClientIP = pr.ActivityInfo.ClientIP;
            ActivityInfo.TaskName = pr.ActivityInfo.TaskName;
            ActivityInfo.Headers = pr.ActivityInfo.Headers;
            ActivityInfo.Message = errorMessage;
            Logger.LogActivity(ActivityInfo, "ErrorMessage");


            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                writer.WriteStartObject();
                writer.WritePropertyName("Error Code");
                writer.WriteValue((int)errorCode);
                writer.WritePropertyName("Error Type");
                writer.WriteValue(errorCode.ToString());
                writer.WritePropertyName("Error Message");
                writer.WriteValue(errorMessage);
                writer.WritePropertyName("LogMessageID");
                writer.WriteValue(Logger.message.MessageId);
            }



            bool IsHTMLformat = false;
            try
            {
                if (pr.dictionary["format"].ToString().ToLower() == "html")
                    IsHTMLformat = true;
            }
            catch { }

            if (IsHTMLformat)// returns a HTML page with the error message, if the user wants an html reult
            {
                string HtmlContent = "<html><head>";
                HtmlContent += "<title>Skyserver Error</title>";
                HtmlContent += "</head><body bgcolor=white>";
                HtmlContent += "<h2>An error occured</h2>";
                HtmlContent += "<H3 BGCOLOR=pink><font color=red>" + context.Exception.Message + "<br><br></font></H3>";
                HtmlContent += "<br>Technical info: <br> " + strbldr.ToString();
                HtmlContent += "</BODY></HTML>";

                HttpResponseMessage resp = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(HtmlContent, tCode, "text/html"),
                    ReasonPhrase = reasonPhrase
                };
                throw new HttpResponseException(resp);

            }
            else // this returns the jason string with the error message, for all formats except html
            {
                HttpResponseMessage resp = new HttpResponseMessage(errorCode)
                {
                    Content = new StringContent(strbldr.ToString(), tCode, responseType),
                    ReasonPhrase = reasonPhrase
                };
                throw new HttpResponseException(resp);
            }

        }

        
    }
}
