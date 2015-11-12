using System;
using System.Collections;
using net.ivoa.VOTable;

namespace net.ivoa.data
{

    public class VOTableWrapper : DataWrapper
    {
        public VOTABLE vot;
        private Hashtable dataTypesReverse, dataTypes;

        public VOTableWrapper(VOTABLE v)
        {
            vot = v;
            ActiveTable = 0;
            dataTypes = VOTableUtil.getdataTypeTable();
            dataTypesReverse = VOTableUtil.getdataTypeTableReverse();
        }
        private int table;
        public int ActiveTable
        {
            get { return table; }
            set { table = value; }
        }
        public int TableCount { get { return vot.RESOURCE[0].TABLE.Length; } }
        public int RowCount
        {
            get
            {
                try
                {
                    return ((TABLEDATA)vot.RESOURCE[0].TABLE[ActiveTable].DATA.Item).TR.Length;
                }
                catch (NullReferenceException nre) { nre = nre; return 0; }
            }
        }
        public int RowCap
        {
            set
            {
                if (RowCount > value)
                {
                    TABLEDATA data = (TABLEDATA)vot.RESOURCE[0].TABLE[ActiveTable].DATA.Item;
                    TR[] old = data.TR;
                    data.TR = new TR[value];
                    for (int x = 0; x < value; x++)
                        data.TR[x] = old[x];
                }
            }
        }
        public int ColCount { get { return vot.RESOURCE[0].TABLE[ActiveTable].Items.Length; } }
        public string ColName(int c) { return ((FIELD)vot.RESOURCE[0].TABLE[ActiveTable].Items[c]).name; }
        public void ColName(int c, string s) { ((FIELD)vot.RESOURCE[0].TABLE[ActiveTable].Items[c]).name = s; }
        public object ColType(int c) { return ((FIELD)vot.RESOURCE[0].TABLE[ActiveTable].Items[c]).datatype; }
        public void ColType(int c, object t) { ((FIELD)vot.RESOURCE[0].TABLE[ActiveTable].Items[c]).datatype = (dataType)t; }
        public Type MSColType(int c) { return (Type)dataTypesReverse[ColType(c)]; }
        public void MSColType(int c, Type t) { ColType(c, (dataType)dataTypes[t]); }
        public int AddCol(string name, Type mstype)
        {
            NewColHeader(name, mstype);
            return NewEmptyCol();
        }
        public object this[int r, int c]
        {
            get
            {
                TABLEDATA data = (TABLEDATA)vot.RESOURCE[0].TABLE[ActiveTable].DATA.Item;
                if (null != data && null != data.TR && null != data.TR[r] && null != data.TR[r].TD && null != data.TR[r].TD[c] && null != data.TR[r].TD[c].Text)
                    return data.TR[r].TD[c].Text[0];
                else
                    return "";
            }
            set
            {
                TABLEDATA data = (TABLEDATA)vot.RESOURCE[0].TABLE[ActiveTable].DATA.Item;
                data.TR[r].TD[c].Text[0] = value.ToString();
            }
        }
        private int NewColHeader(string name, Type mstype)
        {
            object[] old = vot.RESOURCE[0].TABLE[ActiveTable].Items;
            vot.RESOURCE[0].TABLE[ActiveTable].Items = new object[ColCount + 1];
            for (int x = 0; x < old.Length; x++)
                vot.RESOURCE[0].TABLE[ActiveTable].Items[x] = old[x];
            int newIndex = ColCount - 1;
            vot.RESOURCE[0].TABLE[ActiveTable].Items[newIndex] = new FIELD();
            ((FIELD)vot.RESOURCE[0].TABLE[ActiveTable].Items[newIndex]).name = name;
            ((FIELD)vot.RESOURCE[0].TABLE[ActiveTable].Items[newIndex]).datatype = (dataType)dataTypes[mstype];
            return newIndex;
        }
        private int NewEmptyCol()
        {
            TABLEDATA olddata = (TABLEDATA)vot.RESOURCE[0].TABLE[ActiveTable].DATA.Item;
            TABLEDATA data = new TABLEDATA();
            data.TR = new TR[olddata.TR.Length];
            int newIndex = ColCount - 1;
            for (int i = 0; i < olddata.TR.Length; i++)
            {
                data.TR[i] = new TR();
                data.TR[i].TD = new TD[olddata.TR[i].TD.Length + 1];
                for (int j = 0; j < olddata.TR[i].TD.Length; j++)
                {
                    data.TR[i].TD[j] = new TD();
                    data.TR[i].TD[j].Text = new string[1];
                    data.TR[i].TD[j].Text[0] = olddata.TR[i].TD[j].Text[0];
                }
                data.TR[i].TD[newIndex] = new TD();
                data.TR[i].TD[newIndex].Text = new string[1];
                data.TR[i].TD[newIndex].Text[0] = null;
            }
            vot.RESOURCE[0].TABLE[ActiveTable].DATA.Item = data;
            return newIndex;
        }
        public static VOTABLE Wrapper2VOTable(DataWrapper dw)
        {
            if (dw is VOTableWrapper) return ((VOTableWrapper)dw).vot;
            VOTABLE vot = new VOTABLE();
            vot.version = VOTABLEVersion.Item11;
            vot.RESOURCE = new RESOURCE[1];
            vot.RESOURCE[0] = new RESOURCE();
            vot.RESOURCE[0].type = RESOURCEType.results;
            int ntbl = dw.TableCount;
            vot.RESOURCE[0].TABLE = new TABLE[ntbl];
            vot.INFO = new INFO[ntbl];
            Hashtable dataTypes = VOTableUtil.getdataTypeTable();
            for (int t = 0; t < ntbl; t++)
            {
                dw.ActiveTable = t;
                vot.INFO[t] = new INFO();
                vot.INFO[t].name = "rowcount, table " + t;
                vot.INFO[t].value = "" + dw.RowCount;
                vot.RESOURCE[0].TABLE[t] = new TABLE();
                vot.RESOURCE[0].TABLE[t].Items = new object[dw.ColCount];
                for (int c = 0; c < dw.ColCount; c++)
                {
                    vot.RESOURCE[0].TABLE[t].Items[c] = new FIELD();
                    ((FIELD)vot.RESOURCE[0].TABLE[t].Items[c]).name = dw.ColName(c);
                    ((FIELD)vot.RESOURCE[0].TABLE[t].Items[c]).datatype = (dataType)dataTypes[dw.MSColType(c)];
                }
                TABLEDATA data = new TABLEDATA();
                data.TR = new TR[dw.RowCount];
                for (int r = 0; r < data.TR.Length; r++)
                {
                    data.TR[r] = new TR();
                    data.TR[r].TD = new TD[dw.ColCount];
                    for (int c = 0; c < data.TR[r].TD.Length; c++)
                    {
                        data.TR[r].TD[c] = new TD();
                        data.TR[r].TD[c].Text = new string[1];
                        data.TR[r].TD[c].Text[0] = dw[r, c].ToString();
                    }
                }
                vot.RESOURCE[0].TABLE[t].DATA = new DATA();
                vot.RESOURCE[0].TABLE[t].DATA.Item = data;
            }
            return vot;
        }
        public int NewRow() { throw new Exception("not implemented"); }
    }

}