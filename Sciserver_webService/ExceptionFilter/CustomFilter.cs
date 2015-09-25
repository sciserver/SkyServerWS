using System;
using System.Web;
using System.Web.Http.Filters;
using System.Net.Http;
using System.Net;
using System.Web.Http;

namespace Sciserver_webService.ExceptionFilter
{
 

    public class ExceptionHandleAttribute : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext context)
        {           
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
                reasonPhrase = "There action is not supported by web service";
            }
            else if (context.Exception is HttpUnhandledException)
            {
                jsonString += (int)HttpStatusCode.ExpectationFailed;
                errorCode = HttpStatusCode.ExpectationFailed;
                reasonPhrase = "This Error is not handled";
               
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
                reasonPhrase = "There is some Internal Error Occured";
            }

            jsonString += ",\n  \"Error Type\": \" " + errorCode+ "\"";
            jsonString += ",\n  \"Error Message\": \" " + context.Exception.Message + "\"";
            jsonString += "}";

            //// to do
            //// There should be some logging code here
            
            HttpResponseMessage resp = new HttpResponseMessage(errorCode)
            {
                Content = new StringContent(jsonString, tCode, responseType),
                ReasonPhrase = reasonPhrase
            };             
            throw new HttpResponseException(resp);
        }
    }
}
