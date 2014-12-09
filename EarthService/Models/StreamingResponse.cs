using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Net.Http;
using System.Net;
using Newtonsoft.Json;
using System.IO;
using System.Text;


namespace Sciserver_webService.Models
{
    public class StreamingResponse
    {
        private String data = "";
        private String resultContent = "";
        private String query = "";
        public StreamingResponse(String d, String query){
            this.data = d;
            this.resultContent = d;
            this.query = query;
        }
        //public void WriteToStream(Stream outputStream, HttpContent content, TransportContext context)
        //{
        //    StreamWriter writer = new StreamWriter(outputStream);
        //    try
        //    {
        //        //for (int i = 0; i < 10; i++)
        //        //{
        //        //   writer.WriteLine("Line " + (i + 1));
        //        //}   
        //        writer.WriteLine(data);
        //    }
        //    finally
        //    {
        //        writer.Close();
        //    }
        //}

        public void WriteToStream(Stream outputStream, HttpContent content, TransportContext context)
        {
            string[] lines = resultContent.Split('\n');
            var csv = new List<string[]>(); // or, List<YourClass>            
            foreach (string line in lines)
                csv.Add(line.Split(',')); // or, populate YourClass   
            StreamWriter strWriter = new StreamWriter(outputStream);
            
            try
            {
                //using (StringWriter sw = new StringWriter(strWriter))
                //{ 
                //    sw.WriteLine("#Query: "+query);                    
                //    sw.WriteLine("#QueryResults:");
                //    for (int i = 1; i < lines.Length; i++)
                //    {                       
                //        for (int Index = 0; Index < csv[0].Length; Index++)
                //        {
                //            sw.WriteLine(csv[0][Index]);
                //            sw.WriteLine(csv[i][Index]);
                //        }                        
                //    }                    
                //}
            }
            finally
            {
                strWriter.Close();
            }
        }      

    }
}