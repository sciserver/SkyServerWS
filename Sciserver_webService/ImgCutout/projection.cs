///Current version
///ID:          $Id: projection.cs,v 1.2 2003/04/10 21:54:32 szalay Exp $
///Revision:    $Revision: 1.2 $
///Date:        $Date: 2003/04/10 21:54:32 $
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
namespace Sciserver_webService.ImgCutout
{

/*    
    /// <summary>
	/// Interface definition for different projections.
	/// </summary>
	public interface IProjection
	{
		PointF EqToScreen(double ra_, double dec_, float size_);    
	}




	/// <summary>
	/// SDSSProjection
	/// Gives us the definition of the projection to the screen
	/// using the basic Coord class. The projection is that of 
	/// the Frame nearest to (ra,dec).
	/// </summary>
	public class SDSSProjection: IProjection
	{
		public static string Revision
		{
			get 
			{
				return "$Revision: 1.2 $";
			}
		}
		private PointF cOffset;							// reference point offset
		private double rotation;						// rotation of reference frame
		private double cosR, sinR;						// sin, cos of rotation matrix
		private Coord  refCoord;						// the reference coordinate system
		private Int32  width, height;					// screen size
		/// <summary>
		/// InitProjection. Initialize the transformation for the projection
		/// to be used on the screen.
		/// </summary>	
		public SDSSProjection(double ra_, double dec_, double imageScale_, int width_, int height_, Coord fc_)
		{
			// rotation (degrees) to make north "up" -- compute from coordinates.
			rotation = fc_.rotateDegreesToNorth(ra_,dec_);
			cosR = Math.Cos(rotation*Coord.D2R);
			sinR = Math.Sin(rotation*Coord.D2R);
			// Create a reference coordinate for the graphic context
			refCoord = new Coord();							
			refCoord.copy(fc_);
			refCoord.scale /= imageScale_;
			// and define the upper corner
			cOffset		= new PointF(0.0F,0.0F);
			PointF p	= refCoord.EqToFrame(ra_, dec_);
			cOffset		= new PointF(p.X, p.Y);
			width		= width_;
			height		= height_;
			
                        //if (debug && false) 
                        //{
                        //    PointF q = Projection.EqToScreen(ra,dec,0.0F);
                        //    debugMessage.AppendFormat("Initializing projection\n\n");        
                        //    debugMessage.AppendFormat("refCoord.scale: {0}, imageScale: {1}\n", refCoord.scale, imageScale);        
                        //    debugMessage.AppendFormat("refCoord.info: {0}\n", refCoord.info);
                        //    debugMessage.AppendFormat("Rotation: {0}\n", rotation);        
                        //    debugMessage.AppendFormat("Astrom: {0}, {1}, {2}, {3}, {4}, {5}\n {6},{7}\n",
                        //            refCoord.a, refCoord.b, refCoord.c, 
                        //            refCoord.d, refCoord.e, refCoord.f,
                        //            refCoord.node, refCoord.incl);
                        //}
				
		}		
		//=============================================================
		// Coordinate transformations
		//=============================================================
		/// <summary>
		/// EqToScreen. Maps an (ra,dec) coordinate to the Screen.
		/// This is the basic coordinate transformation function
		///  </summary>
		/// <param name="ra_">Right ascension in degrees, double</param>
		/// <param name="dec_">Declination in degrees, double</param>
		/// <param name="size_">Symbol size in pixels, float</param>
		public PointF EqToScreen(double ra_, double dec_, float size_) 
		{
			PointF p = refCoord.EqToFrame(ra_, dec_);
			// back to the center
			p.X -= cOffset.X;	
			p.Y -= cOffset.Y;
			// now rotate and swap the X coordinate
            PointF q = new PointF(
				(float)( -cosR * p.X + sinR * p.Y),                 
				(float)( sinR * p.X + cosR * p.Y)
				);
			// shift back to center			
			q.X += (float)(width/2  - 0.5*size_);
			q.Y += (float)(height/2 - 0.5*size_);
            //@Deoyani Nandrekar
            return q;
		}
		public PointF[] ScreenToEq(PointF[] points) 
		{
			// to be implemented later.........
			// allocate space for the new array
			int size = points.Length;
			PointF[] g = new PointF[size];
			for(int i=0;i<size;i++)
			{
				g[i] = (PointF)(EqToScreen(points[i].X, points[i].Y, 0.0F));
			}
			return g;
		}
	}

*/

   	/// <summary>
	/// Interface definition for different projections.
	/// </summary>
	public interface IProjection
	{
		PointF EqToScreen(double ra_, double dec_, float size_);
		PointF EqToScreen(PointEq e_, float size_);
		PointEq ScreenToEq(PointF p_);
	}

     
    /// <summary>
    /// CARTProjection
    /// Gives us the definition of the projection to the screen
    /// using a cartesian projection, centered on (ra=180, dec=0).
    /// </summary>
    public class CARTProjection : IProjection
    {
        public static string Revision
        {
            get
            {
                return "$Revision: 1.2 $";
            }
        }
        private Int32 width, height;					// screen size
        private PointF cOffset;							// reference point offset
        private double scale;							// image scale
        private double[] n, w, u;						// normal, west, up vectors
        /// <summary>
        /// InitProjection. Initialize the transformation for the projection
        /// to be used on the screen.
        /// </summary>	
        public CARTProjection(double ra_, double dec_, double ppd_, int width_, int height_)
        {
            ra_ = 180.0;
            dec_ = 0.0;
            n = V3.Normal(ra_, dec_);
            w = V3.West(ra_, dec_);
            u = V3.North(ra_, dec_);

            scale = ppd_ / V3.D2R;
            width = width_;
            height = height_;
            cOffset = new PointF((float)(width / 2.0), (float)(height / 2.0));
        }

        //------------------------------
        // Coordinate transformations
        //------------------------------

        /// <summary>
        /// EqToScreen. Maps an (ra,dec) coordinate to the Screen.
        /// This is the basic coordinate transformation function
        ///  </summary>
        /// <param name="ra_">Right ascension in degrees, double</param>
        /// <param name="dec_">Declination in degrees, double</param>
        /// <param name="size_">Symbol size in pixels, float</param>
        public PointF EqToScreen(double ra_, double dec_, float size_)
        {
            double gn = Math.Cos(dec_ * V3.D2R);
            if (gn != 0)
            {
                gn = 1.0 / gn;
            }
            PointF q = new PointF(cOffset.X - (float)(scale * (ra_ - 180) * gn), cOffset.Y - (float)(scale * dec_));
            return q;
        }

        public PointF EqToScreen(PointEq e, float size_)
        {
            return EqToScreen(e.ra, e.dec, size_);
        }


        /// <summary>
        /// ScreenToEq. Maps a screen coordinate to an (ra,dec).
        /// This is the inverse coordinate transformation function
        ///  </summary>
        /// <param name="p">The screen coordinate of the point</param>
        public PointEq ScreenToEq(PointF p)
        {
            double[] r = new Double[3];

            //----------------------------------------
            // first subtract the offset, and rescale
            //----------------------------------------
            PointF q = new PointF((float)((cOffset.X - p.X) / scale), (float)((cOffset.Y - p.Y) / scale));

            //------------------------------------------------
            // now these are the dot products with w and u
            // compute the dot product with the normal vector
            //------------------------------------------------
            double qdec = 90 * q.Y;
            double qra = 180 + 180 * q.X * Math.Cos(qdec * V3.D2R);
            PointEq g = new PointEq(qra, qdec);
            return g;
        }
    }


    /// <summary>
    /// TANProjection
    /// Gives us the definition of the projection to the screen
    /// using a tangent plane projection, centered on (ra,dec).
    /// </summary>
    public class TANProjection : IProjection
    {
        public static string Revision
        {
            get
            {
                return "$Revision: 1.2 $";
            }
        }
        private Int32 width, height;					// screen size
        private PointF cOffset;							// reference point offset
        private double scale;							// image scale
        private double[] n, w, u;						// normal, west, up vectors
        /// <summary>
        /// InitProjection. Initialize the transformation for the projection
        /// to be used on the screen.
        /// </summary>	
        public TANProjection(double ra_, double dec_, double ppd_, int width_, int height_)
        {
            n = V3.Normal(ra_, dec_);
            w = V3.West(ra_, dec_);
            u = V3.North(ra_, dec_);

            scale = ppd_ / V3.D2R;
            width = width_;
            height = height_;
            cOffset = new PointF((float)(width / 2.0), (float)(height / 2.0));
        }

        //------------------------------
        // Coordinate transformations
        //------------------------------

        /// <summary>
        /// EqToScreen. Maps an (ra,dec) coordinate to the Screen.
        /// This is the basic coordinate transformation function
        ///  </summary>
        /// <param name="ra_">Right ascension in degrees, double</param>
        /// <param name="dec_">Declination in degrees, double</param>
        /// <param name="size_">Symbol size in pixels, float</param>
        public PointF EqToScreen(double ra_, double dec_, float size_)
        {
            double[] r = V3.Normal(ra_, dec_);
            double gn = 0;
            double gw = 0;
            double gu = 0;
            for (int i = 0; i < 3; i++)
            {
                gn += r[i] * n[i];
                gw += r[i] * w[i];
                gu += r[i] * u[i];
            }
            PointF q = new PointF(cOffset.X - (float)(scale * gw), cOffset.Y - (float)(scale * gu));
            return q;
        }

        public PointF EqToScreen(PointEq e, float size_)
        {
            return EqToScreen(e.ra, e.dec, size_);
        }


        /// <summary>
        /// ScreenToEq. Maps a screen coordinate to an (ra,dec).
        /// This is the inverse coordinate transformation function
        ///  </summary>
        /// <param name="p">The screen coordinate of the point</param>
        public PointEq ScreenToEq(PointF p)
        {
            double[] r = new Double[3];
            //----------------------------------------
            // first subtract the offset, and rescale
            //----------------------------------------
            PointF q = new PointF((float)((cOffset.X - p.X) / scale), (float)((cOffset.Y - p.Y) / scale));
            //------------------------------------------------
            // now these are the dot products with w and u
            // compute the dot product with the normal vector
            //------------------------------------------------
            double gn = 1.0 - q.X * q.X - q.Y * q.Y;
            gn = Math.Sqrt(gn);
            for (int i = 0; i < 3; i++)
            {
                r[i] = gn * n[i] + q.X * w[i] + q.Y * u[i];
            }
            PointEq g = V3.ToEq(r);
            return g;
        }
    }



    /// <summary>
    /// STRProjection
    /// Gives us the definition of the projection to the screen
    /// using a stereographic projection, centered on (ra,dec).
    /// </summary>
    public class STRProjection : IProjection
    {
        public static string Revision
        {
            get
            {
                return "$Revision: 1.2 $";
            }
        }
        private Int32 width, height;					// screen size
        private PointF cOffset;							// reference point offset
        private double scale;							// image scale
        private double[] n, w, u;
        /// <summary>
        /// InitProjection. Initialize the transformation for the projection
        /// to be used on the screen.
        /// </summary>	
        public STRProjection(double ra_, double dec_, double ppd_, int width_, int height_)
        {
            n = V3.Normal(ra_, dec_);
            w = V3.West(ra_, dec_);
            u = V3.North(ra_, dec_);

            scale = ppd_ / V3.D2R;
            width = width_;
            height = height_;
            cOffset = new PointF((float)(width / 2.0), (float)(height / 2.0));
        }

        //-----------------------------
        // Coordinate transformations
        //-----------------------------

        /// <summary>
        /// EqToScreen. Maps an (ra,dec) coordinate to the Screen.
        /// This is the basic coordinate transformation function
        ///  </summary>
        /// <param name="ra_">Right ascension in degrees, double</param>
        /// <param name="dec_">Declination in degrees, double</param>
        /// <param name="size_">Symbol size in pixels, float</param>
        public PointF EqToScreen(double ra_, double dec_, float size_)
        {
            double[] r = V3.Normal(ra_, dec_);
            double gn = 0;
            double gw = 0;
            double gu = 0;
            for (int i = 0; i < 3; i++)
            {
                gn += r[i] * n[i];
                gw += r[i] * w[i];
                gu += r[i] * u[i];
            }
            double a = 2.0 / (1 + gn);
            PointF q = new PointF(cOffset.X - (float)(scale * a * gw), cOffset.Y - (float)(scale * a * gu));
            return q;
        }

        public PointF EqToScreen(PointEq e, float size_)
        {
            return EqToScreen(e.ra, e.dec, size_);
        }


        public PointEq ScreenToEq(PointF p)
        {
            double[] r = new Double[3];

            //----------------------------------------
            // first subtract the offset, and rescale
            //----------------------------------------
            PointF q = new PointF((float)((cOffset.X - p.X) / scale), (float)((cOffset.Y - p.Y) / scale));

            //------------------------------------------------
            // now these are the dot products with w and u
            // compute the dot product with the normal vector
            //------------------------------------------------
            double rho = 0.25 * (q.X * q.X + q.Y * q.Y);
            for (int i = 0; i < 3; i++)
            {
                r[i] = ((1.0 - rho) * n[i] + q.X * w[i] + q.Y * u[i]) / (1.0 + rho);
            }
            PointEq g = V3.ToEq(r);
            return g;
        }
    }
}
/*      
        Revision History
        $Log: projection.cs,v $
        Revision 1.2  2003/04/10 21:54:32  szalay
        projection interface added
*/
