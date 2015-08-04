using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Data.SqlTypes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using net.ivoa.data;

// this class is to read the upload file and create a temp file query string
namespace Sciserver_webService.Common
{
    public class UploadDataReader : IDataReader {

        TextReader origReader;
        public int rowsParsed;     
        string tmpFileName;
        string[] columnNames;
        string type;
        string nearBy;

        public UploadDataReader() { }
        public UploadDataReader(TextReader s)
        {
            origReader = s;
        }


        public string UploadTo(string radecText, string type, string nearBy)
        {
            this.type = type;
            this.nearBy = nearBy;
            //string CommandText = createUploadTable();
            string CommandText = ""; 
            CommandText += loadUpload(radecText);
            CommandText += createXtable();
            return CommandText;
        }

        public string UploadTo(string type, string nearBy)
        {
            this.type = type;
            this.nearBy = nearBy;
            ///string CommandText = createUploadTable();
            string CommandText = "";
            CommandText += loadUpload();
            CommandText += createXtable();
            return CommandText;
        }

        private string loadUpload()
        {
            string line = origReader.ReadLine();//advance to first real row
            columnNames = line.Split(',');

            string cmdQuery = "";
            cmdQuery = createUploadTable();
            
            
            cmdQuery += " Insert into #upload values ";
            
            int cnt = 0;
            line = origReader.ReadLine();
            while (line != null)
            {
                cnt++;
                //string[] data = line.Split(',');
                cmdQuery += "( " + cnt + "," + line + " ),";
                line = origReader.ReadLine();
            }
            cmdQuery = cmdQuery.Trim(',');
            return cmdQuery;
            
        }

      
        private string loadUpload(string radecText)
        {
            string[] lines = radecText.Split(new string[] { "\n", "\r\n", "\\n", "\\r\\n" }, StringSplitOptions.None);
            columnNames = lines[0].Split(','); 

            string cmdQuery = "";
            cmdQuery = createUploadTable();
            cmdQuery += " Insert into #upload values ";

            for (int i = 1; i < lines.Length; i++) {
                if(lines[i] != "")
                    cmdQuery += "( " + i + "," + lines[i] + " ),";
            }
            cmdQuery = cmdQuery.Trim(',');
            return cmdQuery;
        }

        private string createUploadTable()
        {

            string qry = "create table #upload ( up_id int, ";
            for (int i = 0; i < columnNames.Length; i++)
            {
                qry += " up_" + columnNames[i] + " ";
                //qry += " " + GetSqlType(i);
                //if (GetSqlType(i) == SqlDbType.VarChar)
                //    qry += "(MAX)";

                qry += " " + SqlDbType.Float;
                qry += ",";
            }
            qry = qry.Trim(',');
            qry += ")";
            return qry;
        }

        private string createXtable()
        {
            string cmd = "";
            if (type == "spec")
            {
                if (nearBy == "nearby")
                {
                    cmd = " ";
                    cmd += " CREATE TABLE #x (up_id int,SpecobjID bigint) ";
                    
                    var fun = " ";
                    fun += " dbo.fGetNearbySpecObjEq( U.up_ra ,U.up_dec ,U.up_sep )";
                    cmd += " INSERT INTO #x Select U.up_id, S.* from #upLoad U Cross Apply (select SpecObjid from " + fun + ") S ";
                }
                else
                {
                    cmd = " ";
                    cmd += " CREATE TABLE #x (up_id int,SpecobjID bigint) ";
                    
                    var fun = " ";
                    fun += " dbo.fGetNearestSpecObjIdEq( up_ra,up_dec,up_sep ) ";
                    cmd += " INSERT INTO #x SELECT up_id," + fun + "as SpecobjId ";
                    cmd += " FROM #upload WHERE" + fun + "IS NOT NULL ";
                }
            }
            else
            {
                if (nearBy == "nearby")
                {
                    cmd = " ";
                    cmd += " CREATE TABLE #x (up_id int,objID bigint) ";
                    
                    var fun = " ";
                    fun += " dbo.fGetNearbyObjEq( U.up_ra ,U.up_dec ,U.up_sep )";
                    cmd += " INSERT INTO #x Select U.up_id, S.* from #upLoad U Cross Apply (select Objid from " + fun + ") S ";
                }
                else
                {
                    cmd = " ";
                    cmd += " CREATE TABLE #x (up_id int,objID bigint) ";                    
                    var fun = " ";
                    fun += " dbo.fGetNearestObjIdEq( up_ra,up_dec,up_sep ) ";
                    cmd += " INSERT INTO #x SELECT up_id," + fun + "as objId ";
                    cmd += " FROM #upload WHERE" + fun + "IS NOT NULL ";
                }
            }
            return cmd;
        }

        

        /// <summary>
        /// This to create table for temp upload only 3 columns are allowed to upload
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        private string GetCreateTableQry(string tableName) {
            string qry = "create table " + tableName + " (\n";
            for (int i = 0; i < columnNames.Length; i++) {
                qry += "\n[" + columnNames[i] + "]";
                qry += " " + GetSqlType(i);
                if (GetSqlType(i) == SqlDbType.VarChar)
                    qry += "(MAX)";
                qry += ",";
            }
            qry = qry.Trim(',');
            qry += ")";
            return qry;            
        }

      
        void Init(string cstring, string tableName) {
            tmpFileName = Path.GetTempFileName();
            //copying to a temp file
            using (StreamWriter sw = new StreamWriter(File.OpenWrite(tmpFileName))) {
                string line = origReader.ReadLine();
                while (line != null) {
                    sw.WriteLine(line);
                    line = origReader.ReadLine();
                }
                sw.Flush();
            }
            using (StreamReader sr = new StreamReader(File.OpenRead(tmpFileName))) {
                string line = sr.ReadLine();//header line
                line = sr.ReadLine();//advance to first real row
                while (line != null) {                       
                        line = sr.ReadLine();
                }
                
            }
            
        }
        
        /// <summary>
        /// split line
        /// </summary>
        /// <param name="line"></param>
        /// <param name="splitToken"></param>
        /// <returns></returns>
        string[] splitLine(string line, char[] splitToken)
        {
            return line.Split(splitToken, StringSplitOptions.RemoveEmptyEntries);
            
        }

        int[] CTypesI;
        public SqlDbType GetSqlType(int index)
        {
            return TypeChain[CTypesI[index]].SqlType;
        }

        // Try this for the SQL type
        static readonly List<ImpType> TypeChain = new List<ImpType>() { 
            new DateTimeType(), 
            new I1Type(), new I8Type(), new I16Type(), new I32Type(), new I64Type(),//integers
            new SmallFloatType(), new BigFloatType(),//floating points
            new StrType() //default to string
        };

        public int FieldCount { get { return this.columnNames.Length; } }
        public object this[string colName] { get { return this[GetOrdinal(colName)]; } }
        public object this[int colIndex] { get { return this; } }
        public bool GetBoolean(int i) { return (bool)this[i]; }
        public byte GetByte(int i) { return (byte)this[i]; }
        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferOffset, int length) { throw new NotImplementedException(); }
        public char GetChar(int i) { return (char)this[i]; }
        public long GetChars(int i, long fieldOffset, char[] buffer, int bufferOffset, int length) { throw new NotImplementedException(); }
        public IDataReader GetData(int i) { throw new NotImplementedException(); }
        public string GetDataTypeName(int i) { return (GetFieldType(i).Name); }
        public DateTime GetDateTime(int i) { return (DateTime)this[i]; }
        public decimal GetDecimal(int i) { return (decimal)this[i]; }
        public double GetDouble(int i) { return (double)this[i]; }
        public Type GetFieldType(int i) { throw new NotImplementedException("GetFieldType"); }
        public float GetFloat(int i) { return (float)this[i]; }
        public Guid GetGuid(int i) { return (Guid)this[i]; }
        public short GetInt16(int i) { return (short)this[i]; }
        public int GetInt32(int i) { return (int)this[i]; }
        public long GetInt64(int i) { return (long)this[i]; }
        public string GetName(int i) { return this.columnNames[i]; }
        public int GetOrdinal(string colName) { return 0; }
        public string GetString(int i) { return this[i].ToString(); }
        public object GetValue(int i) { return this[i]; }
        public int GetValues(object[] values) { throw new NotImplementedException(); }
        public bool IsDBNull(int i) { return false; }
        public int Depth
        {
            get { throw new NotImplementedException(); }
        }
        public bool IsClosed
        {
            get { return false; }
        }
        public int RecordsAffected
        {
            get { throw new NotImplementedException(); }
        }
        public void Close() { }
        public DataTable GetSchemaTable() { throw new NotImplementedException(); }
        public bool NextResult()
        {
            throw new NotImplementedException("next result");
        }
        public bool Read()
        {
            //string line = tmpFileReader.ReadLine();
            //if (line == null) return false;
            //else
            //{
            //    csi.ParseRow(line);
            //    return true;
            //}
            return true;
        }
        // IDispose
        public void Dispose()
        {
            //if (tmpFileReader != null)
            //    tmpFileReader.Close();
            //if (tmpFileName != null)
            //    File.Delete(tmpFileName);
        }
    }
}