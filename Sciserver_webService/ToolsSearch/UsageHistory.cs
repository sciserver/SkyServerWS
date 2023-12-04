using System;
using System.Collections.Generic;
using System.Web;
using Sciserver_webService.Common;
using System.Data.SqlClient;
using System.Data;


namespace Sciserver_webService.ToolsSearch
{
    public class UsageHistory
    {


        public DataSet ResultDataSet = new DataSet();
        SqlConnection oConn;

        public string query = "";
        int? CustomMessageType = null;
        string format = "csv";//default value
        bool DoShowAllHistory = false;//default value
        Dictionary<string, string> ShortTaskNamesDict = null;

        public UsageHistory(Dictionary<string, string> requestDir, Dictionary<string, string> ExtraInfo, HttpRequest Request)
        {
            if (requestDir.Keys.Count == 0)
            {
                throw new ArgumentException("Request has no input parameters.");
            }

            foreach (string key in requestDir.Keys)
            {
                string keyL = key.ToLower();
                if (keyL == "format")
                {
                    try { format = requestDir[key]; }
                    catch { }
                }
            }
            try
            {
                using (oConn = new SqlConnection(KeyWords.SciserverLogDBconnection))
                {
                    oConn.Open();
                    SqlCommand Cmd = BuildCommand(oConn);
                    query = Cmd.CommandText;
                    Cmd.CommandTimeout = Int32.Parse(KeyWords.DatabaseSearchTimeout);
                    var Adapter = new SqlDataAdapter(Cmd);
                    Adapter.Fill(ResultDataSet);
                    oConn.Close();
                }
            }
            catch { throw; }
            //PrepareResultSet(ref ResultDataSet, requestDir);
        }

        public SqlCommand BuildCommand(SqlConnection oConn)
        {

            SqlCommand cmd = oConn.CreateCommand();
            cmd.CommandText = "SELECT * FROM vSkyServerUsage";
            return cmd;
        }
    }
}