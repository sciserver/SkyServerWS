using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Configuration;
using System.Net;
using System.IO;

namespace WebServicesTest
{
    [TestClass]
    public class WebServiceTest
    {
        [TestMethod]
        public void TestConeSearch()
        {
            string requestUri = ConfigurationManager.AppSettings["WebServiceUri"] + "ConeSearch/ConeSearchService";
            string parameters = "ra=145&dec=34&sr=0.4";

            var request = (HttpWebRequest)WebRequest.Create(requestUri+"?"+parameters);
            request.Method = "GET";
            //request.ContentType = "application/json";
            //request.Headers.Add("X-Auth-Token", "");                       

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            StreamReader reader = new StreamReader(response.GetResponseStream());
            string s = reader.ReadToEnd();
            reader.Close();

            String r = "objid,ra,dec,type,u,g,r,i,z,Err_u,Err_g,Err_r,Err_i,Err_z,psfMag_u,psfMagErr_u,psfMag_g,psfMagErr_g,psfMag_r,psfMagErr_r,psfMag_i,psfMagErr_i,psfMag_z,psfMagErr_z\n";
r+="1237664871898481109,144.993713853412,33.9998595972388,GALAXY,22.76426,22.45936,21.88338,21.73658,21.83738,0.3666119,0.1063959,0.09865708,0.1309934,0.4330766,22.80189,0.3340747,22.63518,0.1106879,22.05456,0.09911135,21.91485,0.130704,21.99736,0.4067098\n";
r += "1237664871898480896,145.004948088172,34.0047067092208,STAR,21.8021,21.70746,21.53257,21.49842,20.47161,0.154552,0.05148993,0.06584511,0.09497431,0.1294998,21.74096,0.1393216,21.7165,0.06162296,21.51044,0.06417897,21.51015,0.0933324,20.45054,0.1233625\n";
r+="1237664871898481122,145.000043453448,33.9937119425584,GALAXY,24.34384,22.77464,22.4832,22.21194,21.37107,0.9045116,0.1307753,0.1568346,0.1865005,0.2903498,24.33658,0.8661367,22.86678,0.1318145,22.56147,0.1518914,22.3099,0.182533,21.38643,0.2657976";
            Assert.AreEqual(r, s);
        }

        [TestMethod]
        public void TestGetSIAP()
        {
            string requestUri = ConfigurationManager.AppSettings["WebServiceUri"] + "SIAP/getSIAP?POS=132,12&SIZE=0.1&FORMAT=image/jpeg";
            var request = (HttpWebRequest)WebRequest.Create(requestUri);
            request.Method = "GET";
            //request.ContentType = "application/json";
            //request.Headers.Add("X-Auth-Token", "");
            //StreamWriter writer = new StreamWriter(request.GetRequestStream());
            //writer.Write("");
            //writer.Close();
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            StreamReader reader = new StreamReader(response.GetResponseStream());
            string s = reader.ReadToEnd();
            reader.Close();

            string test = "{\"DESCRIPTION\":null,\"DEFINITIONS\":null,\"INFO\":[{\"ID\":null,\"name\":\"QUERY_STATUS\",\"value\":\"OK\",\"Text\":null},{\"ID\":null,\"name\":\"FieldsUrl\",\"value\":null,\"Text\":[\"http://apus.pha.jhu.edu/DR9FIELDS/sdssfields.asmx\"]},{\"ID\":null,\"name\":\"CutOutUrl\",\"value\":null,\"Text\":[\"http://skyservice.pha.jhu.edu/dr10/ImgCutout/getjpeg.aspx\"]}],\"RESOURCE\":[{\"DESCRIPTION\":null,\"Items\":[{\"DESCRIPTION\":null,\"VALUES\":null,\"LINK\":null,\"ID\":null,\"unit\":null,\"datatype\":6,\"precision\":null,\"width\":null,\"ref\":null,\"name\":\"INPUT:POS\",\"ucd\":null,\"utype\":null,\"value\":\"132,12\",\"arraysize\":\"*\"},{\"DESCRIPTION\":null,\"VALUES\":null,\"LINK\":null,\"ID\":null,\"unit\":null,\"datatype\":6,\"precision\":null,\"width\":null,\"ref\":null,\"name\":\"INPUT:SIZE\",\"ucd\":null,\"utype\":null,\"value\":\"0.1\",\"arraysize\":\"*\"},{\"DESCRIPTION\":null,\"VALUES\":null,\"LINK\":null,\"ID\":null,\"unit\":null,\"datatype\":6,\"precision\":null,\"width\":null,\"ref\":null,\"name\":\"INPUT:FORMAT\",\"ucd\":null,\"utype\":null,\"value\":\"image/jpeg\",\"arraysize\":\"*\"}],\"LINK\":null,\"TABLE\":[{\"DESCRIPTION\":null,\"Items\":[{\"DESCRIPTION\":null,\"VALUES\":null,\"LINK\":null,\"ID\":\"Title\",\"unit\":null,\"datatype\":6,\"precision\":null,\"width\":null,\"ref\":null,\"name\":null,\"ucd\":\"VOX:Image_Title\",\"utype\":null,\"arraysize\":\"*\"},{\"DESCRIPTION\":null,\"VALUES\":null,\"LINK\":null,\"ID\":\"width\",\"unit\":null,\"datatype\":4,\"precision\":null,\"width\":null,\"ref\":null,\"name\":null,\"ucd\":\"VOX:Image_Pix_Width\",\"utype\":null,\"arraysize\":null},{\"DESCRIPTION\":null,\"VALUES\":null,\"LINK\":null,\"ID\":\"height\",\"unit\":null,\"datatype\":4,\"precision\":null,\"width\":null,\"ref\":null,\"name\":null,\"ucd\":\"VOX:Image_Pix_Height\",\"utype\":null,\"arraysize\":null},{\"DESCRIPTION\":null,\"VALUES\":null,\"LINK\":null,\"ID\":\"size\",\"unit\":null,\"datatype\":9,\"precision\":null,\"width\":null,\"ref\":null,\"name\":null,\"ucd\":\"VOX:Image_Size\",\"utype\":null,\"arraysize\":null},{\"DESCRIPTION\":null,\"VALUES\":null,\"LINK\":null,\"ID\":\"RA\",\"unit\":null,\"datatype\":9,\"precision\":null,\"width\":null,\"ref\":null,\"name\":null,\"ucd\":\"POS_EQ_RA_MAIN\",\"utype\":null,\"arraysize\":null},{\"DESCRIPTION\":null,\"VALUES\":null,\"LINK\":null,\"ID\":\"DEC\",\"unit\":null,\"datatype\":9,\"precision\":null,\"width\":null,\"ref\":null,\"name\":null,\"ucd\":\"POS_EQ_DEC_MAIN\",\"utype\":null,\"arraysize\":null},{\"DESCRIPTION\":null,\"VALUES\":null,\"LINK\":null,\"ID\":\"scale\",\"unit\":null,\"datatype\":9,\"precision\":null,\"width\":null,\"ref\":null,\"name\":null,\"ucd\":\"VOX:Image_Scale\",\"utype\":null,\"arraysize\":\"*\"},{\"DESCRIPTION\":null,\"VALUES\":null,\"LINK\":null,\"ID\":\"format\",\"unit\":null,\"datatype\":6,\"precision\":null,\"width\":null,\"ref\":null,\"name\":null,\"ucd\":\"VOX:Image_Format\",\"utype\":null,\"arraysize\":\"*\"},{\"DESCRIPTION\":null,\"VALUES\":null,\"LINK\":null,\"ID\":\"url\",\"unit\":null,\"datatype\":6,\"precision\":null,\"width\":null,\"ref\":null,\"name\":null,\"ucd\":\"VOX:Image_AccessReference\",\"utype\":null,\"arraysize\":\"*\"},{\"DESCRIPTION\":null,\"VALUES\":null,\"LINK\":null,\"ID\":\"equinox\",\"unit\":null,\"datatype\":9,\"precision\":null,\"width\":null,\"ref\":null,\"name\":null,\"ucd\":\"VOX:Image_Equinox\",\"utype\":null,\"arraysize\":null},{\"DESCRIPTION\":null,\"VALUES\":null,\"LINK\":null,\"ID\":\"naxes\",\"unit\":null,\"datatype\":4,\"precision\":null,\"width\":null,\"ref\":null,\"name\":null,\"ucd\":\"VOX:Image_Naxes\",\"utype\":null,\"arraysize\":null},{\"DESCRIPTION\":null,\"VALUES\":null,\"LINK\":null,\"ID\":\"naxis\",\"unit\":null,\"datatype\":4,\"precision\":null,\"width\":null,\"ref\":null,\"name\":null,\"ucd\":\"VOX:Image_Naxis\",\"utype\":null,\"arraysize\":\"*\"},{\"DESCRIPTION\":null,\"VALUES\":null,\"LINK\":null,\"ID\":\"crtype\",\"unit\":null,\"datatype\":6,\"precision\":null,\"width\":null,\"ref\":null,\"name\":null,\"ucd\":\"VOX:WCS_CoordProjection\",\"utype\":null,\"arraysize\":\"*\"},{\"DESCRIPTION\":null,\"VALUES\":null,\"LINK\":null,\"ID\":\"crpix\",\"unit\":null,\"datatype\":9,\"precision\":null,\"width\":null,\"ref\":null,\"name\":null,\"ucd\":\"VOX:WCS_CoordRefPixel\",\"utype\":null,\"arraysize\":\"*\"},{\"DESCRIPTION\":null,\"VALUES\":null,\"LINK\":null,\"ID\":\"crval\",\"unit\":null,\"datatype\":9,\"precision\":null,\"width\":null,\"ref\":null,\"name\":null,\"ucd\":\"VOX:WCS_CoordRefValue\",\"utype\":null,\"arraysize\":\"*\"},{\"DESCRIPTION\":null,\"VALUES\":null,\"LINK\":null,\"ID\":\"cdval\",\"unit\":null,\"datatype\":9,\"precision\":null,\"width\":null,\"ref\":null,\"name\":null,\"ucd\":\"VOX:WCS_CDMatrix\",\"utype\":null,\"arraysize\":\"*\"}],\"LINK\":null,\"DATA\":{\"Item\":{\"TR\":[{\"TD\":[{\"encodingSpecified\":false,\"Text\":[\"Sloan Digital Sky Survey\"]},{\"encodingSpecified\":false,\"Text\":[\"908\"]},{\"encodingSpecified\":false,\"Text\":[\"908\"]},{\"encodingSpecified\":false,\"Text\":[\"824464\"]},{\"encodingSpecified\":false,\"Text\":[\"132\"]},{\"encodingSpecified\":false,\"Text\":[\"12\"]},{\"encodingSpecified\":false,\"Text\":[\"0.000110035211267606\"]},{\"encodingSpecified\":false,\"Text\":[\"image/jpeg\"]},{\"encodingSpecified\":false,\"Text\":[\"http://skyservice.pha.jhu.edu/dr10/ImgCutout/getjpeg.aspx?ra=132&dec=12&height=908&width=908&scale=0.39612676056338\"]},{\"encodingSpecified\":false,\"Text\":[\"J2000\"]},{\"encodingSpecified\":false,\"Text\":[\"2\"]},{\"encodingSpecified\":false,\"Text\":[\"908,908\"]},{\"encodingSpecified\":false,\"Text\":[\"RA--TAN, DEC--TAN\"]},{\"encodingSpecified\":false,\"Text\":[\"454,454\"]},{\"encodingSpecified\":false,\"Text\":[\"132,12\"]},{\"encodingSpecified\":false,\"Text\":[\"1,1\"]}]}]}},\"ID\":null,\"name\":null,\"ref\":null}],\"RESOURCE1\":null,\"Any\":null,\"name\":null,\"ID\":null,\"type\":0,\"AnyAttr\":null}],\"ID\":null,\"versionSpecified\":false}";
            Assert.AreEqual(test, s);
        }

        [TestMethod]
        public void TestGetRadial() {
            string requestUri = ConfigurationManager.AppSettings["WebServiceUri"] + "SearchTools/RadialSearch";
            string parameters = "ra=258.2&dec=64&radius=0.35&whichway=equitorial&limit=10&fp=none";
            var request = (HttpWebRequest)WebRequest.Create(requestUri + "?" + parameters);
            request.Method = "GET";

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            StreamReader reader = new StreamReader(response.GetResponseStream());
            string s = reader.ReadToEnd();
            reader.Close();
            string test = "objid,run,rerun,camcol,field,obj,type,ra,dec,u,g,r,i,z,Err_u,Err_g,Err_r,Err_i,Err_z\n";
                   test+="1237671939804562872,6162,301,3,133,1464,3,258.210626871138,64.0022531796968,23.38558,23.00034,21.47793,21.68395,21.40833,1.056581,0.302376,0.1237138,0.2104803,0.6197171";
            Assert.AreEqual(test, s);
            
        }

        [TestMethod]
        public void TestGetRectangular() { 
            
            string requestUri = ConfigurationManager.AppSettings["WebServiceUri"] + "SearchTools/RectangularSearch";
            string parameters = "min_ra=258.2&max_ra=258.3&min_dec=64.1&max_dec=64.2&searchtype=equitorial&limit=10&uband=0,15&gband=0,17";
            var request = (HttpWebRequest)WebRequest.Create(requestUri + "?" + parameters);
            request.Method = "GET";

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            StreamReader reader = new StreamReader(response.GetResponseStream());
            string s = reader.ReadToEnd();
            reader.Close();
            string test = "objid,run,rerun,camcol,field,obj,type,ra,dec,u,g,r,i,z,Err_u,Err_g,Err_r,Err_i,Err_z\n";
            test += "1237671768542609419,6122,301,4,131,11,6,258.25164806561,64.1413785663663,14.06628,11.42715,11.17008,11.1293,12.77769,0.006413009,0.0009883192,0.0008952241,0.0009348655,0.01516947";
            Assert.AreEqual(test, s);
        }

        [TestMethod]
        public void TestGetSQL() {
            string requestUri = ConfigurationManager.AppSettings["WebServiceUri"] + "SearchTools/SqlSearch";
            string parameters = "cmd=select top 1 ra,dec from Frame";

            var request = (HttpWebRequest)WebRequest.Create(requestUri + "?" + parameters);
            request.Method = "GET";

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            StreamReader reader = new StreamReader(response.GetResponseStream());
            string s = reader.ReadToEnd();
            reader.Close();

            string test = "ra,dec\n";
            test += "336.514598443829,-0.931640678911959";

            Assert.AreEqual(test, s);
        }

        [TestMethod]
        public void TestGetImagingCone() {
            string requestUri = ConfigurationManager.AppSettings["WebServiceUri"] + "ImagingQuery/Cone";
            string parameters = "radius=1.0&dec=0.2&ra=10&objType=doGalaxy,doStar&uMin=0&uMax=20";

            var request = (HttpWebRequest)WebRequest.Create(requestUri + "?" + parameters);
            request.Method = "GET";
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            StreamReader reader = new StreamReader(response.GetResponseStream());
            string s = reader.ReadToEnd();
            reader.Close();

            string test = "run,rerun,camCol,field,obj\n2728,301,4,480,81";
            Assert.AreEqual(test, s);
        }

        [TestMethod]
        public void TestGetImagingRect() {
            string requestUri = ConfigurationManager.AppSettings["WebServiceUri"] + "ImagingQuery/Rectangular";
            string parameters = "ramin=258&ramax=258.2&decmin=64&decmax=64.1&izMin=3&izMax=4&riMin=0&riMax=20&flagsonlist=BRIGHT,EDGE&imgparams=typical,minimal,radec,model_mags";

            var request = (HttpWebRequest)WebRequest.Create(requestUri + "?" + parameters);
            request.Method = "GET";
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            StreamReader reader = new StreamReader(response.GetResponseStream());
            string s = reader.ReadToEnd();
            reader.Close();

            string test = "run,rerun,camCol,field,obj,ra,dec,r,u,g,i,z\n";
                   test +="6162,301,3,133,1597, 258.05638302,  64.02582379,25.27066,25.26588,26.20419,23.72898,20.54672\n";
                   test +="6162,301,3,133,1602, 258.16825829,  64.03483363,25.11036,24.10958,24.63364,24.52086,20.95418\n";
                   test +="6162,301,3,133,1596, 258.01929995,  64.02830193,24.59832,24.40295,24.57849,24.57469,20.84985";

            Assert.AreEqual(test, s);
        }

        [TestMethod]
        public void TestGetImagingProx() {
            string requestUri = ConfigurationManager.AppSettings["WebServiceUri"] + "ImagingQuery/Proximity?radius=1.0&searchNearBy=nearest";
            var request = (HttpWebRequest)WebRequest.Create(requestUri);
            request.Method = "POST";            
            //request.ContentType = "application/json";
            //request.Headers.Add("X-Auth-Token", "");
            StreamWriter writer = new StreamWriter(request.GetRequestStream());
            writer.Write("ra,dec,sep\n");
            writer.Write("256.443154,58.0255,1.0\n");
            writer.Write("29.94136,0.08930,1.0");
            writer.Close();
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            StreamReader reader = new StreamReader(response.GetResponseStream());
            string s = reader.ReadToEnd();
            reader.Close();

            string test = "ra,dec,run,rerun,camCol,field,obj\n";
                  test += "256.443144833208,58.0252475748062,1339,301,1,56,53\n";
                  test += "29.9413538220669,0.0892922027267701,4263,301,4,315,171";
                  Assert.AreEqual(test, s);
        }

        [TestMethod]
        public void TestGetImagingNoPos() { 

            string requestUri = ConfigurationManager.AppSettings["WebServiceUri"] + "ImagingQuery/NoPosition";
            string parameters = "izMin=3&izMax=3.01&riMin=0&riMax=0.1&flagsonlist=BRIGHT,EDGE&limit=2";

            var request = (HttpWebRequest)WebRequest.Create(requestUri + "?" + parameters);
            request.Method = "GET";
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            StreamReader reader = new StreamReader(response.GetResponseStream());
            string s = reader.ReadToEnd();
            reader.Close();

            string test =  "run,rerun,camCol,field,obj\n";
                   test += "3170,301,3,34,867\n";
                   test += "1463,301,2,34,18";

            Assert.AreEqual(test, s);
            
        }
        [TestMethod]
        public void TestGetSpectroCone()
        {
            string requestUri = ConfigurationManager.AppSettings["WebServiceUri"] + "SpectroQuery/ConeSpectro";
            string parameters = "radius=5.0&dec=0.2&ra=10&uMin=0&uMax=20&objType=doGalaxy,doStar&limit=2";

            var request = (HttpWebRequest)WebRequest.Create(requestUri + "?" + parameters);
            request.Method = "GET";
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            StreamReader reader = new StreamReader(response.GetResponseStream());
            string s = reader.ReadToEnd();
            reader.Close();

            string test = "plate,mjd,fiberid\n1904,53682,400\n1133,52993,592";
            Assert.AreEqual(test, s);
        }

        [TestMethod]
        public void TestGetSpectroRect()
        {
            string requestUri = ConfigurationManager.AppSettings["WebServiceUri"] + "SpectroQuery/RectangularSpectro";
            string parameters = "ramin=258&ramax=258.2&decmin=64&decmax=64.1&redshiftMin=0&redshiftMax=0.1&zWarning=0&class=galaxy&foramt=csv&limit=2";

            var request = (HttpWebRequest)WebRequest.Create(requestUri + "?" + parameters);
            request.Method = "GET";
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            StreamReader reader = new StreamReader(response.GetResponseStream());
            string s = reader.ReadToEnd();
            reader.Close();

            string test = "plate,mjd,fiberid\n352,51694,378\n350,51691,203";

            Assert.AreEqual(test, s);
        }

        [TestMethod]
        public void TestGetSpectroProx()
        {
            string requestUri = ConfigurationManager.AppSettings["WebServiceUri"] + "SpectroQuery/ProximitySpectro?radius=1.0&searchNearBy=nearest";
            var request = (HttpWebRequest)WebRequest.Create(requestUri);
            request.Method = "POST";
            //request.ContentType = "application/json";
            //request.Headers.Add("X-Auth-Token", "");
            StreamWriter writer = new StreamWriter(request.GetRequestStream());
            writer.Write("ra,dec,sep\n");
            writer.Write("256.443154,58.0255,1.0\n");
            writer.Write("29.94136,0.08930,1.0");
            writer.Close();

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            StreamReader reader = new StreamReader(response.GetResponseStream());
            string s = reader.ReadToEnd();
            reader.Close();

            string test = "ra,dec,plate,mjd,fiberid\n";
            test += "256.44315,58.025253,355,51788,322\n";
            test += "29.941359,0.089300589,403,51871,556";
            Assert.AreEqual(test, s);
        }

        [TestMethod]
        public void TestGetSpectroNoPos()
        {

            string requestUri = ConfigurationManager.AppSettings["WebServiceUri"] + "SpectroQuery/NoPositionSpectro";
            string parameters = "limit=5&zwarning=0&redshiftMin=0.1&redshiftMax=0.2&class=galaxy";

            var request = (HttpWebRequest)WebRequest.Create(requestUri + "?" + parameters);
            request.Method = "GET";
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            StreamReader reader = new StreamReader(response.GetResponseStream());
            string s = reader.ReadToEnd();
            reader.Close();

            string test = "plate,mjd,fiberid\n1753,53383,321\n1753,53383,327\n1753,53383,328\n1753,53383,329\n1753,53383,331";

            Assert.AreEqual(test, s);

        }

        [TestMethod]
        public void TestGetIRCone()
        {
            string requestUri = ConfigurationManager.AppSettings["WebServiceUri"] + "IRSpectraQuery/ConeIR";
            string parameters = "limit=2&irspecparams=typical&ra=271.75&dec=-20.19&radius=5.0&Lcenter=10&Bcenter=0.2&lbRadius=5.0&jMin=&hMin=&kMin=&jMax=&hMax=&kMax=&jhMin=&hkMin=&jhMax=&hkMax=&snrMin=&vhelioMin=&scatterMin=&snrMax=&vhelioMax=&scatterMax=&tempMin=&loggMin=&fehMin=&afeMin=&tempMax=&loggMax=&fehMax=&afeMax=&irTargetFlagsOnList=ignore&irTargetFlagsOffList=ignore&irTargetFlags2OnList=ignore&irTargetFlags2OffList=ignore";

            var request = (HttpWebRequest)WebRequest.Create(requestUri + "?" + parameters);
            request.Method = "GET";
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            StreamReader reader = new StreamReader(response.GetResponseStream());
            string s = reader.ReadToEnd();
            reader.Close();

            string test = "apogee_id,glon,glat,snr,vhelio_avg,vscatter,teff,logg,metals,alphafe\n";
            test +="2M18070338-2007267, 10.06439,  0.22540,130.58,83.4232,0,-1,99,99,99\n";
            test +="2M18070370-2013467,  9.97281,  0.17289,187.884,-51.1309,0,-1,99,99,99";
            Assert.AreEqual(test, s);
        }

        [TestMethod]
        public void TestGetIRGalactic()
        {
            string requestUri = ConfigurationManager.AppSettings["WebServiceUri"] + "IRSpectraQuery/GalacticIR";
            string parameters = "limit=3&format=csv&irspecparams=typical&Lcenter=10&Bcenter=0.2&lbRadius=5.0&jMin=&hMin=&kMin=&jMax=&hMax=&kMax=&jhMin=&hkMin=&jhMax=&hkMax=&snrMin=&vhelioMin=&scatterMin=&snrMax=&vhelioMax=&scatterMax=&tempMin=&loggMin=&fehMin=&afeMin=&tempMax=&loggMax=&fehMax=&afeMax=&irTargetFlagsOnList=ignore&irTargetFlagsOffList=ignore&irTargetFlags2OnList=ignore&irTargetFlags2OffList=ignore";

            var request = (HttpWebRequest)WebRequest.Create(requestUri + "?" + parameters);
            request.Method = "GET";
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            StreamReader reader = new StreamReader(response.GetResponseStream());
            string s = reader.ReadToEnd();
            reader.Close();

            string test = "apogee_id,glon,glat,snr,vhelio_avg,vscatter,teff,logg,metals,alphafe\n";
            test += "2M18070338-2007267, 10.06439,  0.22540,130.58,83.4232,0,-1,99,99,99\n";
            test += "2M18070370-2013467,  9.97281,  0.17289,187.884,-51.1309,0,-1,99,99,99\n";
            test += "2M18064795-2009143, 10.00887,  0.26352,197.755,-0.445241,0,-1,99,99,99";

            Assert.AreEqual(test, s);
        }       

        [TestMethod]
        public void TestGetIRNoPos()
        {

            string requestUri = ConfigurationManager.AppSettings["WebServiceUri"] + "IRSpectraQuery/NoPositionIR";
            string parameters = "limit=3&irTargetFlagsOnList=APOGEE_WISE_DERED&irTargetFlagsOffList=APOGEE_FIRST_LIGHT&irTargetFlags2OnList=APOGEE_BULGE_GIANT&irTargetFlags2OffList=ignore";

            var request = (HttpWebRequest)WebRequest.Create(requestUri + "?" + parameters);
            request.Method = "GET";
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            StreamReader reader = new StreamReader(response.GetResponseStream());
            string s = reader.ReadToEnd();
            reader.Close();



            string test = "apogee_id,glon,glat,snr,vhelio_avg,vscatter,teff,logg,metals,alphafe\n";
            test +="2M08083688+3124261,190.33925, 29.24899,183.526,38.4861,0.0800501,4796.201,3.291358,0.3189631,0.046783\n";
            test+="2M08084488+3153117,189.81831, 29.40482,669.71,25.5522,0.162775,4716.458,2.683368,0.1820424,0.0085982\n";
            test+="2M08085353+3152009,189.84942, 29.42913,506.041,3.7988,0.315062,3962.3,0.8647699,-0.2462844,0.25072";

            Assert.AreEqual(test, s);

        }
    }
}
