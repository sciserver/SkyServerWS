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
                name: "ConeSearch",
                //routeTemplate: "{anything}/ConeSearch/{controller}/",
                routeTemplate: "ConeSearch/{controller}/",
                defaults: new { controller = "ConeSearch" }
            );
            //config.Routes.MapHttpRoute(
            //    name: "FieldArrayRect",
            //    routeTemplate: "{anything}/SDSSFields/{controller}",
            //    defaults: new { controller = "FieldsArrayRect" }
            //);
            
            ////// Following four are for SDSS Fields
            //config.Routes.MapHttpRoute(
            //    name: "FieldsArray",
            //    routeTemplate: "{anything}/SDSSFields/{controller}/{id1}",
            //    defaults: new { controller = "FieldsArray", id1 = RouteParameter.Optional }
            //);
            config.Routes.MapHttpRoute(
                name: "ListOfFields",
                //routeTemplate: "{anything}/SDSSFields/{controller}",
                routeTemplate: "SDSSFields/{controller}",
                defaults: new { controller = "ListOfFields" }
            );           

            //config.Routes.MapHttpRoute(
            //    name: "UrlsOfFields",
            //    routeTemplate: "{anything}/SDSSFields/{controller}/{id4}",
            //    defaults: new {controller = "UrlsOfFields", id4 = RouteParameter.Optional }
            //);



            //// SIAP 
            config.Routes.MapHttpRoute(
                name: "getSIAPInfo",
                //routeTemplate: "{anything}/SIAP/{controller}",
                routeTemplate: "SIAP/{controller}",
                defaults: new { controller = "getSIAP"}
            );

            //// SDSS ImageCutout
            config.Routes.MapHttpRoute(
                name: "getJpeg",
                //routeTemplate: "{anything}/ImgCutout/{controller}",
                routeTemplate: "ImgCutout/{controller}",
                defaults: new { controller = "getJpeg" }
            );


            /// Radial Search
            config.Routes.MapHttpRoute(
                name: "RadialSearch",
                //routeTemplate: "{anything}/SearchTools/{controller}",
                routeTemplate: "SearchTools/{controller}",
                defaults: new { controller = "RadialSearch" }
            );

            /// Rectangular Search
            config.Routes.MapHttpRoute(
                name: "RectangularSearch",
                //routeTemplate: "{anything}/SearchTools/{controller}",
                routeTemplate: "SearchTools/{controller}",
                defaults: new { controller = "RectangularSearch" }
            );

            /// SQL Search
            config.Routes.MapHttpRoute(
                name: "SqlSearch",
                //routeTemplate: "SearchTools/{controller}",
                routeTemplate: "SearchTools/{controller}",
                defaults: new { controller = "SqlSearch" }
            );

            /// Database state
            config.Routes.MapHttpRoute(
                name: "State",
                routeTemplate: "SearchTools/{controller}",
                defaults: new { controller = "DBState" }
            );


            /// TESTing new casjobs REST client 
            config.Routes.MapHttpRoute(
                name: "NewRectangular",
                //routeTemplate: "{anything}/SearchTools/{controller}",
                routeTemplate: "SearchTools/{controller}",
                defaults: new { controller = "NewRectangular" }
            );


            /// Imaging Query
            config.Routes.MapHttpRoute(
                name: "Cone",
                //routeTemplate: "{anything}/ImagingQuery/{controller}",
                routeTemplate: "ImagingQuery/{controller}",
                defaults: new { controller = "Cone" }
            );


            /// Spectro Query
            config.Routes.MapHttpRoute(
                name: "ConeSpectro",
                //routeTemplate: "{anything}/SpectroQuery/{controller}",
                routeTemplate: "SpectroQuery/{controller}",
                defaults: new { controller = "ConeSpectro" }
            );

            /// IR-Spectro Query
            config.Routes.MapHttpRoute(
                name: "ConeIR",
                //routeTemplate: "{anything}/IRSpectraQuery/{controller}",
                routeTemplate: "IRSpectraQuery/{controller}",
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
