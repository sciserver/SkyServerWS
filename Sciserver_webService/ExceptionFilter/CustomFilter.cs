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
using System.Configuration;
using System.Collections.Generic;

namespace Sciserver_webService.ExceptionFilter
{
 

    public class ExceptionHandleAttribute : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext context)
        {

            System.Text.Encoding tCode = System.Text.Encoding.UTF8;
            String responseType = "application/json";
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


            //ProcessRequest pr = new ProcessRequest(context.Request, "");
            SciserverLogging Logger = new SciserverLogging();
            RequestMisc rm = new RequestMisc(context.Request, "");

            //// There should be some logging code here

            LoggedInfo ActivityInfo = rm.ActivityInfo;
            ActivityInfo.Exception = context.Exception;
            ActivityInfo.Message = rm.GetLoggedMessage("");
            Logger.LogActivity(ActivityInfo, "ErrorMessage");

            //preparing the message sent to the user

            errorMessage = context.Exception.Message + ((context.Exception.InnerException != null) ? (": " + context.Exception.InnerException.Message) : "");

            StringBuilder strbldr = new StringBuilder();
            StringWriter sw = new StringWriter(strbldr); 
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                writer.WriteStartObject();
                writer.WritePropertyName("ErrorCode");
                writer.WriteValue((int)errorCode);
                writer.WritePropertyName("ErrorType");
                writer.WriteValue(errorCode.ToString());
                writer.WritePropertyName("ErrorMessage");
                writer.WriteValue(errorMessage);
                writer.WritePropertyName("LogMessageID");
                writer.WriteValue(Logger.message.MessageId);
            }
            string TechInfoJson = strbldr.ToString();

            strbldr = new StringBuilder(); 
            sw = new StringWriter(strbldr); 
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                writer.WriteStartObject();
                writer.WritePropertyName("ErrorCode");
                writer.WriteValue((int)errorCode);
                writer.WritePropertyName("ErrorType");
                writer.WriteValue(errorCode.ToString());
                writer.WritePropertyName("ErrorMessage");
                writer.WriteValue(errorMessage);
                writer.WritePropertyName("LogMessageID");
                writer.WriteValue(Logger.message.MessageId);
                writer.WritePropertyName("username");
                writer.WriteValue(Logger.user_name);
                writer.WritePropertyName("userid");
                writer.WriteValue(Logger.userid);
                writer.WritePropertyName("pageurl");
                writer.WriteValue(ActivityInfo.URI);
                writer.WritePropertyName("referrer");
                writer.WriteValue(ActivityInfo.Referrer);
                writer.WritePropertyName("StackTrace");
                writer.WriteValue(ActivityInfo.Exception.StackTrace);
                writer.WritePropertyName("InnerTrace");
                writer.WriteValue(ActivityInfo.Exception.InnerException != null ? ActivityInfo.Exception.InnerException.StackTrace : "");

            }
            string TechInfoJsonAll = strbldr.ToString();


            bool IsHTMLformat = false;
            try
            {
                if (rm.dictionary["format"].ToString().ToLower() == "html" || (rm.dictionary["format"].ToLower() == "mydb" && !rm.IsDirectUserConnection)   )// || (rm.dictionary["format"].ToString().ToLower() == "mydb" && !rm.IsDirectUserConnection ) )
                    IsHTMLformat = true;
            }
            catch { }

            if (IsHTMLformat)// returns a HTML page with the error message, if the user wants an html reult
            {
                string HtmlContent = "<html><head>";
                HtmlContent += "<title>Skyserver Error</title>";
                HtmlContent += "</head><body bgcolor=white>";
                HtmlContent += "<h2>An error occured</h2>";
                HtmlContent += "<H3 BGCOLOR=pink><font color=red>" + WebUtility.HtmlEncode(context.Exception.Message) + "<br><br></font></H3>";
                HtmlContent += "<br><br> <form method =\"POST\" target=\"_blank\" name=\"bugreportform\" action=\"" + ConfigurationManager.AppSettings["BugReportURL"] + "\">";
                Dictionary<string, string> ErrorFields = JsonConvert.DeserializeObject<Dictionary<string, string>>(TechInfoJsonAll);
                foreach (string key in ErrorFields.Keys)
                {
                    HtmlContent += "<input type=\"hidden\" name=\"popz_" + key + "\" id=\"popz_" + key + "\" value=\"" + WebUtility.HtmlEncode(ErrorFields[key]) + "\" />";
                }
                HtmlContent += "<input type=\"hidden\" name=\"popz_bugreport\" id=\"popz_bugreport\" value=\"" + WebUtility.HtmlEncode(TechInfoJsonAll) + "\" />";
                HtmlContent += "<input id=\"submit\" type=\"submit\" value=\"Click to Report Error\">";
                HtmlContent += "</form>";
                HtmlContent += "<br>Technical info: <br> " + WebUtility.HtmlEncode(TechInfoJson);
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
                    Content = new StringContent(TechInfoJson, tCode, responseType),
                    ReasonPhrase = reasonPhrase
                };
                throw new HttpResponseException(resp);
            }

        }

        
    }
}
