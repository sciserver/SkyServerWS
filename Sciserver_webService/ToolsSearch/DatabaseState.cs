using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using net.ivoa.VOTable;
using Newtonsoft.Json;
using System.IO;
using System.Text;
using Sciserver_webService.Common;
using System.Web.Http;
using System.Threading;
using System.Threading.Tasks;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Net.Http.Headers;
using System.Net.Http.Formatting;
using Sciserver_webService.ExceptionFilter;
using net.ivoa.VOTable;
using Sciserver_webService.Common;
using SciServer.Logging;
using System.Web;
using Newtonsoft.Json;
using System.IO;
using System.Text;
using Sciserver_webService.Models;
using System.Data.SqlClient;

using System.Configuration;
using System.Data;

namespace Sciserver_webService.ToolsSearch
{

    public class DatabaseState : IHttpActionResult
    {

        public DatabaseState()
        {
        }

        public async Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage();

            HttpResponseMessage resp = new HttpResponseMessage();

            String a = ConfigurationManager.AppSettings["DBconnectionString"];
            //return a;
            DataSet ds = new DataSet();
            DataTable dt = new DataTable("DatabaseInfo");
            dt.Columns.Add("db_server");
            dt.Columns.Add("db_name");
            dt.Columns.Add("db_info");
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.AppSettings["DBconnectionString"]))
            {
                dt.Rows.Add(conn.DataSource, conn.Database, "For Search Tools.");
            }
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.AppSettings["SkyServer"]))
            {
                dt.Rows.Add(conn.DataSource, conn.Database, "For image cutout (object overlays).");
            }
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.AppSettings["SkyServerImage"]))
            {
                dt.Rows.Add(conn.DataSource, conn.Database, "For image cutout (SDSS image tiles).");
            }
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.AppSettings["SkyServer2Mass"]))
            {
                dt.Rows.Add(conn.DataSource, conn.Database, "For image cutout (2MASS image tiles).");
            }

            ds.Tables.Add(dt);
            ds.RemotingFormat = SerializationFormat.Xml;
            Action<Stream, HttpContent, TransportContext> WriteToStream = WriteToStream = (stream, foo, bar) => { OutputUtils.WriteJson(ds, stream); stream.Close(); };
            resp.Content = new PushStreamContent(WriteToStream, new MediaTypeHeaderValue((KeyWords.contentJson)));
            return resp;

        }
    }
}