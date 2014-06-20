using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;

namespace Sciserver_webService
{
    public class Globals
    {
        public static string DR = ConfigurationManager.AppSettings["DataRelease"];

        public static string casjobsREST =ConfigurationManager.AppSettings["CASJobsREST"] ;
        public static string casjobsContextPath = "contexts/"+DR+"/query";
        public static string xauth = "X-Auth-Token";
        public static string contentJson = "application/json";
        public static long   CJobsWSID = long.Parse(ConfigurationManager.AppSettings["CJobsWSID"]);
        public static string CJobsPasswd = ConfigurationManager.AppSettings["CJobsPassWD"];
        public static string CJobsTARGET = ConfigurationManager.AppSettings["CJobsTARGET"];

        /// <summary>
        ///  Search tools
        /// </summary>
        public static string Rectangular = "FOR RECTANGULAR SEARCH";
        public static string Radial = "FOR RADIAL SEARCH";
        public static string SQL = "FOR SQL SEARCH";
    }
}