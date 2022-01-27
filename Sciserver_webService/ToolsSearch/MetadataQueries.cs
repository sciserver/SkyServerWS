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
        public static string stripeFromRun = "SELECT stripe, startField, (endField-startField+1) as nFields FROM Run WHERE run = @run";
        public static string legacyPlates = "SELECT plateID, plate, mjd from PlateX order by plateID";
        public static string legacyPlate= "select plate,mjd,ra,dec from plateX where plateid = @plateID";
        public static string apogeePlate = "select plate,mjd,racen,deccen from apogeePlate where plate_visit_id = @apogeeplateid";
        public static string runs2 = "select distinct stripe, run from Run order by stripe, run";

        //listing plates for survey
        public static string sdssPlateMJDList = "SELECT CAST(plateID as VARCHAR(20)) as plateID, plate, mjd from PlateX where survey='sdss' order by plateID";
        public static string seguePlateMJDList = "SELECT CAST(plateID as VARCHAR(20)) as plateID, plate, mjd from PlateX where survey='segue1' or survey='segue2' order by plateID";
        public static string bossPlateMJDList = "SELECT CAST(plateID as VARCHAR(20)) as plateID, plate, mjd from PlateX where survey='boss' order by plateID";
        public static string apogeePlateMJDList = "SELECT plate_visit_id as plateID, plate, mjd from apogeePlate order by plate,mjd";

        // getting objects in plate:
        




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
            name = name.ToLower();
            if(name == "dataconstants" || name == "profiledefs" || name == "sdssconstants" || name == "siteconstants" || name == "stripedefs"){
                string cmd = "";
                if (name == "dataconstants2")
                {
                    cmd = "select field, name, cast(value as varchar(max)), description from " + name;  
                }else
                {
                    cmd = "select * from " + name;
                }
                if (name == "dataconstants") cmd += " order by field, value";
                return cmd;
            }else{
                return "";
            }
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
        public static string descriptionFromDBObjects = "select description from DBObjects where name=@name";
        public static string textFromDBObjects = "select text from DBObjects where name=@name";




        public static string nearestobj = "SELECT TOP 1 cast(P.objID as varchar) AS 'objId', " +
                                            "  LTRIM(STR(P.ra,10,5))as 'ra', LTRIM(STR(P.dec,8,5)) as 'dec', " +
                                            "  dbo.fPhotoTypeN(P.type) as 'type', LTRIM(STR(P.u,6,2)) AS 'u', LTRIM(STR(P.g,6,2)) AS 'g', " +
                                            "  LTRIM(STR(P.r,6,2)) AS 'r', LTRIM(STR(P.i,6,2)) AS 'i', LTRIM(STR(P.z,6,2)) AS 'z'" +
                                            "  FROM dbo.fGetNearestObjEq (@ra,@dec,@radius) as N, " +
                                            "  PhotoObjAll as P" +
                                            "  WHERE N.objID = P.objID AND P.i>0 ";


        public static string nearestspecobjid = "select cast(s.specObjId as varchar) as specObjId from PhotoObjAll p LEFT OUTER JOIN SpecObj s ON s.bestobjid=p.objid where p.objId=@objid";

        public static string nearestapogee = " SELECT TOP 1 P.apstar_id AS 'apogee_Id', LTRIM(STR(P.ra,10,5))as 'ra', LTRIM(STR(P.dec,8,5)) as 'dec' ,  'apogee' as 'type'," +
                                                   " '' AS 'u', '' AS 'g',   '' AS 'r', '' AS 'i', '' AS 'z' " +
                                                   " FROM dbo.fGetNearestApogeeStarEq (@ra,@dec,@radius) as N,   ApogeeStar as P  WHERE N.apogee_id = P.apogee_id  ";


        public static string imgparams = "SELECT [name] FROM DBColumns WHERE tableName='PhotoObjAll'";
        public static string specparams = "SELECT[name] FROM DBColumns WHERE tableName = 'SpecObjAll'";
        public static string irspecparams = "SELECT [name] FROM DBColumns WHERE tableName='apogeeStar'";
        public static string photoflags = "SELECT [name] FROM DataConstants WHERE field='PhotoFlags' ORDER BY value";
        public static string primtargetflags = "SELECT [name] FROM DataConstants WHERE field='PrimTarget' ORDER BY value";
        public static string sectargetflags = "SELECT [name] FROM DataConstants WHERE field='SecTarget' ORDER BY value";
        public static string bosstargetflags = "SELECT [name] FROM DataConstants WHERE field='BossTarget1' ORDER BY value";
        public static string ebosstargetflags = "SELECT [name] FROM DataConstants WHERE field='EbossTarget0' ORDER BY value";
        public static string apogeetarget1flags = "SELECT [name] FROM DataConstants WHERE field='ApogeeTarget1' AND [name] != '' AND [name] NOT IN ('APOGEE_FAINT', 'APOGEE_MEDIUM', 'APOGEE_BRIGHT', 'APOGEE_CHECKED') ORDER BY field,value";
        public static string apogeetarget2flags = "SELECT [name] FROM DataConstants WHERE field='ApogeeTarget2' AND [name] != '' AND [name] NOT IN ('APOGEE_EMBEDDEDCLUSTER_STAR', 'APOGEE_LONGBAR', 'APOGEE_EMISSION_STAR', 'APOGEE_KEPLER_COOLDWARF', 'APOGEE_MIRCLUSTER_STAR', 'APOGEE_CHECKED') ORDER BY field,value";
        public static string fieldfromname = "SELECT [name] FROM DataConstants WHERE field=@name ORDER BY value";

        // implementing TAP schema:

        public static string allTables = "select  'dbo' as 'schema_name', name as 'table_name', case when type='U' then 'table' else 'view' end as 'table_type', null as 'utype', description, rank as 'table_index',text " +
                                      "FROM DBObjects where type = 'V' or type = 'U' order by rank, name";

        public static string onlyTables = "select  'dbo' as 'schema_name', name as 'table_name', case when type='U' then 'table' else 'view' end as 'table_type', null as 'utype', description, rank as 'table_index',text " +
                                          "FROM DBObjects where type = 'U' order by rank, name";
        public static string onlyViews = "select  'dbo' as 'schema_name', name as 'table_name', case when type='U' then 'table' else 'view' end as 'table_type', null as 'utype', description, rank as 'table_index',text " +
                                          "FROM DBObjects where type = 'V' order by rank, name";

        public static string allColumns = "select o.name as 'table_name', f.name as 'column_name', f.type as 'datatype', " +
                                          "case when f.type= 'varchar' or f.type= 'nvarchar' or f.type= 'varbinary' then f.length else null end as 'arraysize', " +
                                          "null as 'xtype', f.length as 'size', f.description as 'description', null as 'utype', f.unit as 'unit', " +
                                          "f.ucd as 'ucd', null as 'indexed', null as 'principal', null as 'std', f.columnID  as 'column_index',  case when o.type='V' then 1 else 0 end as 'is_view' " +
                                          "from (SELECT name, type FROM DBObjects where (type = 'V' or type = 'U') ) as o cross apply dbo.fDocColumns(o.name) as f ORDER BY o.name, f.columnID";
        public static string columnsForTable = "select o.name as 'table_name', f.name as 'column_name', f.type as 'datatype', " +
                                                  "case when f.type= 'varchar' or f.type= 'nvarchar' or f.type= 'varbinary' then f.length else null end as 'arraysize', " +
                                                  "null as 'xtype', f.length as 'size', f.description as 'description', null as 'utype', f.unit as 'unit', " +
                                                  "f.ucd as 'ucd', null as 'indexed', null as 'principal', null as 'std', f.columnID  as 'column_index',  case when o.type='V' then 1 else 0 end as 'is_view' " +
                                                  "from (SELECT name, type FROM DBObjects where (type = 'V' or type = 'U') and name=@name  ) as o cross apply dbo.fDocColumns(o.name) as f ORDER BY o.name, f.columnID";


        public static string allFunctionsData = "select o.name,o.description, o.text,o.rank,f.* from DBObjects as o cross apply fDocFunctionParams(o.name) as f where o.access='U' and o.type='F' order by o.name, f.pnum";
        public static string allFunctionsDescriptions = "select o.name,o.description, o.text,o.rank from DBObjects as o where o.access='U' and o.type='F' order by o.name";
        public static string functionParameters = "select o.name,o.description, o.text,o.rank,f.* from DBObjects as o cross apply fDocFunctionParams(o.name) as f where o.access='U' and o.type='F' and o.name=@name order by o.name, f.pnum";


        public static string allProceduresData = "select o.name,o.description, o.text,o.rank,f.* from DBObjects as o cross apply fDocFunctionParams(o.name) as f where o.access='U' and o.type='P' order by o.name, f.pnum";
        public static string allProceduresDescriptions = "select o.name,o.description, o.text,o.rank from DBObjects as o where o.access='U' and o.type='P' order by o.name";
        public static string proceduresParameters = "select o.name,o.description, o.text,o.rank,f.* from DBObjects as o cross apply fDocFunctionParams(o.name) as f where o.access='U' and o.type='P' and o.name=@name order by o.name, f.pnum";


        public static string allIndexes = "select[tableName] as 'Table Name', case when code = 'I' then 'covering index' else type end as 'Index Type', " +
                                            "case when code = 'F' then[foreignKey] else REPLACE(fieldList, ',', ', ') end as 'Key or Field List' from IndexMap order by[tableName],[indexMapId]";

        public static string indexesForTable = "select case when code = 'I' then 'covering index' else type end as 'Index Type', " +
                                                "case when code = 'F' then[foreignKey] else REPLACE(fieldList, ',', ', ') end as 'Key or Field List' from IndexMap where tableName = @name order by[tableName],[indexMapId]";

        public static string constants_list = "select name, description from DBObjects where name like '%Constants%' or name like '%Defs%'";

        



    }
}