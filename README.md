[![Gitpod ready-to-code](https://img.shields.io/badge/Gitpod-ready--to--code-blue?logo=gitpod)](https://gitpod.io/#https://github.com/keif888/SQLServerMetadata)

# SQL Server Metadata Toolkit

Downloads [here](https://github.com/keif888/SQLServerMetadata/releases) 

# Project Description

MSDN's SQL 2005 tool kit updated to 2008, 2012, 2014, 2016, 2017 and 2019 for detecting metadata in SQL Server, SSIS, SSRS and SSAS.

This project is an update on the project released on MSDN Code Gallery  [SQL Server Metadata Toolkit](http://code.msdn.microsoft.com/SqlServerMetadata)

It has the ability to scan the following versions 2005, 2008, 2008 R2, 2012, 2014, 2016, 2017 and 2019...

Please note that support for Transact SQL capabilities that have been introduced since isn't there.  If you encounter a T-SQL statement that doesn't parse, please post it as an [issue](https://github.com/keif888/SQLServerMetadata/issues).

It can scan SSIS, SSAS, SQL Server Databases, and Reporting Services.

It has an "Executor", which allows the user to use a GUI to run the various analysers.  If you hover over the buttons at the bottom, the actual command will be shown in the status bar, and you can then copy that command if you so desire...

# Release Notes

**Beta 28** Fixes issues #35, #37. This release adds SQL 2019 capabilities, and ODBC call handling to the Parser.

**Beta 27** Fixes issues #23, #24, #25, #27, #30, #36.

**Beta 26** Fixes issue #21. This release also adds missing configuration into the database to allow viewing of the database dependencies for SQL 2014 and above.

**Beta 25** Fixes for issues #19 and #20. This release adds the ability for the analysis to ignore SSIS components that are not registered on the machine doing the analysis by treating them as uninteresting.

**Beta 24** Further fixes for issue #17 where v0.23.0.0 did not include enough of the DLL's which make up SQL Server SMO capabilities for SQL 2017.

**Beta 23** Fixes issue #17 which was caused by not including the SQL Server SMO capabilities for SQL 2017, as Microsoft are no longer packaging this capability into the GAC.

**Beta 22** Adds capability to detect .ispac files, and scan them.  Adds capability to detect .conmgr and/or .params files, and switch to "hybid mode".  In "hybrid mode" the folder is checked for the expected .dtproj files, and these are compiled to .ispac files and scanned.  If there are no .dtproj files, then no SSIS packages will be scanned in that folder.  If there are no .conmgr or .params files (in sub folders) then standard SSIS scan will be performed.

**Beta 21** Corrects issue with Project Deployed packages not seeing the connections or parameters.  Also updates the TSQL parser to latest available.

**Beta 20** Corrects issues with SSIS Catalog capabilites, adds SQL 2016 and 2017, and utlises the latest MSAGL libraries

**Alpha 19** Adds SSIS Catalog capabilities

**Alpha 18** corrects issues with missing DLL's, adds encrypted package support, adds multiple SSIS Servers (not SSIS Catalog) capability.

**Alpha 17** corrected issues with databases requested to scan not being available, missing schema name for some objects, three part names appearing in reports.  It has also added a command line option to return three part names for all database objects found.

**Alpha 16** corrected a number of issues with cross database object references and synonyms, single quotes in object names, shared data sets in reports.

**Alpha 15** added SQL 2014 support, and changed the installer to WIX

**Alpha 14** wasn't released to the general public

**Alpha 13** is a feature patch release, which adds SQL 2012 abilities, adds Insert statements (and many others), corrects a Primary Key violation error, adds Print and Save to the viewer.

**Alpha 12** is a feature patch release, which corrects the SQL 2005/2008 Reporting Services analysis (so it works), corrects the database create on the first run, so all tables and procedures are created, and adds the ability to scan only one analysis services database.

**Alpha 11** is a functionallity release which adds an execution tool, Reporting Services analysis, Database analysis, and tweaks to the Viewer.
The DependencyViewer has been changed to use MSAGL to display the graph, with print, and save options, as well as the ability to change the layout that is selected from Top Down (old default) to Left Right, Right Left, and Bottom Up.

**Alpha 10** was a feature patch release, which adds the ability specify specific Integration Services folders within Integration Services (as opposed to the file system).

**Alpha 9** was a bug patch release which fixes cases where SSIS' AccessMode is incorrectly set (3 instead of 1).  Also improves the EXEC parser.  It also addresses the issue in previous versions not enumerating the SSIS Components when they are only installed in the x32 SSIS directory (as most component installers do).
There are enhancements to add Column names as Attributes, and with the Lookup, Fuzzy Lookup, Derived Column and Multiple Hash components, additional information is added to the Column name attributes.

**Alpha 8** was a functionallity release which added SQL Commands from Variables

**Alpha 7** was a bug patch release which adds a number of new statements into the Parser.  (CAST, EXECUTE, WITH CTE)

**Alpha 6** wasn't released to the general public

**Alpha 5** now includes additional handling for the Kimball SCD component.

**Alpha 4** now processes most SQL Statements to get the names of tables, and link these into the display.

**Alpha 3** wasn't released to the general public

**Alpha 2** has added handling for Containers, which was missing from the MSDN version.  These are now recursively checked for Data Flows.

**Alpha 1** wasn't released to the general public
The Database has been updated to handle longer names for attributes, and some views have been updated to handle SQL 2005 and SQL 2008 GUIDs.


## Known Issues:
The Executor doesn't have the option to change the SSIS server.

## Example Screenshot
The following picture shows the Dependency Viewer:
![Dependency Viewer](https://github.com/keif888/SQLServerMetadata/blob/master/WikiImages/Home_DepViewer.PNG)

## Other Downloads Required:
To be able to analyse SSIS requires either the appropriate version of Visual Studio/BIDS/SSDT, or the appropriate version of SSIS to be installed.

To be able to analyse SQL/AS 2005, without having SQL 2005 installed, requires the SQLSERVER2005_ASAMO10, SQLSysClrTypes and SharedManagementObjects (32 bit).  But these are no longer available from Microsoft.

To be able to analyse SQL/AS 2008, without having SQL 2008 installed, requires the SQLSERVER2008_ASAMO10, SQLSysClrTypes and SharedManagementObjects (32 bit) from here: https://www.microsoft.com/en-us/download/details.aspx?id=44272

To be able to analyse SQL/AS 2012, without having SQL 2012 installed, requires the SQL_AS_AMO, SQLSysClrTypes and SharedManagementObjects (32 bit) from here: https://www.microsoft.com/en-us/download/details.aspx?id=56041

To be able to analyse SQL/AS 2014, without having SQL 2014 installed, requires the SQL_AS_AMO, SQLSysClrTypes and SharedManagementObjects (32 bit) from here: https://www.microsoft.com/en-us/download/details.aspx?id=42295

To be able to analyse SQL/AS 2016, without having SQL 2016 installed, requires the SQL_AS_AMO, SQLSysClrTypes and SharedManagementObjects (32 bit) from here: https://www.microsoft.com/en-us/download/details.aspx?id=103444

From 2017 onwards, SQL and Analysis services capability has been provided directly in the download.
