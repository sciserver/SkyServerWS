using System;
using System.Data;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace net.ivoa.data {
    /// <summary>
    /// good ref for mappings...
    /// http://msdn.microsoft.com/en-us/library/system.data.sqldbtype.aspx
    /// </summary>
    abstract class ImpType {
        public abstract SqlDbType SqlType { get; }
        public abstract bool StrIsType(string s);
        public bool IsNull(string s) {
            return s == null || s == "";
        }
        public abstract object ParseToken(string token);
    }
}
