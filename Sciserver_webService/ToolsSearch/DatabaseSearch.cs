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
using System.Data.SqlClient;

namespace Sciserver_webService.UseCasjobs
{
    public class DatabaseSearch
    {

        public string query = "";
        public string Format = "";
        public string TaskName = "";
        public string windows_name = "";

        public DatabaseSearch(ref Dictionary<string, string> requestDir, string ClientIP, bool IsDirectUserConnection, string server_name)
        {

            try
            {
                this.query = Convert.ToString(requestDir["cmd"]);
            }
            catch (FormatException fx) { throw new ArgumentException("InputParameters are not in proper format."); }
            catch (Exception e) { throw new ArgumentException("There are not enough parameters to process your request."); }

            try
            {
                this.Format = Convert.ToString(requestDir["format"]); //Format = "" is the same as application/x-dataset
            }
            catch
            {
                if(IsDirectUserConnection)//in case the user did not specify a format
                    this.Format = "xml";
                else
                    this.Format = "";//which is the same as application/x-dataset
            }
            this.TaskName = "DirectUserQuery";
            if (!IsDirectUserConnection)
            {
                try
                {
                    this.TaskName = Convert.ToString(requestDir["task"]);
                }
                catch {}
                if (this.TaskName == "")
                    this.TaskName = "UnknownTaskFromUserAgent";
            }
            try 
            {
                this.windows_name = System.Environment.MachineName;
            }
            catch {};
            if (this.windows_name == "")
                this.windows_name = "UnknownWinName"; 


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

            this.query = "EXEC spExecuteSQL '" + c2 + "','" + KeyWords.MaxRows + "','" + server_name + "','" + windows_name + "','" + ClientIP + "','" + TaskName.Substring(0,Math.Min(TaskName.Length,32)) + "'";// parsing the query against harmful sql commands
            // @cmd,@limit,@webserver,@winname,@clientIP,@access,@system,@maxQueries
            //this.query = c2;

        }
    }
}