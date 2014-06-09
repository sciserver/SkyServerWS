using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Net.Http.Headers;
using Sciserver_webService.ConeSearch;
using Sciserver_webService.ExceptionFilter;
using Sciserver_webService.casjobs;
using Sciserver_webService.Models;

using net.ivoa.VOTable;

/// deoyani@pha.jhu.edu
namespace Sciserver_webService.Controllers
{
    public class ConeSearchController : ApiController
    {
        //// Get The cone search results
        [ExceptionHandleAttribute]        
        public VOTABLE Get([FromUri] String ra = null, [FromUri] String dec= null, [FromUri] String sr= null)
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
           if (ra == null || dec == null || sr == null) throw new ArgumentException("There are not enough parameters to process your request.");
           StreamingCone streamingResponse = new StreamingCone();
           return streamingResponse.ConeSearch(ra, dec, sr);            
        }

        //public HttpResponseMessage get(int id)
        //{ 
        //    RunQuery RunQuery = new RunQuery();
        //    //return RunQuery.testQuery();
        //    HttpResponseMessage response = Request.CreateResponse();
        //    StreamingResponse streamingResponse = new StreamingResponse(RunQuery.testQuery());
        //    response.Content = new PushStreamContent(streamingResponse.WriteToStream, new MediaTypeHeaderValue("text/plain"));
        //    return response;

        //}

        //public HttpResponseMessage Get()
        //{
        //    //HttpResponseMessage response = Request.CreateResponse();
        //    throw new ArgumentException("There are not enough parameters to process your request. \n specify ra,dec and sr values.");        
        //}
    }
    
}
