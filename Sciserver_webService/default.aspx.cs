using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Sciserver_webService.Common;

namespace Sciserver_webService
{
    public partial class _default : System.Web.UI.Page
    {
        //private static string urlbase = string.Format("{0}://{1}{2}{3}", HttpContext.Current.Request.Url.Scheme, HttpContext.Current.Request.Url.Authority, HttpContext.Current.Request.ApplicationPath,"<DR>" );

        private static string urlbase = HttpContext.Current.Request.Url.AbsoluteUri.ToLower().Replace("default.aspx","<DR>");

        //cone search 1

        private static string ConeSearch1 = urlbase + "/ConeSearch/ConeSearchService?ra=145&dec=34&sr=1 \n\t";
        private static string coneParams = " ra,dec,sr";

        //ImgCutout 2
        private static string ImgCutout1 = urlbase + "/ImgCutout/getjpeg?ra=134&dec=32&scale=0.7&width=512&height=512 ";
        private static string imgcutParams1 = "ra,dec,scale,width,height";

        private static string ImgCutout2 = urlbase + "/ImgCutout/getjpegCodec?R=94&C=1&F=11&Z=50";
        private static string imgcutParams2 = "run(R),camcol(C),field(F),zoom(Z)";

        //SIAP 3
        private static string SIAP1 = urlbase + "/SIAP/getSIAP?POS=132,12&SIZE=0.1&FORMAT=image/jpeg";
        private static string siapParams1 = "pos,format,size";

        private static string SIAP2 = urlbase + "/SIAP/getSIAPinfo?POS=132,12&SIZE=0.1&FORMAT=metadata&bandpass=i";
        private static string siapParams2 = "pos,size,format,bandpass";

        private static string SIAP3 = urlbase + "/SIAP/getSIAPinfoAll?POS=132,12&SIZE=0.01";
        private static string siapParams3 = "pos,size";

        //sdssFields 4
        private static string SDSSFields1 = urlbase + "/SDSSFields/FieldArray?ra=132&dec=12&radius=10&format=json";
        private static string fieldsParams1 = "ra,dec,radius";

        private static string SDSSFields2 = urlbase + "/SDSSFields/FieldArrayRect?ra=132&dec=12&dra=0.1&ddec=0.1&format=json";
        private static string fieldsParams2 = "ra,dec,dra,ddec";

        private static string SDSSFields3 = urlbase + "/SDSSFields/ListOfFields?ra=132&dec=12&radius=10";
        private static string fieldsParams3 = "ra,dec,radius | optional : format";

        private static string SDSSFields4 = urlbase + "/SDSSFields/UrlsOfFields?ra=132&dec=12&radius=10&band=i,z&format=json";
        private static string fieldsParams4 = "ra,dec,radius,band | optional Parameter: format";
        
        //Search Tools 3
        private static string SqlSearch = urlbase + "/SearchTools/SqlSearch?cmd=select top 10 ra,dec from Frame&format=csv";
        private static string sqlParam = "cmd,format";
        //whichway=equitorial&ra=258.25&dec=64.05&radius=3&min_u=0&max_u=20&min_g=0&max_g=20&min_r=0&max_r=20&min_i=0&max_i=20&min_z=0&max_z=20&format=html&limit=10
        private static string RadialSearch = urlbase + "/SearchTools/RadialSearch?ra=258.2&dec=64&radius=4.1&whichway=equitorial&limit=10&format=json&fp=none&uband=0,17&gband=0,15&whichquery=imaging";
        private static string radialParam = "ra,dec,radius,searchtype,limit,format,fp \n optional parameters: uband,gband,rband,iband,zband";

        private static string RectangleSearch = urlbase + "/SearchTools/RectangularSearch?min_ra=250.2&max_ra=250.5&min_dec=35.1&max_dec=35.5&searchtype=equitorial&limit=10&format=json&whichquery=irspectra";
        private static string rectParam = "min_ra,max_ra,min_dec,max_dec,searchtype,limit,format \n optional parameters: uband,gband,rband,iband,zband";

        //imageQyery 4
        private static string ImagingQuery1 = urlbase + "/ImagingQuery/Cone?limit=50&format=csv&imgparams=minimal&specparams=none&ra=10&dec=0.2&radius=5.0&magType=model&uMin=0&gMin=&rMin=&iMin=&zMin=&uMax=20&gMax=&rMax=&iMax=&zMax=&ugMin=&grMin=&riMin=&izMin=&ugMax=&grMax=&riMax=&izMax=&objType=doGalaxy,doStar&minQA=&flagsOnList=ignore&flagsOffList=ignore";
        private static string imgParams1 = "ra,dec,radius,objType,uMin,uMax";
        //limit=50&format=csv&imgparams=minimal&specparams=none&ra=10&dec=0.2&radius=5.0&magType=model&uMin=0&gMin=&rMin=&iMin=&zMin=&uMax=20&gMax=&rMax=&iMax=&zMax=&ugMin=&grMin=&riMin=&izMin=&ugMax=&grMax=&riMax=&izMax=&objType=doGalaxy,doStar&minQA=&flagsOnList=ignore&flagsOffList=ignore
        private static string ImagingQuery2 = urlbase + "/ImagingQuery/NoPosition?limit=30&izMin=3&izMax=4&riMin=&riMax=&flagsonlist=BRIGHT,EDGE&magType=model&uMin=0&gMin=&rMin=&iMin=&zMin=&uMax=20&gMax=&rMax=&iMax=&zMax=&ugMin=&grMin=&ugMax=&grMax=&objType=doGalaxy,doStar&minQA=&flagsOffList=ignore";
        private static string imgParams2 = "OptionalParams :izMin,izMax,riMin,riMax,flagsonlist";

        private static string ImagingQuery3 = urlbase + "/ImagingQuery/Proximity";//?radius=1.0&searchNearBy=nearest";
        private static string imgParams3 = "POST : radius,searchNearBy,nearest";

        private static string ImagingQuery4 = urlbase + "/ImagingQuery/Rectangular?limit=50&izMin=3&izMax=4&riMin=0&riMax=20&flagsonlist=BRIGHT,EDGE&ramin=258&ramax=258.2&decmin=64&decmax=64.1&imgparams=typical,minimal&magType=model&uMin=&gMin=&rMin=&iMin=&zMin=&uMax=&gMax=&rMax=&iMax=&zMax=&ugMin=&grMin=&ugMax=&grMax=&objType=doGalaxy,doStar&minQA=&flagsOffList=ignore&format=csv";
        private static string imgParams4 = "ramin,decmin,ramax,decmax";        

        //spectro Query 4
        private static string SpectroQuery1 = urlbase + "/SpectroQuery/ConeSpectro?radius=5.0&dec=0.2&ra=10&uMin=0&uMax=20&objType=doGalaxy,doStar&limit=50&format=csv&specparams=minimal&imgparams=none&redshiftMin=&zWarning=on&redshiftMax=&priFlagsOnList=ignore&priFlagsOffList=ignore&secFlagsOnList=ignore&secFlagsOffList=ignore&magType=model&gMin=&rMin=&iMin=&zMin=&gMax=&rMax=&iMax=&zMax=&ugMin=&grMin=&riMin=&izMin=&ugMax=&grMax=&riMax=&izMax=&minQA=&flagsOnList=ignore&flagsOffList=ignore";
        private static string specParams1 = "ra,dec,radius,uMin,uMax,gMin,gMax,rMin,rMax,iMin,iMax,zMin,zMax";        

        private static string SpectroQuery2 = urlbase + "/SpectroQuery/NoPositionSpectro?uMin=0&uMax=20&objType=doGalaxy,doStar&limit=50&format=csv&specparams=minimal&imgparams=none&redshiftMin=&zWarning=on&redshiftMax=&priFlagsOnList=ignore&priFlagsOffList=ignore&secFlagsOnList=ignore&secFlagsOffList=ignore&magType=model&gMin=&rMin=&iMin=&zMin=&gMax=&rMax=&iMax=&zMax=&ugMin=&grMin=&riMin=&izMin=&ugMax=&grMax=&riMax=&izMax=&minQA=&flagsOnList=ignore&flagsOffList=ignore";
        private static string specParams2 = "uMin,uMax,gMin,gMax,rMin,rMax,iMin,iMax,zMin,zMax,flagonlist,flagofflist";

        private static string SpectroQuery3 = urlbase + "/SpectroQuery/ProximitySpectro?radius=1.0&searchNearBy=nearest";
        private static string specParams3 = "POST: list of ra,dec,radius ";

        private static string SpectroQuery4 = urlbase + "/SpectroQuery/RectangularSpectro?ramin=258&ramax=258.2&decmin=64&decmax=64.1&redshiftMin=0&redshiftMax=0.1&zWarning=0&class=galaxy&uMin=0&uMax=20&objType=doGalaxy,doStar&limit=50&format=csv&specparams=minimal&imgparams=none&priFlagsOnList=ignore&priFlagsOffList=ignore&secFlagsOnList=ignore&secFlagsOffList=ignore&magType=model&gMin=&rMin=&iMin=&zMin=&gMax=&rMax=&iMax=&zMax=&ugMin=&grMin=&riMin=&izMin=&ugMax=&grMax=&riMax=&izMax=&minQA=&flagsOnList=ignore&flagsOffList=ignore";
        private static string specParams4 = "ramin,ramax,decmin,decmax,redshiftmin,redshiftmax,zwarning,class";

        //irspectro 3
        private static string IRSpectraQuery1 = urlbase + "/IRSpectraQuery/ConeIR?limit=50&format=json&irspecparams=typical&ra=271.75&dec=-20.19&radius=5.0&Lcenter=10&Bcenter=0.2&lbRadius=5.0&jMin=&hMin=&kMin=&jMax=&hMax=&kMax=&jhMin=&hkMin=&jhMax=&hkMax=&snrMin=&vhelioMin=&scatterMin=&snrMax=&vhelioMax=&scatterMax=&tempMin=&loggMin=&fehMin=&afeMin=&tempMax=&loggMax=&fehMax=&afeMax=&irTargetFlagsOnList=ignore&irTargetFlagsOffList=ignore&irTargetFlags2OnList=ignore&irTargetFlags2OffList=ignore";
        private static string irspecParams1 = "irspecparams,ra,dec,radius ..";

        private static string IRSpectraQuery2 = urlbase + "/IRSpectraQuery/GalacticIR?limit=50&format=csv&irspecparams=typical&Lcenter=10&Bcenter=0.2&lbRadius=5.0&jMin=&hMin=&kMin=&jMax=&hMax=&kMax=&jhMin=&hkMin=&jhMax=&hkMax=&snrMin=&vhelioMin=&scatterMin=&snrMax=&vhelioMax=&scatterMax=&tempMin=&loggMin=&fehMin=&afeMin=&tempMax=&loggMax=&fehMax=&afeMax=&irTargetFlagsOnList=ignore&irTargetFlagsOffList=ignore&irTargetFlags2OnList=ignore&irTargetFlags2OffList=ignore";
        private static string irspecParams2 = "";

        private static string IRSpectraQuery3 = urlbase + "/IRSpectraQuery/NoPositionIR?limit=50&format=csv&irspecparams=typical&jMin=&hMin=&kMin=&jMax=&hMax=&kMax=&jhMin=&hkMin=&jhMax=&hkMax=&snrMin=&vhelioMin=&scatterMin=&snrMax=&vhelioMax=&scatterMax=&tempMin=&loggMin=&fehMin=&afeMin=&tempMax=&loggMax=&fehMax=&afeMax=&irTargetFlagsOnList=ignore&irTargetFlagsOffList=ignore&irTargetFlags2OnList=ignore&irTargetFlags2OffList=ignore";
        private static string irspecParams3 = "";
        
        //objid 3
        private static string ObjectCrossId1 = urlbase + "/ObjectCrossId/ImgCrossId";
        private static string objCross1 = "";

        private static string ObjectCrossId2 = urlbase + "/ObjectCrossId/IRSpectraCrossId";
        private static string objCross2 = "";

        private static string ObjectCrossId3 = urlbase + "/ObjectCrossId/SpectraCrossId";
        private static string objCross3 = "";
                
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            this.cmdCreate_Click(sender,e);
        }


        protected void cmdCreate_Click(object sender, System.EventArgs e)
        {
            tbl.Controls.Clear();

            int rows = 10; //Int32.Parse(txtRows.Text);
            int cols = 4;//Int32.Parse(txtCols.Text);

            for (int i = 0; i < rows-1; i++)
            {
                TableRow rowNew = new TableRow();
                tbl.Controls.Add(rowNew);
                for (int j = 0; j < cols; j++)
                {
                    TableCell cellNew = new TableCell();
                    
                    Label lblNew = new Label();
                   
                    LiteralControl lit = new LiteralControl();
                    //lblNew.Text = "(" + i.ToString() + "," + j.ToString() + ")<br />";
                    switch(j){
                        
                        case 0: lit.Text = serviceNames[i]; break;
                        case 1: lit.Text = serviceDesc[i]; break;
                        //case 2: lit.Text = serviceURLs[i]; break;
                        case 2: if (i == 0) lit.Text = "Service Urls";
                                else
                                lit.Text = tableNew(serviceUrls[i-1], serviceUrls[i-1].Length, true); 
                                break;
                        case 3: if (i == 0) lit.Text = "Service Parameters";
                                else
                                    lit.Text = tableNew(serviceParams[i-1], serviceParams[i-1].Length, false); 
                                break;
                        default: lit.Text = "test"; break;
                    }
                    //System.Web.UI.WebControls.Image imgNew = new System.Web.UI.WebControls.Image();
                    //imgNew.ImageUrl = "cellpic.png";

                    //HtmlString h = new HtmlString("<table border='1'><tr>T1</tr><tr>TEST</tr></table>");
                    //lit.Text = lblNew.Text;
                    cellNew.Text = lit.Text;
                    //cellNew.Controls.Add(lit);
                    //cellNew.Controls.Add(lblNew);
                    //cellNew.Controls.Add(imgNew);

                    //if (chkBorder.Checked == true)
                    //{
                        cellNew.BorderStyle = BorderStyle.Groove;
                        cellNew.BorderWidth = Unit.Pixel(1);
                    //}

                    rowNew.Controls.Add(cellNew);
                }
            }
        }

        private string tableNew(string[] table, int numservice,bool isurl) { 
            int rows = numservice; //Int32.Parse(txtRows.Text);
            int cols = 1;//Int32.Parse(txtCols.Text);
            //string text = "<style>table,th,td{border:1px solid black;border-collapse:collapse;}</style><table>";
            string text = "<table cellspacing=\"10\">";
            for (int i = 0; i < rows; i++)
            {                
                for (int j = 0; j < cols; j++)
                {
                    text += "<tr>";
                    text += "<td>";
                    if (isurl) text += "<a href=\"" + table[i] + "\">" + table[i] + "</a>";
                    else text += table[i];
                    text += "</td>";
                    text += "</tr>";
                }                
            }
            text += "</table>";
            return text;
        }

        private string[] serviceNames = new string[10] { "Service Name", "ConeSearch", "ImgCutout", "SIAP", "SDSSFields", "SearchTools", 
                                                         "ImagingQuery", "SpectroQuery", "IRSpectra", "ObjectCrossid" };
        private string[] serviceDesc = new string[10] { "Service Description", ConeSearchDesc, ImgCutoutDesc, SIAPDesc, SDSSFieldDesc, 
                                                        SearchToolsDesc, ImagingQueryDesc, spectraQueryDesc, IRSpectraQueryDesc, crossidDesc };

        private string[][] serviceUrls = {new string[]{ConeSearch1}, new string[]{ImgCutout1,ImgCutout2}, new string[]{SIAP1,SIAP2,SIAP3},
                                          new string[]{SDSSFields1,SDSSFields2,SDSSFields3,SDSSFields4}, new string[]{SqlSearch,RadialSearch,RectangleSearch}, 
                                          new string[]{ImagingQuery1,ImagingQuery2,ImagingQuery3,ImagingQuery4}, new string[]{SpectroQuery1,SpectroQuery2,SpectroQuery3,SpectroQuery4}, 
                                          new string[]{IRSpectraQuery1,IRSpectraQuery2,IRSpectraQuery3}, new string[]{ObjectCrossId1,ObjectCrossId2,ObjectCrossId3}
                                         };
        private string[][] serviceParams = { new string[] {coneParams }, new string[]{imgcutParams1,imgcutParams2}, new string[]{siapParams1,siapParams2,siapParams3} ,
                                             new string[]{fieldsParams1,fieldsParams2,fieldsParams3,fieldsParams4}, new string[]{sqlParam,radialParam,rectParam},
                                             new string[]{ imgParams1,imgParams2,imgParams3,imgParams4}, new string[]{specParams1,specParams2,specParams3,specParams4},
                                             new string[]{irspecParams1,irspecParams2,irspecParams3}, new string[]{objCross1,objCross2,objCross3}
                                           };

        private int[] numService = {1,2,3,4,3,4,4,3,3};

        private static string sloan = "Sloan Digital Sky Survey(SDSS)";
        private static string ConeSearchDesc = "This service returns the Search results for given RA, DEC, SR values from "+sloan;
        private static string ImgCutoutDesc = "This service returns image cutout of a sky from the data observed by " + sloan + ". Inputs should be ra,dec, width, height, scale along with some optional parameters.";
        private static string SIAPDesc = "This service returns positional information and image urls for the given set of input parametes and its divided in three sub services.  It implements IVOA's Simple Image Access Protocol(SIAP).";
        private static string SDSSFieldDesc = "This service returns SDSS fields information. There are four sub web services.";
        private static string SearchToolsDesc = "There are three web services under and all these represent 'Search Tools' on skyserver web site.";
        private static string ImagingQueryDesc = "This set of services represnts Imaging Query tools. They are divided in four sub positional services. Cone,Rectangular,Proximity,NoPosition etc.";
        private static string spectraQueryDesc = "These services represent the SpectroQuery tool of Skyserver search tools.They are divided in four sub positional services. Cone,Rectangular,Proximity,NoPosition etc.";
        private static string IRSpectraQueryDesc = "InfraRed Spectra Query also has four sub web services this represnt the web tool on skyserver. ";
        private static string crossidDesc = "Cross matching the uploaded data with sdss data.";

        
    }
}