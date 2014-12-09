using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sciserver_webService.Models
{
    public class CustomResponse
    {
        public readonly string Value;

        public CustomResponse(string value)
        {
            this.Value = value;
        }
    }
}