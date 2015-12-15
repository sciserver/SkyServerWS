using System;
using System.Web;
using System.Web.Http.Filters;
using System.Net.Http;
using System.Net;
using System.Web.Http;
using Sciserver_webService.Common;
using System.Text;

namespace Sciserver_webService.ExceptionFilter
{
 

    public class ExceptionHandleAttribute : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext context)
        {

            //var v = context.Request;

            System.Text.Encoding tCode = System.Text.Encoding.UTF8;
            String responseType = "application/json";
            var jsonString = "{ \"Error Code\" : " ;
            String reasonPhrase = "";
            HttpStatusCode errorCode = HttpStatusCode.InternalServerError;

            //// if code is not implemented
            if (context.Exception is NotImplementedException)
            {
                jsonString += (int)HttpStatusCode.NotImplemented;
                errorCode =  HttpStatusCode.NotImplemented;
                reasonPhrase = "This method is not implemented";
            }
            else if(context.Exception is NotSupportedException)
            {
                jsonString += (int)HttpStatusCode.BadRequest; 
                errorCode = HttpStatusCode.BadRequest; // ???
                reasonPhrase = "The action is not supported by web service";
            }
            else if (context.Exception is HttpUnhandledException)
            {
                jsonString += (int)HttpStatusCode.ExpectationFailed;
                errorCode = HttpStatusCode.ExpectationFailed;
                reasonPhrase = "This error is not handled";
               
            }            
            else if (context.Exception is ArgumentException)
            {
                jsonString += (int)HttpStatusCode.BadRequest;
                errorCode = HttpStatusCode.BadRequest;
                reasonPhrase = "Input parameters are not proper";                
            }
            else if (context.Exception is Exception)
            {
                jsonString += (int)HttpStatusCode.InternalServerError;
                errorCode = HttpStatusCode.InternalServerError;
                reasonPhrase = "An internal error occured";
            }

            jsonString += ",\n  \"Error Type\": \" " + errorCode+ "\"";
            jsonString += ",\n  \"Error Message\": \" " + context.Exception.Message + "\"";
            jsonString += "}";

            //// to do
            //// There should be some logging code here
            ProcessRequest pr = new ProcessRequest(context.Request, "");
            SciserverLogging Logger = new SciserverLogging();
            LoggedInfo ActivityInfo = new LoggedInfo();
            ActivityInfo.Exception = context.Exception;
            ActivityInfo.ClientIP = pr.ActivityInfo.ClientIP;
            ActivityInfo.TaskName = pr.ActivityInfo.TaskName;
            
            ActivityInfo.Headers = pr.ActivityInfo.Headers;
            ActivityInfo.Message = jsonString;
            Logger.LogActivity(ActivityInfo, "ErrorMessage");


            bool IsHTMLformat = false;
            try
            {
                if (pr.dictionary["format"].ToString().ToLower() == "html")
                    IsHTMLformat = true;
            }
            catch { }

            if (IsHTMLformat)// returns a HTML page with the error message, if the user wants an html reult
            {

                            StringBuilder sb = new StringBuilder();
                            sb.AppendFormat("<html><head>\n");
                            sb.AppendFormat("<title>Skyserver Error</title>\n");
                            sb.AppendFormat("</head><body bgcolor=white>\n");
                            sb.AppendFormat("<h2>An error occured</h2>");
                            sb.AppendFormat("<H3 BGCOLOR=pink><font color=red><br>" + context.Exception.Message + "</font></H3>");
                            sb.AppendFormat("</BODY></HTML>\n");

                            HttpResponseMessage resp = new HttpResponseMessage(HttpStatusCode.OK)
                            {
                                Content = new StringContent(sb.ToString(), tCode, "text/html"),
                                ReasonPhrase = reasonPhrase
                            };
                            throw new HttpResponseException(resp);

            }
            else // this returns the jason string with the error message, for all formats except html
            {
                HttpResponseMessage resp = new HttpResponseMessage(errorCode)
                {
                    Content = new StringContent(jsonString, tCode, responseType),
                    ReasonPhrase = reasonPhrase
                };
                throw new HttpResponseException(resp);
            }

        }
    }
}
