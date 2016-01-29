using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sciserver_webService.ExceptionFilter
{
    public class NotFoundException : Exception
    {
        public NotFoundException()
            : base()
        {
        }

        public NotFoundException(string message)
            : base(message)
        {
        }

        public NotFoundException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}