using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;

namespace Sciserver_webService.Common
{
    public class KeyWords
    {
        //// Casjobs REST api 
        public static string CasjobsQuery = "Query";
        public static string CasjobsTaskName = "TaskName";

        //// Authentication Token
        public static string XAuthToken = "X-Auth-Token";

        // this is data release and target setting for all the different services, eg. DR10
        public static string DR = ConfigurationManager.AppSettings["TARGET"];
        /// <summary>
        /// Authentication settings used for the KeyStone authentication settings
        /// </summary>
        public static string xauth = "X-Auth-Token";
        public static string contentJson = "application/json";
        public static string contentXML = "application/xml";
        public static string contentCSV = "text/plain";
        public static string contentVOTable = "application/x-votable+xml";// – VOTABLE XML
        public static string contentFITS = "application/fits"; // – FITS
        public static string contentDataset = "application/x-dataset";// – serialized .NET DataSet

        /// <summary>
        /// Casjobs setting for REST api
        /// </summary>
        public static string casjobsREST = ConfigurationManager.AppSettings["CASJobsREST"];
        //public static string casjobsContextPath = "contexts/" + DR + "/query";
        public static string loggingMessageType = "SKYSERVER_WS";
        /// <summary>
        /// CASJobs old settings
        /// </summary>
        //public static long CJobsWSID = long.Parse(ConfigurationManager.AppSettings["CJobsWSID"]);
        //public static string CJobsPasswd = ConfigurationManager.AppSettings["CJobsPassWD"];
        //public static string CJobsTARGET = DR;
        /// <summary>
        /// 
        /// </summary>
        public static string ConeSelect = ConfigurationManager.AppSettings["ConeSelect"];
        
        ///Maximum number of rows returned by SQExecuteSQL
        public static string MaxRows = ConfigurationManager.AppSettings["MaxRows"];

        ///Data Release
        public static string DataRelease = ConfigurationManager.AppSettings["Release"];

        ///dasUrlBase URL
        public static string dasUrlBase = ConfigurationManager.AppSettings["dasUrlBase"];

        ///dasUrlBase URL
        public static string defaultSpRerun = ConfigurationManager.AppSettings["defaultSpRerun"];




        ///Casjob messages for VOServices
        public static string ConeSearch = "FOR CONE SEARCH";
        public static string SIAPMessage = "FOR SIAP";
        public static string SDSSFIELDSMessage = "FOR SDSS FIELDS";

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
        
        /// <summary>
        /// For different web service type names 
        /// </summary>
        public static string imagingQuery = "img";
        public static string spectroQuery = "spec";
        public static string irspectroQuery = "irspec";
        public static string sqlSearchQuery = "SqlSearch";
        public static string RectangularQuery = "RectangularSearch";
        public static string RadialQuery = "RadialSearch";
        public static string ConeSearchQuery = "ConeSearch";
        public static string SIAP = "SIAP";
        public static string SDSSFields = "SDSSFields";
        

        public static string limit = "limit";
        public static string dataset = "dataset";

        public static string imgparams = "imgparams";        
        public static string specparams = "specparams";
        public static string irspecparams = "irspecparams";

        public static string positionType = "positionType";
        public static string magType = "magType";
        public static string objType = "objType";

        public static string uFilter = "uFilter";
        public static string gFilter = "gFilter";
        public static string rFilter = "rFilter";
        public static string iFilter = "iFilter";
        public static string zFilter = "zFilter";

        public static string cone = "cone";        
        public static string proximity = "proximity";
        public static string noposition = "noposition";
        public static string rectangular = "rectangular";
        // for galactic co-ordinates in IRSpectraQuery 
        public static string conelb = "conelb";       
        //SIAP
        public static string getSIAP = "getSIAP";
        public static string getSIAPInfo = "getSIAP";
        public static string getSIAPInfoAll = "getSIAP";        
        //SDSSFields
        public static string FieldArray = "FieldArray";
        public static string FieldArrayRect = "FieldArrayRect";
        public static string ListOfFields = "ListOfFields";
        public static string UrlsOfFields = "UrlsOfFields";        

        public static string imagingConstraint = "imagingConstraint";
        public static string Lcenter = "Lcenter";
        public static string Bcenter = "Bcenter";
        public static string lbRadius = "lbRadius";      
  
        public static string uMin ="uMin";
        public static string gMin ="gMin";
        public static string rMin ="rMin";
        public static string iMin ="iMin";
        public static string zMin ="zMin";
        public static string uMax ="uMax";
        public static string gMax ="gMax";
        public static string rMax ="rMax";
        public static string iMax ="iMax";
        public static string zMax ="zMax";
        public static string ugMin ="ugMin";
        public static string grMin ="grMin";
        public static string riMin ="riMin";
        public static string izMin ="izMin";
        public static string ugMax ="ugMax";
        public static string grMax ="grMax";
        public static string riMax ="riMax";
        public static string izMax = "izMax";
        public static string jMin = "jMin";
        public static string hMin = "hMin";
        public static string kMin = "kMin";
        public static string jMax = "jMax";
        public static string hMax = "hMax";
        public static string kMax = "kMax";
        public static string snrMin = "snrMin";
        public static string snrMax = "snrMax";
        public static string vhelioMin ="vhelioMin";
        public static string vhelioMax  ="vhelioMax";
        public static string scatterMin ="scatterMin";
        public static string scatterMax = "scatterMax";
        public static string tempMin = "tempMin";
        public static string loggMin = "loggMin";
        public static string tempMax = "tempMax";
        public static string loggMax = "loggMax";
        public static string fehMin = "fehMin";
        public static string fehMax = "fehMax";
        public static string afeMin = "afeMin";
        public static string afeMax = "afeMax";
        public static string addQA = "addQA";
        public static string minQA = "minQA";
        public static string redshiftMin = "redshiftMin";
        public static string redshiftMax = "redshiftMax";
        public static string zWarning = "zWarning";
        public static string classText = "class";
        public static string ALL = "ALL";
        public static string OR = "OR";
        public static string flagsOnList = "flagsOnList";
        public static string flagsOffList = "flagsOffList";
        public static string priFlagsOnList = "priFlagsOnList";
        public static string priFlagsOffList = "priFlagsOffList";
        public static string secFlagsOnList = "secFlagsOnList";
        public static string secFlagsOffList = "secFlagsOffList";
        public static string irTargetFlagsOnList = "irTargetFlagsOnList";
        public static string irTargetFlagsOffList = "irTargetFlagsOffList";
        public static string irTargetFlags2OnList = "irTargetFlags2OnList";
        public static string irTargetFlags2OffList = "irTargetFlags2OffList";
        public static string searchNearBy = "searchNearBy";
        public static string nearby = "nearby";
        public static string ignore = "ignore";
        public static string doGalaxy = "doGalaxy";
        public static string doStar = "doStar";
        public static string doSky = "doSky";
        public static string doUnknown = "doUnknown";


        // this skyserver url
        //public static string skyserverUrl = ConfigurationManager.AppSettings["skyserverUrl"];
        
        
    }
}