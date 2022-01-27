using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.IO;
using System.Text;
using Sciserver_webService.Common;
using System.Data.SqlClient;
using System.Data;
using System.Globalization;
using System.Web.Http;
using System.Net.Http;
using System.Net;
using System.Data.SqlTypes;

using System.Reflection;
using System.Reflection.Emit;

namespace Sciserver_webService.ToolsSearch
{
    public class MetadataSearch
    {

        protected const string ZERO_ID = "0x0000000000000000";
        Dictionary<string, string> ParameterValuePairs = new Dictionary<string, string>();
        Dictionary<String, SqlDbType> ParameterSqlTypePairs = new Dictionary<String, SqlDbType>();
        public ObjectInfo objectInfo = new ObjectInfo();

        string format = "";

        //protected HRefs hrefs = new HRefs();

        long? id = null;
        string apid;
        decimal? specId = null;
        string sidstring = null;
        double? qra = null;
        double? qdec = null;

        int? mjd = null;
        short? plate = null;
        short? fiber = null;
        private HttpCookie cookie;
        private string token = "";

        Int32? run = null;
        Int16? rerun = null;
        byte? camcol = null;
        Int16? field = null;
        Int16? obj = null;
        long? fieldId = null;
        long? plateId = null;
        int? zoom = null;
        string plateIdApogee = null;

        string ResolvedName;


        SqlConnection oConn;

        public DataSet ResultDataSet;
        public string query = "";
        public string ClientIP = "";
        public string TaskName = "";
        public string server_name = "";
        public string windows_name = "";

        public string COMMAND = "";// this has the query that is logged in SpExecuteSQL

        public string name = null;
        public string tablename = null;
        public decimal? plateID = null;
        public string apogeeplateid = null;
        public char? type = null;
        public decimal? objid = null;
        public double? ra = null;
        public double? dec = null;
        public double? radius = null;


        public MetadataSearch(Dictionary<string, string> requestDir, Dictionary<string, string> ExtraInfo, HttpRequest Request)
        {

            if (Request.QueryString.Keys.Count == 0)
            {
                throw new ArgumentException("Request has no input parameters.");
            }

            try
            {
                ClientIP = ExtraInfo["ClientIP"];
                TaskName = ExtraInfo["TaskName"];
                server_name = ExtraInfo["server_name"];
                windows_name = ExtraInfo["windows_name"];
            }
            catch (Exception e) { throw new Exception(e.Message); };

            ResultDataSet = new DataSet();

            foreach (string key in Request.QueryString.Keys)
            {
                if (key != null)
                {
                    string keyL = key.ToLower();

                    if (keyL == "query")
                    {
                        query = Request.QueryString[key];
                    }
                    else if (keyL == "name")
                        try { name = Request.QueryString[key]; }// 
                        catch { }
                    else if (keyL == "tablename")
                        try { tablename = Request.QueryString[key]; }// 
                        catch { }
                    else if (keyL == "plateid")
                        try { plateID = decimal.Parse(Request.QueryString[key]); }//
                        catch { }
                    else if (keyL == "apogeeplateid")
                        try { apogeeplateid = Request.QueryString[key]; }// 
                        catch { }
                    else if (keyL == "type")
                        try { type = char.Parse(Request.QueryString[key]); }// 
                        catch { }
                    else if (keyL == "ra")
                        try { ra = double.Parse(Request.QueryString[key]); }// 
                        catch { }
                    else if (keyL == "dec")
                        try { dec = double.Parse(Request.QueryString[key]); }// 
                        catch { }
                    else if (keyL == "radius")
                        try { radius = double.Parse(Request.QueryString[key]); }// 
                        catch { }
                    else if (keyL == "objid")
                        try { objid = decimal.Parse(Request.QueryString[key]); }// 
                        catch { }
                    else if (keyL == "run")
                        try { run = Int32.Parse(Request.QueryString[key]); }// 
                        catch { }

                }
            }

            //running the query and storing the resultset.
            using (oConn = new SqlConnection(KeyWords.DBconnectionString))
            {
                oConn.Open();
                switch (query.ToLower())
                {
                    case "dbcomponents":
                        ResultDataSet = getDBComponents(); break;
                    default://this goes to the big single-statement-query section
                        ResultDataSet = SetTableResult(query); break;
                }
                oConn.Close();
            }
        }

        public DataTable GetDataTableFromQuery(SqlConnection oConn, string query, Dictionary<String, String> StringParameterValuePairs)
        {
            DataTable dt = new DataTable();
            try
            {
                dt = GetDataSetFromQuery(oConn, query, StringParameterValuePairs).Tables[0];
            }
            catch { }
            return dt;
        }

        public DataTable GetDataTableFromQuery(SqlConnection oConn, string query)
        {
            DataTable dt = new DataTable();
            try
            {
                dt = GetDataSetFromQuery(oConn, query).Tables[0];
            }
            catch { }
            return dt;
        }

        public DataSet GetDataSetFromQuery(SqlConnection oConn, string query)
        {
            DataSet DataSet = new DataSet();
            try
            {
                SqlCommand Cmd = oConn.CreateCommand();
                Cmd.CommandText = query;
                //Cmd.CommandTimeout = KeyWords.DatabaseSearchTimeout == null || KeyWords.DatabaseSearchTimeout == "" ? 600 : Int32.Parse(KeyWords.DatabaseSearchTimeout);
                Cmd.CommandTimeout = Int32.Parse(KeyWords.DatabaseSearchTimeout);
                var Adapter = new SqlDataAdapter(Cmd);
                Adapter.Fill(DataSet);
            }
            catch { }
            return DataSet;
        }

        public DataSet GetDataSetFromQuery(SqlConnection oConn, string query, Dictionary<String, String> StringParameterValuePairs)
        {
            Dictionary<String, SqlDbType> StringParameterSqlTypePairs = new Dictionary<String, SqlDbType>();
            foreach (string parameter in StringParameterValuePairs.Keys)
            {
                StringParameterSqlTypePairs.Add(parameter, SqlDbType.NVarChar);
            }
            return GetDataSetFromQuery(oConn, query, StringParameterValuePairs, StringParameterSqlTypePairs);
        }

        public DataSet GetDataSetFromQuery(SqlConnection oConn, string query, Dictionary<String, String> ParameterValuePairs, Dictionary<String, SqlDbType> ParameterSqlTypePairs)
        {
            DataSet DataSet = new DataSet();
            try
            {
                SqlCommand Cmd = oConn.CreateCommand();
                Cmd.CommandText = query;
                foreach (string parameter in ParameterValuePairs.Keys)
                {
                    Cmd.Parameters.Add(parameter, ParameterSqlTypePairs[parameter]);
                    Cmd.Parameters[parameter].Value = ParameterValuePairs[parameter];
                }
                Cmd.CommandTimeout = Int32.Parse(KeyWords.DatabaseSearchTimeout);
                var Adapter = new SqlDataAdapter(Cmd);
                Adapter.Fill(DataSet);
            }
            catch(Exception ex)
            {
                var a = ex;
            }
            return DataSet;
        }



        public DataSet getDBComponents()
        {
            DataSet ds = new DataSet();
            string cmd = "";

            string[] TableNames = { "Tables", "Views", "Functions", "Procedures", "Constants", "Indices"};

            cmd = MetadataQueries.schema_showDropList("U");
            cmd += "; " + MetadataQueries.schema_showDropList("V");
            cmd += "; " + MetadataQueries.schema_showDropList("F");
            cmd += "; " + MetadataQueries.schema_showDropList("P");
            cmd += "; " + MetadataQueries.schema_showDropList("C");
            cmd += "; " + MetadataQueries.schema_showDropList("I");

            DataSet dd = GetDataSetFromQuery(oConn, cmd);
            ds.Merge(dd);

            for (int i = 0; i < TableNames.Length; i++)
                ds.Tables[i].TableName = TableNames[i];

            return ds;
        }


        public DataSet SetTableResult(string QueryName)
        {
            DataSet ds = new DataSet();
            string cmd = "";
            Dictionary<string, string> ParameterValuePairs = new Dictionary<string, string>();


            switch (QueryName)
            {
                //matches
                case "allTablesColumns":
                    cmd = MetadataQueries.allTablesColumns; break;
                case "tableColumns":
                    cmd = MetadataQueries.tableColumns;
                    ParameterValuePairs.Clear(); ParameterValuePairs.Add("@tablename", tablename);
                    ParameterSqlTypePairs.Clear(); ParameterSqlTypePairs.Add("@tablename", SqlDbType.NVarChar);
                    break;
                case "tableColumnNames":
                    cmd = MetadataQueries.tableColumnNames; ParameterValuePairs.Clear(); 
                    ParameterValuePairs.Clear(); ParameterValuePairs.Add("@tablename", tablename);
                    ParameterSqlTypePairs.Clear(); ParameterSqlTypePairs.Add("@tablename", SqlDbType.NVarChar);
                    break;
                case "runs":
                    cmd = MetadataQueries.runs; break;
                case "stripeFromRun":
                    cmd = MetadataQueries.stripeFromRun;
                    ParameterValuePairs.Clear(); ParameterValuePairs.Add("@run", run.ToString());
                    ParameterSqlTypePairs.Clear(); ParameterSqlTypePairs.Add("@run", SqlDbType.Int);
                    break;
                case "legacyPlates":
                    cmd = MetadataQueries.legacyPlates; break;
                case "legacyPlate":
                    cmd = MetadataQueries.legacyPlate;
                    ParameterValuePairs.Clear(); ParameterValuePairs.Add("@plateID", plateID.ToString());
                    ParameterSqlTypePairs.Clear(); ParameterSqlTypePairs.Add("@plateID", SqlDbType.Decimal);
                    break;
                case "apogeePlate":
                    cmd = MetadataQueries.apogeePlate;
                    ParameterValuePairs.Clear(); ParameterValuePairs.Add("@apogeeplateid", apogeeplateid.ToString());
                    ParameterSqlTypePairs.Clear(); ParameterSqlTypePairs.Add("@apogeeplateid", SqlDbType.NVarChar);
                    break;
                case "runs2":
                    cmd = MetadataQueries.runs2; break;
                case "parent":
                    cmd = MetadataQueries.schema_parentfromViewName;
                    ParameterValuePairs.Clear(); ParameterValuePairs.Add("@name", name);
                    ParameterSqlTypePairs.Clear(); ParameterSqlTypePairs.Add("@name", SqlDbType.NVarChar);
                    break;
                case "constantslist":
                    cmd = MetadataQueries.constants_list; break;
                case "constants":
                    cmd = MetadataQueries.schema_constants(name);
                    break;
                case "constantsFields":
                    cmd = MetadataQueries.schema_constantsFields; break;
                case "shortTable":
                    cmd = MetadataQueries.schema_showShortTable(type);
                    ParameterValuePairs.Clear(); ParameterValuePairs.Add("@type", type.ToString());
                    ParameterSqlTypePairs.Clear(); ParameterSqlTypePairs.Add("@type", SqlDbType.Char);
                    break;
                case "function":
                    cmd = MetadataQueries.schema_function;
                    ParameterValuePairs.Clear(); ParameterValuePairs.Add("@name", name.ToString());
                    ParameterSqlTypePairs.Clear(); ParameterSqlTypePairs.Add("@name", SqlDbType.NVarChar);
                    break;
                case "indices":
                    cmd = MetadataQueries.schema_indices(name);
                    ParameterValuePairs.Clear(); ParameterValuePairs.Add("@name", name.ToString());
                    ParameterSqlTypePairs.Clear(); ParameterSqlTypePairs.Add("@name", SqlDbType.NVarChar);
                    break;
                case "description":
                    cmd = MetadataQueries.schema_description;
                    ParameterValuePairs.Clear(); ParameterValuePairs.Add("@name", name.ToString());
                    ParameterSqlTypePairs.Clear(); ParameterSqlTypePairs.Add("@name", SqlDbType.NVarChar);
                    break;
                case "descriptionFromDBObjects":
                    cmd = MetadataQueries.descriptionFromDBObjects;
                    ParameterValuePairs.Clear(); ParameterValuePairs.Add("@name", name.ToString());
                    break;
                case "textFromDBObjects":
                    cmd = MetadataQueries.textFromDBObjects;
                    ParameterValuePairs.Clear(); ParameterValuePairs.Add("@name", name.ToString());
                    break;
                case "access":
                    cmd = MetadataQueries.schema_access;
                    ParameterValuePairs.Clear(); ParameterValuePairs.Add("@name", "%" + name.ToString() + "%");
                    ParameterSqlTypePairs.Clear(); ParameterSqlTypePairs.Add("@name", SqlDbType.NVarChar);
                    break;
                case "enum":
                    cmd = MetadataQueries.schema_enum;
                    ParameterValuePairs.Clear(); ParameterValuePairs.Add("@name", name.ToString());
                    ParameterSqlTypePairs.Clear(); ParameterSqlTypePairs.Add("@name", SqlDbType.NVarChar);
                    break;

                case "nearestobj":
                    if (ra == null || dec == null || radius == null)
                        throw new ArgumentException("Unspecified value of either ra, dec or radius.");
                    cmd = MetadataQueries.nearestobj;
                    ParameterValuePairs.Clear(); ParameterValuePairs.Add("@ra", ra.ToString()); ParameterValuePairs.Add("@dec", dec.ToString()); ParameterValuePairs.Add("@radius", radius.ToString());
                    ParameterSqlTypePairs.Clear(); ParameterSqlTypePairs.Add("@ra", SqlDbType.Float); ParameterSqlTypePairs.Add("@dec", SqlDbType.Float); ParameterSqlTypePairs.Add("@radius", SqlDbType.Float);
                    break;
                case "nearestspecobjid":
                    cmd = MetadataQueries.nearestspecobjid;
                    ParameterValuePairs.Clear(); ParameterValuePairs.Add("@objid", objid.ToString());
                    ParameterSqlTypePairs.Clear(); ParameterSqlTypePairs.Add("@objid", SqlDbType.NVarChar);
                    break;
                case "nearestapogee":
                    if (ra == null || dec == null || radius == null)
                        throw new ArgumentException("Unspecified value of either ra, dec or radius.");
                    cmd = MetadataQueries.nearestapogee;
                    ParameterValuePairs.Clear(); ParameterValuePairs.Add("@ra", ra.ToString()); ParameterValuePairs.Add("@dec", dec.ToString()); ParameterValuePairs.Add("@radius", radius.ToString());
                    ParameterSqlTypePairs.Clear(); ParameterSqlTypePairs.Add("@ra", SqlDbType.Float); ParameterSqlTypePairs.Add("@dec", SqlDbType.Float); ParameterSqlTypePairs.Add("@radius", SqlDbType.Float);
                    break;
                case "fieldfromname":
                    cmd = MetadataQueries.fieldfromname;
                    ParameterValuePairs.Clear(); ParameterValuePairs.Add("@name", name.ToString());
                    ParameterSqlTypePairs.Clear(); ParameterSqlTypePairs.Add("@name", SqlDbType.NVarChar);
                    break;
                case "photoflags":
                    cmd = MetadataQueries.photoflags; break;
                case "imgparams":
                    cmd = MetadataQueries.imgparams; break;
                case "specparams":
                    cmd = MetadataQueries.specparams; break;
                case "irspecparams":
                    cmd = MetadataQueries.irspecparams; break;
                case "primtargetflags":
                    cmd = MetadataQueries.primtargetflags; break;
                case "sectargetflags":
                    cmd = MetadataQueries.sectargetflags; break;
                case "bosstargetflags":
                    cmd = MetadataQueries.bosstargetflags; break;
                case "ebosstargetflags":
                    cmd = MetadataQueries.ebosstargetflags; break;
                case "apogeetarget1flags":
                    cmd = MetadataQueries.apogeetarget1flags; break;
                case "apogeetarget2flags":
                    cmd = MetadataQueries.apogeetarget2flags; break;
                // implementing TAP schema:
                case "alltables":
                    cmd = MetadataQueries.allTables; break;
                case "onlyviews":
                    cmd = MetadataQueries.onlyViews; break;
                case "onlytables":
                    cmd = MetadataQueries.onlyTables; break;
                case "allcolumns":
                    cmd = MetadataQueries.allTablesColumns; break;
                case "columnsfortable":
                    cmd = MetadataQueries.columnsForTable;
                    ParameterValuePairs.Clear(); ParameterValuePairs.Add("@name", name.ToString());
                    ParameterSqlTypePairs.Clear(); ParameterSqlTypePairs.Add("@name", SqlDbType.NVarChar);
                    break;
                case "allfunctionsdata":
                    cmd = MetadataQueries.allFunctionsData;break;
                case "allfunctionsdescriptions":
                    cmd = MetadataQueries.allFunctionsDescriptions; break;
                case "functionparameters":
                    cmd = MetadataQueries.functionParameters;
                    ParameterValuePairs.Clear(); ParameterValuePairs.Add("@name", name.ToString());
                    ParameterSqlTypePairs.Clear(); ParameterSqlTypePairs.Add("@name", SqlDbType.NVarChar);
                    break;
                case "allproceduresdata":
                    cmd = MetadataQueries.allProceduresData; break;
                case "allproceduresdescriptions":
                    cmd = MetadataQueries.allFunctionsDescriptions; break;
                case "procedureparameters":
                    cmd = MetadataQueries.proceduresParameters;
                    ParameterValuePairs.Clear(); ParameterValuePairs.Add("@name", name.ToString());
                    ParameterSqlTypePairs.Clear(); ParameterSqlTypePairs.Add("@name", SqlDbType.NVarChar);
                    break;
                case "allindexes":
                    cmd = MetadataQueries.allIndexes; break;
                case "indexesfortable":
                    cmd = MetadataQueries.indexesForTable;
                    ParameterValuePairs.Clear(); ParameterValuePairs.Add("@name", name.ToString());
                    ParameterSqlTypePairs.Clear(); ParameterSqlTypePairs.Add("@name", SqlDbType.NVarChar);
                    break;
                case "sdssplatemjd":
                    cmd = MetadataQueries.sdssPlateMJDList;
                    break;
                case "segueplatemjd":
                    cmd = MetadataQueries.seguePlateMJDList;
                    break;
                case "bossplatemjd":
                    cmd = MetadataQueries.bossPlateMJDList;
                    break;
                case "apogeeplatemjd":
                    cmd = MetadataQueries.apogeePlateMJDList;
                    break;
                default:
                    cmd = ""; break;
            }
            ds.Merge(GetDataSetFromQuery(oConn, cmd, ParameterValuePairs));
            return ds;
        }

        
    }
}