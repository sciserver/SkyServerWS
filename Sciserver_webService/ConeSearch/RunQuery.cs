using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using Jhu.Graywulf.Format;
using Jhu.Graywulf.Schema;
using System.Xml;


namespace Sciserver_webService.ConeSearch
{
    public class RunQuery
    {
        private System.Globalization.CultureInfo culture = System.Globalization.CultureInfo.InvariantCulture;
        public String testQuery(){
            var w = new StringWriter();
            var wr = new XmlTextWriter(w);
            using (var cn = new SqlConnection("Initial Catalog=BESTDR10;Data Source=sdss3n.pha.jhu.edu;User ID=skyuser;Password=nchips54"))
            {
                cn.Open();
                using (var cmd = new SqlCommand(" SELECT top 2 ra,dec from Frame ", cn))
                {
                    using (var dr = cmd.ExecuteReader())
                    {
                       var vot = new VOTable(wr, culture);
                       vot.WriteFromDataReader(dr);                        
                    }
                }
            }
            return w.ToString();
        }
    }
}