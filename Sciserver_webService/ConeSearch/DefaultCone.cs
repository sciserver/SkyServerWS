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
using Sciserver_webService.UseCasjobs;
using Sciserver_webService.Common;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;


namespace Sciserver_webService.ConeSearch
{
    public class DefaultCone
    {
        private double ra = 0.0;
        private double dec = 0.0;
        private double sr = 0.0;

        public DefaultCone() { 
        }

        public VOTABLE ConeSearch(DataSet ds)
        {
            try
            {
                //Hashtable ucds = FetchUCDS(ds, cjobs);
                VOTABLE vot = VOTableUtil.DataSet2VOTable(ds);
                vot.DESCRIPTION = new anyTEXT();                
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
                    //f.ucd = ucds[fix(col.ColumnName)] != null ? ucds[fix(col.ColumnName)].ToString() : "UNKNOWN";
                    vot.RESOURCE[0].TABLE[0].Items[x + 3] = f;
                }

                return vot;
            }
            catch (Exception exp)
            {
                throw new Exception(exp.Message);
            }
        }

        //private Hashtable FetchUCDS(DataSet ds, JobsSoapClient cjobs)
        //{
        //    string qry = "select name,ucd from dbcolumns where tablename = 'photoobjall' and (";

        //    for (int x = 0; x < ds.Tables[0].Columns.Count; x++)
        //    {
        //        qry += " name like '" + fix(ds.Tables[0].Columns[x].ColumnName) + "'";
        //        if (x < ds.Tables[0].Columns.Count - 1) qry += " or ";
        //    }
        //    qry += ")";
        //    Hashtable rst = new Hashtable();
        //    try
        //    {
        //        RunCasjobs run = new RunCasjobs(qry, "", "ConeSearch Sub task", KeyWords.dataset, "DR12");               
                
        //        Task<Stream> task2 =  await run.quickRun();

        //        BinaryFormatter formatter = new BinaryFormatter();
        //        DataSet d = (DataSet)formatter.Deserialize();
        //        foreach (DataRow row in d.Tables[0].Rows)
        //        {

        //            if (!rst.Contains(row[0]))
        //                rst.Add(row[0].ToString().ToUpper().Trim(), row[1]);
        //        }
        //        string crap = "";
        //        foreach (object o in rst.Keys)
        //            crap += o.ToString() + " ";
        //    }
        //    catch
        //    {
        //        rst.Add("OBJID", "ID_MAIN");
        //        rst.Add("RA", "POS_EQ_RA_MAIN");
        //        rst.Add("DEC", "POS_EQ_DEC_MAIN");
        //    }
        //    return rst;

        //}

        private string fix(string s)
        {
            return Regex.Replace(s, "<C[0-9]+\\/>", "").ToUpper().Trim();
        }

    }
}