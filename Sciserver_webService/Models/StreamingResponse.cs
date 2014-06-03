using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Net.Http;
using System.Net;

namespace Sciserver_webService.Models
{
    public class StreamingResponse
    {
        public void WriteToStream(Stream outputStream, HttpContent content, TransportContext context)
        {
            StreamWriter writer = new StreamWriter(outputStream);
            try
            {
                for (int i = 0; i < 10; i++)
                {
                    writer.WriteLine("Line " + (i + 1));
                }   
            }
            finally
            {
                writer.Close();
            }
        }
    }
}