using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sciserver_webService.ToolsSearch
{
    public class Validation
    {
        
        private string band;        
       
        private String[] searchtypes = new String[] { "equitorial", "galactic" };

        public String whichquery = "imaging";
        public double ra { get; set; }
        public double dec { get; set; }
        public double radius { get; set; }
        public string fp { get; set; }
        public string format { get; set; }

        public Boolean gband { get; set; }
        public Boolean uband { get; set; }
        public Boolean rband { get; set; }
        public Boolean iband { get; set; }
        public Boolean zband { get; set; }
        
        public int umin { get; set; }
        public int umax { get; set; }
        public int gmin { get; set; }
        public int gmax { get; set; }
        public int imin { get; set; }
        public int imax { get; set; }
        public int rmin { get; set; }
        public int rmax { get; set; }
        public int zmin { get; set; }
        public int zmax { get; set; }
                
        public int limit = 10;
        public string searchtype { get; set; }

        private string returnType = "json"; // default search type

        public double ra_max { get; set; }
        public double dec_max { get; set; }

        public String uband_s =null,  gband_s=null,  rband_s=null,  iband_s=null,  zband_s=null,  limit_s=null, returntype_s=null;

        //public string option = "imaging";

        public string returnFormat {
            get { return returnType; }
        }

        public Validation() { }

        public Validation(Dictionary<string, string> requestDir)
        {
           try {
               this.ra = Convert.ToDouble(requestDir["min_ra"]);
               this.dec = Convert.ToDouble(requestDir["min_dec"]);
               this.ra_max = Convert.ToDouble(requestDir["max_ra"]);
               this.dec_max = Convert.ToDouble(requestDir["max_dec"]);
           }
           catch (FormatException fx) { throw new ArgumentException("InputParameters are not in proper format."); }
           catch (Exception e) { throw new ArgumentException("There are not enough parameters to process your request."); }

           try { this.uband_s = requestDir["uband"]; }catch (Exception e) { }                       
           try { this.gband_s = requestDir["gband"];}catch (Exception e) { }
           try { this.rband_s = requestDir["rband"];}catch (Exception e) { }
           try { this.iband_s = requestDir["iband"];}catch (Exception e) { }
           try { this.zband_s = requestDir["zband"];}catch (Exception e) { }
           try { this.searchtype = requestDir["searchtype"]; } catch (Exception e) { }
           try { this.returntype_s = requestDir["returntype"]; } catch (Exception e) { }
           try { this.limit_s = requestDir["limit"]; } catch (Exception e) { }
           try { this.whichquery = requestDir["whichquery"]; }catch (Exception e) { this.whichquery = "imaging"; }
           //try { this.option = requestDir["option"]; } catch (Exception e) { this.option = "imaging"; } 
        }

        public Validation(Dictionary<string, string> requestDir, String r)
        {
            try
            {
                this.ra = Convert.ToDouble(requestDir["ra"]);
                this.dec = Convert.ToDouble(requestDir["dec"]);
                this.radius = Convert.ToDouble(requestDir["radius"]);
                try
                {
                    this.fp = requestDir["fp"];
                }
                catch (Exception e) {
                    this.fp = "none";
                }
            }
            catch (FormatException fx) { throw new ArgumentException("InputParameters are not in proper format."); }
            catch (Exception e) { throw new ArgumentException("There are not enough parameters to process your request."); }

            try { this.uband_s = requestDir["uband"]; }
            catch (Exception e) { }
            try { this.gband_s = requestDir["gband"]; }
            catch (Exception e) { }
            try { this.rband_s = requestDir["rband"]; }
            catch (Exception e) { }
            try { this.iband_s = requestDir["iband"]; }
            catch (Exception e) { }
            try { this.zband_s = requestDir["zband"]; }
            catch (Exception e) { }
            try { this.searchtype = requestDir["searchtype"]; }
            catch (Exception e) { }
            try { this.format = requestDir["format"]; }
            catch (Exception e) { }
            try { this.limit_s = requestDir["limit"]; }
            catch (Exception e) { }
        }

        public bool ValidateInput(string ra, string dec, string sr)
        {
            try
            {
                this.ra = Convert.ToDouble(ra);
                this.dec = Convert.ToDouble(dec);
                this.radius = Convert.ToDouble(sr);
                return true;
            }
            catch (Exception e)
            {
                throw new Exception("The input values are not in correct format. ra,dec,radius all need real numbers.");
            }
        }

        public bool ValidateOtherParameters(String uband, String gband, String rband, String iband, String zband,
                         String searchtype , String returntype, String limit )
        {
            try {
                
                this.gband = false; this.uband = false; this.rband = false; this.iband = false; this.zband = false;
                this.searchtype = "equitorial"; // default search type

                string[] values;
                if (gband != null)
                {
                    try
                    {
                        values = gband.Split(',');
                        gmin = Int32.Parse(values[0]);
                        gmax = Int32.Parse(values[1]);
                    }
                    catch (Exception e)
                    {
                        gmin = 0; gmax = 20;
                    }
                    this.gband = true;
                }

                if (rband != null) {
                    try
                    {
                        values = rband.Split(',');
                        rmin = Int32.Parse(values[0]);
                        rmax = Int32.Parse(values[1]);
                    }
                    catch (Exception e)
                    {
                        rmin = 0; rmax = 20;
                    }
                    this.rband = true;
                }
                if (iband != null) {
                    try
                    {
                        values = iband.Split(',');
                        imin = Int32.Parse(values[0]);
                        imax = Int32.Parse(values[1]);
                    }
                    catch (Exception e)
                    {
                        imin = 0; imax = 20;
                    }
                    this.iband = true;
                }
                if (zband != null) {

                    try
                    {
                        values = zband.Split(',');
                        zmin = Int32.Parse(values[0]);
                        zmax = Int32.Parse(values[1]);
                    }
                    catch (Exception e)
                    {
                        zmin = 0; zmax = 20;
                    }
                    this.zband = true;
                }
                if (uband != null)
                {

                    try
                    {
                        values = uband.Split(',');
                        umin = Int32.Parse(values[0]);
                        umax = Int32.Parse(values[1]);
                    }
                    catch (Exception e)
                    {
                        umin = 0; umax = 20;
                    }
                    this.uband = true;
                }
                if (searchtype != null) {

                    if (searchtypes.Contains(searchtype))
                    {
                        this.searchtype = searchtype;
                    } else {
                        this.searchtype = searchtypes[0];
                    }                    
                }
                if (returntype != null)
                {
                    this.returnType = returntype;
                }
                if (limit != null)
                {
                    this.limit = Int32.Parse(limit);
                }
                return true;
            }
            catch (Exception ex) {
                throw new Exception(""+ex.Message);
            }
        }

        public bool ValidateInput(string ra, string dec, string ra2, string dec2)
        {
            try
            {
                this.ra = Convert.ToDouble(ra);
                this.dec = Convert.ToDouble(dec);
                this.ra_max = Convert.ToDouble(ra2);
                this.dec_max = Convert.ToDouble(dec2);
                return true;
            }
            catch (Exception e)
            {
                throw new Exception("The input values are not in correct format. ra,dec,radius all need real numbers.");
            }
        }
    }
}