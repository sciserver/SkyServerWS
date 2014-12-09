using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Net.Http.Headers;
using System.Web.Mvc;

namespace Sciserver_webService
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {

            
            //EarthScience
            config.Routes.MapHttpRoute(
                name: "getData",
                routeTemplate: "EarthScience/{controller}",
                defaults: new { controller = "getData" }
            );            
            
            /// Radial Search
            config.Routes.MapHttpRoute(
                name: "RadialSearch",
                routeTemplate: "dr10/SearchTools/{controller}",
                defaults: new { controller = "RadialSearch" }
            );

            /// Rectangular Search
            config.Routes.MapHttpRoute(
                name: "RectangularSearch",
                routeTemplate: "dr10/SearchTools/{controller}",
                defaults: new { controller = "RectangularSearch" }
            );

            /// SQL Search
            config.Routes.MapHttpRoute(
                name: "SqlSearch",
                routeTemplate: "dr10/SearchTools/{controller}",
                defaults: new { controller = "SqlSearch" }
            );

            config.Filters.Add(new ExceptionFilter.ExceptionHandleAttribute());
            //GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;            
            //config.Formatters.XmlFormatter.SupportedMediaTypes.Remove(new MediaTypeHeaderValue("application/xml"));
            //var json = config.Formatters.JsonFormatter;
            ////json.SerializerSettings.PreserveReferencesHandling = Newtonsoft.Json.PreserveReferencesHandling.Objects;
            //config.Formatters.Remove(config.Formatters.XmlFormatter);
            //config.Formatters.Remove(config.Formatters.JsonFormatter);            
            //config.Formatters.Remove(json);
            config.Formatters.XmlFormatter.UseXmlSerializer = true;
            //config.Formatters.XmlFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("image/jpeg"));

        }
    }
}
