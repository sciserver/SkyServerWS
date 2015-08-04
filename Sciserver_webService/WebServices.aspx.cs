using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Sciserver_webService.Common;

namespace Sciserver_webService
{
    public partial class WebServices : System.Web.UI.Page
    {
        string baseurl = HttpContext.Current.Request.Url.AbsoluteUri.ToLower().Replace("WebServices.aspx",KeyWords.DataRelease.ToUpper());
        protected void Page_Load(object sender, EventArgs e)
        {

        }
    }
}