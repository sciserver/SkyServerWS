using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using net.ivoa.VOTable;
using Newtonsoft.Json;
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
    public class ObjectSearch
    {

        protected const string ZERO_ID = "0x0000000000000000";
        Dictionary<string, string> ParameterValuePairs = new Dictionary<string, string>();
        public ObjectInfo objectInfo = new ObjectInfo();

        string format = "";

        //protected HRefs hrefs = new HRefs();

        long? id = null;
        string apid;
        long? specId = null;
        string sidstring = null;
        double? qra = null;
        double? qdec = null;

        int? mjd = null;
        short? plate = null;
        short? fiber = null;
        private HttpCookie cookie;
        private string token = "";

        Int16? run = null;
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


        public ObjectSearch(Dictionary<string, string> requestDir, Dictionary<string, string> ExtraInfo, HttpRequest Request)
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

                    if (keyL == "name")
                        try { resolveName(Request.QueryString[key]); }// this sets qra and qdec to be those of the resolved object
                        catch { }
                    if (keyL == "id" || keyL == "objid")
                    {
                        string s = Request.QueryString[key];
                        id = Utilities.ParseId(s);
                    }
                    if (keyL == "sid")
                    {
                        string s = Request.QueryString[key].Trim().ToUpper();
                        if (s != null & !"".Equals(s) & (s.StartsWith("APOGEE") || s.StartsWith("2M")))
                            apid = HttpUtility.UrlEncode(Request.QueryString[key]); //sidstring = s;
                        else
                            sidstring = (string.Equals(s, "")) ? s : Utilities.ParseId(Request.QueryString[key]).ToString();
                    }
                    if (keyL == "spec" || keyL == "specobjid")
                    {
                        string s = Request.QueryString[key];
                        sidstring = (string.Equals(s, "")) ? s : Utilities.ParseId(s).ToString();
                    }
                    if (keyL == "apid")
                    {
                        string s = HttpUtility.UrlEncode(Request.QueryString[key]);
                        if (s != null & !"".Equals(s) & (s.ToLower().StartsWith("apogee") || s.ToLower().StartsWith("2m")))
                        {
                            apid = s;
                        }
                    }
                    if (keyL == "ra") try { qra = Utilities.parseRA(Request.QueryString[key]); }
                        catch { }// need to parse J2000
                    if (keyL == "dec") try { qdec = Utilities.parseDec(Request.QueryString[key]); }
                        catch { } // need to parse J2000
                    if (keyL == "plate") try { plate = short.Parse(Request.QueryString[key]); }
                        catch { }
                    if (keyL == "mjd") try { mjd = int.Parse(Request.QueryString[key]); }
                        catch { }
                    if (keyL == "fiber") try { fiber = short.Parse(Request.QueryString[key]); }
                        catch { }

                    if (keyL == "run")
                    {
                        try { string s = Request.QueryString[key]; run = string.Equals(s, "") ? run : Convert.ToInt16(s); }
                        catch { }
                    }
                    if (keyL == "rerun")
                    {
                        try { string s = Request.QueryString[key]; rerun = string.Equals(s, "") ? rerun : Convert.ToInt16(s); }
                        catch { }
                    }
                    if (keyL == "camcol")
                    {
                        try { string s = Request.QueryString[key]; camcol = string.Equals(s, "") ? camcol : Convert.ToByte(s); }
                        catch { }
                    }
                    if (keyL == "field")
                    {
                        try { string s = Request.QueryString[key]; field = string.Equals(s, "") ? field : Convert.ToInt16(s); }
                        catch { }
                    }
                    if (keyL == "obj")
                    {
                        try { string s = Request.QueryString[key]; obj = string.Equals(s, "") ? obj : Convert.ToInt16(s); }
                        catch { }
                    }
                    if (keyL == "fieldid")
                    {
                        try { string s = Request.QueryString[key]; fieldId = string.Equals(s, "") ? fieldId : Utilities.ParseId(s); }
                        catch { }
                    }
                    if (keyL == "plateid")
                    {
                        try { string s = Request.QueryString[key]; plateId = string.Equals(s, "") ? plateId : Utilities.ParseId(s); }
                        catch { }
                    }
                    if (keyL == "zoom")
                    {
                        try { string s = Request.QueryString[key]; zoom = string.Equals(s, "") ? zoom : Convert.ToInt32(s); }
                        catch { }
                    }
                    if (keyL == "plateidapogee")
                    {
                        string s = Request.QueryString[key];
                        if (s.StartsWith("apogee"))
                            plateIdApogee = s;
                        else
                            plateIdApogee = null;
                    }
                }
            }

            using (oConn = new SqlConnection(KeyWords.DBconnectionString))
            {
                oConn.Open();
                switch (query.ToLower())
                {
                    case "loadexplore":
                        getObjPmts(); parseIds(); ResultDataSet = getLoadExplore(); break;
                    case "specfitparameters":
                        parseIdsForTableResult(); ResultDataSet = getSpecFitParameters(); break;
                    case "galaxyzoo":
                        parseIdsForTableResult(); ResultDataSet = getGalaxyZoo(); break;
                    case "neighbors":
                        parseIdsForTableResult(); ResultDataSet = getNeighbors(); break;
                    case "matches":
                        parseIdsForTableResult(); ResultDataSet = getMatches(); break;
                    case "allspec":
                        parseIdsForTableResult(); ResultDataSet = getAllSpec(); break;
                    case "plate":
                        parseIdsForTableResult(); ResultDataSet = getPlate(); break;
                    default://this goes to the big single-statement-query section
                        parseIdsForTableResult(); ResultDataSet = SetTableResult(query); break;
                }
                oConn.Close();
            }
        }

        
        private void parseIdsForTableResult()
        {
            if(id != null)
                objectInfo.id = id;
            if (id != null)
                objectInfo.objId = id.ToString();
            if (sidstring != null && sidstring != "")
            {
                objectInfo.specId = Int64.Parse(sidstring);
                objectInfo.specObjId = sidstring;
            }
            if (apid != null) 
                objectInfo.apid = apid;
            if (qra != null) 
                objectInfo.ra = qra;
            if (qdec != null) 
                objectInfo.dec = qdec;
            if (plate != null)
                objectInfo.plate = plate;
            if (mjd != null) 
                objectInfo.mjd = mjd;
            if (fiber != null) 
                objectInfo.fiberId = fiber;
            if (run != null) 
                objectInfo.run = run;
            if (rerun != null) 
                objectInfo.rerun = rerun;
            if (camcol != null) 
                objectInfo.camcol = camcol;
            if (field != null) 
                objectInfo.field = field;
            if (fieldId != null) 
                objectInfo.fieldId = fieldId.ToString();
            if (plateId != null)
                objectInfo.plateId = plateId;
            if (zoom != null)
                objectInfo.zoom = zoom;
            if (plateIdApogee != null)
                objectInfo.plateIdApogee = plateIdApogee;
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
            catch { }
            return DataSet;
        }



        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------


/*
        private DataTable parseToDataTable(list theObject)
        {
            DataTable deta = new DataTable();

            foreach (PropertyInfo info in typeof(TheObject.Item).GetProperties())
            {
                deta.Columns.Add(info.Name, info.PropertyType);
            }
            deta.AcceptChanges();


            foreach (workspace.TheObject.Item item in theObject)
            {
                workspace.TheObject.Item datos = new workspace.TheObject.Item();
                datos = item;
                DataRow row = deta.NewRow();

                foreach (var property in datos.GetType().GetProperties())
                {
                    row[property.Name] = property.GetValue(datos, null);
                }
                deta.Rows.Add(row);
            }

            deta.AcceptChanges();
            return deta;
        }
*/



        public DataSet getSpecById()
        {
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();
            string cmd = "";

            string[] TableNames1 = {"SpecById"};

            if (objectInfo.specObjId != null && !objectInfo.specObjId.Equals(""))
            {
                cmd = SpecQueries.SpecById.Replace("@specId", objectInfo.specObjId.ToString());
                ds.Merge(GetDataSetFromQuery(oConn, cmd));
            }
            else
            {
                for (int i = 0; i < TableNames1.Length; i++)
                    ds.Merge(new DataTable());
            }
            for (int i = 0; i < TableNames1.Length; i++)
                ds.Tables[i].TableName = TableNames1[i];

            return ds;
        }


        public DataSet getPlate()
        {
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();
            string cmd = "";

            dt.Reset();
            dt.Columns.Add("objId", typeof(string));
            dt.Columns.Add("specObjId", typeof(string));
            dt.Columns.Add("apid", typeof(string));
            dt.Columns.Add("id", typeof(long));
            dt.Columns.Add("specId", typeof(long));
            dt.Columns.Add("name", typeof(string));
            dt.Rows.Add(new object[] { objectInfo.objId, objectInfo.specObjId, objectInfo.apid, objectInfo.id, objectInfo.specId, objectInfo.name });
            dt.TableName = "objectInfo";
            ds.Merge(dt);


            string[] TableNames1 = { "Plate", "PlateShow" };

            if (objectInfo.plateId != null && !objectInfo.plateId.Equals(""))
            {
                cmd = ExploreQueries.Plate.Replace("@plateId", objectInfo.plateId.ToString());
                cmd += "; " + ExploreQueries.PlateShow.Replace("@plateId", objectInfo.plateId.ToString());
                ds.Merge(GetDataSetFromQuery(oConn, cmd));
            }
            else
            {
                for (int i = 0; i < TableNames1.Length; i++)
                    ds.Merge(new DataTable());
            }
            for (int i = 0; i < TableNames1.Length; i++)
                ds.Tables[i + 1].TableName = TableNames1[i];

            return ds;
        }


        public DataSet getAllSpec()
        {
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();
            string cmd = "";

            dt.Reset();
            dt.Columns.Add("objId", typeof(string));
            dt.Columns.Add("specObjId", typeof(string));
            dt.Columns.Add("apid", typeof(string));
            dt.Columns.Add("id", typeof(long));
            dt.Columns.Add("specId", typeof(long));
            dt.Columns.Add("name", typeof(string));
            dt.Rows.Add(new object[] { objectInfo.objId, objectInfo.specObjId, objectInfo.apid, objectInfo.id, objectInfo.specId, objectInfo.name });
            dt.TableName = "objectInfo";
            ds.Merge(dt);


            string[] TableNames1 = { "AllSpec1", "AllSpec2" };

            if (objectInfo.objId != null && !objectInfo.objId.Equals(""))
            {
                cmd = ExploreQueries.AllSpec1.Replace("@objId", objectInfo.objId.ToString());
                cmd += "; " + ExploreQueries.AllSpec2.Replace("@objId", objectInfo.objId.ToString());
                ds.Merge(GetDataSetFromQuery(oConn, cmd));
            }
            else
            {
                for (int i = 0; i < TableNames1.Length; i++)
                    ds.Merge(new DataTable());
            }
            for (int i = 0; i < TableNames1.Length; i++)
                ds.Tables[i + 1].TableName = TableNames1[i];

            return ds;
        }




        public DataSet getNeighbors()
        {
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();
            string cmd = "";

            dt.Reset();
            dt.Columns.Add("objId", typeof(string));
            dt.Columns.Add("specObjId", typeof(string));
            dt.Columns.Add("apid", typeof(string));
            dt.Columns.Add("id", typeof(long));
            dt.Columns.Add("specId", typeof(long));
            dt.Columns.Add("name", typeof(string));
            dt.Rows.Add(new object[] { objectInfo.objId, objectInfo.specObjId, objectInfo.apid, objectInfo.id, objectInfo.specId, objectInfo.name });
            dt.TableName = "objectInfo";
            ds.Merge(dt);


            string[] TableNames1 = { "neighbors1", "neighbors2" };

            if (objectInfo.objId != null && !objectInfo.objId.Equals(""))
            {
                cmd = ExploreQueries.neighbors1.Replace("@objId", objectInfo.objId.ToString());
                cmd += "; " + ExploreQueries.neighbors2.Replace("@objId", objectInfo.objId.ToString());
                ds.Merge(GetDataSetFromQuery(oConn, cmd));
            }
            else
            {
                for (int i = 0; i < TableNames1.Length; i++)
                    ds.Merge(new DataTable());
            }
            for (int i = 0; i < TableNames1.Length; i++)
                ds.Tables[i + 1].TableName = TableNames1[i];

            return ds;
        }

        public DataSet getMatches()
        {
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();
            string cmd = "";

            dt.Reset();
            dt.Columns.Add("objId", typeof(string));
            dt.Columns.Add("specObjId", typeof(string));
            dt.Columns.Add("apid", typeof(string));
            dt.Columns.Add("id", typeof(long));
            dt.Columns.Add("specId", typeof(long));
            dt.Columns.Add("name", typeof(string));
            dt.Rows.Add(new object[] { objectInfo.objId, objectInfo.specObjId, objectInfo.apid, objectInfo.id, objectInfo.specId, objectInfo.name });
            dt.TableName = "objectInfo";
            ds.Merge(dt);


            string[] TableNames1 = { "matches1", "matches2" };

            if (objectInfo.objId != null && !objectInfo.objId.Equals(""))
            {
                cmd = ExploreQueries.matches1.Replace("@objId", objectInfo.objId.ToString());
                cmd += "; " + ExploreQueries.matches2.Replace("@objId", objectInfo.objId.ToString());
                ds.Merge(GetDataSetFromQuery(oConn, cmd));
            }
            else
            {
                for (int i = 0; i < TableNames1.Length; i++)
                    ds.Merge(new DataTable());
            }
            for (int i = 0; i < TableNames1.Length; i++)
                ds.Tables[i + 1].TableName = TableNames1[i];

            return ds;
        }


        public DataSet getSpecFitParameters()
        {
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();
            string cmd = "";

            dt.Reset();
            dt.Columns.Add("objId", typeof(string));
            dt.Columns.Add("specObjId", typeof(string));
            dt.Columns.Add("apid", typeof(string));
            dt.Columns.Add("id", typeof(long));
            dt.Columns.Add("specId", typeof(long));
            dt.Columns.Add("name", typeof(string));
            dt.Rows.Add(new object[] { objectInfo.objId, objectInfo.specObjId, objectInfo.apid, objectInfo.id, objectInfo.specId, objectInfo.name });
            dt.TableName = "objectInfo";
            ds.Merge(dt);


            string[] TableNames1 = { "fitsParametersSppParams", "fitsParametersStellarMassStarformingPort", "fitsParameterSstellarMassPassivePort", "fitsParametersEmissionLinesPort",
                                    "fitsParametersStellarMassPCAWiscBC03", "fitsParametersstellarMassPCAWiscM11", "fitsParametersStellarmassFSPSGranEarlyDust", 
                                    "fitsParametersStellarmassFSPSGranEarlyNoDust", "fitsParametersStellarmassFSPSGranWideDust", "fitsParametersStellarmassFSPSGranWideNoDust"};

            if (objectInfo.specId != null && !objectInfo.specId.Equals(""))
            {
                cmd = ExploreQueries.fitsParametersSppParams.Replace("@specId", objectInfo.specId.ToString());
                cmd += "; " + ExploreQueries.fitsParametersStellarMassStarformingPort.Replace("@specId", objectInfo.specId.ToString());
                cmd += "; " + ExploreQueries.fitsParameterSstellarMassPassivePort.Replace("@specId", objectInfo.specId.ToString());
                cmd += "; " + ExploreQueries.fitsParametersEmissionLinesPort.Replace("@specId", objectInfo.specId.ToString());
                cmd += "; " + ExploreQueries.fitsParametersStellarMassPCAWiscBC03.Replace("@specId", objectInfo.specId.ToString());
                cmd += "; " + ExploreQueries.fitsParametersstellarMassPCAWiscM11.Replace("@specId", objectInfo.specId.ToString());
                cmd += "; " + ExploreQueries.fitsParametersStellarmassFSPSGranEarlyDust.Replace("@specId", objectInfo.specId.ToString());
                cmd += "; " + ExploreQueries.fitsParametersStellarmassFSPSGranEarlyNoDust.Replace("@specId", objectInfo.specId.ToString());
                cmd += "; " + ExploreQueries.fitsParametersStellarmassFSPSGranWideDust.Replace("@specId", objectInfo.specId.ToString());
                cmd += "; " + ExploreQueries.fitsParametersStellarmassFSPSGranWideNoDust.Replace("@specId", objectInfo.specId.ToString());

                ds.Merge(GetDataSetFromQuery(oConn, cmd));
            }
            else
            {
                for (int i = 0; i < TableNames1.Length; i++)
                    ds.Merge(new DataTable());
            }
            for (int i = 0; i < TableNames1.Length; i++)
                ds.Tables[i + 1].TableName = TableNames1[i];

            return ds;
        }

        public DataSet getGalaxyZoo()
        {
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();
            string cmd = "";

            dt.Reset();
            dt.Columns.Add("objId", typeof(string));
            dt.Columns.Add("specObjId", typeof(string));
            dt.Columns.Add("apid", typeof(string));
            dt.Columns.Add("id", typeof(long));
            dt.Columns.Add("specId", typeof(long));
            dt.Columns.Add("name", typeof(string));
            dt.Rows.Add(new object[] { objectInfo.objId, objectInfo.specObjId, objectInfo.apid, objectInfo.id, objectInfo.specId, objectInfo.name });
            dt.TableName = "objectInfo";
            ds.Merge(dt);


            string[] TableNames1 = { "zooSpec1", "zooSpec2", "zooNoSpec", "zooConfidence", "zooConfidence2", 
                                    "zooMirrorBias", "zooMirrorBias2", "zooMonochromeBias", "zooMonochromeBias2", "zoo2MainSpecz", "zoo2MainSpecz2", "zoo2MainPhotoz",  
                                    "zoo2MainPhotoz2", "zoo2Stripe82Normal", "zoo2Stripe82Normal2", "zoo2Stripe82Coadd1", "zoo2Stripe82Coadd1_2", 
                                    "zoo2Stripe82Coadd2", "zoo2Stripe82Coadd2_2" };

            if (objectInfo.objId != null && !objectInfo.objId.Equals(""))
            {
                cmd  = ExploreQueries.zooSpec1.Replace("@objId", objectInfo.objId);
                cmd += "; " + ExploreQueries.zooSpec2.Replace("@objId", objectInfo.objId);
                cmd += "; " + ExploreQueries.zooNoSpec.Replace("@objId", objectInfo.objId);
                cmd += "; " + ExploreQueries.zooConfidence.Replace("@objId", objectInfo.objId);
                cmd += "; " + ExploreQueries.zooConfidence2.Replace("@objId", objectInfo.objId);
                cmd += "; " + ExploreQueries.zooMirrorBias.Replace("@objId", objectInfo.objId);
                cmd += "; " + ExploreQueries.zooMirrorBias2.Replace("@objId", objectInfo.objId);
                cmd += "; " + ExploreQueries.zooMonochromeBias.Replace("@objId", objectInfo.objId);
                cmd += "; " + ExploreQueries.zooMonochromeBias2.Replace("@objId", objectInfo.objId);
                cmd += "; " + ExploreQueries.zoo2MainSpecz.Replace("@objId", objectInfo.objId);
                cmd += "; " + ExploreQueries.zoo2MainSpecz2.Replace("@objId", objectInfo.objId);
                cmd += "; " + ExploreQueries.zoo2MainPhotoz.Replace("@objId", objectInfo.objId);
                cmd += "; " + ExploreQueries.zoo2MainPhotoz2.Replace("@objId", objectInfo.objId);
                cmd += "; " + ExploreQueries.zoo2Stripe82Normal.Replace("@objId", objectInfo.objId);
                cmd += "; " + ExploreQueries.zoo2Stripe82Normal2.Replace("@objId", objectInfo.objId);
                cmd += "; " + ExploreQueries.zoo2Stripe82Coadd1.Replace("@objId", objectInfo.objId);
                cmd += "; " + ExploreQueries.zoo2Stripe82Coadd1_2.Replace("@objId", objectInfo.objId);
                cmd += "; " + ExploreQueries.zoo2Stripe82Coadd2.Replace("@objId", objectInfo.objId);
                cmd += "; " + ExploreQueries.zoo2Stripe82Coadd2_2.Replace("@objId", objectInfo.objId);

                ds.Merge(GetDataSetFromQuery(oConn, cmd));
            }
            else
            {
                for (int i = 0; i < TableNames1.Length; i++)
                    ds.Merge(new DataTable());
            }
            for (int i = 0; i < TableNames1.Length; i++)
                ds.Tables[i + 1].TableName = TableNames1[i];

            return ds;
        }


        public DataSet getLoadExplore()
        {
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();
            string cmd = "";

            dt.Reset();
            dt.Columns.Add("objId", typeof(string));
            dt.Columns.Add("specObjId", typeof(string));
            dt.Columns.Add("apid", typeof(string));
            dt.Columns.Add("id", typeof(long));
            dt.Columns.Add("specId", typeof(long));
            dt.Columns.Add("name", typeof(string));
            dt.Rows.Add(new object[] { objectInfo.objId, objectInfo.specObjId, objectInfo.apid, objectInfo.id, objectInfo.specId, objectInfo.name});
            dt.TableName = "objectInfo";
            ds.Merge(dt);


            string[] TableNames = { "MetaData", "ImagingData", "ImagingDataUnits", "CrossId_USNO", "CrossId_FIRST", "CrossId_ROSAT", "CrossId_RC3", "CrossId_WISE", "CrossId_TWOMASS", "QuickLookMetaData" };
            
            if (objectInfo.objId != null && !objectInfo.objId.Equals(""))
            {
                cmd = ExploreQueries.getObjParamaters.Replace("@objId", objectInfo.objId);
                cmd += "; " + ExploreQueries.getImagingQuery.Replace("@objId", objectInfo.objId);
                cmd += "; " + ExploreQueries.unitQuery;
                cmd += "; " + ExploreQueries.USNO.Replace("@objId", objectInfo.objId);
                cmd += "; " + ExploreQueries.FIRST.Replace("@objId", objectInfo.objId);
                cmd += "; " + ExploreQueries.ROSAT.Replace("@objId", objectInfo.objId);
                cmd += "; " + ExploreQueries.RC3.Replace("@objId", objectInfo.objId);
                cmd += "; " + ExploreQueries.WISE.Replace("@objId", objectInfo.objId);
                cmd += "; " + ExploreQueries.TWOMASS.Replace("@objId", objectInfo.objId);
                cmd += "; " + QuickLookQueries.getParamsFromObjID.Replace("@objid", objectInfo.objId);
                
                DataSet dd = GetDataSetFromQuery(oConn, cmd);
                ds.Merge(dd);
            }
            else
            {
                for (int i = 0; i < TableNames.Length;i++)
                    ds.Merge(new DataTable());
            }
            for (int i = 0; i < TableNames.Length; i++)
                ds.Tables[i+1].TableName = TableNames[i];



            if (objectInfo.specId != null && !objectInfo.specId.Equals(""))
            {
                cmd = ExploreQueries.getSpectroQuery.Replace("@objId", objectInfo.objId).Replace("@specId", objectInfo.specId.ToString());
                dt = GetDataTableFromQuery(oConn, cmd);
                dt.TableName = "SpectralData";
                ds.Merge(dt);
            }
            else
            {
                dt.Reset();
                dt.TableName = "SpectralData";
                ds.Merge(dt);
            }




            // executing the apogee queries:
            if (objectInfo.apid != null && !objectInfo.apid.Equals(""))
            {
                string cmd2 = "";
                string id = objectInfo.apid;
                string[] injection = new string[] { " ", "--", ";", "/*", "*/", "'", "\"" };
                bool DoQuery = true;
                foreach (string s in injection)
                {
                    if (objectInfo.apid.IndexOf(s) > 0)
                        DoQuery = false;
                }
                if (DoQuery)
                {
                    string FIND_APSTAR_ID = @"where a.apstar_id = @id";
                    string FIND_APOGEE_ID = @"where a.apogee_id = @id";

                    if (id.StartsWith("apogee")) { cmd2 = ExploreQueries.APOGEE_BASE_QUERY + FIND_APSTAR_ID; }
                    else { cmd2 = ExploreQueries.APOGEE_BASE_QUERY + FIND_APOGEE_ID; }
                    ParameterValuePairs.Clear(); ParameterValuePairs.Add("@id", id);
                    //cmd2 = cmd2.Replace("@id", "'" + id + "'");

                    dt = GetDataTableFromQuery(oConn, cmd2, ParameterValuePairs);
                    dt.TableName = "ApogeeData";
                    ds.Merge(dt);

                    string apogee_id = (string)dt.Rows[0]["apogee_id"];
                    cmd2 = ExploreQueries.APOGEEVISITS_BASE_QUERY;
                    ParameterValuePairs.Clear(); ParameterValuePairs.Add("@id", apogee_id);
                    //cmd2 = cmd2.Replace("@id", "'" + apogee_id + "'");

                    dt = GetDataTableFromQuery(oConn, cmd2, ParameterValuePairs);
                    dt.TableName = "ApogeeVisits";
                    ds.Merge(dt);
                }
            }
            else
            {
                dt.Reset();
                dt.TableName = "ApogeeData";
                ds.Merge(dt);
                dt.TableName = "ApogeeVisits";
                ds.Merge(dt);
            }

            return ds;
        }


        public DataSet getInfoSection2()
        {
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();
            string cmd = "";

            dt.Reset();
            dt.Columns.Add("objId", typeof(string));
            dt.Columns.Add("specObjId", typeof(string));
            dt.Columns.Add("apid", typeof(string));
            dt.Columns.Add("id", typeof(long));
            dt.Columns.Add("specId", typeof(long));
            dt.Columns.Add("name", typeof(string));
            dt.Rows.Add(new object[] { objectInfo.objId, objectInfo.specObjId, objectInfo.apid, objectInfo.id, objectInfo.specId, objectInfo.name });
            dt.TableName = "objectInfo";
            ds.Merge(dt);


            string[] TableNames1 = { "matches1", "matches2", "neighbors1", "neighbors2", "zooSpec1", "zooSpec2", "zooNoSpec", "zooConfidence", "zooConfidence2", 
                                    "zooMirrorBias", "zooMirrorBias2", "zooMonochromeBias", "zooMonochromeBias2", "zoo2MainSpecz", "zoo2MainSpecz2", "zoo2MainPhotoz",  
                                    "zoo2MainPhotoz2", "zoo2Stripe82Normal", "zoo2Stripe82Normal2", "zoo2Stripe82Coadd1", "zoo2Stripe82Coadd1_2", 
                                    "zoo2Stripe82Coadd2", "zoo2Stripe82Coadd2_2", "AllSpec1", "AllSpec2" };

            if (objectInfo.objId != null && !objectInfo.objId.Equals(""))
            {
                //other observations
                cmd = ExploreQueries.matches1.Replace("@objId", objectInfo.objId);
                cmd += "; " + ExploreQueries.matches2.Replace("@objId", objectInfo.objId);
                //neighbors
                cmd += "; " + ExploreQueries.neighbors1.Replace("@objId", objectInfo.objId);
                cmd += "; " + ExploreQueries.neighbors2.Replace("@objId", objectInfo.objId);
                // galaxy zoo
                cmd += "; " + ExploreQueries.zooSpec1.Replace("@objId", objectInfo.objId);
                cmd += "; " + ExploreQueries.zooSpec2.Replace("@objId", objectInfo.objId);
                cmd += "; " + ExploreQueries.zooNoSpec.Replace("@objId", objectInfo.objId);
                cmd += "; " + ExploreQueries.zooConfidence.Replace("@objId", objectInfo.objId);
                cmd += "; " + ExploreQueries.zooConfidence2.Replace("@objId", objectInfo.objId);
                cmd += "; " + ExploreQueries.zooMirrorBias.Replace("@objId", objectInfo.objId);
                cmd += "; " + ExploreQueries.zooMirrorBias2.Replace("@objId", objectInfo.objId);
                cmd += "; " + ExploreQueries.zooMonochromeBias.Replace("@objId", objectInfo.objId);
                cmd += "; " + ExploreQueries.zooMonochromeBias2.Replace("@objId", objectInfo.objId);
                cmd += "; " + ExploreQueries.zoo2MainSpecz.Replace("@objId", objectInfo.objId);
                cmd += "; " + ExploreQueries.zoo2MainSpecz2.Replace("@objId", objectInfo.objId);
                cmd += "; " + ExploreQueries.zoo2MainPhotoz.Replace("@objId", objectInfo.objId);
                cmd += "; " + ExploreQueries.zoo2MainPhotoz2.Replace("@objId", objectInfo.objId);
                cmd += "; " + ExploreQueries.zoo2Stripe82Normal.Replace("@objId", objectInfo.objId);
                cmd += "; " + ExploreQueries.zoo2Stripe82Normal2.Replace("@objId", objectInfo.objId);
                cmd += "; " + ExploreQueries.zoo2Stripe82Coadd1.Replace("@objId", objectInfo.objId);
                cmd += "; " + ExploreQueries.zoo2Stripe82Coadd1_2.Replace("@objId", objectInfo.objId);
                cmd += "; " + ExploreQueries.zoo2Stripe82Coadd2.Replace("@objId", objectInfo.objId);
                cmd += "; " + ExploreQueries.zoo2Stripe82Coadd2_2.Replace("@objId", objectInfo.objId);
                //all spectra
                cmd += "; " + ExploreQueries.AllSpec1.Replace("@objId", objectInfo.objId);
                cmd += "; " + ExploreQueries.AllSpec2.Replace("@objId", objectInfo.objId);
                ds.Merge(GetDataSetFromQuery(oConn, cmd));
            }
            else
            {
                for (int i = 0; i < TableNames1.Length; i++)
                    ds.Merge(new DataTable());
            }
            for (int i = 0; i < TableNames1.Length; i++)
                ds.Tables[i + 1].TableName = TableNames1[i];



            string[] TableNames2 = { "plate", "plateShow", "fitsParametersSppParams", "fitsParametersStellarMassStarformingPort", "fitsParameterSstellarMassPassivePort", "fitsParametersEmissionLinesPort", 
                "fitsParametersStellarMassPCAWiscBC03", "fitsParametersstellarMassPCAWiscM11", "fitsParametersStellarmassFSPSGranEarlyDust", "fitsParametersStellarmassFSPSGranEarlyNoDust", 
                "fitsParametersStellarmassFSPSGranWideDust", "fitsParametersStellarmassFSPSGranWideNoDust"};


            if (objectInfo.specId != null && !objectInfo.specId.Equals(""))
            {
                //plate
                cmd += "; " + ExploreQueries.Plate.Replace("@plateId", objectInfo.plateId.ToString());
                cmd += "; " + ExploreQueries.PlateShow.Replace("@plateId", objectInfo.plateId.ToString());
                //parameters
                cmd = ExploreQueries.fitsParametersSppParams.Replace("@specId", objectInfo.specId.ToString());
                cmd += "; " + ExploreQueries.fitsParametersStellarMassStarformingPort.Replace("@specId", objectInfo.specId.ToString());
                cmd += "; " + ExploreQueries.fitsParameterSstellarMassPassivePort.Replace("@specId", objectInfo.specId.ToString());
                cmd += "; " + ExploreQueries.fitsParametersEmissionLinesPort.Replace("@specId", objectInfo.specId.ToString());
                cmd += "; " + ExploreQueries.fitsParametersStellarMassPCAWiscBC03.Replace("@specId", objectInfo.specId.ToString());
                cmd += "; " + ExploreQueries.fitsParametersstellarMassPCAWiscM11.Replace("@specId", objectInfo.specId.ToString());
                cmd += "; " + ExploreQueries.fitsParametersStellarmassFSPSGranEarlyDust.Replace("@specId", objectInfo.specId.ToString());
                cmd += "; " + ExploreQueries.fitsParametersStellarmassFSPSGranEarlyNoDust.Replace("@specId", objectInfo.specId.ToString());
                cmd += "; " + ExploreQueries.fitsParametersStellarmassFSPSGranWideDust.Replace("@specId", objectInfo.specId.ToString());
                cmd += "; " + ExploreQueries.fitsParametersStellarmassFSPSGranWideNoDust.Replace("@specId", objectInfo.specId.ToString());

                ds.Merge(GetDataSetFromQuery(oConn, cmd));
            }
            else
            {
                for (int i = 0; i < TableNames2.Length; i++)
                    ds.Merge(new DataTable());
            }
            for (int i = TableNames1.Length; i < TableNames2.Length; i++)
                ds.Tables[i + 1].TableName = TableNames2[i];


            return ds;
        }


// the next pieces of code (till the end of the page) load the necessary info to resolve the object
        private void parseIds()
        {
            if (objectInfo.objId != null && !objectInfo.objId.Equals(""))
                objectInfo.id = Utilities.ParseId(objectInfo.objId);

            if (objectInfo.specObjId != null && !objectInfo.specObjId.Equals(""))
                objectInfo.specId = Utilities.ParseId(objectInfo.specObjId);

        }

        private void getObjPmts()
        {
            if (fiber.HasValue && plate.HasValue && mjd.HasValue) ObjIDFromPlfib(plate, mjd, fiber);
            else if (qra.HasValue && qdec.HasValue) pmtsFromEq(qra, qdec);
            else if (specId.HasValue || !String.IsNullOrEmpty(sidstring)) pmtsFromSpec(sidstring);
            else if (id.HasValue && !specId.HasValue) pmtsFromPhoto(id);
            else if (!id.HasValue && !specId.HasValue && (run.HasValue && rerun.HasValue && camcol.HasValue && field.HasValue && obj.HasValue)) pmtsFrom5PartSDSS(run, rerun, camcol, field, obj);
            else if (!String.IsNullOrEmpty(apid)) parseApogeeID(apid);
        }

        private void ObjIDFromPlfib(short? plate, int? mjd, short? fiber)
        {

            string cmd = ExploreQueries.getObjIDFromPlatefiberMjd;
            cmd = cmd.Replace("@mjd", mjd.ToString());
            cmd = cmd.Replace("@plate", plate.ToString());
            cmd = cmd.Replace("@fiberId", fiber.ToString());

            //DataSet ds = runQuery.RunDatabaseSearch(cmd, globals.ContentDataset, ClientIP, "Skyserver.Explore.Summary.getObjIDFromPlatefiberMjd");
            DataSet ds = GetDataSetFromQuery(oConn, cmd);
            using (DataTableReader reader = ds.Tables[0].CreateDataReader())
            {
                if (reader.Read())
                {
                    objectInfo.objId = reader["objId"] is DBNull ? null : Functions.BytesToHex((byte[])reader["objId"]);
                    objectInfo.specObjId = reader["specObjId"] is DBNull ? null : Functions.BytesToHex((byte[])reader["specObjId"]);
                    objectInfo.ra = (double)reader["ra"];
                    objectInfo.dec = (double)reader["dec"];
                }
            } // using DataTableReader

            cmd = ExploreQueries.getApogeeFromEq;
            cmd = cmd.Replace("@qra", objectInfo.ra.ToString());
            cmd = cmd.Replace("@qdec", objectInfo.dec.ToString());
            //cmd = cmd.Replace("@searchRadius", (0.5 / 60).ToString());
            cmd = cmd.Replace("@searchRadius", (KeyWords.EqSearchRadius).ToString());
            // if we couldn't find that plate/mjd/fiber, maybe it's an APOGEE object
            if (!String.IsNullOrEmpty(objectInfo.objId))
            {
                //ds = runQuery.RunCasjobs(cmd, "Explore: Summary");
                //ds = runQuery.RunDatabaseSearch(cmd, globals.ContentDataset, ClientIP, "Skyserver.Explore.Summary.getApogeeFromEq");
                ds = GetDataSetFromQuery(oConn, cmd);
                using (DataTableReader reader = ds.Tables[0].CreateDataReader())
                {
                    if (reader.Read())
                    {
                        objectInfo.apid = (string)reader["apstar_id"];
                    }
                } // using DataTableReader                
            }

        }

        private void apogeeFromEq(double? qra, double? qdec)
        {
            string cmd = ExploreQueries.getApogeeFromEq;
            cmd = cmd.Replace("@qra", qra.ToString());
            cmd = cmd.Replace("@qdec", qdec.ToString());
            cmd = cmd.Replace("@searchRadius", (KeyWords.EqSearchRadius).ToString());
            //cmd = cmd.Replace("@searchRadius", (0.5 / 60).ToString());
            //DataSet ds = runQuery.RunCasjobs(cmd,"Explore: Summary");
            //DataSet ds = runQuery.RunDatabaseSearch(cmd, globals.ContentDataset, ClientIP, "Skyserver.Explore.Summary.getApogeeFromEq");
            DataSet ds = GetDataSetFromQuery(oConn, cmd);
            using (DataTableReader reader = ds.Tables[0].CreateDataReader())
            {
                if (reader.Read())
                {
                    objectInfo.apid = (string)reader["apstar_id"];
                }
            }

        }


        private void photoFromEq(double? qra, double? qdec)
        {
            string cmd = ExploreQueries.getPhotoFromEq;
            cmd = cmd.Replace("@qra", qra.ToString());
            cmd = cmd.Replace("@qdec", qdec.ToString());
            cmd = cmd.Replace("@searchRadius", (KeyWords.EqSearchRadius).ToString());
            //cmd = cmd.Replace("@searchRadius", (0.5 / 60).ToString());
            //DataSet ds = runQuery.RunCasjobs(cmd,"Explore: Summary");
            //DataSet ds = runQuery.RunDatabaseSearch(cmd, globals.ContentDataset, ClientIP, "Skyserver.Explore.Summary.getPhotoFromEq");
            DataSet ds = GetDataSetFromQuery(oConn, cmd);
            using (DataTableReader reader = ds.Tables[0].CreateDataReader())
            {
                if (reader.Read())
                {
                    objectInfo.objId = reader["objId"] is DBNull ? null : Functions.BytesToHex((byte[])reader["objId"]);
                    objectInfo.specObjId = reader["specObjId"] is DBNull ? null : Functions.BytesToHex((byte[])reader["specObjId"]);
                }
            }
        }

        private void pmtsFromEq(double? qra, double? qdec)
        {
            string cmd = ExploreQueries.getpmtsFromEq;
            cmd = cmd.Replace("@qra", qra.ToString());
            cmd = cmd.Replace("@qdec", qdec.ToString());
            cmd = cmd.Replace("@searchRadius", (KeyWords.EqSearchRadius).ToString());
            //cmd = cmd.Replace("@searchRadius", (0.5 / 60).ToString());
            //DataSet ds = runQuery.RunCasjobs(cmd,"Explore: Summary");
            //DataSet ds = runQuery.RunDatabaseSearch(cmd, globals.ContentDataset, ClientIP, "Skyserver.Explore.Summary.getpmtsFromEq");
            DataSet ds = GetDataSetFromQuery(oConn, cmd);
            using (DataTableReader reader = ds.Tables[0].CreateDataReader())
            {
                if (reader.Read())
                {
                    objectInfo.objId = reader["objId"] is DBNull ? null : Functions.BytesToHex((byte[])reader["objId"]);
                    objectInfo.specObjId = reader["specObjId"] is DBNull ? null : Functions.BytesToHex((byte[])reader["specObjId"]);
                }
            }
            if (objectInfo.objId != null && !objectInfo.objId.Equals(""))
            {
                // This is required to get the primary specObjId (with sciprimary=1). PhotoTag.specObjId is not necessarily primary...
                pmtsFromPhoto(Utilities.ParseId(objectInfo.objId));
                apogeeFromEq(qra, qdec);
            }


        }


        private void pmtsFromSpec(string sidstring)
        {
            long? sidnumber = 0;

            // sidstring no longer stores apogee IDs, so there is no need to run pmtsFromSpecWithApogeeID(sidstring)
/*            try
            {
                pmtsFromSpecWithApogeeID(sidstring);
                if (objectInfo.apid != null && objectInfo.apid != string.Empty)
                {
                    photoFromEq(objectInfo.ra, objectInfo.dec);
                }
            }
            catch (Exception e) { }
 */
            try
            {
                sidnumber = Convert.ToInt64(sidstring);
                pmtsFromSpecWithSpecobjID(sidnumber);
                if (objectInfo.specObjId != null && objectInfo.specObjId != ZERO_ID)
                {
                    apogeeFromEq(objectInfo.ra, objectInfo.dec);
                }
            }
            catch (Exception e) { }
        }

        private void pmtsFromSpecWithApogeeID(string sid)
        {
            string whatdoiget = null;
            if (sid.StartsWith("apogee")) { whatdoiget = "apstar_id"; } else { whatdoiget = "apogee_id"; }

            string cmd = ExploreQueries.getpmtsFromSpecWithApogeeId;
            cmd = cmd.Replace("@whatdoiget", whatdoiget);
            cmd = cmd.Replace("@sid", "'" + sid + "'");

            //DataSet ds = runQuery.RunCasjobs(cmd, "Explore: Summary");
            //DataSet ds = runQuery.RunDatabaseSearch(cmd, globals.ContentDataset, ClientIP, "Skyserver.Explore.Summary.getpmtsFromSpecWithApogeeId");
            DataSet ds = GetDataSetFromQuery(oConn, cmd);
            using (DataTableReader reader = ds.Tables[0].CreateDataReader())
            {
                if (reader.Read())
                {
                    objectInfo.apid = reader.GetString(0);
                    objectInfo.ra = reader.GetDouble(1);
                    objectInfo.dec = reader.GetDouble(2);
                }
            } // using DataReader


        }

        private void pmtsFromSpecWithSpecobjID(long? sid)
        {
            string cmd = ExploreQueries.getpmtsFromSpecWithSpecobjID;
            cmd = cmd.Replace("@sid", sid.ToString());
            //DataSet ds = runQuery.RunCasjobs(cmd,"Explore: Summary");
            //DataSet ds = runQuery.RunDatabaseSearch(cmd, globals.ContentDataset, ClientIP, "Skyserver.Explore.Summary.getpmtsFromSpecWithSpecobjID");
            DataSet ds = GetDataSetFromQuery(oConn, cmd);
            using (DataTableReader reader = ds.Tables[0].CreateDataReader())
            {
                if (reader.Read())
                {
                    objectInfo.ra = (double)reader["ra"];
                    objectInfo.dec = (double)reader["dec"];
                    objectInfo.fieldId = reader["fieldId"] is DBNull ? null : Functions.BytesToHex((byte[])reader["fieldId"]);
                    objectInfo.objId = reader["objId"] is DBNull ? null : Functions.BytesToHex((byte[])reader["objId"]);
                    objectInfo.specObjId = reader["specObjId"] is DBNull ? null : Functions.BytesToHex((byte[])reader["specObjId"]);
                    objectInfo.plateId = reader["plateId"] is DBNull ? null : Utilities.ParseId(Functions.BytesToHex((byte[])reader["plateId"]));
                    objectInfo.mjd = (int)reader["mjd"];
                    objectInfo.fiberId = (short)reader["fiberId"];
                    objectInfo.plate = (short)reader["plate"];
                }
            } // using DataReader

        }


        private void pmtsFromPhoto(long? id)
        {

            string cmd = ExploreQueries.getpmtsFromPhoto;
            cmd = cmd.Replace("@objid", id.ToString());

            //DataSet ds = runQuery.RunCasjobs(cmd,"Explore: Summary");
            //DataSet ds = runQuery.RunDatabaseSearch(cmd, globals.ContentDataset, ClientIP, "Skyserver.Explore.Summary.getpmtsFromPhoto");
            DataSet ds = GetDataSetFromQuery(oConn, cmd);
            using (DataTableReader reader = ds.Tables[0].CreateDataReader())
            {
                if (reader.Read())
                {
                    objectInfo.ra = (double)reader["ra"];
                    objectInfo.dec = (double)reader["dec"];
                    objectInfo.run = (short)reader["run"];
                    objectInfo.rerun = (short)reader["rerun"];
                    objectInfo.camcol = (byte)reader["camcol"];
                    objectInfo.field = (short)reader["field"];
                    objectInfo.fieldId = reader["fieldId"] is DBNull ? null : Functions.BytesToHex((byte[])reader["fieldId"]);
                    objectInfo.objId = reader["objId"] is DBNull ? null : Functions.BytesToHex((byte[])reader["objId"]);
                    objectInfo.specObjId = reader["specObjId"] is DBNull ? null : Functions.BytesToHex((byte[])reader["specObjId"]);

                }
            }

            // get the plateId and fiberId from the specObj, if it exists
            if (objectInfo.specObjId != null && !ZERO_ID.Equals(objectInfo.specObjId))
            {
                long specId = long.Parse(objectInfo.specObjId.Substring(2), NumberStyles.AllowHexSpecifier);
                cmd = ExploreQueries.getPlateFiberFromSpecObj;
                cmd = cmd.Replace("@specId", specId.ToString());

                //ds = runQuery.RunCasjobs(cmd,"Explore: Summary");
                //ds = runQuery.RunDatabaseSearch(cmd, globals.ContentDataset, ClientIP, "Skyserver.Explore.Summary.getPlateFiberFromSpecObj");
                ds = GetDataSetFromQuery(oConn, cmd);
                using (DataTableReader reader = ds.Tables[0].CreateDataReader())
                {
                    if (reader.Read())
                    {
                        objectInfo.plateId = reader["plateId"] is DBNull ? null : Utilities.ParseId(Functions.BytesToHex((byte[])reader["plateId"]));
                        objectInfo.mjd = (int)reader["mjd"];
                        objectInfo.fiberId = (short)reader["fiberId"];
                        objectInfo.plate = (short)reader["plate"];
                    }
                } // using DataReader
            }

            try
            {
                apogeeFromEq(objectInfo.ra, objectInfo.dec);
            }
            catch { }

        }


        private void pmtsFrom5PartSDSS(Int16? Run, Int16? Rerun, byte? Camcol, Int16? Field, Int16? Obj)
        {


            string Skyversion = "";
            string cmd = ExploreQueries.getSkyversion;
            //DataSet ds = runQuery.RunDatabaseSearch(cmd, globals.ContentDataset, ClientIP, "Skyserver.Explore.Summary.getSkyversion");
            DataSet ds = GetDataSetFromQuery(oConn, cmd);
            using (DataTableReader reader = ds.Tables[0].CreateDataReader())
            {
                if (reader.Read())
                {
                    Skyversion = reader["skyversion"] is DBNull ? Skyversion : reader["skyversion"].ToString();
                }
            }


            cmd = ExploreQueries.getpmtsFrom5PartSDSS;
            cmd = cmd.Replace("@skyversion", Skyversion);
            cmd = cmd.Replace("@run", Run == null ? "null" : Run.ToString());
            cmd = cmd.Replace("@rerun", Rerun == null ? "null" : Rerun.ToString());
            cmd = cmd.Replace("@camcol", Camcol == null ? "null" : Camcol.ToString());
            cmd = cmd.Replace("@field", Field == null ? "null" : Field.ToString());
            cmd = cmd.Replace("@obj", Obj == null ? "null" : Obj.ToString());

            //DataSet ds = runQuery.RunCasjobs(cmd,"Explore: Summary");
            //ds = runQuery.RunDatabaseSearch(cmd, globals.ContentDataset, ClientIP, "Skyserver.Explore.Summary.getpmtsFrom5PartSDSS");
            ds = GetDataSetFromQuery(oConn, cmd);
            using (DataTableReader reader = ds.Tables[0].CreateDataReader())
            {
                if (reader.Read())
                {
                    objectInfo.ra = (double)reader["ra"];
                    objectInfo.dec = (double)reader["dec"];
                    objectInfo.run = (short)reader["run"];
                    objectInfo.rerun = (short)reader["rerun"];
                    objectInfo.camcol = (byte)reader["camcol"];
                    objectInfo.field = (short)reader["field"];
                    objectInfo.fieldId = reader["fieldId"] is DBNull ? null : Functions.BytesToHex((byte[])reader["fieldId"]);
                    objectInfo.objId = reader["objId"] is DBNull ? null : Functions.BytesToHex((byte[])reader["objId"]);
                    objectInfo.specObjId = reader["specObjId"] is DBNull ? null : Functions.BytesToHex((byte[])reader["specObjId"]);

                }
            }

            // get the plateId and fiberId from the specObj, if it exists
            if (objectInfo.specObjId != null && !ZERO_ID.Equals(objectInfo.specObjId))
            {
                long specId = long.Parse(objectInfo.specObjId.Substring(2), NumberStyles.AllowHexSpecifier);
                cmd = ExploreQueries.getPlateFiberFromSpecObj;
                cmd = cmd.Replace("@specId", specId.ToString());

                //ds = runQuery.RunCasjobs(cmd,"Explore: Summary");
                //ds = runQuery.RunDatabaseSearch(cmd, globals.ContentDataset, ClientIP, "Skyserver.Explore.Summary.getPlateFiberFromSpecObj");
                ds = GetDataSetFromQuery(oConn, cmd);
                using (DataTableReader reader = ds.Tables[0].CreateDataReader())
                {
                    if (reader.Read())
                    {
                        objectInfo.plateId = reader["plateId"] is DBNull ? null : Utilities.ParseId(Functions.BytesToHex((byte[])reader["plateId"]));
                        objectInfo.mjd = (int)reader["mjd"];
                        objectInfo.fiberId = (short)reader["fiberId"];
                        objectInfo.plate = (short)reader["plate"];
                    }
                } // using DataReader
            }

            try
            {
                apogeeFromEq(objectInfo.ra, objectInfo.dec);
            }
            catch { }


        }


        private void parseApogeeID(string idstring)
        {
            double qra = 0, qdec = 0;
            objectInfo.apid = apid;
            string cmd = "";
            apid = apid.ToLower();
            string taskname = "";
            /*
            if (apid.Contains("apogee"))
            {
                cmd = ExploreQueries.getApogee; taskname = "Skyserver.Explore.Summary.getApogee";
            }
            else
            {
                cmd = ExploreQueries.getApogee2; taskname = "Skyserver.Explore.Summary.getApogee2";
            }
            */ 
            if (apid.StartsWith("apogee"))
            {
                cmd = ExploreQueries.getApogee; taskname = "Skyserver.Explore.Summary.getApogee";
            }
            else if ((apid.StartsWith("2m")))
            {
                cmd = ExploreQueries.getApogee2; taskname = "Skyserver.Explore.Summary.getApogee2";
            }
            else { cmd = ""; }


            ParameterValuePairs.Clear(); ParameterValuePairs.Add("@apogeeId", apid);
            //cmd = cmd.Replace("@apogeeId", apid);

            //DataSet ds = runQuery.RunCasjobs(cmd,"Explore: Summary");
            //DataSet ds = runQuery.RunDatabaseSearch(cmd, globals.ContentDataset, ClientIP, taskname);
            DataSet ds = GetDataSetFromQuery(oConn, cmd, ParameterValuePairs);
            using (DataTableReader reader = ds.Tables[0].CreateDataReader())
            {
                if (reader.Read())
                {
                    qra = (double)reader["ra"];
                    qdec = (double)reader["dec"];

                }
            }
            cmd = ExploreQueries.getpmtsFromEq;
            cmd = cmd.Replace("@qra", qra.ToString());
            cmd = cmd.Replace("@qdec", qdec.ToString());
            cmd = cmd.Replace("@searchRadius", (KeyWords.EqSearchRadius).ToString());
            //cmd = cmd.Replace("@searchRadius", (0.5/60).ToString());

            //ds = runQuery.RunCasjobs(cmd,"Explore: Summary");
            //ds = runQuery.RunDatabaseSearch(cmd, globals.ContentDataset, ClientIP, "Skyserver.Explore.Summary.getpmtsFromEq");
            ds = GetDataSetFromQuery(oConn, cmd);
            using (DataTableReader reader = ds.Tables[0].CreateDataReader())
            {
                if (reader.Read())
                {
                    objectInfo.objId = reader["objId"] is DBNull ? null : Functions.BytesToHex((byte[])reader["objId"]);
                    objectInfo.specObjId = reader["specObjId"] is DBNull ? null : Functions.BytesToHex((byte[])reader["specObjId"]);
                }
            }

        }

        public DataSet SetTableResult(string QueryName)
        {
            DataSet ds = new DataSet();
            string cmd = "";
            Dictionary<string, string> ParameterValuePairs = new Dictionary<string,string>(); 


            switch (QueryName)
            {
                    //matches
                case "matches1":
                    cmd = ExploreQueries.matches1.Replace("@objId", objectInfo.objId);break;
                case "matches2":
                    cmd = ExploreQueries.matches2.Replace("@objId", objectInfo.objId);break;
                //neighbors
                case "neighbors1":
                    cmd = ExploreQueries.neighbors1.Replace("@objId", objectInfo.objId);break;
                case "neighbors2":
                    cmd += "; " + ExploreQueries.neighbors2.Replace("@objId", objectInfo.objId);break;
                    // galaxy zoo
                case "zooSpec":
                    cmd = ExploreQueries.zooSpec.Replace("@objId", objectInfo.objId); break;
                case "zooSpec1":
                    cmd = ExploreQueries.zooSpec1.Replace("@objId", objectInfo.objId);break;
                case "zooSpec2":
                    cmd = ExploreQueries.zooSpec2.Replace("@objId", objectInfo.objId);break;
                case "zooNoSpec":
                    cmd = ExploreQueries.zooNoSpec.Replace("@objId", objectInfo.objId);break;
                case "zooConfidence":
                    cmd = ExploreQueries.zooConfidence.Replace("@objId", objectInfo.objId);break;
                case "zooConfidence2":
                    cmd = ExploreQueries.zooConfidence2.Replace("@objId", objectInfo.objId);break;
                case "zooMirrorBias":
                    cmd = ExploreQueries.zooMirrorBias.Replace("@objId", objectInfo.objId);break;                
                case "zooMirrorBias2":
                    cmd = ExploreQueries.zooMirrorBias2.Replace("@objId", objectInfo.objId);break;
                case "zooMonochromeBias":
                    cmd = ExploreQueries.zooMonochromeBias.Replace("@objId", objectInfo.objId);break;
                case "zooMonochromeBias2":
                    cmd = ExploreQueries.zooMonochromeBias2.Replace("@objId", objectInfo.objId);break;
                case "zoo2MainSpecz":
                    cmd = ExploreQueries.zoo2MainSpecz.Replace("@objId", objectInfo.objId);break;
                case "zoo2MainSpecz2":
                    cmd = ExploreQueries.zoo2MainSpecz2.Replace("@objId", objectInfo.objId);break;
                case "zoo2MainPhotoz":
                    cmd = ExploreQueries.zoo2MainPhotoz.Replace("@objId", objectInfo.objId);break;
                case "zoo2MainPhotoz2":
                    cmd = ExploreQueries.zoo2MainPhotoz2.Replace("@objId", objectInfo.objId);break;
                case "zoo2Stripe82Normal":
                    cmd = ExploreQueries.zoo2Stripe82Normal.Replace("@objId", objectInfo.objId);break;
                case "zoo2Stripe82Normal2":
                    cmd = ExploreQueries.zoo2Stripe82Normal2.Replace("@objId", objectInfo.objId);break;
                case "zoo2Stripe82Coadd1":
                    cmd = ExploreQueries.zoo2Stripe82Coadd1.Replace("@objId", objectInfo.objId);break;
                case "zoo2Stripe82Coadd1_2":
                    cmd = ExploreQueries.zoo2Stripe82Coadd1_2.Replace("@objId", objectInfo.objId);break;
                case "zoo2Stripe82Coadd2":
                    cmd = ExploreQueries.zoo2Stripe82Coadd2.Replace("@objId", objectInfo.objId);break;
                case "zoo2Stripe82Coadd2_2":
                    cmd = ExploreQueries.zoo2Stripe82Coadd2_2.Replace("@objId", objectInfo.objId);break;
                //all spectra
                case "AllSpec1":
                    cmd = ExploreQueries.AllSpec1.Replace("@objId", objectInfo.objId);break;
                case "AllSpec2":
                    cmd = ExploreQueries.AllSpec2.Replace("@objId", objectInfo.objId);break;
                //plate
                case "Plate":
                    cmd = ExploreQueries.Plate.Replace("@plateId", objectInfo.plateId.ToString());break;
                case "PlateShow":
                    cmd = ExploreQueries.PlateShow.Replace("@plateId", objectInfo.plateId.ToString());break;
                case "PlatePlusShow":
                    cmd = ExploreQueries.Plate.Replace("@plateId", objectInfo.plateId.ToString()) + ";" + ExploreQueries.PlateShow.Replace("@plateId", objectInfo.plateId.ToString());break;
                //parameters
                case "fitsParametersSppParams":
                    cmd = ExploreQueries.fitsParametersSppParams.Replace("@specId", objectInfo.specId.ToString());break;
                case "fitsParametersStellarMassStarformingPort":
                    cmd = ExploreQueries.fitsParametersStellarMassStarformingPort.Replace("@specId", specId.ToString());break;
                case "fitsParameterSstellarMassPassivePort":
                    cmd = ExploreQueries.fitsParameterSstellarMassPassivePort.Replace("@specId", specId.ToString());break;
                case "fitsParametersEmissionLinesPort":
                    cmd = ExploreQueries.fitsParametersEmissionLinesPort.Replace("@specId", specId.ToString());break;
                case "fitsParametersStellarMassPCAWiscBC03":
                    cmd = ExploreQueries.fitsParametersStellarMassPCAWiscBC03.Replace("@specId", specId.ToString());break;
                case "fitsParametersstellarMassPCAWiscM11":
                    cmd = ExploreQueries.fitsParametersstellarMassPCAWiscM11.Replace("@specId", specId.ToString());break;
                case "fitsParametersStellarmassFSPSGranEarlyDust":
                    cmd = ExploreQueries.fitsParametersStellarmassFSPSGranEarlyDust.Replace("@specId", specId.ToString());break;
                case "fitsParametersStellarmassFSPSGranEarlyNoDust":
                    cmd = ExploreQueries.fitsParametersStellarmassFSPSGranEarlyNoDust.Replace("@specId", specId.ToString());break;
                case "fitsParametersStellarmassFSPSGranWideDust":
                    cmd = ExploreQueries.fitsParametersStellarmassFSPSGranWideDust.Replace("@specId", specId.ToString());break;
                case "fitsParametersStellarmassFSPSGranWideNoDust":
                    cmd = ExploreQueries.fitsParametersStellarmassFSPSGranWideNoDust.Replace("@specId", specId.ToString());break;
                    //
                case "PhotoObjQuery":
                    cmd = ExploreQueries.PhotoObjQuery.Replace("@objId", objectInfo.objId);break;
                case "PhotoTagQuery":
                    cmd = ExploreQueries.PhotoTagQuery.Replace("@objId", objectInfo.objId);break;
                case "PhotoZ":
                    cmd = ExploreQueries.PhotoZ.Replace("@objId", objectInfo.objId);break;
                case "FieldQuery":
                    cmd = ExploreQueries.FieldQuery.Replace("@fieldId", objectInfo.fieldId);break;
                case "FrameQuery":
                    cmd = ExploreQueries.FrameQuery.Replace("@fieldId", objectInfo.fieldId);break;
                    //
                case "SpecObjQuery":
                    cmd = ExploreQueries.SpecObjQuery.Replace("@specId", objectInfo.specId.ToString());break;
                case "sppLinesQuery":
                    cmd = ExploreQueries.sppLinesQuery.Replace("@specId", objectInfo.specId.ToString());break;
                case "sppParamsQuery":
                    cmd = ExploreQueries.sppParamsQuery.Replace("@specId", objectInfo.specId.ToString());break;
                case "galSpecLineQuery":
                    cmd = ExploreQueries.galSpecLineQuery.Replace("@specId", objectInfo.specId.ToString());break;
                case "galSpecIndexQuery":
                    cmd = ExploreQueries.galSpecIndexQuery.Replace("@specId", objectInfo.specId.ToString());break;
                case "galSpecInfoQuery":
                    cmd = ExploreQueries.galSpecInfoQuery.Replace("@specId", objectInfo.specId.ToString());break;
                case "stellarMassStarformingPortQuery":
                    cmd = ExploreQueries.stellarMassStarformingPortQuery.Replace("@specId", objectInfo.specId.ToString());break;
                case "stellarMassPassivePortQuery":
                    cmd = ExploreQueries.stellarMassPassivePortQuery.Replace("@specId", objectInfo.specId.ToString());break;
                case "emissionLinesPortQuery":
                    cmd = ExploreQueries.emissionLinesPortQuery.Replace("@specId", objectInfo.specId.ToString());break;
                case "stellarMassPCAWiscBC03Query":
                    cmd = ExploreQueries.stellarMassPCAWiscBC03Query.Replace("@specId", objectInfo.specId.ToString());break;
                case "stellarMassPCAWiscM11Query":
                    cmd = ExploreQueries.stellarMassPCAWiscM11Query.Replace("@specId", objectInfo.specId.ToString());break;
                case "stellarMassFSPSGranEarlyDust":
                    cmd = ExploreQueries.stellarMassFSPSGranEarlyDust.Replace("@specId", objectInfo.specId.ToString());break;
                case "stellarMassFSPSGranEarlyNoDust":
                    cmd = ExploreQueries.stellarMassFSPSGranEarlyNoDust.Replace("@specId", objectInfo.specId.ToString());break;
                case "stellarMassFSPSGranWideDust":
                    cmd = ExploreQueries.stellarMassFSPSGranWideDust.Replace("@specId", objectInfo.specId.ToString());break;
                case "stellarMassFSPSGranWideNoDust":
                    cmd = ExploreQueries.stellarMassFSPSGranWideNoDust.Replace("@specId", objectInfo.specId.ToString());break;
                case "apogeeStar":
                    cmd = ExploreQueries.apogeeStar; ParameterValuePairs.Clear();ParameterValuePairs.Add("@apid", objectInfo.apid);  break;
                case "aspcapStar":
                    cmd = ExploreQueries.aspcapStar; ParameterValuePairs.Clear();ParameterValuePairs.Add("@apid", objectInfo.apid);  break;
                case "fitsimg":
                    cmd = ExploreQueries.fitsimg.Replace("@fieldId", objectInfo.fieldId.ToString());break;
                case "fitsspec":
                    cmd = ExploreQueries.fitsspec.Replace("@specObjId", objectInfo.specObjId.ToString()); break;
                case "SpecById":
                    cmd = SpecQueries.SpecById.Replace("@specId", objectInfo.specObjId.ToString()); break;
                case "SpecByPF":
                    cmd = SpecQueries.SpecByPF.Replace("@plateId", objectInfo.plateId.ToString()).Replace("@fiberId", objectInfo.fiberId.ToString()); break;
                case "FrameById":
                    cmd = FrameQueries.FrameById.Replace("@fieldId", objectInfo.fieldId).Replace("@zoom", objectInfo.zoom.ToString()); break;
                case "FiberList":
                    cmd = PlateQueries.FiberList.Replace("@plateid", objectInfo.plateId.ToString());break;
                case "PlateAPOGEE":
                    cmd = PlateQueries.PlateAPOGEE; ParameterValuePairs.Clear();ParameterValuePairs.Add("@apogeeplateid", objectInfo.plateIdApogee.ToString()); break;//cmd = PlateQueries.PlateAPOGEE.Replace("@apogeeplateid", objectInfo.plateIdApogee.ToString()); break;
                default:
                    cmd = "";break;
            }
            ds.Merge(GetDataSetFromQuery(oConn, cmd, ParameterValuePairs));
            return ds;
        }


        private void resolveName(string name)
        {
            string script = "output console=off script=off\nformat object \"%D,%MAIN_ID,%COO(d;A,D)\"\n" + name;
            WebClient client = new WebClient();
            string URL = KeyWords.NameResolverURL + HttpUtility.UrlEncode(script);
            Stream data = client.OpenRead(URL);
            StreamReader reader = new StreamReader(data);
            string s = reader.ReadToEnd();
            if (!s.StartsWith("1,"))
            {
                throw new Exception("Name could not be resolved.");
            }
            string[] parts = s.Split(new char[] { ',', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            double ra = 0.0;
            if (double.TryParse(parts[2], out ra))
            {
                this.ResolvedName = parts[1];
                this.qra = Utilities.parseRA(parts[2]);
                this.qdec = Utilities.parseRA(parts[3]);
                this.objectInfo.name = this.ResolvedName;
            }
            else
            {
                throw new Exception("Name was resolved, but coordinates are undefined.");
            }
        }




    }
}