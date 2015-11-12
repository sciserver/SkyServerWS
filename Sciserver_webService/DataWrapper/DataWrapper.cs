using System;
using System.Data;
using System.IO;
using net.ivoa.VOTable;

namespace net.ivoa.data
{
    public interface DataWrapper
    {
        int ActiveTable { get; set; }
        int TableCount { get; }
        int RowCount { get; }
        int ColCount { get; }
        int RowCap { set; }//set max rows to render
        string ColName(int c);
        void ColName(int c, string a);
        object ColType(int c);
        void ColType(int c, object t);
        Type MSColType(int c);
        void MSColType(int c, Type t);
        int NewRow();//return index of newrow
        int AddCol(string name, Type mstype);//return index of col added
        object this[int r, int c]
        {
            get;
            set;
        }
    }

    public abstract class WrapperFactory
    {
        public static DataWrapper GetDataWrapper(object o)
        {
            if (o is DataSet)
            {
                return new DataSetWrapper((DataSet)o);
            }
            else if (o is VOTABLE)
            {
                return new VOTableWrapper((VOTABLE)o);
            }
            else if (o is DataSetWrapper)
            {
                return new DataSetWrapper(((DataSetWrapper)o).ds.Clone());
            }
            else if (o is VOTableWrapper)
            {
                return new VOTableWrapper(((VOTableWrapper)o).vot);
            }
            else
                throw new Exception("Unknown VOData Type: " + o.GetType().Name);
        }

        public const string HTML_NEWLINE = "<br>";
        public const string UNIX_NEWLINE = "\n";
        public const string DOS_NEWLINE = "\n\r";
        public static string Wrapper2CSV(DataWrapper dw, string newline)
        {
            StringWriter writer = new StringWriter();
            for (int t = 0; t < dw.TableCount; t++)
            {
                dw.ActiveTable = t;
                for (int c = 0; c < dw.ColCount; c++)
                {
                    //titles
                    writer.Write(dw.ColName(c));
                    if (c < dw.ColCount - 1) writer.Write(",");
                }
                writer.Write(newline);
                for (int r = 0; r < dw.RowCount; r++)
                {
                    for (int c = 0; c < dw.ColCount; c++)
                    {
                        writer.Write(object2string(dw[r, c], NOMAXCHARS));
                        if (c < dw.ColCount - 1) writer.Write(",");
                    }
                    if (r < (dw.RowCount - 1)) writer.Write(newline);
                }
                writer.Write(newline);
            }
            writer.Close();
            return writer.ToString();
        }
        public static string Wrapper2HTML(DataWrapper dw, int maxchars)
        {
            return WrapperFactory.Wrapper2HTML(dw, null, null, null, null, null, "1", "1", maxchars);
        }
        public static string Wrapper2HTML(DataWrapper dw, string alttdclass, int maxchars)
        {
            return WrapperFactory.Wrapper2HTML(dw, null, null, null, null, alttdclass, "1", "0", maxchars);
        }
        public static string Wrapper2HTML(DataWrapper dw, string tableclass, string thclass, string trclass,
            string tdclass, string alttdclass, string cellpadding, string cellspacing, int maxchars)
        {
            tableclass = string2class(tableclass);
            thclass = string2class(thclass);
            tdclass = string2class(tdclass);
            trclass = string2class(trclass);
            alttdclass = string2class(alttdclass);
            cellpadding = cellpadding == null ? "" : " cellpadding=\"" + cellpadding + "\" ";
            cellspacing = cellspacing == null ? "" : " cellspacing=\"" + cellspacing + "\" ";
            StringWriter writer = new StringWriter();
            for (int t = 0; t < dw.TableCount; t++)
            {
                dw.ActiveTable = t;
                writer.WriteLine("<table " + tableclass + cellpadding + cellspacing + ">");
                writer.Write("<tr " + trclass + ">");
                for (int c = 0; c < dw.ColCount; c++)//titles
                    writer.Write("<th " + thclass + ">" + dw.ColName(c) + "</th>");
                writer.Write("</tr>");
                for (int r = 0; r < dw.RowCount; r++)
                {
                    writer.WriteLine("<tr " + trclass + ">");
                    string thistdclass = (r % 2) == 0 ? tdclass : alttdclass;
                    for (int c = 0; c < dw.ColCount; c++)
                        writer.Write("<td " + thistdclass + ">" + object2string(dw[r, c], maxchars) + "</td>");
                    writer.Write("</tr>");
                }
                writer.Write("</table>");
            }
            writer.Close();
            return writer.ToString();
        }
        public const int NOMAXCHARS = -1;
        private static string object2string(object o, int maxchars)
        {
            if (o == null) o = "";
            if (o is Byte[])
            {
                Byte[] x = (Byte[])o;
                if (maxchars != NOMAXCHARS && x.Length > maxchars)
                {
                    Byte[] xx = new Byte[maxchars];
                    Array.Copy(x, xx, maxchars);
                    x = xx;
                }
                o = HexEncoding.ToString(x);
            }
            string outVal = o.ToString();
            if (outVal.Length == 0) outVal = "null";
            if (maxchars != NOMAXCHARS && outVal.Length > maxchars) outVal = outVal.Substring(0, maxchars) + "...";
            return outVal;
        }
        private static string string2class(string s)
        {
            return s == null ? "" : "class=\"" + s + "\" ";
        }
    }
}