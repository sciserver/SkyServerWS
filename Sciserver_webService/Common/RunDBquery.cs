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
        string ErrorMessage = "There no errors.";
        DataSet ResultsDataSet = new DataSet();
        LoggedInfo ActivityInfo;
        String queryType = "";
        String positionType = "";

        public RunDBquery(string query, string format, string TaskName, Dictionary<string, string> ExtraInfo, LoggedInfo ActivityInfo, string queryType, string positionType)
        {
            this.query = query;
            this.format = format;
            this.TaskName = TaskName;
            this.ExtraInfo = ExtraInfo;
            this.ActivityInfo = ActivityInfo;
            this.queryType = queryType;
            this.positionType = positionType;
        }


        public async Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage();
            try
            {
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
                //BinaryFormatter fmt = new BinaryFormatter();
                Action<Stream, HttpContent, TransportContext> WriteToStream = null;
                BinaryFormatter fmt;

                // do not add the SQL query as a second table in vo services and csv/txt/fits formats.
                if (!format.Contains("csv") && !format.Contains("txt") && !format.Contains("text/plain") && !format.Contains("fits") && queryType != KeyWords.SDSSFields && queryType != KeyWords.ConeSearchQuery && queryType != KeyWords.SIAP)
                {
                    AddQueryTable(ResultsDataSet);// this adds to "ResultsDataSet" a new Table that shows the sql command.
                }

                switch (format)
                {
                    case "csv":
                    case "txt":
                    case "text/plain":
                        ResultsDataSet.RemotingFormat = SerializationFormat.Xml;
                        WriteToStream = (stream, foo, bar) => { OutputUtils.writeCSV(ResultsDataSet, stream); stream.Close(); };
                        response.Content = new PushStreamContent(WriteToStream, new MediaTypeHeaderValue((KeyWords.contentCSV)));
                        break;
                    case "fits":
                    case "application/fits":
                        ResultsDataSet.RemotingFormat = SerializationFormat.Binary;
                        WriteToStream = (stream, foo, bar) => { OutputUtils.WriteFits(ResultsDataSet, stream); stream.Close(); };
                        response.Content = new PushStreamContent(WriteToStream, new MediaTypeHeaderValue((KeyWords.contentFITS)));
                        response.Content.Headers.Add("Content-Disposition", "attachment;filename=\"result.fits\"");
                        break;
                    case "votable":
                    case "application/x-votable+xml":
                        ResultsDataSet.RemotingFormat = SerializationFormat.Xml;
                        WriteToStream = (stream, foo, bar) => { OutputUtils.WriteVOTable(ResultsDataSet, stream); stream.Close(); };
                        response.Content = new PushStreamContent(WriteToStream, new MediaTypeHeaderValue((KeyWords.contentVOTable)));
                        break;
                    case "xml":
                    case "application/xml":
                        ResultsDataSet.RemotingFormat = SerializationFormat.Xml;
                        WriteToStream = (stream, foo, bar) => { OutputUtils.WriteXml(ResultsDataSet, stream); stream.Close(); };
                        response.Content = new PushStreamContent(WriteToStream, new MediaTypeHeaderValue((KeyWords.contentXML)));
                        break;
                    case "json":
                    case "application/json":
                        ResultsDataSet.RemotingFormat = SerializationFormat.Xml;
                        WriteToStream = (stream, foo, bar) => { OutputUtils.WriteJson(ResultsDataSet, stream); stream.Close(); };
                        response.Content = new PushStreamContent(WriteToStream, new MediaTypeHeaderValue((KeyWords.contentJson)));
                        break;
                    case "html":
                    case "dataset":
                    case "application/x-dataset":
                        ProcessDataSet proc = new ProcessDataSet(query, format, TaskName, ExtraInfo, ErrorMessage, true, positionType, queryType);
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


                //logging 
                SciserverLogging logger = new SciserverLogging();
                logger.LogActivity(ActivityInfo, "CustomMessage");


                return response;
                //return processDBqueryResults(stream);
            }
            catch (Exception e)
            {
                if (ExtraInfo["FormatFromUser"] == "html")
                {
                    ErrorMessage = e.Message;
                    ProcessDataSet proc = new ProcessDataSet(query, format, TaskName, ExtraInfo, ErrorMessage, false, positionType, queryType);
                    response.Content = proc.GetContent(ResultsDataSet);
                }
                else
                {
                    throw new Exception(e.Message);
                    //response.Content = new StringContent("There is an error running this Query:\n" + query + "\n\n" + "Extra information:\n" + e.Message);
                }
                return response;
            }


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