using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.Net.Http;
using Sciserver_webService.UseCasjobs;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Net.Http.Headers;
namespace Sciserver_webService.Common
{
    public class OutputFormat
    {
        public Stream httpStream = null;
        public async Task<HttpResponseMessage> getResults(String query, String token, String casjobsMessage, String format)
        {
            RunCasjobs run = new RunCasjobs();
            HttpResponseMessage respM = run.postCasjobs(query, token, casjobsMessage, format);
            httpStream = await respM.Content.ReadAsStreamAsync();
            HttpResponseMessage resp = new HttpResponseMessage();
            resp.Content = new StreamContent(httpStream);
            resp.Content.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
            httpStream.Flush();
            return resp;
         }

        public HttpResponseMessage getResults(HttpResponseMessage resp,String query,String token,String casjobsMessage, String format) {

            RunCasjobs run = new RunCasjobs();

            String result = run.postCasjobs(query, token, casjobsMessage).Content.ReadAsStringAsync().Result;

            switch(format){

                case "json" :
                    resp.Content = new StringContent(getJson(result,query)); break;

                //case "csv":
                //    resp.Content = new StringContent(result); break;

                default: resp.Content = new StringContent(result); break;
                //default: break;
            }
            return resp;
        }


    

        public string getJson(string resultContent, string query)
        {
            string[] lines = resultContent.Split('\n');
            var csv = new List<string[]>(); // or, List<YourClass>            
            foreach (string line in lines)
                csv.Add(line.Split(',')); // or, populate YourClass                       

            query = query.Replace("\n", "");
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);

            using (JsonWriter writer = new JsonTextWriter(sw))
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
            return sb.ToString();
        }
        
    }
}