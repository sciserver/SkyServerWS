using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Sciserver_webService.ImgCutout
{
/*
	public class PointEq
	{
		public double ra,dec;						// the right ascension and declination
		public PointEq(double ra_, double dec_)
		{
			ra	= ra_;
			dec	= dec_;
		}
	}
*/
	public class V3
	{
		public static double D2R = Math.PI/180.0;	// degrees to radians

		public static double[] FromXYZ(double x_, double y_, double z_)
		{
			double[] r = new double[3];	
			r[0] = x_;
			r[1] = y_;
			r[2] = z_;
			return r; 
		}

		/// <summary>
		/// Dot. Calculate the dot product of two Cartesian vectors
		/// </summary>
		/// <param name="r">First V3 vector</param>
		/// <param name="s">Second V3 vector</param>
		/// <returns>dot product, as a double</returns>
		public static double Dot(double[] r_, double[] s_)
		{
			double dot=0.0;
			for (int m=0;m<3;m++)
			{
				dot += r_[m]*s_[m];
			}
			return dot;
		}

		/// <summary>
		/// Wedge. Calculate the wedge product of two Cartesian vectors
		/// </summary>
		/// <param name="r">First V3 vector</param>
		/// <param name="s">Second V3 vector</param>
		/// <returns>wedge product, as a double[]</returns>
		public static double[] Wedge(double[] r, double[] s)
		{
			double[] w = new double[3];
			w[0] = r[1]*s[2]-r[2]*s[1];
			w[1] = r[2]*s[0]-r[0]*s[2];
			w[2] = r[0]*s[1]-r[1]*s[0];
			return w;
		}

		/// <summary>
		/// Normalize. Return the normalized unit vector.
		/// </summary>
		/// <param name="r">V3 vector</param>
		/// <returns>normalized vector as a double[]</returns>
		public static double[] Normalize(double[] r)
		{
			double norm = 0.0;
			double sc   = 0.0;
			double[] n = new double[3];
			for (int i=0;i<3;i++) norm += r[i]*r[i];
			if (norm>0) sc = 1.0/Math.Sqrt(norm);
			for (int i=0;i<3;i++) n[i] = sc* r[i];
			return n;
		}

		/// <summary>
		/// Normal. Calculate the Cartesian normal vector 
		/// corresponding to the point (ra,dec).
		/// </summary>
		/// <param name="ra_">Right ascension in degrees</param>
		/// <param name="dec_">Declination in degrees</param>
		/// <returns>Array of 3 doubles, a 3-vector</returns>
		public static double[] Normal(double ra_, double dec_) 
		{
			double[] n = new double[3];	
			n[0] =  Math.Cos(dec_ * D2R) * Math.Cos(ra_ * D2R);
			n[1] =  Math.Cos(dec_ * D2R) * Math.Sin(ra_ * D2R);
			n[2] =  Math.Sin(dec_ * D2R);
			return n;
		}

		/// <summary>
		/// Normal. Calculate the Cartesian normal vector 
		/// corresponding to the PointEq p(ra,dec).
		/// </summary>
		/// <param name="p">PointEq object</param>
		/// <returns>Array of 3 doubles, a 3-vector</returns>
		public static double[] Normal(PointEq p)
		{
			double[] n = new double[3];
			n[0] = Math.Cos(D2R * p.dec)*Math.Cos(D2R * p.ra);
			n[1] = Math.Cos(D2R * p.dec)*Math.Sin(D2R * p.ra);
			n[2] = Math.Sin(D2R * p.dec);
			return n;
		}

		/// <summary>
		/// GetNorth. Calculate the Cartesian normal vector 
		/// corresponding to the North direction at the point (ra,dec).
		/// </summary>
		/// <param name="ra_">Right ascension in degrees</param>
		/// <param name="dec_">Declination in degrees</param>
		/// <returns>Array of 3 doubles, a 3-vector</returns>
		public static double[] North(double ra_, double dec_) 
		{
			double[] u = new double[3];	
			u[0] = -Math.Sin(dec_ * D2R) * Math.Cos(ra_ * D2R);
			u[1] = -Math.Sin(dec_ * D2R) * Math.Sin(ra_ * D2R);
			u[2] =  Math.Cos(dec_ * D2R);
			return u;
		}

		/// <summary>
		/// West. Calculate the Cartesian normal vector 
		/// corresponding to the west direction at the point (ra,dec).
		/// </summary>
		/// <param name="ra_">Right ascension in degrees</param>
		/// <param name="dec_">Declination in degrees</param>
		/// <returns>Array of 3 doubles, a 3-vector</returns>
		public static double[] West(double ra_, double dec_) 
		{
			double[] w = new double[3];	
			w[0] = -Math.Sin(ra_ * D2R);
			w[1] =  Math.Cos(ra_ * D2R);
			w[2] =  0;
			return w;
		}

		/// <summary>
		/// ToEq. Calculate the Equatorial point corresponding to 
		/// a Cartesian normal vector (x,y,z).
		/// </summary>
		/// <param name="x_">Cartesian x</param>
		/// <param name="y_">Cartesian y</param>
		/// <param name="z_">Cartesian z</param>
		/// <returns>Array of 2 doubles, a 2-vector</returns>
		public static PointEq ToEq(double[] r)
		{
			double _dec	= Math.Asin(r[2]) / D2R;
			double _ra  = 0.0;
			// catch r[0]=r[1]=0 case, the north/south pole
			if (Math.Abs(r[0])<1.0E-9 && Math.Abs(r[1])<1.0E-9)
			{
				_dec = 90.0*r[2];
			}
			else 
			{
				_ra	= Math.Atan2(r[1], r[0]) / D2R;
				if (_ra < 0) _ra += 360.0;
			}
			PointEq p = new PointEq(_ra,_dec);
			return p;
		}
	}


	public class HTMArc
	{
		public Int64 cvx, arc;
		public Int32 patch, draw;
		public double ra1, dec1, ra2, dec2, x, y, z, c;
		public HTMArc(Int64 cvx_, Int32 patch_, Int32 draw_, 
			double ra1_, double dec1_, double ra2_, double dec2_, 
			double x_, double y_, double z_, double c_,
			Int64 arc_) 
		{
			cvx  = cvx_;
			patch = patch_;
			draw = draw_;
			ra1  = ra1_;
			dec1 = dec1_;
			ra2  = ra2_;
			dec2 = dec2_;
			x = x_;
			y = y_;
			z = z_;
			c = c_;
			arc = arc_;
		}
	}

	public class HTMRegion
	{
		public Int64 rid;
		public Int16 mask;
		public string type;
		public int count, fill;
		public HTMArc[] arc;
		public HTMRegion(Int64 _rid, Int16 _mask, string _type)
		{
			rid   = _rid;
			mask  = _mask;
			type  = _type;
			count = 0;
			fill = 0;
		}
	}
}
