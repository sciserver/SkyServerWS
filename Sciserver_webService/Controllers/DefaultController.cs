using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Sciserver_webService.Controllers
{
    public class DefaultController : ApiController
    {
        // GET api/default
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/default/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/default
        public void Post([FromBody]string value)
        {
        }

        // PUT api/default/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/default/5
        public void Delete(int id)
        {
        }
    }
}
