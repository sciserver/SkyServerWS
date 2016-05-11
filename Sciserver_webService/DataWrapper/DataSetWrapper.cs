using System;
using System.Data;
using System.IO;

namespace net.ivoa.data
{

    public class DataSetWrapper : DataWrapper
    {
        public DataSet ds;
        public DataSetWrapper(DataSet ds_)
        {
            this.ds = ds_;
            ActiveTable = 0;
        }
        private int table;
        public int ActiveTable
        {
            get { return table; }
            set { table = value; }
        }
        public int TableCount { get { return ds.Tables.Count; } }
        public int RowCount
        {
            get
            {
                return ds.Tables[ActiveTable].Rows.Count;
            }
        }
        public int RowCap
        {
            set
            {
                DataTable dt = ds.Tables[ActiveTable];
                while (dt.Rows.Count > value)
                    dt.Rows.RemoveAt(dt.Rows.Count - 1);
            }
        }
        public int ColCount { get { return ds.Tables[ActiveTable].Columns.Count; } }
        public string ColName(int c) { return ds.Tables[ActiveTable].Columns[c].ColumnName; }
        public void ColName(int c, string s) { ds.Tables[ActiveTable].Columns[c].ColumnName = s; }
        public object ColType(int c) { return ds.Tables[ActiveTable].Columns[c].DataType; }
        public void ColType(int c, object t) { ds.Tables[ActiveTable].Columns[c].DataType = (Type)t; }
        public Type MSColType(int c) { return (Type)ColType(c); }
        public void MSColType(int c, Type t) { ColType(c, t); }
        public int NewRow()
        {
            DataRow newRow = ds.Tables[ActiveTable].NewRow();
            ds.Tables[ActiveTable].Rows.Add(newRow);//adds to end i hope...
            return ds.Tables[ActiveTable].Rows.Count - 1;
        }
        public int AddCol(string name, Type mstype)
        {
            ds.Tables[ActiveTable].Columns.Add(name, mstype);
            return ds.Tables[ActiveTable].Columns.Count - 1;
        }
        public object this[int r, int c]
        {
            get { return ds.Tables[ActiveTable].Rows[r][c]; }
            set { ds.Tables[ActiveTable].Rows[r][c] = value; }
        }

        public static DataSet Wrapper2DataSet(DataWrapper dw)
        {
            if (dw is DataSetWrapper) return ((DataSetWrapper)dw).ds;
            DataSet rst = new DataSet();
            for (int t = 0; t < dw.TableCount; t++)
            {
                dw.ActiveTable = t;
                DataTable dt = new DataTable();
                for (int c = 0; c < dw.ColCount; c++)
                {
                    DataColumn dc = new DataColumn();
                    dc.ColumnName = dw.ColName(c);
                    dc.DataType = dw.MSColType(c);
                    dt.Columns.Add(dc);
                }
                for (int r = 0; r < dw.RowCount; r++)
                {//rows
                    DataRow row = dt.NewRow();
                    for (int c = 0; c < dw.ColCount; c++)
                        row[c] = dw[r, c];
                    dt.Rows.Add(row);
                }
                rst.Tables.Add(dt);
            }
            return rst;
        }
    }
}