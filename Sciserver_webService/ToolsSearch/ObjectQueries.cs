using Sciserver_webService.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sciserver_webService.ToolsSearch
{


    public class PlateQueries
    {
        public static string FiberList = "EXEC spGetFiberList @plateid";

        public static string PlateAPOGEE = "select s.apstar_id, v.fiberID, v.apogee_id from apogeeVisit v " +
                                         "join apogeeStar s on v.apogee_id=s.apogee_id "+
                                         "join apogeePlate p on (p.plate=v.plate and p.mjd=v.mjd ) "+
                                         "where p.plate_visit_id = @apogeeplateid "+
                                         "order by v.fiberid";// note that @apogeeplateid is a string, and has to be checked against sql injection before sending the query.
        public static string PlateMANGA = "select mangaid from mangadrpall where plate = @mangaplate order by mangaid";
    }

    public class SpecQueries
    {
        public static string SpecById = "SELECT img FROM SpecObjAll WHERE specobjID=@specId";
        public static string SpecByPF = "SELECT img FROM SpecObjAll WHERE plateID=@plateId AND fiberID=@fiberId";
    }

    public class FrameQueries
    {
        public static string FrameById = "SELECT run,camCol,field,zoom FROM Frame WHERE fieldID=@fieldId AND zoom=@zoom";
        public static string FrameByRCFZ = "SELECT run,camCol,field,zoom FROM Frame WHERE zoom=@zoom AND run=@run AND camCol=@camcol AND field=@field";
    }

    public class ExploreQueries
    {
        static string commissColumn = KeyWords.ReleaseNumber < 17 ? "a.commiss," : "";

        ///Left Side panel of the Explore Page
        //photoObj
        public static string PhotoObjQuery = "select * from PhotoObjAll where objId=@objId";


        //photoTag
        public static string PhotoTagQuery = "select * from PhotoTag where objId=@objId";


        //Field
        public static string FieldQuery = "select * from Field where fieldId =@fieldId";


        //Frame: this are all columns except for column "img"
        public static string FrameQuery = "select fieldID,zoom,run,rerun,camcol,field,stripe,strip,a,b,c,d,e,f,node,incl,raMin,raMax,decMin,decMax,mu,nu,ra,dec,cx,cy,cz,htmID,'not displayed' as 'img' from Frame where fieldId=@fieldId";
        //public static string FrameQuery = "select * from Frame where fieldId=@fieldId";
        //specObjall
        public static string SpecObjQuery = "select * from SpecObjAll where specObjId=@specId";


        //sppLines
        public static string sppLinesQuery = "select * from sppLines where specObjId=@specId";


        //sppParams
        public static string sppParamsQuery = "select * from sppParams where specObjId=@specId";


        //galspec
        public static string galSpecLineQuery = "select * from galSpecLine where specObjId=@specId";


        //galspecIndex
        public static string galSpecIndexQuery = "select * from galSpecIndx where specObjId=@specId";


        //galspecInfo
        public static string galSpecInfoQuery = "select * from galSpecInfo where specObjId=@specId";


        //stellarMassStarformingPort
        public static string stellarMassStarformingPortQuery = @"select * from stellarMassStarformingPort where specObjId=@specId";

        //stellarMassPassivePort
        public static string stellarMassPassivePortQuery = @"select * from stellarMassPassivePort where specObjId=@specId ";


        //emissionLinesPort
        public static string emissionLinesPortQuery = "select * from emissionLinesPort where specObjId=@specId";


        //stellarMassPCAWiscBC03
        public static string stellarMassPCAWiscBC03Query = "select * from stellarMassPCAWiscBC03 where specObjId=@specId";


        //stellarMassPCAWiscM11
        public static string stellarMassPCAWiscM11Query = "select * from stellarMassPCAWiscM11 where specObjId=@specId";


        //For DR10 and above
        //stellarMassFSPSGranEarlyDust
        public static string stellarMassFSPSGranEarlyDust = "select * from stellarMassFSPSGranEarlyDust where specObjId=@specId";


        //stellarMassFSPSGranEarlyNoDust
        public static string stellarMassFSPSGranEarlyNoDust = "select * from stellarMassFSPSGranEarlyNoDust where specObjId=@specId";


        //stellarMassFSPSGranWideDust
        public static string stellarMassFSPSGranWideDust = "select * from stellarMassFSPSGranWideDust where specObjId=@specId";


        //stellarMassFSPSGranWideDust
        public static string stellarMassFSPSGranWideNoDust = "select * from stellarMassFSPSGranWideNoDust where specObjId=@specId";


        //apogeeStar
        public static string apogeeStar = "select * from apogeeStar where apstar_id=@apid";// note that @apid is a string, and has to be checked against sql injection before sending the query.
        // HttpUtility.UrlEncode("'" + apogeeId + "'");

        //aspcapStar
        public static string aspcapStar = "select * from aspcapStar where apstar_id=@apid";// note that @apid is a string, and has to be checked against sql injection before sending the query.
        //+HttpUtility.UrlEncode("'" + apogeeId + "'");      

        //PhotoZ
        public static string PhotoZ = "select * from Photoz where objid=@objId";
        //string c1 = "select * from Photoz2 where objid=" + objid;       

        //PhotzRF
        //public static string PhotozRF= "select * from PhotozRF where objid=@objId";
        //string c2 = "select * from Photoz2 where objid=" + objid;

        #region plate
        //Plate
        public static string Plate = "select * from PlateX where plateId=@plateId";

        public static string PlateShow = @" select specObjID as specObjId, fiberId, class as name, str(z,5,3) as z 
                                from SpecObjAll where plateID=@plateId order by fiberId";

        #endregion

        #region AllSpectra
        //AllSpec Queries
        public static string AllSpec1 = @"select s.specObjId, s.plate as plate, s.mjd as MJD, s.fiberID as fiber, 
                            str(t.ra,10,5) as ra, str(t.dec,10,5) as dec, str(s.ra,10,5) as specRa, str(s.dec,10,5) as specDec, s.sciencePrimary, 
                            str(dbo.fDistanceArcMinEq(t.ra,t.dec,s.ra,s.dec),10,8) as distanceArcMin, s.class as class  
                            from SpecObjAll s, photoobjall t
                            where t.objid=@objId  and s.bestobjid=t.objid  order by scienceprimary desc, plate, MJD, fiber";


        public static string AllSpec2 = @"select s.specObjId, s.plate as plate, s.mjd as MJD, s.fiberID as fiber, str(t.ra,10,5) as ra, str(t.dec,10,5) as dec, 
                            str(s.ra,10,5) as specRa, str(s.dec,10,5) as specDec,  s.sciencePrimary, 
                            str(dbo.fDistanceArcMinEq(t.ra,t.dec,s.ra,s.dec),10,8) as distanceArcMin, s.class as class  
                            from SpecObjAll s, photoobjall t 
                            where t.objid=@objId  and s.fluxobjid=t.objid order by  MJD, plate, fiber, 
                            scienceprimary desc, distanceArcMin asc";

        #endregion

        #region matches
        ///Matches Queries
        public static string matches1 = @"select dbo.fIAUFromEq(p.ra,p.dec) as 'IAU name', p.objid, p.thingid, dbo.fPhotoModeN(p.mode) as 'mode description'
                                        from Photoobjall p where p.objid=@objId";


        /*        public static string matches2  = @" select t.objid, t.thingid, p.mode, dbo.fPhotoModeN(p.mode) as '(mode description)'
                                                from thingindex t join photoobjall p on t.objid = p.objid 
                                                where t.objid=@objId and p.mode != 1 order by p.mode";
        */
        public static string matches2 = @" select d.objid, d.thingid, p2.mode, dbo.fPhotoModeN(p2.mode) as 'mode description'
                                        from thingIndex t join detectionIndex d on t.thingId=d.thingId join phototag p on t.objid = p.objid join phototag p2 on d.objid = p2.objid 
                                        where t.objid=@objId order by p2.mode";






        #endregion

        #region neighbors
        ///Neighbors
        public static string neighbors1 = @" select dbo.fIAUFromEq(p.ra,p.dec) as 'IAU name', p.objid, p.thingid from photoobjall p where p.objid=@objId";


        public static string neighbors2 = @" select n.neighborObjId as objId,str(t.ra,10,5) as ra, str(t.dec,10,5) as dec, str(n.distance,5,3) as 'distance (arcmin)',
                                dbo.fPhotoTypeN(n.neighborType) as type, neighborMode as mode, dbo.fPhotoModeN(n.neighborMode) as '(mode description)'
                                from Neighbors n, photoobjall t where n.neighborObjid=t.objid and n.objId=@objId order by n.distance asc ";

        #endregion

        #region fits_parameters
        /// Fits Parameters Queries
        public static string fitsParametersSppParams = @" select targetstring as 'Targeting criteria', flag as 'SEGUE flags',spectypehammer as 'HAMMER spectral type', SPECTYPESUBCLASS as 'Spectral subclass',
                        str(elodiervfinal,7,2) as 'Radial velocity (km/s)', str(elodiervfinalerr,8,3) as 'RV error', str(teffadop,6,0) as 'Effective temp (K)', 
                        str(teffadopunc,6,1) as 'Teff error' , str(fehadop,7,2) as '[Fe/H] (dex)', str(fehadopunc,8,3) as '[Fe/H] error', str(loggadop,7,2) as 'log<sub>10</sub>(g) (dex)', 
                        str(loggadopunc,8,3) as 'log<sub>10</sub>(g) error' from sppParams where specObjId=@specId";


        public static string fitsParametersStellarMassStarformingPort = @"  select logMass as 'Best-fit log<sub>10</sub>(stellar mass)',minLogMass as '1-&#963 min', maxLogMass as '1-&#963 max',
                        age as 'Best-fit age (Gyr)', minAge as '1-&#963 min Age', maxAge as '1-&#963 max Age',
                        SFR as 'Best-fit SFR (M<sub>&#9737</sub> / yr)', minSFR as '1-&#963 min SFR', maxSFR as '1-&#963 max SFR' 
                        from stellarMassStarformingPort where specObjId=@specId";

        public static string fitsParameterSstellarMassPassivePort = @" select logMass as 'Best-fit log<sub>10</sub>(stellar mass)', minLogMass as '1-&#963 min', maxLogMass as '1-&#963 max'
                         , age as 'Best-fit age (Gyr)', minAge as '1-&#963 min Age', maxAge as '1-&#963 max Age', SFR as 'Best-fit SFR (M<sub>&#9737</sub> / yr)',
                         minSFR as '1-&#963 min SFR', maxSFR as '1-&#963 max SFR' 
                         from stellarMassPassivePort where specObjId=@specId";

        public static string fitsParametersEmissionLinesPort = @" select velstars as 'Stellar velocity (km/s)',sigmaStars as 'Stellar velocity disperson (km/s)', 
                                                                sigmaStarsErr as 'Velocity dispersion error' ,chisq as 'Chi-squared fit of template',
                                                                bpt as 'BPT classification' from emissionLinesPort where specObjId=@specId";


        public static string fitsParametersStellarMassPCAWiscBC03 = @" select str(mstellar_median,7,2) as 'Best-fit log<sub>10</sub>(stellar mass)',
                                                                    str(mstellar_err,8,3) as 'Error', str(vdisp_median,8,2) as 'Median veldisp (km/s)', str(vdisp_err,9,3) as 'Error VelDisp'
                                                                    from stellarMassPCAWiscBC03 where specObjId=@specId";

        public static string fitsParametersstellarMassPCAWiscM11 = @"select str(mstellar_median,7,2) as 'Best-fit log<sub>10</sub>(stellar mass)', str(mstellar_err,8,3) as 'Error',
                                                                    str(vdisp_median,8,2) as 'Median veldisp (km/s)', str(vdisp_err,9,3) as 'Error VelDisp'
                                                                    from stellarMassPCAWiscM11 where specObjId=@specId";

        public static string fitsParametersStellarmassFSPSGranEarlyDust = @"select str(logmass,7,2) as 'Best-fit log<sub>10</sub>(stellar mass)', str(logmass_err,8,3) as 'Error', 
                                                                          str(age,8,2) as 'Age (Gyr)', 
                                                                          str(ssfr,8,2) as 'SSFR', str(metallicity,8,2) as 'metallicity'
                                                                          from stellarmassFSPSGranEarlyDust where specObjId=@specId";

        public static string fitsParametersStellarmassFSPSGranEarlyNoDust = @"select str(logmass,7,2) as 'Best-fit log<sub>10</sub>(stellar mass)', 
                                                                            str(logmass_err,8,3) as 'Error', str(age,8,2) as 'Age (Gyr)', 
                                                                            str(ssfr,8,2) as 'SSFR', str(metallicity,8,2) as 'metallicity'
                                                                            from stellarmassFSPSGranEarlyNoDust where specObjId=@specId";

        public static string fitsParametersStellarmassFSPSGranWideDust = @"select str(logmass,7,2) as 'Best-fit log<sub>10</sub>(stellar mass)', str(logmass_err,8,3) as 'Error',
                                                                        str(age,8,2) as 'Age (Gyr)', 
                                                                        str(ssfr,8,2) as 'SSFR', str(metallicity,8,2) as 'metallicity'
                                                                        from stellarmassFSPSGranWideDust where specObjId=@specId";

        public static string fitsParametersStellarmassFSPSGranWideNoDust = @"select str(logmass,7,2) as 'Best-fit log<sub>10</sub>(stellar mass)', str(logmass_err,8,3) as 'Error',
                                                                            str(age,8,2) as 'Age (Gyr)', 
                                                                            str(ssfr,8,2) as 'SSFR', str(metallicity,8,2) as 'metallicity'
                                                                            from stellarmassFSPSGranWideNoDust where specObjId=@specId";

        #endregion

        #region galaxyzoo
        //GalaxyZooQueries
        public static string zooSpec = "select * from zooSpec where objid=@objId";


        public static string zooSpec1 = @"select objid,nvote as 'Votes',str(p_el_debiased,5,3) 'Elliptical proabability (debiased)',
                             str(p_cs_debiased,5,3) 'Spiral probability (debiased)'
                             from zooSpec where objid=@objId";

        public static string zooSpec2 = @" select str(p_cw,5,3) as 'Clockwise spiral probability', str(p_acw,5,3) as 'Anticlockwise spiral probability',
                            str(p_edge,5,3) as 'Edge-on spiral probablity', str(p_mg,5,3) as 'Merger system probability'
                            from zooSpec where objid=@objId";

        public static string zooNoSpec = "select  * from  zooNoSpec where objid =@objId";

        public static string galaxyZoo3 = "select objid,nvote,p_el,p_cs  from zooNoSpec where objid=@objId";

        public static string zooConfidence = "select * from zooConfidence where objid=@objId";

        public static string zooConfidence2 = " select objid,f_unclass_clean,f_misclass_clean from zooConfidence where objid=@objId";

        public static string zooMirrorBias = "select * from zooMirrorBias where objid=@objId";

        public static string zooMirrorBias2 = " select objid,nvote_mr1,nvote_mr2 from zooMirrorBias where objid=@objId";

        public static string zooMonochromeBias = "select * from zooMonochromeBias where objid=@objId";

        public static string zooMonochromeBias2 = "select objid,nvote_mon from zooMonochromeBias where objid=@objId";

        public static string zoo2MainSpecz = "select * from zoo2MainSpecz where dr8objid=@objId";

        public static string zoo2MainSpecz2 = @"select dr8objid, total_classifications, total_votes
                                               from zoo2MainSpecz where dr8objid=@objId";

        public static string zoo2MainPhotoz = "select * from zoo2MainPhotoz where dr8objid=@objId";

        public static string zoo2MainPhotoz2 = @"select dr8objid, total_classifications, total_votes
                                               from zoo2MainPhotoz where dr8objid=@objId";

        public static string zoo2Stripe82Normal = "select * from zoo2Stripe82Normal where dr8objid=@objId";

        public static string zoo2Stripe82Normal2 = @"select dr8objid, total_classifications, total_votes
                                                    from zoo2Stripe82Normal where dr8objid=@objId";

        public static string zoo2Stripe82Coadd1 = "select * from zoo2Stripe82Coadd1 where dr8objid=@objId";

        public static string zoo2Stripe82Coadd1_2 = @"select dr8objid, total_classifications, total_votes
                                                    from zoo2Stripe82Coadd1 where dr8objid=@objId";

        public static string zoo2Stripe82Coadd2 = "select * from zoo2Stripe82Coadd2 where dr8objid=@objId";

        public static string zoo2Stripe82Coadd2_2 = @"select dr8objid, total_classifications, total_votes
                                                    from zoo2Stripe82Coadd2 where dr8objid=@objId";
        #endregion

        ///Metadata Queries
        ///
        public static String getObjParamaters = @"select p.ra, p.dec, s.specObjId, p.clean, s.survey, cast(p.mode as int) as mode, 
                                                dbo.fPhotoTypeN(p.type) as otype, p.mjd, p.run, p.rerun, p.camcol, p.field, p.obj, p.objid 
                                                from PhotoObjAll p LEFT OUTER JOIN SpecObjAll s ON s.bestobjid=p.objid AND s.scienceprimary=1
                                                where p.objId= @objId";
        /*
                    public static String getObjParamaters = @"select p.ra, p.dec, s.specObjId, p.clean, s.survey, cast(p.mode as int) as mode, 
                                                        dbo.fPhotoTypeN(p.type) as otype, p.mjd, p.run, p.rerun, p.camcol, p.field, p.obj, p.objid, 
                                    s.mjd as mjd_spec,s.plate,cast(s.plateId as binary(8)) as plateId,s.fiberid,cast(p.fieldId as binary(8)) as fieldId,
                                    str(p.modelMag_u,7,2) as u, str(p.modelMag_g,7,2) as g, str(p.modelMag_r,7,2) as r, str(p.modelMag_i,7,2) as i, str(p.modelMag_z,7,2) as z, 
                                    str(modelMagErr_u,7,2) as err_u, str(modelMagErr_g,7,2) as err_g, str(modelMagErr_r,7,2) as err_r, str(modelMagErr_i,7,2) as err_i, str(modelMagErr_z,7,2) as err_z,
                                    dbo.fPhotoFlagsN(flags) as flags, class as spectralClass, dbo.fPhotoTypeN(p.type) as otype, s.z as redshift 
                                                        from PhotoObjAll p LEFT OUTER JOIN SpecObjAll s ON s.bestobjid=p.objid AND s.scienceprimary=1
                                                        where p.objId= @objId";
        */


        /// Imaing Query
        public static String getImagingQuery = @" select " +
            //--phototag
             "dbo.fPhotoFlagsN(pt.flags) as 'flags',pt.ra, pt.dec, pt.run, pt.rerun, pt.camcol, pt.field, " +
             "cast(pt.fieldId as binary(8)) as fieldId, pt.objId as objId, " +
            //--PhotoObjall
             "pa.clean,  dbo.fPhotoTypeN(pa.type) as otype, " +
             "pa.u as u, pa.g  as g, pa.r as r, pa.i as i, pa.z as z, " +
             "pa.err_u as err_u,  pa.err_g  as err_g,  pa.err_r  as err_r, pa.err_i  as err_i, pa.err_z as err_z, " +
            //-- photoObj
            "dbo.fPhotoModeN(po.mode) as mode,po.mjd as 'mjdNum',  (po.nDetect-1) as 'Other observations', po.parentID, po.nChild, str(po.extinction_r,7,2) as extinction_r," +
            "str(po.petroRad_r,9,2)+' &plusmn '+str(po.petroRadErr_r,10,3) as 'petrorad_r'," +
            //--- photz,photozRF,zoospec 
            "(str(phz.z,7,3)+' &plusmn '+str(phz.zerr,8,4)) as 'photoZ_KD', " +
            //---(str(phzrf.z,7,3)+' &plusmn; '+str(phzrf.zerr,8,4)) as 'photoZ_RF', 
            "case (1*zz.spiral+10*zz.elliptical+100*zz.uncertain) when 1 then 'Spiral' when 10 then 'Elliptical' when 100 then 'Uncertain' else '-' end as 'GalaxyZoo_Morph' " +
            //--all joins
            " from PhotoTag pt  " +
            " left outer join PhotoObj po on po.objid = pt.objid " +
            " left outer join Photoz phz on pt.objid=phz.objid " +
            //---left outer join PhotozRF phzrf on pt.objid=phzrf.objid 
            " left outer join zooSpec zz on pt.objid=zz.objid " +
            " left outer join field f on f.fieldID=pt.fieldID " +
            " left outer join photoobjall pa with (nolock)on  pa.objid = pt.objid " +
            " where pt.objId= @objId";


        /// Spectral parameters
        public static string getSpectroQuery =
                @" select s.plateID, s.plate,s.mjd,fiberid ,s.instrument ,class as 'objclass', z as 'redshift_z', zerr as 'redshift_err' 
            , dbo.fSpecZWarningN(zWarning) as 'redshift_flags',s.survey, s.programname, s.scienceprimary as 'primary', 
            (x.nspec-1) as 'otherspec',s.sourcetype, velDisp as 'veldisp', velDispErr as 'veldisp_err' 
             ,case s.survey 
                  WHEN 'sdss'   THEN (select( 
						                      (case when s.legacy_target1  = 0  then '' else ' legacy_target1: ' + dbo.fPrimtargetN(s.legacy_target1) end )+
							                  (case when s.legacy_target2  = 0  then '' else ' legacy_target2: ' + dbo.fPrimTargetN(s.legacy_target2) end )+
							                  (case when s.special_target1 = 0  then '' else ' special_target1: '+ dbo.fSpecialTarget1N(s.special_target1) end )))
                  WHEN 'boss'   THEN (select( 
                                              (case when boss_target1 = 0      then '' else ' boss_target1: '     + dbo.fBossTarget1N(boss_target1) end ) + 
							                  (case when ancillary_target1 = 0 then '' else ' ancillary_target1: '+ dbo.fAncillaryTarget1N(ancillary_target1) end)+
							                  (case when ancillary_target2 = 0 then '' else ' ancillary_target2: '+ dbo.fAncillaryTarget2N(ancillary_target2) end ) ))
                  WHEN 'segue1' THEN (select(
                                              (case when segue1_target1 = 0    then '' else ' segue1_target1: '   + dbo.fSEGUE1target1N(segue1_target1) end) +
							                  (case when segue1_target2 = 0    then '' else ' segue1_target2: '   + dbo.fSEGUE1target2N(segue1_target2) end )))
                  WHEN 'segue2' THEN (select (
							                  (case when segue2_target1 = 0   then '' else ' segue2_target1: ' + dbo.fSEGUE2target1N(segue2_target1) end ) + 
							                  (case when segue2_target2 = 0   then '' else ' segue2_target2: ' + dbo.fSEGUE2target2N(segue2_target2) end )))
             ELSE ' No Data ' 
             END 
             as 'targeting_flags', s.run2d 
             from  PlateX p ,SpecObjAll s 
             join (select bestobjid, count(*) as nspec from specobjall where bestobjid=@objId
             group by bestobjid) x on s.bestobjid=x.bestobjid  where p.plateId=s.plateId and  s.specObjId=@specId";
        //iQuery += " --WHEN 'apogee' THEN (select apogee_target1,apogee_target2 ) ";

        #region cross_id

        public static string USNO = @" select 'USNO' as Catalog, str(10*propermotion,6,2)+' &plusmn; '+str(sqrt(power(muraerr,2)+power(mudecerr,2)),8,3) as 'Proper motion (mas/yr)',
                             angle as 'PM angle (deg E)' from USNO where objId=@objId";

        public static string FIRST = @" select 'FIRST' as Catalog, str(peak,8,2)+' &plusmn; '+str(rms,8,2) as 'Peak flux (mJy)', major as 'Major axis (arcsec)', 
                              minor as 'Minor axis (arcsec)' from First where objId=@objId";


        public static string ROSAT = " select 'ROSAT' as Catalog, cps, hr1, hr2, ext from ROSAT where objId=@objId";

        public static string RC3 = @" select 'RC3' as Catalog, hubble as 'Hubble type', str(m21,5,2)+' &plusmn; '+str(m21err,6,3) as '21 cm magnitude', 
                               hi as 'Neutral Hydrogen Index' from RC3 where objId=@objId";

        public static string WISE = @" select 'WISE' as Catalog,w.w1mag,w.w2mag,w.w3mag,w.w4mag,'link' as 'Full WISE data' 
                            from WISE_xmatch x join WISE_allsky as w WITH(INDEX(pk_WISE_allsky_cntr)) on x.wise_cntr=w.cntr where x.sdss_objid=@objId";
        //cmd = cmd.Replace("@wiselink", wiseLinkCrossId);


        public static string TWOMASS = " select '2MASS' as Catalog, j as 'J', h as 'H', k as 'K_s', phQual from TwoMASS where objId=@objId";

        public static string wiseLinkCrossId = @"select * from wise_xmatch x join wise_allsky a on x.wise_cntr=a.cntr 
                                                where x.sdss_objid=@objId";

        #endregion

        #region Apogee_Queries

        public static string APOGEE_BASE_QUERY = @" select   a.ra,    a.dec,   a.apstar_id,    a.apogee_id,    a.glon,    a.glat,  a.location_id, " + commissColumn +  @" a.vhelio_avg,    a.vscatter,     b.teff,
                    b.teff_err,   b.logg,    b.logg_err,  b.param_m_h,    b.param_m_h_err,     b.param_alpha_m,    b.param_alpha_m_err, c.j,   c.h,   c.k,   c.j_err,   c.h_err,   c.k_err, 
                    case c.src_4_5      when 'none' then NULL      when 'WISE' then c.wise_4_5      when 'IRAC' then c.irac_4_5      end      as mag_4_5,   case c.src_4_5     
                    when 'none' then NULL      when 'WISE' then c.wise_4_5_err      when 'IRAC' then c.irac_4_5_err      end      as mag_4_5_err,   c.src_4_5,  
                    dbo.fApogeeTarget1N(a.apogee_target1) as apogeeTarget1N,   dbo.fApogeeTarget2N(a.apogee_target2) as apogeeTarget2N, 
                    dbo.fApogeeStarFlagN(a.starflag) as apogeeStarFlagN,   dbo.fApogeeAspcapFlagN(aspcapflag) as apogeeAspcapFlagN  
                    from apogeeStar a left join aspcapStar b on a.apstar_id = b.apstar_id left join apogeeObject c on a.apogee_id = c.apogee_id ";

        public static string APOGEE_BASE_QUERY_DR13 = @" select   a.ra,    a.dec,   a.apstar_id,    a.apogee_id,    a.glon,    a.glat,    a.location_id, " + commissColumn + @"  a.vhelio_avg,    a.vscatter,     b.teff,
                    b.teff_err,   b.logg,    b.logg_err,  b.m_h as 'param_m_h',   b.m_h_err as 'param_m_h_err',     b.alpha_m as 'param_alpha_m', b.alpha_m_err as 'param_alpha_m_err', c.j,   c.h,   c.k,   c.j_err,   c.h_err,   c.k_err, 
                    case c.src_4_5      when 'none' then NULL      when 'WISE' then c.wise_4_5      when 'IRAC' then c.irac_4_5      end      as mag_4_5,   case c.src_4_5     
                    when 'none' then NULL      when 'WISE' then c.wise_4_5_err      when 'IRAC' then c.irac_4_5_err      end      as mag_4_5_err,   c.src_4_5,  
                    dbo.fApogeeTarget1N(a.apogee_target1) as apogeeTarget1N,   dbo.fApogeeTarget2N(a.apogee_target2) as apogeeTarget2N, 
                    dbo.fApogeeStarFlagN(a.starflag) as apogeeStarFlagN,   dbo.fApogeeAspcapFlagN(aspcapflag) as apogeeAspcapFlagN, a.field as 'field_name', a.[file] 
                    from apogeeStar a left join aspcapStar b on a.apstar_id = b.apstar_id left join apogeeObject c on a.apogee_id = c.apogee_id left join apogeeField f on f.location_id = a.location_id ";

        public static string APOGEEVISITS_BASE_QUERY = "select visit_id, plate,  mjd, fiberid, dateobs, vrel, apred_version, [file]  from apogeeVisit where apogee_id = @id order by dateobs";


        #endregion


        #region Summary.aspx

        public static string getObjIDFromPlatefiberMjd = @" select p.objId as objId, s.specObjId as specObjId
                            ,p.ra,p.dec
                            from SpecObjAll s JOIN PhotoTag p ON s.bestobjid=p.objid JOIN PlateX q ON s.plateId=q.plateId
                            where s.mjd = @mjd and s.fiberId = @fiberId  and q.plate = @plate";


        public static string getAPOGEEId_PlateFiberMjd = @" select s.apstar_id
                         from apogeeVisit v JOIN apogeeStar s ON s.apogee_id=v.apogee_id
                         where v.plate = @plate  and v.mjd = @mjd  and v.fiberID = @fiberId";



        public static string getApogeeFromEq = @" select top 1 p.apstar_id
                                               from apogeeStar p, dbo.fGetNearestApogeeStarEq(@qra , @qdec , @searchRadius) n 
                                               where p.apstar_id=n.apstar_id";

        public static string getMangaFromMangaId = @"select plateIFU,mangaid,objra,objdec,ifura,ifudec,drp3qual,bluesn2,redsn2,mjdmax,mngtarg1,mngtarg2,mngtarg3,
                                                            htmID,plate,ifudsgn,versdrp3,srvymode from mangaDrpAll where mangaid=@mangaId";


        public static string getMangaFromEq = @" select f.*,m.plate,m.ifudsgn,m.versdrp3,m.srvymode from dbo.fGetNearbyMangaObjEq(@qra , @qdec , @searchRadius) f 
                                                join mangaDrpAll m on m.plateifu = f.plateIFU 
                                                where (dbo.fDistanceArcMinEq(@qra , @qdec ,m.ifura,m.ifudec) <= 32.0/2.0/60.0 and LEFT(m.ifudsgn,DATALENGTH(m.ifudsgn)-2) = 127)
                                                       or (dbo.fDistanceArcMinEq(@qra , @qdec ,m.ifura,m.ifudec) <= 27.0/2.0/60.0 and LEFT(m.ifudsgn,DATALENGTH(m.ifudsgn)-2) = 91)
                                                       or (dbo.fDistanceArcMinEq(@qra , @qdec ,m.ifura,m.ifudec) <= 22.0/2.0/60.0 and LEFT(m.ifudsgn,DATALENGTH(m.ifudsgn)-2) = 61)
                                                       or (dbo.fDistanceArcMinEq(@qra , @qdec,m.ifura,m.ifudec) <= 17.0/2.0/60.0 and LEFT(m.ifudsgn,DATALENGTH(m.ifudsgn)-2) = 37)
                                                       or (dbo.fDistanceArcMinEq(@qra , @qdec ,m.ifura,m.ifudec) <= 12.0/2.0/60.0 and LEFT(m.ifudsgn,DATALENGTH(m.ifudsgn)-2) = 19)
                                                       or (dbo.fDistanceArcMinEq(@qra , @qdec ,m.ifura,m.ifudec) <= 7.0/2.0/60.0 and LEFT(m.ifudsgn,DATALENGTH(m.ifudsgn)-2) =  7)
                                                       or (m.ifudesignsize <  0) order by f.redsn2 desc";


        public static string getMastarFromEq = @"select distinct gv.plate,gv.ifudesign,gv.mjd,gv.nexp,gv.heliov,gv.verr,gv.v_errcode,gv.mjdqual,
                                                                 gs.mangaid,gs.objra,gs.objdec,gs.catalogra,gs.catalogdec,gs.nvisits,gs.nplates,
                                                                 gs.photocat,gs.cat_epoch,gs.mngtarg2,gs.minmjd,gs.maxmjd,
                                                                 gs.psfmag_1,gs.psfmag_2,gs.psfmag_3,gs.psfmag_4,gs.psfmag_5,
                                                                 gs.input_logg,gs.input_teff,gs.input_fe_h,gs.input_alpha_m,gs.input_source
                                                 from dbo.fGetNearbyMastarObjEq(@qra , @qdec , @searchRadius) f
                                                 join mastar_goodstars as gs on f.mangaid=gs.mangaid
                                                 join mastar_goodvisits as gv on f.mangaid=gv.mangaid order by gs.mangaid";

        public static string getMastarFromEqDR17 = @"select distinct gv.plate,gv.ifudesign,gv.mjd,gv.nexp_visit,gv.heliov,gv.verr,gv.v_errcode,gv.mjdqual,
                                                                 gs.mangaid,gs.ra,gs.dec,gs.nvisits,gs.nplates,
                                                                 gs.photocat,gs.epoch,gs.mngtarg2,gs.minmjd,gs.maxmjd,
                                                                 gs.psfmag_1,gs.psfmag_2,gs.psfmag_3,gs.psfmag_4,gs.psfmag_5,
                                                                 gs.input_logg,gs.input_teff,gs.input_fe_h,gs.input_alpha_m,gs.input_source
                                                 from dbo.fGetNearbyMastarObjEq(@qra , @qdec , @searchRadius) f
                                                 join mastar_goodstars as gs on f.mangaid=gs.mangaid
                                                 join mastar_goodvisits as gv on f.mangaid=gv.mangaid order by gs.mangaid";

        public static string getMastarFromMangaId = @"select     gv.plate,gv.ifudesign,gv.mjd,gv.nexp,gv.heliov,gv.verr,gv.v_errcode,gv.mjdqual,
                                                                 gs.mangaid,gs.objra,gs.objdec,gs.catalogra,gs.catalogdec,gs.nvisits,gs.nplates,
                                                                 gs.photocat,gs.cat_epoch,gs.mngtarg2,gs.minmjd,gs.maxmjd,
                                                                 gs.psfmag_1,gs.psfmag_2,gs.psfmag_3,gs.psfmag_4,gs.psfmag_5,
                                                                 gs.input_logg,gs.input_teff,gs.input_fe_h,gs.input_alpha_m,gs.input_source
                                                    from mastar_goodstars gs join mastar_goodvisits as gv on gs.mangaid=gv.mangaid where gs.mangaid=@mangaId  order by gs.mangaid";

        public static string getMastarFromMangaIdDR17 = @"select     gv.plate,gv.ifudesign,gv.mjd,gv.nexp_visit,gv.heliov,gv.verr,gv.v_errcode,gv.mjdqual,
                                                                 gs.mangaid,gs.ra,gs.dec,gs.nvisits,gs.nplates,
                                                                 gs.photocat,gs.epoch,gs.mngtarg2,gs.minmjd,gs.maxmjd,
                                                                 gs.psfmag_1,gs.psfmag_2,gs.psfmag_3,gs.psfmag_4,gs.psfmag_5,
                                                                 gs.input_logg,gs.input_teff,gs.input_fe_h,gs.input_alpha_m,gs.input_source
                                                    from mastar_goodstars gs join mastar_goodvisits as gv on gs.mangaid=gv.mangaid where gs.mangaid=@mangaId  order by gs.mangaid";


        public static string getPhotoFromEq = @" select top 1 p.objId as objId, p.specObjId as specObjId
                                             from PhotoTag p, dbo.fGetNearbyObjAllEq(@qra , @qdec , @searchRadius) n 
                                             where p.objId=n.objId order by n.mode asc, n.distance asc";



        public static string getpmtsFromEq = @" select top 1 p.objId as objId, p.specObjId as specObjId, p.ra ,p.dec, p.run, p.rerun, p.camcol,p.field,cast(p.fieldId as binary(8)) as fieldid  
                            from PhotoTag p, dbo.fGetNearbyObjAllEq(@qra , @qdec , @searchRadius) n
                            where p.objId=n.objId order by n.mode asc, n.distance asc";

        public static string getSkyversion = "select top 1 skyversion from run";

        public static string getpmtsFromSpecWithApogeeId = @" select st.apstar_id, st.ra, st.dec
                                                            from apogeeStar st
                                                            where st.@whatdoiget = @sid";


        public static string getpmtsFromSpecWithSpecobjID = @" select p.ra, p.dec,
                    cast(p.fieldId as binary(8)) as fieldId,
                    s.specObjId as specObjId,
                    p.objId as objId,
                    s.plateId as plateId, s.mjd, s.fiberId, q.plate
                    from SpecObjAll s JOIN PhotoTag p ON s.bestobjId=p.objid JOIN PlateX q ON s.plateId=q.plateId
                    where s.specObjId= @sid";


        public static string getpmtsFromPhoto = @" select p.ra, p.dec, p.run, p.rerun, p.camcol, p.field,
                     cast(p.fieldId as binary(8)) as fieldId,
                     s.specobjid as specObjId,
                     p.objId as objId 
                     from PhotoTag p 
                     left outer join SpecObjAll s ON s.bestobjid=p.objid AND s.scienceprimary=1
                     where p.objId=dbo.fObjId(@objid)";


        public static string getpmtsFrom5PartSDSS = @"select p.ra, p.dec, p.run, p.rerun, p.camcol, p.field,
                     cast(p.fieldId as binary(8)) as fieldId,
                     s.specobjid as specObjId,
                     p.objId as objId 
                     from PhotoTag p 
                     left outer join SpecObjAll s ON s.bestobjid=p.objid AND s.scienceprimary=1
                     where p.objId=dbo.fObjidFromSDSS(@skyversion,@run,@rerun,@camcol,@field,@obj)";

        public static string getPlateFiberFromSpecObj = @"select s.plateId as plateId, s.mjd, s.fiberId, q.plate 
                           from SpecObjAll s JOIN PlateX q ON s.plateId=q.plateId 
                           where specObjId=@specId";


        public static string getParseApogeeId = "select apstar_id from apogeeStar where apstar_id= @id";


        public static string getUnit = "select unit from DBcolumns where tablename='@tablename' and [name]='@name'";


        public static string getSpec = "select specobjid,survey from specobjall where specobjid= @specId";


        public static string getApogee = @" select apstar_id, ra, dec, apogee_id, glon, glat," + commissColumn + @" location_id 
                                            from apogeeStar
                                            where apstar_id=@apogeeId";// note that @apogeeId is a string, and has to be checked against sql injection before sending the query.

        public static string getApogee2 = @" select apstar_id, ra, dec, apogee_id, glon, glat," + commissColumn + @" location_id
                                            from apogeeStar
                                            where apogee_id=@apogeeId";// note that @apogeeId is a string, and has to be checked against sql injection before sending the query.

        public static string getManga = @"select top 1 objra as ra, objdec as dec from mangadrpall where mangaid=@mangaId order by redsn2 desc";
        public static string getMastar = @"select top 1 objra as ra, objdec as dec from mastar_goodstars where mangaid=@mangaId";
        public static string getMastarDR17 = @"select top 1 ra, dec from mastar_goodstars where mangaid=@mangaId";


        public static string getPlateFromApogee = @"select  top 1   a.ra,    a.dec,   a.apstar_id,    a.apogee_id, v.plate,v.fiberId,v.mjd 
                            from apogeeStar a join apogeeVisit v on a.apogee_id=v.apogee_id 
                            where a.apstar_id = '@apogeeId'";

        #endregion

        public static string fitsimg = @"select
                 dbo.fGetUrlFitsCFrame(@fieldId,'u'),
                 dbo.fGetUrlFitsCFrame(@fieldId,'g'),
                 dbo.fGetUrlFitsCFrame(@fieldId,'r'),
                 dbo.fGetUrlFitsCFrame(@fieldId,'i'),
                 dbo.fGetUrlFitsCFrame(@fieldId,'z'),
                 dbo.fGetUrlFitsBin(@fieldId,'u'),
                 dbo.fGetUrlFitsBin(@fieldId,'g'),
                 dbo.fGetUrlFitsBin(@fieldId,'r'),
                 dbo.fGetUrlFitsBin(@fieldId,'i'),
                 dbo.fGetUrlFitsBin(@fieldId,'z'),
                 dbo.fGetUrlFitsMask(@fieldId,'u'),
                 dbo.fGetUrlFitsMask(@fieldId,'g'),
                 dbo.fGetUrlFitsMask(@fieldId,'r'),
                 dbo.fGetUrlFitsMask(@fieldId,'i'),
                 dbo.fGetUrlFitsMask(@fieldId,'z'),
                 dbo.fGetUrlFitsAtlas(@fieldId),
                 dbo.fGetUrlFitsField(@fieldId)";



        public static string fitsspec = "select dbo.fGetUrlFitsSpectrum(@specObjId)";




        public static string unitQuery = @"SELECT name, unit,tablename FROM DBcolumns
                                                WHERE tablename in('PhotoObjAll') 
                                                AND name IN ('u', 'g', 'r', 'i', 'z', 'err_u','err_g','err_r','err_i','err_z',
                                                'mjd', 'mode', 'nDetect', 'parentID','nChild','extinction_r','petroRad_r')
                                                ORDER BY name";



    }


        // these queries are the same as those in Explore.ExplorerQueries. We could inherit those in here.
        //public class QuickLookQueries: Explore.ExplorerQueries
    public class QuickLookQueries
    {
        static string commissColumn = KeyWords.ReleaseNumber < 17 ? "a.commiss," : "";

        public static string QueryHeader = @"select p.objId as objId,s.specObjId as specObjId " +
                        ",p.ra,p.dec," +
                        "s.mjd,s.plate,plateId as plateId,s.fiberid,cast(p.fieldId as binary(8)) as fieldId,p.run,p.rerun,p.camcol,p.field," +
                        "str(p.modelMag_u,7,2) as u, str(p.modelMag_g,7,2) as g, str(p.modelMag_r,7,2) as r, str(p.modelMag_i,7,2) as i, str(p.modelMag_z,7,2) as z," +
                        "str(modelMagErr_u,7,2) as err_u, str(modelMagErr_g,7,2) as err_g, str(modelMagErr_r,7,2) as err_r, str(modelMagErr_i,7,2) as err_i, str(modelMagErr_z,7,2) as err_z," +
                        "dbo.fPhotoFlagsN(flags) as flags, class as spectralClass, dbo.fPhotoTypeN(p.type) as otype, s.z as redshift, s.run2d ";


        public static string getParamsFromPlateFiberMjd = QueryHeader + " from PhotoTag p JOIN SpecObjAll s ON s.bestobjid=p.objid where s.mjd = @mjd and s.fiberId = @fiberId  and s.plate = @plate";


        public static string getParamsFromObjID = QueryHeader + " from PhotoTag p LEFT JOIN SpecObjAll s ON (s.bestobjid=p.objid AND s.scienceprimary=1) where p.objId=dbo.fObjId(@objid)";

        //public static string getpmtsFrom5PartSDSS = @"declare @skyversion int;select top 1 @skyversion=skyversion from run;" + QueryHeader + " from PhotoTag p LEFT JOIN SpecObjAll s ON (s.bestobjid=p.objid AND s.scienceprimary=1) where p.objId=dbo.fObjidFromSDSS(@skyversion,@run,@rerun,@camcol,@field,@obj);";
        public static string getpmtsFrom5PartSDSS = QueryHeader + " from PhotoTag p LEFT JOIN SpecObjAll s ON (s.bestobjid=p.objid AND s.scienceprimary=1) where p.objId=dbo.fObjidFromSDSS(@skyversion,@run,@rerun,@camcol,@field,@obj)";

        public static string getSkyversion = "select top 1 skyversion from run";



        public static string getParamsFromSpecObjID = QueryHeader + " from PhotoTag p JOIN SpecObjAll s ON (s.bestobjid=p.objid AND s.scienceprimary=1) where s.specobjid=@sid";

        public static string getObjIDFromEq = @" select top 1 p.objId from PhotoTag p join dbo.fGetNearbyObjAllEq(@qra , @qdec , @searchRadius) n on (p.objId=n.objId) order by n.mode asc, n.distance asc";

        public static string getParamsFromEq = QueryHeader + " from PhotoTag p JOIN SpecObjAll s ON s.bestobjid=p.objid, dbo.fGetNearbyObjAllEq(@qra , @qdec , @searchRadius) n where p.objId=n.objId order by n.mode asc, n.distance asc";



        ///This is for QuickLook. This query gets the magnitudes (and some other misc) of an object, given the objID. Query returns only strings.
        public static String getMagnitudesFromObjID = @" select
                                                         from PhotoObjAll p LEFT OUTER JOIN SpecObjAll s ON (s.bestobjid=p.objid AND s.scienceprimary=1) where p.ObjID=@objId";



        public static string getApogee = @" select apstar_id, ra, dec, apogee_id, glon, glat," + commissColumn + @" location_id
                                            from apogeeStar
                                            where apstar_id='@apogeeId'";

        public static string getApogee2 = @" select apstar_id, ra, dec, apogee_id, glon, glat," + commissColumn + @" location_id
                                            from apogeeStar
                                            where apogee_id='@apogeeId'";

    }




}