using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Net.Http;
using System.Net;
using System.Configuration;
using System.Text;
using System.Text.RegularExpressions;
using System.Data;
using System.Data.SqlClient;
using System.Xml;

using net.ivoa.VOTable;
using Sciserver_webService.casjobs;

namespace Sciserver_webService.ConeSearch
{
    
    public class StreamingCone
    {
        private double ra;
        private double dec;
        private double sr;

        private static long CJobsWSID = long.Parse(ConfigurationSettings.AppSettings["CJobsWSID"]);
        private static string CJobsPasswd = ConfigurationSettings.AppSettings["CJobsPassWD"];
        private static string CJobsTARGET = ConfigurationSettings.AppSettings["CJobsTARGET"];
        private static string CJobsURL = ConfigurationSettings.AppSettings["CJobsURL"];
        private static string ConeSelect = ConfigurationSettings.AppSettings["ConeSelect"];

        //public StreamingCone(string ra, string dec, string sr) {
        //    this.ValidateInput(ra, dec, sr);
        //}

        public StreamingCone() { }

        public void WriteToStream(Stream outputStream, HttpContent content, TransportContext context)
        {
            StreamWriter writer = new StreamWriter(outputStream);
            try
            {
                for (int i = 0; i < 10; i++)
                {
                    writer.WriteLine("Line " + (i + 1));
                }   
            }
            finally
            {
                writer.Close();
            }
        }

        private bool ValidateInput(string ra, string dec, string sr)
        {
            try
            {
                this.ra = Convert.ToDouble(ra);
                this.dec = Convert.ToDouble(dec);
                this.sr = Convert.ToDouble(sr);
                return true;
            }
            catch (Exception e)
            {
                //VOTABLE v = VOTableUtil.CreateErrorVOTable(e.Message);
                //return v;
                return false;
            }
        }

        private bool CheckLimits(System.Double ra, System.Double dec, System.Double sr)
        {
            if ((sr < 0.0)) return false;
            if ((ra < 0.0) || (ra > 360.0)) return false;
            if ((dec < -90.0) || (dec > 90.0)) return false;
            return true;
        }
        
        
        public VOTABLE ConeSearch(String ra, String dec, String sr)
        {
            if (!ValidateInput(ra, dec, sr)) throw new Exception("The input values are not in correct format. ra,dec,sr all need real numbers.");

            if (!CheckLimits(this.ra, this.dec, this.sr))  throw new Exception(" Wrong input parameters ");
            try
            {
                this.sr *= 60.0; // in arcminutes because dbo.fGetNearbyObjEq requires  arcminutes ;

                StringBuilder qry = new StringBuilder();
                qry.Append("select " + ConeSelect);
                qry.Append("  from PhotoPrimary p, dbo.fGetNearbyObjEq(" + this.ra + "," + this.dec + "," + this.sr + ") n");
                qry.Append("  where p.objId=n.objId");

                this.sr /= 60.0; // back to degrees as the ervices requieres;
                JobsSoapClient cjobs = new JobsSoapClient();
                DataSet ds = cjobs.ExecuteQuickJobDS(CJobsWSID, CJobsPasswd, qry.ToString(), CJobsTARGET, "FOR CONESEARCH", false);
                Hashtable ucds = FetchUCDS(ds, cjobs);
                VOTABLE vot = VOTableUtil.DataSet2VOTable(ds);
                vot.DESCRIPTION = new anyTEXT();
                // = "ConeSearch results from the Sloan Digital Sky Survey ";
                vot.RESOURCE[0].TABLE[0].Items = new object[ds.Tables[0].Columns.Count + 3];
                Hashtable votypes = VOTableUtil.getdataTypeTable();
                PARAM p = new PARAM();
                p.name = "inputRA";
                p.datatype = (dataType)votypes[typeof(System.Single)];
                p.value = this.ra.ToString();
                p.unit = "degrees";
                vot.RESOURCE[0].TABLE[0].Items[0] = p;
                p = new PARAM();
                p.name = "inputDEC";
                p.datatype = (dataType)votypes[typeof(System.Single)];
                p.unit = "degrees";
                p.value = this.dec.ToString();
                vot.RESOURCE[0].TABLE[0].Items[1] = p;
                p = new PARAM();
                p.name = "inputSR";
                p.datatype = (dataType)votypes[typeof(System.Single)];
                p.unit = "degrees";
                p.value = this.sr.ToString();
                vot.RESOURCE[0].TABLE[0].Items[2] = p;

                vot.DESCRIPTION.Any = new System.Xml.XmlNode[1];
                XmlDocument doc = new XmlDocument();
                vot.DESCRIPTION.Any[0] = doc.CreateTextNode("DESCRIPTION");
                vot.DESCRIPTION.Any[0].InnerText = "ConeSearch results from the Sloan Digital Sky Survey";
                //vot.RESOURCE[0].Items[ind] = p;

                for (int x = 0; x < ds.Tables[0].Columns.Count; x++)
                {
                    DataColumn col = ds.Tables[0].Columns[x];
                    FIELD f = new FIELD();
                    f.datatype = (dataType)votypes[col.DataType];
                    f.ID = fix(col.ColumnName);
                    f.ucd = ucds[fix(col.ColumnName)] != null ? ucds[fix(col.ColumnName)].ToString() : "UNKNOWN";
                    vot.RESOURCE[0].TABLE[0].Items[x + 3] = f;
                }

                return vot;
            }catch(Exception exp){
                throw new Exception(exp.Message);
            }
        }

        private Hashtable FetchUCDS(DataSet ds, JobsSoapClient cjobs)
        {
            string qry = "select name,ucd from dbcolumns where tablename = 'photoobjall' and (";

            for (int x = 0; x < ds.Tables[0].Columns.Count; x++)
            {
                qry += " name like '" + fix(ds.Tables[0].Columns[x].ColumnName) + "'";
                if (x < ds.Tables[0].Columns.Count - 1) qry += " or ";
            }
            qry += ")";
            Hashtable rst = new Hashtable();
            try
            {
                DataSet d = cjobs.ExecuteQuickJobDS(CJobsWSID, CJobsPasswd, qry, CJobsTARGET, "CONESEARCH UCD LOOKUP", false);
                foreach (DataRow row in d.Tables[0].Rows)
                {

                    if (!rst.Contains(row[0]))
                        rst.Add(row[0].ToString().ToUpper().Trim(), row[1]);
                }
                string crap = "";
                foreach (object o in rst.Keys)
                    crap += o.ToString() + " ";
            }
            catch
            {
                rst.Add("OBJID", "ID_MAIN");
                rst.Add("RA", "POS_EQ_RA_MAIN");
                rst.Add("DEC", "POS_EQ_DEC_MAIN");
            }
            return rst;

        }

        private string fix(string s)
        {
            return Regex.Replace(s, "<C[0-9]+\\/>", "").ToUpper().Trim();
        }

    }
}