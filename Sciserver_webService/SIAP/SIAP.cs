using System;
using System.Text;
using System.Xml;
using net.ivoa.VOTable;

namespace Sciserver_webService.sdssSIAP
{
    public class SIAP
    {

        public static string[] formats = { "metadata", "all", "image/fits", "graphic", "image/jpeg" };
        public static string[] bands = { "u", "g", "r", "i", "z", "*" };

        public SiapTable getSiapInfo(string POS, string SIZE, string FORMAT, string bandpass)
        {
            double ra, dec;
            double size1;
            double? size2;

            bool finish = false;
            SiapTable v = new SiapTable();
            string inf = FORMAT.ToLower();
            int i;
            for (i = 0; i < formats.Length; i++)
            {
                if (inf.CompareTo(formats[i]) == 0) break;
            }
            switch (i)
            {
                case 0:
                    v.getMetadata();
                    AddParams(v, POS, SIZE, FORMAT, bandpass);
                    AddPossibleParams(v);
                    finish = true;
                    break;
                case 1:
                    //	bandpass= "*"; // FORMAT = ALL doesn't mean all bands
                    break;
                case 2:// intentional fall through
                case 3:// intentional fall through 					
                case 4:

                    break;

                default:
                    StringBuilder f = new StringBuilder("");
                    f.AppendFormat("Wrong FORMAT={0}. Please select one from the following list:", FORMAT);
                    f.Append(String.Join(",", formats));
                    v = new SiapTable(f.ToString());
                    finish = true;
                    break;
            }


            if (!finish) try
                {
                    string[] poss = POS.Split(new char[] { ',' });
                    if (poss.Length < 2)
                    {
                        //SiapTable eV = new SiapTable("Wrong Input Parameters: Please specify RA,DEC in POS parameter");
                        //return eV;
                        throw new ArgumentException("Wrong Input Parameters: Please specify RA,DEC in POS parameter"); 
                    }
                    ra = Convert.ToDouble(poss[0]);
                    dec = Convert.ToDouble(poss[1]);

                    string[] sizs = SIZE.Split(new char[] { ',' });
                    if (sizs.Length > 2)
                    {
                        //SiapTable eV = new SiapTable("Wrong Input Parameters: Please specify correct SIZE");
                        //return eV;

                        throw new ArgumentException("Wrong Input Parameters: Please specify correct SIZE");
                    }
                    size1 = double.Parse(sizs[0]);
                    size2 = null;
                    if (sizs.Length > 1)
                        size2 = double.Parse(sizs[1]);

                    string message = valid_input(ra, dec, size1, size2, bandpass);
                    if (message.CompareTo("") != 0) //!valid_input(ra, dec, SIZE, bandpass))
                    {
                        //SiapTable eV = new SiapTable("Wrong Input Parameters:" + message);
                        //return eV;
                        throw new ArgumentException("Wrong Input Parameters:" + message);
                    }

                    v.populate(ra, dec, size1, size2, FORMAT, bandpass);
                    AddParams(v, POS, SIZE, FORMAT, bandpass);

                }
                catch (Exception e)
                {
                    //v = new SiapTable(e.ToString()); // Message for regular SOAP exceptions
                    throw new Exception(e.ToString());

                }
            return v;
        }


        public static void AddParams(SiapTable vot, string POS, string SIZE, string FORMAT, string band)
        {
            int c = 0;
            PARAM posp = null;
            PARAM sizep = null;
            PARAM formp = null;
            PARAM bandp = null;
            if (POS != null && POS.Length > 0)
            {
                posp = new PARAM();
                posp.name = "INPUT:POS";
                posp.value = POS;
                posp.datatype = dataType.@char;
                posp.arraysize = "*";
                c++;
            }

            if (!string.IsNullOrEmpty(SIZE))
            {
                sizep = new PARAM();
                sizep.name = "INPUT:SIZE";
                sizep.value = SIZE;
                //sizep.datatype = dataType.@double;
                sizep.datatype = dataType.@char;
                sizep.arraysize = "*";
                c++;
            }
            if (FORMAT != null && FORMAT.Length > 0)
            {
                formp = new PARAM();
                formp.name = "INPUT:FORMAT";
                formp.value = FORMAT;
                formp.datatype = dataType.@char;
                formp.arraysize = "*";
                c++;
            }
            if (band != null && band.Length > 0)
            {
                bandp = new PARAM();
                bandp.name = "INPUT:BANDPASS";
                bandp.value = band;
                bandp.datatype = dataType.@char;
                bandp.arraysize = "*";
                c++;
            }

            if (vot.RESOURCE[0] == null)
            {
                vot.RESOURCE = new RESOURCE[1];
                vot.RESOURCE[0] = new RESOURCE();
            }

            int ind = 0;
            if (vot.RESOURCE[0].Items == null)
            {
                vot.RESOURCE[0].Items = new object[c];
            }
            else// gpt to add more  
            {
                object[] oldits = vot.RESOURCE[0].Items;
                int count = oldits.Length;
                vot.RESOURCE[0].Items = new object[count + c];
                for (ind = 0; ind < count; ind++)
                {
                    vot.RESOURCE[0].Items[ind] = oldits[ind];
                }
            }
            // finally add the params
            if (null != posp) vot.RESOURCE[0].Items[ind++] = posp;
            if (null != sizep) vot.RESOURCE[0].Items[ind++] = sizep;
            if (null != formp) vot.RESOURCE[0].Items[ind++] = formp;
            if (null != bandp) vot.RESOURCE[0].Items[ind++] = bandp;
        }

        public static void AddPossibleParams(SiapTable vot)
        {
            int c = 4;
            int ind = 0;
            if (vot.RESOURCE[0].Items == null)
            {
                vot.RESOURCE[0].Items = new object[c];
            }
            else// gpt to add more  
            {
                object[] oldits = vot.RESOURCE[0].Items;
                int count = oldits.Length;
                vot.RESOURCE[0].Items = new object[count + c];
                for (ind = 0; ind < count; ind++)
                {
                    vot.RESOURCE[0].Items[ind] = oldits[ind];
                }
            }

            addParam(vot, dataType.@char, "INPUT:POS", "Search position in decimal degrees in the form \"ra,dec\"", ind++);
            addParam(vot, dataType.@double, "INPUT:SIZE", "SIZE in degrees (double). Example: 0.1", ind++);
            addParam(vot, dataType.@char, "INPUT:FORMAT", "Type of image(s) to return one of " + String.Join(",", formats), ind++);
            addValues(vot, formats, ind - 1);
            addParam(vot, dataType.@char, "INPUT:BANDPASS",
            "Sloan filter you are interested in. Any combination of 'u', 'g', 'r', 'i','z' or '*' to get them all  "
                    , ind++);
            addValues(vot, bands, ind - 1);            
        }

        // assumes parm exists - adds values to it
        public static void addValues(SiapTable vot, string[] values, int ind)
        {
            PARAM p = (PARAM)vot.RESOURCE[0].Items[ind];
            p.VALUES = new VALUES();
            p.VALUES.OPTION = new OPTION[values.Length];
            for (int v = 0; v < values.Length; v++)
            {
                p.VALUES.OPTION[v] = new OPTION();
                p.VALUES.OPTION[v].value = values[v];
            }
        }

        public static void addParam(SiapTable vot, dataType dt, string name, string desc, int ind)
        {
            PARAM p = new PARAM();
            p.name = name;
            p.DESCRIPTION = new anyTEXT();
            p.datatype = dt;
            if (dt == dataType.@char) p.arraysize = "*";

            p.DESCRIPTION.Any = new System.Xml.XmlNode[1];
            XmlDocument doc = new XmlDocument();

            p.DESCRIPTION.Any[0] = doc.CreateTextNode("DESCRIPTION");

            p.DESCRIPTION.Any[0].InnerText = desc;
            vot.RESOURCE[0].Items[ind] = p;
        }
        private string valid_input(double ra, double dec, double size1, double? size2, string bandpass)
        {
            // if ((sr < 0.0) || sr > MAX)) return false;
            StringBuilder msg = new StringBuilder("");
            bool wrongband = false;

            if ((ra < 0.0) || (ra > 360.0) || (dec < -90.0) || (dec > 90.0)) msg.AppendFormat(" POS={0},{1} ", ra, dec);
            if (!size2.HasValue)
            {
                if ((size1 < 0.0)) msg.AppendFormat(" SIZE={0} ", size1);
            }
            else
            {
                if ((size1 < 0) || (size2 < 0)) msg.AppendFormat(" SIZE={0},{1}", size1, size2.Value);
            }


            bandpass = bandpass.ToLower();
            foreach (char b in bandpass)
            {
                if ("ugriz*".IndexOf(b) == -1)
                {
                    wrongband = true;
                    break;
                }
            }
            if (wrongband) msg.AppendFormat(" BANDPASS={0} ", bandpass);
            return msg.ToString();
        }
    }
}