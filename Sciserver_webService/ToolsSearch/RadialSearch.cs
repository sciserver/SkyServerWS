using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Data;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using System.Text;
using System.IO;
using Sciserver_webService.casjobs;
using Sciserver_webService.Common;
using Sciserver_webService.UseCasjobs;



namespace Sciserver_webService.ToolsSearch
{
    public class RadialSearch
    {

        public String imageQuery = "";
        public String irQuery = "";
        public RadialSearch(Dictionary<string,string> requestDir) 
        {
            Validation val = new Validation(requestDir, "radialSearch");
            bool temp = val.ValidateOtherParameters(val.uband_s, val.gband_s, val.rband_s, val.iband_s, val.zband_s, val.searchtype, val.returntype_s, val.limit_s);
            this.buildImageQuery(val);
            this.buildIRQuery(val);
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

        private void buildImageQuery(Validation val)
        {
            string sql = "";

            if (val.fp != "none")
            {
                sql = "SELECT * FROM dbo.fFootprintEq(" + val.ra + "," + val.dec + "," + val.radius + ")";
            }

            else if (val.fp != "only")
            {
                sql = "SELECT ";
                sql += (val.limit <= 0) ? "" : " TOP " + val.limit;

                if (val.format == "html")
                {
                    sql += " ''<a target=INFO href=" + KeyWords.skyserverUrl + "/tools/explore/obj.aspx?id='' + cast(p.objId as varchar(20)) + ''>'' + cast(p.objId as varchar(20)) + ''</a>'' as objID,\n";
                }
                else
                {
                    sql += " p.objid,\n";
                }
                sql += "   p.run, p.rerun, p.camcol, p.field, p.obj,\n";
                sql += "   p.type, p.ra, p.dec, p.u,p.g,p.r,p.i,p.z,\n";
                sql += "   p.Err_u, p.Err_g, p.Err_r,p.Err_i,p.Err_z\n";
                sql += "   FROM fGetNearbyObjEq(" + val.ra + "," + val.dec + "," + val.radius + ") n,";
                sql += " PhotoPrimary p\n";
                sql += "   WHERE n.objID=p.objID ";

                int ccount = 1;               
            }
            
            this.imageQuery = this.addWhereClause(sql, val);
        }

        private void buildIRQuery(Validation val)
        {
            string sql;

            sql = "SELECT ";
            sql += (val.limit <= 0) ? "" : " TOP " + val.limit;

            if (val.format == "html")
            {
                sql += " ''<a target=INFO href=" + KeyWords.skyserverUrl + "/tools/explore/summary.aspx?apid='' + cast(p.apstar_id as varchar(40)) + ''>'' + cast(p.apstar_id as varchar(40)) + ''</a>'' as apstar_id,\n";
            }
            else
            {
                sql += " p.apstar_id, \n";
            }
            sql += "   p.apogee_id,p.ra, p.dec, p.glon, p.glat,\n";
            sql += "   p.vhelio_avg,p.vscatter,\n";
            sql += "   a.teff,a.logg,a.metals\n";
            sql += "   FROM apogeeStar p\n";
            sql += "   JOIN fGetNearbyApogeeStarEq(" + val.ra + "," + val.dec + "," + val.radius + ") n on p.apstar_id=n.apstar_id\n";
            sql += "   JOIN aspcapStar a on a.apstar_id = p.apstar_id";

            int ccount = 0;

            this.irQuery = this.addWhereClause(sql, val);
        }

        private string addWhereClause(string queryString, Validation val)
        {

            if (val.uband) { queryString += " AND p.u between " + val.umin + " AND " + val.umax; }
            if (val.gband) { queryString += " AND p.g between " + val.gmin + " AND " + val.gmax; }
            if (val.iband) { queryString += " AND p.i between " + val.imin + " AND " + val.imax; }
            if (val.rband) { queryString += " AND p.r between " + val.rmin + " AND " + val.rmax; }
            if (val.zband) { queryString += " AND p.z between " + val.zmin + " AND " + val.zmax; }

            return queryString;
        }

        public string getJson(string[] resultContent, string[] query)
        {
            string[] lines = resultContent[0].Split('\n');
            var csv = new List<string[]>(); // or, List<YourClass>            
            foreach (string line in lines)
                csv.Add(line.Split(',')); // or, populate YourClass          
            //string json = new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(csv);

            string[] lines2 = resultContent[1].Split('\n');
            var csv2 = new List<string[]>(); // or, List<YourClass>            
            foreach (string line2 in lines2)
                csv2.Add(line2.Split(',')); // or, populate YourClass          

            string[] querytype = new string[2] { "Imaging", "IR Spectra" };

            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);

            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                writer.Formatting = Formatting.Indented;
                writer.WriteStartArray();

                for (int r = 0; r < resultContent.Length; r++)
                {
                    writer.WriteStartObject();
                    writer.WritePropertyName("Query");
                    writer.WriteValue(query[r]);
                    writer.WritePropertyName("Query type");
                    writer.WriteValue(querytype[r]);
                    writer.WritePropertyName("Query Results:");
                    writer.WriteStartArray();
                    if (r == 0)
                    {
                        for (int i = 1; i < lines.Length; i++)
                        {
                            writer.WriteStartObject();
                            for (int Index = 0; Index < csv[0].Length; Index++)
                            {
                                writer.WritePropertyName(csv[0][Index]);
                                writer.WriteValue(csv[i][Index]);
                            }
                            writer.WriteEndObject();
                        }
                    }
                    else
                    {
                        for (int i = 1; i < lines2.Length; i++)
                        {
                            writer.WriteStartObject();
                            for (int Index = 0; Index < csv2[0].Length; Index++)
                            {
                                writer.WritePropertyName(csv2[0][Index]);
                                writer.WriteValue(csv2[i][Index]);
                            }
                            writer.WriteEndObject();
                        }
                    }
                    writer.WriteEndArray();
                    writer.WriteEndObject();
                }
                writer.WriteEndArray();
            }
            return sb.ToString();
        }

        public String getResult(String token, String casjobsMessage, String format)
        {
            try
            {
                RunCasjobs run = new RunCasjobs();
                String imageQueryResult = run.postCasjobs(this.imageQuery, token, casjobsMessage).Content.ReadAsStringAsync().Result;
                String irQueryResult = run.postCasjobs(this.irQuery, token, casjobsMessage).Content.ReadAsStringAsync().Result;
                String results = "";
                if (format.Equals("json"))
                    results = this.getJson(new String[2] { imageQueryResult, irQueryResult }, new String[2] { this.imageQuery, this.irQuery });
                else
                {
                    results = "# Imaging Query:" + this.imageQuery.Replace("\n", "") + "\n\n" + imageQueryResult;
                    results += "\n\n#IR Spectra Query:" + this.irQuery.Replace("\n", "") + "\n\n" + irQueryResult;
                }


                return results;

            }
            catch (Exception e)
            {
                throw new Exception("Error while running casjobs:" + e.Message);
            }
        }
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