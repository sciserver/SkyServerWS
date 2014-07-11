using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using System.Reflection;
using System.Net.Http;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Net.Http.Headers;

// using httweb request
namespace Sciserver_webService.UseCasjobs
{
  
    public class HttpService
    {
        public HttpService()
        {
            ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(AcceptCertificate);
        }
        public string result = "";
        public HttpResponseMessage response;


      

        public async void postTest(Uri host, string path, Dictionary<string, string> headers, string payload)
        {
            //Uri url = new Uri(host, path);
            HttpClient client = new HttpClient();
            client.BaseAddress = host;

            // Add an Accept header for JSON format.
            //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));


            StringContent content = new StringContent("\"select top 10 ra, dec from Frame\"");

            content.Headers.Add("X-Auth-Token", "882adff2e60144d0bfa81179143019c4");
            //content.Headers.Add("Content-Type", "application/json");
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");


            response = await client.PostAsync(path, content);
            response.EnsureSuccessStatusCode();

            //result = await response.Content.ReadAsStringAsync();
            
        }

       
        public  HttpResponseMessage  postTest(Uri host, string path, Dictionary<string, string> headers, string payload, NetworkCredential credential)
        {
            //Uri url = new Uri(host, path);
            HttpClient client = new HttpClient();
            client.BaseAddress = host;

            // Add an Accept header for JSON format.
            //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));


            StringContent content = new StringContent("\"select top 10 ra, dec from Frame\"");
            
            content.Headers.Add("X-Auth-Token", "882adff2e60144d0bfa81179143019c4");
            //content.Headers.Add("Content-Type", "application/json");
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            
            HttpResponseMessage response =  client.PostAsync(path,content).Result;
            response.EnsureSuccessStatusCode();
            return response;

        }
        //    Uri url = new Uri(host, path);
        //    MvcHtmlString encodedPayload = MvcHtmlString.Create(payload);
        //    UTF8Encoding encoding = new UTF8Encoding();
        //    byte[] data = encoding.GetBytes(encodedPayload.ToHtmlString());

        //    var httpClient = new HttpClient();
        //    HttpResponseMessage response = await httpClient.PostAsync(url, new StringContent(data));
        //}
        public String PostJSON(Uri host, string path, Dictionary<string, string> headers, string payload, NetworkCredential credential)
        {
            try
            {
                Uri url = new Uri(host, path);
                MvcHtmlString encodedPayload = MvcHtmlString.Create(payload);
                UTF8Encoding encoding = new UTF8Encoding();
                byte[] data = encoding.GetBytes(encodedPayload.ToHtmlString());

                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;                
                request.Method = "POST";
                request.Credentials = credential;
                request.ContentLength = data.Length;
                request.KeepAlive = false;
                
                //request.ContentType = "application/json";
                //request.Headers.Add(request.ContentType , new MediaTypeHeaderValue("application/json"));

                MvcHtmlString htmlString1;
                MvcHtmlString htmlString2;
                foreach (KeyValuePair<string, string> header in headers)
                {
                    htmlString1 = MvcHtmlString.Create(header.Key);
                    htmlString2 = MvcHtmlString.Create(header.Value);
                    request.Headers.Add(htmlString1.ToHtmlString(), htmlString2.ToHtmlString());
                }
                
                
                //request.SetRawHeader("content-type", "application/json");
                
                using (Stream requestStream = request.GetRequestStream())
                {
                    requestStream.Write(data, 0, data.Length);
                    requestStream.Close();
                }

                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                using (Stream responseStream = response.GetResponseStream())
                {
                    if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.Created)
                    {
                        throw new HttpException((int)response.StatusCode, response.StatusDescription);
                    }

                    //XDocument xmlDoc = XDocument.Load(responseStream);
                    String json = "";
                    //responseStream.Close();
                    //response.Close();
                    using (StreamReader reader = new StreamReader(responseStream))
                    {
                        json = reader.ReadToEnd();
                    }

                    return json;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        //public XDocument Post(Uri host, string path, Dictionary<string, string> headers, string payload, NetworkCredential credential)
        //{
        //    try
        //    {
        //        Uri url = new Uri(host, path);
        //        MvcHtmlString encodedPayload = MvcHtmlString.Create(payload);
        //        UTF8Encoding encoding = new UTF8Encoding();
        //        byte[] data = encoding.GetBytes(encodedPayload.ToHtmlString());

        //        HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
        //        request.Method = "POST";
        //        request.Credentials = credential;
        //        request.ContentLength = data.Length;
        //        request.KeepAlive = false;
        //        request.ContentType = "application/xml";

        //        MvcHtmlString htmlString1;
        //        MvcHtmlString htmlString2;
        //        foreach (KeyValuePair<string, string> header in headers)
        //        {
        //            htmlString1 = MvcHtmlString.Create(header.Key);
        //            htmlString2 = MvcHtmlString.Create(header.Value);
        //            request.Headers.Add(htmlString1.ToHtmlString(), htmlString2.ToHtmlString());
        //        }

        //        using (Stream requestStream = request.GetRequestStream())
        //        {
        //            requestStream.Write(data, 0, data.Length);
        //            requestStream.Close();
        //        }

        //        using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
        //        using (Stream responseStream = response.GetResponseStream())
        //        {
        //            if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.Created)
        //            {
        //                throw new HttpException((int)response.StatusCode, response.StatusDescription);
        //            }

        //            XDocument xmlDoc = XDocument.Load(responseStream);
        //            responseStream.Close();
        //            response.Close();

        //            return xmlDoc;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw;
        //    }
        //}

        //public XDocument Get(Uri host, string path, Dictionary<string, string> parameters, NetworkCredential credential)
        //{
        //    try
        //    {
        //        Uri url;
        //        StringBuilder parameterString = new StringBuilder();

        //        if (parameters == null || parameters.Count <= 0)
        //        {
        //            parameterString.Clear();
        //        } else {
        //            parameterString.Append("?");
        //            foreach (KeyValuePair<string, string> parameter in parameters)
        //            {
        //                parameterString.Append(parameter.Key + "=" + parameter.Value + "&");
        //            }
        //        }
        //        url = new Uri(host, path + parameterString.ToString().TrimEnd(new char[] { '&' }));

        //        HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
        //        request.Credentials = credential;
        //        using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
        //        {
        //            if (response.StatusCode != HttpStatusCode.OK)
        //            {
        //                throw new HttpException((int)response.StatusCode, response.StatusDescription);
        //            }

        //            XDocument xmlDoc = XDocument.Load(response.GetResponseStream());
        //            return xmlDoc;

        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw;
        //    }
        //}


        public String Getstring(Uri host, string path, Dictionary<string, string> parameters, NetworkCredential credential)
        {
            try
            {
                Uri url;
                StringBuilder parameterString = new StringBuilder();

                if (parameters == null || parameters.Count <= 0)
                {
                    parameterString.Clear();
                }
                else
                {
                    parameterString.Append("?");
                    foreach (KeyValuePair<string, string> parameter in parameters)
                    {
                        parameterString.Append(parameter.Key + "=" + parameter.Value + "&");
                    }
                }
                url = new Uri(host, path + parameterString.ToString().TrimEnd(new char[] { '&' }));

                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                request.Credentials = credential;
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        throw new HttpException((int)response.StatusCode, response.StatusDescription);
                    }

                     return response.GetResponseStream().ToString();
                    //XDocument xmlDoc = XDocument.Load(response.GetResponseStream());
                    //return xmlDoc;

                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /*
        I use this class for internal web services.  For external web services, you'll want
        to put some logic in here to determine whether or not you should accept a certificate
        or not if the domain name in the cert doesn't match the url you are accessing.
        */
        private static bool AcceptCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true;
        }     

    }
}

public static class HttpWebRequestExtensions
{
    static string[] RestrictedHeaders = new string[] {
            "Accept",
            "Connection",
            "Content-Length",
            "Content-Type",
            "Date",
            "Expect",
            "Host",
            "If-Modified-Since",
            "Range",
            "Referer",
            "Transfer-Encoding",
            "User-Agent", 
            "Proxy-Connection"
    };

    static Dictionary<string, PropertyInfo> HeaderProperties = new Dictionary<string, PropertyInfo>(StringComparer.OrdinalIgnoreCase);

    static HttpWebRequestExtensions()
    {
        Type type = typeof(HttpWebRequest);
        foreach (string header in RestrictedHeaders)
        {
            //string propertyName = header.Replace("-", "");
            //PropertyInfo headerProperty = type.GetProperty(propertyName);
            PropertyInfo headerProperty = type.GetProperty(header);
            HeaderProperties[header] = headerProperty;
        }
    }

    public static void SetRawHeader(this HttpWebRequest request, string name, string value)
    {
        if (HeaderProperties.ContainsKey(name))
        {
            HeaderProperties[name].SetValue(request, value, null);
        }
        else
        {
            request.Headers[name] = value;
        }
    }
}