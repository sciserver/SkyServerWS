using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using net.ivoa.VOTable;
using Newtonsoft.Json;
using System.IO;
using System.Text;
using Sciserver_webService.Common;

namespace Sciserver_webService.ToolsSearch
{
    public class SqlSearch
    {

        public string query = "";
        public string queryResult { get; set; }
        public string syntax = "";
        public string QueryForUserDisplay = "";

        public SqlSearch() { 
        
        }

        //public SqlSearch(string query)
        //{
        //    this.query = query;
        //}

        public string ClientIP = "";
        public string TaskName = "";
        public string server_name = "";
        public string windows_name = "";

        public SqlSearch(Dictionary<string, string> requestDir, Dictionary<string, string> ExtraInfo)
        {

           try
           {
               this.query = Convert.ToString(requestDir["cmd"]);
           }
           catch (FormatException fx) { throw new ArgumentException("InputParameters are not in proper format."); }
           catch (Exception e) { throw new ArgumentException("There are not enough parameters to process your request."); }

           string c = this.query;
           string c2 = Regex.Replace(c, @"\/\*(.*\n)*\*\/", "");	                        // remove all multi-line comments
           c2 = Regex.Replace(c2, @"^[ \t\f\v]*--.*\r\n", "", RegexOptions.Multiline);		// remove all isolated single-line comments
           c2 = Regex.Replace(c2, @"--[^\r^\n]*", "");				                        // remove all embedded single-line comments
           c2 = Regex.Replace(c2, @"[ \t\f\v]+", " ");                      				// replace multiple whitespace with single space
           c2 = Regex.Replace(c2, @"^[ \t\f\v]*\r\n", "", RegexOptions.Multiline);			// remove empty lines
           c = c2;                                          								// make a copy of massaged query
           c2 = c2.Replace("'", "''");		                                                
           // 'c' is query version that's printed on output page
           // 'c2' is the version that is sent to DB server 

           try
           {
               ClientIP = ExtraInfo["ClientIP"];
               TaskName = ExtraInfo["TaskName"];
               server_name = ExtraInfo["server_name"];
               windows_name = ExtraInfo["windows_name"];
           }
           catch(Exception e) { throw new Exception(e.Message);};

           try
           {
               syntax = requestDir["syntax"];
           }
           catch
           {
               syntax = "NoSyntax";
           }

           if (syntax == "Syntax")
           {
               string[] clines = c.Split('\n');
               c = "<i>Line#</i>\n";
               for (int i = 0; i < clines.Length; i++)
               {
                   if ((i < (clines.Length - 1)) || (clines[i].Length > 0))
                   {
                       if ((i + 1) < 10)
                           c += "<i>" + (i + 1) + ".</i>   " + clines[i];
                       else if ((i + 1) < 100)
                           c += "<i>" + (i + 1) + ".</i>  " + clines[i];
                       else
                           c += "<i>" + (i + 1) + ".</i> " + clines[i];
                   }
               }
               //this.query = "EXEC spExecuteSQL 'set parseonly on " + c2 + "','" + KeyWords.MaxRows + "', @log=0, @filter=1";// parsing the query against harmful sql commands
               this.query = "EXEC spExecuteSQL 'set parseonly on " + c2 + "','" + KeyWords.MaxRows + "','" + server_name + "','" + windows_name + "','" + ClientIP + "','" + TaskName + "',@filter=1,@log=1";// parsing the query against harmful sql commands
               // @cmd,@limit,@webserver,@winname,@clientIP,@access,@system,@maxQueries

           }
           else
           {
               this.query = "EXEC spExecuteSQL '" + c2 + "','" + KeyWords.MaxRows + "','" + server_name + "','" + windows_name + "','" + ClientIP + "','" + TaskName + "',@filter=1,@log=1";// parsing the query against harmful sql commands
           }
           QueryForUserDisplay = c;
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