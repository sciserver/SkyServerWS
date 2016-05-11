using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using Sciserver_webService.ToolsSearch;
using Sciserver_webService.Common;

namespace Sciserver_webService.ConeSearch
{
    public class ConeSearch
    {
        public String ConeSearchQuery{get;set;}
        
        private double ra,dec,sr;

        public ConeSearch() { }
        public ConeSearch(Dictionary<string, string> dictionary) {
            this.validateInput(dictionary);
            if (!CheckLimits(ra, dec, sr)) throw new Exception("Check the values of ra, dec and search radius. Permitted values are -90 <= dec <= 90, 0 <= ra <= 360 and sr > 0.");
        }

        private bool CheckLimits(System.Double ra, System.Double dec, System.Double sr)
        {
            if ((sr < 0.0)) return false;
            if ((ra < 0.0) || (ra > 360.0)) return false;
            if ((dec < -90.0) || (dec > 90.0)) return false;
            return true;
        }

        private bool validateInput(Dictionary<string, string> requestDir)
        {
            try
            {
                this.ra = Convert.ToDouble(requestDir["ra"]);
                this.dec = Convert.ToDouble(requestDir["dec"]);
                this.sr = Convert.ToDouble(requestDir["sr"]);
                return true;
            }
            catch (FormatException fx) { throw new ArgumentException("InputParameters are not in proper format."); }
            catch (Exception e) { throw new ArgumentException("There are not enough parameters to process your request Or Parameters values are not properly entered."); }
        }

        public String getConeSearchQuery() {

            StringBuilder qry = new StringBuilder();
            qry.Append(" select " + KeyWords.ConeSelect);
            qry.Append(" from PhotoPrimary p, dbo.fGetNearbyObjEq(" + this.ra + "," + this.dec + "," + this.sr + ") n");
            qry.Append(" where p.objId=n.objId");
            return qry.ToString();
        }
    }
}