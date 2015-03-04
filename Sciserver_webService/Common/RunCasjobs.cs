using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Data;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using net.ivoa.VOTable;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;

using System.Threading.Tasks;
using System.Text;
using System.IO;
using System.Web.Http;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;

using Sciserver_webService.casjobs;
using Sciserver_webService.ConeSearch;
using Sciserver_webService.SDSSFields;
using Sciserver_webService.Common;

///This class is used to submit query to casjobs
namespace Sciserver_webService.UseCasjobs
{

    public class RunCasjobs  : IHttpActionResult 
    {
        String query = "";
        String casjobsTaskName = "";
        String returnType = "";
        String token ="";
        String casjobsTarget = "";
      

        net.ivoa.VOTable.VOTABLE vot;

        public RunCasjobs(string query, String token, string casjobsTaskName, string returnType, string target) {
            this.query = query;
            this.token = token;
            this.casjobsTaskName = casjobsTaskName;
            this.returnType = returnType;
            this.casjobsTarget = target;
        }    


        public async Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(KeyWords.casjobsREST + "contexts/" + casjobsTarget + "/query");

            StringContent content = new StringContent(this.getJsonContent(query, casjobsTaskName));
            if (!(token == null || token == String.Empty))
                content.Headers.Add(KeyWords.xauth, token);
            content.Headers.ContentType = new MediaTypeHeaderValue(KeyWords.contentJson);

            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(returnType));//"application/json"));

            System.IO.Stream stream = await client.PostAsync(client.BaseAddress, content).Result.Content.ReadAsStreamAsync();

            return processCasjobsResults(stream);
        }


        private HttpResponseMessage processCasjobsResults(Stream stream) {

            var response = new HttpResponseMessage();
            DataSet ds;
            if (returnType.Equals(KeyWords.contentDataset))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                ds = (DataSet)formatter.Deserialize(stream);
                NewSDSSFields sf = new NewSDSSFields();
                switch (casjobsTaskName)
                {

                    case "FOR CONE SEARCH":
                        DefaultCone cstest = new DefaultCone();
                        vot = cstest.ConeSearch(ds);
                        response.Content = new StringContent(ToXML(vot), Encoding.UTF8, "application/xml");
                        break;

                    case "SDSSFields:FieldArray":

                        response.Content = new StringContent(ToXML(sf.FieldArray(ds)), Encoding.UTF8, "application/xml");
                        break;

                    case "SDSSFields:FieldArrayRect":

                        response.Content = new StringContent(ToXML(sf.FieldArrayRect(ds)), Encoding.UTF8, "application/xml");
                        break;

                    case "SDSSFields:ListOfFields":

                        response.Content = new StringContent(ToXML(sf.ListOfFields(ds)), Encoding.UTF8, "application/xml");
                        break;

                    case "SDSSFields:UrlsOfFields":

                        response.Content = new StringContent(ToXML(sf.UrlOfFields(ds)), Encoding.UTF8, "application/xml");
                        break;

                    default:
                        response.Content = new StreamContent(stream);
                        break;
                }

            }
            else
            {
                response.Content = new StreamContent(stream);
            }            
            return response;
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
      
      
     

       
        ////// Using New CAsjobs WebService
        
        //public HttpResponseMessage postCasjobs(string query, String token, string casjobsTaskName)
        //{
        //    HttpClient client = new HttpClient();
        //    client.BaseAddress = new Uri(KeyWords.casjobsREST);

        //    StringContent content = new StringContent("{\"Query\":\"" + query + "\" , \"TaskName\":\""+casjobsTaskName+"\"}");
        //    if(!(token == null || token == String.Empty))
        //    content.Headers.Add(KeyWords.xauth, token);
        //    content.Headers.ContentType = new MediaTypeHeaderValue(KeyWords.contentJson);

        //    HttpResponseMessage response = client.PostAsync(KeyWords.casjobsContextPath, content).Result;
            
        //    response.EnsureSuccessStatusCode();
            
        //    if(response.IsSuccessStatusCode)
        //        return response;
        //    else
        //        throw new ApplicationException("Query did not return results successfully, check input and try again later.");                
        //}

        //public HttpResponseMessage postCasjobs(string query, String token, string casjobsTaskName, string format)
        //{
        //    HttpClient client = new HttpClient();
        //    client.BaseAddress = new Uri(KeyWords.casjobsREST);

        //    //StringContent content = new StringContent("{\"Query\":\"" + query + "\" , \"TaskName\":\"" + casjobsTaskName + "\"}");
        //    StringContent content = new StringContent(this.getJsonContent(query,casjobsTaskName));
        //    if (!(token == null || token == String.Empty))
        //        content.Headers.Add(KeyWords.xauth, token);
        //    content.Headers.ContentType = new MediaTypeHeaderValue(KeyWords.contentJson);

        //    HttpResponseMessage response = client.PostAsync(KeyWords.casjobsContextPath, content).Result;

        //    response.EnsureSuccessStatusCode();

        //    if (response.IsSuccessStatusCode)
        //        return response;
        //    else
        //        throw new ApplicationException("Query did not return results successfully, check input and try again later.");
        //}

        
        
        ////public async Task<HttpResponseMessage> postCasjobs(string query, String token, string casjobsTaskName, HttpResponseMessage response)
        ////{
        ////    //string casjobsTaskname = "test";
        ////    HttpClient client = new HttpClient();
        ////    client.BaseAddress = new Uri(KeyWords.casjobsREST);

        ////    StringContent content = new StringContent("{\"Query\":\"" + query + "\" , \"TaskName\":\"" + casjobsTaskName + "\"}");
        ////    if (!(token == null || token == String.Empty))
        ////        content.Headers.Add(KeyWords.xauth, token);
        ////    content.Headers.ContentType = new MediaTypeHeaderValue(KeyWords.contentJson);

        ////    System.IO.Stream stream = await client.PostAsync(KeyWords.casjobsContextPath, content).Result.Content.ReadAsStreamAsync();
        ////    //response.EnsureSuccessStatusCode();
        ////    //if (response.IsSuccessStatusCode)
        ////    //    return response;
        ////    //else
        ////    //    throw new ApplicationException("Query did not return results successfully, check input and try again later.");
        ////    return response;
        ////}

        ///// <summary>
        /////  Upload data to run queries using table join
        ///// </summary>
        ///// <param name="datastring"></param>
        ///// <param name="token"></param>
        ///// <returns></returns>
        //public HttpResponseMessage uploadCasjobs(string datastring, string token)
        //{
        //    HttpClient client = new HttpClient();
        //    client.BaseAddress = new Uri(KeyWords.casjobsREST);

        //    StringContent content = new StringContent("\"" + datastring + "\"");

        //    content.Headers.Add(KeyWords.xauth, token);
        //    content.Headers.ContentType = new MediaTypeHeaderValue(KeyWords.contentJson);

        //    HttpResponseMessage response = client.PostAsync(KeyWords.casjobsContextPath, content).Result;
        //    response.EnsureSuccessStatusCode();
        //    if (response.IsSuccessStatusCode)
        //        return response;
        //    else
        //        throw new ApplicationException("Query did not return results successfully, check input and try again later.");

        //}
    }
}