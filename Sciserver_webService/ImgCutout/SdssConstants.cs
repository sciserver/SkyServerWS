// #define DR2PATCH
///Current version
///ID:          $Id: SdssConstants.cs,v 1.10 2007/07/23 17:10:30 nieto Exp $
///Revision:    $Revision: 1.10 $
///Date:        $Date: 2007/07/23 17:10:30 $
using System;
using System.Configuration;
using System.Drawing;
using System.ComponentModel;        
using System.Reflection;
namespace Sciserver_webService.ImgCutout
{
	/// <summary>
	/// The SdssConstants class holds all constants related to SDSS.
	/// </summary>
	public class SdssConstants
	{
		/// <summary>
		/// Revision from CVS
		/// </summary>
		public static string Revision
		{
			get {	return "$Revision: 2.00 $";	}
		}
        public const double FrameHalfDiag = 8.4F;			            // arc mins			      
		public const double	 plateRadiusArcMin = 89.4;					// arcMinute
		public const double	 plateRadiusDeg	= 1.49;					    // degrees        
		public const byte	 sflag		    = 1;						// specObject
		public const byte	 pflag		    = 2;						// photoObject
		public const byte	 tflag		    = 4;						// targetObject
		public const byte	 mflag		    = 8;						// maskObject
		public const byte	 plateflag	    = 16;						// plateObject

        //public const string Twomass = "2Mass";
        //public const string sdss = "sdss";
        //public static string survey = "sdss";
        public static Boolean isSdss = true;


        private static string _DataRelease = 
            ConfigurationManager.AppSettings["DataRelease"].ToUpper();

        private static int _DR = -1;
        private static int _prevVersion = 7; /// Till dr7 some constants were different

        public SdssConstants()
		{
			// TODO: Add constructor logic here
		}

      

        public static float FrameHeight{
            get
            {
                if (isSdss)
                {
                    return 1489.0F; // For SDSS
                }
                else
                {
                    return 1024.0F; // 2mass height

                }
            }
        
        }

        public static float FrameWidth
        {
            get
            {
                if (isSdss)
                {

                    return 2048.0F; // For SDSS
                }
                else
                {
                    return 512.0F; // 2mass 
                }
            }
          
        }

        public static int PixelsPerDegree 
        {        
            get
            {
                if (isSdss)
                {
                    return 9089; // For SDSS
                }
                else
                {
                    return 5120; // 2mass
                }            
            }
        }
        
        public static double PixelsPerArcMin
        {
            get
            {
                return PixelsPerDegree / 60.0;
            }
        }

       
        public static int prevVersion {
            get 
            {
                return _prevVersion;
            }
        }

        public static int MaxZoom
        {
            get
            {
                if (isSdss)
                {
                    if (sDR > prevVersion)
                        return 4;
                    else
                        return 7;
                }
                else {
                    return 1;
                }
            }
        }

        public static int zoom10(int zoom)
        {
            if (sDR > prevVersion)
            {
                int z10;
                switch (zoom)
                {
                    case 0:  z10 =  0; break;
                    case 1:  z10 = 50; break;
                    case 2:  z10 = 25; break;
                    default: z10 = 12; break;
                }
                return z10;
            }
            else
            {
                return (zoom > 3) ? 30 : zoom * 10;
            }

        }

        public static string OutlineTable
        {
            get
            {
                if (sDR > prevVersion)
                    return "AtlasOutline";
                else
                    return "ObjMask";
            }
        }

        public static int OutlinePix
        {
            get
            {
                if (sDR > prevVersion)
                    return 1;
                    //return 4;
                else
                    return 4;
            }
        }

        public static int OutlineOff
        {
            get
            {
                if (sDR > prevVersion)
                    return 1;
                else
                    return 0;
            }
        }

        
        
        private static float ClipXLeft
        {
            get
            {
                if (sDataRelease.Equals("DR1"))
                    return 32.0F;                   // Alex: I think it is 32  (was 48) !!!
                else if  (sDR > prevVersion)//(sDataRelease.Equals("DR10"))
                    return 32.0F;
                else
                    return 24.0F;
            }
        }

        private static float ClipXRight
        {
            get
            {
                if (sDataRelease.Equals("DR1"))
                    return 32.0F;                   // Alex: I think it is 32  (was 48) !!!
                else if (sDR >prevVersion)//(sDataRelease.Equals("DR10"))
                    return 32.0F;
                else
                    return 40.0F;
            }
        }

        private static float ClipYTop
        {
            get
            {
                //if (sDataRelease.Equals("DR10"))
                //    return 33.0F;

                //else if (sDR > prevVersion)
                //    return 48.0F;

                if (sDR>prevVersion)
                    return 33.0F;
                else
                    return 64.0F;
            }
        }
        private static float ClipYBottom
        {
            get
            {
                //if (sDataRelease.Equals("DR10"))
                //    return 32.0F;

                //else if (sDR > prevVersion)
                //    return 48.0F;
                if (sDR > prevVersion)
                    return 32.0F;
                else
                    return 64.0F;
            }
        }

        /// <summary>
        /// sDataRelease. String version of the DR version, or NULL if invalid
        /// </summary>
        public static string sDataRelease 
        {
            get
            {
                if(
                    (_DataRelease=="EDR") ||
                    (_DataRelease=="DR1") ||
                    (_DataRelease=="DR2") ||
                    (_DataRelease=="DR3") ||
                    (_DataRelease=="DR4") ||
                    (_DataRelease=="DR5") ||
                    (_DataRelease=="DR6") ||
                    (_DataRelease=="DR7") ||
                    (_DataRelease=="DR8") ||
                    (_DataRelease=="DR9") ||
                    (_DataRelease=="DR10") 
                  ) return _DataRelease;
                return null;
            }
        }

        /// <summary>
        /// sDataRelease. String version of the DR version, or NULL if invalid
        /// </summary>
        public static int sDR
        {
            get
            {
                if (sDataRelease.Equals("EDR"))
                    _DR = 0;                
                else
                    _DR = int.Parse(sDataRelease.Remove(0,2));
                
                if (
                    (_DR == 0) ||
                    (_DR == 1) ||
                    (_DR == 2) ||
                    (_DR == 3) ||
                    (_DR == 4) ||
                    (_DR == 5) ||
                    (_DR == 6) ||
                    (_DR == 7) ||
                    (_DR == 8) ||
                    (_DR == 9) ||
                    (_DR == 10)
                  ) return _DR;
                return -1;
            }
        }

 
		/// <summary>
		/// FieldGeometry. Returns the four corners of the field
		/// in unscaled pixel coordinates with optional clipping
		///  </summary>
		public static PointF[]	FieldGeometry(bool clip )
		{
			float fX1, fX2, fY1, fY2;
            //-----------------------------------------------------------------
			// compute the pixel coordinates of corners with optional clipping
            //-----------------------------------------------------------------
            if (!isSdss) clip = false;  // no clipping for twomass images
            int clipit	= (clip?1:0);
            float offY = -0.0F;
            fX1 = (float) (ClipXLeft * clipit);				
    		fX2	= (float) (FrameWidth - ClipXRight * clipit);							
			fY1	= (float) (ClipYTop * clipit +offY);
			fY2	= (float) (FrameHeight - ClipYBottom * clipit +offY);

			//-----------------------------------
            // create array for the four corners
            //-----------------------------------
			PointF[] p = new PointF[4];

            if (sDR > prevVersion)
            {
                p[0] = new PointF(fX1, fY2);
                p[1] = new PointF(fX2, fY2);
                p[2] = new PointF(fX2, fY1);
                p[3] = new PointF(fX1, fY1);
            }
            else {
                p[0] = new PointF(fX1, fY1);
                p[1] = new PointF(fX2, fY1);
                p[2] = new PointF(fX2, fY2);
                p[3] = new PointF(fX1, fY2);
            }
            return p;
		}
	}	
}
/* Revision History
        $Log: SdssConstants.cs,v $
        Revision 2.00  2011/12/20 16:40 szalay
            stripped out a lot of unncessary code, and made only the essential properties public
            fixed FieldGeometry to take care of the image flip in the FITS files
        Revision 1.10  2007/07/23 17:10:30  nieto
        Set to handle up to DR10 label (as far as no clipping change). Added JOIN syntax in SQL queries
        Revision 1.8  2005/10/20 14:22:08  nieto
        Added DR5 patch to fix the wrong clipping
        Revision 1.7  2005/08/08 19:45:47  nieto
        Adding DR4 to the patch fro the wrong cut of jpegs
        Revision 1.6  2005/02/24 21:32:35  nieto
        Changed to show correct DR3 label (repeats DR2Patch for DR3)
        Revision 1.5  2004/02/23 14:07:36  nieto
        Major changes in order to:
        	- Improve Message/Exception errors.
        	- Added a new Web Method that accepts query to mark objects:
        		SQL query
        		List of objects
        		Mark String like SR(10, 30) Stars with magnitude
        				 between 10 and 30 in the R band
        Revision 1.3  2003/04/08 17:12:00  nieto
        Trying to include projection.cs class in project
        Revision 1.2  2003/03/19 18:49:55  nieto
        more clean up
*/
