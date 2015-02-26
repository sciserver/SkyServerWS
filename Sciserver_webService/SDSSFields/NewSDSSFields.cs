using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;

using Sciserver_webService.casjobs;

namespace Sciserver_webService.SDSSFields
{
    public class NewSDSSFields
    {
        private string strConn, cmdTemplate, cmdTemplate2, urlPrefix;      
        
        private double ra, dec, sr,dra,ddec;
        private string band ="all";
        public String sqlQuery { get; set; }

        public NewSDSSFields()
        { }        

        public NewSDSSFields(Dictionary<string,string> dictionary, String positiontype) {

                this.validateInput(dictionary,positiontype);                
        }

        private bool validateInput(Dictionary<string, string> requestDir, String positiontype)
        {
                try
                {
                    this.ra = Convert.ToDouble(requestDir["ra"]);
                    this.dec = Convert.ToDouble(requestDir["dec"]);
                    switch (positiontype)
                    {
                        case "FieldArray":
                            //cmdTemplate = ConfigurationManager.AppSettings["CmdTemplate"];                            
                            cmdTemplate = FieldQueries.cmdTemplate1;
                            this.sr = Convert.ToDouble(requestDir["radius"]);
                            if (!CheckLimits(ra, dec, sr)) throw new Exception("check the values of ra,dec and search radius");                
                            this.sqlQuery = this.SqlSelectCommand();                            
                            break;

                        case "FieldArrayRect":
                            //cmdTemplate = ConfigurationManager.AppSettings["CmdTemplate2"];
                            cmdTemplate = FieldQueries.cmdTemplate2;
                            this.dra = Convert.ToDouble(requestDir["dra"]);
                            this.ddec = Convert.ToDouble(requestDir["ddec"]);
                            this.sqlQuery = this.SqlRectCommand();
                            break;

                        case "ListOfFields":
                            //cmdTemplate = ConfigurationManager.AppSettings["ListOfFields"];
                            cmdTemplate = FieldQueries.ListOfFields;
                            this.sr = Convert.ToDouble(requestDir["radius"]);
                            if (!CheckLimits(ra, dec, sr)) throw new Exception("check the values of ra,dec and search radius");
                            this.sqlQuery = this.SqlSelectCommand();  
                            break;

                        case "UrlsOfFields":
                            //cmdTemplate = ConfigurationManager.AppSettings["CmdTemplate"];
                            cmdTemplate = FieldQueries.UrlOfFields;
                            this.sr = Convert.ToDouble(requestDir["radius"]);
                            if (!CheckLimits(ra, dec, sr)) throw new Exception("check the values of ra,dec and search radius");
                            try { this.band = requestDir["band"]; }catch(Exception e) {}
                            this.sqlQuery = this.SqlSelectCommand().Replace("TEMPURL",getBandUrl());  
                            
                            break;

                        default: break;
                            
                   }
                    
                    return true;
                }
                catch (FormatException fx) { throw new ArgumentException("InputParameters are not in proper format."); }
                catch (Exception e) { throw new ArgumentException("There are not enough parameters to process your request Or Parameters values are not properly entered."); }
            }

            private bool CheckLimits(System.Double ra, System.Double dec, System.Double sr)
            {
                if ((sr < 0.0)) return false;
                if ((ra < 0.0) || (ra > 360.0)) return false;
                if ((dec < -90.0) || (dec > 90.0)) return false;
                return true;
            }

            ///dbo.fGetUrlFitsCFrame(f.fieldId,'u') as u_url,&#xA;   
            ///dbo.fGetUrlFitsCFrame(f.fieldId,'g') as g_url,&#xA;  
            ///dbo.fGetUrlFitsCFrame(f.fieldId,'r') as r_url,&#xA;   
            ///dbo.fGetUrlFitsCFrame(f.fieldId,'i') as i_url,&#xA;   
            ///dbo.fGetUrlFitsCFrame(f.fieldId,'z') as z_url&#xA; 
            ///
            /// <summary>
            /// Build SQL query from the input parameters
            /// </summary>
            /// <param name="ra">RA of center in degrees (double)</param>
            /// <param name="dec">Dec of center in degrees (double)</param>
            /// <param name="radius">Search radius in arcmins (double)</param>
            /// <returns>SQL query (string)</returns>
            private string SqlSelectCommand()
            {
                StringBuilder sql = new StringBuilder(cmdTemplate);
                sql.Replace("TEMPLATE", this.ra + "," + this.dec + "," + this.sr);

                return sql.ToString();
            }

            private string getBandUrl() {
                string bandurls = "";
                if (this.band.Equals("all")) this.band = "u,g,r,i,z"; 
                string[] bands = this.band.Split(',');
                for (int i = 0; i < bands.Length; i++) {
                    bandurls += "dbo.fGetUrlFitsCFrame(f.fieldId,'"+bands[i]+"') as "+bands[i]+"_url, ";
                }
                return bandurls;
            }
            /// <summary>
            /// Build SQL query from the input parameters
            /// </summary>
            /// <param name="ra">RA of center in degrees (double)</param>
            /// <param name="dec">Dec of center in degrees (double)</param>
            /// <param name="size">Search size in degrees (double)</param>
            /// <returns>SQL query (string)</returns>
            private string SqlRectCommand()
            {
                StringBuilder sql = new StringBuilder(cmdTemplate);
                sql.AppendFormat("and f.raMax >= {0}", this.ra - this.dra);
                sql.AppendFormat("and f.raMin <= {0}", this.ra + this.dra);
                sql.AppendFormat("and f.decMax >= {0}", this.dec - this.ddec);
                sql.AppendFormat("and f.decMin <= {0}", this.dec + this.ddec);
                return sql.ToString();
            }

        

            /// <summary>
            /// Build SQL query from the input parameters
            /// </summary>
            /// <param name="ra">RA of center in degrees (double)</param>
            /// <param name="dec">Dec of center in degrees (double)</param>
            /// <param name="radius">Search radius in arcmins (double)</param>
            /// <returns>SQL query (string)</returns>
            public string SqlSelectCommand(double ra, double dec, double radius)
            {
                StringBuilder sql = new StringBuilder(cmdTemplate);
                sql.Replace("TEMPLATE", ra + "," + dec + "," + radius);
                return sql.ToString();
            }

            /// <summary>
            /// Build SQL query from the input parameters
            /// </summary>
            /// <param name="ra">RA of center in degrees (double)</param>
            /// <param name="dec">Dec of center in degrees (double)</param>
            /// <param name="size">Search size in degrees (double)</param>
            /// <returns>SQL query (string)</returns>
            public string SqlSelectCommand(double ra, double dec, double dra, double ddec)
            {
                StringBuilder sql = new StringBuilder(cmdTemplate2);
                sql.AppendFormat("and f.raMax >= {0}", ra - dra);
                sql.AppendFormat("and f.raMin <= {0}", ra + dra);
                sql.AppendFormat("and f.decMax >= {0}", dec - ddec);
                sql.AppendFormat("and f.decMin <= {0}", dec + ddec);
                return sql.ToString();
            }

          

            /// <summary>
            /// Util func to return a bunch of 0s, used to create the URLs
            /// </summary>
            /// <param name="n">Number of zeros</param>
            /// <returns>zeros (string)</returns>
            protected string Zeros(int n)
            {
                string ret = "";
                for (int i = 0; i < n; i++) ret += "0";
                return ret;
            }

            /// <summary>
            /// Runs the query and returns all matching fields
            /// </summary>
            /// <param name="ra">RA of center in degrees (double)</param>
            /// <param name="dec">Dec of center in degrees (double)</param>
            /// <param name="radius">Search radius in arcmins (double)</param>
            /// <returns>All fields (Field[])</returns>
       
            public Field[] FieldArray(DataSet ds)
            {                
                int num = ds.Tables[0].Rows.Count;
                Field[] field = new Field[num];
                for (int i = 0; i < num; i++)
                {
                    DataRow row = ds.Tables[0].Rows[i];
                    field[i] = new Field(ds.Tables[0].Rows[i]);
                    for (int j = 0; j < field[i].passband.Length; j++)
                    {
                        field[i].passband[j].url = field[i].bandUrls[j];
                        //field[i].passband[j].url = this.FieldUrl(field[i],field[i].passband[j].filter);
                    }
                }
                return field;
            }
                
            /// <summary>
            /// Runs the query and returns all matching fields
            /// </summary>
            /// <param name="ra">RA of center in degrees (double)</param>
            /// <param name="dec">Dec of center in degrees (double)</param>
            /// <param name="dra">Delta RA in degrees (double)</param>
            /// <param name="ddec">Delta Dec in degrees (double)</param>
            /// <returns>All fields (Field[])</returns>
        
            public Field[] FieldArrayRect(DataSet ds)
            {                
                int num = ds.Tables[0].Rows.Count;
                Field[] field = new Field[num];
                for (int i = 0; i < num; i++)
                {
                    DataRow row = ds.Tables[0].Rows[i];
                    field[i] = new Field(ds.Tables[0].Rows[i]);
                    for (int j = 0; j < field[i].passband.Length; j++)
                    {
                        //field[i].passband[j].url = this.FieldUrl(field[i], field[i].passband[j].filter);
                        field[i].passband[j].url = field[i].bandUrls[j];
                    }
                }
                return field;
            }

            /// <summary>
            /// Simple interface to get basic of the relevant fields
            /// </summary>
            /// <param name="ra">RA of center in degrees (double)</param>
            /// <param name="dec">Dec of center in degrees (double)</param>
            /// <param name="radius">Search radius in arcmins (double)</param>
            /// <returns>Basic info of fields (string[])</returns>
            public string[] ListOfFields(DataSet ds)
            {                
                int num = ds.Tables[0].Rows.Count;
                string[] ret = new string[num];
                for (int i = 0; i < num; i++)
                {
                    DataRow row = ds.Tables[0].Rows[i];
                    ret[i] = row["run"] + "," + row["rerun"] + "," + row["camcol"] + "," + row["field"];
                }
                return ret;
            }



            /// <summary>
            /// Simple interface to get the urls of fields within the search radius
            /// </summary>
            /// <param name="ra">RA of center in degrees (double)</param>
            /// <param name="dec">Dec of center in degrees (double)</param>
            /// <param name="radius">Search radius in arcmins (double)</param>
            /// <param name="band">Passband name, one of u,g,r,i,z (string)</param>
            /// <returns>URLs (string[])</returns>
            public string[] UrlOfFields(DataSet ds)
            {
                //band = band.ToLower();
                band = "ugriz";
                char[] bands = { 'u', 'g', 'r', 'i', 'z' };
                int num = ds.Tables[0].Rows.Count;
                Field[] field = new Field[num];
                string[] ret = new string[num * band.Length];
                int j = 0;
                for (int i = 0; i < num; i++)
                {
                    DataRow row = ds.Tables[0].Rows[i];
                    field[i] = new Field(ds.Tables[0].Rows[i]);

                    //foreach (char b in band)
                    //{
                    //    if ("ugriz".IndexOf(b) == -1)
                    //        throw new ArgumentException("Band should be one of \"ugriz\", it was " + b);

                    //    int pos = Array.IndexOf(bands, b);
                    for (int pos = 0; pos < bands.Length; pos++)
                    {
                        ret[j++] = field[i].bandUrls[pos];
                    }

                }
                return ret;
            }

        /// <summary>
        /// Get URL for field given by run, rerun, etc...
        /// </summary>
        /// <param name="run"></param>
        /// <param name="rerun"></param>
        /// <param name="camcol"></param>
        /// <param name="field"></param>
        /// <param name="band"></param>
        /// <returns>URL (string)</returns>
        public string FieldUrl(int run, int rerun, int camcol, int field, string band)
        {
            band = band.ToLower();
            string bands = "ugriz";
            if (bands.IndexOf(band) < 0)
                throw new ApplicationException("Parameter 'band' is invalid! Should be one of u,g,r,i or z...");
            string run6digit = Zeros(6 - run.ToString().Length) + run;
            string field4digit = Zeros(4 - field.ToString().Length) + field;
            ///return this.urlPrefix+"run="+run+"&rerun="+rerun+"&camcol="
            ///	+camcol+"&filter="+band+"&field="+field4digit;            
            // Since field id on file system always 4 digit and run always 6 digit appending the string with zeros            
            return this.urlPrefix + "/" + rerun + "/" + run + "/" + camcol + "/frame-" + band
                   + "-" + run6digit + "-" + camcol
                   + "-" + field4digit + ".fits.bz2";
        }


        /// <summary>
        /// Get URL for field - used by FieldArray()
        /// </summary>
        /// <param name="f">Field</param>
        /// <param name="band"></param>
        /// <returns>URL (string)</returns>
        public string FieldUrl(Field f, string band)
        {
            return FieldUrl(f.run, f.rerun, f.camcol, f.field, band);
        }

   }
}