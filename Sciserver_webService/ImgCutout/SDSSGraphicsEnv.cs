///Current version
///ID:          $Id: SDSSGraphicsEnv.cs,v 1.9 2004/02/23 14:07:36 nieto Exp $
///Revision:    $Revision: 1.9 $
///Date:        $Date: 2004/02/23 14:07:36 $
using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
namespace Sciserver_webService.ImgCutout
{
	/// <summary>
	/// SDSSGraphicsEnv defines our own Graphic Enviroment for SDSS
	/// </summary>
	public class SDSSGraphicsEnv
	{	
		public static string Revision
		{
			get 
			{
				return "$Revision: 1.9 $";
			}
		}
        //-----------------------------------
        private bool    debug = false;		
		private Int32	width, height;						// Image size
		private double	imageScale;							// Image scale 
		private double	ppm;								// image resolution in pixels per arcminute
        private double  ppd;                                // pixel resolution in pixels per degree
        private double  pixelScale;						    // Pixel scale in arcsec/pixel units
        //-----------------------------------
        // define the different projections
        //-----------------------------------
        public  IProjection proj;							// the abstract Projection object		
		//private SDSSProjection  SDSSproj;					// the actual SDSSProjection object		
        private TANProjection tproj;					    // the TAN projection object
        private STRProjection sproj;					    // the STR projection object
		//-----------------------------------
        private Graphics  gc;
		private Bitmap	  img;
        //-----------------------------------
        // predefine the different colors
        //-----------------------------------
		private Pen queryPen	= new Pen(Color.Fuchsia, 1);
		private Pen photoPen	= new Pen(Color.DodgerBlue, 1);			
		private Pen specPen		= new Pen(Color.Red, 1);					
		private Pen targetPen	= new Pen(Color.GreenYellow, 1);			
		private Pen bboxPen		= new Pen(Color.FromArgb(255,0,255),0.25F);
		private Pen outlinePen	= new Pen(Color.FromArgb(0,255,0),0.25F);			
		private Pen fieldPen	= new Pen(Color.Gray,0.25F);						
		private Pen maskPen		= new Pen(Color.Tomato,1);			
		private Pen platePen	= new Pen(Color.HotPink);
		private Pen testPen		= new Pen(Color.DarkTurquoise);
		private Pen gridPen		= new Pen(Color.LightGreen,1);
		private Pen rulerPen	= new Pen(Color.LightGreen,2);
        private Pen regionPen   = new Pen(Color.Orange, 2.0F);
        /// <summary>
        /// for Apoggee objects
        /// </summary>
        private Pen apoPen = new Pen(Color.MintCream, 2);	
        //-----------------------------------
		// label formats
        //-----------------------------------
        private SolidBrush drawBrush    = new SolidBrush(Color.White);
        private SolidBrush fillBrush    = new SolidBrush(Color.FromArgb(17, 59, 227));
		private StringFormat drawFormat = new StringFormat();
		private Font font				= new Font("Arial", 9);
        //-----------------------------------
        // define where the label is printed. 
        // yLabel changes as we write lines.
        //-----------------------------------
        private int xLabel = 20;
		private int yLabel	= 15;
		private StringBuilder debugMessage = new StringBuilder();		
        private string imageType = "";


		/// <summary>
		/// SDSSGraphicsEnv constructor
		/// </summary>
		public SDSSGraphicsEnv ( Int32 h_, Int32 w_, double isc_, double ppd_, bool dbg_, string _imageType)
		{			
			debug		= dbg_;
			width		= w_;
			height		= h_;
            ppd         = ppd_;
			ppm			= ppd_ /60.0;
			imageScale	= isc_;
            pixelScale  = 3600.0 / ppd_;
            imageType   = _imageType;

			//---------------------------------------------
            // Extend line ends to fill the whole pixel 
            //---------------------------------------------
			outlinePen.StartCap = LineCap.Square;
			outlinePen.EndCap	= LineCap.Square;								
			//---------------------------------------------
			// Allocate the drawing canvas 
            //---------------------------------------------
            PixelFormat pf  = PixelFormat.Format32bppRgb;		// use colors
			img			    = new Bitmap(width, height, pf);	// Make our new image to user's adjusted specs
			gc		        = Graphics.FromImage( img);		// Make an graphic obj we can draw on				
			gc.SmoothingMode = SmoothingMode.AntiAlias;	// set as default SmoothingMode
		}


		/// <summary>
		/// Initialize the Projection for the canvas.
		/// </summary>
		/// <param name="ra_">Right ascension in degrees</param>
		/// <param name="dec_">Declination in degrees</param>
		/// <param name="fc_">Astrometric transformation of center Frame</param>
//		public void InitializeProjection(double ra_, double dec_, Coord fc_, string ptype_)
		public void InitializeProjection(double ra_, double dec_, string ptype_)
		{
			// call the SDSSProjection constructor
			//SDSSproj	= new SDSSProjection( ra_, dec_, imageScale, width, height, fc_);

			//create the cast onto the abstract Interface
			//proj	= (IProjection)SDSSproj;

            //-------------------------------------
            // call the Projection constructor, then
            // cast onto the abstract Interface
            //-------------------------------------
            if (ptype_ == "TAN")
            {
                tproj = new TANProjection(ra_, dec_, ppd, width, height);
                proj = (IProjection)tproj;
            }
            if (ptype_ == "STR")
            {
                sproj = new STRProjection(ra_, dec_, ppd, width, height);
                proj = (IProjection)sproj;
            }        
        }
		//=============================================================
		// Image related functions
		//=============================================================
        //@Deoyani Nandrekar
        ///// <summary>
        ///// get image property
        ///// </summary>	
        //public Bitmap image   // 
        //{
        //    get {return this.img;}
        //}


        public ImageFormat getImageFormat(string imgType)
        {
            switch (imgType)
            {
                case "png": return ImageFormat.Png;
                case "gif": return ImageFormat.Gif;
                case "tif": return ImageFormat.Tiff;
                case "bmp": return ImageFormat.Bmp;
                default   : return ImageFormat.Jpeg;
            }
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

		/// <summary>
		/// getBuffer. Returns the bytes of the image buffer as a JPEG.
		/// </summary>
		/// <returns>Byte[] of the JPEG image</returns>
		public Byte[] getBuffer()
		{
			MemoryStream theJpeg = new MemoryStream();      // place to store the Jpeg
            // System.Windows.Media.Imaging.JpegBitmapEncoder
            //EncoderParameters jpegParms = new EncoderParameters(1);
            //jpegParms.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 95L);
            //img.Save(theJpeg, GetEncoderInfo("image/jpeg"), jpegParms);

            img.Save(theJpeg, getImageFormat(imageType));	// make the Jpeg				
			Byte[] imgdata = theJpeg.ToArray();			    // the image to be returned as a Jpeg byte stream	           
			return(imgdata);							    // return image
		}


        ///// <summary>
        ///// InvertImage(): It will invert each pixel of the image.
        ///// Uses the ColorMatrix function
        ///// </summary>
        //public void InvertImage()
        //{
        //    try
        //    {
        //        ColorMatrix cm = new ColorMatrix(new float[][]
        //    {
        //        new float[] {-1, 0 ,0, 0, 0},
        //        new float[] { 0,-1, 0, 0, 0},
        //        new float[] { 0, 0,-1, 0, 0},
        //        new float[] { 0, 0, 0, 1, 0},
        //        new float[] { 0, 0, 0, 0, 1}
        //    });
        //        ImageAttributes ia = new ImageAttributes();
        //        ia.SetColorMatrix(cm);
        //        gc.DrawImage(img, new Rectangle(0, 0, img.Width, img.Height),
        //            0, 0, img.Width, img.Height,
        //            GraphicsUnit.Pixel, ia);
        //    }
        //    catch (Exception exp) { 

        //    }
        //}

        /// <summary>
        /// Invert. It will invert each pixel of the image.
        /// </summary>
        public void Invert()
        {
            // GDI+ still lies to us - the return format

            BitmapData bmData = img.LockBits(
                new Rectangle(0, 0, img.Width, img.Height),
                ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb
                );
            int stride = bmData.Stride;
            System.IntPtr Scan0 = bmData.Scan0;
            unsafe
            {
                byte* p = (byte*)(void*)Scan0;
                int nOffset = stride - img.Width * 3;
                int nWidth = img.Width * 3;
                for (int y = 0; y < img.Height; ++y)
                {
                    for (int x = 0; x < nWidth; ++x)
                    {
                        p[0] = (byte)(255 - p[0]);
                        ++p;
                    }
                    p += nOffset;
                }
            }
            img.UnlockBits(bmData);
        } 
        
        /// <summary>
        /// GetViewPort. It will compute the ViewPort from the projection.
        /// </summary>
        public string GetViewPort()
        {
            string vs;
            //------------------------
            // construct the viewport
            //------------------------
            PointEq cc = proj.ScreenToEq(new PointF((float)(width / 2), (float)(height / 2)));
            PointEq nw = proj.ScreenToEq(new PointF(0.0F, 0.0F));
            PointEq ne = proj.ScreenToEq(new PointF((float)(width), 0.0F));
            PointEq sw = proj.ScreenToEq(new PointF(0.0F, (float)(height)));
            PointEq se = proj.ScreenToEq(new PointF((float)(width), (float)(height)));
            //----------------------------
            // convert the vertices to V3
            //----------------------------
            double[] nw3 = V3.Normal(nw);
            double[] ne3 = V3.Normal(ne);
            double[] sw3 = V3.Normal(sw);
            double[] se3 = V3.Normal(se);
            double[] cen = V3.Normal(cc);

            vs = "REGION CONVEX ";

            //-----------------------------------------
            // compute distance from center
            //-----------------------------------------
            double dist = Math.Acos(V3.Dot(cen, nw3));
            //-----------------------------------------
            // is it less than a hemisphere?
            //-----------------------------------------
            if (dist < Math.PI / 2)
            {
                //-----------------------------------------
                // compute the normal vectors 
                // corresponding to the great circle edges
                //-----------------------------------------
                double[] n3 = V3.Normalize(V3.Wedge(nw3, ne3));
                double[] s3 = V3.Normalize(V3.Wedge(se3, sw3));
                double[] w3 = V3.Normalize(V3.Wedge(sw3, nw3));
                double[] e3 = V3.Normalize(V3.Wedge(ne3, se3));

                vs += n3[0].ToString() + " " + n3[1].ToString() + " " + n3[2].ToString() + " 0.0 ";
                vs += s3[0].ToString() + " " + s3[1].ToString() + " " + s3[2].ToString() + " 0.0 ";
                vs += e3[0].ToString() + " " + e3[1].ToString() + " " + e3[2].ToString() + " 0.0 ";
                vs += w3[0].ToString() + " " + w3[1].ToString() + " " + w3[2].ToString() + " 0.0 ";
            }
            else
            {
                PointEq nn = proj.ScreenToEq(new PointF((float)(width / 2), 0.0F));
                double[] top = V3.Normal(nn);
                //----------------------------------------------------
                // compute distance from center, should be negative
                //----------------------------------------------------
                double cval = V3.Dot(cen, top);
                vs += cen[0].ToString() + " " + cen[1].ToString() + " "
                    + cen[2].ToString() + " " + cval.ToString();
            }

            //addDebugMessage("ViewPort: "+vs+"\n");
            return vs;
        }

		//=============================================================
		// High level drawing routines
		//=============================================================
		/// <summary>
		/// drawImage. Draw the image onto the canvas using the affine 
		/// transformation through the Graphics.Transform
		///  </summary>
		/// <param name="coord">The affine transformation of a Field</param>
		/// <param name="tile">The bytes of the JPEG image of the Field</param>
		public void drawFrame(Coord coord, Image tile) 
		{	
			//----------------------------------------------------
			// Draw the image onto the canvas using the affine 
			// transformation through the Graphics.Transform
			//----------------------------------------------------	
			PointF[] p = SdssConstants.FieldGeometry(true);					
			gc.Transform = coord.m;
			gc.DrawImage(tile, new PointF[] {p[0], p[1], p[3] });			
			if (debug)
			{
				// draw the outline for debugging
				// gc.DrawLines(testPen, new PointF[] {p[0],p[1],p[2],p[3],p[0]});
			}
			if (debug && false)
			{
				debugMessage.AppendFormat("drawImage {0}\n", coord.info);        	
				debugMessage.AppendFormat("Affine: [{0}, {1}, {2},  {3}, {4}, {5}]\n", 
					coord.m.Elements[0],coord.m.Elements[1],coord.m.Elements[2],
					coord.m.Elements[3],coord.m.Elements[4],coord.m.Elements[5]);        
			}
			gc.ResetTransform();
		}


		/// <summary>
		/// drawField. Draw the outlines of a field specified by the coord.
		///  </summary>
		/// <param name="coord">The affine transformation of a Field</param>
		public void drawField (Coord coord)	
		{
			PointF[] p = SdssConstants.FieldGeometry(false);
			gc.Transform = coord.m;
			gc.DrawLines(fieldPen, new PointF[] {p[0],p[1],p[2],p[3],p[0]});
			gc.ResetTransform();
			if (debug && false)
				{
				   debugMessage.AppendFormat("DrawField {0}\n", coord.info);        
				}
			}



        /// <summary>
        /// drawRegion. Draw the outlines of a region.
        ///  </summary>
        /// <param name="hr">The struct containing an HTMRegion</param>
        public void drawRegion(HTMRegion hr, bool fill)
        {
            if (hr.arc.Length == 0)
            {
                debugMessage.Append("NULL Region " + hr.rid.ToString() + "\n");
                return;
            }

            //debugMessage.Append("DrawRegion "+hr.rid.ToString()+" " + fill.ToString()+"\n");

            //----------------------------
            // do an adaptive resolution
            //----------------------------
            int nstep = 100 - 3 * ((int)(pixelScale / 10.0));
            if (nstep < 30) nstep = 30;
            double delta = 2 * Math.PI / (nstep - 1);

            //-------------
            // create path
            //-------------
            GraphicsPath path = new GraphicsPath();

            //----------------------------------------
            // allocate enough storage for the points
            //----------------------------------------
            int npoints = nstep * hr.arc.Length;
            PointF[] pts = new PointF[npoints];

            int np = 0;
            try
            {
                Int64 convex = hr.arc[0].cvx;
                Int32 patch = hr.arc[0].patch;
                for (int k = 0; k < hr.arc.Length; k++)
                {
                    //---------------------------------------------------
                    // if a new patch or a new convex, draw it right now
                    //---------------------------------------------------
                    if (hr.arc[k].cvx != convex | hr.arc[k].patch != patch)
                    {
                        //drawPatch(pts, np, cl, hr.fill);
                        if (np != 0) addPatch(pts, np, path);
                        convex = hr.arc[k].cvx;
                        patch = hr.arc[k].patch;
                        np = 0;
                    }

                    //-------------------------------------------
                    // if it is not drawable, go to the next arc
                    //-------------------------------------------
                    if (hr.arc[k].draw == 0) continue;

                    double[] r1 = V3.Normal(hr.arc[k].ra1, hr.arc[k].dec1);
                    double[] r2 = V3.Normal(hr.arc[k].ra2, hr.arc[k].dec2);

                    //----------------------------------------------------------
                    // get the equation of the plane, and the circle of the arc
                    //----------------------------------------------------------
                    double cth = hr.arc[k].c;
                    double sth = Math.Abs(Math.Sin(Math.Acos(cth)));
                    double[] n = { hr.arc[k].x, hr.arc[k].y, hr.arc[k].z };
                    PointEq g = V3.ToEq(n);
                    double[] w = V3.West(g.ra, g.dec);
                    double[] u = V3.North(g.ra, g.dec);

                    for (int m = 0; m < 3; m++)
                    {
                        n[m] *= cth;
                        r1[m] -= n[m];
                        r2[m] -= n[m];
                        w[m] *= sth;
                        u[m] *= sth;
                    }

                    //----------------------------------------------------------
                    // compute the two angles defining the two roots on the arc
                    //----------------------------------------------------------
                    double phi1 = Math.Atan2(V3.Dot(r1, u), V3.Dot(r1, w));
                    double phi2 = Math.Atan2(V3.Dot(r2, u), V3.Dot(r2, w));
                    if (phi2 <= phi1) phi2 += Math.PI * 2.0;

                    //---------------------------------
                    // compute precise number of steps
                    //---------------------------------
                    int steps = (int)Math.Floor((phi2 - phi1) / delta);
                    if (steps < 15) steps = 15;

                    //----------------------
                    // do the loop over phi
                    //----------------------
                    double[] r = new double[3];
                    double phi = phi1;
                    double inc = (phi2 - phi1) / (steps - 1);
                    for (int i = 0; i < steps; i++)
                    {
                        for (int m = 0; m < 3; m++)
                        {
                            r[m] = n[m] + Math.Cos(phi) * w[m] + Math.Sin(phi) * u[m];
                        }
                        pts[np++] = proj.EqToScreen((float)V3.ToEq(r).ra, (float)V3.ToEq(r).dec, 0.0F);
                        phi += inc;
                    }
                }

                if (np != 0) addPatch(pts, np, path);
                Color cl = regionColor(hr.type, hr.mask);
                Pen rpen = new Pen(cl, 0.25F);
                SolidBrush oBrush = new SolidBrush(Color.FromArgb(64, cl));
                if (hr.fill > 0)
                {
                    rpen.Color = Color.Red;
                    oBrush.Color = Color.FromArgb(128, Color.Yellow);
                }
                if (fill || hr.fill>0 ) gc.FillPath(oBrush, path);

                //--------------------------
                // now draw the path itself
                //--------------------------
                gc.DrawPath(rpen, path);
            }
            catch { displayMessage("exception in drawRegion " + hr.rid.ToString() + "\n"); }
            if (debug && false)
            {
                debugMessage.Append("drawRegion\n");
            }
        }


        /// <summary>
        /// addPatch. Insert the outline of a patch given by a polyline into the path.
        ///  </summary>
        /// <param name="pts">The array containing the points in screen coordinates</param>
        /// <param name="np">The number of points to be drawn</param>
        /// <param name="path">The GraphicsPath to be appended to</param>

        public void addPatch(PointF[] pts, int np, GraphicsPath path)
        {
            //-----------------------------------------------
            // copy the points so that we can use a polyline
            //-----------------------------------------------
            PointF[] pp = new PointF[np];
            for (int i = 0; i < np; i++) pp[i] = pts[i];

            path.CloseFigure();
            path.AddLines(pp);
        }



		/// <summary>
		/// drawQueryObj. Draw the position of a Query Object by ra,dec.
		///  </summary>
		/// <param name="oRa">The right ascension of the object in degrees</param>
		/// <param name="oDec">The declination of the object in degrees</param>
		/// <param name="size">The size of the symbol in screen pixels</param>
		public void drawQueryObj	(double oRa, double oDec, float size)		
		{
			//float a = 1.10F * size;
			//float b = 0.55F * size;
            //PointF p = proj.EqToScreen(oRa,oDec, size);		
            //PointF[] tri = new PointF[3];
            //tri[0] = new PointF(p.X , p.Y);
            //tri[1] = new PointF(p.X + a, p.Y);
            //tri[2] = new PointF(p.X + b, p.Y + a);
            //gc.DrawPolygon(queryPen, tri);

            /// @deoyani made changes 
            float b = 0.55F * size;
            PointF p = proj.EqToScreen(oRa, oDec, size);
            PointF[] tri = new PointF[3];
            tri[0] = new PointF(p.X - b, p.Y - b );
            tri[1] = new PointF(p.X + b, p.Y - b);
            tri[2] = new PointF(p.X , p.Y + b);
            gc.DrawPolygon(queryPen, tri);
		}


		/// <summary>
		/// drawSpecObj. Draw the position of a SpecObj by ra,dec.
		///  </summary>
		/// <param name="oRa">The right ascension of the object in degrees</param>
		/// <param name="oDec">The declination of the object in degrees</param>
		/// <param name="size">The size of the symbol in screen pixels</param>
		public void drawSpecObj	(double oRa, double oDec, float size)		
		{
			PointF p = proj.EqToScreen(oRa,oDec,0.0F);					
			gc.DrawRectangle(specPen, p.X-size/2 , p.Y-size/2, size, size);
		}


		/// <summary>
		/// drawPhotoObj. Draw the position of a PhotoObj by ra,dec.
		///  </summary>
		/// <param name="oRa">The right ascension of the object in degrees</param>
		/// <param name="oDec">The declination of the object in degrees</param>
		/// <param name="size">The size of the symbol in screen pixels</param>
		public void drawPhotoObj(double oRa, double oDec, float size)
		{				
            PointF p = proj.EqToScreen(oRa,oDec, 0.0F);			
            gc.DrawEllipse(photoPen, p.X-size/2, p.Y-size/2, size, size);
		}


        /// <summary>
        /// drawApojee. 
        ///  </summary>
        /// <param name="oRa">The right ascension of the object in degrees</param>
        /// <param name="oDec">The declination of the object in degrees</param>
        /// <param name="size">The size of the symbol in screen pixels</param>
        public void drawApogeeObj(double oRa, double oDec, float size)
        {
            //PointF p = proj.EqToScreen(oRa, oDec, 0.0F);
            
            //gc.DrawEllipse(apoPen, p.X - size / 2, p.Y - size / 2, size, size);
            float b = 0.55F * size;
            PointF p = proj.EqToScreen(oRa, oDec, 0.0F);
            PointF[] tri = new PointF[3];
            tri[0] = new PointF(p.X - b, p.Y - b);
            tri[1] = new PointF(p.X + b, p.Y - b);
            tri[2] = new PointF(p.X, p.Y + b);
            gc.DrawPolygon(apoPen, tri);

        }


		/// <summary>
		/// drawTargetObj. Draw the position of a TargetObj by ra,dec.
		///  </summary>
		/// <param name="oRa">The right ascension of the object in degrees</param>
		/// <param name="oDec">The declination of the object in degrees</param>
		/// <param name="size">The size of the symbol in screen pixels</param>
		public void drawTargetObj(double oRa, double oDec, float size)
		{
			PointF p = proj.EqToScreen(oRa,oDec,0.0F);		// get the center point
			float a = 0.75F*size;
			float b = 0.25F*size;
			gc.DrawLine(targetPen,p.X-a,p.Y-a,p.X-b,p.Y-b);
			gc.DrawLine(targetPen,p.X-a,p.Y+a,p.X-b,p.Y+b);
			gc.DrawLine(targetPen,p.X+a,p.Y-a,p.X+b,p.Y-b);
			gc.DrawLine(targetPen,p.X+a,p.Y+a,p.X+b,p.Y+b);
		}


        /// <summary>
        /// drawTestObj. Draw the position of an Object by row,col.
        ///  </summary>
        /// <param name="oRa">The right ascension of the object in degrees</param>
        /// <param name="oDec">The declination of the object in degrees</param>
        /// <param name="size">The size of the symbol in screen pixels</param>
        public void drawTestObj(Coord coord, double oRowc, double oColc, float size)
        {
            gc.Transform = coord.m;
            gc.DrawRectangle(bboxPen, (float)oColc - size / 2, (float)oRowc - size / 2,
                (float)(size), (float)(size));
            gc.ResetTransform();

        }


		/// <summary>
		/// drawPlate. Draw the outline of a Plate centered at ra,dec.
		///  </summary>
		/// <param name="pRa">The right ascension of the plate center in degrees</param>
		/// <param name="pDec">The declination of the plate center in degrees</param>
		/// <param name="pRadius">The radius of the circle in arcminutes</param>
		public void drawPlate ( double pRa, double pDec, double pRadius) 
		{   
			// the increment for the angle in degrees
			double  inc	= 1.0;
			double[] n = GetV3Normal(pRa,pDec);
			double[] w = GetV3West(pRa,pDec);
			double[] u = GetV3North(pRa,pDec);
			double[] r = new double[3];
			double plateCos = Math.Cos(pRadius * Coord.D2R / 60.0);
			double plateSin = Math.Sin(pRadius * Coord.D2R / 60.0);
			int steps	= (int)(360/inc) + 1;
			PointF[] g	= new PointF[steps];
			inc *= Coord.D2R;
			for (int j = 0; j < steps; j++) 
			{ 
				double phi = (double)(j * inc);				
				double phiCos = Math.Cos(phi) * plateSin;
				double phiSin = Math.Sin(phi) * plateSin;
				for (int c = 0; c < 3; c++) 
				{
					r[c] = n[c]*plateCos + phiCos*w[c] + phiSin*u[c];					
				}	
				double oDec	= Math.Asin(r[2]) / Coord.D2R;
				double oRa	= Math.Atan2(r[1], r[0]) / Coord.D2R;
				if (oRa < 0) oRa += 360.0;
				g[j] = proj.EqToScreen(oRa,oDec,0.0F);
			}				
			gc.DrawLines(platePen, g);
		}
		
		
		/// <summary>
		/// drawBoundingBox. Will draw the bounding box of a given object.
		/// </summary>
		/// <param name="coord">The astrometric transformation object of the Frame</param>
		/// <param name="xmin">The x coordinate of the upper left hand corner</param>
		/// <param name="xmax">The x coordinate of the lower right hand corner</param>
		/// <param name="ymin">The y coordinate of the upper left hand corner</param>
		/// <param name="ymax">The y coordinate of the lower right hand corner</param>
		public void drawBoundingBox (Coord coord, double xmin, double xmax, double ymin, double ymax)		
		{				
			gc.Transform = coord.m;
			gc.DrawRectangle(bboxPen, (float)xmin, (float)ymin, 
				(float)(xmax-xmin+SdssConstants.OutlinePix), (float)(ymax-ymin+SdssConstants.OutlinePix));		
			gc.ResetTransform();
		}


		/// <summary>
		/// drawOutline. Draws the outline of the object as given by the compressed span.
		/// </summary>
		/// <param name="coord">The astrometric transformation object of the Frame</param>
		/// <param name="span">The compressed outline in span format (R. Lupton)</param>
		public void drawOutline (Coord coord, StringBuilder span)
		{															
			gc.Transform = coord.m;
			//-------------------------------------------------
			span.Replace('"', 'k');
			span.Replace("k", "");
			try
			{								
				ArrayList l = polyFunk.getPoly(span.ToString());
				for(int i=0; i < l.Count;  i++)
				{
					gc.DrawLine(outlinePen,((Line)l[i]).p1,((Line)l[i]).p2);						
				}					
			}
			catch {	displayMessage("Exception in drawOutline " + span);}
			
			//if (debug) debugMessage.Append("drawOutline: "+span+"\n");

			//------------------------------------------------
			gc.ResetTransform();
		}


		/// <summary>
		/// drawMask. Draws a selected list of masks over the canvas.
		/// Masks are encoded as spherical polygons, given by their
		/// corners in equatorial coordinates.
		/// </summary>
		/// <param name="area">The string giving the vertex list</param>
		public void drawMask(StringBuilder area)
		{
			// Remove the quotes and POLY text from the area
			// area.Replace('"', 'k');
			// area.Replace("kPOLY ", "");
			//area.Remove(area.Length-1, 1);
            //TextWriter newfile = new StreamWriter("C:\\Deoyani\\Projects\\ImgCutout\\MaskFile1.txt");
			string[] sp = area.ToString().Split(new char[] {' '});
            //newfile.WriteLine("Area :" + area);
            //newfile.WriteLine("SP :" + sp.Length);
			int np = sp.Length / 2;
			PointF[] pts = new PointF[np-1];
			double oRa, oDec;
			try
			{
                for (int i = 2, j = 0; i < sp.Length; i += 2, j++)
                {
                    oRa = Convert.ToDouble(sp[i]);
                    oDec = Convert.ToDouble(sp[i + 1]);
                    pts[j] = proj.EqToScreen(oRa, oDec, 0.0F);
                    //newfile.WriteLine("SP RA:" + sp[i] + " :: SP DEC :" + sp[i + 1] + ":: PTS : " + pts[j]);
                }         
				gc.DrawPolygon(maskPen,pts);
			}
			catch {displayMessage("exception in drawMask " + area);}
            //newfile.Close();
		}

        /// <summary>
        /// drawLabel. Draws a label at the next line.
        /// </summary>
        /// <param name="label">The text of the label.</param>
        public void drawErrorMessage(string label,int width, int height)
        {
            gc.ResetTransform();
            gc.DrawString(label, font, drawBrush, width/2-200 , height/2-40, drawFormat);
            string[] lines = label.Split(new char[] { '\n' });
            yLabel += (int)font.SizeInPoints * (lines.Length + 3);
        }

		/// <summary>
		/// drawLabel. Draws a label at the next line.
		/// </summary>
		/// <param name="label">The text of the label.</param>
		public void drawLabel(string label)
		{
			gc.ResetTransform();
			gc.DrawString(label, font, drawBrush, xLabel, yLabel, drawFormat);
			string[] lines = label.Split(new char[] {'\n'});
			yLabel +=  (int)font.SizeInPoints * (lines.Length + 3); 
		}

        /// <summary>
        /// drawWarning(): draws a warning message on the canvas
        /// </summary>
        /// <param name="warn"></param>
        public void drawWarning(String warn)
        {
            gc.ResetTransform();
            int cFudge = 8;            
            float xc = width / 2;
            float yc = height / 2;
            gc.DrawString(warn, font, drawBrush, xc - cFudge, 2 * cFudge, drawFormat);
        }
      

        /// <summary>
		/// drawGrid. Draws the axes and tickmarks on the canvas.
		/// </summary>
		public void drawGrid()
		{
			//-------------------------------------------------------------------
			// write the grid with tickmarks 
			gc.ResetTransform();
			// first draw X, Y grid and the N, S, E, W labels on the grid
			int cFudge	= 8;
			float inner	= 0.05F*Math.Max(width,height);
			float outer	= 0.125F;
			float xc	= width/2;
			float yc	= height/2;
			gc.DrawLine(gridPen, xc, yc-inner, xc, height*outer);				// x axis
			gc.DrawLine(gridPen, xc, yc+inner, xc, height*(1.0F -outer) );	// x axis
			gc.DrawLine(gridPen, xc-inner, yc, width*outer, yc);				// y axis
			gc.DrawLine(gridPen, xc+inner, yc, width*(1.0F-outer), yc);		// y axis
			gc.DrawString("N", font, drawBrush, xc-cFudge, 2*cFudge, drawFormat);
			gc.DrawString("S", font, drawBrush, xc-cFudge, height-4*cFudge, drawFormat);				
			gc.DrawString("E", font, drawBrush, 2*cFudge, yc-cFudge, drawFormat);		
			gc.DrawString("W", font, drawBrush, width-4*cFudge, yc-cFudge, drawFormat);
		}

        /// <summary>
        /// drawRuler(): draw a ruler on the canvas
        /// </summary>
		public void drawRuler()
		{
			// first draw X, Y grid and the N, S, E, W labels on the grid
			int cFudge	= 8;						
			float xc	= width/2;
			float yc	= height/2;
			// now draw decide on the tick size
			scaleItem[] ruler = new scaleItem []
			{
				new scaleItem("0.5''", 1.0/120.0),
				new scaleItem("1''", 1.0/60.0),
				new scaleItem("2''", 2.0/60.0),
				new scaleItem("5''", 5.0/60.0),
				new scaleItem("10''", 1.0/6.0),
				new scaleItem("20''", 2.0/6.0),
				new scaleItem("1'", 1.0),
				new scaleItem("2'", 2.0),
				new scaleItem("5'", 5.0),
				new scaleItem("10'", 10.0),
				new scaleItem("20'", 20.0),
				new scaleItem("30'", 30.0),
				new scaleItem("1deg", 60.0),
				new scaleItem("1.5deg", 90.0),
				new scaleItem("2deg", 120.0)
			};
			double extent = Math.Min(width,height);
			int ticks = 10;
			int i;
			for (i=0; i < ruler.Length; i++) 
			{
				ticks = (int)Math.Floor(extent / (ruler[i].size*ppm) );
				if ((ticks >= 4) && (ticks < 12) ) 	break;
			}
			if (i == ruler.Length) i--;
			string label	= ruler[i].label;
			double tickSize = ruler[i].size * ppm;
            //------------------------
			// draw the tickmarks
            //------------------------
			int pos =  0;
			for (i = -ticks; i <= ticks; i++)
			{
				pos = (int) Math.Floor(i*tickSize);
				gc.DrawLine(gridPen, 0, yc + pos,	cFudge, yc + pos);
				gc.DrawLine(gridPen, width, yc + pos, width-cFudge, yc + pos);
				gc.DrawLine(gridPen, xc + pos, 0,	xc + pos,	cFudge);
				gc.DrawLine(gridPen, xc + pos, height, xc + pos, height-cFudge);
			}
			//-------------------------------------------------					
			// draw the tick and write the label at the center
            //-------------------------------------------------
			int x1	= xLabel;		
			int x2	= xLabel + (int)tickSize;
			int len	= 4;
			int yy  = yLabel+20;
			gc.DrawLine(rulerPen, x1, yy      , x2, yy      );
			gc.DrawLine(rulerPen, x1, yy - len, x1, yy + len);
			gc.DrawLine(rulerPen, x2, yy - len, x2, yy + len);
			xLabel += (int)(0.5*tickSize) - cFudge;
			this.drawLabel(label);
		}


		//=============================================================
		// Debug and printing routines
		//=============================================================
		/// <summary>
		/// addDebugMessage. Add a line to the current debug message string.
		/// </summary>
		/// <param name="s">string to print</param>
		public void addDebugMessage(string s)
		{
			debugMessage.Append(s);
		}


		/// <summary>
		/// drawDebugMessage. Draws the debugmessage at the current location.
		/// </summary>
		public void drawDebugMessage(int width,int height)
		{
			//drawLabel(debugMessage.ToString());
            drawErrorMessage(debugMessage.ToString(), width, height);
		}


		/// <summary>
		/// displayMessage. Draw the message on the screen.
		/// </summary>
		/// <param name="error">String to display</param>
		public void displayMessage(string error)
		{	
			gc.ResetTransform();
			gc.FillRectangle(new SolidBrush(Color.Black),0,0,width,height);
			gc.DrawString(error,new Font("Arial", 10),new SolidBrush(Color.White), 
				10  ,10 ,new StringFormat());
		}

		//=============================================================
		// Support routines
		//=============================================================
		private class scaleItem
		{
			public string label;
			public double size;
			public scaleItem (string strLabel, double vArcminutes) 
			{
				label = strLabel;
				size  = vArcminutes;
			}
		}


		/// <summary>
		/// GetAffineTransform. Compute the affine transformation for the
		/// graphic context that maps the pixel coordinates of a Frame to
		/// the world coordinate system of the projection of the canvas.
		/// </summary>
		/// <param name="coord">The input astrometric transformation of the Frame</param>
		/// <returns>Matrix for the canvas graphic context</returns>
		public Matrix getAffineTransform(Coord coord)
		{
			// compute the corners in both coordinate systems            
			PointF[] p	= SdssConstants.FieldGeometry(true);
			int len		= p.Length;	
			PointF[] g	= new PointF[len];
			Coord fc	= new Coord();
            if (SdssConstants.isSdss)
                fc.copy(coord);
            else
                fc.copy2Mass(coord);
			    
			fc.scale	= 1.0;

			// loop through the array
			for(int i=0;i<len;i++)
			{              
                if(SdssConstants.isSdss)
                    fc.FrameToEq(p[i].X, p[i].Y);  // SDSS
                else 
                    fc.FrameToEq(p[i].X, p[i].Y, fc.crpix1, fc.crpix2, fc.cdelt1, fc.cdelt2, fc.crval1, fc.crval2);  // 2mass

                g[i] = proj.EqToScreen(fc.ra, fc.dec, 0.0F);  
			}
			// build up the terms for solving the best fit affine transformation
			float qu=0, qv=0, qux=0, quy=0, qvx=0, qvy=0, 
				  qx=0, qy=0, qxx=0, qyy=0, qxy=0;

            //ImgCutout
            for (int i = 0; i < 4; i++)
            {
                qu += g[i].X / 4;
                qv += g[i].Y / 4;
                qux += g[i].X * p[i].X / 4;
                qvx += g[i].Y * p[i].X / 4;
                quy += g[i].X * p[i].Y / 4;
                qvy += g[i].Y * p[i].Y / 4;
                qx += p[i].X / 4;
                qy += p[i].Y / 4;
                qxx += p[i].X * p[i].X / 4;
                qxy += p[i].X * p[i].Y / 4;
                qyy += p[i].Y * p[i].Y / 4;
            }

			// we seek the affine transformation in the form (see Petzold book p. 292)
			//		x' = sx*x + rx * y + dx
			//		y' = ry*x + sy * y + dy;
			double d = (float) (det3x3(new double[] {1.0, qx, qy,  qx, qxx, qxy,  qy, qxy, qyy}));
			float dx = (float) (det3x3(new double[] { qu, qx, qy, qux, qxx, qxy, quy, qxy, qyy})/d);
			float sx = (float) (det3x3(new double[] {1.0, qu, qy,  qx, qux, qxy,  qy, quy, qyy})/d);
			float rx = (float) (det3x3(new double[] {1.0, qx, qu,  qx, qxx, qux,  qy, qxy, quy})/d);
			float dy = (float) (det3x3(new double[] { qv, qx, qy, qvx, qxx, qxy, qvy, qxy, qyy})/d);
			float ry = (float) (det3x3(new double[] {1.0, qv, qy,  qx, qvx, qxy,  qy, qvy, qyy})/d);
			float sy = (float) (det3x3(new double[] {1.0, qx, qv,  qx, qxx, qvx,  qy, qxy, qvy})/d);
			if (debug && false)
			{
				debugMessage.AppendFormat("Affine: [{0}][ {1}, {2},  {3}, {4}, {5}, {6}]\n",coord.info, sx,ry,rx,sy,dx,dy);        
				debugMessage.AppendFormat("             [ W {0}, H {1}]\n",p[2].X-p[0].X, p[2].Y-p[0].Y);        
				debugMessage.AppendFormat("             [ W {0}, H {1}]\n",g[2].X-g[0].X, g[2].Y-g[0].Y);        
			}           
			return new Matrix(sx,ry,rx,sy,dx,dy);
		}
		
        
        /// <summary>
		/// det3x3. Computes the determinant of a 3x3 matrix.
		/// </summary>
		/// <param name="a">Vector of 9 doubles, the elements of the matrix</param>
		/// <returns></returns>
		private double det3x3(double[] a)
		{
			return (a[0]*a[4]*a[8] + a[1]*a[5]*a[6] + a[2]*a[3]*a[7] 
				- a[0]*a[7]*a[5] - a[3]*a[1]*a[8] - a[6]*a[4]*a[2]);
		}

		/// <summary>
		/// GetV3Normal. Calculate the Cartesian normal vector 
		/// corresponding to the point (ra,dec).
		/// </summary>
		/// <param name="ra_">Right ascension in degrees</param>
		/// <param name="dec_">Declination in degrees</param>
		/// <returns>Array of 3 doubles, a 3-vector</returns>
		private double[] GetV3Normal(double ra_, double dec_) 
		{
			double[] n = new double[3];	
			n[0] =  Math.Cos(dec_ * Coord.D2R) * Math.Cos(ra_ * Coord.D2R);
			n[1] =  Math.Cos(dec_ * Coord.D2R) * Math.Sin(ra_ * Coord.D2R);
			n[2] =  Math.Sin(dec_ * Coord.D2R);
			return n;
		}


		/// <summary>
		/// GetV3North. Calculate the Cartesian normal vector 
		/// corresponding to the North direction at the point (ra,dec).
		/// </summary>
		/// <param name="ra_">Right ascension in degrees</param>
		/// <param name="dec_">Declination in degrees</param>
		/// <returns>Array of 3 doubles, a 3-vector</returns>
		private double[] GetV3North(double ra_, double dec_) 
		{
			double[] u = new double[3];	
			u[0] = -Math.Sin(dec_ * Coord.D2R) * Math.Cos(ra_ * Coord.D2R);
			u[1] = -Math.Sin(dec_ * Coord.D2R) * Math.Sin(ra_ * Coord.D2R);
			u[2] =  Math.Cos(dec_ * Coord.D2R);
			return u;
		}


		/// <summary>
		/// GetV3West. Calculate the Cartesian normal vector 
		/// corresponding to the Est direction at the point (ra,dec).
		/// </summary>
		/// <param name="ra_">Right ascension in degrees</param>
		/// <param name="dec_">Declination in degrees</param>
		/// <returns>Array of 3 doubles, a 3-vector</returns>
		private double[] GetV3West(double ra_, double dec_) 
		{
			double[] w = new double[3];	
			w[0] = -Math.Sin(ra_ * Coord.D2R);
			w[1] =  Math.Cos(ra_ * Coord.D2R);
			w[2] =  0;
			return w;
		}


		/// <summary>
		/// drawAxis. Debugging, draws the N direction in red, the E in green.
		/// </summary>
		/// <param name="ra_">Right ascension of center</param>
		/// <param name="dec_">Declination of center</param>
		public void drawAxis(double ra_, double dec_)
		{
			PointF p = proj.EqToScreen(ra_,dec_,0.0F);
			PointF n = proj.EqToScreen(ra_,dec_+0.1F,0.0F);
			PointF w = proj.EqToScreen(ra_+0.1F,dec_,0.0F);
			gc.DrawLine(specPen,p,n);
			gc.DrawLine(outlinePen,p,w);
		}


        /// <summary>
        /// regionColor. Returns the drawing color of a given region type.
        /// </summary>
        /// <param name="type">Region type as a string in upper case</param>
        /// <param name="isMask">0 if not a mask, 1 if mask</param>
        /// returns Color.
        public Color regionColor(string type, int isMask)
        {
            //---------------------------
            // set the appropriate color
            //---------------------------
            Color cl;
            switch (type)
            {
                case "CAMCOL":
                    cl = Color.White;
                    break;
                case "CHUNK":
                    cl = Color.Orange;
                    break;
                case "HOLE":
                    cl = Color.FromArgb(0, 255, 255);
                    break;
                case "TIHOLE":
                    cl = Color.FromArgb(128, 128, 255);
                    break;
                case "PLATE":
                    cl = Color.White;
                    break;
                case "PRIMARY":
                    cl = Color.Green;
                    break;
                case "RUN":
                    cl = Color.Blue;
                    break;
                case "SECTOR":
                    cl = Color.Yellow;
                    break;
                case "SECTORLET":
                    cl = Color.Yellow;
                    break;
                case "SEGMENT":
                    cl = Color.FromArgb(128, 255, 128);
                    break;
                case "SKYBOX":
                    cl = Color.FromArgb(128, 255, 128);
                    break;
                case "STAVE":
                    cl = Color.FromArgb(0, 255, 255);
                    break;
                case "STRIPE":
                    cl = Color.FromArgb(0, 255, 0);
                    break;
                case "TIGEOM":
                    cl = Color.FromArgb(255, 128, 128);
                    break;
                case "TILE":
                    cl = Color.FromArgb(0, 255, 255);
                    break;
                case "TIPRIMARY":
                    cl = Color.FromArgb(155, 200, 100);
                    break;
                case "TILEBOX":
                    cl = Color.FromArgb(255, 255, 0);
                    break;
                case "WEDGE":
                    cl = Color.FromArgb(255, 255, 0);
                    break;
                default:
                    cl = Color.White;
                    break;
            }
            if (isMask > 0) cl = Color.FromArgb(255, 0, 0);
            return cl;
        }

        ///***
        // * This is specially written to get json data format, which is used in navi drag.
        // ***/
        //public String getBufferBase64()
        //{
        //    try
        //    {
        //        MemoryStream theJpeg = new MemoryStream();  // place to store the Jpeg
        //        img.Save(theJpeg, getImageFormat(imageType));		// make the Jpeg				
        //        Byte[] imgdata = theJpeg.ToArray();
        //        string encodedData = Convert.ToBase64String(imgdata);
        //        return "data:image/jpeg;base64," + encodedData;
        //    }
        //    catch (Exception e)
        //    {
        //        throw new Exception("Error in base64Encode" + e.Message);
        //    }
        //}
    }
}
/* Revision History
        $Log: SDSSGraphicsEnv.cs,v $
        Revision 2.0 2011-12-23 Alex
            fixed BoundingBox to use SdssConstans.OutlinePix for offset
 
        Revision 1.9  2004/02/23 14:07:36  nieto
        Major changes in order to:
        	- Improve Message/Exception errors.
        	- Added a new Web Method that accepts query to mark objects:
        		SQL query
        		List of objects
        		Mark String like SR(10, 30) Stars with magnitude
        				 between 10 and 30 in the R band
        Revision 1.8  2003/12/12 20:18:04  nieto
        Modifications for DR2 jpeg images. Current code works for DR1. It's set up
        to get different version changing a compiling option.
        Fixed a bug for Grid options when size was 2048x2048.
        Revision 1.6  2003/04/10 21:54:32  szalay
         projection interface added
        Revision 1.5  2003/04/08 17:12:00  nieto
        Trying to include projection.cs class in project
        Revision 1.3  2003/03/25 23:18:33  nieto
        testing draw fields with their own coord
*/
