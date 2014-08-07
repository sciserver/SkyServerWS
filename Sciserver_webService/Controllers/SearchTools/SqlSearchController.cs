using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Net.Http.Headers;
using System.Net.Http.Formatting;
using Sciserver_webService.ExceptionFilter;
using Sciserver_webService.UseCasjobs;
using net.ivoa.VOTable;
using Sciserver_webService.Common;

namespace Sciserver_webService.Controllers
{
    public class SqlSearchController : ApiController
    {
        //[ExceptionHandleAttribute]
        //public HttpResponseMessage get([FromUri] String query = null, [FromUri] String returntype = "json")
        //{
        //    try {
        //        if (query == null) throw new ArgumentException("There is no parameter input for your request. Enter Query , returntype(optional) parameter.");
        //        SqlSearch sq = new SqlSearch();
        //        HttpResponseMessage resp = new HttpResponseMessage();
        //        if (returntype.Equals("json"))
        //        {
        //            resp.Content = new StringContent(sq.getJSONstring(query));
        //            resp.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        //        }
        //        else if (returntype.Equals("votable")) {

        //            resp.Content = new ObjectContent<VOTABLE>(sq.getVOTable(query), new XmlMediaTypeFormatter(), new MediaTypeHeaderValue("application/xml"));
        //            //, new MediaTypeFormatterCollection() { new XmlMediaTypeFormatter() }, new FormatterSelector());
        //            //resp.Content.Headers.ContentType = new MediaTypeHeaderValue("application/xml");
        //        }

        //        resp.StatusCode = HttpStatusCode.OK;

        //        return resp;
               
        //    }
        //    catch(Exception e)
        //    {
        //        throw new Exception(e.Message);
        //    }

        //}

         /// <summary>
         /// This is using new REST casjobs
         /// </summary>
         /// <returns></returns>
         [ExceptionHandleAttribute]
         public HttpResponseMessage get()
         {
             HttpResponseMessage resp = new HttpResponseMessage();
             string token = "";             
             Dictionary<String, String> dictionary = null;
             string query = "";
             RunCasjobs run = new RunCasjobs();

             IEnumerable<string> values;
             if (ControllerContext.Request.Headers.TryGetValues(KeyWords.XAuthToken, out values))
             {
                 try
                 {
                     // Keystone authentication
                     token = values.First();
                     var userAccess = Keystone.Authenticate(token);
                 }
                 catch (Exception ex) {
                     throw new UnauthorizedAccessException("Check the token you are using.");
                 }                 

             }

             // logging for the request
             dictionary = Request.GetQueryNameValuePairs().ToDictionary(kv => kv.Key, kv => kv.Value, StringComparer.OrdinalIgnoreCase);
             query = dictionary["query"];

             string queryResult = run.postCasjobs(query, token, KeyWords.sqlSearchQuery).Content.ReadAsStringAsync().Result;
             string strContent = "{ \"Query:\"" + query + ", \"QueryResult\": " + queryResult + "}";
             resp.Content = new StringContent(strContent);
             return resp;
         }
    }
}
