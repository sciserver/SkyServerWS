using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;
using System.Data.SqlTypes;
using System.Globalization;

namespace Sciserver_webService.Common
{
    public class Utilities
    {   /// <summary>
        /// Parses an identifier represented as integer or hex string 
        /// starting with '0x...' (e.g. '0x112d0bd721480030'). 
        /// </summary>
        public static long? ParseId (string s)
        {
            long? id = null;
            if (s != null & !"".Equals(s))
            {
                s = s.ToLower();
                try
                {
                    if (s.StartsWith("0x"))
                        id = long.Parse(s.Substring(2), NumberStyles.AllowHexSpecifier);
                    else
                        id = long.Parse(s);
                }
                catch (Exception) { }
            }

            return id;
        }

        public static bool valueCheck(string name, double val, double lo, double hi)
        {
            //var err = false;
            if (val > hi)
            {
                //Response.Write(name + " must be less than " + hi + " <P>");
                return true;
            }
            if (val < lo)
            {
                //Response.Write(name + " must be more than " + lo + "<P>");
                return true;
            }
            if (double.IsNaN(val))
            {
                //Response.Write("Please enter numerical values for " + name + "<P>");
                return true;
            }
            return false;
        }

        public static bool rangeCheck(string name, double min, double max, double lo, double hi) {
	    if(min > max){
		    //Response.Write("Minimum "+name+" value must be less than maximum <P>" );
		    return true;
		    }
	    if (max>hi){
		    //Response.Write("Max "+name+" must be less than "+hi+" <P>");
		    return true;
		    }
	    if (min<lo){
		    //Response.Write("Min "+name+" must be more than "+lo+"<P>");
		    return true;
		    }
       if (double.IsNaN(min) || double.IsNaN(max) ) {
		    //Response.Write("Please enter numerical values for "+name+" min and max.<P>");
		    return true;
		    }
       return false;
    }

        public static string getURL(HttpRequest request)
        {
            string host = request.ServerVariables["SERVER_NAME"];
            string path = request.ServerVariables["SCRIPT_NAME"];

            string root = "http://" + host;
            string[] q = path.Split('/');

            string lang = "";
            for (int i = 0; i < q.Length; i++)
            {
                if (i > 0) root += "/";
                root += q[i];
                lang = q[i];
                if (lang == "en" || lang == "de" || lang == "jp"
                  || lang == "hu" || lang == "sp" || lang == "ce" || lang == "pt" || lang == "zh" || lang == "uk" || lang == "ru")
                {
                    // depth = q.length - i - 2;
                    return root;
                }
            }
            return root;
        }

        public static string BytesToHex(byte[] bytes)
        {
            if (bytes == null) return null;
            else
                return "0x" + BitConverter.ToString(bytes).Replace("-", string.Empty).ToLower();
        }

        public static double parseRA(string s_ra) {
	        double v;
	        if (s_ra.Contains(":")) {
		        v = hms2deg(s_ra,':');
	        } else {
		        if(Regex.IsMatch(s_ra,@"\d \d")) {
			        v = hms2deg(s_ra,' ');
		        } else {
			        v = double.Parse(s_ra);
			        if(double.IsNaN(v)) v=180.0;
			        if (v<0) v+= 360;
			        if (v>360) v-= 360;
		        }
	        }
	        return v;
        }

        public static double parseDec(string s_dec) {
	        double v;
	        if (s_dec.Contains(":")) {
		        v = dms2deg(s_dec,':');
	        } else {
		        if(Regex.IsMatch(s_dec,@"\d \d")) {
			        v = dms2deg(s_dec,' ');
		        } else {
			        v = double.Parse(s_dec);
			        if(double.IsNaN(v)) v=0.0;
			        if (v<-90) v= -90;
			        if (v>90) v= 90;
		        }
	        }
	        return v;
        }

        public static double hms2deg(string s, char c) {
		
            while(s.Length > 0 && (s.Substring(0,1).Equals(" ") || s.Substring(0,1).Equals("+")) )
			    s = s.Substring(1);
		    string[] a = s.Split(c);
		    return 15*double.Parse(a[0])+double.Parse(a[1])/4.0+double.Parse(a[2])/240.0;
	    }

	    public static double dms2deg(string s, char c) {
            while(s.Length > 0 && (s.Substring(0,1).Equals(" ") || s.Substring(0,1).Equals("+")) )
			    s = s.Substring(1);
		    string[] a = s.Split(c);
		    if(s.Contains("-"))	
			    return -(-1.0*double.Parse(a[0])+double.Parse(a[1])/60.0+double.Parse(a[2])/3600.0);
		    else
			    return 1.0*double.Parse(a[0])+double.Parse(a[1])/60.0+double.Parse(a[2])/3600.0;
	    }

        public static string ccut(string name,int count,double min,double max) {
	        return ((count==0)?" WHERE ":" AND ")+name+" BETWEEN "+min+" AND "+max;
        }


        public static double dec2glat(double ra, double dec)
        {
            double galPoleRA = 192.859508;
            double galPoleDec = 27.128336;
            double galAscNode = 32.932;

            // convert all angles from degrees to radians for the trig functions
            ra = ra * (Math.PI / 180);
            dec = dec * (Math.PI / 180);
            galPoleRA = galPoleRA * (Math.PI / 180);
            galPoleDec = galPoleDec * (Math.PI / 180);
            galAscNode = galAscNode * (Math.PI / 180);

            double glat = Math.Asin((Math.Cos(dec) * Math.Cos(galPoleDec) * Math.Cos(ra - galPoleRA)) + (Math.Sin(dec) * Math.Sin(galPoleDec)));

            glat = glat * (180 / Math.PI);

            if (glat < -90) glat = glat + 90;
            if (glat > 90) glat = glat - 90;

            return glat;
        }

        public static double ra2glon(double ra, double dec)
        {
            double glat = dec2glat(ra,dec); // first, find the galactic latitude (B) since we'll need it in the formula

            double galPoleRA = 192.859508;
            double galPoleDec = 27.128336;
            double galAscNode = 32.932;

            // convert all angles from degrees to radians for the trig functions
            ra = ra * (Math.PI / 180);
            dec = dec * (Math.PI / 180);
            galPoleRA = galPoleRA * (Math.PI / 180);
            galPoleDec = galPoleDec * (Math.PI / 180);
            galAscNode = galAscNode * (Math.PI / 180);
            glat = glat * (Math.PI / 180);

            double numerator = Math.Sin(dec) - (Math.Sin(glat) * Math.Sin(galPoleDec));
            double denominator = Math.Cos(dec) * Math.Sin(ra - galPoleRA) * Math.Cos(galPoleDec);

            double glon = Math.Atan2(numerator, denominator) + galAscNode;  // use atan2 instead of atan to solve ambiguity of arctan

            glon = glon * (180 / Math.PI); // convert answer back to degrees

            if (glon < 0) glon = glon + 360;
            if (glon > 360) glon = glon - 360;

            return glon;
        }


        public static double glon2ra(double L, double B)
        {
            // function to convert from a galactic longitude (b) to right ascension
            // input: the galactic latitude (l) and longitude (b) of a point in degrees
            // output: the celestial RA in degrees

            double i = 192.859508;
            double j = 27.128336;
            double k = 32.932;

            double m = L - k;

            // convert all angles from degrees to radians for the trig functions
            L = L * (Math.PI / 180);
            B = B * (Math.PI / 180);
            double x = i * (Math.PI / 180);
            double y = j * (Math.PI / 180);
            double z = m * (Math.PI / 180);

            double numerator = Math.Cos(B) * Math.Cos(z);
            double denominator = (Math.Sin(B) * Math.Cos(y)) - (Math.Cos(B) * Math.Sin(y) * Math.Sin(z));

            double ra = Math.Atan2(numerator, denominator) + x;  // use atan2 instead of atan to solve ambiguity of arctan
            // double ra = Math.Atan(numerator / denominator) + x;

            ra = ra * (180 / Math.PI); // convert answer back to degrees
            if (ra > 360) ra = ra - 360;
            return ra;
        }

        public static double glat2dec(double L, double B)
        {
            // function to convert from a galactic latitude (L) to declination
            // input: the galactic latitude (L) and longitude (B) of a point in degrees
            // output: that point's celestial dec in degrees

            double i = 192.859508;
            double j = 27.128336;
            double k = 32.932;

            double m = L - k;

            // convert all angles from degrees to radians for the trig functions
            L = L * (Math.PI / 180);
            B = B * (Math.PI / 180);
            double x = i * (Math.PI / 180);
            double y = j * (Math.PI / 180);
            double z = m * (Math.PI / 180);

            double dec = Math.Asin((Math.Cos(B) * Math.Cos(y) * Math.Sin(z)) + (Math.Sin(B) * Math.Sin(y)));
            dec = dec * (180 / Math.PI); // convert answer back to degrees

            return dec;

        }

        public static string getSqlString(object o)
        {
            string result = "";          
            if (o is SqlBinary)
            {
                result = Utilities.BytesToHex(((SqlBinary)o).Value);
            }
            else
            {
                result = o.ToString();
            }
            return result;
        }

        public static void ValueCheckOrFail(string name, double val, double lo, double hi)
        {
            if (val > hi)
            {
                //throw new Exception(name + " must be less than " + hi);
                throw new Exception(name + " must be between " + lo.ToString() + " and " + hi.ToString() + ".");
            }
            if (val < lo)
            {
                //throw new Exception(name + " must be more than " + lo);
                throw new Exception(name + " must be between " + lo.ToString() + " and " + hi.ToString() + ".");
            }
            if (double.IsNaN(val))
            {
                throw new Exception("Please enter numerical values for " + name + ".");
            }
        }

        public static void RangeCheckOrFail(string name, double min, double max, double lo, double hi)
        {
            if (min > max)
            {
                throw new Exception("Minimum " + name + " magnitude must be less than maximum "+name+" magnitude.");
            }
            if (max > hi)
            {
                //throw new Exception("Max " + name + " must be less than " + hi);
                throw new Exception(name + " magnitude must be between " + lo.ToString() + " and " + hi.ToString());
            }
            if (min < lo)
            {
                //throw new Exception("Min " + name + " must be more than " + lo);
                throw new Exception(name + " magnitude must be between " + lo.ToString() + " and " + hi.ToString());
            }
            if (double.IsNaN(min) || double.IsNaN(max))
            {
                throw new Exception("Please enter numerical values for minimum and maximum " + name + " magnitude.");
            }
        }
    }
}