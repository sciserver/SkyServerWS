using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sciserver_webService.casjobs;
using System.Configuration;
using System.Data;
using System.Runtime.Serialization;
using Newtonsoft.Json;
//using System.Xml;
using net.ivoa.VOTable;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;

namespace Sciserver_webService.SearchTools
{

    public class RunCasjobs
    {
       

        private string casjobsMessage { get; set; }

        public RunCasjobs() { }

        public RunCasjobs(string casjobsMessage) {
            this.casjobsMessage = casjobsMessage;
        }

        private DataSet getQueryResult(String query)
        {
            try
            {
                JobsSoapClient cjobs = new JobsSoapClient();
                DataSet ds = cjobs.ExecuteQuickJobDS(Globals.CJobsWSID, Globals.CJobsPasswd, query, Globals.CJobsTARGET, casjobsMessage, false);
                return ds;
            }
            catch (Exception e) {
                throw new Exception("Error running Query :\n"+e.Message);
            }
        }

        public String executeQuickQuery(String query)
        {
            try
            {
                JobsSoapClient cjobs = new JobsSoapClient();
                String ds = cjobs.ExecuteQuickJob(Globals.CJobsWSID, Globals.CJobsPasswd, query, Globals.CJobsTARGET, casjobsMessage, false);
                return ds;
            }
            catch (Exception e)
            {
                throw new Exception("Error during Query execution:\n" + e.Message);
            }
        }
     

        public String getJSON(String query) {
            try
            {
                return JsonConvert.SerializeObject(getQueryResult(query), Formatting.Indented);
            }
            catch (Exception e) {
                throw new Exception("Error serializing result in JSON format.\n"+e.Message);
            }
        }
       
        
        public VOTABLE getVOtable(String query) {
            try
            {
                VOTABLE vot = VOTableUtil.DataSet2VOTable(getQueryResult(query));
                return vot;
            }
            catch (Exception e) {
                throw new Exception("Error getting results in VOTable format.\n"+e.Message);
            }

        }

        //// Using New CAsjobs WebService
        
        public HttpResponseMessage postCasjobs(string query, string token)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(Globals.casjobsREST);

            StringContent content = new StringContent("\"" + query + "\"");

            content.Headers.Add(Globals.xauth, token);
            content.Headers.ContentType = new MediaTypeHeaderValue(Globals.contentJson);


            HttpResponseMessage response = client.PostAsync(Globals.casjobsContextPath, content).Result;
            response.EnsureSuccessStatusCode();
            if(response.IsSuccessStatusCode)
                return response;
            else
                throw new ApplicationException("Query did not return results successfully, check input and try again later.");
                
        }
    }
}