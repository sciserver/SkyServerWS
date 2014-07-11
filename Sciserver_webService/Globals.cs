using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;

namespace Sciserver_webService
{
    public class Globals
    {
        // this is data release and target setting for all the different services, eg. DR10
        public static string DR = ConfigurationManager.AppSettings["TARGET"];
        /// <summary>
        /// Authentication settings used for the KeyStone authentication settings
        /// </summary>
        public static string xauth = "X-Auth-Token";
        public static string contentJson = "application/json";
        public static string contentXML = "application/XML";
        /// <summary>
        /// Casjobs setting for REST api
        /// </summary>
        public static string casjobsREST =ConfigurationManager.AppSettings["CASJobsREST"] ;
        public static string casjobsContextPath = "contexts/"+DR+"/query";
        /// <summary>
        /// CASJobs old settings
        /// </summary>
        public static long   CJobsWSID = long.Parse(ConfigurationManager.AppSettings["CJobsWSID"]);
        public static string CJobsPasswd = ConfigurationManager.AppSettings["CJobsPassWD"];
        public static string CJobsTARGET = DR;
        /// <summary>
        ///  Search tools and casjobs message
        /// </summary>
        public static string Rectangular = "FOR RECTANGULAR SEARCH";
        public static string Radial = "FOR RADIAL SEARCH";
        public static string SQL = "FOR SQL SEARCH";
        public static string ImagingQuery = "FOR IMAGING QUERY";
        public static string SpectroQuery = "FOR SPECTRO QUERY";
        public static string IRSpectroQuery = "FOR IR-SPECTRO QUERY";
        /// <summary>
        /// Imaging Query
        /// </summary>
        public static string Database = ConfigurationManager.AppSettings["database"];
        public static string Release = DR;
        public static int DefTimeout = Int32.Parse(ConfigurationManager.AppSettings["defTimeout"]);

        

    }
}