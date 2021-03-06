﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sciserver_webService.Common;

namespace Sciserver_webService.ToolsSearch
{
    public class Validation
    {
        
        private string band;

        public String whichway = "";
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
        public Boolean jband { get; set; }
        public Boolean hband { get; set; }
        public Boolean kband { get; set; }

        public double umin { get; set; }
        public double umax { get; set; }
        public double gmin { get; set; }
        public double gmax { get; set; }
        public double imin { get; set; }
        public double imax { get; set; }
        public double rmin { get; set; }
        public double rmax { get; set; }
        public double zmin { get; set; }
        public double zmax { get; set; }
        public double jmin { get; set; }
        public double jmax { get; set; }
        public double hmin { get; set; }
        public double hmax { get; set; }
        public double kmin { get; set; }
        public double kmax { get; set; }
                
        public Int64 limit = 10;
        public string coordtype { get; set; }

        private string returnType = "json"; // default search type

        public double ra_max { get; set; }
        public double dec_max { get; set; }

        public String uband_s =null,  gband_s=null,  rband_s=null,  iband_s=null,  zband_s=null,  limit_s=null, returntype_s=null;
        public String jband_s = null, hband_s = null, kband_s = null;
        //public string option = "imaging";

        public string returnFormat {
            get { return returnType; }
        }

        public Validation() { }

        public Validation(Dictionary<string, string> requestDir)
        {

            try { this.returntype_s = requestDir["returntype"]; }
            catch (Exception e) { }
            try { this.format = requestDir["format"]; }
            catch (Exception e) { }
            try { this.limit_s = requestDir["limit"]; }
            catch (Exception e) { }
            try { this.coordtype = requestDir["coordtype"]; }
            catch (Exception e) { }

            string RAstring = "RA";
            string DECstring = "Dec";

            if (coordtype == "galactic")
            {
                RAstring = "'l'";
                DECstring = "'b'";
            }

            try
            {
                this.ra = Utilities.parseRA(requestDir["min_ra"]);
                this.dec = Utilities.parseDec(requestDir["min_dec"]);
                this.ra_max = Utilities.parseRA(requestDir["max_ra"]);
                this.dec_max = Utilities.parseDec(requestDir["max_dec"]);
            }
            catch (FormatException fx) { throw new ArgumentException("Error: Input RA and Dec should be valid numerical values."); }
            catch (Exception e) { throw new ArgumentException("There are not enough parameters to process your request."); }




            if (this.coordtype == "galactic")
            {
                if (this.ra_max < this.ra)
                { throw new ArgumentException("Error: lower 'l' limit should be less than upper 'l' limit."); }

                if (this.dec_max < this.dec)
                { throw new ArgumentException("Error: lower 'b' limit should be less than upper 'b' limit."); }

                Utilities.ValueCheckOrFail("'l'", this.ra, 0.0, 360.0);
                Utilities.ValueCheckOrFail("'l'", this.ra_max, 0.0, 360.0);
                Utilities.ValueCheckOrFail("'b'", this.dec, -90.0, 90.0);
                Utilities.ValueCheckOrFail("'b'", this.dec_max, -90.0, 90.0);
            }
            else
            {
                if (this.ra_max < this.ra)
                { throw new ArgumentException("Error: lower RA limit should be less than upper RA limit."); }

                if (this.dec_max < this.dec)
                { throw new ArgumentException("Error: lower Dec limit should be less than upper Dec limit."); }

                Utilities.ValueCheckOrFail("RA", this.ra, 0.0, 360.0);
                Utilities.ValueCheckOrFail("RA", this.ra_max, 0.0, 360.0);
                Utilities.ValueCheckOrFail("Dec", this.dec, -90.0, 90.0);
                Utilities.ValueCheckOrFail("Dec", this.dec_max, -90.0, 90.0);
            }
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
            try { this.jband_s = requestDir["jband"]; }
            catch (Exception e) { }
            try { this.hband_s = requestDir["hband"]; }
            catch (Exception e) { }
            try { this.kband_s = requestDir["kband"]; }
            catch (Exception e) { }
            try { this.uband = requestDir["check_u"] == "u" ? true : false; }
            catch (Exception e) { }
            try { this.gband = requestDir["check_g"] == "g" ? true : false; }
            catch (Exception e) { }
            try { this.rband = requestDir["check_r"] == "r" ? true : false; }
            catch (Exception e) { }
            try { this.iband = requestDir["check_i"] == "i" ? true : false; }
            catch (Exception e) { }
            try { this.zband = requestDir["check_z"] == "z" ? true : false; }
            catch (Exception e) { }
            try { this.jband = requestDir["check_j"] == "j" ? true : false; }
            catch (Exception e) { }
            try { this.hband = requestDir["check_h"] == "h" ? true : false; }
            catch (Exception e) { }
            try { this.kband = requestDir["check_k"] == "k" ? true : false; }
            catch (Exception e) { }

            try
            {
                if (this.coordtype == "galactic")
                {
                    double RA = Utilities.glon2ra(this.ra, this.dec);
                    double DEC = Utilities.glat2dec(this.ra, this.dec);
                    this.ra = RA;
                    this.dec = DEC;
                    RA = Utilities.glon2ra(this.ra_max, this.dec_max);
                    DEC = Utilities.glat2dec(this.ra_max, this.dec_max);
                    this.ra_max = RA;
                    this.dec_max = DEC;
                    double coord;
                    if (this.ra > this.ra_max)
                    {
                        coord = this.ra_max;
                        this.ra_max = this.ra;
                        this.ra = coord;
                    }
                    if (this.dec > this.dec_max)
                    {
                        coord = this.dec_max;
                        this.dec_max = this.dec;
                        this.dec = coord;
                    }
                }
            }
            catch (Exception e) { this.coordtype = ""; }
        }

        public Validation(Dictionary<string, string> requestDir, String r)
        {

            try { this.returntype_s = requestDir["returntype"]; }
            catch (Exception e) { }
            try { this.format = requestDir["format"]; }
            catch (Exception e) { }
            try { this.limit_s = requestDir["limit"]; }
            catch (Exception e) { }
            try { this.coordtype = requestDir["coordtype"]; }
            catch (Exception e) { }

            string RAstring = "RA";
            string DECstring = "Dec";
            
            if (coordtype == "galactic")
            {
                RAstring = "'l'";
                DECstring = "'b'";
            }


            try
            {
                this.ra = Utilities.parseRA(requestDir["ra"]);
                this.dec = Utilities.parseDec(requestDir["dec"]);
                this.radius = Convert.ToDouble(requestDir["radius"]);
                try
                {
                    this.fp = requestDir["fp"];
                }
                catch (Exception e) {
                    this.fp = "none";
                }
            }
            catch (FormatException fx) 
            { 
                throw new ArgumentException("Error: Input " + RAstring +", "+ DECstring + " and radius should be valid numerical values."); 
            }
            catch (Exception e) { throw new ArgumentException("There are not enough parameters to process your request."); }


            Utilities.ValueCheckOrFail(RAstring, this.ra, 0.0, 360.0);
            Utilities.ValueCheckOrFail(DECstring, this.dec, -90.0, 90.0);
            Utilities.ValueCheckOrFail("radius", this.radius, 0.0, 60.0);

            try { this.uband_s = requestDir["uband"]; }catch (Exception e) { }
            try { this.gband_s = requestDir["gband"]; }catch (Exception e) { }
            try { this.rband_s = requestDir["rband"]; }catch (Exception e) { }
            try { this.iband_s = requestDir["iband"]; }catch (Exception e) { }
            try { this.zband_s = requestDir["zband"]; }catch (Exception e) { }
            try { this.jband_s = requestDir["jband"]; }catch (Exception e) { }
            try { this.hband_s = requestDir["hband"]; }catch (Exception e) { }
            try { this.kband_s = requestDir["kband"]; }catch (Exception e) { }
            try { this.uband = requestDir["check_u"] == "u" ? true : false; }catch (Exception e) { }
            try { this.gband = requestDir["check_g"] == "g" ? true : false; }catch (Exception e) { }
            try { this.rband = requestDir["check_r"] == "r" ? true : false; }catch (Exception e) { }
            try { this.iband = requestDir["check_i"] == "i" ? true : false; }catch (Exception e) { }
            try { this.zband = requestDir["check_z"] == "z" ? true : false; }catch (Exception e) { }
            try { this.jband = requestDir["check_j"] == "j" ? true : false; }catch (Exception e) { }
            try { this.hband = requestDir["check_h"] == "h" ? true : false; }catch (Exception e) { }
            try { this.kband = requestDir["check_k"] == "k" ? true : false; }catch (Exception e) { }
            try
            {
                this.coordtype = requestDir["coordtype"];
                if (this.coordtype == "galactic")
                {
                    double RA = Utilities.glon2ra(this.dec,this.ra);
                    double DEC = Utilities.glat2dec(this.dec, this.ra);
                    this.ra = RA;
                    this.dec = DEC;
                }
            }
            catch (Exception e) { this.coordtype = ""; }
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
                throw new Exception("The input values are not in correct format. RA, Dec and radius need to be real numbers.");
            }
        }

        public bool ValidateOtherParameters(String uband, String gband, String rband, String iband, String zband, String jband, String hband, String kband,
                         String searchtype , String returntype, String limit )
        {
            try {
                
                //this.gband = false; this.uband = false; this.rband = false; this.iband = false; this.zband = false;
                //this.searchtype = "equatorial"; // default search type

                string[] values;
                if (gband != null & this.gband == true)
                {
                    try
                    {
                        values = gband.Split(',');
                        gmin = Double.Parse(values[0]);
                        gmax = Double.Parse(values[1]);
                    }
                    catch (Exception e)
                    {
                        gmin = 0; gmax = 20;
                        throw new Exception("Please enter numerical values for the magnitude limits.");
                    }
                }

                if (rband != null & this.rband == true) {
                    try
                    {
                        values = rband.Split(',');
                        rmin = Double.Parse(values[0]);
                        rmax = Double.Parse(values[1]);
                    }
                    catch (Exception e)
                    {
                        rmin = 0; rmax = 20;
                        throw new Exception("Please enter numerical values for the magnitude limits.");
                    }
                }
                if (iband != null & this.iband == true)
                {
                    try
                    {
                        values = iband.Split(',');
                        imin = Double.Parse(values[0]);
                        imax = Double.Parse(values[1]);
                    }
                    catch (Exception e)
                    {
                        imin = 0; imax = 20;
                        throw new Exception("Please enter numerical values for the magnitude limits.");
                    }
                }
                if (zband != null & this.zband == true)
                {

                    try
                    {
                        values = zband.Split(',');
                        zmin = Double.Parse(values[0]);
                        zmax = Double.Parse(values[1]);
                    }
                    catch (Exception e)
                    {
                        zmin = 0; zmax = 20;
                        throw new Exception("Please enter numerical values for the magnitude limits.");
                    }
                }
                if (uband != null & this.uband == true)
                {

                    try
                    {
                        values = uband.Split(',');
                        umin = Double.Parse(values[0]);
                        umax = Double.Parse(values[1]);
                    }
                    catch (Exception e)
                    {
                        umin = 0; umax = 20;
                        throw new Exception("Please enter numerical values for the magnitude limits.");
                    }
                }
                if (jband != null & this.jband == true)
                {

                    try
                    {
                        values = jband.Split(',');
                        jmin = Double.Parse(values[0]);
                        jmax = Double.Parse(values[1]);
                    }
                    catch (Exception e)
                    {
                        jmin = 0; jmax = 20;
                        throw new Exception("Please enter numerical values for the magnitude limits.");
                    }
                }
                if (hband != null & this.hband == true)
                {

                    try
                    {
                        values = hband.Split(',');
                        hmin = Double.Parse(values[0]);
                        hmax = Double.Parse(values[1]);
                    }
                    catch (Exception e)
                    {
                        hmin = 0; hmax = 20;
                        throw new Exception("Please enter numerical values for the magnitude limits.");
                    }
                }
                if (kband != null & this.kband == true)
                {

                    try
                    {
                        values = kband.Split(',');
                        kmin = Double.Parse(values[0]);
                        kmax = Double.Parse(values[1]);
                    }
                    catch (Exception e)
                    {
                        kmin = 0; kmax = 20;
                        throw new Exception("Please enter numerical values for the magnitude limits.");
                    }
                } 
                if (this.uband == true) Utilities.RangeCheckOrFail("u", umin, umax, 0, 35);
                if (this.gband == true) Utilities.RangeCheckOrFail("g", gmin, gmax, 0, 35);
                if (this.rband == true) Utilities.RangeCheckOrFail("r", rmin, rmax, 0, 35);
                if (this.iband == true) Utilities.RangeCheckOrFail("i", imin, imax, 0, 35);
                if (this.zband == true) Utilities.RangeCheckOrFail("z", zmin, zmax, 0, 35);
                if (this.jband == true) Utilities.RangeCheckOrFail("j", jmin, jmax, 0, 35);
                if (this.hband == true) Utilities.RangeCheckOrFail("h", hmin, hmax, 0, 35);
                if (this.kband == true) Utilities.RangeCheckOrFail("k", kmin, kmax, 0, 35);

                //if (searchtype != null) {
                //
                //    if (searchtypes.Contains(searchtype))
                //    {
                //        this.searchtype = searchtype;
                //    } else {
                //        this.searchtype = searchtypes[0];
                //    }                    
                //}
                if (returntype != null)
                {
                    this.returnType = returntype;
                }
                if (limit != null)
                {
                    try { this.limit = Int64.Parse(limit); }
                    catch { throw new Exception("Row limit should be a valid numerical value."); }
                }
                return true;
            }
            catch (Exception ex) {
                //throw new Exception("Input Parameters Validation Exception:"+ex.Message);
                throw new Exception("Error: " + ex.Message);
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
                throw new Exception("The input values are not in correct format: all need to be real numbers.");
            }
        }
    }
}