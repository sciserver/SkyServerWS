using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.Services;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Net;
using Sciserver_webService.UseCasjobs;
using Sciserver_webService.Common;


namespace Sciserver_webService.ImgCutout
{
    
    public class ImgCutout 
    {
        //-------------------------------------		
        // Limits of the input parameters
        //-------------------------------------
        //int max_zoom = SdssConstants.MaxZoom;
        const int max_width = 2048;
        const int max_height = 2048;
        const int min_width = 64;
        const int min_height = 64;
        const double max_scale = 2400.0;
        const double min_scale = 0.015;

        //-------------------------------------		
        // Limits of the input parameters For 2mass
        //-------------------------------------
        //int max_zoom_2m = SdssConstants.MaxZoomTwoMass;
        const int max_width_2m = 3601;
        const int max_height_2m = 3600;
        const int min_width_2m = 250;
        const int min_height_2m = 250;
        const double max_scale_2m = 60.0;
        const double min_scale_2m = 0.015;

        //-------------------------------------
        // Input parameters
        //-------------------------------------
        private double ra, dec;				    // normalized [ra,dec]
        private Int32 width, height;			// image size
        private double scale;					// arcseconds/pixel

        // private StringBuilder query;	// It can be a SQL SELECT query or  a string like SR(12,20) 
        // which will mark Stars with magnitudes between 12 and 20 in the R band												
        // The code follows the pattern:
        // objType: S | G | P  --	S for Stars
        //							G for Galaxies
        //							P for Both Stars and Galaxies																							
        // bad: u | g | r | i | z | a --  will select objects with
        //		
        //         band  BETWEEN low_mag AND high_mag
        // 
        // if band is 'a' then it will look for all the objects 
        // with values betwen low_mag and high_mag for any band	(compositions of OR)
        // if band and magnitude ranges are not specified then will mark only Stars, 
        // Galaxies, or PhotoPrimary objects 
        //-------------------------------------
        // Derived parameters
        //-------------------------------------
        // zoom: the image pyramid level [0..max_zoom]
        // imageScale: the additional scaling of 
        // the image beyond walking the pyramid
        //-------------------------------------
        private int zoom;					    // zoom level 
        private double ppd;					    // pixels per degree
        private double imageScale;				// scale of the image
        private string imgtype;                 // the image type
        private float zoomScale;			    // reference Coordinate scale
        private float size;					    // size of the symbols drawn
        private double radius, fradius;		    // radius for field searches
        private Hashtable cTable;				// array of astrometry transformations		
        private StringBuilder query;			// It can be a SQL SELECT query or 

        //-------------------------------------
        // Debug support
        //-------------------------------------
        private StringBuilder dbgMsg = new StringBuilder();
        private StringBuilder errMsg = new StringBuilder();
        private bool debug = true;
        //-------------------------------------
        // Drawing options
        //-------------------------------------
        private bool
        draw2Mass = false,
        drawApogee = false,
        drawList		= false,
        drawQuery		= false, 
        drawPlate = false,
        drawPhotoObjs = false,
        drawSpecObjs = false,
        drawTargetObjs = false,
        drawGrid = false,
        drawRuler = false,
        drawLabel = false,
        drawOutline = false,
        drawBoundingBox = false,
        drawField = false,
        drawMask = false,        
        drawFrames = false,
        invertImage = false,
        drawRegion = false,
        isRegionList = false,
        fillRegion = false;
        //-------------------------------------
        // region drawing support
        //-------------------------------------
        private string regionList = "";
        private string regionTypes = "";
        private string viewPort;
        Array ridlist;
        private string imgfield = null;
        //-------------------------------------

        //-------------------------------------
        // Graphics canvas
        //-------------------------------------
        public SDSSGraphicsEnv canvas = null;
        //-------------------------------------

        ////-------------------------------------
        //// DataBase properties
        ////-------------------------------------
        //private string sConnect = null;
        //private string sDataRelease = null;
        //private int sDR = -1;  /// Added for releases after dr7
        //private SqlConnection SqlConn = null;
        //private SqlDataReader reader = null;
        ////-------------------------------------

        ////----------------------------------------------------
        //// DataBase properties for new database only for Image
        //// Added after discussions for DR9/10
        ////----------------------------------------------------
        //private string sConnectImage = null;
        //private SqlConnection SqlConnImage = null;
        //private SqlDataReader readerImage = null;
        ////----------------------------------------------------

        ////----------------------------------------------------
        //// DataBase properties for new database only for 2Mass Images
        ////----------------------------------------------------
        //private string sConnectImage2Mass = null;
        //private SqlConnection SqlConnImage2Mass = null;
        ////private SqlDataReader readerImage2Mass = null;
        ////----------------------------------------------------
        // Authentication token
        private string token = "";
        private string datarelease = "";

        /// <summary>
        /// Constructor and getting connection strings for databases
        /// </summary> 
        public ImgCutout()
        {
            
            //sDataRelease = SdssConstants.sDataRelease;
            //if (sDataRelease == null)
            //    throw new System.Exception("DataRelease keyword not found or invalid.\n" +
            //    "Please check AppSettings in the Web.config file!");
            //sDR = SdssConstants.sDR;

            //sConnect = ConfigurationManager.AppSettings["SkyServer"];
            //sConnectImage = ConfigurationManager.AppSettings["SkyServerImage"];

            //if (sConnect == null || sDataRelease == null || sConnectImage == null)
            //    throw new System.Exception("SkyServer keyword not found or invalid. \n" +
            //    "Please check AppSettings in the Web.config file!");

            //sConnectImage2Mass = ConfigurationManager.AppSettings["SkyServer2Mass"];
            //if (sConnectImage2Mass == null || sDataRelease == null)
            //    throw new System.Exception("SkyServer2Mass keyword not found or invalid. \n" +
            //     "Please check AppSettings in the Web.config file!");
            
        }

      
        /// <summary>
        /// GetJpeg retuns a Jpeg of the image around a point at the specified zoom.
        ///	optionally draws circles/squares/crosses around photo/spectro/target objects
        ///	optionally draws a grid on the output 
        ///	optionally labels the output.
        ///	optionally inverts the image.
        ///	It throws an authorization exception if it cannot connect to the database.
        /// </summary>
        /// <param name="dec"> The image center point declination in J2000 degrees. Should be double in [0...360]</param>
        /// <param name="ra"> The image center point right ascencion in J2000 degrees. Should be double in [-90..90]</param>
        /// <param name="scale"> Arcseconds per pixel. Limited to the range 0.015 .. 60.
        /// The native number for SDSS is 0.396126761"/pixel.
        /// </param>
        /// <param name="height"> The image height in pixels. Should be int in [64 .. 2048] </param>
        /// <param name="width"> The image width in pixels. Should be int in [64 .. 2048], int</param>
        /// <param name="opt"> String coding over drawing requests: 
        ///			'P': draws Photo Objects
        ///			'S': draws Spectro Objects
        ///			'T': draws Target Objects
        ///			'G': draws a Grid
        ///			'R': draws a Ruler
        ///			'L': draws a Label
        ///			'B': draws BoundingBox
        ///			'O': draws Outline
        ///			'M': draws Mask 
        ///			'Q': draws Plate 
        ///			'I': inverts Image </param>
        /// <returns> byte[] JPEG image.</returns>
        /// <example> 
        ///		ws.GetJpeg(ra, dec, scale, height, width, opt)
        /// </example>
       
        public byte[] GetJpeg(
            double ra_,						// right ascension in J2000 degrees
            double dec_, 						// declination in J2000 degrees
            double scale_,						// arcsec/pixel (0.3961267 is native 1:1 for SDSS)
            Int32 width_,						// image width  (in pixels)
            Int32 height_,					// image height (in pixels)
            string opt_,						// drawing options
            string query_,
            string imgtype_,
            string imgfield_,
            string token_
            )
        {
            return GetJpegQuery(ra_, dec_, scale_, width_, height_, opt_, query_, imgtype_, imgfield_, token_);
        }


      
        public byte[] GetJpegQuery(
            double ra_,						// right ascension in J2000 degrees
            double dec_, 						// declination in J2000 degrees
            double scale_,						// arcsec/pixel (0.3961267 is native 1:1 for SDSS)
            Int32 width_,						// image width  (in pixels)
            Int32 height_,					// image height (in pixels)
            string opt_,						// drawing options
            string query_,						// mark objects selected by a query, or Region list
            string imgtype_,
            string imgfield_,
            string token_
            )
        {
            try{

                getImageCutout(ra_, dec_, scale_, width_, height_, opt_, query_, imgtype_, imgfield_, token_);
            }
            catch (Exception e)
            {

                canvas.addDebugMessage(e.Message);
                canvas.drawDebugMessage(width,height);
            }
            
            return (canvas.getBuffer());							// return image
        }

        ///// <summary>
        /////// Drawing options on with json data
        /////// </summary>

        //public string GetJpegQuery64(
        //   double ra_,						// right ascension in J2000 degrees
        //   double dec_, 						// declination in J2000 degrees
        //   double scale_,						// arcsec/pixel (0.3961267 is native 1:1 for SDSS)
        //   Int32 width_,						// image width  (in pixels)
        //   Int32 height_,					// image height (in pixels)
        //   string opt_,						// drawing options
        //   string query_,						// mark objects selected by a query
        //   string imgtype_,
        //   string imgfield_
        //   )
        //{
        //    try
        //    {
        //        getImageCutout(ra_, dec_, scale_, width_, height_, opt_, query_, imgtype_, imgfield_);
        //    }
        //    catch (Exception e)
        //    {
        //        canvas.drawDebugMessage(width,height);
        //    }
        //    finally { disconnectAllDatabases(); }
        //    return (canvas.getBufferBase64());
        //}

        private void getImageCutout(
            double ra_,						// right ascension in J2000 degrees
            double dec_, 						// declination in J2000 degrees
            double scale_,						// arcsec/pixel (0.3961267 is native 1:1 for SDSS)
            Int32 width_,						// image width  (in pixels)
            Int32 height_,					// image height (in pixels)
            string opt_,						// drawing options
            string query_,						// mark objects selected by a query, or Region list
            string imgtype_,
            string imgfield_,
            string token_
            ) {

                try
                {
                    // to get datarelease
                    datarelease = HttpContext.Current.Request.RequestContext.RouteData.Values["anything"] as string; /// which SDSS Data release is to be accessed
                                                                                                                     
                    //-------------------------------------
                    // validate ranges and values of input
                    //-------------------------------------
                    validateInput(ra_, dec_, scale_, height_, width_, opt_, query_, imgtype_, imgfield_);
                    token = token_;
                    if (draw2Mass) SdssConstants.isSdss = false;
                    else SdssConstants.isSdss = true;

                    //-------------------------------------
                    // set image scale and zoom 
                    //-------------------------------------
                    zoom = 0;
                    ppd = 3600.0 / scale;
                    imageScale = ppd / SdssConstants.PixelsPerDegree;
                    while (zoom < SdssConstants.MaxZoom & imageScale <= .5)
                    {
                        zoom++;											    // go higher in the pyramid
                        imageScale *= 2;								        // change the scaling accordingly
                    }
                    zoomScale = (float)Math.Pow(2, zoom);				    // set the scale according to the real zoom
                    size = (float)((zoom > 3) ? 6 : 12 * imageScale);
                    //---------------------------------------------
                    // set SQL search radii for fields and objects
                    //---------------------------------------------
                    radius = 60.0 * 0.5 * Math.Sqrt(width * width + height * height) / ppd;
                    fradius = SdssConstants.FrameHalfDiag + radius;
                    //---------------------------------------------------
                    // initialize the canvas, connection and projection
                    //---------------------------------------------------
                    if (drawQuery) validateQuery(query_);

                    canvas = new SDSSGraphicsEnv(width, height, imageScale, ppd, debug, imgtype);
                    
                    //connectToDataBase();

                    byte oflag = 0;
                    if (drawPhotoObjs) oflag |= SdssConstants.pflag;
                    if (drawSpecObjs) oflag |= SdssConstants.sflag;
                    if (drawTargetObjs) oflag |= SdssConstants.tflag;

                    canvas.InitializeProjection(ra, dec, "TAN"); ///TAN or STR     
                    canvas.GetViewPort();

                    imgfield = imgfield_;

                    if (drawFrames | drawField) getFrames();
                    if (drawQuery) getQueryObjects(query_);
                    
                    OverlayOptions options = new OverlayOptions( canvas, size, ra, dec, radius, zoom, fradius,datarelease, token);

                    if (drawApogee) options.getApogeeObjects();
                    if (drawField) options.getFields(cTable);
                    if (drawPhotoObjs | drawSpecObjs | drawTargetObjs) options.getObjects(drawPhotoObjs, drawSpecObjs, drawTargetObjs);
                    if (drawBoundingBox | drawOutline) options.getOutlines(drawBoundingBox, drawOutline, cTable);
                    if (drawMask) options.getMasks();
                    if (drawLabel) options.getLabel(datarelease, scale, imageScale);
                    if (drawPlate) options.getPlates();
                    if (drawList) getListObjects();
                    if (drawRuler)
                    {
                        canvas.drawRuler();
                        drawGrid = false;
                    }
                    if (drawGrid)
                    {
                        canvas.drawGrid();
                        canvas.drawRuler();
                    }
                    if (invertImage)
                    {
                        canvas.Invert();
                    }
                    if (debug)
                    {
                        canvas.addDebugMessage(dbgMsg.ToString());
                        canvas.drawDebugMessage(width,height);
                    }
                }
                catch (Exception e) { throw e; }
                finally { 
                    ///disconnectFromDataBase(); 
                }
            
        }

        Coord coord = null;
        // coord of current tile            

        /// <summary>
        /// getFrames. Fetch the images and put them onto the canvas.
        /// This is for using sdss and twomass images table separately
        /// @Deoyani N-H
        /// </summary>
        private void getFrames()
        {
            int zoom10x = SdssConstants.zoom10(zoom);
            StringBuilder sQ = new StringBuilder();
                if (draw2Mass)
                {
                    try{

                        
                        //connectToDataBaseImage2Mass();
                        sQ.Append ("SELECT  f.img, f.CRVAL1,f.CRVAL2, f.CRPIX1,f.CRPIX2, f.CDELT1, f.CDELT2,f.fieldid \n");
                        sQ.AppendFormat (" From dbo.fTwoMassGetNearbyFrameEq({0}, {1}, {2}) as n  \n",ra,dec, fradius);
                        sQ.Append(" join  dbo.TwoMassImageFrame as f  on f.fieldid=n.fieldid  \n");

                        RunCasjobs run = new RunCasjobs(sQ.ToString(), token, "ImgCutout:2MASS", KeyWords.contentDataset, "TWOMASSDb");
                        DataSet ds = run.runQuery();

                        //SqlCommand cmd1 = new SqlCommand(sQ.ToString(), SqlConnImage2Mass);
                        //readerImage2Mass = cmd1.ExecuteReader();
                        using (DataTableReader readerImage2Mass = ds.Tables[0].CreateDataReader())
                        {
                            if (!readerImage2Mass.HasRows)
                            {//throw new Exception("Requested (ra, dec) is outside the 2MASS footprint. \n");
                                canvas.addDebugMessage("Requested (ra, dec) is outside the 2MASS footprint. \n");
                                canvas.drawDebugMessage(width, height);
                                return;
                            }

                            while (readerImage2Mass.Read())
                            {

                                Image tile = null;
                                try
                                {
                                    tile = Image.FromStream(new MemoryStream((Byte[])readerImage2Mass[0]));
                                }
                                catch (Exception e)
                                {

                                    tile = null;

                                }

                                if (tile != null)
                                {
                                    coord = new Coord(
                                        Convert.ToDouble(readerImage2Mass[1]),
                                        Convert.ToDouble(readerImage2Mass[2]),
                                        Convert.ToDouble(readerImage2Mass[3]),
                                        Convert.ToDouble(readerImage2Mass[4]),
                                        Convert.ToDouble(readerImage2Mass[5]),
                                        Convert.ToDouble(readerImage2Mass[6]));

                                    coord.m = canvas.getAffineTransform(coord);

                                    canvas.drawFrame(coord, tile);
                                }
                            }
                            if (readerImage2Mass != null) readerImage2Mass.Close();
                            //canvas.drawWarning(" Two MASS Image !");

                        }
                       
                    }
                    catch(Exception e){
                        showException("Exception in getFrame() Test ::", sQ.ToString(), e);
                    }
                    finally{
                       ///disconnectFromDataBaseImage2Mass();
                    }

                }
                else
                {
                    try
                    {
                        //connectToDataBaseImage();
                        sQ.Append("SELECT img , f.a, f.b, f.c, f.d, f.e, f.f, f.node, f.incl, f.ra, f.dec, f.fieldID, \n");
                        sQ.Append(" dbo.fSDSS(f.fieldID) , f.run, f.camcol, f.rerun,f.field \n");
                        sQ.AppendFormat("FROM dbo.fGetNearbyFrameEq({0}, {1}, {2}, {3}) as n JOIN Frame f \n", ra, dec, fradius, zoom10x);
                        sQ.AppendFormat("ON f.fieldID = n.fieldID  and f.zoom =  {0} \n", zoom10x);
                        sQ.Append(" and f.iflag = 1  and f.ifieldflag=1 order by f.iorder");

                        RunCasjobs run = new RunCasjobs(sQ.ToString(), token, "ImgCutout:SDSS", KeyWords.contentDataset, "sdssimgdb");
                        DataSet ds = run.runQuery();

                        using (DataTableReader reader = ds.Tables[0].CreateDataReader())
                        {
                            IntPtr imageFromBytedata = IntPtr.Zero;
                            cTable = new Hashtable();
                            // SqlCommand cmd = new SqlCommand(sQ.ToString(), SqlConnImage);
                            //reader = cmd.ExecuteReader();

                            if (!reader.HasRows)
                            {
                                //throw new Exception("Requested (ra, dec) is outside the SDSS footprint. \n");
                                canvas.addDebugMessage("Requested (ra, dec) is outside the SDSS footprint. \n");
                                canvas.drawDebugMessage(width, height);
                                return;
                            }

                            while (reader.Read())
                            {
                                //----------------------------------------------
                                // get the astrometry coordinates of this tile
                                //----------------------------------------------
                                coord = new Coord(
                                    Convert.ToDouble(reader[1]),		// a
                                    Convert.ToDouble(reader[2]),		// b 
                                    Convert.ToDouble(reader[3]),		// c
                                    Convert.ToDouble(reader[4]),		// d
                                    Convert.ToDouble(reader[5]),		// e
                                    Convert.ToDouble(reader[6]),		// f
                                    Convert.ToDouble(reader[7]),		// node
                                    Convert.ToDouble(reader[8]), 		// inclination
                                    zoomScale,							// zoomScale
                                    Convert.ToString(reader[12])		// info
                                    );

                                if (debug)
                                {
                                    //canvas.addDebugMessage("info="+ coord.info+" "+Convert.ToString(reader[11])+"\n");
                                }

                                // compute the affine transformation
                                coord.m = canvas.getAffineTransform(coord);
                                // fetch fieldId, and save coord in Hashtable
                                string fieldId = Convert.ToString(reader[11]);
                                cTable.Add(fieldId, coord);

                                if (drawFrames)
                                {
                                    //-----------------------------------------
                                    // fetch the tile into the memory stream
                                    //-----------------------------------------
                                    Image tile = Image.FromStream(new MemoryStream((Byte[])reader[0]));

                                    //--------------------------------
                                    // draw the tile onto the canvas
                                    //--------------------------------
                                    if (tile != null)
                                    {
                                        canvas.drawFrame(coord, tile);

                                    }
                                    tile.Dispose();
                                }
                            }
                            if (reader != null) reader.Close();
                        }
                    }
                    catch (Exception exp)
                    {
                        showException("Exception in getFrame()",sQ.ToString(),exp);
                    }
                    finally {
                        //disconnectFromDataBaseImage();
                    }
                }            
         
        }


        //%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
        //%%%%%%%%%%%%%  utilities %%%%%%%%%%%%%%%
        //%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%

        ///<summary>
        /// validateInput(). Validate the range limits the input parameters.
        ///</summary>		
        private void validateInput(double ra_, double dec_, double scale_,
                                   int height_, int width_, string opt_,
                                   string query_, string imgtype_, string imgfield_)
        {
            // Normalize ra and dec
            ra = ra_;
            dec = dec_;
/*
            dec = dec % 180;					// bring dec within the circle
            if (Math.Abs(dec) > 90)				// if it is "over the pole",
            {
                dec = (dec - 90) % 180;			// bring int back to the [-90..90] range
                ra += 180;						// and go 1/2 way round the globe
            }
            ra = ra % 360;					// bring ra into [0..360]
            if (ra < 0) ra += 360;
*/
            // modified by Manuchehr Taghizadeh-Popp on 08/31/2015
            dec = dec % 360;					// brings dec into [0..360]
            if (dec < 0)
            {
                dec = dec + 360;     // only allows positive dec values
            }
            else if (dec > 90 && dec < 270) // if dec is at the other side of the poles
            {
                ra = ra + 180;// go 1/2 way around the globe
                dec = 180 - dec;
            }
            else if (dec >= 270)  // if dec is at this side from the south pole
            { 
                dec = dec - 360;
            }
            ra = ra % 360;// brings ra into [0..360]


            for (int i = 0; i < opt_.Length; i++)
            {
                char c = opt_[i];
                switch (c)
                {
                    case 'B': drawBoundingBox = true;
                        break;
                    case 'C': drawFrames = true;
                        break;
                    case 'F': drawField = true;
                        break;
                    case 'G': drawGrid = true;
                        break;
                    case 'I': invertImage = true;
                        break;
                    case 'L': drawLabel = true;
                        break;
                    case 'M': drawMask = true;
                        break;
                    case 'O': drawOutline = true;
                        break;
                    case 'P': drawPhotoObjs = true;
                        break;
                    case 'S': drawSpecObjs = true;
                        break;
                    case 'T': drawTargetObjs = true;
                        break;
                    case 'Q': drawPlate = true;
                        break;
                    case 'X': draw2Mass = true;
                        break;
                    case 'A': drawApogee = true;
                        break;
                    case '?': debug = true;
                        break;
                }
            }


            if (query_.Length > 0) drawQuery = true;

            //--------------------------------
            // clip height, width and ppd
            //--------------------------------

            if (draw2Mass)
            {
                height = Math.Max(min_height_2m, Math.Min(max_height_2m, height_));
                width = Math.Max(min_width_2m, Math.Min(max_width_2m, width_));
                scale = Math.Max(min_scale_2m, Math.Min(max_scale_2m, scale_));
            }
            else
            {
                height = Math.Max(min_height, Math.Min(max_height, height_));
                width = Math.Max(min_width, Math.Min(max_width, width_));
                scale = Math.Max(min_scale, Math.Min(max_scale, scale_));
            }
            imgtype = imgtype_;
        }
        


      

        //*******************************************************************************************
        // This section is added since DR8 to get Jpeg Images from the cas directly        
        //*******************************************************************************************
        [WebMethod(BufferResponse = false,
        Description = "Returns the bytes of the Jpeg image for a given pointing"
        + "<br><b>Input 1:</b> run in degrees (double)"
        + "<br><b>Input 2:</b> Dec in degrees (double)"
        + "<br><b>Input 3:</b> Scale, in arcsec/pixel (double)"
        + "<br><b>Input 4:</b> Width in pixels (int)"
        + "<br><b>Output:</b> Image (byte[])")]
        public byte[] GetJpegImg(
            string run,
            string camcol,
            string field,
            string zoom,
            string token
            )
        {
            byte[] bytes = null;

            try
            {
                string cmdStr = "SELECT img FROM Frame WHERE zoom=" + zoom + " AND run="
                              + run + " AND camCol=" + camcol + " AND field=" + field;

                //connectToDataBaseImage();
                //SqlCommand cmd = new SqlCommand(cmdStr, SqlConnImage);
                //reader = cmd.ExecuteReader();
                RunCasjobs runcas = new RunCasjobs(cmdStr, token, "ImgCutout:GetJPGImg", KeyWords.contentDataset, "sdssimgdb");
                        DataSet ds = runcas.runQuery();

               using (DataTableReader reader = ds.Tables[0].CreateDataReader())
               {
                while (reader.Read())       // read the next record in the dataset
                {
                    MemoryStream theJpeg = new MemoryStream();
                    Image img_ = Image.FromStream(new MemoryStream((Byte[])reader[0]));
                    //img_.Save(theJpeg, ImageFormat.Jpeg);

                    EncoderParameters jpegParms = new EncoderParameters(1);
                    jpegParms.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 95L);
                    img_.Save(theJpeg, GetEncoderInfo("image/jpeg"), jpegParms);
                    		
                    bytes = theJpeg.ToArray();
                }
                if (reader != null) reader.Close();    // close the reader.  
               }
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                //disconnectFromDataBaseImage();
            }
            return bytes;
        }

        private static ImageCodecInfo GetEncoderInfo(String mimeType)
        {
            int j;
            ImageCodecInfo[] encoders;
            encoders = ImageCodecInfo.GetImageEncoders();
            for (j = 0; j < encoders.Length; ++j)
            {
                if (encoders[j].MimeType == mimeType)
                    return encoders[j];
            }
            return null;
        }

        ////// ############################################################################################
        //// this section is to draw query

        private enum QUERYTYPE
        {
            SQL = 0,	// SQL
            FLT = 1,	// Filter
            LST = 2,	// List of objects
            UKN = 3
        }

        /// <summary>
        /// getQueryObjects. Mark objects selected by a SQL query.
        /// </summary>
        private void getQueryObjects(string originalQ)
        {
            try
            {
                //set up the data adapter to get our data...
                
                RunCasjobs runcas = new RunCasjobs(query.ToString(), token, "ImgCutout:GetJPGImg", KeyWords.contentDataset, datarelease);
                DataSet ds = runcas.runQuery();
                
                //if (ds.Tables["MarkedObjects"].Columns["error_message"] != null)
                //{
                //    throw new Exception("");
                //}
                //drawTable(ds.Tables["MarkedObjects"]);
                drawTable(ds.Tables[0]);
            }
            catch (Exception e)
            {
                errMsg.AppendFormat("Wrong mark query. {0} \n\nYour input was:\n{1}", e.Message, originalQ);
            }
        }


        /// <summary>
        /// getListObjects(). Mark objects from a list.
        /// The function processes each line creating a DataTable of objects.
        /// The fisrt line in the list must have the name of the columns below.
        /// The list may include as many columns as desired as far as 
        /// RA and DEC are included.
        /// </summary>
        private void getListObjects()
        {
            // create the table that will hold query results
            DataTable dt = new DataTable("Coordinates");
            try
            {
                string[] querylines = query.ToString().Split(new char[] { '\n' });
                string line = cleanLine(querylines[0]);
                string[] columns = line.Split(new char[] { ' ', '\t', ',', ';' });
                for (int j = 0; j < columns.Length; j++)
                    dt.Columns.Add(columns[j]);
                for (int i = 1; i < querylines.Length; i++)
                {
                    line = cleanLine(querylines[i]);
                    string[] fields = line.Split(new char[] { ' ', '\t' });
                    if (fields.Length != columns.Length)
                    {
                        if (line.CompareTo("") == 0) continue;
                        else
                            throw new Exception("Wrong number of columns in row " + i.ToString());
                    }
                    DataRow newRow;
                    newRow = dt.NewRow();
                    for (int j = 0; j < fields.Length; j++)
                    {
                        newRow[columns[j]] = fields[j];
                    }
                    string sra = Convert.ToString(newRow["ra"]);
                    if (sra.IndexOf(":") != -1)
                        newRow["ra"] = Coord.hms2deg(sra).ToString();
                    string sdec = Convert.ToString(newRow["dec"]);
                    if (sdec.IndexOf(":") != -1)
                        newRow["dec"] = Coord.dms2deg(sdec).ToString();
                    dt.Rows.Add(newRow);
                }
                drawTable(dt);
            }
            catch (Exception e)
            {
                errMsg.AppendFormat("Wrong mark query. {0} \n\nYour input was:\n{1}", e.Message, query.ToString());
            }
        }


        /// <summary>
        /// drawTable(): Mark objects included in a table with a triangle 
        /// </summary>
        private void drawTable(DataTable dt)
        {
            double oRa = 0.0, oDec = 0.0;
            foreach (DataRow dataRow in dt.Rows)
            {
                oRa = Convert.ToDouble(dataRow["ra"]);
                oDec = Convert.ToDouble(dataRow["dec"]);
                canvas.drawQueryObj(oRa, oDec, size);
            }
        }

        ///<summary>
        /// getQueryType(). Determines the type of marking query. SQL, List of objects or Filter
        ///</summary>				
        private QUERYTYPE getQueryType(string qry)
        {
            if (qry.IndexOf("SELECT") != -1) return QUERYTYPE.SQL; //SQL
            if ((qry.IndexOf("RA") != -1) && (qry.IndexOf("DEC") != -1)) return QUERYTYPE.LST; //List of objects
            if (qry.IndexOfAny(new char[] { 'S', 'G', 'P' }) != -1) return QUERYTYPE.FLT;
            return QUERYTYPE.UKN;
        }
        ///<summary>
        /// validateQuery(). Filtering queries of the form SR(10,20) are decomposed 
        /// and the query string is composed.
        ///</summary>		
        private bool validateQuery(string myq)
        {
            bool correctQuery = true;
            char objType;
            char band;
            double low_mag, high_mag;
            string query_ = myq.ToUpper();
            QUERYTYPE queryType = getQueryType(query_);
            switch (queryType)
            {
                case QUERYTYPE.SQL:
                    //string server_name = "skyservice";
                    string rows = "500000";
                    //string access = "ImgCutout";

                    /// If the query contains SELECT, the query is considered correct and the validation 
                    /// is postponed to be run by spExecute store procedure.
                    query = new StringBuilder();
                    query.AppendFormat("EXEC spExecuteSQL '{0}', '{1}' ", myq, rows);
                    //This needs to be clarify
                    // EXEC spExecuteSQL '" + c +"  ', 100000,'" + server_name + "','" + windows_name + "','" + remote_addr + "','" + access + "'";
                    break;
                case QUERYTYPE.LST:
                    query = new StringBuilder(myq);
                    drawQuery = false;
                    drawList = true;
                    break;
                case QUERYTYPE.FLT:
                    try
                    {
                        query_ = query_.Replace(" ", ""); //Get rid of the spaces		
                        objType = query_[0];
                        string t = check_and_set_ObjType(objType);
                        if (query_.Length > 1)
                        {
                            band = query_[1];
                            int openB = query_.IndexOf('(');
                            int closeB = query_.IndexOf(')');
                            //it should leave only the numbers
                            StringBuilder magnitudes = new StringBuilder(
                                query_.Substring(openB + 1, closeB - openB - 1));
                            string m = magnitudes.ToString();
                            string[] mag = m.Split(new char[] { ',' });
                            low_mag = Convert.ToDouble(mag[0]);
                            high_mag = Convert.ToDouble(mag[1]);
                            check_and_set_Band(band, t, low_mag, high_mag);
                        }
                    }
                    catch (Exception e)
                    {
                        errMsg.AppendFormat("Wrong mark query. {0}\n\nYour input was:\n{1}", e.Message, myq);
                        correctQuery = false;
                    }
                    break;
                default:
                    errMsg.AppendFormat("Wrong mark query. String in wrong format \n\nYour input was:\n{0}", myq);
                    correctQuery = false;
                    drawQuery = false;
                    break;
            }
            return correctQuery;
        }


        ///<summary>
        /// check_and_set_ObjType(). Determines what type of objects are being filtered and builds the proper SQL query.
        /// Returns the Table alias to be used by the check_and_set_Band() function.
        ///</summary>	
        private string check_and_set_ObjType(char objType)
        {
            string table = "", t = "";
            switch (objType)
            {
                case 'P':
                    table = "PhotoPrimary";
                    t = "p";
                    break;
                case 'G':
                    table = "Galaxy";
                    t = "g";
                    break;
                case 'S':
                    table = "Star";
                    t = "s";
                    break;
                default:
                    throw new Exception("Valid object types: P | S | G \n");
            }
            query = new StringBuilder("SELECT ");
            query.AppendFormat("{0}.ra as ra, {0}.dec as dec \n", t);
            query.AppendFormat("FROM dbo.fGetNearbyObjEq({0},{1},{2}) n JOIN {3} {4} \n",
                ra, dec, radius, table, t);
            query.AppendFormat("ON n.objId = {0}.objId \n", t);
            return t;
        }


        ///<summary>
        /// check_and_set_Band(). Determines what Band is being filtered and builds the proper SQL query.
        ///</summary>
        private void check_and_set_Band(char band, string t, double low_mag, double high_mag)
        {
            switch (band)
            {
                case 'U':
                case 'G':
                case 'R':
                case 'I':
                case 'Z':
                    query.AppendFormat("WHERE {0}.{1} BETWEEN {2} AND {3}",
                        t, band.ToString(), low_mag, high_mag);
                    break;
                case 'A':
                    query.AppendFormat("WHERE {0}.u BETWEEN {1} AND {2} \n",
                        t, low_mag, high_mag);
                    query.AppendFormat("OR {0}.g BETWEEN {1} AND {2} \n",
                        t, low_mag, high_mag);
                    query.AppendFormat("OR {0}.r BETWEEN {1} AND {2} \n",
                        t, low_mag, high_mag);
                    query.AppendFormat("OR {0}.i BETWEEN {1} AND {2} \n",
                        t, low_mag, high_mag);
                    query.AppendFormat("OR {0}.z BETWEEN {1} AND {2} \n",
                        t, low_mag, high_mag);
                    break;
                default:
                    throw new Exception("Valid band values : A | U | G | R | I | Z \n");
            }
        }



        /// <summary>
        /// cleanLine(): Replaces separation caracters ';' and ',' for spaces 
        /// and then double spaces for only one space to avoid unnecessary
        /// columns when building the attributes table.
        /// </summary>
        private string cleanLine(string ln)
        {
            string line = ln;
            line = line.Replace(";", " ");
            line = line.Replace(",", " ");
            while (line.IndexOf("  ") != -1)
            {
                line = line.Replace("  ", " ");
            }
            line = line.Trim();
            return line;
        }

        /////// Database connections ###############################################################################
        /// <summary>
        /// Assemble a generic message and throw the Exception
        /// </summary>
        public void showException(string sFunction, string sQuery, Exception e)
        {
            StringBuilder msg = new StringBuilder();
            msg.AppendFormat("{0} has failed:\n{1}\n", sFunction, sQuery);
            msg.AppendFormat("Exception Message:{0}", e.Message);
            throw new Exception(msg.ToString()); //Actual exception
        }


        ///// <summary>
        ///// Disconnect from Database.
        ///// </summary>
        //private void disconnectFromDataBase()
        //{
        //    if (reader != null) reader.Close();	// deallocate the SQL connection
        //    if (SqlConn != null)				// close the SQL connection
        //    {
        //        SqlConn.Close();
        //        SqlConn.Dispose();
        //    }
        //}
        ///// <summary>
        ///// Connect to Database.
        ///// </summary>
        //private void connectToDataBase()
        //{
        //    SqlConn = new SqlConnection(sConnect);
        //    try { SqlConn.Open(); }
        //    catch (Exception e)
        //    {
        //        StringBuilder msg = new StringBuilder();
        //        msg.AppendFormat("Cannot connect to Database: {0}, Source Data: {1}\n",
        //                          SqlConn.Database, SqlConn.DataSource);
        //        throw new Exception(msg.ToString() + e.Message);
        //    }
        //}
        ////All this is added if we use two different souces of data one specifically for images
        /// <summary>
        /// Disconnect from Database.
        /// </summary>
        //private void disconnectFromDataBaseImage()
        //{
        //    if (readerImage != null) readerImage.Close();	// deallocate the SQL connection
        //    if (SqlConnImage != null)				// close the SQL connection
        //    {
        //        SqlConnImage.Close();
        //        SqlConnImage.Dispose();
        //    }
        //}

        ///// <summary>
        ///// Connect to Database.
        ///// </summary>
        //private void connectToDataBaseImage()
        //{
        //    SqlConnImage = new SqlConnection(sConnectImage);
        //    try { SqlConnImage.Open(); }
        //    catch (Exception e)
        //    {
        //        StringBuilder msgImage = new StringBuilder();
        //        msgImage.AppendFormat("Cannot connect to Database: {0}, Source Data: {1}\n",
        //                          SqlConnImage.Database, SqlConnImage.DataSource);
        //        throw new Exception(msgImage.ToString() + e.Message);
        //    }
        //}


        //private void connectToAllDatabases() {
        //    connectToDataBase(); //Main SDSS
        //    connectToDataBaseImage(); // only for images db
        //    connectToDataBaseImage2Mass(); // To get only 2mass data         
        //}

        //private void disconnectAllDatabases() {
        //    disconnectFromDataBase();
        //    disconnectFromDataBaseImage();
        //    disconnectFromDataBaseImage2Mass();            
        //}

        ////*******************************************************************************************
        //// This is for Two Mass Images access   
        ////*******************************************************************************************     
        ///// <summary>
        ///// Disconnect from Database.
        ///// </summary>
        //private void disconnectFromDataBaseImage2Mass()
        //{
        //    if (readerImage2Mass != null) readerImage2Mass.Close();	// deallocate the SQL connection
        //    if (SqlConnImage2Mass != null)				// close the SQL connection
        //    {
        //        SqlConnImage2Mass.Close();
        //        SqlConnImage2Mass.Dispose();
        //    }
        //}


        ///// <summary>
        ///// Connect to Database.
        ///// </summary>
        //private void connectToDataBaseImage2Mass()
        //{
        //    SqlConnImage2Mass = new SqlConnection(sConnectImage2Mass);
        //    try { SqlConnImage2Mass.Open(); }
        //    catch (Exception e)
        //    {
        //        StringBuilder msgImage2Mass = new StringBuilder();
        //        msgImage2Mass.AppendFormat("Cannot connect to Database: {0}, Source Data: {1}\n",
        //                          SqlConnImage2Mass.Database, SqlConnImage2Mass.DataSource);
        //        throw new Exception(msgImage2Mass.ToString() + e.Message);
        //    }
        //}
        
        /// <summary>
        /// 
        /// </summary>
        [WebMethod(BufferResponse = false,
        Description = "Returns the bytes of the Jpeg image for a given pointing"
        + "<br><b>Input 1:</b> run in degrees (double)"
        + "<br><b>Input 2:</b> Dec in degrees (double)"
        + "<br><b>Input 3:</b> Scale, in arcsec/pixel (double)"
        + "<br><b>Input 4:</b> Width in pixels (int)"
        + "<br><b>Output:</b> Image (byte[])")]
        public byte[] Get2MassJpegImg(
            string id,
            string zoom
            )
        {
            byte[] bytes = null;

            try
            {
                string cmdStr = "SELECT img FROM Frame2Mass WHERE zoom=" + zoom + " AND id=" + id;

                RunCasjobs run = new RunCasjobs(cmdStr, token, "ImgCutout:2MASS", KeyWords.contentDataset, "TWOMASSDb");
                DataSet ds = run.runQuery(); 
                using (DataTableReader reader = ds.Tables[0].CreateDataReader())
                {
                    while (reader.Read())       // read the next record in the dataset
                    {
                        MemoryStream theJpeg = new MemoryStream();
                        Image img_ = Image.FromStream(new MemoryStream((Byte[])reader[0]));
                        img_.Save(theJpeg, ImageFormat.Jpeg);
                        bytes = theJpeg.ToArray();
                    }
                    if (reader != null) reader.Close();    // close the reader.  
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                //disconnectFromDataBaseImage();
            }
            return bytes;
        }


       //###############################
       // Only for monitoring system
       //###############################
       

        //[WebMethod(BufferResponse = false, Description = "This is just to check availability of service")]
        //public byte[] checkAvailability(
        //    double ra_,						// right ascension in J2000 degrees
        //    double dec_, 					// declination in J2000 degrees
        //    double scale_,					// arcsec/pixel (0.3961267 is native 1:1 for SDSS)
        //    Int32 width_,					// image width  (in pixels)
        //    Int32 height_,					// image height (in pixels)
        //    string opt_,					// drawing options
        //    string query_,
        //    string imgtype_,
        //    string imgfield_
        //    )
        //{

        //    try
        //    {

        //        connectToAllDatabases();
        //    }
        //    catch (Exception e)
        //    {
        //        throw new Exception(e.Message);
        //    }
        //    finally { disconnectAllDatabases(); }

        //    try{
                
        //        getImageCutout(ra_, dec_, scale_, width_, height_, opt_, query_, imgtype_, imgfield_,"");
        //        return canvas.getBuffer();
        //    }
        //    catch (Exception e)
        //    {
        //        throw new Exception(e.Message);
        //    }
        //    finally { disconnectAllDatabases(); }
            
        //}
    }
}


    ///Current version
///ID:          $Id: ImgCutout.asmx.cs,v 1.26 2007/07/23 17:10:29 nieto Exp $
///Revision:    $Revision: 1.26 $
///Date:        $Date: 2007/07/23 17:10:29 $ 
///Revision history:	April 2002:  First design, Gray & Szalay
///							June  2002:	 Went from "zoom" to pixels per degree
///									 rotated image so north is up
///									 added tick marks and more detailed label
///									 added support for inverted images
///									 performance improvements and went to stored procedure.
///							July  2002:  cleaned up code and added more documentation
///									 deallocated SQL connections (self garbage collection).
///									 replaced exceptions with returned images containing diagnostic messge.
///									 fixed a bug in the tick calculation
///									 revised the text in the image label.
///							February 2003:	Change tile for img in queries in order to query for Frames and Mosaic
///										Patch to account for new zoom clipping (xZoomClippingOffset, yZoomClippingOffset)
///							February 14 2003: Tamas Budavari clean-up and handling string option opt 
///							February 20 2003: Maria Nieto-Santisteban clean-up code: 
///											- variables renamed
///											- added Bounding Box
///							March	 06 2003: New functionalities AND splitting and clean-up code.
///											- Added Target, BoundingBox, Fields, Mask, Plates.
///											- Modified ruler
///							July	 21 2003: For PersonalSkyserver Dr1.
///											- Moved to center on 195,2.5
///											- Converted from fGetJpegObjects to fGetObjectsEq
///							Oct   2009: Deoyani Nandrekar added j2k img data handling functionality
///											- j2kcodec c library is used to read imgdata and put in proper binary format 										
///											- Options for user to save image in different formats
///							2011-11-21: Deoyani: Added query logic in getFrames for DR8 
///							                   - Corrected getMask() function 
///		                    2011-12-21: Alex: removed lots of stale code (GetJpegImg, GetJpegCodec, DimeJpeg)
///		                                    - moved hms2deg and deg2hms into Coord, where it belongs
///		                                    - move max_zoom branching to SDSSConst
///		                    2012-02-21: Deoyani: Added GetJpegImg, GetJpegCodec again as it is used from cas directly since DR8.
///		                    2012-05-21: Deoyani: Added getJpegQuery64 to get Imagedata in base 64 format.
///		                                        -Added GetImage64.aspx to get imagedata in json string format , used for new navi drag
///		                                        -getDetails added for new navi, these added functions does not affect the ImageCutout code
///						    2012-10-07: Deoyani: Updating code. combining my changes and Alex's geometry viewer changes, optimization and projection. 	
///						    2013-04-18: Deoyani: Updated getFrames with the optimized query, this connects to different database serves only images.
///						                         Commented some part of alex's code and put  back 'query' option related code
///						    2013-06-07: Deoyani:  Added 2mass related code for 2mass cutout service                       
///						    2014-2015: Deoyani : worked on converting everything in RESTful web services, removing direct connections to databases and running all queries through casjobs
