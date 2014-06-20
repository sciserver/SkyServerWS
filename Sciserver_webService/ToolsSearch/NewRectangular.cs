using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Net;
using System.Net.Security;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;

using System.Net.Http;

namespace Sciserver_webService.SearchTools
{
    public class NewRectangular
    {
        private HttpService _httpService = new HttpService();    
        private String query = "";
        private String token = "";

        public NewRectangular(String ra, String dec, String ra2, String dec2,String uband ,String gband,
                              String rband , String iband,String zband,String searchtype , String returntype, String token) {
           
            if (ra == null || dec == null || ra2 == null || dec2 == null) throw new ArgumentException("There are not enough parameters to process your request.");

            Validation val = new Validation();
            if(val.ValidateInput(ra,dec,ra2,dec2)){
               bool temp = val.ValidateOtherParameters(uband, gband, rband, iband, zband, searchtype, returntype);
               this.query = getQuery(val.ra, val.dec, val.ra2,val.dec2);
               this.query = addWhereClause(this.query, val);

               this.token = token;
     
            }
        }


        public HttpResponseMessage getResponse() {
            RunCasjobs run = new RunCasjobs();
            return run.postCasjobs(this.query,this.token) ;
        }

        // Using some other trial code
         //public HttpResponseMessage getJSONstring() {
         
         //    HttpService _httpService = new HttpService();

         //    Uri host = new Uri("http://dev.sciserver.org/CasJobs/RestApi/");
         //    string path = "contexts/dr10/query";             
         //    Dictionary<string, string> headerParameters = new Dictionary<string, string>();
         //    headerParameters.Add("X-Auth-Token", "882adff2e60144d0bfa81179143019c4");
         //    //headerParameters.Add("Content-Type", "application/json");             
         //    //Best not to store this in your class // this is for username and password
         //    NetworkCredential credential = new NetworkCredential("X-Auth-Token", "882adff2e60144d0bfa81179143019c4");

         //    //// Trial with HttpClient
         //    _httpService.postTest(host, path, headerParameters, this.query);

         //    //String json  = _httpService.PostJSON(host, path, headerParameters,this.query, credential);


         //    return _httpService.response;
         //}

         private string getQuery(double ra, double dec, double ra2, double dec2)
         {
             // This part is used for UI to have direct link to explore page , for web service we just return objid
             //string query = " SELECT  TOP 10 '<a target=INFO href=../../../en/tools/explore/obj.aspx?id=' + cast(p.objId as varchar(20)) ";
             //       query += " + '>' + cast(p.objId as varchar(20)) + '</a>' as objID,";  

             string query = "Select top 10 p.objid, ";
             query += " p.run, p.rerun, p.camcol, p.field, p.obj,";
             query += " p.type, p.ra, p.dec, p.u,p.g,p.r,p.i,p.z,";
             query += " p.Err_u, p.Err_g, p.Err_r,p.Err_i,p.Err_z";
             query += " FROM fGetObjFromRect(" + ra + "," + ra2 + "," + dec + "," + dec2 + " ) n, PhotoPrimary p  WHERE n.objID=p.objID ";
             return query;
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
    }
}