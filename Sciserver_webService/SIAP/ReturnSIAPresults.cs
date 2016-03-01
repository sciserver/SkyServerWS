using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Xml.Serialization;
using Sciserver_webService.Common;

namespace Sciserver_webService.sdssSIAP
{
    public class ReturnSIAPresults : IHttpActionResult
    {
        
        String casjobsTaskName = "";
        String returnType = "";       
        String casjobsTarget = "";
        Dictionary<String, String> dictionary = null;
        LoggedInfo ActivityInfo = null;

        public ReturnSIAPresults() { }

        public ReturnSIAPresults(string casjobsTaskName, string returnType,  string casjobsTarget,  Dictionary<String, String> dictionary, LoggedInfo ActivityInfo) {
            this.casjobsTaskName = casjobsTaskName;
            this.returnType = returnType;            
            this.casjobsTarget = casjobsTarget;
            this.dictionary = dictionary;
            this.ActivityInfo = ActivityInfo;
        } 

        public async Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage();
            sdssSIAP.SIAP siap = new sdssSIAP.SIAP();
            SiapTable vout = new SiapTable();

            switch (casjobsTaskName)
            {
              case "getSIAP":
                    vout = siap.getSiapInfo(dictionary["POS"], dictionary["SIZE"], dictionary["FORMAT"], "");
                  break;
              case "getSIAPInfo":
                  vout = siap.getSiapInfo(dictionary["POS"], dictionary["SIZE"], dictionary["FORMAT"], dictionary["bandpass"]);

                  break;
              case "getSIAPInfoAll":
                  vout = siap.getSiapInfo(dictionary["POS"], dictionary["SIZE"], "All", "*");
                  break;

              default : break; 
            }

            response.Content = new StringContent(ToXML(vout), Encoding.UTF8, "application/xml");
            //logging
            SciserverLogging logger = new SciserverLogging();
            logger.LogActivity(ActivityInfo, "SkyserverMessage");
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
    }
}