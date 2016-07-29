using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Web;
using net.ivoa.VOTable;
using Newtonsoft.Json;
using Sciserver_webService.Common;
using Sciserver_webService.SDSSFields;


namespace Sciserver_webService.sdssSIAP
{
    /// <summary>
    /// Extension of VOTable to do SIAP so we can produce the XML.
    /// </summary>
    
    [System.Xml.Serialization.XmlRootAttribute("VOTABLE", Namespace = "http://vizier.u-strasbg.fr/xml/VOTable-1.1.xsd", IsNullable = false)]
    public class SiapTable : VOTABLE
    {

        
        private string SDSSgetImage = ConfigurationSettings.AppSettings["UrlSdssGetJpeg"];
        //private string ppd = "";

        protected bool dobandpass = false;
        static public int FIELDCOUNT = 16;
        private string UrlSdssFields = "";
        private double pixPerDeg = 9088.0;
        private double scale = 3600.0 / 9088.0; // 0.396127, SDSS default scale in arcseconds/pixel
        private string datarelease = "";
        private string casjobstaskname = "";
        

        public SiapTable()
        {
            ///******
            ///This part is added to get any data release working with thsi DR1 to DRxx
            ///******
            //datarelease = HttpContext.Current.Request.RequestContext.RouteData.Values["anything"] as string;
            datarelease = ConfigurationManager.AppSettings["DataRelease"];
            //this.datarelease = datarelease;
            this.casjobstaskname = "SDSSFields For SIAP";

            SDSSgetImage = SDSSgetImage.Replace("*DataRelease*", datarelease);
            string[] temp = HttpContext.Current.Request.Url.AbsoluteUri.ToLower().Split(new string[]{"siap"}, StringSplitOptions.None);
            UrlSdssFields = temp[0]+"SDSSFields";
            //****
            
            ////old code
            //if (null != ppd)
            //{
            //    this.pixPerDeg = Convert.ToInt32(ppd);
            //}

            this.version = net.ivoa.VOTable.VOTABLEVersion.Item11;
            this.RESOURCE = new RESOURCE[1];
            this.RESOURCE[0] = new RESOURCE();
            this.RESOURCE[0].type = RESOURCEType.results;

            INFO info = new INFO();
            info.name = "QUERY_STATUS";
            info.value = "OK";

            this.INFO = new INFO[1];
            this.INFO[0] = info;

            this.RESOURCE[0].TABLE = new TABLE[1];
            this.RESOURCE[0].TABLE[0] = new TABLE();
        }

        public SiapTable(string error)
        {
            this.reportError(error);
        }

        public void reportError(string error)
        {
            this.version = net.ivoa.VOTable.VOTABLEVersion.Item11;
            INFO info = new INFO();

            info.name = "QUERY_STATUS";
            info.value = "ERROR";
            info.Text = new string[1];
            info.Text[0] = error;

            this.INFO = new INFO[3];
            this.INFO[0] = info;
            addUrlInfo(1);
        }

        /// <summary>
        /// Inserts all the Field ucds for the table
        /// </summary>
        /// <param name="t"></param>
        public void setupFields(TABLE t)
        {
            int n = FIELDCOUNT;

            t.Items = new object[n];
            n = 0;

            FIELD f = new FIELD();
            t.Items[n] = f;
            f.datatype = dataType.@char;
            f.ID = "Title";
            f.ucd = "VOX:Image_Title";
            f.arraysize = "*";
            n++;

            f = new FIELD();
            t.Items[n] = f;
            f.datatype = dataType.@int;
            f.ID = "width";
            f.ucd = "VOX:Image_Pix_Width";
            n++;

            f = new FIELD();
            t.Items[n] = f;
            f.datatype = dataType.@int;
            f.ID = "height";
            f.ucd = "VOX:Image_Pix_Height";
            n++;

            f = new FIELD();
            t.Items[n] = f;
            f.datatype = dataType.@double;
            f.ID = "size";
            f.ucd = "VOX:Image_Size";
            n++;

            f = new FIELD();
            t.Items[n] = f;
            f.datatype = dataType.@double;
            f.ID = "RA";
            f.ucd = "POS_EQ_RA_MAIN";
            n++;

            f = new FIELD();
            t.Items[n] = f;
            f.datatype = dataType.@double;
            f.ID = "DEC";
            f.ucd = "POS_EQ_DEC_MAIN";
            n++;

            f = new FIELD();
            t.Items[n] = f;
            f.datatype = dataType.@double;
            f.ID = "scale";
            f.ucd = "VOX:Image_Scale";
            f.arraysize = "*";
            n++;

            f = new FIELD();
            t.Items[n] = f;
            f.datatype = dataType.@char;
            f.ID = "format";
            f.ucd = "VOX:Image_Format";
            f.arraysize = "*";
            n++;

            f = new FIELD();
            t.Items[n] = f;
            f.datatype = dataType.@char;
            f.ID = "url";
            f.ucd = "VOX:Image_AccessReference";
            f.arraysize = "*";
            n++;

            f = new FIELD();
            t.Items[n] = f;
            f.datatype = dataType.@double;
            f.ID = "equinox";
            f.ucd = "VOX:Image_Equinox";
            n++;

            f = new FIELD();
            t.Items[n] = f;
            f.datatype = dataType.@int;
            f.ID = "naxes";
            f.ucd = "VOX:Image_Naxes";
            n++;

            f = new FIELD();
            t.Items[n] = f;
            f.datatype = dataType.@int;
            f.ID = "naxis";
            f.ucd = "VOX:Image_Naxis";
            f.arraysize = "*";
            n++;

            f = new FIELD();
            t.Items[n] = f;
            f.datatype = dataType.@char;
            f.ID = "crtype";
            f.ucd = "VOX:WCS_CoordProjection";
            f.arraysize = "*";
            n++;

            f = new FIELD();
            t.Items[n] = f;
            f.datatype = dataType.@double;
            f.ID = "crpix";
            f.ucd = "VOX:WCS_CoordRefPixel";
            f.arraysize = "*";
            n++;

            f = new FIELD();
            t.Items[n] = f;
            f.datatype = dataType.@double;
            f.ID = "crval";
            f.ucd = "VOX:WCS_CoordRefValue";
            f.arraysize = "*";
            n++;

            f = new FIELD();
            t.Items[n] = f;
            f.datatype = dataType.@double;
            f.ID = "cdval";
            f.ucd = "VOX:WCS_CDMatrix";
            f.arraysize = "*";
            n++;

        }

        public void getJpeg(double ra, double dec, double SIZE)
        {

            rescale(SIZE);
            TABLE x = this.RESOURCE[0].TABLE[0];

            setupFields(x);
            x.DATA = new DATA();
            x.DATA.Item = new TABLEDATA();

            string[] values = new string[FIELDCOUNT];

            // just checking whether or not inside the footprint
            string[][] vals = setupValuesFits(ra, dec, 0.01, null, "u");
            // now we know if there is any real data here 
            if (vals.Length == 0)
            {
                StringBuilder msg = new StringBuilder();
                msg.AppendFormat("POS={0},{1} outside SDSS footprint", ra, dec);
                // put out error message in INFO tag
                this.reportError(msg.ToString());
            }
            else
            {
                int jpg = 0;
                setupValuesJpeg(values, ra, dec, SIZE);
                jpg++;

                TABLEDATA tdata = new TABLEDATA();
                x.DATA.Item = tdata;
                tdata.TR = new TR[jpg];// Jpeg
                setupData(x.DATA, 0, values);// for Jpeg			
            }
        }

        /// <summary>
        /// Rescale the jpeg image when according to SIZE, the image would be bigger than 2048 or smaller than 64 pixels
        /// </summary>
        /// <param name="SIZE">Size in degrees</param>		
        /// <returns></returns>
        private void rescale(double SIZE)
        {
            if (SIZE > 2048.0 * this.scale / 3600.0) // 2048 is the biggest size for the imgCutout
            {
                this.scale = SIZE * 3600.0 / 512.0;
            }

            if (SIZE < 64.0 * this.scale / 3600.0) //64 is the smallest size for the imgCutout
            {
                this.scale = SIZE * 3600.0 / 64.0;
            }
        }



        /// <summary>
        /// This sets up metadata for the votable
        /// </summary>
        public void getMetadata()
        {
            this.INFO[0].value = "METADATA";
            TABLE x = this.RESOURCE[0].TABLE[0];
            // add the fields with UCDs etc 
            setupFields(x);
        }

        // assumes space already allocate in INFO of this
        public int addUrlInfo(int ind)
        {
            this.INFO[ind] = new INFO();
            this.INFO[ind].name = "FieldsUrl";
            this.INFO[ind].Text = new string[1];
            this.INFO[ind].Text[0] = UrlSdssFields;
            ind++;
            this.INFO[ind] = new INFO();
            this.INFO[ind].name = "CutOutUrl";
            this.INFO[ind].Text = new string[1];
            this.INFO[ind].Text[0] = SDSSgetImage;
            ind++;
            return ind;
        }
        /// <summary>
        /// This sets up the resource and table data for the votable
        /// </summary>
        /// <param name="ra"></param>
        /// <param name="dec"></param>
        /// <param name="size"></param>
        public void populate(double ra, double dec, double size1, double? size2, string format, string bandpass)
        {
            if (size1 == 0) size1 = 512.0 * this.scale / 3600.0; // Set up 512 pixels ~ 0.05 degree
            // as default when size = 0
            try
            {
                this.dobandpass = bandpass != null && !bandpass.Equals("*") && (bandpass.Length > 0);
                this.RESOURCE = new RESOURCE[1];
                this.RESOURCE[0] = new RESOURCE();
                this.RESOURCE[0].type = RESOURCEType.results;
                INFO info = new INFO();

                int ind = 0;
                this.INFO = new INFO[3];
                this.INFO[ind++] = info;
                info.name = "QUERY_STATUS";
                info.value = "OK";
                ind = addUrlInfo(ind);

                TABLEDATA theData = new TABLEDATA();

                this.RESOURCE[0].TABLE = new TABLE[1];
                TABLE x = new TABLE();
                this.RESOURCE[0].TABLE[0] = x; ;

                x.DATA = new DATA();
                x.DATA.Item = theData;

                // add the fields with UCDs etc 
                setupFields(this.RESOURCE[0].TABLE[0]);

                string[] values = new string[FIELDCOUNT];

                // go off and get the fits entries
                string[][] vals = setupValuesFits(ra, dec, size1, size2, bandpass);
                // now we know if there is any real data here 
                if (vals.Length == 0)
                {
                    // put out error message in INFO tag
                    StringBuilder msg = new StringBuilder();
                    msg.AppendFormat("POS={0},{1} outside SDSS footprint", ra, dec);
                    this.reportError(msg.ToString());
                }
                else
                {
                    // create the Jpeg Entry
                    int jpg = 0;
                    string form = format.ToUpper();
                    bool dojpg = (form.IndexOf("JP") >= 0 || form.IndexOf("ALL") >= 0 || form.IndexOf("GRAPHIC") >= 0) && !dobandpass;
                    bool dofits = (form.IndexOf("FITS") >= 0 || form.IndexOf("ALL") >= 0);

                    double size = size1;
                    if (size2.HasValue)
                        size = Math.Sqrt(size1 * size1 + size2.Value * size2.Value);

                    if (dojpg)
                    {
                        rescale(size);
                        setupValuesJpeg(values, ra, dec, size);
                        jpg++;
                    }
                    int nr = jpg;
                    if (dofits) nr += vals.Length;

                    theData.TR = new TR[nr];// all fits (if required) + Jpeg(if its there)

                    if (dojpg)
                    {
                        setupData(x.DATA, 0, values);// for Jpeg
                    }
                    // add all fits info
                    if (dofits)
                    {
                        for (int s = 0; s < vals.Length; s++)
                        {
                            setupData(x.DATA, s + jpg, vals[s]);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                this.reportError(e.ToString());
            }
        }

        static public double rad(double deg)
        {
            return deg / 180 * Math.PI;
        }

       

        /// <summary>
        ///  need to look at another service to get this data
        ///  then we just put it in the format we want here
        /// </summary>
        /// <param name="ra"> center ra</param>
        /// <param name="dec"> center dec </param>
        /// <param name="size"> width in degrees</param>
        /// <returns></returns>

        protected  string[][] setupValuesFits(double ra, double dec, double size1, double? size2, string bandpass)
        {
            runSDSSFields(ra, dec, (size1 * 60.0 * 0.5 + 8.4F));
            

            ArrayList list = new ArrayList();
            int count = 0;

            for (int f = 0; f < fieldArray.Length; f++)
            {

                IEnumerator enumer = fieldArray[f].passband.GetEnumerator();
                while (enumer.MoveNext())
                {
                    string[] values = new string[FIELDCOUNT];
                    int ind = 0;
                    Band p = (Band)enumer.Current;

                    if (!this.dobandpass ||
                        (bandpass.ToLower().IndexOf(p.filter.ToLower()) >= 0)
                        )
                    {

                        values[ind++] = "Sloan Digital Sky Survey - Filter " + p.filter;///title
                        values[ind++] = "" + p.wcs.NAXIS1;//width
                        values[ind++] = "" + p.wcs.NAXIS2;//height
                        values[ind++] = "" + (p.wcs.NAXIS1 * p.wcs.NAXIS2);// size of data  Image";                        
                        values[ind++] = "" + p.wcs.CRVAL1;
                        values[ind++] = "" + p.wcs.CRVAL2;
                        values[ind++] = "" + 1 / p.pixperdeg; // image scale in degrees per pixel as the standard requires
                        values[ind++] = "image/fits";
                        values[ind++] = p.url;// url
                        values[ind++] = "J2000";// epoch
                        values[ind++] = "2";// naxes
                        values[ind++] = p.wcs.NAXIS1 + "," + p.wcs.NAXIS2; // naxis
                        values[ind++] = p.wcs.CTYPE1 + "," + p.wcs.CTYPE2;// crtype
                        values[ind++] = p.wcs.CRPIX2 + "," + p.wcs.CRPIX1; // crpix
                        values[ind++] = p.wcs.CRVAL1 + "," + p.wcs.CRVAL2;//crval
                        values[ind++] = p.wcs.CD2_1 + "," + p.wcs.CD2_2 + "," + p.wcs.CD1_1 + "," + p.wcs.CD1_2;//cdval

                        count++;
                        list.Add(values);
                    }
                }
            }

            string[][] ret = new string[count][];
            for (int i = 0; i < count; i++)
            {
                ret[i] = (string[])list[i];
            }

            return ret;

        }
        /// <summary>
        /// Create the Jpeg values - these ar not very accurate
        /// </summary>
        /// <param name="values"> the array to put the values in </param>
        /// <param name="ra"> the ra of the center</param>
        /// <param name="dec"> dec of the center </param>
        /// <param name="size">size of the area i.e. width</param>
        protected void setupValuesJpeg(string[] values, double ra, double dec, double size)
        {
            int ind = 0;

            int h = (int)(size * 3600.0 / this.scale);
            int w = h;
            int crp = h / 2;

            values[ind++] = "Sloan Digital Sky Survey";///title
            values[ind++] = "" + w;	//width
            values[ind++] = "" + h;	//height
            values[ind++] = "" + (h * w);// size of data  Image";
            values[ind++] = "" + ra;
            values[ind++] = "" + dec;
            values[ind++] = "" + this.scale / 3600.0;// image scale in degrees per pixel
            values[ind++] = "image/jpeg";
            values[ind++] = SDSSgetImage + "?ra=" + ra + "&dec=" + dec + "&height=" + h + "&width=" + w +
                            "&scale=" + this.scale;

            values[ind++] = "J2000";// epoch
            values[ind++] = "2";// naxes
            values[ind++] = w + "," + h; // naxis
            values[ind++] = "RA--TAN, DEC--TAN";// crtype
            values[ind++] = crp + "," + crp; // crpix
            values[ind++] = ra + "," + dec;//crval
            values[ind++] = "1,1"; //cdval ????


        }
        /// <summary>
        ///  put the values into the TD sections
        /// </summary>
        /// <param name="d"> Data section from the VOTable</param>
        /// <param name="ind">	index of the data row </param>
        /// <param name="values">the values to put in this row </param>

        protected void setupData(DATA d, int ind, string[] values)
        {

            ((TABLEDATA)d.Item).TR[ind] = new TR();
            TR t = ((TABLEDATA)d.Item).TR[ind];

            t.TD = new TD[values.Length];

            for (int i = 0; i < values.Length; i++)
            {
                t.TD[i] = new TD();
                t.TD[i].Text = new string[1];
                t.TD[i].Text[0] = values[i];
            }
        }




        /*
        * This is used to run SDSSFields Added by Deoyani Nandrekar-Heinis
        */
        public Field[] fieldArray;

        public async void runSDSSFields(double ra, double dec, double sr)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(KeyWords.casjobsREST + "contexts/" + datarelease + "/query");
            NewSDSSFields newsdss = new NewSDSSFields(ra, dec, sr);
            String query = newsdss.getFieldsArrayQuery();
            StringContent content = new StringContent(this.getJsonContent(query, casjobstaskname));
            content.Headers.ContentType = new MediaTypeHeaderValue(KeyWords.contentJson);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(KeyWords.contentDataset));
            System.IO.Stream stream = await client.PostAsync(client.BaseAddress, content).Result.Content.ReadAsStreamAsync();
            BinaryFormatter formatter = new BinaryFormatter();
            DataSet ds;
            try
            {
                ds = (DataSet)formatter.Deserialize(stream);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

            fieldArray = newsdss.FieldArray(ds);
        }

        private String getJsonContent(String query, String casjobsTaskName)
        {
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                writer.Formatting = Formatting.Indented;
                writer.WriteStartObject();
                writer.WritePropertyName("Query");
                writer.WriteValue(query);
                writer.WritePropertyName("TaskName");
                writer.WriteValue(casjobsTaskName);
                writer.WriteEndObject();
            }
            return sb.ToString();
        }
    }
}

/* Revision History for the soap web services // @deoyani just updated to make it RESTful, would be a good task to clean and rewrite

	$Log: not supported by cvs2svn $
	Revision 1.5  2004/06/22 20:34:48  womullan
	 added siap.test and swizle functionality for testing
	
	Revision 1.4  2004/05/04 20:46:19  womullan
	FIxed VOTable again - added aspx wrapper
	
	Revision 1.3  2004/05/04 16:17:36  womullan
	 fixed info name
	
	Revision 1.2  2004/05/04 16:03:29  womullan
	 added namspace
	
	Revision 1.1  2004/05/04 00:05:06  womullan
	 Adde votable1.1 - fixed format calls to work. Added params in the metadata call and in return parms passe dinin general
	
	Revision 1.7  2003/12/05 21:30:43  nieto
	SIAP.asmx.cs
	
		Added a new method: getSiap (POS, SIZE, FORMAT)
			FORMAT = ALL => jpeg and fits for all bands
			FORMAT = image/fits => all fits images in all bands
			FORMAT = image/jpeg => only the jpeg
			FORMAR = METADATA => only Metadata
		Added a validation function
	
	Siap.cs
		Implemented Special case SIZE = 0 set up 512 pixels as default  ~ 0.05 degrees
		scale = 0.39.
	
		Fixed radius search patched to include 8.4 arcminutes. The NearbyFramesEQ SQL function
		needs this extra size to do the right search for frames.
	
		Implemented rescaling of jpeg size when images are bigger than 2048 or smaller than 64.
	
		Fixed scale to be degrees/pixel in the VOTable  as requested by the protocol.
	
		Better error messages.
	
		Changed EPOCH parameter … to EQUINOX. Changed J200 for J2000
	
	References: pointing to devel/SdssFields instead of budavari/SdssFields
	
	Revision 1.6  2003/11/11 18:16:30  womullan
	 INFO for ouside footprint
	
	Revision 1.5  2003/07/07 15:03:21  womullan
	longer names better fro registry or at least for DIS
	
	Revision 1.4  2003/04/11 13:43:36  womullan
	 added band pass
	
	Revision 1.3  2003/03/06 20:15:43  budavari
	Put in NameSpace and urls in web.config
	
	Revision 1.2  2003/03/06 17:01:09  womullan
	Addded Revision Property and Reviisions webmethod
	
	Revision 1.1.1.1  2003/03/04 22:26:34  womullan
	import
	
*/