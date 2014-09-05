using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using net.ivoa.VOTable;

using Newtonsoft.Json;
using System.IO;
using System.Text;

namespace Sciserver_webService.UseCasjobs
{
    public class SqlSearch
    {

        private string query = "";

        public string queryResult { get; set; }

        public SqlSearch() { 
        
        }

        public SqlSearch(string query)
        {
            this.query = query;
        }


        //public string getJson(string resultContent, string query)
        //{
        //     string[] lines = resultContent.Split('\n');
        //     var csv = new List<string[]>(); // or, List<YourClass>            
        //     foreach (string line in lines)
        //         csv.Add(line.Split(',')); // or, populate YourClass                       
             
        //     StringBuilder sb = new StringBuilder();
        //     StringWriter sw = new StringWriter(sb);

        //     using (JsonWriter writer = new JsonTextWriter(sw))
        //     {
        //         writer.Formatting = Formatting.Indented;
        //         writer.WriteStartObject();
        //         writer.WritePropertyName("Query");
        //         writer.WriteValue(query);
        //         writer.WritePropertyName("Query Results:");
        //         writer.WriteStartArray();

        //         for (int i = 1; i < lines.Length; i++)
        //         {
        //             writer.WriteStartObject();
        //             for (int Index = 0; Index < csv[0].Length; Index++)
        //             {
        //                 writer.WritePropertyName(csv[0][Index]);
        //                 writer.WriteValue(csv[i][Index]);
        //             }
        //             writer.WriteEndObject();
        //         }
        //         writer.WriteEndArray();
        //         writer.WriteEndObject();
        //     }
        //     return sb.ToString();
        //}
    }
}