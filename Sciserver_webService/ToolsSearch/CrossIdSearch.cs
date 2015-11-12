using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using net.ivoa.VOTable;
using Newtonsoft.Json;
using System.IO;
using System.Text;
using Sciserver_webService.Common;
using System.Data.SqlClient;
using System.Data;
using System.Globalization;
using System.Web.Http;
using System.Net.Http;

namespace Sciserver_webService.ToolsSearch
{
    public class CrossIdSearch
    {

        public string query = "";
        public string Format = "";

        string[] ubody;
        string utype;
        string uvalue;

        string reSignedFloat = @"^((((\+|-)?\d+(\.\d*)?)|((\+|-)?(\d*\.)?\d+))([eE](\+|-){1}\d+)?)$";
        //string reObjID = @"\d{15,18}";
        //string reGator = @"^\\\s+";
        string reSplit = @"(\,|\s+)";
        //string reName = "\"(\\w+)\"";
        //string reColon = @"\w+: ";
        //string reBlank = @"^\s*";
        string reSelect = @"select";
        string rePobjid = @"p\.objid\s*,";
        int firstCol = 0;
        List<string> colNames = new List<string>();

        protected string url;

        //string windows_name;
        //string server_name;
        //string remote_addr;

        HttpRequest Request;
        public string table = "";
        string radius;
        public string QueryForUserDisplay = "";

        public string photoUpType = "";
        public string spectroUpType = "";
        public string apogeeUpType = "";
        public string spectroScope = "";
        public string photoScope = "";
        public string UserQuery = "";
        public string searchType = "";


        public string ClientIP = "";
        public string TaskName = "";
        public string server_name = "";
        public string windows_name = "";


        public CrossIdSearch(Dictionary<string, string> requestDir, Dictionary<string, string> ExtraInfo, HttpRequest Request, HttpContent Content)
        {
            this.Request = Request;

            try
            {
                ClientIP = ExtraInfo["ClientIP"];
                TaskName = ExtraInfo["TaskName"];
                server_name = ExtraInfo["server_name"];
                windows_name = ExtraInfo["windows_name"];
            }
            catch (Exception e) { throw new Exception(e.Message); };


            try
            {
                photoUpType = Request["photoUpType"];
                spectroUpType = Request["spectroUpType"];
                apogeeUpType = Request["apogeeUpType"];
                spectroScope = Request["spectroScope"];
                photoScope = Request["photoScope"];
                UserQuery = Request["uquery"];
                searchType = Request["searchType"];
            }
            catch { throw new ArgumentException("List of necessary input parameters is incomplete (either photoUpType, spectroUpType, apogeeUpType, spectroScope, photoScope or uquery are missing)"); }



            // reading hte upload file or list
            try
            {
                var task = Content.ReadAsStreamAsync();
                task.Wait();
                Stream stream = task.Result;
                this.table = (new StreamReader(stream)).ReadToEnd();
                if (this.table.Length == 0)
                {
                    this.table = Request["paste"];
                }
                if (this.table.Length == 0)
                    throw new ArgumentException("Unable to read upload file or list.");
            }
            catch { throw new ArgumentException("Unable to read upload file or list."); }
            ubody = this.table.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

            //reading the search radius, if ra-dec search is chosen
            this.radius = "1";// in arcminutes
            try {
                if ("ra-dec".Equals(photoUpType))
                {
                    this.radius = float.Parse(Request["radius"]).ToString();
                }
            }
            catch{throw new ArgumentException("The value of 'radius' is non numerical");}
            if (float.Parse(radius) <= 0 || float.Parse(radius) > 3)
                throw new ArgumentException("Radius variable should be a positive number smaller than 3 arcmin.");

            
            //reading the firstcol parameter
            try { firstCol = int.Parse(Request["firstcol"]); if (firstCol < 0) { throw new ArgumentException("Number of preceding non-data columns (firstcol) should be an integer number greater or equal to zero."); } }
            catch { throw new ArgumentException("Number of preceding non-data columns (firstcol) not specified or has incorrect format."); }


            // this runs the main code
            try
            {
                ProcessRequest();
            }
            catch (Exception e) { throw new Exception(e.Message); }

            
            
            string c2 = Regex.Replace(this.query, @"\/\*(.*\n)*\*\/", "");	                        // remove all multi-line comments
            c2 = Regex.Replace(c2, @"^[ \t\f\v]*--.*\r\n", "", RegexOptions.Multiline);		// remove all isolated single-line comments
            c2 = Regex.Replace(c2, @"--[^\r^\n]*", "");				                        // remove all embedded single-line comments
            c2 = Regex.Replace(c2, @"[ \t\f\v]+", " ");                      				// replace multiple whitespace with single space
            c2 = Regex.Replace(c2, @"^[ \t\f\v]*\r\n", "", RegexOptions.Multiline);			// remove empty lines
            //c2 = c2.Replace("'", "''");

            // 'c' is query version that's printed on output page
            // 'c2' is the version that is sent to DB server 
            this.query = c2;
            c2=c2.Replace("'", "''");
            this.query = "EXEC spExecuteSQL '" + c2 + "','" + KeyWords.MaxRows + "','" + server_name + "','" + windows_name + "','" + ClientIP + "','" + TaskName.Substring(0, Math.Min(TaskName.Length, 32)) + "',@filter=0,@log=1";// parsing the query against harmful sql commands
            // @cmd,@limit,@webserver,@winname,@clientIP,@access,@system,@maxQueries
            //this.query = c2;
        }



        protected void ProcessRequest()
        {
            getUploadFormat(table);
            UploadCmd();
            loadUpload(table);

            if ("photo".Equals(searchType))
            {
                if ("ra-dec".Equals(photoUpType))
                {
                    if ("allPrim".Equals(photoScope) || "allObj".Equals(photoScope))
                        getNearby("");
                    else // if(f.photoScope.value=="nearPrim" || f.photoScope.value=="nearObj") 
                        getNearest("");
                }
                else
                    getObjID();
            }
            else if ("spectro".Equals(searchType))
            {
                if ("ra-dec".Equals(spectroUpType))
                {
                    if ("allPrim".Equals(spectroScope) || "allObj".Equals(spectroScope))
                        getNearby("Spec");
                    else // if(f.spectroScope.value=="nearPrim" || f.spectroScope.value=="nearObj") 
                        getNearest("Spec");
                }
                else
                    getPmf();
            }
            else //if ("apogee".Equals(Request["searchType"]))
            {
                getNearestApogee();
            }
            //}

        }

    



        private void loadUpload(string radecText)
        {
            string[] lines = radecText.Split(new string[] { "\n", "\r\n", "\\n", "\\r\\n" }, StringSplitOptions.None);
            string[] names = Regex.Split(lines[0], reSplit, RegexOptions.ExplicitCapture).Where(str => !str.Equals(String.Empty)).ToArray();
            //columnNames = lines[0].Split(',');
            bool IsGalactic = false;
            int il = -1;
            int ib = -1;
            double glon = 0;
            double glat = 0;

            if ((searchType == "apogee") && (apogeeUpType == "l-b"))
            {
                IsGalactic = true;
                for (int i = 0; i < names.Length; i++)
                {
                    if (names[i] == "l") { il = i;}
                    if (names[i] == "b") { ib = i;}
                }
            }

            string cmdQuery = "";
            cmdQuery += " \nINSERT INTO #upload values ";

            
            for (int i = 1; i < lines.Length; i++)
            {
                if (lines[i] != "")
                {
                    string[] RowElements = Regex.Split(lines[i], reSplit, RegexOptions.ExplicitCapture).Where(str => !str.Equals(String.Empty)).ToArray();
                    cmdQuery += "( " + i;
                    if(IsGalactic)
                    {
                        glon = double.Parse(RowElements[ib]);
                        glat = double.Parse(RowElements[il]);
                        RowElements[il] = Utilities.glon2ra(glat, glon).ToString("F5", CultureInfo.InvariantCulture);
                        RowElements[ib] = Utilities.glat2dec(glat, glon).ToString("F5", CultureInfo.InvariantCulture);
                    }
                    for (int j = 0; j < firstCol; j++)
                        cmdQuery += ", '" + RowElements[j] + "'";
                    for (int j = firstCol; j < RowElements.Length; j++)
                        cmdQuery += ", " + RowElements[j];
                    cmdQuery += "),";
                }
            }
            cmdQuery = cmdQuery.Trim(',');
            this.query += cmdQuery;
            this.QueryForUserDisplay += cmdQuery;
        }

        private string getUploadFormat(string u)
        {
            //string[] body = u.Split('\n');
            string s = ubody[0];
            uvalue = "";

            // check for Gator format
            if (Regex.IsMatch(s, @"^\\\ \w+"))
            {
                // need to scroll forward to the beginning of data
                while (Regex.IsMatch(ubody[0], @"^\\")) ubody = ubody.Skip(1).ToArray();
                if (Regex.IsMatch(ubody[0], @"^\s*\|") && "".Equals(uvalue))
                {
                    uvalue = ubody[0].Replace("|", " ");
                    ubody = ubody.Skip(2).ToArray();
                }
                utype = "G";
                return "G";
            }

            // is it a comma/whitespace?
            string[] s2 = s.Split('#');			// allow for # as first char for column headers line
            if (s2.Length > 1)
                s = s2[1];
            string[] c = Regex.Split(s, reSplit, RegexOptions.ExplicitCapture).Where(str => !str.Equals(String.Empty)).ToArray();
            //colNames.AddRange(c);
            int n = c.Length;
            int fc = 0;
            fc = firstCol;
            if (fc >= n)
                return Error("first data column number " + firstCol + " is larger than number of columns");

            int ndata = n - fc, nvalid;
            if (searchType == "photo")
            {
                if (photoUpType == "ra-dec")
                    nvalid = 2;
                else
                    nvalid = 5;
            }
            else if (searchType == "spectro")
            {
                if (spectroUpType == "ra-dec")
                    nvalid = 2;
                else
                    nvalid = 3;
            }
            else //if (Request["searchType"] == "apogee")
            {
                nvalid = 2;
            }

            if (ndata != nvalid)
                return Error("problem with upload file/text:<br> " + ndata + " data items in first line: '" + s + "'<br>Is the number of preceding columns correct?");

            // we have ndata values, are they numbers or text
            uvalue = "";

            //if( dbg == 1 ) showLine( "checking upload type\n" );

            if (searchType == "photo")
            {
                if (photoUpType == "run-rerun")
                {
                    // if the first line has objID (so no heading line)
                    if (Regex.IsMatch(c[n - 5], @"\d+"))
                    {
                        for (int i = 0; i < fc; i++)
                            uvalue += "col" + i + " ";
                        uvalue += "run rerun camcol field obj";
                        colNames.AddRange(Regex.Split(uvalue, reSplit, RegexOptions.ExplicitCapture).Where(str => !str.Equals(String.Empty)).ToArray());
                        utype = "N" + n;
                        return "N" + n;
                    }
                    else if (Regex.IsMatch(c[n - 5], @"^run$", RegexOptions.IgnoreCase) && Regex.IsMatch(c[n - 4], @"^rerun$", RegexOptions.IgnoreCase) &&
                         Regex.IsMatch(c[n - 3], @"^camcol$", RegexOptions.IgnoreCase) && Regex.IsMatch(c[n - 2], @"^field$", RegexOptions.IgnoreCase) &&
                         Regex.IsMatch(c[n - 1], @"^obj$", RegexOptions.IgnoreCase))
                    {
                        foreach (string i in c)
                            uvalue += " " + i;
                        colNames.AddRange(Regex.Split(uvalue, reSplit, RegexOptions.ExplicitCapture).Where(str => !str.Equals(String.Empty)).ToArray());
                        ubody = ubody.Skip(1).ToArray();
                        utype = "H" + n;
                        return "H" + n;
                    }
                    else
                        return Error("problem with upload file/text: incorrect header names on first line '" + s + "'");
                }
                else
                {
                    // if the first line has floats (ra/dec with no heading line)
                    if (Regex.IsMatch(c[n - 2], reSignedFloat))
                    {
                        for (int i = 0; i < fc; i++)
                            uvalue += "col" + i + " ";
                        uvalue += "ra dec";
                        colNames.AddRange(Regex.Split(uvalue, reSplit, RegexOptions.ExplicitCapture).Where(str => !str.Equals(String.Empty)).ToArray());
                        utype = "N" + n;
                        return "N" + n;
                    }
                    else if (Regex.IsMatch(c[n - 2], @"^ra$", RegexOptions.IgnoreCase) && Regex.IsMatch(c[n - 1], @"^dec$", RegexOptions.IgnoreCase))
                    {
                        foreach (string i in c)
                            uvalue += " " + i;
                        colNames.AddRange(Regex.Split(uvalue, reSplit, RegexOptions.ExplicitCapture).Where(str => !str.Equals(String.Empty)).ToArray());
                        ubody = ubody.Skip(1).ToArray();
                        utype = "H" + n;
                        return "H" + n;
                    }
                    else
                        return Error("problem with upload file/text: incorrect header names on first line '" + s + "'");
                }
            }
            else if (searchType == "spectro")
            {
                //if( dbg == 1 ) showLine( "checking spectro fields\n" );
                if (spectroUpType == "plate-mjd-fiber")
                {
                    // if the first line has plate (so no heading line)
                    if (Regex.IsMatch(c[n - 3], @"\d+"))
                    {
                        for (int i = 0; i < fc; i++)
                            uvalue += "col" + i + " ";
                        uvalue += "plate mjd fiber";
                        colNames.AddRange(Regex.Split(uvalue, reSplit, RegexOptions.ExplicitCapture).Where(str => !str.Equals(String.Empty)).ToArray());
                        utype = "N" + n;
                        return "N" + n;
                    }
                    else if (Regex.IsMatch(c[n - 3], @"^plate$", RegexOptions.IgnoreCase) && Regex.IsMatch(c[n - 2], @"^mjd$", RegexOptions.IgnoreCase) &&
                         Regex.IsMatch(c[n - 1], @"^fiber$", RegexOptions.IgnoreCase))
                    {
                        //if( dbg == 1 ) showLine( "plate-mjd-fiber list\n" );
                        foreach (string i in c)
                            uvalue += " " + i;
                        colNames.AddRange(Regex.Split(uvalue, reSplit, RegexOptions.ExplicitCapture).Where(str => !str.Equals(String.Empty)).ToArray());
                        ubody = ubody.Skip(1).ToArray();
                        utype = "H" + n;
                        return "H" + n;
                    }
                    else
                        return Error("problem with upload file/text: incorrect header names on first line '" + s + "'");
                }
                else
                {
                    // if the first line has floats (ra/dec with no heading line)
                    if (Regex.IsMatch(c[n - 2], reSignedFloat))
                    {
                        for (int i = 0; i < fc; i++)
                            uvalue += "col" + i + " ";
                        uvalue += "ra dec";
                        colNames.AddRange(Regex.Split(uvalue, reSplit, RegexOptions.ExplicitCapture).Where(str => !str.Equals(String.Empty)).ToArray());
                        utype = "N" + n;
                        return "N" + n;
                    }
                    else if (Regex.IsMatch(c[n - 2], @"^ra$", RegexOptions.IgnoreCase) && Regex.IsMatch(c[n - 1], @"^dec$", RegexOptions.IgnoreCase))
                    {
                        foreach (string i in c)
                            uvalue += " " + i;
                        colNames.AddRange(Regex.Split(uvalue, reSplit, RegexOptions.ExplicitCapture).Where(str => !str.Equals(String.Empty)).ToArray());
                        ubody = ubody.Skip(1).ToArray();
                        utype = "H" + n;
                        return "H" + n;
                    }
                    else
                        return Error("problem with upload file/text: incorrect header names on first line '" + s + "'");
                }
            }
            else //if (Request["searchType"] == "apogee")
            {
                if (apogeeUpType == "ra-dec")
                {
                    if (Regex.IsMatch(c[n - 2], reSignedFloat))
                    {
                        for (int i = 0; i < fc; i++)
                            uvalue += "col" + i + " ";
                        uvalue += "ra dec";
                        colNames.AddRange(Regex.Split(uvalue, reSplit, RegexOptions.ExplicitCapture).Where(str => !str.Equals(String.Empty)).ToArray());
                        utype = "N" + n;
                        return "N" + n;
                    }
                    else if (Regex.IsMatch(c[n - 2], @"^ra$", RegexOptions.IgnoreCase) && Regex.IsMatch(c[n - 1], @"^dec$", RegexOptions.IgnoreCase))
                    {
                        foreach (string i in c)
                            uvalue += " " + i;
                        colNames.AddRange(Regex.Split(uvalue, reSplit, RegexOptions.ExplicitCapture).Where(str => !str.Equals(String.Empty)).ToArray());
                        ubody = ubody.Skip(1).ToArray();
                        utype = "H" + n;
                        return "H" + n;
                    }
                    else
                        return Error("problem with upload file/text: incorrect header names on first line '" + s + "'");
                }
                else //if (Request["apogeeUpType"] == "l-b")
                {
                    if (Regex.IsMatch(c[n - 2], reSignedFloat))
                    {
                        for (int i = 0; i < fc; i++)
                            uvalue += "col" + i + " ";
                        uvalue += "l b";
                        colNames.AddRange(Regex.Split(uvalue, reSplit, RegexOptions.ExplicitCapture).Where(str => !str.Equals(String.Empty)).ToArray());
                        utype = "N" + n;
                        return "N" + n;
                    }
                    else if (Regex.IsMatch(c[n - 2], @"^l$", RegexOptions.IgnoreCase) && Regex.IsMatch(c[n - 1], @"^b$", RegexOptions.IgnoreCase))
                    {
                        foreach (string i in c)
                            uvalue += " " + i;
                        colNames.AddRange(Regex.Split(uvalue, reSplit, RegexOptions.ExplicitCapture).Where(str => !str.Equals(String.Empty)).ToArray());
                        ubody = ubody.Skip(1).ToArray();
                        utype = "H" + n;
                        return "H" + n;
                    }
                    else
                        return Error("problem with upload file/text: incorrect header names on first line '" + s + "'");
                }

            }
        }

        protected string Error(string msg)
        {
            //Response.Write("<h2>Error: " + msg + "</h2>\n");
            return msg;
        }

        private void getNearestApogee()
        {
            string cmd = "";
            cmd += "\ncreate table #x (up_id int, apstar_id varchar(50))";
            cmd += "\nINSERT INTO #x \nSELECT up_id, (SELECT apstar_id FROM fGetNearestApogeeStarEq(up_ra,up_dec," + radius + ")) as apstar_id \n     ";
            cmd += "FROM #upload WHERE (SELECT apstar_id FROM fGetNearestApogeeStarEq(up_ra,up_dec," + radius + ")) IS NOT NULL ";

            var uquery = UserQuery;

            if (firstCol > 0)
            {
                var fields = "\nSELECT ";
                for (int i = 0; i < firstCol; i++)
                    fields += "u.up_" + colNames[i] + " as [" + colNames[i] + "],";
                uquery = Regex.Replace(uquery, reSelect, fields, RegexOptions.IgnoreCase);
            }
            this.QueryForUserDisplay += cmd + "\n" + uquery;
            this.query += cmd + "\nEXEC spExecuteSQL '" + uquery + "','" + KeyWords.MaxRows + "', @log=0, @filter=1";
        }

        private void getNearest(string spec)
        {
            string cmd = "";

            cmd = cmd + "\ncreate table #x (up_id int," + spec + "objID bigint)";

            string fun;
            if ((spec == "" && photoScope == "nearObj") || (spec == "Spec" && spectroScope == "nearObj"))
                fun = " dbo.fGetNearest" + spec + "ObjIdAllEq";
            else
                fun = " dbo.fGetNearest" + spec + "ObjIdEq";
            fun += "(up_ra,up_dec," + radius + ") ";
            cmd += "\nINSERT INTO #x \nSELECT up_id," + fun + "as " + spec + "objId \n     ";
            cmd += "FROM #upload WHERE" + fun + "IS NOT NULL ";

            var uquery = UserQuery;
            if (spec == "" && !Regex.IsMatch(uquery, rePobjid, RegexOptions.IgnoreCase))
                uquery = Regex.Replace(uquery, reSelect, "\nSELECT x.objID,", RegexOptions.IgnoreCase);
            if (firstCol > 0)
            {
                var fields = "SELECT ";
                for (int i = 0; i < firstCol; i++)
                    fields += "u.up_" + colNames[i] + " as [" + colNames[i] + "],";
                uquery = Regex.Replace(uquery, reSelect, fields, RegexOptions.IgnoreCase);
            }
            this.QueryForUserDisplay += cmd + uquery;
            if (spec == "" && Format == "html")
                uquery = Regex.Replace(uquery, rePobjid, "''<a target=INFO href=" + url + "/tools/explore/obj.aspx?id='' + cast(x.objId as varchar(20)) + ''>''+ cast(x.objId as varchar(20)) + ''</a>'' as objID,", RegexOptions.IgnoreCase);

            this.query += cmd + " " + "\nEXEC spExecuteSQL '" + uquery + "','" + KeyWords.MaxRows + "', @log=0, @filter=1";
        }

        private void getObjID()
        {
            string cmd = "";
            cmd += "\ncreate table #x (up_id int,objID bigint)";
            cmd += "\nINSERT INTO #x \nSELECT up_id, p.objID ";
            int n = colNames.Count;
            cmd += "FROM #upload, PhotoTag p \n   WHERE p.objID = dbo.fObjidFromSDSS(" + KeyWords.skyVersion + ",up_" + colNames[n - 5];
            cmd += ",up_" + colNames[n - 4] + ",up_" + colNames[n - 3] + ",up_" + colNames[n - 2];
            cmd += ",up_" + colNames[n - 1] + ")\n";

            var uquery = UserQuery;

            if (!Regex.IsMatch(uquery, rePobjid, RegexOptions.IgnoreCase))
                uquery = Regex.Replace(uquery, reSelect, "\nSELECT x.objID,", RegexOptions.IgnoreCase);
            var fields = "\nSELECT ";
            for (int i = 0; i < n; i++)
                fields += "u.up_" + colNames[i] + " as " + colNames[i] + ",";
            //uquery = Regex.Replace(uquery, reSelect, fields, RegexOptions.IgnoreCase);

            this.QueryForUserDisplay += cmd + uquery;
            if (Format == "html")
                uquery = Regex.Replace(uquery, rePobjid, "''<a target=INFO href=" + url + "/tools/explore/obj.aspx?id='' + cast(x.objId as varchar(20)) + ''>''+ cast(x.objId as varchar(20)) + ''</a>'' as objID,", RegexOptions.IgnoreCase);
            this.query += cmd + " " + "\nEXEC spExecuteSQL '" + uquery + "','" + KeyWords.MaxRows + "', @log=0, @filter=1";
            
        }

        private void getPmf()
        {
            string uquery = UserQuery;
            this.QueryForUserDisplay += "\n" + uquery;
            this.query += "\nEXEC spExecuteSQL '" + uquery + "','" + KeyWords.MaxRows + "', @log=0, @filter=1";
        }


        private void UploadCmd()
        {
            string[] names = Regex.Split(uvalue, reSplit, RegexOptions.ExplicitCapture).Where(str => !str.Equals(String.Empty)).ToArray();
            var cmd = "CREATE TABLE #upload ( up_id int";
            foreach (string i in names)
            {
                string name = i;
                if (i == "l") name = "ra";
                if (i == "b") name = "dec";
                cmd += ", up_" + name + " ";
                switch (name)
                {
                    case "objID":
                        cmd += "bigint";
                        break;
                    case "ra":
                    case "dec":
                        cmd += "float";
                        break;
                    case "plate":
                    case "mjd":
                    case "fiber":
                    case "fiberid":
                        cmd += "int";
                        break;
                    default:
                        cmd += "varchar(32)";
                        break;
                }
            }
            cmd += " )";
            this.query = cmd;
            this.QueryForUserDisplay = cmd;
        }

        private void getNearby(string spec)
        {
            string cmd = "";
            DataSet dataSet = new DataSet();


            if ((spec == "" && photoScope == "allPrim") || (spec == "Spec" && spectroScope == "allPrim"))
            {
                //if (dbg==1) showLine("<h4>Get all nearby primary objects within "+radius+" arcmins</h4>");
                cmd = cmd + "\ncreate table #x (up_id int," + spec + "objID bigint)";
                cmd = cmd +"\ninsert into #x \nEXEC dbo.spGet" + spec + "NeighborsPrim " + radius;
            }
            else
            {
                //if (dbg==1) showLine("<h4>Get all nearby objects within "+radius+" arcmins</h4>");
                cmd = cmd + "\ncreate table #x (up_id int," + spec + "objID bigint)";
                cmd = cmd + "\ninsert into #x \nEXEC dbo.spGet" + spec + "NeighborsAll " + radius;
            }

            string uquery = UserQuery;
            if (spec == "" && !Regex.IsMatch(uquery, rePobjid, RegexOptions.IgnoreCase))
                uquery = Regex.Replace(uquery, reSelect, "SELECT x.objID,", RegexOptions.IgnoreCase);
            if (firstCol > 0)
            {
                var fields = "\nSELECT ";
                for (int i = 0; i < firstCol; i++)
                    fields += "u.up_" + colNames[i] + " as " + colNames[i] + ",";
                uquery = Regex.Replace(uquery, reSelect, fields, RegexOptions.IgnoreCase);
            }

            this.QueryForUserDisplay += cmd + uquery;
            if (spec == "" && Format == "html")
                uquery = Regex.Replace(uquery, rePobjid, "''<a target=INFO href=" + url + "/tools/explore/obj.aspx?id='' + cast(x.objId as varchar(20)) + ''>''+ cast(x.objId as varchar(20)) + ''</a>'' as objID,", RegexOptions.IgnoreCase);

            this.query += cmd + " " + "\nEXEC spExecuteSQL '" + uquery + "','" + KeyWords.MaxRows + "', @log=0, @filter=1";
        }
    }
}