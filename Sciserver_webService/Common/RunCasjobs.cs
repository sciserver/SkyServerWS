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




///This class is used to submit query to casjobs
namespace Sciserver_webService.UseCasjobs
{

    public class RunCasjobs  : IHttpActionResult 
    {
        String query = "";
        String casjobsTaskName = "";
        String returnType = "";
        String token ="";
        String casjobsTarget = "";
        Dictionary<string, string> ExtraInfo = null;


        net.ivoa.VOTable.VOTABLE vot;

        public RunCasjobs(string query, String token, string casjobsTaskName, string returnType, string target) {
            this.query = query;
            this.token = token;
            this.casjobsTaskName = casjobsTaskName;
            this.returnType = returnType;
            this.casjobsTarget = target;
        }

        public RunCasjobs(string query, String token, string casjobsTaskName, string returnType, string target, Dictionary<string, string> ExtraInfo)
        {
            this.query = query;
            this.token = token;
            this.casjobsTaskName = casjobsTaskName;
            this.returnType = returnType;
            this.casjobsTarget = target;
            this.ExtraInfo = ExtraInfo;
        }



        public async Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(KeyWords.casjobsREST + "contexts/" + casjobsTarget + "/query");

            StringContent content = new StringContent(this.getJsonContent(query, casjobsTaskName));
            if (!(token == null || token == String.Empty))
                content.Headers.Add(KeyWords.xauth, token);
            content.Headers.ContentType = new MediaTypeHeaderValue(KeyWords.contentJson);

            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(returnType));//"application/json"));
            client.Timeout = new TimeSpan(0,0,0,KeyWords.TimeoutCASJobs);// default is 100000ms
            System.IO.Stream stream = await client.PostAsync(client.BaseAddress, content).Result.Content.ReadAsStreamAsync();

            return processCasjobsResults(stream);
        }


        private HttpResponseMessage processCasjobsResults(Stream stream) {

            var response = new HttpResponseMessage();
            DataSet ds;
            if (returnType.Equals(KeyWords.contentDataset) && this.ExtraInfo["FormatFromUser"] != "html")
            {
                BinaryFormatter formatter = new BinaryFormatter();
                ds = (DataSet)formatter.Deserialize(stream);
                NewSDSSFields sf = new NewSDSSFields();
                switch (casjobsTaskName)
                {

                    case "FOR CONE SEARCH":
                        DefaultCone cstest = new DefaultCone();
                        vot = cstest.ConeSearch(ds);
                        response.Content = new StringContent(ToXML(vot), Encoding.UTF8, "application/xml");
                        break;

                    case "SDSSFields:FieldArray":

                        response.Content = new StringContent(ToXML(sf.FieldArray(ds)), Encoding.UTF8, "application/xml");
                        break;

                    case "SDSSFields:FieldArrayRect":

                        response.Content = new StringContent(ToXML(sf.FieldArrayRect(ds)), Encoding.UTF8, "application/xml");
                        break;

                    case "SDSSFields:ListOfFields":

                        response.Content = new StringContent(ToXML(sf.ListOfFields(ds)), Encoding.UTF8, "application/xml");
                        break;

                    case "SDSSFields:UrlsOfFields":

                        response.Content = new StringContent(ToXML(sf.UrlOfFields(ds)), Encoding.UTF8, "application/xml");
                        break;

                    default:
                        response.Content = new StreamContent(stream);
                        break;
                }

            }
            else
            {
                if (ExtraInfo["FormatFromUser"] == "html")
                {
                    response.Content = new StringContent(getHtmlContent(ref stream));
                }
                else
                {
                    response.Content = new StreamContent(stream);
                }
            
            }            
            return response;
        }





        private String getHtmlContent(ref Stream stream)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            //string syntax = ExtraInfo.ContainsKey("syntax") ? ExtraInfo["syntax"] : "NoSyntax";
            this.query = ExtraInfo["QueryForUserDisplay"] == "" ? this.query : ExtraInfo["QueryForUserDisplay"];
            try
            {
                DataSet ds = (DataSet)formatter.Deserialize(stream);
                if (ExtraInfo["syntax"] == "Syntax") // in case user want only to verify the syntax of the sql command:
                {
                    return getOKsyntaxHTMLresult(ref ds);
                }
                else if (ExtraInfo["fp"] == "only") // in case user want to run the "is in footprint?" query
                {
                    return getFootprintTableHTMLresult(ref ds);
                }
                else // in case we want to run que sql command and retrrieve the resultset:
                {
                    return getTableHTMLresult(ref ds);
                }
            }
            catch // if the stream (s results table) is not deserialized and casted properly, means that it is really an error message.
            {
                if (ExtraInfo["syntax"] == "Syntax") // in case user want only to verify the syntax of the sql command:
                {
                    return getINCORRECTsyntaxHTMLresult(ref stream);
                }
                else // in case we want to run que sql command and retrrieve the resultset:
                {
                    return getGenericHTMLerror(ref stream);
                }
            }
        }

        private string getFootprintTableHTMLresult(ref DataSet ds)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("<html><head>\n");
            sb.AppendFormat("<title>SDSS Footprint Check</title>\n");
            sb.AppendFormat("</head><body bgcolor=white>\n");
            sb.AppendFormat("<h2>SDSS Footprint Check</h2>");
            sb.AppendFormat("<H3> <font>" + ds.Tables[0].Rows[0][0].ToString() + "</font></H3>\n");
            sb.AppendFormat("</BODY></HTML>\n");
            return sb.ToString();
        }
        


        private string getOKsyntaxHTMLresult(ref DataSet ds)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("<html><head>\n");
            sb.AppendFormat("<title>SDSS Query Syntax Check</title>\n");
            sb.AppendFormat("</head><body bgcolor=white>\n");
            sb.AppendFormat("<h2>SQL Syntax Check</h2>");
            sb.AppendFormat("<H3> <font color=green>" + ds.Tables[0].Rows[0][0].ToString() + "</font></H3>\n");
            sb.AppendFormat("<h3>Your SQL command was: <br><pre>" + ExtraInfo["QueryForUserDisplay"] + "</pre></h3><hr>"); // writes command
            sb.AppendFormat("</BODY></HTML>\n");
            return sb.ToString();
        }

        private string getINCORRECTsyntaxHTMLresult(ref Stream stream)
        {
            StreamContent content = new StreamContent(stream);
            string ErrorMessage = content.ReadAsStringAsync().Result;// e.g.: ",\"Error Type\":\"InternalServerError\",\"Error Message\":\"Failed to execute a query: Incorrect syntax near 'Frame'.\",\"LogMessageID\":\"3c152f69-5042-45de-b3aa-d40795e7953e\"}"

            string message = "";
            string[] Expressions = new string[] { "\"Error Message\":\"(.*?)\"", "\"Error Message\": \"(.*?)\"", "\"Error Message\" :\"(.*?)\"", "\"Error Message\" : \"(.*?)\"" };
            foreach (string expresion in Expressions)
            {
                Regex regex = new Regex(expresion);
                var v = regex.Match(ErrorMessage);
                message = v.Groups[1].ToString();
                if (message != "")
                    break;
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("<html><head>\n");
            sb.AppendFormat("<title>SDSS Query Syntax Check</title>\n");
            sb.AppendFormat("</head><body bgcolor=white>\n");
            sb.AppendFormat("<h2>SQL Syntax Check</h2>");
            sb.AppendFormat("<H3 BGCOLOR=pink><font color=red>SQL returned the following error: <br>     " + message + "</font></H3>");
            sb.AppendFormat("<h3>Your SQL command was: <br><pre>" + ExtraInfo["QueryForUserDisplay"] + "</pre></h3><hr>"); // writes command
            sb.AppendFormat("</BODY></HTML>\n");
            return sb.ToString();

        }


        private string getGenericHTMLerror(ref Stream stream)
        {
            StreamContent content = new StreamContent(stream);
            string ErrorMessage = content.ReadAsStringAsync().Result;// e.g.: ",\"Error Type\":\"InternalServerError\",\"Error Message\":\"Failed to execute a query: Incorrect syntax near 'Frame'.\",\"LogMessageID\":\"3c152f69-5042-45de-b3aa-d40795e7953e\"}"
            Regex regex = new Regex(",\"Error Message\":\"(.*?)\"");
            var v = regex.Match(ErrorMessage);
            ErrorMessage = v.Groups[1].ToString();

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("<html><head>\n");
            sb.AppendFormat("<title>SDSS error message</title>\n");
            sb.AppendFormat("</head><body bgcolor=white>\n");
            sb.AppendFormat("<h2>SDSS error message</h2>");
            sb.AppendFormat("<H3 BGCOLOR=pink><font color=red>SQL returned the following error: <br>     " + ErrorMessage + "</font></H3>");
            sb.AppendFormat("<h3>Your SQL command was: <br><pre>" + ExtraInfo["QueryForUserDisplay"] + "</pre></h3><hr>"); // writes command
            sb.AppendFormat("</BODY></HTML>\n");
            return sb.ToString();

        }


        private string getTableHTMLresult(ref DataSet ds)
        {

            string ColumnName = "";

            string[] Queries = { this.query };
            char[] splitter = { ';' };
            if (this.query.Contains(splitter[0].ToString()))
            {
                Queries = this.query.Split(splitter);
            }
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("<html><head>\n");
            sb.AppendFormat("<title>SDSS Query Results</title>\n");
            sb.AppendFormat("</head><body bgcolor=white>\n");
            int NumTables = ds.Tables.Count;
            string [] QueryTitle = new string[NumTables];
            if ((this.casjobsTaskName.Contains(KeyWords.RadialQuery) || this.casjobsTaskName.Contains(KeyWords.RectangularQuery)) && NumTables == 2)
            {
                    QueryTitle[0] = "Imaging";
                    QueryTitle[1] = "Infrared Spectra";
            }

            for (int t = 0; t < NumTables; t++)
            {
                string[] runs = null, reruns = null, camcols = null, fields = null, plates = null, mjds = null, spreruns = null, fibers = null;
                bool run = false, rerun = false, camcol = false, field = false, dasFields = false;
                bool plate = false, mjd = false, sprerun = false, fiber = false, dasSpectra = false;
                int runI = -1, rerunI = -1, camcolI = -1, fieldI = -1;
                int plateI = -1, mjdI = -1, sprerunI = -1, fiberI = -1;
                int DefaultSpRerun = int.Parse(KeyWords.defaultSpRerun);

                sb.AppendFormat("<h1>"+QueryTitle[t]+"</h1>");
                sb.AppendFormat("<h3>Your SQL command was: <br><pre>" + Queries[t] + "</pre></h3>"); // writes command
                int NumRows = ds.Tables[t].Rows.Count;
                if (NumRows == 0)
                {
                    sb.AppendFormat("<h3><br><font color=red>No objects have been found</font> </h3>");
                }
                else
                {
                    for (int r = 0; r < NumRows; r++)
                    {
                        int NumColumns = ds.Tables[t].Columns.Count;
                        if (r == 0)// filling the first row with the names of the columns
                        {
                            sb.AppendFormat("<table border='1' BGCOLOR=cornsilk>\n");
                            sb.AppendFormat("<tr align=center>");
                            for (int c = 0; c < NumColumns; c++)
                            {
                                ColumnName = ds.Tables[t].Columns[c].ColumnName;
                                sb.AppendFormat("<td><font size=-1>{0}</font></td>", ColumnName);
                                switch (ColumnName.ToLower())
                                {
                                    case "run":
                                        run = true;
                                        runI = c;
                                        break;
                                    case "rerun":
                                        rerun = true;
                                        rerunI = c;
                                        break;
                                    case "camcol":
                                        camcol = true;
                                        camcolI = c;
                                        break;
                                    case "field":
                                        field = true;
                                        fieldI = c;
                                        break;
                                    case "plate":
                                        plate = true;
                                        plateI = c;
                                        break;
                                    case "mjd":
                                        mjd = true;
                                        mjdI = c;
                                        break;
                                    case "sprerun":
                                        sprerun = true;
                                        sprerunI = c;
                                        break;
                                    case "fiberid":
                                        fiber = true;
                                        fiberI = c;
                                        break;
                                    default:
                                        break;
                                }
                            }
                            sb.AppendFormat("</tr>");
                            if (run == true && camcol == true && field == true)
                            {
                                dasFields = true;
                                runs = new string[NumRows];
                                reruns = new string[NumRows];
                                camcols = new string[NumRows];
                                fields = new string[NumRows];
                            }
                            if (plate == true && mjd == true && fiber == true)
                            {
                                dasSpectra = true;
                                plates = new string[NumRows];
                                mjds = new string[NumRows];
                                spreruns = new string[NumRows];
                                fibers = new string[NumRows];
                            }
                        }

                        sb.AppendFormat("<tr align=center BGCOLOR=#eeeeff>");
                        for (int c = 0; c < NumColumns; c++)
                            sb.AppendFormat("<td nowrap><font size=-1>{0}</font></td>", ds.Tables[t].Rows[r][c].ToString());
                        sb.AppendFormat("</tr>");

                        if (dasFields == true)
                        {
                            runs[r] = ds.Tables[t].Rows[r][runI].ToString();
                            if (rerunI > -1) reruns[r] = ds.Tables[t].Rows[r][rerunI].ToString();
                            camcols[r] = ds.Tables[t].Rows[r][camcolI].ToString();
                            fields[r] = ds.Tables[t].Rows[r][fieldI].ToString();
                        }
                        if (dasSpectra == true)
                        {
                            plates[r] = ds.Tables[t].Rows[r][plateI].ToString();
                            if (sprerun == true)
                                spreruns[r] = ds.Tables[t].Columns[sprerunI].ColumnName;
                            else
                                spreruns[r] = "" + DefaultSpRerun;
                            mjds[r] = ds.Tables[t].Rows[r][mjdI].ToString();
                            fibers[r] = ds.Tables[t].Rows[r][fiberI].ToString();
                        }

                    }
                    if (KeyWords.dasUrlBase.Length > 1 && (dasFields == true || dasSpectra == true))
                    {
                        sb.AppendFormat("<p><table><tr>\n");
                        var str = "";
                        if (dasFields == true && dasSpectra == true)
                            str = "(s)";
                        sb.AppendFormat("<tr><td colspan=2><h3>Use the button" + str + " below to upload the results of the above query to the SAS and retrieve the corresponding FITS files:</h3></td></tr>");
                        if (dasFields == true)
                        {
                            sb.AppendFormat("<td><form method='post' action='" + KeyWords.dasUrlBase + "bulkFields/runCamcolFields'/>\n");
                            //				Response.Write( "<input type='hidden' name='search' value ='runcamcolfield'/>\n" );
                            sb.AppendFormat("<input type='hidden' name='runcamcolfields' value='");
                            for (int i = 0; i < NumRows; i++)
                                sb.AppendFormat(runs[i] + "," + camcols[i] + "," + fields[i] + "\n");
                            sb.AppendFormat("'/>\n");
                            sb.AppendFormat("<input type='submit' name='submit' value='Submit'/>Upload list of fields to SAS\n");
                            sb.AppendFormat("</form></td>");
                        }
                        if (dasSpectra == true)
                        {
                            sb.AppendFormat("<td><form method='post' action='" + KeyWords.dasUrlBase + "bulkSpectra/plateMJDFiber'/>\n");
                            sb.AppendFormat("<input type='hidden' name='platemjdfibers' value='");
                            for (int i = 0; i < NumRows; i++)
                                sb.AppendFormat(plates[i] + "," + mjds[i] + "," + fibers[i] + "\n");
                            sb.AppendFormat("'/>\n");
                            sb.AppendFormat("<input type='submit' name='submitPMF' value='Submit'/>Upload list of spectra to SAS\n");
                            sb.AppendFormat("</form></td>");
                        }
                        sb.AppendFormat("</tr></table>");
                    }



                    sb.AppendFormat("</TABLE>");
                }
                sb.AppendFormat("<hr>");
            }
            sb.AppendFormat("</BODY></HTML>\n");
            return sb.ToString();


        }
        
        
        private String getJsonContent(String query, String casjobsTaskName)
        {
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                writer.Formatting = Formatting.Indented;
                writer.WriteStartObject();
                writer.WritePropertyName("Query");
                writer.WriteValue(query);
                writer.WritePropertyName("TaskName");
                writer.WriteValue(casjobsTaskName);
                writer.WriteEndObject();
            }
            return sb.ToString();
        }


        /// <summary>
        ///  This is specifically used for the sdss vo services which are returning random types of xml outputs
        /// </summary>
        /// <returns></returns>
        public string ToXML(Object o)
        {
            var stringwriter = new System.IO.StringWriter();
            var serializer = new XmlSerializer(o.GetType());
            serializer.Serialize(stringwriter, o);
            return stringwriter.ToString();
        }

        // this is just to Dataset from running query in casjobs
        public DataSet runQuery()
        {
            // throw new IndexOutOfRangeException("There is an invalid argument");

            try
            {
                var request = (HttpWebRequest)WebRequest.Create(KeyWords.casjobsREST + "contexts/" + casjobsTarget + "/query");
                request.Method = "POST";
                request.ContentType = "application/json";
                request.Accept = this.returnType;//"application/x-dataset";
                request.Timeout = KeyWords.TimeoutCASJobs;//timeout in milliseconds

                if (!token.Equals("") && token != null)
                    request.Headers.Add("X-Auth-Token", token);

                StreamWriter streamWriter = new StreamWriter(request.GetRequestStream());
                StringWriter sw = new StringWriter();
                JsonWriter jsonWriter = new JsonTextWriter(sw);
                jsonWriter.WriteStartObject();
                jsonWriter.WritePropertyName("Query");
                jsonWriter.WriteValue(this.query);
                //jsonWriter.WritePropertyName("ReturnDataSet");
                //jsonWriter.WriteValue(true);
                jsonWriter.WriteEndObject();
                jsonWriter.Close();
                streamWriter.Write(sw.ToString());
                streamWriter.Close();

                DataSet ds = null;
                //var getresponse = request.GetResponse();
                //HttpWebResponse response = (HttpWebResponse)getresponse;
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    BinaryFormatter fmt = new BinaryFormatter();
                    ds = (DataSet)fmt.Deserialize(response.GetResponseStream());
                }
                return ds;
            }
            catch (System.Exception e)
            {
                throw new Exception("There is an error while running Query:\n Query:" + this.query + " " + e.Message + " ");
            }
            /*catch (System.Net.WebException e)
            {
                //throw new Exception("There is an error while running Query:\n Query:" + this.query + " ");
                string text = "";
                WebResponse errResp = e.Response;
                using (Stream respStream = errResp.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(respStream);
                    text = reader.ReadToEnd();
                }
                throw new Exception("There is an error while running Query:\n Query:" + this.query + " " + text + " ");
            }*/
        }
              
     

       
        ////// Using New CAsjobs WebService
        
        //public HttpResponseMessage postCasjobs(string query, String token, string casjobsTaskName)
        //{
        //    HttpClient client = new HttpClient();
        //    client.BaseAddress = new Uri(KeyWords.casjobsREST);

        //    StringContent content = new StringContent("{\"Query\":\"" + query + "\" , \"TaskName\":\""+casjobsTaskName+"\"}");
        //    if(!(token == null || token == String.Empty))
        //    content.Headers.Add(KeyWords.xauth, token);
        //    content.Headers.ContentType = new MediaTypeHeaderValue(KeyWords.contentJson);

        //    HttpResponseMessage response = client.PostAsync(KeyWords.casjobsContextPath, content).Result;
            
        //    response.EnsureSuccessStatusCode();
            
        //    if(response.IsSuccessStatusCode)
        //        return response;
        //    else
        //        throw new ApplicationException("Query did not return results successfully, check input and try again later.");                
        //}

        //public HttpResponseMessage postCasjobs(string query, String token, string casjobsTaskName, string format)
        //{
        //    HttpClient client = new HttpClient();
        //    client.BaseAddress = new Uri(KeyWords.casjobsREST);

        //    //StringContent content = new StringContent("{\"Query\":\"" + query + "\" , \"TaskName\":\"" + casjobsTaskName + "\"}");
        //    StringContent content = new StringContent(this.getJsonContent(query,casjobsTaskName));
        //    if (!(token == null || token == String.Empty))
        //        content.Headers.Add(KeyWords.xauth, token);
        //    content.Headers.ContentType = new MediaTypeHeaderValue(KeyWords.contentJson);

        //    HttpResponseMessage response = client.PostAsync(KeyWords.casjobsContextPath, content).Result;

        //    response.EnsureSuccessStatusCode();

        //    if (response.IsSuccessStatusCode)
        //        return response;
        //    else
        //        throw new ApplicationException("Query did not return results successfully, check input and try again later.");
        //}

        
        
        ////public async Task<HttpResponseMessage> postCasjobs(string query, String token, string casjobsTaskName, HttpResponseMessage response)
        ////{
        ////    //string casjobsTaskname = "test";
        ////    HttpClient client = new HttpClient();
        ////    client.BaseAddress = new Uri(KeyWords.casjobsREST);

        ////    StringContent content = new StringContent("{\"Query\":\"" + query + "\" , \"TaskName\":\"" + casjobsTaskName + "\"}");
        ////    if (!(token == null || token == String.Empty))
        ////        content.Headers.Add(KeyWords.xauth, token);
        ////    content.Headers.ContentType = new MediaTypeHeaderValue(KeyWords.contentJson);

        ////    System.IO.Stream stream = await client.PostAsync(KeyWords.casjobsContextPath, content).Result.Content.ReadAsStreamAsync();
        ////    //response.EnsureSuccessStatusCode();
        ////    //if (response.IsSuccessStatusCode)
        ////    //    return response;
        ////    //else
        ////    //    throw new ApplicationException("Query did not return results successfully, check input and try again later.");
        ////    return response;
        ////}

        ///// <summary>
        /////  Upload data to run queries using table join
        ///// </summary>
        ///// <param name="datastring"></param>
        ///// <param name="token"></param>
        ///// <returns></returns>
        //public HttpResponseMessage uploadCasjobs(string datastring, string token)
        //{
        //    HttpClient client = new HttpClient();
        //    client.BaseAddress = new Uri(KeyWords.casjobsREST);

        //    StringContent content = new StringContent("\"" + datastring + "\"");

        //    content.Headers.Add(KeyWords.xauth, token);
        //    content.Headers.ContentType = new MediaTypeHeaderValue(KeyWords.contentJson);

        //    HttpResponseMessage response = client.PostAsync(KeyWords.casjobsContextPath, content).Result;
        //    response.EnsureSuccessStatusCode();
        //    if (response.IsSuccessStatusCode)
        //        return response;
        //    else
        //        throw new ApplicationException("Query did not return results successfully, check input and try again later.");

        //}

/*
        public HttpResponseMessage GetTagStream(HttpRequestMessage request)
        {

            request.Headers.AcceptEncoding.Clear();
            HttpResponseMessage response = request.CreateResponse();
            response.Content = new PushStreamContent(OnStreamAvailable, "text/plain");
            return response;
        }

        private void OnStreamAvailable(Stream stream, HttpContent content, TransportContext context)
        {
            ConcurrentDictionary<StreamWriter, StreamWriter> outputs = new ConcurrentDictionary<StreamWriter, StreamWriter>();
            StreamWriter sWriter = new StreamWriter(stream);
            outputs.TryAdd(sWriter, sWriter);

            while (true)
            {
                try
                {
                    if (File.Exists("c:\\TagStream.xml"))
                    {
                        using (FileStream fileStream = File.Open("c:\\TagStream.xml", FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        {
                            try
                            {
                                using (var file = new StreamReader(fileStream))
                                {
                                    try
                                    {
                                        while (true)
                                        {
                                            string pushValue;

                                            if (!file.EndOfStream)
                                                pushValue = file.ReadToEnd();
                                            else
                                                pushValue = QUERY_ALIVE;

                                            foreach (var kvp in outputs.ToArray())
                                            {
                                                try
                                                {
                                                    kvp.Value.Write(pushValue);
                                                    kvp.Value.Flush();
                                                }
                                                catch (Exception e)
                                                {
                                                    StreamWriter sWriterOut;
                                                    outputs.TryRemove(kvp.Value, out sWriterOut);
                                                    if (e is HttpException)
                                                        if (((HttpException)e).ErrorCode == -2147023667) //The remote host closed the connection.
                                                            throw;
                                                }
                                            }
                                            Thread.Sleep(10000);
                                        }
                                    }
                                    finally
                                    {
                                        file.Close();
                                        file.Dispose();
                                    }
                                }
                            }
                            finally
                            {
                                fileStream.Close();
                                fileStream.Dispose();
                            }
                        }
                    }
                    else
                    {
                        foreach (var kvp in outputs.ToArray())
                        {
                            try
                            {
                                kvp.Value.Write(QUERY_ALIVE);
                                kvp.Value.Flush();
                            }
                            catch (Exception e)
                            {
                                StreamWriter sWriterOut2;
                                outputs.TryRemove(kvp.Value, out sWriterOut2);
                                if (e is HttpException)
                                    if (((HttpException)e).ErrorCode == -2147023667) //The remote host closed the connection.
                                        throw;
                            }
                        }
                        Thread.Sleep(10000);
                    }
                }
                catch (HttpException ex)
                {
                    if (ex.ErrorCode == -2147023667) // The remote host closed the connection.
                    {
                        return;
                    }
                }
            }
        }

        */
    }
}