using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sciserver_webService.SDSSFields
{
    public class FieldQueries
    {

        public static string cmdTemplate1 = @"Select 
        r.run, r.rerun, r.camcol, r.field, f.fieldId, r.stripe, r.strip, 
        r.ra, r.dec, r.raMin, r.raMax, r.decMin, r.decMax, 
        r.mu, r.nu, r.incl, r.node,
        r.a, r.b, r.c, r.d, r.e, r.f, 
        f.quality,
        f.a_u, f.b_u, f.c_u, f.d_u, f.e_u, f.f_u,
        f.a_g, f.b_g, f.c_g, f.d_g, f.e_g, f.f_g,
        f.a_r, f.b_r, f.c_r, f.d_r, f.e_r, f.f_r,
        f.a_i, f.b_i, f.c_i, f.d_i, f.e_i, f.f_i,
        f.a_z, f.b_z, f.c_z, f.d_z, f.e_z, f.f_z,
        dbo.fGetUrlFitsCFrame(f.fieldId,'u') as u_url,
        dbo.fGetUrlFitsCFrame(f.fieldId,'g') as g_url,
        dbo.fGetUrlFitsCFrame(f.fieldId,'r') as r_url,
        dbo.fGetUrlFitsCFrame(f.fieldId,'i') as i_url,
        dbo.fGetUrlFitsCFrame(f.fieldId,'z') as z_url
        from dbo.fGetNearbyFrameEq(TEMPLATE,0) n, Frame r, Field f
        where f.fieldId=r.fieldId and r.fieldId=n.fieldId and r.zoom=0";


        public static string cmdTemplate2 = @"select 
        r.run, r.rerun, r.camcol, r.field, f.fieldId, r.stripe, r.strip, 
        r.ra, r.dec, r.raMin, r.raMax, r.decMin, r.decMax, 
        r.mu, r.nu, r.incl, r.node,
        r.a, r.b, r.c, r.d, r.e, r.f, 
        f.quality,
        f.a_u, f.b_u, f.c_u, f.d_u, f.e_u, f.f_u,
        f.a_g, f.b_g, f.c_g, f.d_g, f.e_g, f.f_g,
        f.a_r, f.b_r, f.c_r, f.d_r, f.e_r, f.f_r,
        f.a_i, f.b_i, f.c_i, f.d_i, f.e_i, f.f_i,
        f.a_z, f.b_z, f.c_z, f.d_z, f.e_z, f.f_z,
        dbo.fGetUrlFitsCFrame(f.fieldId,'u') as u_url,
        dbo.fGetUrlFitsCFrame(f.fieldId,'g') as g_url,
        dbo.fGetUrlFitsCFrame(f.fieldId,'r') as r_url,
        dbo.fGetUrlFitsCFrame(f.fieldId,'i') as i_url,
        dbo.fGetUrlFitsCFrame(f.fieldId,'z') as z_url
        from Frame r join Field f on f.fieldId=r.fieldId
        where r.zoom=0";

        public static string ListOfFields = @"select 
        r.run, r.rerun, r.camcol, r.field, f.fieldId,  r.ra, r.dec, r.raMin, r.raMax, r.decMin, r.decMax 
        from dbo.fGetNearbyFrameEq(TEMPLATE,0) n, Frame r, Field f
        where f.fieldId=r.fieldId and r.fieldId=n.fieldId and r.zoom=0 ";

        public static string UrlOfFields = @"select 
        TEMPURL  r.run, r.rerun, r.camcol, r.field,r.stripe, f.fieldId, r.ra, r.dec, r.raMin, r.raMax, r.decMin, r.decMax,  
        r.mu, r.nu, r.incl, r.node,
        from dbo.fGetNearbyFrameEq(TEMPLATE,0) n, Frame r, Field f    
        where f.fieldId=r.fieldId and r.fieldId=n.fieldId and r.zoom=0    ";



        //         <add key="CmdTemplate1" value="select run, rerun, camcol, field from dbo.fGetNearbyObjEq(TEMPLATE) group by run, rerun, camcol, field order by run, rerun, camcol, field" />
        //    <add key="CmdTemplate" value="select 
        //    r.run, r.rerun, r.camcol, r.field, f.fieldId, r.stripe, r.strip, 
        //    r.ra, r.dec, r.raMin, r.raMax, r.decMin, r.decMax, 
        //    r.mu, r.nu, r.incl, r.node,
        //    r.a, r.b, r.c, r.d, r.e, r.f, 
        //    f.quality,
        //    f.a_u, f.b_u, f.c_u, f.d_u, f.e_u, f.f_u,
        //    f.a_g, f.b_g, f.c_g, f.d_g, f.e_g, f.f_g,
        //    f.a_r, f.b_r, f.c_r, f.d_r, f.e_r, f.f_r,
        //    f.a_i, f.b_i, f.c_i, f.d_i, f.e_i, f.f_i,
        //    f.a_z, f.b_z, f.c_z, f.d_z, f.e_z, f.f_z,
        //  dbo.fGetUrlFitsCFrame(f.fieldId,'u') as u_url,
        //    dbo.fGetUrlFitsCFrame(f.fieldId,'g') as g_url,
        //    dbo.fGetUrlFitsCFrame(f.fieldId,'r') as r_url,
        //    dbo.fGetUrlFitsCFrame(f.fieldId,'i') as i_url,
        //    dbo.fGetUrlFitsCFrame(f.fieldId,'z') as z_url
        //  from dbo.fGetNearbyFrameEq(TEMPLATE,0) n, Frame r, Field f
        //  where f.fieldId=r.fieldId and r.fieldId=n.fieldId and r.zoom=0
        //" />
        //    <add key="CmdTemplate2" value="
        //  select 
        //    r.run, r.rerun, r.camcol, r.field, f.fieldId, r.stripe, r.strip, 
        //    r.ra, r.dec, r.raMin, r.raMax, r.decMin, r.decMax, 
        //    r.mu, r.nu, r.incl, r.node,
        //    r.a, r.b, r.c, r.d, r.e, r.f, 
        //    f.quality,
        //    f.a_u, f.b_u, f.c_u, f.d_u, f.e_u, f.f_u,
        //    f.a_g, f.b_g, f.c_g, f.d_g, f.e_g, f.f_g,
        //    f.a_r, f.b_r, f.c_r, f.d_r, f.e_r, f.f_r,
        //    f.a_i, f.b_i, f.c_i, f.d_i, f.e_i, f.f_i,
        //    f.a_z, f.b_z, f.c_z, f.d_z, f.e_z, f.f_z,
        //  dbo.fGetUrlFitsCFrame(f.fieldId,'u') as u_url,
        //    dbo.fGetUrlFitsCFrame(f.fieldId,'g') as g_url,
        //    dbo.fGetUrlFitsCFrame(f.fieldId,'r') as r_url,
        //    dbo.fGetUrlFitsCFrame(f.fieldId,'i') as i_url,
        //    dbo.fGetUrlFitsCFrame(f.fieldId,'z') as z_url
        //  from Frame r join Field f on f.fieldId=r.fieldId
        //  where r.zoom=0
        //" />
        //    <add key="ListOfFields" value="select r.run, r.rerun, r.camcol, r.field, f.fieldId,  r.ra, r.dec, r.raMin, r.raMax, r.decMin, r.decMax from dbo.fGetNearbyFrameEq(TEMPLATE,0) n, Frame r, Field f where f.fieldId=r.fieldId and r.fieldId=n.fieldId and r.zoom=0 " />
        //    <add key="UrlsOfFields" value="select &#xA; TEMPURL  r.run, r.rerun, r.camcol, r.field,r.stripe, f.fieldId, r.ra, r.dec, r.raMin, r.raMax, r.decMin, r.decMax  from dbo.fGetNearbyFrameEq(TEMPLATE,0) n, Frame r, Field f&#xA;    where f.fieldId=r.fieldId and r.fieldId=n.fieldId and r.zoom=0&#xA;    " />

    }
}