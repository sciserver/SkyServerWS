using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sciserver_webService.SDSSFields
{
    public class Validation
    {
        private double ra, dec, sr,rra,ddec;
        private string band;
        public bool ValidateInput(string ra, string dec, string sr)
        {
            try
            {
                this.ra = Convert.ToDouble(ra);
                this.dec = Convert.ToDouble(dec);
                this.sr = Convert.ToDouble(sr);
                return true;
            }
            catch (Exception e)
            {
                throw new Exception("The input values are not in correct format. ra,dec,radius all need real numbers.");
            }
        }

        public bool ValidateInput(string ra, string dec, string rra, string ddec) {
            try
            {
                this.ra = Convert.ToDouble(ra);
                this.dec = Convert.ToDouble(dec);
                this.rra = Convert.ToDouble(rra);
                this.ddec = Convert.ToDouble(ddec);
                
                return true;
            }
            catch (Exception e)
            {
                throw new Exception("The input values are not in correct format. ra,dec,rra,ddec all need real numbers.");
            }
        }

        string[] bands = { "u", "g", "i","r","z" };
        public bool ValidateInput( string band) {

            try {

                if (bands.Contains(band))
                    return true;
                else return false;
            }catch(Exception exp){
                throw new Exception("The input values are not in correct format. ra,dec,rra,ddec all need real numbers.");
            
            }
        }

        public double getRa() {
            return this.ra;
        }

        public double getDec() {
            return this.dec;
        }

        public double getRadius() {
            return this.sr;
        }

        public double getRRa() {
            return this.rra;
        }

        public double getDDec() {
            return this.ddec;
        }

        public string getBand() {
            return band;
        }
    }
}