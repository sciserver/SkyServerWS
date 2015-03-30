using System;
using System.Collections.Generic;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Collections;
using System.Text;
using Sciserver_webService.UseCasjobs;
using Sciserver_webService.Common;

namespace Sciserver_webService.ImgCutout
{
    public class OverlayOptions
    {
        private SqlConnection SqlConn = null;
        public SDSSGraphicsEnv canvas = null;
        double ra ;
        double dec;
        float size;
        double radius;
        double fradius;
        int zoom;
        string datarelease;
        string token;

        public OverlayOptions(SDSSGraphicsEnv canvas, float size, double ra, double dec, double radius, int zoom, double fradius, string datarelease, string token)
        {
            this.canvas = canvas;
            this.ra = ra;
            this.dec = dec;
            this.size = size;
            this.radius = radius;
            this.zoom = zoom;
            this.fradius = fradius;
            this.datarelease = datarelease;
            this.token = token;
        }

        public OverlayOptions(SqlConnection sqlcon, SDSSGraphicsEnv canvas, float size, double ra, double dec, double radius, int zoom, double fradius)
        {
            this.SqlConn = sqlcon;
            this.canvas = canvas;
            this.ra = ra;
            this.dec = dec;
            this.size = size;
            this.radius = radius;
            this.zoom = zoom;
            this.fradius = fradius;
        }

        /// <summary>
        /// getFields. Display the outlines of the Fields on the canvas.
        /// Requires getFrames() to be run beforehand.
        /// </summary>
        internal void getFields(Hashtable cTable)
        {
            try
            {
                foreach (DictionaryEntry d in cTable)
                {
                    canvas.drawField((Coord)cTable[d.Key]);
                }
            }
            catch (Exception e)
            {
                showException("getFields()", "", e);
            }
        }


        /// <summary>
        /// getObjects(). Mark Photo|Spec|Target objects according to the drawing option string.
        /// </summary>
        internal void getObjects(bool drawPhotoObjs ,bool drawSpecObjs , bool drawTargetObjs)
        {
            byte flag = 0;
            if (drawPhotoObjs) flag |= SdssConstants.pflag;
            if (drawSpecObjs) flag |= SdssConstants.sflag;
            if (drawTargetObjs) flag |= SdssConstants.tflag;
            StringBuilder sQ = new StringBuilder(" select *");
            sQ.AppendFormat(" from dbo.fGetObjectsEq({0},{1},{2},{3},{4})",
                flag, ra, dec, radius, zoom);
            //SqlDataReader reader = null;

            RunCasjobs run = new RunCasjobs(sQ.ToString(),token, "ImgCutout:SDSS", KeyWords.contentDataset, datarelease);
            DataSet ds = run.runQuery();
            try
            {
                //SqlCommand cmd = new SqlCommand(sQ.ToString(), this.SqlConn);
                double oRa, oDec;
                byte oFlag;
                //reader = cmd.ExecuteReader();					// invoke fGetObjectsEq()
                using (DataTableReader reader = ds.Tables[0].CreateDataReader())
                {
                    while (reader.Read())
                    {
                        oRa = Convert.ToDouble(reader[0]);		// get ra
                        oDec = Convert.ToDouble(reader[1]);		// get dec
                        oFlag = Convert.ToByte(reader[2]);		// get flag												
                        if (drawSpecObjs && (oFlag & SdssConstants.sflag) > 0)
                            canvas.drawSpecObj(oRa, oDec, size);
                        if (drawPhotoObjs && (oFlag & SdssConstants.pflag) > 0)
                            canvas.drawPhotoObj(oRa, oDec, size);
                        //canvas.drawApogeeObj(oRa,oDec,size);
                        if (drawTargetObjs && (oFlag & SdssConstants.tflag) > 0)
                            canvas.drawTargetObj(oRa, oDec, size);
                    }
                }
                
            }
            catch (Exception e)
            {
                showException("getObjects() [Photo|Spec|Target]", sQ.ToString(), e);
            }
            finally { 
                //try { if (reader != null) reader.Close(); } catch (Exception e) { } 
            }
        }


        /// <summary>
        /// APOGEE objects for 2mass , this needs to be updated once we get final twomass data        
        /// </summary>
        internal void getApogeeObjects()        
        {  
            StringBuilder sq1 = new StringBuilder();
            try
            {
                //canvas.drawLabel("Here Are Apgee:"+radius);
                sq1.AppendFormat("select ra,dec from dbo.fGetNearbyApogeeStarEq ({0},{1},{2})", ra,dec,radius);                
                //SqlCommand cmd = new SqlCommand(sq1.ToString(), SqlConn);
                double oRa, oDec;
                //SqlDataReader sReader = cmd.ExecuteReader();					// invoke fGetObjectsEq()               

                RunCasjobs run = new RunCasjobs(sq1.ToString(), token, "ImgCutout:SDSS", KeyWords.contentDataset, datarelease);
                DataSet ds = run.runQuery();
                using (DataTableReader sReader = ds.Tables[0].CreateDataReader())
                {
                    while (sReader.Read())
                    {
                        oRa = Convert.ToDouble(sReader[0]);		// get ra
                        oDec = Convert.ToDouble(sReader[1]);		// get dec
                        canvas.drawApogeeObj(oRa, oDec, size);
                    }
                    if (sReader != null) sReader.Close();
                }
            }
            catch (Exception e)
            {
                showException("getApogee Objects", sq1.ToString(), e);
            }
        }

       

        /// <summary>
        /// getOutlines. Display the bounding boxes and outlines of photoObj.
        /// </summary>
        internal void getOutlines(bool drawBoundingBox, bool drawOutline,Hashtable cTable)
        {
            if (!SdssConstants.isSdss) {
                canvas.drawWarning(" No Outlines/Bounding Boxes available for twomass !");
                return;
            }
            //@Deoyani A workaround for the outlines exception problem
            if (radius > 64)
            {
                canvas.drawWarning(" No Outlines for this zoom level/scale !");
                return;
            };

            StringBuilder sQ = new StringBuilder("SELECT \n");
            sQ.Append("	(q.objid & 0xFFFFFFFFFFFF0000) as fieldid,\n");
            sQ.Append("	m.rmin, m.rmax, m.cmin, m.cmax, m.span\n from ");
            sQ.Append(SdssConstants.OutlineTable);
            sQ.Append(" m \n JOIN (select min(f.objid) as objid \n");
            sQ.AppendFormat(" from dbo.fGetObjectsEq({0}, {1}, {2}, {3}, {4}) f JOIN \n		",
                SdssConstants.pflag, ra, dec, radius, zoom);
            sQ.Append(SdssConstants.OutlineTable);
            sQ.Append(" o \nwith (nolock)\n");
            sQ.Append(" ON f.objid=o.objid  group by rmin,rmax,cmin,cmax ) q\n");
            sQ.Append(" ON m.objid=q.objid");

            //SqlCommand cmd = new SqlCommand(sQ.ToString(), SqlConn);
            try
            {
                String fieldid;
                StringBuilder span;
                double rmin, rmax, cmin, cmax;
                Coord fc;
                //SqlDataReader reader = cmd.ExecuteReader();		// invoke fGetObjectsEq
                RunCasjobs run = new RunCasjobs(sQ.ToString(), token, "ImgCutout:SDSS", KeyWords.contentDataset, datarelease);
                DataSet ds = run.runQuery();
                using (DataTableReader reader = ds.Tables[0].CreateDataReader())
                {
                    while (reader.Read())							// read the next record in the dataset
                    {
                        fieldid = Convert.ToString(reader[0]);
                        rmin = Convert.ToDouble(reader[1]) * SdssConstants.OutlinePix;
                        rmax = Convert.ToDouble(reader[2]) * SdssConstants.OutlinePix;
                        cmin = Convert.ToDouble(reader[3]) * SdssConstants.OutlinePix;
                        cmax = Convert.ToDouble(reader[4]) * SdssConstants.OutlinePix;

                        span = new StringBuilder("\"" + Convert.ToString(reader[5]) + "\"");
                        fc = (Coord)cTable[fieldid];
                        if (drawBoundingBox)
                        {
                            canvas.drawBoundingBox(fc, cmin, cmax, rmin, rmax);
                        }
                        if (drawOutline)
                        {
                            canvas.drawOutline(fc, span);
                        }
                    }
                    if (reader != null) reader.Close();						// we have to close the reader.
                }
            }
            catch (Exception e)
            {
                showException("getOutlines()", sQ.ToString(), e);
            }
        }

        /// <summary>
        /// getMasks. Display the typical Masks intersecting with the canvas.
        /// </summary>
        internal void getMasks()
        {
            StringBuilder sQ = new StringBuilder(" select m.area \n");
            sQ.AppendFormat(
                " from dbo.fGetObjectsEq({0},{1},{2},{3},{4}) f JOIN Mask m with (nolock)\n",
                SdssConstants.mflag, ra, dec, fradius, zoom);
            sQ.Append(" ON f.objid = m.maskid \n WHERE ( \n");
            sQ.Append(" (m.type = 2) \n");
            sQ.Append(" or (m.type in (0,1,3) and  m.filter = 2) \n");
            sQ.Append(" or (m.type = 4 and m.filter = 2 and m.seeing > 1.7 ) )");
            //SqlCommand cmd = new SqlCommand(sQ.ToString(), SqlConn);
            try
            {
               // SqlDataReader reader = cmd.ExecuteReader();					// invoke fGetObjectsEq
                RunCasjobs run = new RunCasjobs(sQ.ToString(),token, "ImgCutout:SDSS", KeyWords.contentDataset, datarelease);
                DataSet ds = run.runQuery();
                using (DataTableReader reader = ds.Tables[0].CreateDataReader())
                {
                    while (reader.Read())							// read the next record in the dataset
                    {
                        StringBuilder area = new StringBuilder(Convert.ToString(reader[0]));
                        canvas.drawMask(area);
                    }
                    if (reader != null) reader.Close();				// close the reader.
                }
            }
            catch (Exception e)
            {
                showException("getMasks()", sQ.ToString(), e);
            }

        }

        /// <summary>
        /// getLabel. Display the standard label on the image.
        /// </summary>
       internal void getLabel(string sDataRelease, double scale, double imageScale)
        {
            string zoomRatio = (zoom >= 1) ? ("1:" + (int)(Math.Pow(4, zoom))) : ((int)(Math.Pow(imageScale, 2.0) + .5) + ":1");
            string theLabel = String.Format("SDSS {0} \nra: {1:N3} dec: {2:N3}\nscale: {3:N4} arcsec/pix\n"
                + "image zoom: " + zoomRatio, sDataRelease, ra, dec, scale);
            canvas.drawLabel(theLabel);
        }

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


        /// <summary>
        /// getPlates. Display the outlines of the plates intersecting the canvas.
        /// </summary>
       internal void getPlates()
        {
            StringBuilder sQ = new StringBuilder(" select ra, dec ");
            sQ.AppendFormat(" from dbo.fGetObjectsEq({0},{1},{2},{3},{4}) ",
            SdssConstants.plateflag, ra, dec, 2 * radius + SdssConstants.plateRadiusArcMin, zoom);
            //SqlCommand cmd = new SqlCommand(sQ.ToString(), SqlConn);
            try
            {
                double oRa, oDec, oRadius;						//					
                //SqlDataReader reader = cmd.ExecuteReader();					// invoke fGetObjectsEq
                RunCasjobs run = new RunCasjobs(sQ.ToString(), token, "ImgCutout:SDSS", KeyWords.contentDataset, datarelease);
                DataSet ds = run.runQuery();
                using (DataTableReader reader = ds.Tables[0].CreateDataReader())
                {
                    while (reader.Read())							// read the next record in the dataset
                    {
                        oRa = Convert.ToDouble(reader[0]);		// get ra
                        oDec = Convert.ToDouble(reader[1]);		// get dec
                        oRadius = SdssConstants.plateRadiusArcMin;	// plate radius																											
                        canvas.drawPlate(oRa, oDec, oRadius);
                    }
                    if (reader != null) reader.Close();				// close the reader.
                }
            }
            catch (Exception e)
            {
                showException("getPlates()", sQ.ToString(), e);
            }
        }

        //////This is alex's code for all regions update later at the end of all changes ****
        ////%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
        ////%%%%%%%%%%%%%  geometries %%%%%%%%%%%%%%
        ////%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%

        ///// <summary>
        ///// getOneRegion. Display the outlines of the region specified. 
        ///// fill=1 means filled.
        ///// </summary>
        //private void getOneRegion(HTMRegion h, int isList)
        //{
        //    if (debug && false)
        //    {
        //        canvas.addDebugMessage("getOneRegion " + h.rid.ToString() + ", " + isList.ToString() + "\n");
        //    }
        //    //-------------------------------------------------
        //    // get the arcs of the Region
        //    //-------------------------------------------------
        //    StringBuilder sQ = new StringBuilder("");
        //    sQ.Append("select convexid, patchid, draw, ");
        //    sQ.AppendFormat(" ra1, dec1, ra2, dec2, x, y, z, c, arcid");
        //    sQ.Append(" from Region r cross apply dbo.fSphGetArcs(r.regionBinary) a");
        //    sQ.AppendFormat(" where r.regionid={0} ", h.rid);
        //    SqlCommand cmd = new SqlCommand(sQ.ToString(), SqlConn);
        //    ArrayList arcs = new ArrayList();
        //    HTMArc a;

        //    try
        //    {
        //        h.fill = isList;
        //        reader = cmd.ExecuteReader();
        //        while (reader.Read())
        //        {
        //            a = new HTMArc(
        //                Convert.ToInt64(reader[0]),
        //                Convert.ToInt32(reader[1]),
        //                Convert.ToInt32(reader[2]),
        //                Convert.ToDouble(reader[3]),
        //                Convert.ToDouble(reader[4]),
        //                Convert.ToDouble(reader[5]),
        //                Convert.ToDouble(reader[6]),
        //                Convert.ToDouble(reader[7]),
        //                Convert.ToDouble(reader[8]),
        //                Convert.ToDouble(reader[9]),
        //                Convert.ToDouble(reader[10]),
        //                Convert.ToInt64(reader[11])
        //            );
        //            arcs.Add((HTMArc)a);
        //        }
        //        if (reader != null) reader.Close();

        //        //------------------------------------
        //        // copy the ArrayList into the array
        //        //------------------------------------
        //        h.arc = (HTMArc[])arcs.ToArray(typeof(HTMArc));
        //        h.count = h.arc.Length;

        //        //-----------------
        //        // draw the region
        //        //-----------------
        //        canvas.drawRegion(h, fillRegion);
        //    }
        //    catch { canvas.displayMessage("Exceptions in getOneRegion " + h.rid.ToString() + "\n"); }
        //}


        ///// <summary>
        ///// getRegions. Display the outlines of the various regions intersecting the canvas.
        ///// </summary>
        //private void getRegions(int isList)
        //{
        //    StringBuilder sQ = new StringBuilder("");
        //    if (isList == 0)
        //    {
        //        //------------------------------------------------------
        //        // show outlines of regions with type in regionTypes
        //        //------------------------------------------------------
        //        sQ.Append("select regionid, isMask, type from \n");
        //        sQ.Append("dbo.fRegionsIntersectingString(" + regionTypes + ",\n");
        //        sQ.Append("'" + viewPort + "')\n");

        //    }
        //    else // filled regions
        //    {
        //        // show a given Region with a regionId in regionList
        //        sQ.Append("select r.regionid, r.isMask, r.type ");
        //        sQ.Append("from Region r ");
        //        sQ.AppendFormat("where r.regionid in ({0})", regionList);
        //    }
        //    SqlCommand cmd = new SqlCommand(sQ.ToString(), SqlConn);
        //    ArrayList regions = new ArrayList();
        //    HTMRegion r;

        //    try
        //    {
        //        reader = cmd.ExecuteReader();				// invoke fRegionsContainPoint(x,y,z)				
        //        while (reader.Read())						// read the next record in the dataset
        //        {
        //            r = new HTMRegion(
        //                Convert.ToInt64(reader[0]),			// get regionid
        //                Convert.ToInt16(reader[1]),			// get ismask bit
        //                Convert.ToString(reader[2])			// get type
        //            );
        //            regions.Add((HTMRegion)r);
        //        }
        //        if (reader != null) reader.Close();			// close the reader.
        //        HTMRegion[] hr = (HTMRegion[])regions.ToArray(typeof(HTMRegion));

        //        //canvas.addDebugMessage("getRegions "+hr.Length.ToString()+"\n");


        //        for (int i = 0; i < hr.Length; i++)
        //            getOneRegion(hr[i], isList);
        //    }
        //    catch
        //    {
        //        canvas.drawDebugMessage();
        //        canvas.displayMessage(sQ.ToString());
        //        canvas.displayMessage("Exceptions in getRegions at " + ra.ToString() + " " + dec.ToString() + "\n");
        //    }
        //}
        /////%%%%%%%%%%%%%%%%%%%%%%%%%%%%%% Deoyani Trying this one
        /////// This is just a trial for the objects to be drawn directly on images instead of sending whole image back
        ///// <summary>
        ///// getObjects(). Mark Photo|Spec|Target objects according to the drawing option string.
        ///// </summary>
        //private string getObjects64()
        //{

        //    string jsonObjects = "{ \"Objects\":{";
        //    byte flag = 0;
        //    if (drawPhotoObjs) flag |= SdssConstants.pflag;
        //    if (drawSpecObjs) flag |= SdssConstants.sflag;
        //    if (drawTargetObjs) flag |= SdssConstants.tflag;
        //    StringBuilder sQ = new StringBuilder(" select *");
        //    sQ.AppendFormat(" from dbo.fGetObjectsEq({0},{1},{2},{3},{4})",
        //        flag, ra, dec, radius, zoom);
        //    try
        //    {
        //        SqlCommand cmd = new SqlCommand(sQ.ToString(), SqlConn);
        //        double oRa, oDec;
        //        byte oFlag;
        //        int cnt = 0;
        //        reader = cmd.ExecuteReader();					// invoke fGetObjectsEq()
        //        while (reader.Read())
        //        {
        //            oRa = Convert.ToDouble(reader[0]);		// get ra
        //            oDec = Convert.ToDouble(reader[1]);		// get dec
        //            oFlag = Convert.ToByte(reader[2]);		    // get flag												
        //            if (drawPhotoObjs && (oFlag & SdssConstants.pflag) > 0)
        //                jsonObjects += " \"" + cnt + "\": \"" + oRa + "," + oDec + "," + size + "\",";

        //            cnt++;
        //        }

        //        if (reader != null) reader.Close();
        //        jsonObjects += "\"\":\"\" }}";

        //        return jsonObjects;

        //    }
        //    catch (Exception e)
        //    {
        //        //showException("getObjects() [Photo|Spec|Target]", sQ.ToString(), e);
        //        return "Error";
        //    }
        //}
    }
}