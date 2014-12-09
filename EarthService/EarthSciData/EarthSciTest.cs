using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sciserver_webService.EarthSciData
{
    public class EarthSciTest
    {
        private string query = "";

        public string queryResult { get; set; }

        public EarthSciTest() { 
        
        }

        public EarthSciTest(string query)
        {
            this.query = query;
        }

    }
}