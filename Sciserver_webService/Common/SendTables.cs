using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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

using net.ivoa.data;
using Jhu.SharpFitsIO;




namespace Sciserver_webService.Common
{
    public class SendTables : IHttpActionResult
    {

        String Format = "";
        DataSet ResultsDataSet;
        LoggedInfo ActivityInfo;
        Dictionary<string, string> ExtraInfo = new Dictionary<string, string>();

        public SendTables()
        {
            this.Format = "";
            this.ResultsDataSet = new DataSet();
            this.ActivityInfo = new LoggedInfo();
        }
        public SendTables(DataSet ResultsDataSet, string Format, LoggedInfo ActivityInfo, Dictionary<string, string> ExtraInfo)
        {
            this.Format = Format;
            this.ResultsDataSet = ResultsDataSet;
            this.ActivityInfo = ActivityInfo;
            this.ExtraInfo = ExtraInfo;
        }

        public async Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage();
            try
            {
                Action<Stream, HttpContent, TransportContext> WriteToStream = null;

                string FileType = "";
                ExtraInfo.TryGetValue("FormatFromUser", out FileType);
                string SaveResult = "";
                ExtraInfo.TryGetValue("SaveResult", out SaveResult);

                switch (Format)
                {
                    case "csv":
                    case "txt":
                    case "text/plain":
                        ResultsDataSet.RemotingFormat = SerializationFormat.Xml;
                        WriteToStream = (stream, foo, bar) => { OutputUtils.writeCSV(ResultsDataSet, stream); };
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
                        WriteToStream = (stream, foo, bar) => { OutputUtils.WriteFits(ResultsDataSet, stream); };
                        response.Content = new PushStreamContent(WriteToStream, new MediaTypeHeaderValue((KeyWords.contentFITS)));
                        if (SaveResult == "true")
                            response.Content.Headers.Add("Content-Disposition", "attachment;filename=\"result.fits\"");
                        break;
                    case "votable":
                    case "application/x-votable+xml":
                        ResultsDataSet.RemotingFormat = SerializationFormat.Xml;
                        WriteToStream = (stream, foo, bar) => { OutputUtils.WriteVOTable(ResultsDataSet, stream); };
                        response.Content = new PushStreamContent(WriteToStream, new MediaTypeHeaderValue((KeyWords.contentVOTable)));
                        if (SaveResult == "true")
                            response.Content.Headers.Add("Content-Disposition", "attachment;filename=\"result.votable.xml\"");
                        break;
                    case "xml":
                    case "application/xml":
                        ResultsDataSet.RemotingFormat = SerializationFormat.Xml;
                        WriteToStream = (stream, foo, bar) => { OutputUtils.WriteXml(ResultsDataSet, stream); };
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
                    case "dataset":
                    case "application/x-dataset":
                        if (ExtraInfo.ContainsKey("FormatFromUser"))
                        {
                            if (ExtraInfo["FormatFromUser"] == "html")
                            {
                                ResultsDataSet.RemotingFormat = SerializationFormat.Xml;
                                WriteToStream = (stream, foo, bar) => { OutputUtils.WriteHTML(ResultsDataSet, stream); };
                                response.Content = new PushStreamContent(WriteToStream, new MediaTypeHeaderValue((KeyWords.contentHTML)));
                            }
                            else
                            {
                                BinaryFormatter fmt = new BinaryFormatter();
                                ResultsDataSet.RemotingFormat = SerializationFormat.Binary;
                                WriteToStream = (stream, foo, bar) => { fmt.Serialize(stream, ResultsDataSet); stream.Close(); };
                                response.Content = new PushStreamContent(WriteToStream, new MediaTypeHeaderValue(KeyWords.contentDataset));
                            }
                        }
                        break;
                    default:
                        ResultsDataSet.RemotingFormat = SerializationFormat.Xml;
                        WriteToStream = (stream, foo, bar) => { OutputUtils.WriteJson(ResultsDataSet, stream); stream.Close(); };
                        response.Content = new PushStreamContent(WriteToStream, new MediaTypeHeaderValue((KeyWords.contentJson)));
                        if (SaveResult == "true")
                            response.Content.Headers.Add("Content-Disposition", "attachment;filename=\"result.json\"");
                        break;
                }

                //logging 
                SciserverLogging logger = new SciserverLogging();
                //logger.LogActivity(ActivityInfo, "CustomMessage");
                logger.LogActivity(ActivityInfo, "SkyserverMessage");

                return response;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }


        }

    }
}