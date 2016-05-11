using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sciserver_webService.Common
{
    public class Functions
    {
        public const string ZERO_ID = "0x0000000000000000";

        public static string BytesToHex(byte[] bytes)
        {
            if (bytes == null) return null;
            else
                return "0x" + BitConverter.ToString(bytes).Replace("-", string.Empty).ToLower();
        }


        public static string hmsPad(double deg)
        {
            var hh = Math.Floor(deg / 15.0);
            var qq = 4.0 * (deg - 15 * hh);
            var mm = Math.Floor(qq);
            var ss = Math.Floor(600.0 * (qq - mm)) / 10.0;
            return (pad(hh) + " " + pad(mm) + " " + pad(ss));
        }

        public static string pad(double val)
        {
            return (val < 10) ? ("0" + val) : ("" + val);
        }

        public static string pad(string val)
        {
            return (double.Parse(val) < 10) ? ("0" + val) : ("" + val);
        }

        public static string dmsPad(double deg)
        {
            string sign = (deg < 0) ? "-" : "+";
            deg = (deg < 0) ? -deg : deg;
            double dd = Math.Floor(deg);
            double qq = 60.0 * (deg - dd);
            double mm = Math.Floor(qq);
            double ss = Math.Floor(600.0 * (qq - mm)) / 10.0;
            return (sign + pad(dd) + " " + pad(mm) + " " + pad(ss.ToString("F2")));
        }

        public static string SDSSname(double ra, double dec)
        {
            return "SDSS J" + hmsIAU(ra) + dmsIAU(dec);
        }

        public static string hmsIAU(double deg)
        {
            double hh = Math.Floor(deg / 15.0);
            double qq = 4.0 * (deg - 15 * hh);
            double mm = Math.Floor(qq);
            double ss = Math.Floor(6000.0 * (qq - mm)) / 100.0;
            return (pad(hh) + pad(mm) + pad(ss.ToString("F2")));
        }

        public static string dmsIAU(double deg)
        {
            string sign = (deg < 0) ? "-" : "+";
            deg = (deg < 0) ? -deg : deg;
            double dd = Math.Floor(deg);
            double qq = 60.0 * (deg - dd);
            double mm = Math.Floor(qq);
            double ss = Math.Floor(600.0 * (qq - mm)) / 10.0;
            return (sign + pad(dd) + pad(mm) + pad(ss.ToString("F1")));
        }

        public static string hmsC(double deg)
        {
            double hh = Math.Floor(deg / 15.0);
            double qq = 4.0 * (deg - 15 * hh);
            double mm = Math.Floor(qq);
            double ss = Math.Floor(6000.0 * (qq - mm)) / 100.0;
            return (pad(hh) + ":" + pad(mm) + ":" + pad(ss.ToString("F2")));
        }

        public static string dmsC(double deg)
        {
            string sign = (deg < 0) ? "-" : "+";
            deg = (deg < 0) ? -deg : deg;
            double dd = Math.Floor(deg);
            double qq = 60.0 * (deg - dd);
            double mm = Math.Floor(qq);
            double ss = Math.Floor(6000.0 * (qq - mm)) / 100.0;
            return (sign + pad(dd) + ":" + pad(mm) + ":" + pad(ss.ToString("F2")));
        }
    }
}