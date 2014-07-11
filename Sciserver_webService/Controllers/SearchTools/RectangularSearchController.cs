using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Net.Http.Headers;
using System.Net.Http.Formatting;
using Sciserver_webService.ExceptionFilter;
using Sciserver_webService.UseCasjobs;
using net.ivoa.VOTable;



// this is  web service version of Rectangular Search tool on the Skyserver web site.
// It takes two corners of search area in terms of ra and dec and returns result with SDSS objectids
//@deoyani@pha.jhu.edu
namespace Sciserver_webService.Controllers
{
    public class RectangularSearchController : ApiController
    {
        //// Get The cone search results
        [ExceptionHandleAttribute]
        public HttpResponseMessage Get([FromUri] String ra = null, [FromUri] String dec = null, [FromUri] String ra2 = null,
                            [FromUri] String dec2 = null ,[FromUri] String uband = null, [FromUri] String gband = null,
                            [FromUri] String rband= null,[FromUri] String iband = null, [FromUri] String zband = null,
                            [FromUri] String searchtype=null,[FromUri] String returntype="json")
        {
          
            HttpResponseMessage resp = new HttpResponseMessage();
            RectangularSearch rs = new RectangularSearch(ra,dec,ra2,dec2,uband,gband,rband,iband,zband,searchtype,returntype);
            if (returntype.Equals("json"))
            {
                String test = rs.getVOTable().ToString();
                resp.Content = new StringContent(rs.getJSONstring());
                resp.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            }
            else if (returntype.Equals("votable"))
            {

                resp.Content = new ObjectContent<VOTABLE>(rs.getVOTable(), new XmlMediaTypeFormatter(), new MediaTypeHeaderValue("application/xml"));
                //, new MediaTypeFormatterCollection() { new XmlMediaTypeFormatter() }, new FormatterSelector());
                //resp.Content.Headers.ContentType = new MediaTypeHeaderValue("application/xml");
            }

            resp.StatusCode = HttpStatusCode.OK;

            return resp;
            
        }
    }
}


