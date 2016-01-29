using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sciserver_webService.ExceptionFilter
{
    public class AuthException : Exception
    {
        public AuthException()
            : base()
        {
        }

        public AuthException(string message)
            : base(message)
        {
        }

        public AuthException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}