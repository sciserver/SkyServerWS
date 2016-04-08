using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sciserver_webService.Common;
using Sciserver_webService.ConeSearch;
using Sciserver_webService.SDSSFields;
using System.Net;
using System.Data.SqlClient;
using System.Web;

//This class is used to submit query to the database
namespace Sciserver_webService.DoDatabaseQuery
{

    public class RunDBquery : IHttpActionResult
    {
        String query = "";
        String format = "";
        String TaskName = "";
        Dictionary<string, string> ExtraInfo = null;
        string ErrorMessage = "";
        DataSet ResultsDataSet = new DataSet();
        LoggedInfo ActivityInfo;
        String queryType = "";
        String positionType = "";
        HttpResponseMessage response = new HttpResponseMessage();
        System.Text.Encoding tCode = System.Text.Encoding.UTF8;
        SciserverLogging logger;

        public RunDBquery(string query, string format, string TaskName, Dictionary<string, string> ExtraInfo, LoggedInfo ActivityInfo, string queryType, string positionType)
        {
            this.query = query;
            this.format = format;
            this.TaskName = TaskName;
            this.ExtraInfo = ExtraInfo;
            this.ActivityInfo = ActivityInfo;
            this.queryType = queryType;
            this.positionType = positionType;
            this.logger = new SciserverLogging();
            this.logger.AthenticateUser(this.ActivityInfo);

        }

        public async Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            //response = new HttpResponseMessage();
            //HttpResponseMessage respMessage = null;
            //string ResponseResult = "";
            //string requestUri = "";
            try
            {
                if (ExtraInfo["FormatFromUser"] == "mydb")
                {
                    return SendToMyDB();   
                    //response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                }
                else
                {
                    return SendToUser();
                }
                //logging

                //return processDBqueryResults(stream);
            }
            catch (Exception e)
            {
                if (ExtraInfo["DoReturnHtml"].ToLower() == "true")
                {

                    HttpStatusCode errorCode = HttpStatusCode.InternalServerError;
                    string reasonPhrase = errorCode.ToString();
                    string errorMessage = e.Message + ((e.InnerException != null) ? (": " + e.InnerException.Message) : "");

                    SciserverLogging Logger = new SciserverLogging();
                    ActivityInfo.Exception = e;
                    Logger.LogActivity(ActivityInfo, "ErrorMessage");

                    StringBuilder strbldr = new StringBuilder();
                    StringWriter sw = new StringWriter(strbldr);
                    using (JsonWriter writer = new JsonTextWriter(sw))
                    {
                        writer.WriteStartObject();
                        writer.WritePropertyName("ErrorCode");
                        writer.WriteValue((int)errorCode);
                        writer.WritePropertyName("ErrorType");
                        writer.WriteValue(errorCode.ToString());
                        writer.WritePropertyName("ErrorMessage");
                        writer.WriteValue(errorMessage);
                        writer.WritePropertyName("LogMessageID");
                        writer.WriteValue(Logger.message.MessageId);
                        writer.WritePropertyName("LogTime");
                        writer.WriteValue(Logger.message.Time);
                    }
                    string TechnicalErrorInfo = strbldr.ToString();

                    strbldr = new StringBuilder();
                    sw = new StringWriter(strbldr);
                    using (JsonWriter writer = new JsonTextWriter(sw))
                    {
                        writer.WriteStartObject();
                        writer.WritePropertyName("ErrorCode");
                        writer.WriteValue((int)errorCode);
                        writer.WritePropertyName("ErrorType");
                        writer.WriteValue(errorCode.ToString());
                        writer.WritePropertyName("ErrorMessage");
                        writer.WriteValue(errorMessage);
                        writer.WritePropertyName("LogMessageID");
                        writer.WriteValue(Logger.message.MessageId);
                        writer.WritePropertyName("username");
                        writer.WriteValue(Logger.user_name);
                        writer.WritePropertyName("userid");
                        writer.WriteValue(Logger.userid);
                        writer.WritePropertyName("pageurl");
                        writer.WriteValue(ActivityInfo.URI);
                        writer.WritePropertyName("referrer");
                        writer.WriteValue(ActivityInfo.Referrer);
                        writer.WritePropertyName("StackTrace");
                        writer.WriteValue(ActivityInfo.Exception.StackTrace);
                        writer.WritePropertyName("InnerTrace");
                        writer.WriteValue(ActivityInfo.Exception.InnerException != null ? ActivityInfo.Exception.InnerException.StackTrace : "");
                        writer.WritePropertyName("LogTime");
                        writer.WriteValue(Logger.message.Time);
                        writer.WritePropertyName("ClientIP");
                        writer.WriteValue(Logger.message.ClientIP);
                    }
                    string TechnicalErrorInfoAll = strbldr.ToString();

                    bool IsSuccess = false;
                    ProcessDataSet proc = new ProcessDataSet(query, format, TaskName, ExtraInfo, errorMessage, IsSuccess, positionType, queryType, TechnicalErrorInfo, TechnicalErrorInfoAll);
                    response.Content = proc.GetContent(ResultsDataSet);// this will log
                    response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
                    return response;
                }
                else
                {
                    throw new Exception(e.Message);//throwing the exception takes to CustomFilter.cs, where error logging is performed and the error response is returned to the user.
                }
                
            }


        }


        public HttpResponseMessage SendToMyDB()
        {
            HttpResponseMessage respMessage = null;
            string ResponseResult = "";
            string requestUri = "";

            if (logger.IsValidUser)
            {
                string TableName = ExtraInfo["TableName"];

                if (!string.IsNullOrEmpty(TableName))
                {
                    Regex rg = new Regex("[^a-zA-Z0-9]");
                    if (rg.IsMatch(TableName))
                        throw (new Exception("String TableName may only contain letters or numbers."));

                    /*
                    string ForbiddenChar = ",./-?\!";
                    for (int i = 0; i < ForbiddenChar.Length;i++)
                    {
                        if (TableName.Contains(ForbiddenChar.Substring(i,1)))
                            throw (new Exception("TableName may not contain characters like " + ForbiddenChar ));
                    }
*/
                    requestUri = ConfigurationManager.AppSettings["CASJobsREST"] + "contexts/MyDB/tables/";
                    HttpClient client = new HttpClient();
                    client.Timeout = new TimeSpan(0, 0, 0, KeyWords.TimeoutCASJobs);// default is 100000ms
                    client.BaseAddress = new Uri(requestUri);
                    client.DefaultRequestHeaders.Add("X-Auth-Token", logger.Token);
                    respMessage = client.GetAsync(requestUri).Result;
                    ResponseResult = respMessage.Content.ReadAsStringAsync().Result;
                    if (!respMessage.IsSuccessStatusCode)
                    {
                        if (ExtraInfo["DoReturnHtml"].ToLower() == "false")
                            CreateErrorResponseMessageJSON(ResponseResult);
                        else
                            CreateErrorResponseMessageHTML(ResponseResult);

                        logger.LogActivity(ActivityInfo, "SkyserverMessage");
                        return response;
                    }

                    Dictionary<string, string> values;
                    Newtonsoft.Json.Linq.JArray array = (Newtonsoft.Json.Linq.JArray)JsonConvert.DeserializeObject(ResponseResult);//ResponseResult comes in json format
                    for (int i = 0; i < array.Count; i++)
                    {
                        values = JsonConvert.DeserializeObject<Dictionary<string, string>>(array[i].ToString());
                        if (values["Name"] == TableName)
                            throw (new Exception("Table \"" + TableName + "\" already exists in MyDB. Try changing the table name."));
                    }
                }
                else// create a table name
                {
                    //DateTime2 now = DateTime2.Now;
                    TableName = "Table_" + DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
                }

/*
                if (!(ExtraInfo["EntryPoint"].ToLower().Contains("sqlsearch") || ExtraInfo["EntryPoint"].ToLower().Contains("crossid") || ExtraInfo["EntryPoint"].ToLower().Contains("proximity")))// sending query as a job
                {
                    string queryResult = "";
                    StringBuilder strbldr = new StringBuilder();
                    StringWriter sw = new StringWriter(strbldr);
                    using (JsonWriter writer = new JsonTextWriter(sw))
                    {
                        writer.WriteStartObject();
                        writer.WritePropertyName("query");
                        writer.WriteValue(ExtraInfo["QueryForUserDisplay"]);
                        writer.WritePropertyName("TaskName");
                        writer.WriteValue("SkyserverWS.SendToMyDB");
                        writer.WritePropertyName("CreateTable");
                        writer.WriteValue("true");
                        writer.WritePropertyName("TableName");
                        writer.WriteValue(TableName);
                    }

                    StringContent content = new StringContent(strbldr.ToString());
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    content.Headers.Add("X-Auth-Token", logger.Token);

                    //posting the request and getting the result back.
                    HttpClient client2 = new HttpClient();
                    client2.Timeout = new TimeSpan(0, 0, 0, KeyWords.TimeoutCASJobs);// default is 100000ms
                    requestUri = ConfigurationManager.AppSettings["CASJobsREST"] + "contexts/" + KeyWords.DataRelease + "/jobs";
                    client2.BaseAddress = new Uri(requestUri);
                    respMessage = client2.PutAsync(requestUri, content).Result;
                    string JobID = "UNKNOWN";
                    if (respMessage.IsSuccessStatusCode)
                    {
                        JobID = respMessage.Content.ReadAsStringAsync().Result;
                        if (ExtraInfo["DoReturnHtml"].ToLower() == "false")
                        {
                            queryResult = "[{\"JobID\": \"" + JobID + "\", \"TableName\"= \"" + TableName + "\"}]";
                            response.Content = new StringContent(queryResult, tCode, KeyWords.contentJson);
                            response.Content.Headers.ContentType = new MediaTypeHeaderValue(KeyWords.contentJson);
                        }
                        else
                        {
                            ProcessDataSet proc = new ProcessDataSet(query, format, TaskName, ExtraInfo, null, true, positionType, queryType, null, null);
                            queryResult = proc.getCasJobsSubmitHTMLresult(JobID, TableName, logger.Token);
                            response.Content = new StringContent(queryResult, tCode, KeyWords.contentHTML);
                            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
                        }
                    }
                    else
                    {
                        string ErrorMessage = respMessage.Content.ReadAsStringAsync().Result;
                        if (ExtraInfo["DoReturnHtml"].ToLower() == "false")
                            CreateErrorResponseMessageJSON(ErrorMessage);
                        else
                            CreateErrorResponseMessageHTML(ErrorMessage);
                    }

                    logger.LogActivity(ActivityInfo, "SkyserverMessage");
                    return response;

                }
                else // sending query results as a csv
                {
                }
*/

                //sending query result as a csv:
                string queryResult = "";
                RunQuery();
                Action<Stream, HttpContent, TransportContext> WriteToStream = null;
                DataSet ds = new DataSet();
                ds.Tables.Add(ResultsDataSet.Tables[0].Copy());
                WriteToStream = (stream, foo, bar) => { OutputUtils.writeCSV(ds, stream, false); stream.Close(); };
                HttpContent content = new PushStreamContent(WriteToStream, new MediaTypeHeaderValue((KeyWords.contentCSV)));
                //content.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
                content.Headers.Add("X-Auth-Token", logger.Token);
                requestUri = ConfigurationManager.AppSettings["CASJobsREST"] + "contexts/MyDB/tables/" + TableName;
                HttpClient client2 = new HttpClient();
                client2.Timeout = new TimeSpan(0, 0, 0, KeyWords.TimeoutCASJobs);// default is 100000ms
                client2.BaseAddress = new Uri(requestUri);
                respMessage = client2.PostAsync(requestUri, content).Result;
                if (respMessage.IsSuccessStatusCode)
                {
                    if (ExtraInfo["DoReturnHtml"].ToLower() == "false")
                    {
                        queryResult = "[{\"IsSuccessStatusCode\": \"true\", \"TableName\"= \"" + TableName + "\"}]";
                        response.Content = new StringContent(queryResult, tCode, KeyWords.contentJson);
                        response.Content.Headers.ContentType = new MediaTypeHeaderValue(KeyWords.contentJson);
                    }
                    else
                    {
                        ProcessDataSet proc = new ProcessDataSet(query, format, TaskName, ExtraInfo, null, true, positionType, queryType, null, null);
                        queryResult = proc.getTableSubmitHTMLresult(TableName, logger.Token);
                        response.Content = new StringContent(queryResult, tCode, KeyWords.contentHTML);
                        response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
                    }
                }
                else
                {
                    string ErrorMessage = respMessage.Content.ReadAsStringAsync().Result;
                    if (ExtraInfo["DoReturnHtml"].ToLower() == "false")
                        CreateErrorResponseMessageJSON(ErrorMessage);
                    else
                        CreateErrorResponseMessageHTML(ErrorMessage);

                }

                logger.LogActivity(ActivityInfo, "SkyserverMessage");
                return response;


            }
            else
            {
                throw (new UnauthorizedAccessException("You are not logged-in with SciServer. Please log-in and try again."));
            }
        }


        public void CreateErrorResponseMessageHTML(string ErrorMessage)
        {
            //Newtonsoft.Json.Linq.JArray array = (Newtonsoft.Json.Linq.JArray)JsonConvert.DeserializeObject(ErrorMessage);//ResponseResult comes in json format
            Dictionary<string, string> values = JsonConvert.DeserializeObject<Dictionary<string, string>>(ErrorMessage);
            string error = values.ContainsKey("Error Message") ? values["Error Message"] : "UNKNOWN";

            try { values.Add("userid", logger.user_name); }
            catch { };
            try { values.Add("username", logger.user_name); }
            catch { };
            try { values.Add("pageurl", ActivityInfo.URI.ToString()); }
            catch { };
            try { values.Add("referrer", ActivityInfo.Referrer); }
            catch { };
            string TechnicalInfoAll = JsonConvert.SerializeObject(values);

            ProcessDataSet proc = new ProcessDataSet(query, format, TaskName, ExtraInfo, error, false, positionType, queryType, ErrorMessage, TechnicalInfoAll);
            response.Content = new StringContent(proc.getGenericCasJobsHTMLerror(error), tCode, KeyWords.contentHTML);
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
        }

        public void CreateErrorResponseMessageJSON(string ErrorMessage)
        {
            response.Content = new StringContent(ErrorMessage, tCode, KeyWords.contentJson);
            response.Content.Headers.ContentType = new MediaTypeHeaderValue(KeyWords.contentJson);
        }


        public void RunQuery()
        {
            using (SqlConnection Conn = new SqlConnection(KeyWords.DBconnectionString))
            {
                Conn.Open();
                SqlCommand Cmd = Conn.CreateCommand();
                Cmd.CommandText = this.query;
                //Cmd.CommandTimeout = KeyWords.DatabaseSearchTimeout == null || KeyWords.DatabaseSearchTimeout == "" ? 600 : Int32.Parse(KeyWords.DatabaseSearchTimeout);
                Cmd.CommandTimeout = Int32.Parse(KeyWords.DatabaseSearchTimeout);
                //SqlDataReader reader = await Cmd.ExecuteReaderAsync();
                var Adapter = new SqlDataAdapter(Cmd);
                Adapter.Fill(ResultsDataSet);
                Conn.Close();
            }
        }

        public HttpResponseMessage SendToUser()
        {

            /*
                    SqlConnection Conn = new SqlConnection(KeyWords.DBconnectionString);
                    await Conn.OpenAsync();
                    SqlCommand Cmd = Conn.CreateCommand();
                    Cmd.CommandText = this.query;
                    //Cmd.CommandTimeout = KeyWords.DatabaseSearchTimeout == null || KeyWords.DatabaseSearchTimeout == "" ? 600 : Int32.Parse(KeyWords.DatabaseSearchTimeout);
                    Cmd.CommandTimeout = Int32.Parse(KeyWords.DatabaseSearchTimeout);
                    //SqlDataReader reader = await Cmd.ExecuteReaderAsync();
                    var Adapter = new SqlDataAdapter(Cmd);
                    Adapter.Fill(ResultsDataSet);
                    Conn.Close();
*/
            RunQuery();

            //BinaryFormatter fmt = new BinaryFormatter();
            Action<Stream, HttpContent, TransportContext> WriteToStream = null;
            BinaryFormatter fmt;

            // do not add the SQL query as a second table in vo services and csv/txt/fits formats.
            if (!format.Contains("csv") && !format.Contains("txt") && !format.Contains("text/plain") && !format.Contains("fits") && queryType != KeyWords.SDSSFields && queryType != KeyWords.ConeSearchQuery && queryType != KeyWords.SIAP)
            {
                AddQueryTable(ResultsDataSet);// this adds to "ResultsDataSet" a new Table that shows the sql command.
            }

            string FileType = "";
            ExtraInfo.TryGetValue("FormatFromUser", out FileType);
            string SaveResult = "";
            ExtraInfo.TryGetValue("SaveResult", out SaveResult);

            switch (format)
            {
                case "csv":
                case "txt":
                case "text/plain":
                    ResultsDataSet.RemotingFormat = SerializationFormat.Xml;
                    WriteToStream = (stream, foo, bar) => { OutputUtils.writeCSV(ResultsDataSet, stream); stream.Close(); };
                    response.Content = new PushStreamContent(WriteToStream, new MediaTypeHeaderValue((KeyWords.contentCSV)));
                    if (FileType == "csv")
                        FileType = ".csv";
                    else
                        FileType = ".txt";
                    if (SaveResult == "true")
                        response.Content.Headers.Add("Content-Disposition", "attachment;filename=\"result" + FileType + "\"");
                    break;
                case "fits":
                case "application/fits":
                    ResultsDataSet.RemotingFormat = SerializationFormat.Binary;
                    WriteToStream = (stream, foo, bar) => { OutputUtils.WriteFits(ResultsDataSet, stream); stream.Close(); };
                    response.Content = new PushStreamContent(WriteToStream, new MediaTypeHeaderValue((KeyWords.contentFITS)));
                    if (SaveResult == "true")
                        response.Content.Headers.Add("Content-Disposition", "attachment;filename=\"result.fits\"");
                    break;
                case "votable":
                case "application/x-votable+xml":
                    ResultsDataSet.RemotingFormat = SerializationFormat.Xml;
                    WriteToStream = (stream, foo, bar) => { OutputUtils.WriteVOTable(ResultsDataSet, stream); stream.Close(); };
                    response.Content = new PushStreamContent(WriteToStream, new MediaTypeHeaderValue((KeyWords.contentVOTable)));
                    if (SaveResult == "true")
                        response.Content.Headers.Add("Content-Disposition", "attachment;filename=\"result.votable.xml\"");
                    break;
                case "xml":
                case "application/xml":
                    ResultsDataSet.RemotingFormat = SerializationFormat.Xml;
                    WriteToStream = (stream, foo, bar) => { OutputUtils.WriteXml(ResultsDataSet, stream); stream.Close(); };
                    response.Content = new PushStreamContent(WriteToStream, new MediaTypeHeaderValue((KeyWords.contentXML)));
                    if (SaveResult == "true")
                        response.Content.Headers.Add("Content-Disposition", "attachment;filename=\"result.xml\"");
                    break;
                case "json":
                case "application/json":
                    ResultsDataSet.RemotingFormat = SerializationFormat.Xml;
                    WriteToStream = (stream, foo, bar) => { OutputUtils.WriteJson(ResultsDataSet, stream); stream.Close(); };
                    response.Content = new PushStreamContent(WriteToStream, new MediaTypeHeaderValue((KeyWords.contentJson)));
                    if (SaveResult == "true")
                        response.Content.Headers.Add("Content-Disposition", "attachment;filename=\"result.json\"");
                    break;
                case "html":
                case "dataset":
                case "application/x-dataset":
                    ProcessDataSet proc = new ProcessDataSet(query, format, TaskName, ExtraInfo, null, true, positionType, queryType, null, null);
                    response.Content = proc.GetContent(ResultsDataSet);
                    if (ExtraInfo.ContainsKey("FormatFromUser"))
                    {
                        if (ExtraInfo["FormatFromUser"] == "html")
                        {
                            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
                        }
                    }
                    //ResultsDataSet.RemotingFormat = SerializationFormat.Binary;
                    //fmt = new BinaryFormatter();
                    //WriteToStream = (stream, foo, bar) => { fmt.Serialize(stream, ResultsDataSet); stream.Close(); };
                    //response.Content = new PushStreamContent(WriteToStream, new MediaTypeHeaderValue(KeyWords.contentDataset));
                    break;
                default:
                    ResultsDataSet.RemotingFormat = SerializationFormat.Binary;
                    fmt = new BinaryFormatter();
                    WriteToStream = (stream, foo, bar) => { fmt.Serialize(stream, ResultsDataSet); stream.Close(); };
                    response.Content = new PushStreamContent(WriteToStream, new MediaTypeHeaderValue((KeyWords.contentDataset)));
                    break;
            }
            //reader.Close();
            //response.Content = new StringContent(ClientIP);
            logger.LogActivity(ActivityInfo, "SkyserverMessage");
            return response;


        }


        public void AddQueryTable(DataSet ResultsDataSet)
        {
            if (ExtraInfo["QueryForUserDisplay"] != null)
            {
                DataTable dt = new DataTable("SqlQuery");
                dt.Columns.Add("query", typeof(string));

                string Query = ExtraInfo["QueryForUserDisplay"];
                Query = Regex.Replace(Query, @"\/\*(.*\n)*\*\/", "");	                                // remove all multi-line comments
                Query = Regex.Replace(Query, @"^[ \t\f\v]*--.*\r\n", "", RegexOptions.Multiline);		// remove all isolated single-line comments
                Query = Regex.Replace(Query, @"--[^\r^\n]*", "");				                        // remove all embedded single-line comments
                Query = Regex.Replace(Query, @"[ \t\f\v]+", " ");				                        // replace multiple whitespace with single space
                Query = Regex.Replace(Query, @"^[ \t\f\v]*\r\n", "", RegexOptions.Multiline);			// remove empty lines          

                dt.Rows.Add(new object[] { Query });
                ResultsDataSet.Merge(dt);
            }
        }





    }
}