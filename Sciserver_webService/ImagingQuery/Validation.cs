using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

///This class validates all the input parameters for Imaging Query
///ra,dec and search radius are mandatory input parameters all other are optional and some are default values
///
namespace Sciserver_webService.QueryTools
{
    public class Validation
    {
        public double ra  { get; set; }
        public double dec { get; set; }
        public double sr  { get; set; }
        public double ra2 { get; set; }
        public double dec2 { get; set; }
    
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

        public double ug_minColor { get; set; }
        public double ug_maxColor { get; set; }
        public double gr_minColor { get; set; }
        public double gr_maxColor { get; set; }
        public double ri_minColor { get; set; }
        public double ri_maxColor { get; set; }
        public double iz_minColor { get; set; }
        public double iz_maxColor { get; set; }
        
        
        public string searchtype { get; set; }

        public string returnType = "json"; // default return type

        public string objFlagON { get; set; }

        public string objFlagOff { get; set; }

        public string[] objFlagsOn;
        public string[] objFlagsOff;

        public Dictionary<string, string> dictionary = new Dictionary<string, string>();
    
        public Validation() { }
        public Validation (String ra, String dec, String sr, String limit,String imaging, String spectroscopy, String magColorType,
                           String uMag, String iMag, String gMag, String rMag, String zMag, String uColor,String gColor, 
                           String iColor, String rColor, String zColor, String objType, String score, String objFlagON, String objFlagOff) 
        {
            if (validateInput(ra, dec, sr))
            {
               
                this.dec = this.dec % 180;					// bring dec within the circle
                if (Math.Abs(this.dec) > 90)				// if it is "over the pole",
                {
                    this.dec = (this.dec - 90) % 180;	    // bring int back to the [-90..90] range
                    ra += 180;						        // and go 1/2 way round the globe
                }
                this.ra = this.ra % 360;					// bring ra into [0..360]
                if (this.ra < 0) this.ra += 360;

            } else {
                throw new Exception("The input values are not in correct format. ra,dec,radius all need real numbers.");
            }
        }


        public Validation(String ra, String dec, String sr)
        {
            if (validateInput(ra, dec, sr))
            {
                this.dec = this.dec % 180;					// bring dec within the circle
                if (Math.Abs(this.dec) > 90)				// if it is "over the pole",
                {
                    this.dec = (this.dec - 90) % 180;	    // bring int back to the [-90..90] range
                    ra += 180;						        // and go 1/2 way round the globe
                }
                this.ra = this.ra % 360;					// bring ra into [0..360]
                if (this.ra < 0) this.ra += 360;
            }
            else
            {
                throw new Exception("The input values are not in correct format. ra,dec,radius all need real numbers.");
            }
        }

        private bool validateInput(String ra, String dec, String sr) {
            try
            {
                this.ra = Convert.ToDouble(ra);
                this.dec = Convert.ToDouble(dec);
                this.sr = Convert.ToDouble(sr);
                return true;
            }
            catch (Exception e)
            {
                throw new Exception("The input values are not in correct format. ra,dec,radius all need real numbers."+e.Message);
            }
        }

        //private void createDictionary(String ra, String dec, String sr,String limit, String imaging, String spectroscopy, String magColorType,
        //                   String uMag, String iMag, String gMag, String rMag, String zMag, 
        //                   String uColor, String gColor,String iColor, String rColor, String zColor, 
        //                   String objType, String score, String objFlagON, String objFlagOff)
        //{           
        //}
    }
}