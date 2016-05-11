using System;
using System.Data;
using System.Collections.Generic;
using System.Collections;
namespace Sciserver_webService.SDSSFields
{
	/// <summary>
	/// The Field class stores run, rerun, etc... and an array of the passbands with the astrometry (See Band)
	 /// @Deoyani Nandrekar-Heinis Feb 2015
    /// </summary> 
    [Serializable]
	public class Field
	{
		public int stripe, run, rerun, camcol, field;
        public ulong fieldId;
		public double node, incl;
		public Band[] passband;
        public string[] bandUrls;
        public List<KeyValuePair<string, string>> bandUrl = new List<KeyValuePair<string, string>>();

		/// <summary>
		/// Default constructor
		/// </summary>
		public Field()
		{
		}

		/// <summary>
		/// Constructor to init obj with real data
		/// </summary>
		/// <param name="row">Field parameters (DataRow)</param>
		public Field(DataRow row)
		{
			this.stripe = (int) row["stripe"];
			this.fieldId = Convert.ToUInt64(row["fieldId"]);
			this.run = (int) row["run"];
			this.rerun = (int) row["rerun"];
			this.camcol = (int) row["camcol"];
			this.field = (int) row["field"];
			this.node = (double) row["node"]; // only for display
			this.incl = (double) row["incl"]; //  -"-
			//double ra0 = (double) row["ra"];
			//double dec0 = (double) row["dec"];
			//this.center = new SdssCoord(ra0,dec0,-999,-999);
            
            string[] bands = {"u","g","r","i","z"};
            
			passband = new Band[bands.Length];
			for (int i=0; i<passband.Length; i++)
			{
				passband[i] = new Band(row, bands[i]);
			}
            
			string[] urls ={row["u_url"].ToString(), row["g_url"].ToString(), row["r_url"].ToString(), row["i_url"].ToString(),row["z_url"].ToString() };
            //for(int i=0;i<bands.Length;i++){
            //    bandUrl.Add(new KeyValuePair<string,string>(bands[i], urls[i]));
            //}

            this.bandUrls = urls;
		}
	}
}

