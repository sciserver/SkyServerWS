﻿using System;
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

namespace Sciserver_webService.Common
{
    public class ProcessDataSet
    {

        String query = "";
        String TaskName = "";
        String format = "";
        bool IsSuccessStatusCode;
        Dictionary<string, string> ExtraInfo = null;
        string ErrorMessage = "";
        net.ivoa.VOTable.VOTABLE vot;


        public ProcessDataSet(string query, string format, string TaskName, Dictionary<string, string> ExtraInfo, string ErrorMessage, bool IsSuccessStatusCode)
        {
            this.query = query;
            this.format = format;
            this.TaskName = TaskName;
            this.ExtraInfo = ExtraInfo;
            this.ErrorMessage = ErrorMessage;
            this.IsSuccessStatusCode = IsSuccessStatusCode;
        }


        
        
        
        public HttpContent GetContent(DataSet ds)
        {

            var response = new HttpResponseMessage();

            if (this.IsSuccessStatusCode)
            {
                if (this.ExtraInfo["FormatFromUser"] != "html")
                {
                    NewSDSSFields sf = new NewSDSSFields();
                    switch (TaskName)
                    {

                        case "FOR CONE SEARCH":
                            DefaultCone cstest = new DefaultCone();
                            vot = cstest.ConeSearch(ds);
                            response.Content = new StringContent(ToXML(vot), Encoding.UTF8, KeyWords.contentXML);
                            break;

                        case "SDSSFields:FieldArray":

                            response.Content = new StringContent(ToXML(sf.FieldArray(ds)), Encoding.UTF8, KeyWords.contentXML);
                            break;

                        case "SDSSFields:FieldArrayRect":

                            response.Content = new StringContent(ToXML(sf.FieldArrayRect(ds)), Encoding.UTF8, KeyWords.contentXML);
                            break;

                        case "SDSSFields:ListOfFields":

                            response.Content = new StringContent(ToXML(sf.ListOfFields(ds)), Encoding.UTF8, KeyWords.contentXML);
                            break;

                        case "SDSSFields:UrlsOfFields":

                            response.Content = new StringContent(ToXML(sf.UrlOfFields(ds)), Encoding.UTF8, KeyWords.contentXML);
                            break;

                        default:
                            Action<Stream, HttpContent, TransportContext> WriteToStream = null;
                            BinaryFormatter fmt;
                            ds.RemotingFormat = SerializationFormat.Binary;
                            fmt = new BinaryFormatter();
                            WriteToStream = (stream, foo, bar) => { fmt.Serialize(stream, ds); stream.Close(); };
                            response.Content = new PushStreamContent(WriteToStream, new MediaTypeHeaderValue((KeyWords.contentDataset)));
                            break;
                    }

                }
                else
                {
                    response.Content = new StringContent(getHtmlContent(ds));
                }
            }
            else // not IsSuccessStatusCode
            {
                if (ExtraInfo["FormatFromUser"] == "html")
                {
                    response.Content = new StringContent(getHtmlError(ErrorMessage));
                }
                else
                {
                    //response.Content = new StreamContent(stream);
                    response.Content = new StringContent(ErrorMessage);
                }
            }
            return response.Content;
        }


        private String getHtmlError(string ErrorMessage)
        {
            if (ExtraInfo["syntax"] == "Syntax") // in case user want only to verify the syntax of the sql command:
            {
                return getINCORRECTsyntaxHTMLresult(ErrorMessage);
            }
            else // in case we want to run que sql command and retrrieve the resultset:
            {
                return getGenericHTMLerror(ErrorMessage);
            }
        }


        private String getHtmlContent(DataSet ds)
        {
            this.query = ExtraInfo["QueryForUserDisplay"] == "" ? this.query : ExtraInfo["QueryForUserDisplay"];
            try
            {
                if (ExtraInfo["syntax"] == "Syntax") // in case user want only to verify the syntax of the sql command:
                {
                    return getOKsyntaxHTMLresult(ds);
                }
                else if (ExtraInfo["fp"] == "only") // in case user want to run the "is in footprint?" query
                {
                    return getFootprintTableHTMLresult(ds);
                }
                else // in case we want to run que sql command and retrrieve the resultset:
                {
                    return getTableHTMLresult(ds);
                }
            }
            catch (Exception e)
            {
                throw;
                //return e.Message;
            }
        }

        private string getFootprintTableHTMLresult(DataSet ds)
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



        private string getOKsyntaxHTMLresult(DataSet ds)
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

        private string getINCORRECTsyntaxHTMLresult(string ErrorMessage)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("<html><head>\n");
            sb.AppendFormat("<title>SDSS Query Syntax Check</title>\n");
            sb.AppendFormat("</head><body bgcolor=white>\n");
            sb.AppendFormat("<h2>SQL Syntax Check</h2>");
            sb.AppendFormat("<H3 BGCOLOR=pink><font color=red>SQL returned the following error: <br>     " + ErrorMessage + "</font></H3>");
            sb.AppendFormat("<h3>Your SQL command was: <br><pre>" + ExtraInfo["QueryForUserDisplay"] + "</pre></h3><hr>"); // writes command
            sb.AppendFormat("</BODY></HTML>\n");
            return sb.ToString();
        }


        private string getGenericHTMLerror(string ErrorMessage)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("<html><head>\n");
            sb.AppendFormat("<title>SDSS error message</title>\n");
            sb.AppendFormat("</head><body bgcolor=white>\n");
            sb.AppendFormat("<h2>SDSS error message</h2>");
            sb.AppendFormat("<H3 BGCOLOR=pink><font color=red>SQL returned the following error: <br>     " + ErrorMessage + "</font></H3>");
            //sb.AppendFormat("<H3 BGCOLOR=pink><font color=red> Some tips: <br> No multiple SQL commands allowed     </font></H3>");
            sb.AppendFormat("<h3>Your SQL command was: <br><pre>" + ExtraInfo["QueryForUserDisplay"] + "</pre></h3><hr>"); // writes command
            sb.AppendFormat("</BODY></HTML>\n");
            return sb.ToString();
        }


        private string getTableHTMLresult(DataSet ds)
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
            string[] QueryTitle = new string[NumTables];
            if ((this.TaskName.Contains(KeyWords.RadialQuery) || this.TaskName.Contains(KeyWords.RectangularQuery)) && NumTables == 2)
            {
                QueryTitle[0] = "Imaging";
                QueryTitle[1] = "Infrared Spectra";
            }

            for (int t = 0; t < Math.Min(NumTables, Queries.Length); t++)
            {
                string[] runs = null, reruns = null, camcols = null, fields = null, plates = null, mjds = null, spreruns = null, fibers = null;
                bool run = false, rerun = false, camcol = false, field = false, dasFields = false;
                bool plate = false, mjd = false, sprerun = false, fiber = false, dasSpectra = false;
                int runI = -1, rerunI = -1, camcolI = -1, fieldI = -1;
                int plateI = -1, mjdI = -1, sprerunI = -1, fiberI = -1;
                int DefaultSpRerun = int.Parse(KeyWords.defaultSpRerun);

                sb.AppendFormat("<h1>" + QueryTitle[t] + "</h1>");
                sb.AppendFormat("<h3>Your SQL command was: <br><pre>" + Queries[t] + "</pre></h3>"); // writes command
                int NumRows = ds.Tables[t].Rows.Count;
                if (NumRows == 0)
                {
                    sb.AppendFormat("<h3><br><font color=red>No entries have been found</font> </h3>");
                }
                else
                {
                    if (ds.Tables[0].Rows[0][0].ToString().StartsWith("error: limit is") || ds.Tables[0].Rows[0][0].ToString().Contains("Maximum number of rows allowed"))// 
                    {
                        sb.AppendFormat("<h3><br><font color=red> Error: Maximum number of rows is " + (Int64.Parse(KeyWords.MaxRows)).ToString("c0").Remove(0, 1) + ". Try using 'TOP " + KeyWords.MaxRows + "' in the SQL command.</font> </h3>");
                    }
                    else
                    {

                        sb.AppendFormat("<h3>Your query output (max " + (Int64.Parse(KeyWords.MaxRows)).ToString("c0").Remove(0, 1) + " rows): <br></h3>"); // writes command
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
                }
                sb.AppendFormat("<hr>");
            }
            sb.AppendFormat("</BODY></HTML>\n");
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












    }
}