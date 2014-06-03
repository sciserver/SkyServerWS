using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Net.Http.Headers;
using Sciserver_webService.Models;

namespace Sciserver_webService.Controllers
{
    public class StreamingResponseController : ApiController
    {
        // GET api/streamingresponse
        public HttpResponseMessage Get()
        {
            HttpResponseMessage response = Request.CreateResponse();
            StreamingResponse streamingResponse = new StreamingResponse();
            response.Content = new PushStreamContent(streamingResponse.WriteToStream, new MediaTypeHeaderValue("text/plain"));
            return response;
        }
    }
}
