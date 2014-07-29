using System;
using System.Data;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace net.ivoa.data {
    class I1Type : ImpType {
        public override SqlDbType SqlType { get { return SqlDbType.Bit; } }
        public override bool StrIsType(string s) {
            if (base.IsNull(s)) return true;
            if (s == "0" || s == "1") return true;
            if (bool.TryParse(s, out junk)) return true;
            return false;
        }
        public override object ParseToken(string s) {
            if (base.IsNull(s)) return null;
            if (s == "0") return false;
            else if (s == "1") return true;
            else return bool.Parse(s); 
        }
        static bool junk;
    }
    class I8Type : ImpType {
        public override SqlDbType SqlType { get { return SqlDbType.TinyInt; } }
        public override bool StrIsType(string s) {
            if (base.IsNull(s)) return true;
            return byte.TryParse(s, out junk); 
        }
        public override object ParseToken(string s) {
            if (base.IsNull(s)) return null;
            return byte.Parse(s); 
        }
        static byte junk;
    }
    class I16Type : ImpType {
        public override SqlDbType SqlType { get { return SqlDbType.SmallInt; } }
        public override bool StrIsType(string s) {
            if (base.IsNull(s)) return true;
            return Int16.TryParse(s, out junk); 
        }
        public override object ParseToken(string s) {
            if (base.IsNull(s)) return null;
            return Int16.Parse(s); 
        }
        static Int16 junk;
    }
    class I32Type : ImpType {
        public override SqlDbType SqlType { get { return SqlDbType.Int; } }
        public override bool StrIsType(string s) {
            if (base.IsNull(s)) return true;
            return Int32.TryParse(s, out junk); 
        }
        public override object ParseToken(string s) {
            if (base.IsNull(s)) return null;
            return Int32.Parse(s); 
        }
        static Int32 junk;
    }
    class I64Type : ImpType {
        public override SqlDbType SqlType { get { return SqlDbType.BigInt; } }
        public override bool StrIsType(string s) {
            if (base.IsNull(s)) return true;
            return Int64.TryParse(s, out junk); 
        }
        public override object ParseToken(string s) {
            if (base.IsNull(s)) return null;
            return Int64.Parse(s); 
        }
        static Int64 junk;
    }
}
