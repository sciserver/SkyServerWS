using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;
using System.Data.SqlClient;
using System.Data;
using System.IO;
using System.Net;
using Sciserver_webService.Common;

namespace Sciserver_webService.QueryTools
{
    public class BuildQuery
    {
        static string reSplit = @"(\,|\s+)";
        static string reSplitList = @"(\s*(\,|\s)\s*)";
        public static bool readImgFields(List<string> imgFields, string[] names)
        {
            bool ignoreImg = true;

            foreach (string j in names)
            {
                if (j != "none" && j != "blankImg")
                    ignoreImg = false;
                else
                    continue;
                switch (j)
                {
                    case "minimal":
                        addField(imgFields, "run");
                        addField(imgFields, "rerun");
                        addField(imgFields, "camCol");
                        addField(imgFields, "field");
                        addField(imgFields, "obj");
                        break;
                    case "typical":
                        addField(imgFields, "run");
                        addField(imgFields, "rerun");
                        addField(imgFields, "camCol");
                        addField(imgFields, "field");
                        addField(imgFields, "obj");
                        addField(imgFields, "ra");
                        addField(imgFields, "[dec]");
                        addField(imgFields, "camCol");
                        addField(imgFields, "r");
                        break;
                    case "radec":
                        addField(imgFields, "ra");
                        addField(imgFields, "[dec]");
                        break;
                    case "model_mags":
                        addField(imgFields, "u");
                        addField(imgFields, "g");
                        addField(imgFields, "r");
                        addField(imgFields, "i");
                        addField(imgFields, "z");
                        break;
                    case "model_magerrs":
                        addField(imgFields, "modelMagErr_u");
                        addField(imgFields, "modelMagErr_g");
                        addField(imgFields, "modelMagErr_r");
                        addField(imgFields, "modelMagErr_i");
                        addField(imgFields, "modelMagErr_z");
                        break;
                    case "psf_mags":
                        addField(imgFields, "psfMag_u");
                        addField(imgFields, "psfMag_g");
                        addField(imgFields, "psfMag_r");
                        addField(imgFields, "psfMag_i");
                        addField(imgFields, "psfMag_z");
                        break;
                    case "psf_magerrs":
                        addField(imgFields, "psfMagErr_u");
                        addField(imgFields, "psfMagErr_g");
                        addField(imgFields, "psfMagErr_r");
                        addField(imgFields, "psfMagErr_i");
                        addField(imgFields, "psfMagErr_z");
                        break;
                    case "petro_mags":
                        addField(imgFields, "petroMag_u");
                        addField(imgFields, "petroMag_g");
                        addField(imgFields, "petroMag_r");
                        addField(imgFields, "petroMag_i");
                        addField(imgFields, "petroMag_z");
                        break;
                    case "petro_magerrs":
                        addField(imgFields, "petroMagErr_u");
                        addField(imgFields, "petroMagErr_g");
                        addField(imgFields, "petroMagErr_r");
                        addField(imgFields, "petroMagErr_i");
                        addField(imgFields, "petroMagErr_z");
                        break;
                    case "SDSSname":
                        addField(imgFields, "SDSSname");
                        break;
                    default:
                        addField(imgFields, j);
                        break;
                }
            }
            return ignoreImg;
        }

        // Read in the spectro (SpecObj) fields specified by the user and store them in an array
        // for stuffing into the SELECT later.
        public static bool readSpecFields(List<string> specFields, string[] names)
        {
            var ignoreSpec = true;
            
            foreach (string j in names)
            {
                if (j == "none" && names.Length == 1)
                    ignoreSpec = true;
                else
                {
                    ignoreSpec = false;
                    if ((j != "none"))
                    {
                        if (j == "minimal")
                        {
                            addField(specFields, "plate");
                            addField(specFields, "mjd");
                            addField(specFields, "fiberid");
                        }
                        else if (j == "typical")
                        {
                            addField(specFields, "plate");
                            addField(specFields, "mjd");
                            addField(specFields, "fiberid");
                            addField(specFields, "z");
                            addField(specFields, "zErr");
                            addField(specFields, "zWarning");
                            addField(specFields, "class");
                        }
                        else if (j == "radec")
                        {
                            addField(specFields, "ra");
                            addField(specFields, "[dec]");
                        }
                        else
                        {
                            if (j != "bestdb" && j != "blankSpec")
                                addField(specFields, j);
                        }
                    }
                }
            }
            return ignoreSpec;
        }

        // Read in the infrared spectra (apogeeStar) fields specified by the user and store them in an array
        // for stuffing into the SELECT later.
        public static bool readIRspecFields(List<string> IRspecFields, string[] names)
        {
            var ignoreIRspec = true;
            
            foreach (string j in names)
            {
                if (j == "none" && names.Length == 1)
                    ignoreIRspec = true;
                else
                {
                    ignoreIRspec = false;
                    if ((j != "none"))
                    {
                        if (j == "minimal")
                        {
                            addField(IRspecFields, "apogee_id");
                        }
                        else if (j == "typical")
                        {
                            addField(IRspecFields, "apogee_id");
                            addField(IRspecFields, "glon");
                            addField(IRspecFields, "glat");
                            addField(IRspecFields, "snr");
                            addField(IRspecFields, "vhelio_avg");
                            addField(IRspecFields, "vscatter");
                            addField(IRspecFields, "teff");
                            addField(IRspecFields, "logg");
                            if(datarelease >= 12)
                                addField(IRspecFields, "param_m_h");
                            else
                                addField(IRspecFields, "metals");

                            if(datarelease >= 12)
                                addField(IRspecFields, "param_alpha_m");
                            else
                                addField(IRspecFields, "alphafe");
                        }
                        else if (j == "twomassj") { addField(IRspecFields, "j"); }
                        else if (j == "twomassh") { addField(IRspecFields, "h"); }
                        else if (j == "twomassk") { addField(IRspecFields, "k"); }
                        else
                        {
                            addField(IRspecFields, j);
                        }
                    }
                }
            }
            return ignoreIRspec;
        }

        public static bool addField(List<string> list, string field)
        {
            if (list.Contains(field))
            {
                return false;
            }
            else
            {
                list.Add(field);
                return true;
            }
        }

        public static bool checkRect(double raMin, double raMax, double decMin, double decMax)
        {
            // Check for valid parameters
            bool error = false;

            error = error || Utilities.valueCheck("min_ra", raMin, 0, 360);
            error = error || Utilities.valueCheck("raMax", raMax, 0, 360);
            error = error || Utilities.rangeCheck("dec", decMin, decMax, -90, 90);

            double delta_ra = raMax - raMin;
            double delta_dec = decMax - decMin;

            error = error || Utilities.valueCheck("raMax-raMin", delta_ra, 0, 5.0);
            error = error || Utilities.valueCheck("decMax-decMin", delta_dec, 0, 5.0);

            if (error == false)
                return true;
            else
                return false;
        }

        public static string magLimits(string name, string val, string prefix, string magType)
        {
            string constraint = "";
            if (magType.Length > 0 && magType != "model")
                prefix += "." + magType + "Mag_";
            else
                prefix += ".";

            switch (name)
            {
                // Now process the individual constraints.
                case "uMin":
                    constraint = " " + prefix + "u > " + val;
                    break;
                case "gMin":
                    constraint = " " + prefix + "g > " + val;
                    break;
                case "rMin":
                    constraint = " " + prefix + "r > " + val;
                    break;
                case "iMin":
                    constraint = " " + prefix + "i > " + val;
                    break;
                case "zMin":
                    constraint = " " + prefix + "z > " + val;
                    break;
                case "jMin":
                    constraint = " " + prefix + "j > " + val;
                    break;
                case "hMin":
                    constraint = " " + prefix + "h > " + val;
                    break;
                case "kMin":
                    constraint = " " + prefix + "k > " + val;
                    break;
                case "uMax":
                    constraint = " " + prefix + "u < " + val;
                    break;
                case "gMax":
                    constraint = " " + prefix + "g < " + val;
                    break;
                case "rMax":
                    constraint = " " + prefix + "r < " + val;
                    break;
                case "iMax":
                    constraint = " " + prefix + "i < " + val;
                    break;
                case "zMax":
                    constraint = " " + prefix + "z < " + val;
                    break;
                case "jMax":
                    constraint = " " + prefix + "j < " + val;
                    break;
                case "hMax":
                    constraint = " " + prefix + "h < " + val;
                    break;
                case "kMax":
                    constraint = " " + prefix + "k < " + val;
                    break;
                case "ugMin":
                    constraint = " (" + prefix + "u - " + prefix + "g) > " + val;
                    break;
                case "grMin":
                    constraint = " (" + prefix + "g - " + prefix + "r) > " + val;
                    break;
                case "riMin":
                    constraint = " (" + prefix + "r - " + prefix + "i) > " + val;
                    break;
                case "izMin":
                    constraint = " (" + prefix + "i - " + prefix + "z) > " + val;
                    break;
                case "ugMax":
                    constraint = " (" + prefix + "u - " + prefix + "g) < " + val;
                    break;
                case "grMax":
                    constraint = " (" + prefix + "g - " + prefix + "r) < " + val;
                    break;
                case "riMax":
                    constraint = " (" + prefix + "r - " + prefix + "i) < " + val;
                    break;
                case "izMax":
                    constraint = " (" + prefix + "i - " + prefix + "z) < " + val;
                    break;
                case "jhMin":
                    constraint = " (" + prefix + "j - " + prefix + "h) > " + val;
                    break;
                case "hkMin":
                    constraint = " (" + prefix + "h - " + prefix + "k) > " + val;
                    break;
                case "jhMax":
                    constraint = " (" + prefix + "j - " + prefix + "h) < " + val;
                    break;
                case "hkMax":
                    constraint = " (" + prefix + "h - " + prefix + "k) < " + val;
                    break;
                default:
                    break;
            }
            return constraint;
        }

        public static string IRspecParamLimits(string name, string val, string prefix)
        {
            string constraint = "";

            switch (name)
            {
                // Now process the individual constraints.
                case "snrMin":
                    constraint = " " + prefix + ".snr > " + val;
                    break;
                case "snrMax":
                    constraint = " " + prefix + ".snr > " + val;
                    break;
                case "vhelioMin":
                    constraint = " " + prefix + ".vhelio_avg > " + val;
                    break;
                case "vhelioMax":
                    constraint = " " + prefix + ".vhelio_avg < " + val;
                    break;
                case "scatterMin":
                    constraint = " " + prefix + ".vscatter > " + val;
                    break;
                case "scatterMax":
                    constraint = " " + prefix + ".vscatter < " + val;
                    break;
                case "tempMin":
                    constraint = " " + prefix + ".teff > " + val;
                    break;
                case "tempMax":
                    constraint = " " + prefix + ".teff < " + val;
                    break;
                case "loggMin":
                    constraint = " " + prefix + ".logg > " + val;
                    break;
                case "loggMax":
                    constraint = " " + prefix + ".logg < " + val;
                    break;
                case "fehMin":
                    if(datarelease >=12)
                        constraint = " " + prefix + ".param_m_h > " + val;
                    else
                        constraint = " " + prefix + ".metals > " + val;
                    break;
                case "fehMax":
                    if(datarelease  >=12)
                        constraint = " " + prefix + ".param_m_h < " + val;
                    else
                        constraint = " " + prefix + ".metals < " + val;
                    break;
                case "afeMin":
                    if (datarelease >= 12)
                        constraint = " " + prefix + ".param_alpha_m > " + val;
                    else
                        constraint = " " + prefix + ".alphafe > " + val;
                    break;
                case "afeMax":
                    if (datarelease >= 12)
                        constraint = " " + prefix + ".param_alpha_m < " + val;
                    else
                        constraint = " " + prefix + ".alphafe < " + val;
                    break;
                default:
                    break;
            }
            return constraint;
        }

        public static string[] getOptions(string val)
        {
            return val.Split(',');
        }

        private static string getRectangularJoin(ref string cmd, bool targdb, string type, string tableAlias, double raMin, double raMax, double decMin, double decMax)
        {

            string joinClause = "";
                

                if (!checkRect(raMin, raMax, decMin, decMax))
                {
                    cmd = "ERROR: Illegal rectangular search values: " + raMin + "," + raMax + "," + decMin + "," + decMax + ".";
                    return cmd;
                }
                else
                {
                    joinClause += " \nJOIN ";
                    if (targdb)
                        joinClause += " TARG" + KeyWords.Release + ".";
                    joinClause += "dbo.fGetObjFromRect(" + raMin + "," + raMax + "," + decMin + "," + decMax + ") AS b ON ";
                    if (type == "spec")
                        joinClause += "s.bestobjid = b.objID";
                    else if (type == "irspec")
                        joinClause += apogeeAlias + ".apstar_id=b.apstar_id";
                    else
                        joinClause += tableAlias + ".objID = b.objID ";
                }
                return joinClause;
            
        }

        private static string getConeJoin(string type, bool targdb, string raCenter, string decCenter, string radius)
        {
            string joinClause = " \nJOIN ";
            if (targdb)
                joinClause += " TARG" + KeyWords.Release + ".";
            double ra = Utilities.parseRA(raCenter);
            if (type == "irspec")
            {
                joinClause += "dbo.fGetNearbyApogeeStarEq(" + ra + ",";
            }
            else if (type == "spec")
            {
                joinClause += "dbo.fGetNearbySpecObjEq(" + ra + ",";
            }
            else
            {
                joinClause += "dbo.fGetNearbyObjEq(" + ra + ",";
            }

            double dec = Utilities.parseDec(decCenter);
            joinClause += dec + ",";
            joinClause += radius + ") AS b ON ";
            if (type == "irspec")
            {
                joinClause += " b.apstar_id = " + apogeeAlias + ".apstar_id";
            }
            else if (type == "spec")
            {
                joinClause += " b.SpecobjID = S.SpecobjID";
            }
            else
            {
                joinClause += " b.objID = P.objID";
            }



            return joinClause;
        }

        private static Dictionary<string, string> getDictionary(string positiontype, string type, Dictionary<string, string> requestdictionary)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            dictionary.Add("positionType", positiontype);
            dictionary.Add("limit", "50");
            switch(type){
                case "img" :     
                    dictionary.Add("imgparams", "minimal");
                    dictionary.Add("specparams", "none");
                    break;
                case "spec" :
                    dictionary.Add("imgparams", "none");
                    dictionary.Add("specparams", "minimal");
                    dictionary.Add("zWarning", "0");
                    break;
                case "irspec":
                    dictionary.Add("irspecparams", "typical");
                    break;
            }
            foreach (string s in dictionary.Keys)
            {
                if (!requestdictionary.Keys.Contains(s))
                {
                    requestdictionary.Add(s, dictionary[s]);
                };
            }
            return requestdictionary;
        }
                    
        public static void getProximityQueryText(ref string selectClause, ref string joinClause, ref string orderClause,  string type,  List<string> proxList,List<string> objType, double proxRad, Boolean targdb,string nearBy)
        {
            
            if (type == "spec")
            {
                selectClause += " s.ra,s.dec,";
                joinClause += " \nJOIN #x AS x ON x.SpecobjID=s.SpecobjID \nJOIN #upload AS u ON u.up_id = x.up_id ";
                orderClause += " \nORDER BY x.up_id ";
            }
            else
            {
                selectClause += " p.ra,p.dec, ";
                joinClause += " \nJOIN #x AS x ON x.objID=p.objID \nJOIN #upload AS u ON u.up_id = x.up_id ";
                orderClause += " \nORDER BY x.up_id ";
            }
        }

        private static List<string> getObjtype(string val)
        {
            String[] vals = getOptions(val);
            List<string> objType = new List<string>();

            for (int i = 0; i < vals.Length; i++)
            {
                switch (vals[i])
                {
                    case "doGalaxy":
                        doGalaxy = true;
                        objType.Add("3");
                        break;

                    case "doStar":
                        doStar = true;
                        objType.Add("6");
                        break;

                    case "doSky":
                        doSky = true;
                        objType.Add("8");
                        break;

                    case "doUnknown":
                        doUnknown = true;
                        objType.Add("0");
                        break;
                }
            }
            return objType;
        }



        public static string query = "";
        public static string QueryForUserDisplay = "";
        public static void buildQueryMaster(string type, Dictionary<string, string> requestDictionary, string positionType)
        {
            Int64 limit;
            try
            {
                limit = Convert.ToInt64(requestDictionary["limit"]);
            }
            catch { throw (new Exception("Invalid numerical value for maximum number of rows in LIMIT=" + requestDictionary["limit"])); }
            if (limit > Convert.ToInt64(KeyWords.MaxRows))
            {
                throw (new Exception("Numerical value for maximum number of rows is out of range in LIMIT=" + requestDictionary["limit"] + ". Maximum number of rows allowed is " + Convert.ToInt64(KeyWords.MaxRows) + "."));
            }
            QueryForUserDisplay = buildQuery(type, requestDictionary, positionType);
            query = QueryForUserDisplay;
        }


        private static bool doStar = false, doGalaxy = false, doSky = false, doUnknown = false;
        private static bool ignoreImg = false, ignoreSpec = false, ignoreIRspec = false;
        private static string specAlias = "s";
        private static string bestAlias = "p";
        private static string targAlias = "t";
        private static string apogeeAlias = "a";
        private static string aspcapAlias = "q";
        private static string apogeeObjectAlias = "o";
        private static int datarelease = 0;
        public static string buildQuery(string type, Dictionary<string, string> requestDictionary, string positionType)
        {

            datarelease = Convert.ToInt32(requestDictionary["datarelease"]);
            Dictionary<string,string> dictionary = getDictionary(positionType,type,requestDictionary);
            
            string cmd = "";            

            List<string> objType = new List<string>();
            List<string> proxList = new List<string>();
            List<string> imgFields = new List<string>();
            List<string> specFields = new List<string>();
            List<string> IRspecFields = new List<string>();
            
            string orderClause = "";        
           
            
            //string proxMode = " ";
            bool addQA = false;
            string nearBy = "";
            string[] options;
            string selectClause = "\nSELECT ";
            string fromClause = "\nFROM ";
            if ("spec".Equals(type)) fromClause += KeyWords.Database + "..SpecObj as " + specAlias;
            if ("irspec".Equals(type)) fromClause += "apogeeStar as " + apogeeAlias;
            string whereClause = "\nWHERE ";
            string filters = "";
            bool bestdb = true, targdb = false;
            string posType = "cone";
            bool imgConst = false;

            string tableAlias = bestAlias;
            string specTypes = "", magType = "model";
            string joinClause = "";
            string photoTable = "PhotoObj";
            string apogeeTable = "apogeeStar";
            string apogeeObjectTable = "apogeeObject";
            string aspcapTable = "aspcapStar";
            //double raMin = 0, raMax = 0, decMin = 0, decMax = 0;
            double proxRad = 0.0;
            //int t = 0;
            string flagsOn = "", flagsOff = "", priFlagsOn = "", priFlagsOff = "", secFlagsOn = "", secFlagsOff = "";
            string irTargetFlagsOn = "", irTargetFlagsOff = "";
            string irTargetFlags2On = "", irTargetFlags2Off = "";
            string specJoin = "";

            // these variables are used to convert from (L,B) to (RA,dec)
            double Lval = 0;
            double Bval = 0;
            //double calculatedRA = 0;
            //double calculatedDec = 0;

            
            // This needs to go uncommented if the selectClause is not passed to CasJOBS inside SpExecuteSQL.
            Int64 limit;
            try
            {
                limit = Convert.ToInt64(dictionary["limit"]);
                limit = (limit <= 0) ? Convert.ToInt64(KeyWords.MaxRows) : limit;
                selectClause += "TOP " + limit.ToString() + " \n";
            }
            catch (Exception e) { selectClause += "TOP " + dictionary["limit"] + " \n"; }
            



            //KeyValuePair<string, string> entry in dictionary
            foreach (string s in dictionary.Keys)
            {
                string name = s;
                string val = dictionary[s];
                if (val == null || "".Equals(val)) continue;

                switch (name)
                {

                    case "imgparams":
                        options = getOptions(val);
                        ignoreImg = readImgFields(imgFields, options);
                        break;
                    case "irspecparams":
                        options = getOptions(val);
                        ignoreIRspec = readIRspecFields(IRspecFields, options);
                        break;
                    case "dataset":
                        if (val == "targdb")
                        {
                            targdb = true;
                            bestdb = false;
                        }
                        break;
                    case "specparams":
                        options = getOptions(val);
                        ignoreSpec = readSpecFields(specFields, options);
                        break;
                    case "uFilter":
                        filters += "u";
                        break;
                    case "gFilter":
                        filters += "g";
                        break;
                    case "rFilter":
                        filters += "r";
                        break;
                    case "iFilter":
                        filters += "i";
                        break;
                    case "zFilter":
                        filters += "z";
                        break;
                    case "positionType":
                        posType = val;
                        if (posType.Equals("cone"))
                        {

                            joinClause += getConeJoin(type, false, dictionary["ra"], dictionary["dec"], dictionary["radius"]);

                        }
                        else if (posType.Equals("rectangular"))
                        {
                            double raMin = 0, raMax = 0, decMin = 0, decMax = 0;

                            raMin = Utilities.parseRA(dictionary["raMin"]);
                            raMax = Utilities.parseRA(dictionary["raMax"]);
                            decMin = Utilities.parseDec(dictionary["decMin"]);
                            decMax = Utilities.parseDec(dictionary["decMax"]);

                            joinClause += getRectangularJoin(ref cmd, targdb, type, tableAlias, raMin, raMax, decMin, decMax);
                        }
                        else if(posType.Equals("proximity")){                            
                            //string[] paste;
                            //paste = Regex.Split(dictionary["radecTextarea"], reSplit);
                            //if (paste.Length > 0)
                            //{
                            //    proxList.AddRange(val.Split(new string[] { "\n", "\r\n" }, StringSplitOptions.None));
                            //    proxMode = "text";
                            //}                                                    
                            //proxRad = double.Parse(dictionary["radiusDefault"]);
                            getProximityQueryText(ref selectClause,ref joinClause,ref orderClause, type, proxList, objType, proxRad, targdb, nearBy);

                        }
                        else if (posType == "conelb")
                        {
                            Lval = Utilities.parseRA(dictionary["Lcenter"]);
                            Bval = Utilities.parseDec(dictionary["Bcenter"]);
                            double convertedRA = Utilities.glon2ra(Lval, Bval);
                            double convertedDec = Utilities.glat2dec(Lval, Bval);
                            joinClause += " \nJOIN dbo.fgetNearbyApogeeStarEq(" + convertedRA;
                            joinClause += "," + convertedDec + ",";
                            joinClause += dictionary["lbRadius"] + ") AS b ON b.apstar_id = " + apogeeAlias + ".apstar_id";
                        }
                        break;
                    case "imagingConstraint":
                        imgConst = true;
                        if (val == "target")
                            tableAlias = targAlias;
                        else
                            tableAlias = bestAlias;
                        break;
                    case "magType":
                        magType = val;
                        break;                                     

                    case "uMin":
                    case "gMin":
                    case "rMin":
                    case "iMin":
                    case "zMin":
                    case "uMax":
                    case "gMax":
                    case "rMax":
                    case "iMax":
                    case "zMax":
                    case "ugMin":
                    case "grMin":
                    case "riMin":
                    case "izMin":
                    case "ugMax":
                    case "grMax":
                    case "riMax":
                    case "izMax":
                        // For all the constraints, first check if this is first constraint in WHERE;
                        // if not, prepend an AND.
                        if (val.Length == 0)
                            break;
                        if (name != "raMin" && name != "raMax" && name != "decMin" && name != "decMax")
                            // all non-pos constraints constitute imaging constraints
                            imgConst = true;	       // so set flag to true
                        if (whereClause.Length > 8)// 8 is the length of the initial string "WHERE "
                            whereClause += " AND";
                            whereClause += magLimits(name, val, tableAlias, magType);
                        break;

                    // apogee related
                    case "jMin":
                    case "hMin":
                    case "kMin":
                    case "jMax":
                    case "hMax":
                    case "kMax":
                        tableAlias = apogeeObjectAlias;   // check the APOGEE table
                        magType = "";
                        // For all the constraints, first check if this is first constraint in WHERE;
                        // if not, prepend an AND.
                        if (val.Length == 0)
                        break;
                            imgConst = true;
                        if (whereClause.Length > 8)
                            whereClause += " AND";
                            whereClause += magLimits(name, val, tableAlias, magType);
                        break;

                    case "snrMin":
                    case "snrMax":
                    case "vhelioMin":
                    case "vhelioMax":
                    case "scatterMin":
                    case "scatterMax":
                        tableAlias = apogeeAlias;   // check the APOGEE table
                        // For all the constraints, first check if this is first constraint in WHERE;
                        // if not, prepend an AND.
                        if (val.Length == 0)
                            break;
                        imgConst = true;
                        if (whereClause.Length > 8)
                            whereClause += " AND";
                            whereClause += IRspecParamLimits(name, val, tableAlias);
                        break;

                    case "tempMin":
                    case "loggMin":
                    case "tempMax":
                    case "loggMax":
                    case "fehMin":
                    case "fehMax":
                    case "afeMin":
                    case "afeMax":
                        tableAlias = aspcapAlias;   // check the aspcapStar table
                        // For all the constraints, first check if this is first constraint in WHERE;
                        // if not, prepend an AND.
                        if (val.Length == 0)
                            break;
                        imgConst = true;
                        if (whereClause.Length > 8)
                            whereClause += " AND";
                        whereClause += IRspecParamLimits(name, val, tableAlias);
                        break;

                    case "objType":
                        objType = getObjtype(val);
                        break;

                    case "addQA":
                        if (val == "on")
                        {
                            addQA = true;
                        }
                        break;
                    case "minQA":
                        // For all the constraints, first check if this is first constraint in WHERE;
                        // if not, prepend an AND.
                        if (val.Length == 0)
                            break;
                        if (whereClause.Length > 8)
                            whereClause += " AND ";
                        whereClause += "(p.score >= " + val + ")";
                        break;
                    case "redshiftMin":
                        if (whereClause.Length > 8)
                            whereClause += " AND";
                        whereClause += " s.z > " + val;
                        break;
                    case "redshiftMax":
                        if (whereClause.Length > 8)
                            whereClause += " AND";
                        whereClause += " s.z < " + val;
                        break;

                    case "zWarning":                        
                        if (whereClause.Length > 8)
                                whereClause += " AND";
                        whereClause += " s.zWarning = 0";                        
                        break;

                    case "class":
                        options = getOptions(val);
                        foreach (string j in options)
                        {
                            if (j == "ALL")
                            {
                                specTypes = "";
                                break;
                            }
                            else
                            {
                                if (specTypes.Length > 0)
                                    specTypes += " OR ";
                                specTypes += "s.class = '" + j + "'";
                            }
                        }
                        if (specTypes.Length > 0)
                        {
                            if (whereClause.Length > 8)
                                whereClause += " AND";
                            whereClause += " (" + specTypes + ")";
                        }
                        break;

                    case "flagsOnList":
                        options = getOptions(val);
                        foreach (string j in options)
                        {
                            if (j != "ignore")
                            {
                                if (flagsOn.Length > 0)
                                    flagsOn += " + ";
                                flagsOn += "dbo.fPhotoFlags('" + j + "')";
                            }
                        }
                        break;
                    case "flagsOffList":
                        options = getOptions(val);
                        foreach (string j in options)
                        {
                            if (j != "ignore")
                            {
                                if (flagsOff.Length > 0)
                                    flagsOff += " + ";
                                flagsOff += "dbo.fPhotoFlags('" + j + "')";
                            }
                        }
                        break;
                    case "priFlagsOnList":
                        options = getOptions(val);
                        foreach (string j in options)
                        {
                            if (j != "ignore")
                            {
                                if (priFlagsOn.Length > 0)
                                    priFlagsOn += " + ";
                                priFlagsOn += "dbo.fPrimTarget('" + j + "')";
                            }
                        }
                        break;
                    case "priFlagsOffList":
                        options = getOptions(val);
                        foreach (string j in options)
                        {
                            if (j != "ignore")
                            {
                                if (priFlagsOff.Length > 0)
                                    priFlagsOff += " + ";
                                priFlagsOff += "dbo.fPrimTarget('" + j + "')";
                            }
                        }
                        break;
                    case "secFlagsOnList":
                        options = getOptions(val);
                        foreach (string j in options)
                        {
                            if (j != "ignore")
                            {
                                if (secFlagsOn.Length > 0)
                                    secFlagsOn += " + ";
                                secFlagsOn += "dbo.fSecTarget('" + j + "')";
                            }
                        }
                        break;
                    case "secFlagsOffList":
                        options = getOptions(val);
                        foreach (string j in options)
                        {
                            if (j != "ignore")
                            {
                                if (secFlagsOff.Length > 0)
                                    secFlagsOff += " + ";
                                secFlagsOff += "dbo.fSecTarget('" + j + "')";
                            }
                        }
                        break;
                    case "irTargetFlagsOnList":
                        options = getOptions(val);
                        foreach (string j in options)
                        {
                            if (j != "ignore")
                            {
                                if (irTargetFlagsOn.Length > 0)
                                    irTargetFlagsOn += " + ";
                                irTargetFlagsOn += "dbo.fApogeeTarget1('" + j + "')";
                            }
                        }
                        break;
                    case "irTargetFlagsOffList":
                        options = getOptions(val);
                        foreach (string j in options)
                        {
                            if (j != "ignore")
                            {
                                if (irTargetFlagsOff.Length > 0)
                                    irTargetFlagsOff += " + ";
                                irTargetFlagsOff += "dbo.fApogeeTarget1('" + j + "')";
                            }
                        }
                        break;

                    case "irTargetFlags2OnList":
                        options = getOptions(val);
                        foreach (string j in options)
                        {
                            if (j != "ignore")
                            {
                                if (irTargetFlags2On.Length > 0)
                                    irTargetFlags2On += " + ";
                                irTargetFlags2On += "dbo.fApogeeTarget2('" + j + "')";
                            }
                        }
                        break;
                    case "irTargetFlags2OffList":
                        options = getOptions(val);
                        foreach (string j in options)
                        {
                            if (j != "ignore")
                            {
                                if (irTargetFlags2Off.Length > 0)
                                    irTargetFlags2Off += " + ";
                                irTargetFlags2Off += "dbo.fApogeeTarget2('" + j + "')";
                            }
                        }
                        break;

                    case "searchNearBy":
                        if (val == "nearby")
                        {
                            nearBy = val;
                        }
                        break;
                    
                    default:
                        break;
                }

            }
            if (doStar & !doGalaxy && !doSky && !doUnknown)
            {
                photoTable = "Star";
            }
            else if (doGalaxy && !doStar && !doSky && !doUnknown)
            {
                photoTable = "Galaxy";
            }
            else if (doSky && !doGalaxy && !doStar && !doUnknown)
            {
                photoTable = "Sky";
            }
            else if (doUnknown && !doGalaxy && !doStar && !doSky)
            {
                photoTable = "Unknown";
            }
            else if (!ignoreImg)
            {
                photoTable = "PhotoObj";
                if (objType.Count > 0)
                {
                    if (whereClause.Length > 8)
                        whereClause += " AND";
                    whereClause += " (";
                    for (int i = 0; i < objType.Count; i++)
                    {
                        string t = objType[i];
                        whereClause += " " + tableAlias + ".type = " + t;
                        if (i < objType.Count - 1)
                            whereClause += " OR";
                    }
                    whereClause += ")";
                }
            }

            if (!ignoreIRspec)
            {
                apogeeTable = "apogeeStar";
            }

            if (flagsOff.Length > 0)
            {
                if (whereClause.Length > 8)
                    whereClause += " AND";
                whereClause += " (" + tableAlias + ".flags & (" + flagsOff + ") = 0)";
            }
            if (flagsOn.Length > 0)
            {
                if (whereClause.Length > 8)
                    whereClause += " AND";
                whereClause += " (" + tableAlias + ".flags & (" + flagsOn + ") > 0)";
            }
            if (priFlagsOff.Length > 0)
            {
                if (whereClause.Length > 8)
                    whereClause += " AND";
                whereClause += " (" + specAlias + ".primTarget & (" + priFlagsOff + ") = 0)";
            }
            if (priFlagsOn.Length > 0)
            {
                if (whereClause.Length > 8)
                    whereClause += " AND";
                whereClause += " (" + specAlias + ".primTarget & (" + priFlagsOn + ") > 0)";
            }
            if (secFlagsOff.Length > 0)
            {
                if (whereClause.Length > 8)
                    whereClause += " AND";
                whereClause += " (" + specAlias + ".secTarget & (" + secFlagsOff + ") = 0)";
            }
            if (secFlagsOn.Length > 0)
            {
                if (whereClause.Length > 8)
                    whereClause += " AND";
                whereClause += " (" + specAlias + ".secTarget & (" + secFlagsOn + ") > 0)";
            }
            if (irTargetFlagsOff.Length > 0)
            {
                if (whereClause.Length > 8)
                    whereClause += " AND";
                whereClause += " (" + apogeeAlias + ".apogee_target1 & (" + irTargetFlagsOff + ") = 0)";
            }
            if (irTargetFlagsOn.Length > 0)
            {
                if (whereClause.Length > 8)
                    whereClause += " AND";
                whereClause += " (" + apogeeAlias + ".apogee_target1 & (" + irTargetFlagsOn + ") != 0)";
            }
            if (irTargetFlags2Off.Length > 0)
            {
                if (whereClause.Length > 8)
                    whereClause += " AND";
                whereClause += " (" + apogeeAlias + ".apogee_target1 & (" + irTargetFlags2Off + ") = 0)";
            }
            if (irTargetFlags2On.Length > 0)
            {
                if (whereClause.Length > 8)
                    whereClause += " AND";
                whereClause += " (" + apogeeAlias + ".apogee_target1 & (" + irTargetFlags2On + ") != 0)";
            }
            // Put the pieces of the query together.
                
            if (type == "spec")
            {
                if (specFields.Count == 0)
                {
                    string[] specOpts = new string[] { "minimal" };
                    //specOpts[0] = "minimal";
                    readSpecFields(specFields, specOpts);
                }
                for (int i = 0; i < specFields.Count; i++)
                {
                    if (specFields[i] == "ra")
                    {
                        selectClause += "cast(str(s." + specFields[i] + ",13,8) as float) as ra";
                    }
                    else if (specFields[i] == "[dec]")
                    {
                        selectClause += "cast(str(s." + specFields[i] + ",13,8) as float) as dec";
                    }
                    else
                    {
                        selectClause += "s." + specFields[i];
                    }
                    if (i < specFields.Count - 1)
                        selectClause += ",";
                }
                if (targdb && !ignoreImg)
                    specJoin = " ON s.targetObjID = p.objID";
                else if (!targdb && !ignoreImg)
                    specJoin = " ON s.bestObjID = p.objID";
                if (imgConst == true && ignoreImg)
                {
                    // if imaging constraints are specified, add a photo table join
                    //joinClause += "\n\tJOIN " + photoTable + " AS " + tableAlias + " ON ";
                    joinClause += " \nJOIN " + photoTable + " AS " + tableAlias + " ON ";
                    if (targdb)
                        joinClause += "s.targetobjid=" + tableAlias + ".objid";
                    else
                        joinClause += "s.bestobjid=" + tableAlias + ".objid";
                }
                if (!ignoreImg)
                {
                    selectClause += ",";
                    selectClause = buildSelect(bestdb, targdb, photoTable, imgFields, specJoin, selectClause);
                    fromClause = buildFrom(bestdb, targdb, photoTable, imgFields, specJoin, fromClause);
                }
            }
            else if (type == "irspec")
            {
                selectClause = buildSelectApogee(IRspecFields, specJoin, selectClause);
                joinClause += " \nJOIN ";
                joinClause += apogeeObjectTable + " as " + apogeeObjectAlias;
                joinClause += " ON " + apogeeAlias + ".apogee_id=" + apogeeObjectAlias + ".apogee_id";
                joinClause += " \nJOIN ";
                joinClause += aspcapTable + " as " + aspcapAlias;
                joinClause += " ON " + apogeeAlias + ".apstar_id=" + aspcapAlias + ".apstar_id";
            }
            else
            {
                selectClause = buildSelect(bestdb, targdb, photoTable, imgFields, "", selectClause);
                fromClause = buildFrom(bestdb, targdb, photoTable, imgFields, "", fromClause);
                if (!ignoreSpec)
                {
                    fromClause += " \nLEFT OUTER JOIN " + KeyWords.Database + "..SpecObj s ON p.objID = s.bestObjID";
                    for (int i = 0; i < specFields.Count; i++)
                    {
                        if (specFields[i] == "ra")
                        {
                            selectClause += ",ISNULL(cast(str(s." + specFields[i] + ",13,8) as float),0) as ra";
                        }
                        else if (specFields[i] == "[dec]")
                        {
                            selectClause += ",ISNULL(cast(str(s." + specFields[i] + ",13,8) as float),0) as dec";
                        }
                        else if (specFields[i] == "z")
                        {
                            selectClause += ",ISNULL(s." + specFields[i] + ",0) as redshift";
                        }
                        else if (specFields[i] == "zErr")
                        {
                            selectClause += ",ISNULL(s." + specFields[i] + ",0) as redshiftErr";
                        }
                        else
                        {
                            selectClause += ",ISNULL(s." + specFields[i] + ",0) as " + specFields[i];
                        }
                    }
                }
            }
            if (addQA)
                selectClause += ", p.score as score";
            if (filters.Length > 0)
                selectClause += ", '" + filters + "' as filter";
            if (whereClause.Length <= 8)
                whereClause = "";
                cmd += selectClause + " " + fromClause + joinClause + " " + whereClause + " " + orderClause;
            
            return cmd;
        }
               
        public static string buildFrom(bool bestdb, bool targdb, string photoTable, List<string> imgFields, string joinCond, string fromClause)
        {
            string bestAlias = "p", targAlias = "t";
            if (bestdb)
            {
                if (fromClause.Length > 7)// 7 is the length of the initial string "FROM "
                    fromClause += " \nJOIN ";
                fromClause += KeyWords.Database + ".." + photoTable + " AS " + bestAlias + joinCond;
            }
            else
            {
                if (fromClause.Length > 7)// 7 is the length of the initial string "FROM "
                    fromClause += " \nJOIN ";
                fromClause += " TARG" + KeyWords.Release + ".." + photoTable + " AS " + bestAlias + joinCond;
            }

            return fromClause;
        }

        public static string buildSelectApogee(List<string> theFields, string joinCond, string selectClause)
        {
            for (int i = 0; i < theFields.Count; i++)
            {
                if (theFields[i] == "teff" | theFields[i] == "logg" | theFields[i] == "metals" | theFields[i]=="param_m_h" | theFields[i] == "cfe" | theFields[i] == "nfe" | theFields[i] == "alphafe" | theFields[i] == "param_alpha_m")
                {
                    selectClause += addImgSelect(theFields[i], "q");
                }
                else if (theFields[i] == "j" | theFields[i] == "h" | theFields[i] == "k")
                {
                    selectClause += addImgSelect(theFields[i], "o");
                }
                else if (theFields[i] == "glon" | theFields[i] == "glat")
                {
                    selectClause += addImgSelect(theFields[i] + ",9,5) as float) " + theFields[i], "cast(str(a");
                }
                else { selectClause += addImgSelect(theFields[i], "a"); }
                if (i < (theFields.Count - 1))
                    selectClause += ",";
            }
            return selectClause;
        }

        public static string buildSelect(bool bestdb, bool targdb, string photoTable, List<string> imgFields, string joinCond, string selectClause)
        {
            string bestAlias = "p", targAlias = "t";
            if (bestdb)
            {
                for (int i = 0; i < imgFields.Count; i++)
                {
                    selectClause += addImgSelect(imgFields[i], bestAlias);
                    if (i < (imgFields.Count - 1))
                        selectClause += ",";
                }
            }
            else
            {
                if (targdb)
                {
                    for (int i = 0; i < imgFields.Count; i++)
                    {
                        addImgSelect(imgFields[i], bestAlias);
                        if (i < (imgFields.Count - 1))
                            selectClause += ",";
                    }
                }
            }
            return selectClause;
        }

        public static string addImgSelect(string imgField, string table)
        {
            string selectClause = "";
            switch (imgField)
            {
                case "ra":
                    selectClause += "cast(str(" + table + "." + imgField + ",13,8) as float) as ra";
                    break;
                case "[dec]":
                    selectClause += "cast(str(" + table + "." + imgField + ",13,8) as float) as dec";
                    break;
                case "model_colors":
                    selectClause += "cast(str(" + table + ".u - " + table + ".g,11,8) as float) as ugModelColor,";
                    selectClause += "cast(str(" + table + ".g - " + table + ".r,11,8) as float) as grModelColor,";
                    selectClause += "cast(str(" + table + ".r - " + table + ".i,11,8) as float) as riModelColor,";
                    selectClause += "cast(str(" + table + ".i - " + table + ".z,11,8) as float) as izModelColor";
                    break;
                case "ugModelColor":
                    selectClause += "cast(str(" + table + ".u - " + table + ".g,11,8) as float) as ugModelColor";
                    break;
                case "grModelColor":
                    selectClause += "cast(str(" + table + ".g - " + table + ".r,11,8) as float) as grModelColor";
                    break;
                case "riModelColor":
                    selectClause += "cast(str(" + table + ".r - " + table + ".i,11,8) as float) as riModelColor";
                    break;
                case "izModelColor":
                    selectClause += "cast(str(" + table + ".i - " + table + ".z,11,8) as float) as izModelColor";
                    break;
                case "SDSSname":
                    selectClause += "dbo.fIAUFromEq(" + table + ".ra," + table + ".[dec]) as SDSSname";
                    break;
                default:
                    selectClause += table + "." + imgField;
                    break;
            }
            return selectClause;
        }
       
    }
}