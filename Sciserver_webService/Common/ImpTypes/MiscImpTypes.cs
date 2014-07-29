using System;
using System.Data;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace net.ivoa.data {
    class DateTimeType : ImpType {
        public override SqlDbType SqlType { get { return SqlDbType.DateTime; } }
        public override bool StrIsType(string s) {
            return DateTime.TryParse(s, out junk);
        }
        public override object ParseToken(string s) {
            if (base.IsNull(s)) return null;
            return DateTime.Parse(s); 
        }
        static DateTime junk;
    }
    class StrType : ImpType {
        public override SqlDbType SqlType { get { return SqlDbType.VarChar; } }
        public override bool StrIsType(string s) {
            return true;
        }
        public override object ParseToken(string s) {
            if (base.IsNull(s)) return null;
            return s; 
        }
    }
    
}
