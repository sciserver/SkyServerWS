using System;
using System.Data;
using System.Xml.Serialization;
using System.Collections;
namespace Sciserver_webService.SDSSFields
{
	/// <summary>
	/// This class holds astrometric info for passband
	/// </summary> 
	public class Band
	{
		[XmlAttribute] public string filter;
		protected double a, b, c, d, e, f, node, incl;
		public string url = String.Empty;
		public double pixperdeg;
		public SdssCoord center;
		//public SdssCoord[] corner;
		public WCS wcs;

		protected int npix1=2048, npix2=1489; // col, row
		protected double d2r = Math.PI/180.0;

		/// <summary>
		/// Default constructor
		/// </summary>
		public Band()
		{
		}

		/// <summary>
		/// Constructor with data to init variables and the name of the passband
		/// </summary>
		/// <param name="row">Field info from query (DataRow)</param>
		/// <param name="band">Passband name (string)</param>
		public Band(DataRow row, string band)
		{
			if ("ugriz".IndexOf(band.ToLower())<0) 
				throw new ApplicationException ("Parameter 'band' is invalid! Should be one of u,g,r,i or z...");
			this.filter = band;
			this.node = (double) row["node"];
			this.incl = (double) row["incl"];
			string postfix = "_"+band;
            //this.a = (double)row["a"+postfix];
            //this.b = (double)row["b"+postfix];
            //this.c = (double)row["c"+postfix];
            //this.d = (double)row["d"+postfix];
            //this.e = (double)row["e"+postfix];
            //this.f = (double)row["f"+postfix];
            // this Convert is used because only dr8 gives problem it has float values 
            // otherwise simple (double) cast works for all others
            this.a = Convert.ToDouble(row["a" + postfix]);
            this.b = Convert.ToDouble(row["b" + postfix]);
            this.c = Convert.ToDouble(row["c" + postfix]);
            this.d = Convert.ToDouble(row["d" + postfix]);
            this.e = Convert.ToDouble(row["e" + postfix]);
            this.f = Convert.ToDouble(row["f" + postfix]);
			this.pixperdeg = 9088;
			this.wcs = this.Wcs();
		}


        
		/// <summary>
		/// Calculate (RA,Dec) from (mu,nu) coordinates in the field
		/// </summary>
		/// <param name="mu">mu survey coord (double)</param>
		/// <param name="nu">nu survey coord (double)</param>
		/// <returns>Coords (RA,Dec) and (mu,nu) (SdssCoord)</returns>
		public SdssCoord CoordsAtMuNu (double mu, double nu)
		{
			double c_mu = Math.Cos(d2r*(mu-this.node));
			double s_mu = Math.Sin(d2r*(mu-this.node));
			double c_nu = Math.Cos(d2r*nu);
			double s_nu = Math.Sin(d2r*nu);
			double c_i  = Math.Cos(d2r*this.incl);
			double s_i  = Math.Sin(d2r*this.incl);
			// calc ra & dec
			double dec = Math.Asin(s_nu*c_i + s_mu*c_nu*s_i) /d2r;
			double ra = Math.Atan2(s_mu*c_nu*c_i-s_nu*s_i,c_mu*c_nu) /d2r + this.node;
			if (ra < 0.0) ra += 360;
			return new SdssCoord(ra, dec, mu, nu);
		}

		/// <summary>
		/// Calculate (RA,Dec) and (mu,nu) from pixel coordinates in the image
		/// </summary>
		/// <param name="crpix1">column - pixel coordinate (double)</param>
		/// <param name="crpix2">row - pixel coordinate (double)</param>
		/// <returns>Coords (RA,Dec) and (mu,nu) (SdssCoord)</returns>
		public SdssCoord CoordsAtPix (double crpix1, double crpix2)
		{
			return CoordsAtMuNu (a + crpix1*c + crpix2*b, d + crpix1*f + crpix2*e);
		}
		
		/// <summary>
		/// Get the corners of the frame
		/// </summary>
		/// <returns>list of corner coords (SdssCoord[])</returns>
		public SdssCoord[] CornerCoords()
		{
			SdssCoord[] corner = new SdssCoord[4];
			int n = 0;
			for (int i=0; i<2; i++)
			{
				for (int j=0; j<2; j++)
				{
					corner[n++] = CoordsAtMuNu (a+i*(npix1*c+npix2*b), d+j*(npix1*f+npix2*e));
				}
			}
			return corner;
		}

		/// <summary>
		/// Solve symmetric 2D linear equations
		/// </summary>
		/// <param name="a11">matrix element 11 (double)</param>
		/// <param name="a12">matrix element 12 (double)</param>
		/// <param name="a22">matrix element 22 (double)</param>
		/// <param name="v1">result elem 1 (double)</param>
		/// <param name="v2">result elem 2 (double)</param>
		/// <param name="x1"></param>
		/// <param name="x2"></param>
		static protected void SolveSym2D (double a11, double a12, double a22, 
			double v1, double v2, 
			out double x1, out double x2)
		{
			double det = a11*a22-a12*a12;
			x1 = ( a22*v1 - a12*v2)/det;
			x2 = (-a12*v1 + a11*v2)/det;
		}
								
		/// <summary>
		/// Get WCS at center of the image
		/// </summary>
		/// <returns>WCS</returns>
		public WCS Wcs() 
		{
			return this.Wcs(Math.Floor(npix1/2.0),Math.Floor(npix2/2.0));
		}

		/// <summary>
		/// Get WCS at any pixel in the image
		/// </summary>
		/// <param name="crpix1">pixel coord 1 (double)</param>
		/// <param name="crpix2">pixel coord 2 (double)</param>
		/// <returns>WCS</returns>
		public WCS Wcs(double crpix1, double crpix2)
		{
			SdssCoord[] corner;
			center = CoordsAtPix(crpix1, crpix2);
			corner = CornerCoords();
			double mu, nu, ra, dec;

			// build linear eqns and solve for affine transform
			double mumu=0, nunu=0, munu=0, ramu=0, ranu=0, decmu=0, decnu=0;
			for (int i=0; i<corner.Length; i++)
			{
				mu = corner[i].mu-center.mu;
				nu = corner[i].nu-center.nu;
				ra = corner[i].ra-center.ra;
				dec = corner[i].dec-center.dec;
				// increment
				mumu += mu*mu;
				munu += mu*nu;
				nunu += nu*nu;
				ramu += ra*mu;
				ranu += ra*nu;
				decmu += dec*mu;
				decnu += dec*nu;
			}

			double p, q, s, r;
			SolveSym2D(mumu,munu,nunu, ramu, ranu,  out p, out q);
			SolveSym2D(mumu,munu,nunu, decmu,decnu, out s, out r);

			WCS wcs = new WCS();

			wcs.EPOCH = "J2000";
			wcs.CTYPE2="DEC--TAN";
			wcs.CTYPE1="RA---TAN";
			wcs.CUNIT1="deg";
			wcs.CUNIT2="deg";

			wcs.NAXIS1 = this.npix1;
			wcs.NAXIS2 = this.npix2;

			wcs.CRPIX1 = crpix1+0.5; // different SDSS vs FITS convention (Steve Kent) 
			wcs.CRPIX2 = crpix2+0.5; // -"-
			wcs.CRVAL1 = center.ra;
			wcs.CRVAL2 = center.dec;

            wcs.CD1_1 = p * c + q * f;
            wcs.CD1_2 = p * b + q * e;
            wcs.CD2_1 = s * c + r * f;
            wcs.CD2_2 = s * b + r * e;

            // john's suggestions for a fix
            double scale = Math.Cos(center.dec * Math.PI/180);
            wcs.CD1_1 *= scale;
            wcs.CD1_2 *= scale;

			return wcs;
		}

        //******** For SIAP etc @Deoyani added by deoyani
        /// <summary>
        /// Constructor with data to init variables and the name of the passband
        /// </summary>
        /// <param name="row">Field info from query (DataRow)</param>
        /// <param name="band">Passband name (string)</param>
        public Band(Hashtable row, string band)
        {
            if ("ugriz".IndexOf(band.ToLower()) < 0)
                throw new ApplicationException("Parameter 'band' is invalid! Should be one of u,g,r,i or z...");
            this.filter = band;
            this.node = Convert.ToDouble(row["node"]);
            this.incl = Convert.ToDouble(row["incl"]);
            string postfix = "_" + band;           
            // this Convert is used because only dr8 gives problem it has float values 
            // otherwise simple (double) cast works for all others
            this.a = Convert.ToDouble(row["a" + postfix]);
            this.b = Convert.ToDouble(row["b" + postfix]);
            this.c = Convert.ToDouble(row["c" + postfix]);
            this.d = Convert.ToDouble(row["d" + postfix]);
            this.e = Convert.ToDouble(row["e" + postfix]);
            this.f = Convert.ToDouble(row["f" + postfix]);
            this.pixperdeg = 9088;
            this.wcs = this.Wcs();
        }
	}
}

