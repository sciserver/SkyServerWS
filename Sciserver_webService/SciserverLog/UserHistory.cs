using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.IO;
using System.Text;
using Sciserver_webService.Common;
using System.Data.SqlClient;
using System.Data;
using System.Globalization;
using System.Web.Http;
using System.Net.Http;
using System.Net;
using System.Data.SqlTypes;



namespace Sciserver_webService.SciserverLog
{
    public class UserHistory
    {


        public DataSet ResultDataSet;
        SqlConnection oConn;

        public string query = "";
        public string ClientIP = "";
        public string TaskName = "";
        public string server_name = "";
        public string windows_name = "";


        public UserHistory(Dictionary<string, string> requestDir, Dictionary<string, string> ExtraInfo, HttpRequest Request)
        {
            if (Request.QueryString.Keys.Count == 0)
            {
                throw new ArgumentException("Request has no input parameters.");
            }

            try
            {
                ClientIP = ExtraInfo["ClientIP"];
                TaskName = ExtraInfo["TaskName"];
                server_name = ExtraInfo["server_name"];
                windows_name = ExtraInfo["windows_name"];
            }
            catch (Exception e) { throw new Exception(e.Message); };

            ResultDataSet = new DataSet();

            foreach (string key in Request.QueryString.Keys)
            {

                if (key != null)
                {
                    string keyL = key.ToLower();

                    if (keyL == "query")
                    {
                        query = Request.QueryString[key];
                    }

                }
            }

            query = "SELECT top 10 * FROM Messages";



            using (oConn = new SqlConnection(KeyWords.SciserverLogDBconnection))
            {
                oConn.Open();
                ResultDataSet = GetDataSetFromQuery(oConn, query);
                oConn.Close();
            }
        }


        public void getUserHistory(Dictionary<string, string> requestDir, Dictionary<string, string> ExtraInfo, HttpRequest Request)
        {








        }




        public DataSet GetDataSetFromQuery(SqlConnection oConn, string query)
        {
            DataSet DataSet = new DataSet();
            try
            {
                SqlCommand Cmd = oConn.CreateCommand();
                Cmd.CommandText = query;
                Cmd.CommandTimeout = Int32.Parse(KeyWords.DatabaseSearchTimeout);
                var Adapter = new SqlDataAdapter(Cmd);
                Adapter.Fill(DataSet);
            }
            catch { }
            return DataSet;
        }



















    }
}