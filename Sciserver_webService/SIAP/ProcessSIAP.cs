using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Configuration;

namespace Sciserver_webService.SIAP
{
    public class ProcessSIAP
    {
        //public static string[] formats = { "metadata", "all", "image/fits", "graphic", "image/jpeg" };
        public static string[] formats = { "metadata", "all", "fits", "graphic", "jpeg" };
        public static string[] bands = { "u", "g", "r", "i", "z", "*" };

        public String getSiapQuery(string POS, string SIZE, string FORMAT, string bandpass)
        {
            String query = "";
            double ra, dec;
            double size1;
            double? size2;

            bool finish = false;            
            string inf = FORMAT.ToLower();
            int i;
            for (i = 0; i < formats.Length; i++)
            {
                if (inf.CompareTo(formats[i]) == 0) break;
            }
            switch (i)
            {
                case 0:
                    finish = true;
                    break;
                case 1: //	bandpass= "*"; // FORMAT = ALL doesn't mean all bands
                        break;
                case 2: // intentional fall through
                case 3: // intentional fall through 					
                case 4: break;

                default:
                    StringBuilder f = new StringBuilder("");
                    f.AppendFormat("Wrong FORMAT={0}. Please select one from the following list:", FORMAT);
                    f.Append(String.Join(",", formats));
                    //v = new SiapTable(f.ToString());
                    finish = true;
                    break;
            }


            if (!finish) try
                {
                    string[] poss = POS.Split(new char[] { ',' });
                    if (poss.Length < 2)
                    {
                        throw new ArgumentException("Wrong Input Parameters: Please specify RA,DEC in POS parameter"); 
                    }
                    try
                    {
                        ra = Convert.ToDouble(poss[0]);
                        dec = Convert.ToDouble(poss[1]);
                    }
                    catch (Exception e) { 
                        throw new ArgumentException("Ra,Dec Values are not properly specified."+e.Message);
                    }

                    string[] sizs = SIZE.Split(new char[] { ',' });
                    if (sizs.Length > 2)
                    {
                        throw new ArgumentException("Wrong Input Parameters: Please specify correct SIZE");
                    }
                    size1 = double.Parse(sizs[0]);
                    size2 = null;
                    if (sizs.Length > 1)
                        size2 = double.Parse(sizs[1]);

                    string message = valid_input(ra, dec, size1, size2, bandpass);
                    if (message.CompareTo("") != 0) //!valid_input(ra, dec, SIZE, bandpass))
                    {
                        throw new ArgumentException("Wrong Input Parameters:" + message);
                    }

                    this.buildQuery(ra, dec, size1, size2, bandpass);

                }
                catch (Exception e)
                {
                    throw new Exception(e.ToString());
                }
            return query;
        }

        private string valid_input(double ra, double dec, double size1, double? size2, string bandpass)
        {
            // if ((sr < 0.0) || sr > MAX)) return false;
            StringBuilder msg = new StringBuilder("");
            bool wrongband = false;

            if ((ra < 0.0) || (ra > 360.0) || (dec < -90.0) || (dec > 90.0)) msg.AppendFormat(" POS={0},{1} ", ra, dec);
            if (!size2.HasValue)
            {
                if ((size1 < 0.0)) msg.AppendFormat(" SIZE={0} ", size1);
            }
            else
            {
                if ((size1 < 0) || (size2 < 0)) msg.AppendFormat(" SIZE={0},{1}", size1, size2.Value);
            }


            bandpass = bandpass.ToLower();
            foreach (char b in bandpass)
            {
                if ("ugriz*".IndexOf(b) == -1)
                {
                    wrongband = true;
                    break;
                }
            }
            if (wrongband) msg.AppendFormat(" BANDPASS={0} ", bandpass);
            return msg.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="POS"></param>
        /// <param name="SIZE"></param>
        /// <param name="FORMAT"></param>
        /// <param name="bandpass"></param>
        /// <param name="whichquery"></param>
        /// <returns></returns>
       
        private String buildQuery(double ra, double dec, double size1, double? size2, string bandpass)
        {
            String query = "";           

            if (size2.HasValue)
                query = SqlRectCommand(ra,dec,size1,size2.Value);
            else
                query = SqlSelectCommand(ra,dec,size1);

            query = query.Replace("TEMPURL", getBandUrl(bandpass));
            return query;
        }

        public string SqlSelectCommand(double ra, double dec, double radius)
        {
            StringBuilder sql = new StringBuilder(ConfigurationManager.AppSettings["getSIAP1"]);
            sql.Replace("TEMPLATE", ra + "," + dec + "," + radius);
            return sql.ToString();
        }

        private string getBandUrl(String band)
        {
            string bandurls = "";
            if (band.Equals("all")) band = "u,g,r,i,z";
            string[] bands = band.Split(',');
            for (int i = 0; i < bands.Length; i++)
            {
                bandurls += "dbo.fGetUrlFitsCFrame(f.fieldId,'" + bands[i] + "') as " + bands[i] + "_url, ";
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
        private string SqlRectCommand(double ra, double dra,double dec,double ddec)
        {
            StringBuilder sql = new StringBuilder(ConfigurationManager.AppSettings["getSIAP2"]);
            sql.AppendFormat("and f.raMax >= {0}", ra - dra);
            sql.AppendFormat("and f.raMin <= {0}", ra + dra);
            sql.AppendFormat("and f.decMax >= {0}",dec - ddec);
            sql.AppendFormat("and f.decMin <= {0}",dec + ddec);
            return sql.ToString();
        }
        
    }
}