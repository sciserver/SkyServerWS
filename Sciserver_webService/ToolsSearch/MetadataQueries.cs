using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sciserver_webService.ToolsSearch
{


    public class MetadataQueries
    {

        public static string allTablesColumns = "SELECT * FROM DBColumns";
        public static string tableColumns = "SELECT * FROM DBColumns WHERE tableName=@tablename";
        public static string tableColumnNames = "SELECT [name] FROM DBColumns WHERE tableName=@tablename";
        public static string runs = "SELECT run,stripe,startField,endField FROM Run ORDER BY run";
        public static string legacyPlates = "SELECT plateID, plate, mjd from PlateX order by plateID";
        public static string legacyPlate= "select plate,mjd,ra,dec from plateX where plateid = @plateID";
        public static string apogeePlate = "select plate,mjd,racen,deccen from apogeePlate where plate_visit_id = @apogeeplateid";
        public static string runs2 = "select distinct stripe, run from Run order by stripe, run";


        public static string schema_showDropList(string type)
        {
            string cmd;
            if (type == "C")
            {
                cmd = "select name, type from DBObjects where name like '%Constants%'";
                cmd += " or name like '%Defs%'";
            }
            else if (type == "I")
            {
                cmd = "select name, type from DBObjects where type='U'";
                cmd += " and access='U' and name NOT IN ('LoadEvents', 'QueryResults')";
            }
            else if (type == "F" || type == "P")
            {
                cmd = "select name, type from DBObjects where type='" + type + "'";
                cmd += " and access='U' and name NOT IN ('LoadEvents', 'QueryResults')";
            }
            else
            {
                cmd = "select name, type from DBObjects where type='" + type + "'";
                cmd += " and name NOT IN ('LoadEvents', 'QueryResults')";
            }
            cmd += " order by name";
            return cmd;
        }

        public static string schema_parentfromViewName = "select distinct parent from DBViewCols where viewname=@name";

        public static string schema_constants(string name)
        {
            string cmd = "select * from @name";
            if (name == "DataConstants") cmd += " order by field, value";
            return cmd;
        }

        public static string schema_constantsFields = "select distinct c.field, o.description  from DataConstants c, DBObjects o where o.type='V' and o.name = c.field";

        public static string schema_showShortTable(char? type)
        {
            string cmd;
            if (type == 'C')
            {
                cmd = "select name, description from DBObjects where access='U' and name like '%Constants%'";
                cmd += " or name like '%Defs%' and name not like '#%'";
            }
            else if (type == 'F' || type == 'P')
            {
                cmd = "select name, description from DBObjects where access='U' and type=@type";
                cmd += " and access='U'";
            }
            else
                cmd = "select name, description from DBObjects where access='U' and type=@type";
            cmd += " order by name";
            return cmd;
        }

        public static string schema_table = "select [enum], [name], [type], [length], [unit], [ucd], [description] from dbo.fDocColumns(@name) ORDER BY [columnID]";
        public static string schema_function = "select * from fDocFunctionParams(@name)";

        public static string schema_indices(string name)
        {
            string cmd;

            if (name == "")
            {
                cmd = "select [indexMapID],[code],[type],[tableName],[fieldList],[foreignKey] from IndexMap order by [tableName],[indexMapId]";
            }
            else
            {
                cmd = "select [indexMapID],[code],[type],[tableName],";
                cmd += "[fieldList],[foreignKey] from IndexMap ";
                cmd += " where tableName=@name order by [indexMapId]";
            }
            return cmd;
        }

        public static string schema_description = "select description from DataConstants where field=@name and [name]=''";
        public static string schema_access = "select name, type, description from DBObjects where type in ('F','P') and access='U' and UPPER(name) like @name";
        public static string schema_enum = "exec spDocEnum @name";

    }
}