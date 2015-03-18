using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Xml.Serialization;
using Sciserver_webService.SDSSFields;

namespace Sciserver_webService.sdssSIAP
{
    public class ReturnSIAPresults : IHttpActionResult
    {
        
        String casjobsTaskName = "";
        String returnType = "";       
        String casjobsTarget = "";
        Dictionary<String, String> dictionary = null;

        public ReturnSIAPresults() { }

        public ReturnSIAPresults(string casjobsTaskName, string returnType,  string casjobsTarget,  Dictionary<String, String> dictionary) {
            this.casjobsTaskName = casjobsTaskName;
            this.returnType = returnType;            
            this.casjobsTarget = casjobsTarget;
            this.dictionary = dictionary;
        } 

        public async Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage();
            sdssSIAP.SIAP siap = new sdssSIAP.SIAP();
            SiapTable vout = new SiapTable();

            switch (casjobsTaskName)
            {
              case "SIAP:getSIAP":
                    vout = siap.getSiapInfo(dictionary["POS"], dictionary["SIZE"], dictionary["FORMAT"], "");
                  break;
              case "SIAP:getSIAPInfo":
                  vout = siap.getSiapInfo(dictionary["POS"], dictionary["SIZE"], dictionary["FORMAT"], dictionary["bandpass"]);

                  break;
              case "SIAP:getSIAPInfoAll":
                  vout = siap.getSiapInfo(dictionary["POS"], dictionary["SIZE"], "All", "*");
                  break;  

              default : break; 
            }

            response.Content = new StringContent(ToXML(vout), Encoding.UTF8, "application/xml");
            return response;
        }

        /// <summary>
        ///  This is specifically used for the sdss vo services which are returning random types of xml outputs
        /// </summary>
        /// <returns></returns>
        public string ToXML(Object o)
        {
            var stringwriter = new System.IO.StringWriter();
            var serializer = new XmlSerializer(o.GetType());
            serializer.Serialize(stringwriter, o);
            return stringwriter.ToString();
        }

        /// <summary>
        /// This is specifically used in SiapTable.cs Its very simple way to get back dataset from the field url.
        /// </summary>
        /// <param name="ra"></param>
        /// <param name="dec"></param>
        /// <param name="size1"></param>
        /// <param name="size2"></param>
        /// <returns></returns>
        public Field[] runSDSSField(string uri)
        {            
            System.Net.HttpWebRequest request = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(uri);

            System.Net.HttpWebResponse response = (System.Net.HttpWebResponse)request.GetResponse();
         
            DataSet ds = new DataSet("FieldsResults");
            using (StreamReader sr = new StreamReader(response.GetResponseStream())) 
            {
                ds.ReadXml(sr);
            };         

            List<Field> strDetailIDList = new List<Field>();    
            foreach(DataRow row in ds.Tables[0].Rows)
            {
                DataTable dt = row.Table;              
                int cnt = 0;
                Hashtable hashtable = new Hashtable();
                foreach (object item in row.ItemArray)
                {
                    hashtable.Add(dt.Columns[cnt].ColumnName, item);                    
                    cnt++;
                }
                Field f = new Field(hashtable);
                strDetailIDList.Add(f);             
            }

            Field[] fa = strDetailIDList.ToArray();           
            return fa;            
        }        
    }
}