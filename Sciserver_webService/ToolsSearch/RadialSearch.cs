using System;
using System.Collections.Generic;
using Sciserver_webService.Common;
using System.Text.RegularExpressions;

namespace Sciserver_webService.ToolsSearch
{
    public class RadialSearch
    {
        public String query = "";
        public String imageQuery = "";
        public String irQuery = "";
        String skyserverUrl = "";
        int datarelease = 1; // SDSS data release number
        public string fp = "";
        Dictionary<string, string> requestDir = null;
        public string QueryForUserDisplay = "";
        public string WhichPhotometry = "";

        public string ClientIP = "";
        public string TaskName = "";
        public string server_name = "";
        public string windows_name = "";

        public RadialSearch(Dictionary<string, string> requestDir, Dictionary<string, string> ExtraInfo)
        {
            try
            {
                ClientIP = ExtraInfo["ClientIP"];
                TaskName = ExtraInfo["TaskName"];
                server_name = ExtraInfo["server_name"];
                windows_name = ExtraInfo["windows_name"];
            }
            catch (Exception e) { throw new Exception(e.Message); };

            this.requestDir = requestDir;
            Validation val = new Validation(requestDir, "radialSearch");
            skyserverUrl = requestDir["skyserverUrl"];
            datarelease =Convert.ToInt32( requestDir["datarelease"]);
            skyserverUrl = skyserverUrl + "DR" + datarelease.ToString();
            try { WhichPhotometry = requestDir["whichphotometry"]; } catch { }

            Int64 limit;
            try
            {
                limit = Convert.ToInt64(requestDir["limit"]);
            }
            catch { throw (new Exception("Invalid numerical value for maximum number of rows in LIMIT=" + requestDir["limit"])); }
            if (limit > Convert.ToInt64(KeyWords.MaxRows))
            {
                throw (new Exception("Numerical value for maximum number of rows is out of range in LIMIT=" + requestDir["limit"] + ". Maximum number of rows allowed is " + Convert.ToInt64(KeyWords.MaxRows) + "."));
            }


            bool temp = val.ValidateOtherParameters(val.uband_s, val.gband_s, val.rband_s, val.iband_s, val.zband_s, val.jband_s, val.hband_s, val.kband_s, val.coordtype, val.returntype_s, val.limit_s);
            fp = val.fp;
            if (val.fp != "only")// Want to just run the query, and do not want to know if RA,DEC,Radius fall inside footprint
            {
                if (datarelease > 9 && WhichPhotometry == "infrared")
                {
                    query = this.buildIRQuery(val);//sets also the QueryForUserDisplay
                }
                else
                {
                    query = this.buildImageQuery(val);//sets also the QueryForUserDisplay
                }
            }
            else //if only want to know if RA,DEC,Radius fall inside footprint
            {
                query = this.buildFootPrintQuery(val);
            }
            string c = this.query;
            string c2 = Regex.Replace(c, @"\/\*(.*\n)*\*\/", "");	                        // remove all multi-line comments
            c2 = Regex.Replace(c2, @"^[ \t\f\v]*--.*\r\n", "", RegexOptions.Multiline);		// remove all isolated single-line comments
            c2 = Regex.Replace(c2, @"--[^\r^\n]*", "");				                        // remove all embedded single-line comments
            c2 = Regex.Replace(c2, @"[ \t\f\v]+", " ");                      				// replace multiple whitespace with single space
            c2 = Regex.Replace(c2, @"^[ \t\f\v]*\r\n", "", RegexOptions.Multiline);			// remove empty lines
            c2 = c2.Replace("'", "''");
            query = "EXEC spExecuteSQL '" + c2 + "','" + KeyWords.MaxRows + "','" + server_name + "','" + windows_name + "','" + ClientIP + "','" + TaskName + "',@filter=1,@log=1";

            //if (val.whichquery.Equals("imaging"))
            //{
            //  this.buildImageQuery(val);
            //  query = imageQuery;
            //}
            //else
            //{
            //    this.buildIRQuery(val);
            //    query = irQuery;
            //}
        }



        //private void SetRadialArea(HttpRequest request)
        //{
        //    double[] result = new double[3];

        //    double ra = Utilities.parseRA(request["ra"]);
        //    double dec = Utilities.parseDec(request["dec"]);
        //    double radius = double.Parse(request["radius"]);

        //    string whichway = request["whichway"];

        //    if (whichway == "galactic")
        //    {
        //        double newra = Utilities.glon2ra(ra, dec);
        //        double newdec = Utilities.glat2dec(ra, dec);
        //        ra = newra;
        //        dec = newdec;
        //    }

        //    this.ra = ra;
        //    this.dec = dec;
        //    this.radius = radius;
        //}

        private string buildFootPrintQuery(Validation val)
        {
            string sql = "";
            string SQLCheckFoot = "IF EXISTS(select top 1 * FROM dbo.fFootprintEq(" + val.ra + "," + val.dec + "," + val.radius + ")) ";
            if (val.format == "html")
                sql = SQLCheckFoot + " SELECT '<font size=\"large\" color=\"green\">The area you requested (RA=" + val.ra + ", dec=" + val.dec + ", radius=" + val.radius + ") overlaps the SDSS " + KeyWords.DataRelease + " survey area.</font>' as 'SDSS Footprint Check ' ELSE SELECT '<font size=\"large\" color =\"red\">Sorry, the area you requested (RA=" + val.ra + ", dec=" + val.dec + ", radius=" + val.radius + ") is outside the SDSS " + KeyWords.DataRelease + " survey area.</font>' AS 'SDSS Footprint Check '";
            else
                sql = SQLCheckFoot + " SELECT 'The area you requested (RA=" + val.ra + ", dec=" + val.dec + ", radius=" + val.radius + ") overlaps the SDSS " + KeyWords.DataRelease + " survey area.' as 'SDSS Footprint Check ' ELSE SELECT 'Sorry, the area you requested (RA=" + val.ra + ", dec=" + val.dec + ", radius=" + val.radius + ") is outside the SDSS " + KeyWords.DataRelease + " survey area.' as 'SDSS Footprint Check '";
            return sql;
        }

        
        private string buildImageQuery(Validation val)
        {

            string sql = ""; 
            string limit = ( Int64.Parse(val.limit_s)  <= 0) ? KeyWords.MaxRows : (val.limit_s).ToString();

            sql = "SELECT "; 
            sql += " TOP " + limit;
            this.QueryForUserDisplay = sql + " p.objid,\n";

            if (val.format == "html")
            {
                sql += " '<a target=INFO href=" + skyserverUrl + "/en/tools/explore/summary.aspx?id=' + cast(p.objId as varchar(20)) +'>' + cast(p.objId as varchar(20)) + '</a>' as objID,\n";
            }
            else
            {
                sql += " p.objid,\n";
            }

            string sql2 = ""; 
            sql2 += "   p.run, p.rerun, p.camcol, p.field, p.obj,\n";
            sql2 += "   p.type, p.ra, p.dec, p.u,p.g,p.r,p.i,p.z,\n";
            sql2 += "   p.Err_u, p.Err_g, p.Err_r,p.Err_i,p.Err_z\n";
            sql2 += "   FROM fGetNearbyObjEq(" + val.ra + "," + val.dec + "," + val.radius + ") n,";
            sql2 += "   PhotoPrimary p\n";
            sql2 += "   WHERE n.objID=p.objID ";
            sql2 += addWhereClause(val);

            this.imageQuery = sql + sql2; 
            this.QueryForUserDisplay += sql2;
            
            return this.imageQuery;
        }

        private string buildIRQuery(Validation val)
        {
            string sql = "";
            string limit = (Int64.Parse(val.limit_s) <= 0) ? KeyWords.MaxRows : (val.limit_s).ToString();

            sql = "SELECT ";
            sql += " TOP " + limit;
            this.QueryForUserDisplay = sql + " p.apstar_id, \n";

            if (val.format == "html")
            {
                sql += " '<a target=INFO href=" + skyserverUrl + "/en/tools/explore/summary.aspx?apid=' + cast(p.apstar_id as varchar(100)) + '>' + cast(p.apstar_id as varchar(100)) + '</a>' as apstar_id,\n";
            }
            else
            {
                sql += " p.apstar_id, \n";
            }

            string sql2 = "";
            sql2 += "   p.apogee_id,p.ra, p.dec, p.glon, p.glat,\n";
            sql2 += "   p.vhelio_avg,p.vscatter,\n";
            sql2 += "   a.teff,a.logg,\n";
            if(datarelease >= 12) sql2 += " a.param_m_h \n";
            else sql2 += " a.metals\n";
            sql2 += "   FROM apogeeStar p\n";
            sql2 += "   JOIN fGetNearbyApogeeStarEq(" + val.ra + "," + val.dec + "," + val.radius + ") n on p.apstar_id=n.apstar_id\n";
            sql2 += "   JOIN aspcapStar a on a.apstar_id = p.apstar_id\n";
            sql2 += "   JOIN apogeeObject as o ON a.apogee_id=o.apogee_id"; 
            sql2 += addInfraredWhereClause(val);

            this.irQuery = sql + sql2;
            this.QueryForUserDisplay += sql2;
            return this.irQuery;
        }

        private string addWhereClause(Validation val)
        {
            string queryString = ""; 
            if (val.uband) { queryString += " AND p.u between " + val.umin + " AND " + val.umax; }
            if (val.gband) { queryString += " AND p.g between " + val.gmin + " AND " + val.gmax; }
            if (val.iband) { queryString += " AND p.i between " + val.imin + " AND " + val.imax; }
            if (val.rband) { queryString += " AND p.r between " + val.rmin + " AND " + val.rmax; }
            if (val.zband) { queryString += " AND p.z between " + val.zmin + " AND " + val.zmax; }

            return queryString;
        }

        private string addInfraredWhereClause(Validation val)
        {
            string queryString = ""; 
            if (val.jband) { queryString += " AND o.j between " + val.jmin + " AND " + val.jmax; }
            if (val.hband) { queryString += " AND o.h between " + val.hmin + " AND " + val.hmax; }
            if (val.kband) { queryString += " AND o.k between " + val.kmin + " AND " + val.kmax; }

            return queryString;
        }

        //public string getJson(string[] resultContent, string[] query)
        //{
        //    string[] lines = resultContent[0].Split('\n');
        //    var csv = new List<string[]>(); // or, List<YourClass>            
        //    foreach (string line in lines)
        //        csv.Add(line.Split(',')); // or, populate YourClass          
        //    //string json = new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(csv);

        //    string[] lines2 = resultContent[1].Split('\n');
        //    var csv2 = new List<string[]>(); // or, List<YourClass>            
        //    foreach (string line2 in lines2)
        //        csv2.Add(line2.Split(',')); // or, populate YourClass          

        //    string[] querytype = new string[2] { "Imaging", "IR Spectra" };

        //    StringBuilder sb = new StringBuilder();
        //    StringWriter sw = new StringWriter(sb);

        //    using (JsonWriter writer = new JsonTextWriter(sw))
        //    {
        //        writer.Formatting = Formatting.Indented;
        //        writer.WriteStartArray();

        //        for (int r = 0; r < resultContent.Length; r++)
        //        {
        //            writer.WriteStartObject();
        //            writer.WritePropertyName("Query");
        //            writer.WriteValue(query[r]);
        //            writer.WritePropertyName("Query type");
        //            writer.WriteValue(querytype[r]);
        //            writer.WritePropertyName("Query Results:");
        //            writer.WriteStartArray();
        //            if (r == 0)
        //            {
        //                for (int i = 1; i < lines.Length; i++)
        //                {
        //                    writer.WriteStartObject();
        //                    for (int Index = 0; Index < csv[0].Length; Index++)
        //                    {
        //                        writer.WritePropertyName(csv[0][Index]);
        //                        writer.WriteValue(csv[i][Index]);
        //                    }
        //                    writer.WriteEndObject();
        //                }
        //            }
        //            else
        //            {
        //                for (int i = 1; i < lines2.Length; i++)
        //                {
        //                    writer.WriteStartObject();
        //                    for (int Index = 0; Index < csv2[0].Length; Index++)
        //                    {
        //                        writer.WritePropertyName(csv2[0][Index]);
        //                        writer.WriteValue(csv2[i][Index]);
        //                    }
        //                    writer.WriteEndObject();
        //                }
        //            }
        //            writer.WriteEndArray();
        //            writer.WriteEndObject();
        //        }
        //        writer.WriteEndArray();
        //    }
        //    return sb.ToString();
        //}

        //public String getResult(String token, String casjobsMessage, String format)
        //{
        //    try
        //    {
        //        //RunCasjobs run = new RunCasjobs();
        //        //String imageQueryResult = run.postCasjobs(this.imageQuery, token, casjobsMessage).Content.ReadAsStringAsync().Result;
        //        //String irQueryResult = run.postCasjobs(this.irQuery, token, casjobsMessage).Content.ReadAsStringAsync().Result;
        //        //String results = "";
        //        //if (format.Equals("json"))
        //        //    results = this.getJson(new String[2] { imageQueryResult, irQueryResult }, new String[2] { this.imageQuery, this.irQuery });
        //        //else
        //        //{
        //        //    results = "# Imaging Query:" + this.imageQuery.Replace("\n", "") + "\n\n" + imageQueryResult;
        //        //    results += "\n\n#IR Spectra Query:" + this.irQuery.Replace("\n", "") + "\n\n" + irQueryResult;
        //        //}
        //        //return results;

        //        return "";

        //    }
        //    catch (Exception e)
        //    {
        //        throw new Exception("Error while running casjobs:" + e.Message);
        //    }
        //}

        
        /// <summary>
        ///  this is using old casjobs
        /// </summary>
        /// <param name="finalQuery"></param>
        /// <returns></returns>
        //private string runQuery(string finalQuery)
        //{

        //    JobsSoapClient cjobs = new JobsSoapClient();
        //    DataSet ds = cjobs.ExecuteQuickJobDS(CJobsWSID, CJobsPasswd, finalQuery, CJobsTARGET, "FOR RadialSearch", false);
        //    return JsonConvert.SerializeObject(ds, Formatting.Indented);
        //}

        //public string getData(String ra, String dec, String sr,
        //                     String uband, String gband, String rband, String iband, String zband,
        //                     String searchtype, String returntype, String limit)
        //{
        //    try
        //    {
        //        Validation val = new Validation();
        //        if (val.ValidateInput(ra, dec, sr))
        //        {

        //            bool temp = val.ValidateOtherParameters(uband, gband, rband, iband, zband, searchtype, returntype, limit);
        //            //String query = getQuery(val.ra, val.dec, val.radius);
        //            //query = addWhereClause(query, val);

        //            return runQuery(query);
        //        }
        //        else throw new ArgumentException("Enter proper input parameters.");

        //    }
        //    catch (Exception exp)
        //    {
        //        throw new Exception(exp.Message);
        //    }
        //}

       
    }
}