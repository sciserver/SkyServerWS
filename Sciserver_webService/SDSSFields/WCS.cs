using System;

namespace Sciserver_webService.SDSSFields
{
	/// <summary>
	/// This class holds all the WCS parameters
	/// </summary> 
	public class WCS
	{
		public int NAXIS1, NAXIS2;
		public string CTYPE1, CTYPE2;
		public string CUNIT1, CUNIT2;
		public string EPOCH;
		public double CRPIX1, CRPIX2; // col, row
		public double CRVAL1, CRVAL2; // dec, ra
		public double CD1_1, CD1_2, CD2_1, CD2_2;

		public WCS()
		{
		}

	}
}


