@echo off
REM *****************************************************
REM * Copy files from Tamas to bin folder of MapWinGIS  *
REM * But only the needed files                         *
REM *                                                   *
REM * Usage CopyTamasFiles from_dir to_dir              *
REM *                                                   *
REM * Paul Meems, March 2014                            * 
REM * Paul Meems, update for ecw dll, June 2015         *
REM * Usage to test:                                    *
REM * CopyTamasFiles.bat D:\dev\MapwinGIS\GIT\support\GDAL_SDK\v120\bin\win32 D:\dev\MapwinGIS\GIT\src\bin\Win32\
REM *****************************************************

set _from_dir=%1
set _to_dir=%2
if '%_from_dir%'=='' if '%_to_dir%'=='' GOTO usage

REM Copy gdal plugins:
FOR %%G IN (gdal_MrSID.dll gdal_netCDF.dll) DO (
	IF EXIST %_from_dir%\gdal\plugins\%%G (
    xcopy /v /c /r /y %_from_dir%\gdal\plugins\%%G  %_to_dir%gdalplugins\
	)
)

REM Copy gdal plugins:
FOR %%G IN (gdal_ECW_JP2ECW.dll) DO (
	IF EXIST %_from_dir%\gdal\plugins-optional\%%G (
    xcopy /v /c /r /y %_from_dir%\gdal\plugins-optional\%%G  %_to_dir%gdalplugins\
	)
)

REM Copy PROJ4 data:
xcopy /v /c /r /y %_from_dir%\gdal-data\*.* %_to_dir%gdal-data\
xcopy /v /c /r /y %_from_dir%\proj\SHARE\*.* %_to_dir%..\PROJ_NAD\

REM Copy needed Tamas binaries:
FOR %%G IN (cfitsio.dll geos.dll geos_c.dll hdf5.dll hdf5_hl.dll hdf5_cpp.dll hdf5_hl_cpp.dll libcurl.dll 
            iconv.dll libeay32.dll libmysql.dll libpq.dll libxml2.dll lti_dsdk_9.1.dll
            lti_lidar_dsdk_1.1.dll libecwj2.dll netcdf.dll openjp2.dll proj.dll spatialite.dll sqlite3.dll freexl.dll  
            ssleay32.dll szip.dll tbb.dll xdrdll.dll xerces-c_2_8.dll xerces-c_3_1.dll zlib1.dll libtiff.dll expat.dll) DO (
	IF EXIST %_from_dir%\%%G (
    xcopy /v /c /r /y %_from_dir%\%%G  %_to_dir%
	)
)

REM gdal contains the version number, so use a wildcard:
del /f /q %_to_dir%\gdal*.dll
xcopy /v /c /r /y %_from_dir%\gdal2*.dll  %_to_dir%

REM Copy licenses:
xcopy /v /c /r /y %_from_dir%\..\..\..\licenses\*.rtf %_to_dir%..\Licenses\

 
GOTO EOF

:usage
echo incorrect usage:
echo Use CopyTamasFiles from_dir to_dir  
 
 
:EOF