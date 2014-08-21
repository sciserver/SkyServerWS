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
    public class JSONStreamResponse
    {
        private String data = "";
        private String resultContent = "";
        private String query = "";
        public JSONStreamResponse(String d, String query){
            this.data = d;
            this.resultContent = d;
            this.query = query;
        }
       

        public void WriteToStream(Stream outputStream, HttpContent content, TransportContext context)
        {
            string[] lines = resultContent.Split('\n');
            var csv = new List<string[]>(); // or, List<YourClass>            
            foreach (string line in lines)
                csv.Add(line.Split(',')); // or, populate YourClass   
            StreamWriter strWriter = new StreamWriter(outputStream);
            //StringWriter sw = new StringWriter(strWriter);
            try
            {
                using (JsonWriter writer = new JsonTextWriter(strWriter))
                {
                    writer.Formatting = Formatting.Indented;
                    writer.WriteStartObject();
                    writer.WritePropertyName("Query");
                    writer.WriteValue(query);
                    writer.WritePropertyName("Query Results:");
                    writer.WriteStartArray();

                    for (int i = 1; i < lines.Length; i++)
                    {
                        writer.WriteStartObject();
                        for (int Index = 0; Index < csv[0].Length; Index++)
                        {
                            writer.WritePropertyName(csv[0][Index]);
                            writer.WriteValue(csv[i][Index]);
                        }
                        writer.WriteEndObject();
                    }
                    writer.WriteEndArray();
                    writer.WriteEndObject();
                }
            }
            finally
            {
                strWriter.Close();
            }
        }      
    }
}