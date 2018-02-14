# SQL Server Metadata Toolkit

# Project Description

MSDN's SQL 2005 tool kit updated to 2008, 2012, 2014, 2016 and 2017 for detecting metadata in SQL Server, SSIS, SSRS and SSAS.

This project is an update on the project released on MSDN Code Gallery  [SQL Server Metadata Toolkit](http://code.msdn.microsoft.com/SqlServerMetadata)

It has the ability to scan the following versions 2005, 2008, 2008 R2, 2012, 2014, 2016 and 2017...

Please note that support for features that have been introduced since SQL 2008 R2 is still limited.

It can scan SSIS, SSAS, SQL Server Databases, and Reporting Services.

It has an "Executor", which allows the user to use a GUI to run the various analysers.  If you hover over the buttons at the bottom, the actual command will be shown in the status bar, and you can then copy that command if you so desire...

# Release Notes

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

