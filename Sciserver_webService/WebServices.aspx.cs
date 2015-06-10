using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Sciserver_webService
{
    public partial class WebServices : System.Web.UI.Page
    {
        string baseurl = HttpContext.Current.Request.Url.AbsoluteUri.ToLower().Replace("WebServices.aspx","DR12");
        protected void Page_Load(object sender, EventArgs e)
        {

        }
    }
}