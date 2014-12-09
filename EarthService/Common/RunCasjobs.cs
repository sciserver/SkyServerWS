using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
//using Sciserver_webService.casjobs;
using System.Configuration;
using System.Data;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using net.ivoa.VOTable;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using Sciserver_webService.Common;
using System.Threading.Tasks;
//using System.Web.Mvc.ActionResult;
using System.Text;
using System.IO;

///This class is used to submit query to casjobs
namespace Sciserver_webService.UseCasjobs
{

    public class RunCasjobs
    {
       

        //private string casjobsMessage { get; set; }

        public RunCasjobs() { }

        //public RunCasjobs(string casjobsMessage) {
        //    this.casjobsMessage = casjobsMessage;
        //}

        //private DataSet getQueryResult(String query)
        //{
        //    try
        //    {
        //        JobsSoapClient cjobs = new JobsSoapClient();
        //        DataSet ds = cjobs.ExecuteQuickJobDS(KeyWords.CJobsWSID, KeyWords.CJobsPasswd, query, KeyWords.CJobsTARGET, casjobsMessage, false);
        //        return ds;
        //    }
        //    catch (Exception e) {
        //        throw new Exception("Error running Query :\n"+e.Message);
        //    }
        //}

        //public String executeQuickQuery(String query)
        //{
        //    try
        //    {
        //        JobsSoapClient cjobs = new JobsSoapClient();
        //        String ds = cjobs.ExecuteQuickJob(KeyWords.CJobsWSID, KeyWords.CJobsPasswd, query, KeyWords.CJobsTARGET, casjobsMessage, false);
        //        return ds;
        //    }
        //    catch (Exception e)
        //    {
        //        throw new Exception("Error during Query execution:\n" + e.Message);
        //    }
        //}
     

        //public String getJSON(String query) {
        //    try
        //    {
        //        return JsonConvert.SerializeObject(getQueryResult(query), Formatting.Indented);
        //    }
        //    catch (Exception e) {
        //        throw new Exception("Error serializing result in JSON format.\n"+e.Message);
        //    }
        //}
       
        
        //public VOTABLE getVOtable(String query) {
        //    try
        //    {
        //        VOTABLE vot = VOTableUtil.DataSet2VOTable(getQueryResult(query));
        //        return vot;
        //    }
        //    catch (Exception e) {
        //        throw new Exception("Error getting results in VOTable format.\n"+e.Message);
        //    }
        //}

        //// Using New CAsjobs WebService
        
        public HttpResponseMessage postCasjobs(string query, String token, string casjobsTaskName)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(KeyWords.casjobsREST);

            StringContent content = new StringContent("{\"Query\":\"" + query + "\" , \"TaskName\":\""+casjobsTaskName+"\"}");
            if(!(token == null || token == String.Empty))
            content.Headers.Add(KeyWords.xauth, token);
            content.Headers.ContentType = new MediaTypeHeaderValue(KeyWords.contentJson);

            //HttpResponseMessage response = client.PostAsync(KeyWords.casjobsContextPath, content).Result;
            HttpResponseMessage response = client.PostAsync(KeyWords.earthContextPath, content).Result;
            response.EnsureSuccessStatusCode();
            
            if(response.IsSuccessStatusCode)
                return response;
            else
                throw new ApplicationException("Query did not return results successfully, check input and try again later.");                
        }

        public HttpResponseMessage postCasjobs(string query, String token, string casjobsTaskName, string format)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(KeyWords.casjobsREST);

            //StringContent content = new StringContent("{\"Query\":\"" + query + "\" , \"TaskName\":\"" + casjobsTaskName + "\"}");
            StringContent content = new StringContent(this.getJsonContent(query,casjobsTaskName));
            if (!(token == null || token == String.Empty))
                content.Headers.Add(KeyWords.xauth, token);
            content.Headers.ContentType = new MediaTypeHeaderValue(KeyWords.contentJson);

            HttpResponseMessage response = client.PostAsync(KeyWords.casjobsContextPath, content).Result;

            response.EnsureSuccessStatusCode();

            if (response.IsSuccessStatusCode)
                return response;
            else
                throw new ApplicationException("Query did not return results successfully, check input and try again later.");
        }

        private String getJsonContent(String query, String casjobsTaskName) {
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
        
        //public async Task<HttpResponseMessage> postCasjobs(string query, String token, string casjobsTaskName, HttpResponseMessage response)
        //{
        //    //string casjobsTaskname = "test";
        //    HttpClient client = new HttpClient();
        //    client.BaseAddress = new Uri(KeyWords.casjobsREST);

        //    StringContent content = new StringContent("{\"Query\":\"" + query + "\" , \"TaskName\":\"" + casjobsTaskName + "\"}");
        //    if (!(token == null || token == String.Empty))
        //        content.Headers.Add(KeyWords.xauth, token);
        //    content.Headers.ContentType = new MediaTypeHeaderValue(KeyWords.contentJson);

        //    System.IO.Stream stream = await client.PostAsync(KeyWords.casjobsContextPath, content).Result.Content.ReadAsStreamAsync();
        //    //response.EnsureSuccessStatusCode();
        //    //if (response.IsSuccessStatusCode)
        //    //    return response;
        //    //else
        //    //    throw new ApplicationException("Query did not return results successfully, check input and try again later.");
        //    return response;
        //}

        /// <summary>
        ///  Upload data to run queries using table join
        /// </summary>
        /// <param name="datastring"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public HttpResponseMessage uploadCasjobs(string datastring, string token)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(KeyWords.casjobsREST);

            StringContent content = new StringContent("\"" + datastring + "\"");

            content.Headers.Add(KeyWords.xauth, token);
            content.Headers.ContentType = new MediaTypeHeaderValue(KeyWords.contentJson);

            HttpResponseMessage response = client.PostAsync(KeyWords.casjobsContextPath, content).Result;
            response.EnsureSuccessStatusCode();
            if (response.IsSuccessStatusCode)
                return response;
            else
                throw new ApplicationException("Query did not return results successfully, check input and try again later.");

        }
    }
}