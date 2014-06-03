using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.IO;

namespace Sciserver_webService.Controllers
{
    public class StreamingRequestController : ApiController
    {
        // POST api/streamingrequest
        public HttpResponseMessage Post()
        {
            var task = this.Request.Content.ReadAsStreamAsync();
            task.Wait();
            Stream requestStream = task.Result;

            byte[] buffer = new byte[1024];
            int currentLength = 1;
            long totalLength = 0;
            while (currentLength > 0)
            {
                currentLength = requestStream.Read(buffer, 0, 1024);
                totalLength += currentLength;
            }

            requestStream.Close();
            return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, totalLength);
        }
    }
}
