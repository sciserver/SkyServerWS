using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using net.ivoa.VOTable;

namespace Sciserver_webService.SearchTools
{
    public class SqlSearch
    {
        public string getJSON(String query) {
                
            RunCasjobs rCasjobs = new RunCasjobs(); 
            return rCasjobs.getJSON(query);                      
        }

        public string getJSONstring(String query)
        {

            RunCasjobs rCasjobs = new RunCasjobs();
            return rCasjobs.executeQuickQuery(query);
        }

         public VOTABLE getVOTable(String query) {
            
            RunCasjobs rCasjobs = new RunCasjobs();
            return rCasjobs.getVOtable(query);             
        }        
    }
}