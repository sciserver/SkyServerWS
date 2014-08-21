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

        private static string urlbase = string.Format("{0}://{1}{2}", HttpContext.Current.Request.Url.Scheme, HttpContext.Current.Request.Url.Authority, "/" + KeyWords.DR);
        
        private static string ConeSearchURLs = "1. " + urlbase + "/ConeSearch/ConeSearchService?ra=145&dec=34&sr=5 \n\tparameters: ra,dec,sr";
       
        private static string ImgCutout1 = "<td>1. " + urlbase + "/ImgCutout/getjpeg " + "</td><td>  parameters:ra,dec,scale,width,height </td>";
        private static string ImgCutout2 = "<td>2. " + urlbase + "/ImgCutout/getjpegCodec" + "</td><td>  parameters: run,rerun,camcol,field </td>";
        //private static string ImgCutout3 = "<td></td><td></td>";
        private static string ImgCutoutURLs = @"<table border="+0+"><tr>" + ImgCutout1 + "</tr><tr>" + ImgCutout2 + "</tr></table>";

        private static string SIAP1 = "<td>1. " + urlbase + "/SIAP/getSIAP </td><td>parameters: </td> ";
        private static string SIAP2 = "<td>2. " + urlbase + "/SIAP/getSIAPinfo </td><td> parameters : </td>";
        private static string SIAP3 = "<td>3. " + urlbase + "/SIAP/getSIAPinfoAll </td><td> parameters : </td>";
        private static string SIAPURLs = @"<table border=" + 0 + "><tr>" + SIAP1 + "</tr><tr>" + SIAP2 + "</tr><tr>" + SIAP3 + "</tr></table>";


        private static string SDSSFields1 = "<td>1. " + urlbase + "/SDSSFields/FieldArray </td><td>  parameters:</td> ";
        private static string SDSSFields2 = "<td>2. " + urlbase + "/SDSSFields/FieldArrayRect </td><td>  parameters:</td> ";
        private static string SDSSFields3 = "<td>3. " + urlbase + "/SDSSFields/ListOfFields </td><td>  parameters: </td> ";
        private static string SDSSFields4 = "<td>4. " + urlbase + "/SDSSFields/UrlsOfFields </td><td> parameters: </td> ";
        private static string SDSSFieldURLs = @"<table border=" + 0 + "><tr>" + SDSSFields1 + "</tr><tr>" + SDSSFields2 + "</tr><tr>" + SDSSFields3 + "</tr><tr>" + SDSSFields4 + "</tr></table>";

        private static string SqlSearch = "<td>1. <a href=\"" + urlbase + "/SearchTools/SqlSearch?query=select%20top%2010%20ra,dec%20from%20Frame&format=json \">SQLSearch</a> </td><td> parameters: </td> ";
        private static string RadialSearch = "<td>2. " + urlbase + "/SearchTools/RadialSearch?ra=258.2&dec=64&radius=4.1&searchtype=equitorial&limit=10&format=json&fp=none</td><td> parameters: </td> ";
        private static string RectangleSearch = "<td>3. " + urlbase + "/SearchTools/RectangularSearch?min_ra=258.2&max_ra=258.3&min_dec=64&max_dec=64.1&searchtype=equitorial&limit=10&format=json</td><td> parameters: </td> ";
        private static string SearchToolsURLs = @"<table border=" + 0 + "><tr>" + SqlSearch + "</tr><tr>" + RadialSearch + "</tr><tr>" + RectangleSearch + "</tr></table>";


        private static string ImagingQuery1 = "<td>1. " + urlbase + "/ImagingQuery/Cone?radius=5.0&dec=0.2&ra=10&objType=doGalaxy,doStar&uMin=0&uMax=20</td><td> parameters:ra,dec,radius,objType,uMin,uMax </td> ";
        private static string ImagingQuery2 = "<td>2. " + urlbase + "/ImagingQuery/NoPosition</td><td> parameters: </td> ";
        private static string ImagingQuery3 = "<td>3. " + urlbase + "/ImagingQuery/Proximity</td><td> parameters: </td> ";
        private static string ImagingQuery4 = "<td>4. " + urlbase + "/ImagingQuery/Rectangular</td><td> parameters: </td>";
        private static string ImagingQueryURLs = @"<table border=" + 0 + "><tr>" + ImagingQuery1 + "</tr><tr>" + ImagingQuery2 + "</tr><tr>" + ImagingQuery3 + "</tr><tr>" + ImagingQuery4 + "</tr></table>";

        private static string SpectroQuery1 = "<td>1. " + urlbase + "/SpectroQuery/Cone</td><td> parameters: </td>";
        private static string SpectroQuery2 = "<td>2. " + urlbase + "/SpectroQuery/NoPosition</td><td> parameters: </td> ";
        private static string SpectroQuery3 = "<td>3. " + urlbase + "/SpectroQuery/Proximity</td><td> parameters: </td> ";
        private static string SpectroQuery4 = "<td>4. " + urlbase + "/SpectroQuery/Rectangular</td><td> parameters: </td> ";
        private static string spectraQueryURLs = @"<table border=" + 0 + "><tr>" + SpectroQuery1 + "</tr><tr>" + SpectroQuery2 + "</tr><tr>" + SpectroQuery3 + "</tr><tr>" + SpectroQuery4 + "</tr></table>";

        private static string IRSpectraQuery1 = "<td>1. " + urlbase + "/IRSpectraQuery/ConeIR</td><td> parameters: </td>";
        private static string IRSpectraQuery2 = "<td>2. " + urlbase + "/IRSpectraQuery/GalacticIR</td><td> parameters: </td> ";
        private static string IRSpectraQuery3 = "<td>3. " + urlbase + "/IRSpectraQuery/NoPositionIR</td><td> parameters: </td> ";
        private static string IRSpectraQueryURLs = @"<table border=" + 0 + "><tr>" + IRSpectraQuery1 + "</tr><tr>" + IRSpectraQuery2 + "</tr><tr>" + IRSpectraQuery3 + "</tr></table>";

        private static string ObjectCrossId1 = "<td>1. " + urlbase + "/ObjectCrossId/ImgCrossId</td><td> parameters: </td>";
        private static string ObjectCrossId2 = "<td>2. " + urlbase + "/ObjectCrossId/IRSpectraCrossId</td><td> parameters: </td>";
        private static string ObjectCrossId3 = "<td>3. " + urlbase + "/ObjectCrossId/SpectraCrossId</td><td> parameters: </td>";
        private static string crossidURLs = @"<table border=" + 0 + "><tr>" + ObjectCrossId1 + "</tr><tr>" + ObjectCrossId2 + "</tr><tr>" + ObjectCrossId3 + "</tr></table>";
        ///http://localhost:2997/dr10/SearchTools/RectangularSearch?min_ra=258.2&max_ra=258.3&min_dec=64&max_dec=64.1&searchtype=equitorial&limit=10&format=%22html%22
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
            int cols = 3;//Int32.Parse(txtCols.Text);

            for (int i = 0; i < rows; i++)
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
                        //case 0: lblNew.Text = serviceNames[i]; break;
                        //case 1: lblNew.Text = serviceDesc[i]; break;
                        //case 2: lblNew.Text = serviceURLs[i]; break;
                        //default: lblNew.Text = ""; break;

                        case 0: lit.Text = serviceNames[i]; break;
                        case 1: lit.Text = serviceDesc[i]; break;
                        case 2: lit.Text = serviceURLs[i]; break;
                        default: lit.Text = ""; break;
                    }
                    //System.Web.UI.WebControls.Image imgNew = new System.Web.UI.WebControls.Image();
                    //imgNew.ImageUrl = "cellpic.png";

                    HtmlString h = new HtmlString("<table border='1'><tr>T1</tr><tr>TEST</tr></table>");
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

        private string[] serviceNames = new string[10] { "Service Name", "ConeSearch", "ImgCutout", "SIAP", "SDSSFields", "SearchTools", "ImagingQuery", "SpectroQuery", "IRSpectra", "ObjectCrossid" };
        private string[] serviceDesc = new string[10] { "Service Description", ConeSearchDesc, ImgCutoutDesc, SIAPDesc, SDSSFieldDesc, SearchToolsDesc, ImagingQueryDesc, spectraQueryDesc, IRSpectraQueryDesc, crossidDesc };
        private string[] serviceURLs = new string[10] { "Service URLs", ConeSearchURLs, ImgCutoutURLs, SIAPURLs, SDSSFieldURLs, SearchToolsURLs, ImagingQueryURLs, spectraQueryURLs, IRSpectraQueryURLs, crossidURLs };

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

        //private static string urlbase = string.Format("{0}://{1}{2}", HttpContext.Current.Request.Url.Scheme, HttpContext.Current.Request.Url.Authority, HttpContext.Current.Request.Url);
        
    }
}