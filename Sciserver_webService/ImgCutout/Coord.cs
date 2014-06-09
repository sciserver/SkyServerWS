///Current version
///ID:          $Id: Coord.cs,v 1.3 2003/04/08 17:12:00 nieto Exp $
///Revision:    $Revision: 1.3 $
///Date:        $Date: 2003/04/08 17:12:00 $
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
namespace Sciserver_webService.ImgCutout
{
	
    public class PointEq
	{
		public double ra,dec;			// the right ascension and declination
		public PointEq(double pRa, double pDec)
		{
			ra	= pRa;
			dec	= pDec;
		}
	}


	/// <summary>
	/// Summary description for Coord.
	/// </summary>
	/// <summary>
	/// Coord private class carries the transformation of ra/dec to nu/mu and xy. 
	/// </summary>
	public class Coord 
	{
		public static string Revision
		{
			get 
			{
				return "$Revision: 1.3 $";
			}
		}


		public double ra,dec;			        // point (ra, dec) in J2000 degrees
		public double mu,nu;			        // point (ra, dec) in J2000 degrees
		public double row, col;			        // the row and column number in the frame
		private float x,y;				        // pixel offsets from upper left corner 
										        // corresponding to (ra,dec), scaled 
		public double scale;			        // image scale 
		private double a,b,c,d,e,f,node,incl;	// affine tranformation of the stripe to ra,dec
		public string info;				        // information about the field for debugging
		public Matrix m;
        public static double D2R = Math.PI / 180.0;	// degrees to radians
        public double crval1, crval2, crpix1, crpix2, cdelt1, cdelt2;	// wcs for 2mass


		public Coord()
		{}


		public Coord(double _a,double _b,double _c,double _d,double _e,double _f,
			double _node,double _incl,double _scale, string _info) 
		{
			a = _a;
			b = _b;
			c = _c;
			d = _d;
			e = _e;
			f = _f;
			node = _node;
			incl = _incl;
			scale= _scale;
			info = _info;
			m	 = null;
		}

      
		/// <summary>
		/// The copy method copies the values of the coord parameter into the current coordinate object
        /// <param name=coord>			
        /// </summary>
		public void copy( Coord coord ) 
		{
			this.a			= coord.a;
			this.b			= coord.b;
			this.c			= coord.c;
			this.d			= coord.d;
			this.e			= coord.e;
			this.f			= coord.f;
			this.node		= coord.node;
			this.incl		= coord.incl;
			this.scale		= coord.scale;
			this.info		= coord.info;
			this.m			= coord.m;
		}


		/// <summary>
		/// EqtoScreen goes from equatorial coordinates to xy
		/// Sets ra, dec and consequently xOffset and yOffset of 
		/// Coord according to the astrometric transformation.
		/// </summary>
		/// <param name="ra">right ascension degrees in J2000</param>
		/// <param name="dec">declination degrees in J2000</param>
		public PointF EqToFrame(double pRa, double pDec) 
		{   
			// save the angles
			ra	= pRa;
			dec	= pDec;

			// convert angles to radians
			double inclR = incl*D2R;
			double raR	 = (pRa-node) *D2R;
			double decR	 = pDec*D2R;

			// first go to (mu,nu)
			double gx	=  Math.Cos(raR) * Math.Cos(decR);
			double gy	=  Math.Sin(raR) * Math.Cos(decR);
			double gz	=  Math.Sin(decR);

			// reverse rotation by incl around x
			double qx	=  gx;
            double qy	=  gy*Math.Cos(inclR) + gz * Math.Sin(inclR);            
			double qz	= -gy*Math.Sin(inclR) + gz * Math.Cos(inclR);

			// compute mu, nu
			mu	=  Math.Atan2(qy,qx)/D2R + node;
			nu	=  Math.Asin(qz)/D2R;

			// adjust for wraparound of the difference
			double dmu	=  mu - a;
			if (dmu<-180)  dmu += 360;
			if (dmu> 180)  dmu -=360;
 
			// set up the determinant
			double det	 = b * f - e * c;
			double col   = ( (nu-d)*b - dmu*e )/det;
			double row   = ( dmu*f - (nu-d)*c )/det;

			// compute pixel coordinates
			x	= (float)(col/scale);
			y	= (float)(row/scale);
            PointF p = new PointF(x,y);
			return p; 
		} 


		/// <summary>
		/// FrameToEq goes from Frame x,y coordinates and sets ra and dec
		/// </summary>
		/// <param name="x">pixels across (scaled for zoom level)</param>
		/// <param name="y">pixels down (scaled for zoom level)</param>
		public PointEq FrameToEq(float x, float y)  
		{
			// x,y need to be in original pixel scale
			double col = x;
            double row = y;

			// Great circle coordinates in degrees
			mu = (a +  b*row  +  c*col);
			nu = (d +  e*row  +  f*col);

			// convert to radians, then to ra,dec
			double muR = (mu - node) * D2R;
			double nuR = nu * D2R;
			double xg  = Math.Cos(muR) * Math.Cos(nuR);
			double yg  = Math.Sin(muR) * Math.Cos(nuR);
			double zg  = Math.Sin(nuR);

			// compute unit vector
			double inR = incl * D2R;
			double qx  = xg;
			double qy  = yg * Math.Cos(inR) - zg*Math.Sin(inR);
			double qz  = yg * Math.Sin(inR) + zg*Math.Cos(inR);

			// compute ra, dec
			double pRa  = Math.Atan2(qy,qx) / D2R  +  node;
			double pDec = Math.Asin(qz) / D2R;
			if (pRa>360) pRa -= 360;
			if (pRa<0)   pRa += 360;
			ra	= pRa;
			dec	= pDec;
			return new PointEq(pRa,pDec);
		}


        /// <summary>
		/// rotateDegreesToNorth returns the number of degrees 
		/// the image must be rotated to make North be "up"
		/// </summary>
		/// <param name="ra"> center point of the frame</param>
		/// <param name="dec">center point of the frame</param>
		public double rotateDegreesToNorth(double ra, double dec)  
		{
			// compute point due North, and center            
            PointF n = this.EqToFrame(ra, dec+0.1);
			PointF c = this.EqToFrame(ra,dec);
            double angle = -90 - Math.Atan2((double)(n.Y - c.Y), (double)(n.X - c.X)) / D2R;
			return angle;
		}


        /// <summary>
        /// Converts ra into decimal degrees.
        /// </summary>
        public static double hms2deg(String s)
        {
            string[] a = s.Split(':');
            double v = 15.0 * Convert.ToDouble(a[0]) + Convert.ToDouble(a[1]) / 4.0 + Convert.ToDouble(a[2]) / 240.0;
            return v;
        }

        /// <summary>
        /// Converts dec into decimal degrees.
        /// </summary>
        public static double dms2deg(String s)
        {
            string[] a = s.Split(':');
            double v;
            if (s.LastIndexOf("-") == 0)
                v = -(-1.0 * Convert.ToDouble(a[0]) +
                Convert.ToDouble(a[1]) / 60.0 +
                Convert.ToDouble(a[2]) / 3600.0);
            else
                v = (Convert.ToDouble(a[0]) +
                Convert.ToDouble(a[1]) / 60.0 +
                Convert.ToDouble(a[2]) / 3600.0);
            return v;
        }


        //%%%%%%%%%%%%%%%%%%%%%%%  2MASS Section %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%//
        
        //// WCS for 2mass images
        public void get_theta_phi(double alpha_deg, double delta_deg, double alpha_p_deg, double delta_p_deg, ref double theta, ref double phi)
        {

            double deg2rad = Math.PI / 180.0;
            double alpha = alpha_deg * deg2rad;
            double delta = delta_deg * deg2rad;
            double alpha_p = alpha_p_deg * deg2rad;
            double delta_p = delta_p_deg * deg2rad;

            double phi_p = 180.0 * deg2rad;  //;delta_p
            double cd = Math.Cos(delta);
            double sd = Math.Sin(delta);
            double cdp = Math.Cos(delta_p);
            double sdp = Math.Sin(delta_p);

            phi = phi_p + Math.Atan2(-cd * Math.Sin(alpha - alpha_p), sd * cdp - cd * sdp * Math.Cos(alpha - alpha_p));
            theta = Math.Asin(sd * sdp + cd * cdp * Math.Cos(alpha - alpha_p));

        }

        public PointF EqToFrame(double ra, double dec, double ra_ref, double dec_ref, double scale, double x_ref, double y_ref)
        {

            double deg2rad = Math.PI / 180.0;
            double theta = 0.0;
            double phi = 0.0;
            get_theta_phi(ra, dec, ra_ref, dec_ref, ref theta, ref phi);

            double rtheta = 1.0 / Math.Tan(theta) / deg2rad;
            double x = rtheta * Math.Sin(phi);
            double y = -rtheta * Math.Cos(phi);
            //now  get the actual pixels values  //-1 if first pixel is at 0
            double x_pix = -x / scale + x_ref - 1;
            double y_pix = y / scale + y_ref - 1;
            PointF p = new PointF((float)x, (float)y);
            return p;
        }

        //public PointEq FrameToEq(float x, float y, double crpix1, double crpix2, double cdelt1, double cdelt2, double crval1, double crval2)
        //{

        //    double pRa = (x - crpix1 +1) * cdelt1 + crval1;
        //    double pDec = (y - crpix2 +1 ) * cdelt2 + crval2;            
        //    ra = pRa;
        //    dec = pDec;
        //    return new PointEq(pRa, pDec);

        //}
        public PointEq FrameToEq(float x, float y, double crpix1, double crpix2, double cdelt1, double cdelt2, double crval1, double crval2)
        {
            double deg2rad = Math.PI / 180.0;

            double x_int = cdelt1 * (x - crpix1);
            double y_int = cdelt2 * (y - crpix2);

            double phi = Math.Atan2(x_int, (-y_int));
            double rtheta = Math.Sqrt(x_int * x_int + y_int * y_int);
            double theta = Math.Atan(180.0 / Math.PI / rtheta);


            double deltap = crval2 * deg2rad;
            double alphap = crval1 * deg2rad;
            double phip = Math.PI;

            double ct = Math.Cos(theta);
            double st = Math.Sin(theta);
            double cdp = Math.Cos(deltap);
            double sdp = Math.Sin(deltap);

            double t1 = st * cdp - ct * sdp * Math.Cos(phi - phip);
            double t2 = -Math.Cos(theta) * Math.Sin(phi - phip);
            ra = alphap + Math.Atan(t2 / t1);

            double t = st * sdp + ct * cdp * Math.Cos(phi - phip);
            dec = Math.Asin(t);

            ra = ra / deg2rad;
            dec = dec / deg2rad;
            return new PointEq(ra, dec);
        }

        public void copy2Mass(Coord coord)
        {
            this.crval1 = coord.crval1;
            this.crval2 = coord.crval2;
            this.crpix1 = coord.crpix1;
            this.crpix2 = coord.crpix2;
            this.cdelt1 = coord.cdelt1;
            this.cdelt2 = coord.cdelt2;
            this.m = coord.m;
        }

        // This Constructor for 2mass images with wcs read from fits files
        public Coord(double _crval1, double _crval2, double _crpix1, double _crpix2, double _cdelt1, double _cdelt2)
        {
            crval1 = _crval1;
            crval2 = _crval2;
            crpix1 = _crpix1;
            crpix2 = _crpix2;
            cdelt1 = _cdelt1;
            cdelt2 = _cdelt2;
            m = null;
        }

       /////%%%%%%%%%%%%%%%%%%%%%%%%%  end 2mass section %%%%%%%%%%%%%%%%%%%%%%%%% ///
    
    } // end of Coord Class
}
/* Revision History
        $Log: Coord.cs,v $
        Revision 1.4  2011-12-23 Alex 
            removed the DR8 changes by Deoyani, 
            root of the problem was in SDSSConstans.FieldGeometry
        Revision 1.3  2003/04/08 17:12:00  nieto
        Trying to include projection.cs class in project
        Revision 1.2  2003/03/25 23:18:33  nieto
        testing draw fields with their own coord       
*/
