using System;
using System.Data;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace net.ivoa.data {
    class SmallFloatType: ImpType {
        public override SqlDbType SqlType { get { return SqlDbType.Real; } }
        public override bool StrIsType(string s) {
            if (base.IsNull(s)) return true;
            return Single.TryParse(s, out junk);
        }
        public override object ParseToken(string s) {
            if (base.IsNull(s)) return null;
            return Single.Parse(s); 
        }
        static Single junk;
    }
    class BigFloatType : ImpType {
        public override SqlDbType SqlType { get { return SqlDbType.Float; } }
        public override bool StrIsType(string s) {
            if (base.IsNull(s)) return true;
            return double.TryParse(s, out junk); 
        }
        public override object ParseToken(string s) {
            if (base.IsNull(s)) return null;
            return double.Parse(s); 
        }
        static double junk;
    }
    
}
