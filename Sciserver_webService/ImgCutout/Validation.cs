using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sciserver_webService.ImgCutout
{
    public class Validation
    {
        private double ra, dec, scale;
        private string R, C, F, Z;

        public bool ValidateInput(string ra, string dec, string sr)
        {
            try
            {
                if (ra.LastIndexOf(":") == -1)
                    this.ra =  Double.Parse(ra);
                else
                    this.ra = Coord.hms2deg(ra);	                
		        if (dec.LastIndexOf(":")==-1)
		            this.dec = Double.Parse(dec); 
		        else
			        this.dec = Coord.dms2deg(dec);

                this.scale = Convert.ToDouble(sr);

                return true;
            }
            catch (Exception e)
            {
                throw new ArgumentException("The input values are not in correct format. ra must be in [0,360], dec must be in [-90,90], scale must be in [0.015, 60.0], height and width must be in [64,2048]. \n Detail Error Message: "+e.Message);
            }
        }
       

  
        public double getRa() {
            return this.ra;
        }

        public double getDec() {
            return this.dec;
        }

        public double getScale() {
            return this.scale;
        }

        int[] zoom = new int []{0, 12, 25, 50};

        public bool ValidateInput(String run, String  camcol, String field, String zoom) {
            try
            {

                int R = Int32.Parse(run);
                if (!(R > 93 && R < 8162)) throw new ArgumentException("Run (R) is out of range.\n ");
                this.R = run;

                int C = Int32.Parse(camcol);
                if (!(C > 0 && C < 7)) throw new ArgumentException("Camcol (C) out of range.\n ");
                this.C = camcol;

                int F = Int32.Parse(field);
                if (!(F > 10 && F < 1002)) throw new ArgumentException("Field (F) out of range.\n ");
                this.F = field;

                int Z = Int32.Parse(zoom);
                int pos = Array.IndexOf(this.zoom, Z);
                if (pos == -1) throw new ArgumentException("Zoom (Z) out of Range");
                this.Z = zoom;

                return true;
            }
            catch(Exception e){

                String ExpMessage = "R, C, F, Z parameters should be integers.\nRun (R) should be in [94,8162].\n";
                ExpMessage += "Camcol (C) should be in [1,6].\n Field (F) should be in [11,1001].\n";
                ExpMessage += "Zoom (Parameter Z) can be among these values only [0, 12, 25, 50] \n";
                throw new ArgumentException(   "Exception:" + e.Message+ " \n"+ExpMessage);
            }
        }


        public string getRun() { return R; }
        public string getCamcol() { return C; }
        public string getField() { return F; }
        public string getZoom() { return Z; }
  
    }
}