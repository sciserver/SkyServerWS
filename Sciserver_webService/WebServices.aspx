<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="WebServices.aspx.cs" Inherits="Sciserver_webService.WebServices" %>

<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8">
    <meta http-equiv="X-UA-Compatible" content="IE=edge">
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <title>SciServer WebServices</title>
    <link href="assets/bootstrap/dist/css/bootstrap.css" rel="stylesheet">
	<meta charset="utf-8" />
    <!-- HTML5 shim and Respond.js for IE8 support of HTML5 elements and media queries -->
    <!-- WARNING: Respond.js doesn't work if you view the page via file:// -->
    <!--[if lt IE 9]>
      <script src="https://oss.maxcdn.com/html5shiv/3.7.2/html5shiv.min.js"></script>
      <script src="https://oss.maxcdn.com/respond/1.4.2/respond.min.js"></script>
    <![endif]-->
</head>
<body>
	<main>
	<div class="logo"></div>
	<header>WebServices</header>
	
	<nav>
	<ul>
	<li><a href="#conesearch">ConeSearch</a></li>
	<li><a href="#imgcutout">ImgCutout</a></li>
	<li><a href="#stap">SIAP</a></li>
	<li><a href="#sdssfields">SDSSFields</a></li>
	<li><a href="#searchtools">SearchTools</a></li>
	<li><a href="#imagingquery">ImagingQuery</a></li>
	<li><a href="#spectroquery">SpectroQuery</a></li>
	<li><a href="#irspectra">IRSpectra</a></li>
	</ul>
	</nav>
		
	<section>
		<article>
			<header id="conesearch">ConeSearch</header>
			<main>
				<details>This service returns the Search results for given RA, DEC, SR values from Sloan Digital Sky Survey(SDSS).
				</details>
				<section>
				<article>
					<header>Service</header>
						<p><a target="_blank" href="<%=baseurl%>/ConeSearch/ConeSearchService?ra=145&dec=34&sr=1"><%=baseurl%>/ConeSearch/ConeSearchService?ra=145&dec=34&sr=1</a></p>
				</article>
				<article>
					<header>Parameters</header>
					<p>Required</p>
						<dl><dt>ra</dt><dd>Right Ascension.</dd>
						<dt>dec</dt><dd>Declination</dd>
						<dt>sr</dt><dd>Search Radius</dd>
						</dl>
				</article>
			</section>
			</main>
		</article>

		<article>
			<header id="imgcutout">ImgCutout</header>
			<main>
				<details>This service returns image cutout of a sky from the data observed by Sloan Digital Sky Survey(SDSS). Inputs should be ra,dec, width, height, scale along with some optional parameters.
				</details>
				<section>
				<article>
					<header>getJpeg: to get the cutout of given piece of sky from SDSS</header>
						<p><a target="_blank" href="<%=baseurl%>/ImgCutout/getjpeg?ra=134&dec=32&scale=0.7&width=512&height=512"><%=baseurl%>/ImgCutout/getjpeg?ra=134&dec=32&scale=0.7&width=512&height=512</a></p>
				</article>
				<article>
					<header>Parameters</header>
						<p>Required</p>
							<dl>
							<dt>ra</dt><dd>Right Ascension.</dd>
							<dt>dec</dt><dd>Declination</dd>
							<dt>scale</dt><dd>Image Scale in radians</dd>
							<dt>width</dt><dd>in pixels</dd>
							<dt>height</dt><dd>in pixels</dd>
							</dl>
					</dl>
				</article>
			</section>
			<section>
				<article>
					<header>getJpegCodec : to get the field in given run , complete image</header>
						<p><a target="_blank" href="<%=baseurl%>/ImgCutout/getjpegCodec?R=94&C=1&F=11&Z=50"><%=baseurl%>/ImgCutout/getjpegCodec?R=94&C=1&F=11&Z=50</a></p>
				</article>
				<article>
					<header>Parameters</header>
					<p>Required</p>
					<dl>
							<dt>run(R)</dt><dd>SDSS 'run' number</dd>
							<dt>camcol(C)</dt><dd>SDSS 'camcol' number</dd>
							<dt>field(F)</dt><dd>SDSS 'field' number</dd>
							<dt>zoom(Z)</dt><dd>SDSS 'zoom' number</dd>
					</dl>
				</article>
			</section>
			</main>
		</article>

		<article>
			<header id="stap">SIAP</header>
			<main>
				<details>This service returns positional information and image urls for the given set of input parametes and its divided in three sub services. It implements IVOA's Simple Image Access Protocol(SIAP).
				</details>
				<section>
				<article>
					<header>getSIAP</header>
						<p><a target="_blank" href="<%=baseurl%>/SIAP/getSIAP?POS=132,12&SIZE=0.1&FORMAT=image/jpeg"><%=baseurl%>/SIAP/getSIAP?POS=132,12&SIZE=0.1&FORMAT=image/jpeg</a></p>
				</article>
				<article>
					<header>Parameters</header>
					<p>Required</p>
					<dl>
							<dt>pos</dt><dd>Postion in terms of (ra,dec)</dd>
							<dt>format</dt><dd>output format</dd>
							<dt>size</dt><dd>size of the output</dd>
					</dl>
				</article>
			</section>
			<section>
				<article>
					<header>GetSIAPInfo</header>
						<p><a target="_blank" href="<%=baseurl%>/SIAP/getSIAPinfo?POS=132,12&SIZE=0.1&FORMAT=metadata&bandpass=i"><%=baseurl%>/SIAP/getSIAPinfo?POS=132,12&SIZE=0.1&FORMAT=metadata&bandpass=i</a></p>
				</article>
				<article>
					<header>Parameters</header>
					<p>Required</p>
					<dl>
						<dt>pos</dt><dd>Postion in terms of (ra,dec)</dd>
						<dt>size</dt><dd>Size of the query</dd>
						<dt>format</dt><dd>Output format</dd>
						<dt>bandpass</dt><dd>bandpass filter for SDSS</dd>
					</dl>
				</article>
			</section>
			<section>
				<article>
					<header>getAllSIAPInfo</header>
						<p><a target="_blank" href="<%=baseurl%>/SIAP/getSIAPinfoAll?POS=132,12&SIZE=0.01"><%=baseurl%>/SIAP/getAllSIAPinfo?POS=132,12&SIZE=0.01</a></p>
					</article>
					<article>
					<header>Parameters</header>
					<p>Required</p>
					<dl>
						<dt>pos</dt><dd>Position in terms of (ra,dec)</dd>
						<dt>size</dt><dd>Size of the output</dd>
					</dl>
				</article>
			</section>
			</main>
		</article>

		<article>
			<header id="sdssfields">SDSSFields</header>
			<main>
				<details>This service returns SDSS fields information. There are four sub web services.
				</details>
				<section>
				<article>
					<header>FieldArray</header>
						<p><a target="_blank" href="<%=baseurl%>/SDSSFields/FieldArray?ra=132&dec=12&radius=10&format=json"><%=baseurl%>/SDSSFields/FieldArray?ra=132&dec=12&radius=10&format=json</a></p>
				</article>
				<article>
					<header>Parameters</header>
						<p>Required</p>
					<dl>
						<dt>ra</dt><dd>Right Ascension.</dd>
						<dt>dec</dt><dd>Declination</dd>
						<dt>radius</dt><dd>Search Radius</dd>
					</dl>
				</article>
			</section>
			<section>
				<article>
					<header>FieldArrayRect</header>
						<p><a target="_blank" href="<%=baseurl%>/SDSSFields/FieldArrayRect?ra=132&dec=12&dra=0.1&ddec=0.1&format=json"><%=baseurl%>/SDSSFields/FieldArrayRect?ra=132&dec=12&dra=0.1&ddec=0.1&format=json</a></p>
				</article>
				<article>
					<header>Parameters</header>
					<p>Required</p>
					<dl>
							<dt><dt>ra</dt><dd>Right Ascension.</dd>
						    <dt>dec</dt><dd>Declination</dd>
							<dt>dra</dt><dd>ra limit for rectangle</dd>
							<dt>ddec</dt><dd>dec limit for rectangle</dd>
					</dl>
				</article>
			</section>
			<section>
				<article>
					<header>ListOfFields</header>
						<p><a target="_blank" href="<%=baseurl%>/SDSSFields/ListOfFields?ra=132&dec=12&radius=10"><%=baseurl%>/SDSSFields/ListOfFields?ra=132&dec=12&radius=10</a></p>
				</article>
				<article>
					<header>Parameters</header>
					<p>Required</p>
					<dl>
						<dt>ra</dt><dd>Right Ascension.</dd>
						<dt>dec</dt><dd>Declination</dd>
						<dt>radius</dt><dd>Search Radius</dd>
					</dl>
					<p>Optional</p>
					<dl>
						<dt>format</dt><dd>output format</dd>
					</dl>
				</article>
			</section>
			<section>
				<article>
					<header>UrlsOfFields</header>
						<p><a target="_blank" href="<%=baseurl%>/SDSSFields/UrlsOfFields?ra=132&dec=12&radius=10&band=i,z&format=json"><%=baseurl%>/SDSSFields/UrlsOfFields?ra=132&dec=12&radius=10&band=i,z&format=json</a></p>
				</article>
				<article>
					<header>Parameters</header>
					<p>Required</p>
					<dl>
							<dt>ra</dt><dd>Right Ascension.</dd>
						    <dt>dec</dt><dd>Declination</dd>
						    <dt>radius</dt><dd>Search Radius</dd>
							<dt>band</dt><dd>bandpass filter</dd>
					</dl>
					<p>Optional</p>
					<dl>
						<dt>format [Optional]</dt><dd>output format</dd>
					</dl>
				</article>
			</section>
			</main>
		</article>

		<article>
			<header id="searchtools">SearchTools</header>
			<main>
				<details>There are three web services under and all these represent 'Search Tools' on skyserver web site.
				</details>
				<section>
				<article>
				<header>SQL search</header>
					<p><a target="_blank" href="<%=baseurl%>/SearchTools/SqlSearch?cmd=select top 10 ra,dec from Frame&format=csv"><%=baseurl%>/SearchTools/SqlSearch?cmd=select top 10 ra,dec from Frame&format=csv</a></p>
				</article>
				<article>
					<header>Parameters</header>
					<p>Required</p>
					<dl>
						<dt>cmd</dt><dd>SQL query to run</dd>
						<dt>format</dt><dd>output format</dd>
					</dl>
				</article>
			</section>
			<section>
				<article>
					<header>Radial Search</header>
						<p><a target="_blank" href"<%=baseurl%>/SearchTools/RadialSearch?ra=258.2&dec=64&radius=4.1&whichway=equitorial&limit=10&format=json&fp=none&uband=0,17&gband=0,15&whichquery=imaging"><%=baseurl%>/SearchTools/RadialSearch?ra=258.2&dec=64&radius=4.1&whichway=equitorial&limit=10&format=json&fp=none&uband=0,17&gband=0,15&whichquery=imaging</a></p>
					</article>
					<article>
					<header>Parameters</header>
					<p>Required</p>
					<dl>
							<dt>ra</dt><dd>Right Ascension.</dd>
						    <dt>dec</dt><dd>Declination</dd>
						    <dt>radius</dt><dd>Search Radius</dd>
							<dt>whichway</dt><dd>search type , e.g. Equitorial</dd>
							<dt>limit</dt><dd>number of rows per result</dd>
							<dt>format</dt><dd>output format</dd>							
					</dl>
					<P>Optional Parameters</P>
					<dl>
						<dt>uband</dt><dd>'u' filter limits (low,high)</dd>
						<dt>gband</dt><dd>'g' filter limits (low,high)</dd>
						<dt>rband</dt><dd>'r' filter limits (low,high)</dd>
						<dt>iband</dt><dd>'i' filter limits (low,high)</dd>
						<dt>zband</dt><dd>'z' filter limits (low,high)</dd>
					</dl>
				</article>
			</section>
			<section>
				<article>
					<header>RectangularSearch</header>
						<p><a target="_blank" href="<%=baseurl%>/SearchTools/RectangularSearch?min_ra=250.2&max_ra=250.5&min_dec=35.1&max_dec=35.5&searchtype=equitorial&limit=10&format=json&whichquery=irspectra"><%=baseurl%>/SearchTools/RectangularSearch?min_ra=250.2&max_ra=250.5&min_dec=35.1&max_dec=35.5&searchtype=equitorial&limit=10&format=json&whichquery=irspectra</a></p>
				</article>
				<article>
					<header>Parameters</header>
					<p>Required</p>
					<dl>
							<dt>min_ra</dt><dd>Ra :Lower limit</dd>
							<dt>max_ra</dt><dd>Ra :Upper limit</dd>
							<dt>min_dec</dt><dd>Dec : Lower Limit</dd>
							<dt>max_dec</dt><dd>Dec : Upper Limit</dd>
							<dt>searchtype</dt><dd>Search Type e.g. equitorial</dd>
							<dt>limit</dt><dd>number of rows in output</dd>
							<dt>format</dt><dd>output format</dd>
					</dl>
					<P>Optional Parameters</P>
					<dl>
						<dt>uband</dt><dd>'u' filter limits (low,high)</dd>
						<dt>gband</dt><dd>'g' filter limits (low,high)</dd>
						<dt>rband</dt><dd>'r' filter limits (low,high)</dd>
						<dt>iband</dt><dd>'i' filter limits (low,high)</dd>
						<dt>zband</dt><dd>'z' filter limits (low,high)</dd>
					</dl>
				</article>
			</section>
			</main>
		</article>

		<article>
			<header id="imagingquery">ImagingQuery</header>
			<main>
				<details>This set of services represnts Imaging Query tools. They are divided in four sub positional services. Cone,Rectangular,Proximity,NoPosition etc.
				</details>
				<section>
				<article>
					<header>Imaging Cone Service</header>
						<p><a target="_blank" href="<%=baseurl%>/ImagingQuery/Cone?limit=50&format=csv&imgparams=minimal&specparams=none&ra=10&dec=0.2&radius=5.0&magType=model&uMin=0&gMin=&rMin=&iMin=&zMin=&uMax=20&gMax=&rMax=&iMax=&zMax=&ugMin=&grMin=&riMin=&izMin=&ugMax=&grMax=&riMax=&izMax=&objType=doGalaxy,doStar&minQA=&flagsOnList=ignore&flagsOffList=ignore"><%=baseurl%>/ImagingQuery/Cone?limit=50&format=csv&imgparams=minimal&specparams=none&ra=10&dec=0.2&radius=5.0&magType=model&uMin=0&gMin=&rMin=&iMin=&zMin=&uMax=20&gMax=&rMax=&iMax=&zMax=&ugMin=&grMin=&riMin=&izMin=&ugMax=&grMax=&riMax=&izMax=&objType=doGalaxy,doStar&minQA=&flagsOnList=ignore&flagsOffList=ignore</a></p>
				</article>
				<article>
					<header>Parameters</header>
					<p>Required</p>
					<dl>
							<dt>ra</dt><dd>Right Ascension.</dd>
						    <dt>dec</dt><dd>Declination</dd>
						    <dt>radius</dt><dd>Search Radius</dd>
							<dt>objType</dt><dd>Type of Object</dd>
							<dt>uMin</dt><dd>lower limit of 'u'</dd>
							<dt>uMax</dt><dd>upper limit of 'u'</dd>
					</dl>
				</article>
			</section>
			<section>
				<article>
					<header>NoPosition Service</header>
						<p><a target="_blank" href="<%=baseurl%>/ImagingQuery/NoPosition?limit=30&izMin=3&izMax=4&riMin=&riMax=&flagsonlist=BRIGHT,EDGE&magType=model&uMin=0&gMin=&rMin=&iMin=&zMin=&uMax=20&gMax=&rMax=&iMax=&zMax=&ugMin=&grMin=&ugMax=&grMax=&objType=doGalaxy,doStar&minQA=&flagsOffList=ignore"><%=baseurl%>/ImagingQuery/NoPosition?limit=30&izMin=3&izMax=4&riMin=&riMax=&flagsonlist=BRIGHT,EDGE&magType=model&uMin=0&gMin=&rMin=&iMin=&zMin=&uMax=20&gMax=&rMax=&iMax=&zMax=&ugMin=&grMin=&ugMax=&grMax=&objType=doGalaxy,doStar&minQA=&flagsOffList=ignore</a></p>
					<p>Optional Parameters</p>
					<dl>
							<dt>izMin</dt><dd>lower limit 'i' & 'z' filter</dd>
							<dt>izMax</dt><dd>upper limit 'i' & 'z' filter</dd>
							<dt>riMin</dt><dd>lower limit 'i' & 'z' filter</dd>
							<dt>riMax</dt><dd>lower limit 'i' & 'z' filter</dd>
							<dt>flagsonlist</dt><dd>comma seperated list of flags</dd>
					</dl>
				</article>
			</section>
			<section>
				<article>
					<header>Proximity SearchService</header>
						<p><a target="_blank" href="<%=baseurl%>/ImagingQuery/Proximity"><%=baseurl%>/ImagingQuery/Proximity</a></p>
					<p>Parameters [_POST]</p>
					<dl>
							<dt>radius</dt><dd>Search Radius</dd>
							<dt>searchNearBy</dt><dd>All Nearby or Nearest Objects</dd>
							<dt>nearest</dt><dd>All Nearby or Nearest Objects</dd>
					</dl>
				</article>
			</section>
			<section>
				<article>
					<header>Rectangular Search Service</header>
						<p><a target="_blank" href="<%=baseurl%>/ImagingQuery/Rectangular?limit=50&izMin=3&izMax=4&riMin=0&riMax=20&flagsonlist=BRIGHT,EDGE&ramin=258&ramax=258.2&decmin=64&decmax=64.1&imgparams=typical,minimal&magType=model&uMin=&gMin=&rMin=&iMin=&zMin=&uMax=&gMax=&rMax=&iMax=&zMax=&ugMin=&grMin=&ugMax=&grMax=&objType=doGalaxy,doStar&minQA=&flagsOffList=ignore&format=csv"><%=baseurl%>/ImagingQuery/Rectangular?limit=50&izMin=3&izMax=4&riMin=0&riMax=20&flagsonlist=BRIGHT,EDGE&ramin=258&ramax=258.2&decmin=64&decmax=64.1&imgparams=typical,minimal&magType=model&uMin=&gMin=&rMin=&iMin=&zMin=&uMax=&gMax=&rMax=&iMax=&zMax=&ugMin=&grMin=&ugMax=&grMax=&objType=doGalaxy,doStar&minQA=&flagsOffList=ignore&format=csv</a></p>
				</article>
				<article>
					<header>Parameters</header>
						<p>Required</p>
					<dl>
							<dt>ramin</dt><dd>ra lower limit</dd>
							<dt>decmin</dt><dd>dec lower limit</dd>
							<dt>ramax</dt><dd>ra upper limit</dd>
							<dt>decmax</dt><dd>dec upper limit</dd>
					</dl>
				</article>
			</section>
			</main>
		</article>

		<article>
			<header id="spectroquery">SpectroQuery</header>
			<main>
				<details>These services represent the SpectroQuery tool of Skyserver search tools.They are divided in four sub positional services. Cone,Rectangular,Proximity,NoPosition etc.
				</details>
				<section>
				<article>
					<header>Cone Spectro Service</header>
						<p><a target="_blank" href="<%=baseurl%>/SpectroQuery/ConeSpectro?radius=5.0&dec=0.2&ra=10&uMin=0&uMax=20&objType=doGalaxy,doStar&limit=50&format=csv&specparams=minimal&imgparams=none&redshiftMin=&zWarning=on&redshiftMax=&priFlagsOnList=ignore&priFlagsOffList=ignore&secFlagsOnList=ignore&secFlagsOffList=ignore&magType=model&gMin=&rMin=&iMin=&zMin=&gMax=&rMax=&iMax=&zMax=&ugMin=&grMin=&riMin=&izMin=&ugMax=&grMax=&riMax=&izMax=&minQA=&flagsOnList=ignore&flagsOffList=ignore"><%=baseurl%>/SpectroQuery/ConeSpectro?radius=5.0&dec=0.2&ra=10&uMin=0&uMax=20&objType=doGalaxy,doStar&limit=50&format=csv&specparams=minimal&imgparams=none&redshiftMin=&zWarning=on&redshiftMax=&priFlagsOnList=ignore&priFlagsOffList=ignore&secFlagsOnList=ignore&secFlagsOffList=ignore&magType=model&gMin=&rMin=&iMin=&zMin=&gMax=&rMax=&iMax=&zMax=&ugMin=&grMin=&riMin=&izMin=&ugMax=&grMax=&riMax=&izMax=&minQA=&flagsOnList=ignore&flagsOffList=ignore</a></p>
				</article>
			</section>
			<section>
				<article>
					<header>NoPosition Spectro Service</header>
						<p><a target="_blank" href="<%=baseurl%>/SpectroQuery/NoPositionSpectro?uMin=0&uMax=20&objType=doGalaxy,doStar&limit=50&format=csv&specparams=minimal&imgparams=none&redshiftMin=&zWarning=on&redshiftMax=&priFlagsOnList=ignore&priFlagsOffList=ignore&secFlagsOnList=ignore&secFlagsOffList=ignore&magType=model&gMin=&rMin=&iMin=&zMin=&gMax=&rMax=&iMax=&zMax=&ugMin=&grMin=&riMin=&izMin=&ugMax=&grMax=&riMax=&izMax=&minQA=&flagsOnList=ignore&flagsOffList=ignore"><%=baseurl%>/SpectroQuery/NoPositionSpectro?uMin=0&uMax=20&objType=doGalaxy,doStar&limit=50&format=csv&specparams=minimal&imgparams=none&redshiftMin=&zWarning=on&redshiftMax=&priFlagsOnList=ignore&priFlagsOffList=ignore&secFlagsOnList=ignore&secFlagsOffList=ignore&magType=model&gMin=&rMin=&iMin=&zMin=&gMax=&rMax=&iMax=&zMax=&ugMin=&grMin=&riMin=&izMin=&ugMax=&grMax=&riMax=&izMax=&minQA=&flagsOnList=ignore&flagsOffList=ignore</a></p>
				</article>
				<article>
					<header>Parameters</header>
					<p>Required</p>
					<dl>
							<dt>ra</dt><dd>Right Ascension</dd>
							<dt>dec</dt><dd>Declination</dd>
							<dt>radius</dt><dd>Search Radius</dd>
							<dt>uMin</dt><dd>lower limit of 'u'</dd>
							<dt>uMax</dt><dd>upper limit of 'u'</dd>
							<dt>gMin</dt><dd>lower limit of 'g'</dd>
							<dt>gMax</dt><dd>upper limit of 'g'</dd>
							<dt>rMin</dt><dd>lower limit of 'r'</dd>
							<dt>rMax</dt><dd>upper limit of 'r'</dd>
							<dt>iMin</dt><dd>lower limit of 'i'</dd>
							<dt>iMax</dt><dd>upper limit of 'i'</dd>
							<dt>zMin</dt><dd>lower limit of 'z'</dd>
							<dt>zMax</dt><dd>upper limit of 'z'</dd>
					</dl>
				</article>
			</section>
			<section>
				<article>
					<header>Proximity Spectro Service</header>
						<p><a target="_blank" href="<%=baseurl%>/SpectroQuery/ProximitySpectro?radius=1.0&searchNearBy=nearest"><%=baseurl%>/SpectroQuery/ProximitySpectro?radius=1.0&searchNearBy=nearest</a></p>
				</article>
				<article>
					<header>Parameters</header>
					<p>Required</p>
					<dl>
							<dt>uMin</dt><dd>lower limit of 'u'</dd>
							<dt>uMax</dt><dd>upper limit of 'u'</dd>
							<dt>gMin</dt><dd>lower limit of 'g'</dd>
							<dt>gMax</dt><dd>upper limit of 'g'</dd>
							<dt>rMin</dt><dd>lower limit of 'r'</dd>
							<dt>rMax</dt><dd>upper limit of 'r'</dd>
							<dt>iMin</dt><dd>lower limit of 'i'</dd>
							<dt>iMax</dt><dd>upper limit of 'i'</dd>
							<dt>zMin</dt><dd>lower limit of 'z'</dd>
							<dt>zMax</dt><dd>upper limit of 'z'</dd>
							<dt>flagonlist</dt><dd>List of flags ON</dd>
							<dt>flagofflist</dt><dd>List of flags Off</dd>
					</dl>
					<p>_POST Parameters</p>
					<dl>
						<dt>ra</dt><dd>Right Ascension</dd>
						<dt>dec</dt><dd>Declination</dd>
						<dt>radius</dt><dd>Search Radius</dd>
					</dl>
				</article>
			</section>
			<section>
				<article>
					<header>Rectangular Spectro Service</header>
					<p><a target="_blank" href="<%=baseurl%>/SpectroQuery/RectangularSpectro?ramin=258&ramax=258.2&decmin=64&decmax=64.1&redshiftMin=0&redshiftMax=0.1&zWarning=0&class=galaxy&uMin=0&uMax=20&objType=doGalaxy,doStar&limit=50&format=csv&specparams=minimal&imgparams=none&priFlagsOnList=ignore&priFlagsOffList=ignore&secFlagsOnList=ignore&secFlagsOffList=ignore&magType=model&gMin=&rMin=&iMin=&zMin=&gMax=&rMax=&iMax=&zMax=&ugMin=&grMin=&riMin=&izMin=&ugMax=&grMax=&riMax=&izMax=&minQA=&flagsOnList=ignore&flagsOffList=ignore"><%=baseurl%>/SpectroQuery/RectangularSpectro?ramin=258&ramax=258.2&decmin=64&decmax=64.1&redshiftMin=0&redshiftMax=0.1&zWarning=0&class=galaxy&uMin=0&uMax=20&objType=doGalaxy,doStar&limit=50&format=csv&specparams=minimal&imgparams=none&priFlagsOnList=ignore&priFlagsOffList=ignore&secFlagsOnList=ignore&secFlagsOffList=ignore&magType=model&gMin=&rMin=&iMin=&zMin=&gMax=&rMax=&iMax=&zMax=&ugMin=&grMin=&riMin=&izMin=&ugMax=&grMax=&riMax=&izMax=&minQA=&flagsOnList=ignore&flagsOffList=ignore</a></p>
				</article>
				<article>
					<header>Parameters</header>
					<p>Required</p>
					<dl>
							<dt>ramin</dt><dd>ra lower limit</dd>
							<dt>decmin</dt><dd>dec lower limit</dd>
							<dt>ramax</dt><dd>ra upper limit</dd>
							<dt>decmax</dt><dd>dec upper limit</dd>
							<dt>redshiftmin</dt><dd>Min Redshift</dd>
							<dt>redshiftmax</dt><dd>Max Redshift</dd>
							<dt>zwarning</dt><dd>Warning</dd>
							<dt>class</dt><dd>Class</dd>
					</dl>
				</article>
			</section>
			</main>
		</article>

		<article>
			<header id="irspectra">IRSpectra</header>
			<main>
				<details>InfraRed Spectra Query also has four sub web services this represnt the web tool on skyserver.
				</details>
				<section>
				<article>
					<header>ConeIR Service</header>
					<p><a target="_blank" href="<%=baseurl%>/IRSpectraQuery/ConeIR?limit=50&format=json&irspecparams=typical&ra=271.75&dec=-20.19&radius=5.0&Lcenter=10&Bcenter=0.2&lbRadius=5.0&jMin=&hMin=&kMin=&jMax=&hMax=&kMax=&jhMin=&hkMin=&jhMax=&hkMax=&snrMin=&vhelioMin=&scatterMin=&snrMax=&vhelioMax=&scatterMax=&tempMin=&loggMin=&fehMin=&afeMin=&tempMax=&loggMax=&fehMax=&afeMax=&irTargetFlagsOnList=ignore&irTargetFlagsOffList=ignore&irTargetFlags2OnList=ignore&irTargetFlags2OffList=ignore"><%=baseurl%>/IRSpectraQuery/ConeIR?limit=50&format=json&irspecparams=typical&ra=271.75&dec=-20.19&radius=5.0&Lcenter=10&Bcenter=0.2&lbRadius=5.0&jMin=&hMin=&kMin=&jMax=&hMax=&kMax=&jhMin=&hkMin=&jhMax=&hkMax=&snrMin=&vhelioMin=&scatterMin=&snrMax=&vhelioMax=&scatterMax=&tempMin=&loggMin=&fehMin=&afeMin=&tempMax=&loggMax=&fehMax=&afeMax=&irTargetFlagsOnList=ignore&irTargetFlagsOffList=ignore&irTargetFlags2OnList=ignore&irTargetFlags2OffList=ignore</a></p>
				</article>
				<article>
						<header>Parameters</header>
						<p>Required</p>
						<dl>
							<dt>irspecparams</dt><dd>...</dd>
							<dt>ra</dt><dd>Right Ascension</dd>
							<dt>dec</dt><dd>Declination</dd>
							<dt>radius</dt><dd>Search Radius</dd>
						</dl>
					</article>
			</section>
			<section>
					<article>
					<header>Galactic IR Service</header>
							<p><a target="_blank" href="<%=baseurl%>/IRSpectraQuery/GalacticIR?limit=50&format=csv&irspecparams=typical&Lcenter=10&Bcenter=0.2&lbRadius=5.0&jMin=&hMin=&kMin=&jMax=&hMax=&kMax=&jhMin=&hkMin=&jhMax=&hkMax=&snrMin=&vhelioMin=&scatterMin=&snrMax=&vhelioMax=&scatterMax=&tempMin=&loggMin=&fehMin=&afeMin=&tempMax=&loggMax=&fehMax=&afeMax=&irTargetFlagsOnList=ignore&irTargetFlagsOffList=ignore&irTargetFlags2OnList=ignore&irTargetFlags2OffList=ignore"><%=baseurl%>/IRSpectraQuery/GalacticIR?limit=50&format=csv&irspecparams=typical&Lcenter=10&Bcenter=0.2&lbRadius=5.0&jMin=&hMin=&kMin=&jMax=&hMax=&kMax=&jhMin=&hkMin=&jhMax=&hkMax=&snrMin=&vhelioMin=&scatterMin=&snrMax=&vhelioMax=&scatterMax=&tempMin=&loggMin=&fehMin=&afeMin=&tempMax=&loggMax=&fehMax=&afeMax=&irTargetFlagsOnList=ignore&irTargetFlagsOffList=ignore&irTargetFlags2OnList=ignore&irTargetFlags2OffList=ignore</a></p>
					</article>
			</section>
			<section>
					<article>
						<header>NoPositionIR Service</header>
						<p><a target="_blank" href="<%=baseurl%>/IRSpectraQuery/NoPositionIR?limit=50&format=csv&irspecparams=typical&jMin=&hMin=&kMin=&jMax=&hMax=&kMax=&jhMin=&hkMin=&jhMax=&hkMax=&snrMin=&vhelioMin=&scatterMin=&snrMax=&vhelioMax=&scatterMax=&tempMin=&loggMin=&fehMin=&afeMin=&tempMax=&loggMax=&fehMax=&afeMax=&irTargetFlagsOnList=ignore&irTargetFlagsOffList=ignore&irTargetFlags2OnList=ignore&irTargetFlags2OffList=ignore"><%=baseurl%>/IRSpectraQuery/NoPositionIR?limit=50&format=csv&irspecparams=typical&jMin=&hMin=&kMin=&jMax=&hMax=&kMax=&jhMin=&hkMin=&jhMax=&hkMax=&snrMin=&vhelioMin=&scatterMin=&snrMax=&vhelioMax=&scatterMax=&tempMin=&loggMin=&fehMin=&afeMin=&tempMax=&loggMax=&fehMax=&afeMax=&irTargetFlagsOnList=ignore&irTargetFlagsOffList=ignore&irTargetFlags2OnList=ignore&irTargetFlags2OffList=ignore</a></p>
					</article>
			</section>
			</main>
		</article>
	</section>
		
	</main>

<footer>
 <!-- jQuery (necessary for Bootstrap's JavaScript plugins) -->
 <!-- uncomment scripts to use javascript -->
    <!--script src="https://ajax.googleapis.com/ajax/libs/jquery/1.11.2/jquery.min.js"></script-->
    <!-- Include all compiled plugins (below), or include individual files as needed -->
    <!--script src="assets/bootstrap/dist/js/bootstrap.js"></script-->
</footer>
	
</body>
</html>