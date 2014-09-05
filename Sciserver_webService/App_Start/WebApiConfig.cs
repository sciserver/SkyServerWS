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

            //Consearch 
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "dr10/ConeSearch/{controller}/",
                defaults: new { controller = "ConeSearch" }
            );
            config.Routes.MapHttpRoute(
                name: "FieldArrayRect",
                routeTemplate: "dr10/SDSSFields/{controller}",
                defaults: new { controller = "FieldsArrayRect" }
            );
            
            //// Following four are for SDSS Fields
            //config.Routes.MapHttpRoute(
            //    name: "FieldsArray",
            //    routeTemplate: "dr10/SDSSFields/{controller}/{id1}",
            //    defaults: new { controller = "FieldsArray", id1 = RouteParameter.Optional }
            //);
            //config.Routes.MapHttpRoute(
            //    name: "ListOfFields",
            //    routeTemplate: "dr10/SDSSFields/{controller}/{action}/{id2}",
            //    defaults: new { controller = "SDSSFields", id2 = RouteParameter.Optional }
            //);

            

            config.Routes.MapHttpRoute(
                name: "UrlsOfFields",
                routeTemplate: "dr10/SDSSFields/{controller}/{id4}",
                defaults: new {controller = "UrlsOfFields", id4 = RouteParameter.Optional }
            );



            //// SIAP 
            config.Routes.MapHttpRoute(
                name: "getSIAPInfo",
                routeTemplate: "dr10/SIAP/{controller}",
                defaults: new { controller = "getSIAPInfo"}
            );

            //// SDSS ImageCutout
            config.Routes.MapHttpRoute(
                name: "getJpeg",
                routeTemplate: "dr10/ImgCutout/{controller}",
                defaults: new { controller = "getJpeg" }
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

            /// TESTing new casjobs REST client 
            config.Routes.MapHttpRoute(
                name: "NewRectangular",
                routeTemplate: "dr10/SearchTools/{controller}",
                defaults: new { controller = "NewRectangular" }
            );


            /// Imaging Query
            config.Routes.MapHttpRoute(
                name: "Cone",
                routeTemplate: "dr10/ImagingQuery/{controller}",
                defaults: new { controller = "Cone" }
            );


            /// Spectro Query
            config.Routes.MapHttpRoute(
                name: "ConeSpectro",
                routeTemplate: "dr10/SpectroQuery/{controller}",
                defaults: new { controller = "ConeSpectro" }
            );

            /// IR-Spectro Query
            config.Routes.MapHttpRoute(
                name: "ConeIR",
                routeTemplate: "dr10/IRSpectraQuery/{controller}",
                defaults: new { controller = "ConeIR" }
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
