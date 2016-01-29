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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Specialized;


namespace Sciserver_webService.ToolsSearch
{
    public class UserHistory
    {


        public DataSet ResultDataSet = new DataSet();
        SqlConnection oConn;

        public string query = "";

        Int64? limit = null;
        DateTime? Time1 = null;
        DateTime? Time2 = null;
        string UriToken = null;
        string HeaderToken = null;
        string UserID = null;
        string Application = null;
        string ContentSearchText = null;
        int? CustomMessageType = null;
        string format = "html";//default value
        bool DoShowAllHistory = false;//default value
        
        
        public UserHistory(Dictionary<string, string> requestDir, Dictionary<string, string> ExtraInfo, HttpRequest Request)
        {
            if (requestDir.Keys.Count == 0)
            {
                throw new ArgumentException("Request has no input parameters.");
            }

            foreach (string key in requestDir.Keys)
            {
                    string keyL = key.ToLower();

                    if (keyL == "limit")
                    {
                        if (!String.IsNullOrEmpty(requestDir[key]))
                        {
                            try { limit = Convert.ToInt64(requestDir[key]); }
                            catch { throw new Exception("The row limit parameter does not have a valid numerical value."); }
                            if (limit <= 0 || limit > Int64.Parse(KeyWords.MaxRows))
                                throw new Exception("The row limit parameter has to be an integer greater than 0 and smaller than " + Int64.Parse(KeyWords.MaxRows));
                        }
                    }
                    if (keyL == "date_low")
                    {
                        if (!String.IsNullOrEmpty(requestDir[key]))
                        {
                            try { Time1 = DateTime.Parse(requestDir[key]); }
                            catch { throw new Exception("Lower date limit does not have a valid format"); }
                        }
                    }
                    if (keyL == "date_high")
                    {
                        if (!String.IsNullOrEmpty(requestDir[key]))
                        {

                            try { Time2 = DateTime.Parse(requestDir[key]); }
                            catch { throw new Exception("Upper date limit does not have a valid format"); }
                        }
                    }
                    if (keyL == "token")
                    {
                        try { UriToken = requestDir[key]; }
                        catch {}
                    }
                    if (keyL == "application")
                    {
                        try { Application = requestDir[key]; }
                        catch {}
                    }
                    if (keyL == "custommessagetype")
                    {
                        if (!String.IsNullOrEmpty(requestDir[key]))
                        {
                            try { CustomMessageType = Int32.Parse(requestDir[key]); }
                            catch { throw new Exception("CustomMessageType should be a valid integer value."); }
                        }
                    }
                    if (keyL == "format")
                    {
                        try { format = requestDir[key]; }
                        catch {}
                    }
                    if (keyL == "doshowallhistory")
                    {
                        try { DoShowAllHistory = requestDir[key] == "true" ? true : false; }
                        catch { }
                    }
                    /*
                    if (keyL == "contentsearchtext")
                    {
                        try { ContentSearchText = requestDir[key]; }
                        catch { }
                    }
                    */ 

            }

            if (Request.Headers.AllKeys.Contains(KeyWords.XAuthToken))
                HeaderToken = Request.Headers.Get(KeyWords.XAuthToken);

            //Authenticating the Token. Token in header has priority over token in URI
            if(!String.IsNullOrEmpty(HeaderToken))
            {
                try
                {
                    var userAccess = Keystone.Authenticate(HeaderToken);
                    UserID = userAccess.User.Id;
                }
                catch { };
            }
            else if (!String.IsNullOrEmpty(UriToken))
            {
                try
                {
                    var userAccess = Keystone.Authenticate(UriToken);
                    UserID = userAccess.User.Id;
                }
                catch { };
            }
            else
                throw new UnauthorizedAccessException("Unable to find user. Token was not provided.");

            if(String.IsNullOrEmpty(UserID))
                throw new UnauthorizedAccessException("Unable to find user. Token could not be authenticated. Try getting a new token.");

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
            PrepareResultSet(ref ResultDataSet, requestDir);
        }


        public string GetPositionType(DataSet ds, int RowIndex)
        {
            string PositionType = "";
            try
            {
                Dictionary<string, string> values = JsonConvert.DeserializeObject<Dictionary<string, string>>(ds.Tables[0].Rows[RowIndex]["Content"].ToString());
            }
            catch { }
            return PositionType;
        }


        public void SetApplicationName(DataSet ds, int RowIndex)
        {
            string Application0 = ds.Tables[0].Rows[RowIndex]["Application"].ToString();
            string Application = Application0.ToLower();

            if (Application.StartsWith("skyserverws"))// direct call to skyserverws 
            {
                Application = Application0;
            }
            else//use of skyserver tool
            {
                if (Application.Contains("sqlsearch") || Application.Contains("search.sql"))
                    Application = "SQL Search";
                else if (Application.Contains("radial"))
                    Application = "Radial Search";
                else if (Application.Contains("rectangular"))
                    Application = "Rectangular Search";
                else if (Application.Contains("searchform"))
                    Application = "Search Form";
                else if (Application.Contains("iqs"))
                    Application = "Imaging Query";
                else if (Application.Contains("irqs"))
                    Application = "Infrared Spectroscopy Query";
                else if (Application.Contains("sqs"))
                    Application = "Spectroscopy Query";
                else if (Application.Contains("crossid"))
                    Application = "Object Cross-ID";
                else if (Application.Contains(".explore"))
                    Application = "Explore tool";
                else if (Application.Contains(".quicklook"))
                    Application = "QuickLook tool";

                else if (Application.Contains("conesearch"))
                    Application = "Cone Search";
                else if (Application.Contains("userhistory"))
                    Application = "User History";
                else if (Application.Contains("sdssfields"))
                    Application = "SDSS Fields";
                else if (Application.Contains("siap"))
                    Application = "SIAP";
                else
                    Application = Application0;
            }
            ds.Tables[0].Rows[RowIndex]["Application"] = Application;
        }


        public void PrepareResultSet(ref DataSet ds, Dictionary<string, string> requestDir)
        {
            Dictionary<string, string> values;
            bool DoShowInUserHistory = true;

            if (ds.Tables.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    DoShowInUserHistory = true;

                    string json = ds.Tables[0].Rows[i]["content"].ToString();
                    //json = json.Replace("\r\n", "");
                    json = json.TrimStart(new char[] { '[' });
                    json = json.TrimEnd(new char[] { ']' });

                    try
                    {
                        values = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                    }
                    catch { values = null; }

                    if (!String.IsNullOrEmpty(json) && values != null)
                    {

                        if (!DoShowAllHistory)
                        {
                            if (values.ContainsKey("DoShowInUserHistory"))
                                DoShowInUserHistory = values["DoShowInUserHistory"].ToString() == "true" ? true : false;
                        }

                        
                        if (DoShowInUserHistory)
                        {
                            SetApplicationName(ds, i);
                            if (values.ContainsKey("RequestUri"))
                            {

                                Uri req = new Uri(values["RequestUri"].ToString());
                                JObject jsonParams = null;
                                bool success = req.TryReadQueryAsJson(out jsonParams);
                                if (success)
                                    ds.Tables[0].Rows[i]["parameters"] = jsonParams.ToString();


                                if (format == "html" || format.ToLower().Contains("dataset") )
                                {
                                    string Link = values["RequestUri"].ToString();
                                    string Referer = "";
                                    if (values.ContainsKey("Referer"))
                                        Referer = values["Referer"].ToString();

                                    Uri URI = new Uri(values["RequestUri"].ToString());

                                    NameValueCollection Query = URI.ParseQueryString();

                                    bool h1 = values["WebServiceEntryPoint"].Contains("ObjectSearch");
                                    bool h2 = Referer.ToLower().Contains(requestDir["skyserverUrl"].ToLower());

                                    bool h3 = false;
                                    try
                                    {
                                        h3 = Query.Get("query").ToLower() == "loadexplore";
                                    }
                                    catch { }

                                    if (h1 && h2 && h3)
                                    {
                                        
                                        Link = Referer;
                                        
                                        if (values["TaskName"].ToLower().Contains(".quicklook."))
                                            Link = requestDir["skyserverUrl"] + "en/tools/quicklook/summary.aspx?" + URI.Query;
                                        if (values["TaskName"].ToLower().Contains(".explore."))
                                            Link = requestDir["skyserverUrl"] + "en/tools/explore/summary.aspx?" + URI.Query;
                                        
                                    }
                                    if (format == "html")
                                        ds.Tables[0].Rows[i]["content"] = "<a target=INFO href=\"" +  Link + "\">LINK</a>";
                                    else // case of dataset
                                        ds.Tables[0].Rows[i]["content"] = Link;

                                }
                                else
                                {
                                    ds.Tables[0].Rows[i]["content"] = values["RequestUri"].ToString();
                                }

                            }
                            else
                            {
                                ds.Tables[0].Rows[i]["content"] = "";
                            }
                            //dt.Rows.Add(dr.);

                        }
                        else// do not show the info by deleting the row
                        {
                            ds.Tables[0].Rows[i].Delete();
                        }
                        //}
                        //catch (Exception e) { throw new Exception(e.Message + "\n\n" + json + "\n\n"); }
                    }
                    else
                    {
                        ds.Tables[0].Rows[i]["content"] = "";
                    }
                }//end for
                //ds.Tables[0].AcceptChanges();
                ds.AcceptChanges();
            }//end 
        }

        public void PrepareResultSetOLD(ref DataSet ds, Dictionary<string, string> requestDir)
        {
            Dictionary<string, string> values;
            bool DoShowInUserHistory = true;

            if (ds.Tables.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    DoShowInUserHistory = true;

                    string json = ds.Tables[0].Rows[i]["content"].ToString();
                    //json = json.Replace("\r\n", "");
                    json = json.TrimStart(new char[] { '[' });
                    json = json.TrimEnd(new char[] { ']' });

                    try
                    {
                        values = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                    }
                    catch { values = null; }

                    if (!String.IsNullOrEmpty(json) && values != null)
                    {
                        try
                        {
                            values = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                        }
                        catch { }


                        if (!DoShowAllHistory)
                        {
                            if (values.ContainsKey("DoShowInUserHistory"))
                                DoShowInUserHistory = values["DoShowInUserHistory"].ToString() == "true" ? true : false;
                        }


                        if (DoShowInUserHistory)
                        {
                            SetApplicationName(ds, i);
                            if (values.ContainsKey("RequestUri"))
                            {

                                Uri req = new Uri(values["RequestUri"].ToString());
                                JObject jsonParams = null;
                                bool success = req.TryReadQueryAsJson(out jsonParams);
                                if (success)
                                    ds.Tables[0].Rows[i]["parameters"] = jsonParams.ToString();


                                if (format == "html" || format.ToLower().Contains("dataset"))
                                {
                                    string Link = values["RequestUri"].ToString();
                                    string Referer = "";
                                    if (values.ContainsKey("Referer"))
                                        Referer = values["Referer"].ToString();

                                    Uri URI = new Uri(values["RequestUri"].ToString());

                                    NameValueCollection Query = URI.ParseQueryString();

                                    bool h1 = values["WebServiceEntryPoint"].Contains("ObjectSearch");
                                    bool h2 = Referer.ToLower().Contains(requestDir["skyserverUrl"].ToLower());

                                    bool h3 = false;
                                    try
                                    {
                                        h3 = Query.Get("query").ToLower() == "loadexplore";
                                    }
                                    catch { }

                                    if (h1 && h2 && h3)
                                    {

                                        Link = Referer;

                                        if (values["TaskName"].ToLower().Contains(".quicklook."))
                                            Link = requestDir["skyserverUrl"] + "en/tools/quicklook/summary.aspx?" + URI.Query;
                                        if (values["TaskName"].ToLower().Contains(".explore."))
                                            Link = requestDir["skyserverUrl"] + "en/tools/explore/summary.aspx?" + URI.Query;

                                    }
                                    if (format == "html")
                                        ds.Tables[0].Rows[i]["content"] = "<a target=INFO href=\"" + Link + "\">LINK</a>";
                                    else // case of dataset
                                        ds.Tables[0].Rows[i]["content"] = Link;

                                }
                                else
                                {
                                    ds.Tables[0].Rows[i]["content"] = values["RequestUri"].ToString();
                                }

                            }
                            else
                            {
                                ds.Tables[0].Rows[i]["content"] = "";
                            }
                            //dt.Rows.Add(dr.);

                        }
                        else// do not show the info by deleting the row
                        {
                            ds.Tables[0].Rows[i].Delete();
                        }
                        //}
                        //catch (Exception e) { throw new Exception(e.Message + "\n\n" + json + "\n\n"); }
                    }
                    else
                    {
                        ds.Tables[0].Rows[i]["content"] = "";
                    }
                }//end for
                //ds.Tables[0].AcceptChanges();
                ds.AcceptChanges();
            }//end 
        }

        public SqlCommand BuildCommand(SqlConnection oConn)
        {

            SqlCommand cmd = oConn.CreateCommand();
            cmd.CommandText = "SELECT TOP ";
            if (limit != null && limit <= Int64.Parse(KeyWords.MaxRows))
                cmd.CommandText += limit.ToString();
            else
                cmd.CommandText += KeyWords.MaxRows;

            cmd.CommandText += " ROW_NUMBER() OVER( ORDER BY time DESC ) as RowIndex, task_name as Application, FORMAT(Time,'yyyy-MM-dd HH:mm:ss') as Time, content as Content, 'parameters' as Parameters FROM MESSAGES as m LEFT JOIN CustomMessages as c on m.row_id = c.row_id";
            cmd.CommandText += " WHERE user_id= @UserID";
            cmd.Parameters.Add("@UserID", SqlDbType.NVarChar);
            cmd.Parameters["@UserID"].Value = UserID;
            //cmd.CommandText += " WHERE user_id != 'fsdf' ";
            if (Time1 != null)
            {
                cmd.CommandText += " AND Time >= @Time1";
                cmd.Parameters.Add("@Time1", SqlDbType.DateTime);
                cmd.Parameters["@Time1"].Value = Time1.ToString();
            }
            if (Time2 != null)
            {
                cmd.CommandText += " AND Time <= @Time2";
                cmd.Parameters.Add("@Time2", SqlDbType.DateTime);
                cmd.Parameters["@Time2"].Value = Time2.ToString();
            }
            if (!String.IsNullOrEmpty(Application))
            {
                cmd.CommandText += " AND application = @application";
                cmd.Parameters.Add("@application", SqlDbType.NVarChar);
                cmd.Parameters["@application"].Value = Application;
            }
            if (!String.IsNullOrEmpty(Application))
            {
                cmd.CommandText += " AND application = @application";
                cmd.Parameters.Add("@application", SqlDbType.NVarChar);
                cmd.Parameters["@application"].Value = Application;
            }
            if (CustomMessageType != null)
            {
                cmd.CommandText += " AND custom_message_type = @CustomMessageType";
                cmd.Parameters.Add("@CustomMessageType", SqlDbType.Int);
                cmd.Parameters["@CustomMessageType"].Value = CustomMessageType;
            }
            if(!DoShowAllHistory)
            {
                cmd.CommandText += " AND dbo.fJSONgetValue(content,'DoShowInUserHistory') = 'true'";
            }
            /*
            if (!String.IsNullOrEmpty(ContentSearchText))
            {
                cmd.CommandText += " AND content like %@ContentSearchText%";
                cmd.Parameters.Add("@ContentSearchText", SqlDbType.NVarChar);
                cmd.Parameters["@ContentSearchText"].Value = ContentSearchText;
            }
            */ 
            cmd.CommandText += " order by time desc";
            return cmd;
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
}                    /*
                    try
                    {
                        JArray jarr = JArray.Parse(json);
                        int index = jarr.IndexOf("RequestUri");
                        if (index>=0)
                        {
                            ds.Tables[0].Rows[i]["content"] = jarr[index].ToString();
                        }
                        else
                        {
                            ds.Tables[0].Rows[i]["content"] = "";
                        }
                    }
                    catch (Exception e) { throw new Exception(e.Message + "\n\n" + json + "\n\n"); }
                    */
/*
try
{
    JObject jarr = JObject.Parse(json);
    foreach (KeyValuePair<String, JToken> property in jarr)
    //foreach (JToken property in jarr)
    {
        string propertyName = property.Key.ToString();
        if (propertyName == "Headers")
        {
            ds.Tables[0].Rows[i]["content"] = property.Value.ToString();
        }
        else
        {
            ds.Tables[0].Rows[i]["content"] = "";
        }
    }
}
catch (Exception e) { throw new Exception(e.Message + "\n\n" + json + "\n\n"); }
 
*/



/*
                try
                {
                    //dataSet = JsonConvert.DeserializeObject<DataSet>(json);

                    dataSet = (DataSet)JsonConvert.DeserializeObject(json, (typeof(DataSet)));

                    if (dataSet.Tables[0].Columns.Contains("URI"))
                    {
                        ds.Tables[0].Rows[i]["content"] = dataSet.Tables[0].Rows[0]["URI"].ToString();
                    }
                    else
                    {
                        ds.Tables[0].Rows[i]["content"] = "try";
                    }
                }
                catch (Exception e) { throw new Exception(e.Message + "\n\n" + json + "\n\n"); }
 */

