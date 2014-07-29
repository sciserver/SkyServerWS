//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;

//namespace Sciserver_webService.ToolsSearch
//{
//    public class Class1
//    {

//        using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;
//using System.Data.Linq;
//using System.Configuration;
//using Test.Entities;

//namespace Test.Infrastructure
//{
//    public class PersonRepository
//    {
//        private HttpService _httpService;

//        public PersonRepository()
//        {
//            _httpService = new HttpService();
//        }

//        public IQueryable<Person> GetPeople()
//        {
//            try
//            {
//                Uri host = new Uri("http://www.yourdomain.com");
//                string path = "your/rest/path";
//                Dictionary<string, string> parameters = new Dictionary<string, string>();

//                //Best not to store this in your class
//                NetworkCredential credential = new NetworkCredential("username", "password");

//                XDocument xml = _httpService.Get(host, path, parameters, credential);
//                return ConvertPersonXmlToList(xml).AsQueryable();

//            } 
//            catch
//            {
//                throw;
//            }
//        }

//        private List<Person> ConvertPersonXmlToList(XDocument xml)
//        {
//            try
//            {
//                List<Person> perople = new List<Person>();
//                var query = xml.Descendants("Person")
//                                .Select(node => node.ToString(SaveOptions.DisableFormatting));
//                foreach (var personXml in query)
//                {
//                    people.Add(ObjectSerializer.DeserializeObject<Person>(personXml));
//                }
//                return people;
//            }
//            catch
//            {
//                throw;
//            }

//        }
//    }
//}
//    }
//}