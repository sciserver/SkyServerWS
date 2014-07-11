using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sciserver_webService.casjobs;
using System.Configuration;
using System.Data;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Sciserver_webService.UseCasjobs
{
    public class RadialSearch
    {
        private static long CJobsWSID = long.Parse(ConfigurationSettings.AppSettings["CJobsWSID"]);
        private static string CJobsPasswd = ConfigurationSettings.AppSettings["CJobsPassWD"];
        private static string CJobsTARGET = ConfigurationSettings.AppSettings["CJobsTARGET"];
        public string getData(String ra, String dec, String sr,
                              String uband ,String gband, String rband , String iband,String zband,
                              String searchtype , String returntype)
        {
            try {
                Validation val = new Validation();
                if (val.ValidateInput(ra, dec, sr))
                {

                    bool temp = val.ValidateOtherParameters(uband, gband, rband, iband, zband, searchtype, returntype);
                    String query = getQuery(val.ra, val.dec, val.radius);
                    query = addWhereClause(query, val);

                    return runQuery(query);
                }
                else throw new ArgumentException("Enter proper input parameters.");
                
            }catch (Exception exp) {
                throw new Exception(exp.Message);
            }
        }

        private string runQuery(string finalQuery) {

            JobsSoapClient cjobs = new JobsSoapClient();
            DataSet ds = cjobs.ExecuteQuickJobDS(CJobsWSID, CJobsPasswd, finalQuery, CJobsTARGET, "FOR RadialSearch", false);
            return JsonConvert.SerializeObject(ds, Formatting.Indented);           
        }

        private string getQuery(double ra, double dec, double sr)
        {
            // This part is used for UI to have direct link to explore page , for web service we just return objid
            //string query = " SELECT  TOP 10 '<a target=INFO href=../../../en/tools/explore/obj.aspx?id=' + cast(p.objId as varchar(20)) ";
            //       query += " + '>' + cast(p.objId as varchar(20)) + '</a>' as objID,";  

            string query = "Select top 10 p.objid, ";
                   query += " p.run, p.rerun, p.camcol, p.field, p.obj,";
                   query += " p.type, p.ra, p.dec, p.u,p.g,p.r,p.i,p.z,";
                   query += " p.Err_u, p.Err_g, p.Err_r,p.Err_i,p.Err_z";
                   query += " FROM fGetNearbyObjEq("+ra+","+dec+","+sr+") n, PhotoPrimary p  WHERE n.objID=p.objID ";
            return query;
        }

        private string addWhereClause(string queryString, Validation val) {

            if (val.uband) { queryString += " AND p.u between " + val.umin + " AND " + val.umax; }
            if (val.gband) { queryString += " AND p.g between " + val.gmin + " AND " + val.gmax; }
            if (val.iband) { queryString += " AND p.i between " + val.imin + " AND " + val.imax; }
            if (val.rband) { queryString += " AND p.r between " + val.rmin + " AND " + val.rmax; }
            if (val.zband) { queryString += " AND p.z between " + val.zmin + " AND " + val.zmax; }

            return queryString;
        }


       
    }
}