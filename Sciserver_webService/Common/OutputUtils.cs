using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using System.Xml.Serialization;
using net.ivoa.VOTable;
using net.ivoa.data;
using Jhu.SharpFitsIO;
using System.Text.RegularExpressions;


namespace Sciserver_webService.Common
{
    public class OutputUtils
    {
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


        public static void writeCSV(DataSet dataSet, Stream stream)
        {

            using (StreamWriter writer = new StreamWriter(stream))
            {
                int ntables = 0;
                foreach (DataTable table in dataSet.Tables)
                {
                    ntables++;
                    writer.Write("#" + table.TableName + "\n");
                    for (int Index = 0; Index < (table.Columns.Count); Index++)
                    {
                        writer.Write(table.Columns[Index].ColumnName);
                        writer.Write((Index != table.Columns.Count - 1) ? "," : "\n");
                    }
                    foreach (DataRow row in table.Rows)
                    {
                        for (int Index = 0; Index < (table.Columns.Count); Index++)
                        {
                            Type type = table.Columns[Index].DataType;

                            string str = "";
                            object value = row.ItemArray[Index];
                            if (value is byte[])
                            {
                                str = Utilities.BytesToHex((byte[])value);
                            }
                            else
                            {
                                str = value.ToString();
                            }

                            if ((type == typeof(string)) && (str.Contains(",")) && (str[0] != '"'))
                            {
                                Regex.Replace(str, "\"([^\"]*)\"", "``$1''");
                                writer.Write("\"" + str + "\"");
                            }
                            else
                            {
                                writer.Write(str);
                            }
                            writer.Write((Index != table.Columns.Count - 1) ? "," : "\n");
                        }
                    }
                    if (ntables < dataSet.Tables.Count)
                        writer.Write("\n");
                }
            }
        }


        public static void WriteHTML(DataSet ds, Stream stream)
        {

            using (StreamWriter writer = new StreamWriter(stream))
            {


                string ColumnName = "";
                //StringBuilder sb = new StringBuilder();
                writer.Write("<html><head>\n");
                writer.Write("<title>SDSS Query Results</title>\n");
                writer.Write("</head><body bgcolor=white>\n");
                int NumTables = ds.Tables.Count;

                for (int t = 0; t < NumTables; t++)
                {

                    int NumRows = ds.Tables[t].Rows.Count;
                    if (NumRows == 0)
                    {
                        writer.Write("<h3><br><font color=red>No entries have been found</font> </h3>");
                    }
                    else
                    {
                        for (int r = 0; r < NumRows; r++)
                        {
                            int NumColumns = ds.Tables[t].Columns.Count;
                            if (r == 0)// filling the first row with the names of the columns
                            {
                                writer.Write("<table border='1' BGCOLOR=cornsilk>\n");
                                writer.Write("<tr align=center>");
                                for (int c = 0; c < NumColumns; c++)
                                {
                                    ColumnName = ds.Tables[t].Columns[c].ColumnName;
                                    writer.Write("<td><font size=-1>{0}</font></td>", ColumnName);
                                }
                                writer.Write("</tr>");
                            }

                            writer.Write("<tr align=center BGCOLOR=#eeeeff>");
                            for (int c = 0; c < NumColumns; c++)
                                writer.Write("<td nowrap><font size=-1>{0}</font></td>", ds.Tables[t].Rows[r][c].ToString());
                            writer.Write("</tr>");
                        }
                        writer.Write("</TABLE>");

                    }
                    writer.Write("<hr>");
                }
                writer.Write("</BODY></HTML>\n");
            }
        }




        public static string WriteJson(DataSet dataSet)
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
            return sb.ToString();
        }


        public static void WriteJson(DataSet dataSet, Stream stream)
        {
            using (StreamWriter writer = new StreamWriter(stream))
            {
                writer.Write(WriteJson(dataSet));
            }
        }

        public static void WriteVOTable(DataSet dataSet, Stream stream)
        {
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);

            DataWrapper dw = WrapperFactory.GetDataWrapper(dataSet);
            VOTABLE voTable = VOTableWrapper.Wrapper2VOTable(dw);
            XmlSerializer serializer = new XmlSerializer(typeof(VOTABLE));
            serializer.Serialize(sw, voTable);

            using (StreamWriter writer = new StreamWriter(stream))
            {
                writer.Write(sb.ToString());
            }
        }

        public static void WriteFits(DataSet dataSet, Stream stream)
        {

            FitsFile fits = new FitsFile(stream, FitsFileMode.Write);
            fits.IsBufferingAllowed = true;
            // Primary header
            var prim = SimpleHdu.Create(fits, true, true, true);
            prim.WriteHeader();
            // Table
            BinaryTableHdu tab = BinaryTableHdu.Create(fits, true);
            using (DataTableReader reader = dataSet.CreateDataReader())
            {
                tab.WriteFromDataReader(reader);
            }
            fits.Close();
            stream.Close();
        }
    }
}