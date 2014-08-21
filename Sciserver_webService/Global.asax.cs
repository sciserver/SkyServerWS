using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using Sciserver_webService.Formatters;
using SciServer.Logging;
namespace Sciserver_webService
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801
    public class MvcApplication : System.Web.HttpApplication
    {
        public Logger Log = Logger.FromConfig();

        public MvcApplication()
            : base()
        {
            Log.Connect();
        }

        protected void Application_Start()
        {
            
            GlobalConfiguration.Configuration.Formatters.Add(new CustomResponseFormatter());

            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }
    }
}