using System;

namespace Sciserver_webService.SDSSFields
{
	/// <summary>
	/// A simple class that holds (ra,dec) and (mu,nu) coordinates.
	/// </summary> 	
	public class SdssCoord
	{
		public double ra, dec, mu, nu;
		
		/// <summary>
		/// Default constructor
		/// </summary>
		public SdssCoord()
		{
		}

		/// <summary>
		/// Constructor with coords to set
		/// </summary>
		/// <param name="ra">RA (double)</param>
		/// <param name="dec">Dec (double)</param>
		/// <param name="mu">mu (double)</param>
		/// <param name="nu">nu (double)</param>
		public SdssCoord(double ra, double dec, double mu, double nu)
		{
			this.ra = ra;
			this.dec = dec;
			this.mu = mu;
			this.nu = nu;
		}
		
	}
}

