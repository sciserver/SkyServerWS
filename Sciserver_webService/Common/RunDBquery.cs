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

//This class is used to submit query to casjobs
namespace Sciserver_webService.DoDatabaseQuery
{

    public class RunDBquery : IHttpActionResult
    {
        String query = "";
        String Format = "";
       
        public RunDBquery(string query,  string Format)
        {
            this.query = query;
            this.Format = Format;
        }

        public async Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {

            SqlConnection Conn = new SqlConnection(KeyWords.DBconnectionString);
            await Conn.OpenAsync();
            SqlCommand Cmd = Conn.CreateCommand();
            Cmd.CommandText = this.query;
            //SqlDataReader reader = await Cmd.ExecuteReaderAsync();
            DataSet ds = new DataSet();
            var Adapter = new SqlDataAdapter(Cmd);
            Adapter.Fill(ds);
            Conn.Close();
            BinaryFormatter fmt = new BinaryFormatter();
            Action<Stream, HttpContent, TransportContext> WriteToStream = null;
            var response = new HttpResponseMessage();
            switch(Format.ToLower())
            {
                case "xml":
                    ds.RemotingFormat = SerializationFormat.Xml;
                    WriteToStream = (stream, foo, bar) => { WriteXml(ds, stream); };
                    response.Content = new PushStreamContent(WriteToStream, new MediaTypeHeaderValue((KeyWords.contentXML)));
                    break;
                case "json":
                    ds.RemotingFormat = SerializationFormat.Xml;
                    WriteToStream = (stream, foo, bar) => { WriteJson(ds, stream); stream.Close(); };
                    response.Content = new PushStreamContent(WriteToStream, new MediaTypeHeaderValue((KeyWords.contentJson)));
                    break;
                default:
                    ds.RemotingFormat = SerializationFormat.Binary;
                    WriteToStream = (stream, foo, bar) => { fmt.Serialize(stream, ds); stream.Close(); };
                    response.Content = new PushStreamContent(WriteToStream, new MediaTypeHeaderValue(("application/x-dataset")));
                    break;
            }
            //reader.Close();
            //response.Content = new StringContent(ClientIP);
            return response;
            //return processDBqueryResults(stream);


        }


        public static string BytesToHex(byte[] bytes)
        {
            if (bytes == null) return null;
            else
                return "0x" + BitConverter.ToString(bytes).Replace("-", string.Empty).ToLower();
        }

        public static void WriteXml(DataSet dataSet, Stream stream)
        {
            using (StreamWriter writer = new StreamWriter(stream))
            {

                writer.Write("<?xml version=\"1.0\" encoding=\"UTF-8\" ?>\n");
                writer.Write("<!DOCTYPE root [\n");
                writer.Write("<!ELEMENT root (Table*)>\n");
                writer.Write("<!ELEMENT Table (Row*)>\n");
                writer.Write("<!ELEMENT Row (Item*)>\n");
                writer.Write("<!ELEMENT Item (#PCDATA)>\n");
                writer.Write("<!ATTLIST Table name CDATA #REQUIRED>\n");
                writer.Write("<!ATTLIST Item name CDATA #REQUIRED>\n");

                writer.Write("]>\n");

                writer.Write("<root>\n");

                foreach (DataTable table in dataSet.Tables)
                {
                    writer.Write("<Table name=\"" + table.TableName + "\">\n");
                    foreach (DataRow row in table.Rows)
                    {
                        writer.Write("<Row>\n");
                        for (int i = 0; i < table.Columns.Count; i++)
                        {
                            writer.Write("<Item name=\"" + table.Columns[i].ColumnName + "\">");
                            string str = "";
                            object value = row.ItemArray[i];
                            if (value is byte[])
                            {
                                str = BytesToHex((byte[])value);
                            }
                            else
                            {
                                str = value.ToString();
                            }
                            str = str.Replace("<", "&lt;");
                            str = str.Replace(">", "&gt;");

                            writer.Write(str);
                            writer.Write("</Item>\n");
                        }
                        writer.Write("</Row>\n");
                    }
                    writer.Write("</Table>\n");
                }
                writer.Write("</root>\n");
            }
        }

        public static void WriteJson(DataSet dataSet, Stream stream)
        {
            StringBuilder sb = new StringBuilder();

            using (JsonWriter json = new JsonTextWriter(new StringWriter(sb)))
            {
                json.Formatting = Formatting.Indented;
                json.WriteStartArray();

                foreach (DataTable table in dataSet.Tables)
                {
                    json.WriteStartObject();
                    json.WritePropertyName("TableName");
                    json.WriteValue(table.TableName);
                    json.WritePropertyName("Rows");
                    json.WriteStartArray();
                    foreach (DataRow row in table.Rows)
                    {
                        json.WriteStartObject();
                        for (int Index = 0; Index < (table.Columns.Count); Index++)
                        {
                            json.WritePropertyName(table.Columns[Index].ColumnName);
                            string str = "";
                            object value = row.ItemArray[Index];

                            if (value is byte[])
                            {
                                str = BytesToHex((byte[])value);
                            }
                            else
                            {
                                str = value.ToString();
                            }

                            Type type = table.Columns[Index].DataType;
                            if (type == typeof(string) || type == typeof(byte[]) || type == typeof(DateTime))
                                json.WriteValue(str);
                            else
                            {
                                if (type == typeof(bool)) str = str.ToLower();
                                if (String.IsNullOrEmpty(str)) json.WriteNull();
                                else json.WriteRawValue(str);
                            }
                        }
                        json.WriteEndObject();
                    }
                    json.WriteEndArray();
                    json.WriteEndObject();
                }

                json.WriteEndArray();
            }

            using (StreamWriter writer = new StreamWriter(stream))
            {
                writer.Write(sb.ToString());
            }
        }






    }
}