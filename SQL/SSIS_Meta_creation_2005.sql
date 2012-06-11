/****** Object:  Table [dbo].[LookupConnectionID]    Script Date: 12/05/2009 20:31:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LookupConnectionID]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[LookupConnectionID](
    [ConnectionGUID] [nvarchar](1000) NOT NULL,
    [ConnectionDescription] [nvarchar](1000) NOT NULL
) ON [PRIMARY]
INSERT [dbo].[LookupConnectionID] ([ConnectionGUID], [ConnectionDescription]) VALUES (N'{5F2826BC-648B-4f3e-B930-587F4EF331D4}', N'ODBC 2005')
INSERT [dbo].[LookupConnectionID] ([ConnectionGUID], [ConnectionDescription]) VALUES (N'{9B5D63AB-A629-4A56-9F3E-B1044134B649}', N'OLEDB 2005')
INSERT [dbo].[LookupConnectionID] ([ConnectionGUID], [ConnectionDescription]) VALUES (N'{72692A11-F5CC-42b8-869D-84E7C8E48B14}', N'ADO.NET 2005')
INSERT [dbo].[LookupConnectionID] ([ConnectionGUID], [ConnectionDescription]) VALUES (N'{4CF60474-BA87-4ac2-B9F3-B7B9179D4183}', N'ADO 2005')
INSERT [dbo].[LookupConnectionID] ([ConnectionGUID], [ConnectionDescription]) VALUES (N'RelationalDataSource', N'olap relational data source')
INSERT [dbo].[LookupConnectionID] ([ConnectionGUID], [ConnectionDescription]) VALUES (N'{09AD884B-0248-42C1-90E6-897D1CD16D37}', N'ODBC 2008')
INSERT [dbo].[LookupConnectionID] ([ConnectionGUID], [ConnectionDescription]) VALUES (N'{3BA51769-6C3C-46B2-85A1-81E58DB7DAE1}', N'OLEDB 2008')
INSERT [dbo].[LookupConnectionID] ([ConnectionGUID], [ConnectionDescription]) VALUES (N'{A1100566-934E-470C-9ECE-0D5EB920947D}', N'ADO 2008')
INSERT [dbo].[LookupConnectionID] ([ConnectionGUID], [ConnectionDescription]) VALUES (N'{894CAE21-539F-46EB-B36D-9381163B6C4E}', N'ADO.Net 2008')
END
GO
/****** Object:  Table [dbo].[Audit]    Script Date: 12/05/2009 20:31:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Audit]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Audit](
    [PackageGUID] [varchar](50) NOT NULL,
    [DataFlowTaskID] [int] NOT NULL,
    [SourceReadRows] [int] NULL,
    [SourceReadErrorRows] [int] NULL,
    [CleansedRows] [int] NULL,
    [TargetWriteRows] [int] NULL,
    [TargetWriteErrorRows] [int] NULL,
    [Comment] [nvarchar](255) NULL
) ON [PRIMARY]
END
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Version]    Script Date: 12/05/2009 20:31:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Version]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Version](
    [VersionID] [int] NOT NULL,
    [InstallDate] [datetime] NOT NULL,
 CONSTRAINT [PK_Version] PRIMARY KEY CLUSTERED 
(
    [VersionID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[RunScan]    Script Date: 12/05/2009 20:31:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RunScan]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[RunScan](
    [RunKey] [int] NOT NULL,
    [RunDate] [datetime] NOT NULL,
    [RunCommand] [nvarchar](512) NOT NULL,
 CONSTRAINT [PK_RunScan] PRIMARY KEY CLUSTERED 
(
    [RunKey] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'RunScan', NULL,NULL))
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Stores a row for each execution of the DependancyAnalyzer program' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RunScan'
GO
/****** Object:  Table [dbo].[ObjectTypes]    Script Date: 12/05/2009 20:31:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ObjectTypes]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[ObjectTypes](
    [ObjectTypeKey] [nvarchar](255) NOT NULL,
    [ObjectTypeName] [nvarchar](255) NULL,
    [ObjectTypeDesc] [nvarchar](2000) NULL,
    [ObjectMetaType] [nvarchar](255) NULL,
    [Domain] [nvarchar](50) NULL
) ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[Objects]    Script Date: 12/05/2009 20:31:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Objects]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Objects](
    [RunKey] [int] NOT NULL,
    [ObjectKey] [int] NOT NULL,
    [ObjectName] [nvarchar](1000) NULL,
    [ObjectTypeString] [nvarchar](1000) NOT NULL,
    [ObjectDesc] [nvarchar](1000) NULL,
 CONSTRAINT [PK_Objects] PRIMARY KEY CLUSTERED 
(
    [RunKey] ASC,
    [ObjectKey] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[ObjectDependencies]    Script Date: 12/05/2009 20:31:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ObjectDependencies]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[ObjectDependencies](
    [RunKey] [int] NOT NULL,
    [SrcObjectKey] [int] NOT NULL,
    [TgtObjectKey] [int] NOT NULL,
    [DependencyType] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_ObjectDependencies] PRIMARY KEY CLUSTERED 
(
    [RunKey] ASC,
    [SrcObjectKey] ASC,
    [TgtObjectKey] ASC,
    [DependencyType] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[ObjectAttributes]    Script Date: 12/05/2009 20:31:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ObjectAttributes]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[ObjectAttributes](
    [RunKey] [int] NOT NULL,
    [ObjectKey] [int] NOT NULL,
    [ObjectAttrName] [nvarchar](1000) NOT NULL,
    [ObjectAttrValue] [nvarchar](max) NOT NULL
) ON [PRIMARY]
END
GO
/****** Object:  ForeignKey [FK_ObjectAttributes_Objects]    Script Date: 12/05/2009 20:31:46 ******/
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ObjectAttributes_Objects]') AND parent_object_id = OBJECT_ID(N'[dbo].[ObjectAttributes]'))
ALTER TABLE [dbo].[ObjectAttributes]  WITH CHECK ADD  CONSTRAINT [FK_ObjectAttributes_Objects] FOREIGN KEY([RunKey], [ObjectKey])
REFERENCES [dbo].[Objects] ([RunKey], [ObjectKey])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ObjectAttributes_Objects]') AND parent_object_id = OBJECT_ID(N'[dbo].[ObjectAttributes]'))
ALTER TABLE [dbo].[ObjectAttributes] CHECK CONSTRAINT [FK_ObjectAttributes_Objects]
GO
/****** Object:  ForeignKey [FK_ObjectDependencies_Objects]    Script Date: 12/05/2009 20:31:46 ******/
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ObjectDependencies_Objects]') AND parent_object_id = OBJECT_ID(N'[dbo].[ObjectDependencies]'))
ALTER TABLE [dbo].[ObjectDependencies]  WITH CHECK ADD  CONSTRAINT [FK_ObjectDependencies_Objects] FOREIGN KEY([RunKey], [SrcObjectKey])
REFERENCES [dbo].[Objects] ([RunKey], [ObjectKey])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ObjectDependencies_Objects]') AND parent_object_id = OBJECT_ID(N'[dbo].[ObjectDependencies]'))
ALTER TABLE [dbo].[ObjectDependencies] CHECK CONSTRAINT [FK_ObjectDependencies_Objects]
GO
/****** Object:  ForeignKey [FK_ObjectDependencies_Objects1]    Script Date: 12/05/2009 20:31:46 ******/
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ObjectDependencies_Objects1]') AND parent_object_id = OBJECT_ID(N'[dbo].[ObjectDependencies]'))
ALTER TABLE [dbo].[ObjectDependencies]  WITH CHECK ADD  CONSTRAINT [FK_ObjectDependencies_Objects1] FOREIGN KEY([RunKey], [TgtObjectKey])
REFERENCES [dbo].[Objects] ([RunKey], [ObjectKey])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ObjectDependencies_Objects1]') AND parent_object_id = OBJECT_ID(N'[dbo].[ObjectDependencies]'))
ALTER TABLE [dbo].[ObjectDependencies] CHECK CONSTRAINT [FK_ObjectDependencies_Objects1]
GO
/****** Object:  ForeignKey [FK_Objects_RunScan]    Script Date: 12/05/2009 20:31:46 ******/
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Objects_RunScan]') AND parent_object_id = OBJECT_ID(N'[dbo].[Objects]'))
ALTER TABLE [dbo].[Objects]  WITH CHECK ADD  CONSTRAINT [FK_Objects_RunScan] FOREIGN KEY([RunKey])
REFERENCES [dbo].[RunScan] ([RunKey])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Objects_RunScan]') AND parent_object_id = OBJECT_ID(N'[dbo].[Objects]'))
ALTER TABLE [dbo].[Objects] CHECK CONSTRAINT [FK_Objects_RunScan]
GO


/* Start Views */

IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[Connections]'))
EXEC dbo.sp_executesql @statement = N'CREATE VIEW [dbo].[Connections]
AS SELECT 1 AS Column1' 
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[SourceTables]'))
EXEC dbo.sp_executesql @statement = N'CREATE VIEW [dbo].[SourceTables]
AS SELECT 1 AS Column1' 
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[ObjectRelationships]'))
EXEC dbo.sp_executesql @statement = N'CREATE VIEW [dbo].[ObjectRelationships]
AS
SELECT 1 AS Column1' 
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[LineageMap]'))
EXEC dbo.sp_executesql @statement = N'CREATE VIEW [dbo].[LineageMap]
AS
SELECT 1 AS Column1' 
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[TargetTables]'))
EXEC dbo.sp_executesql @statement = N'CREATE VIEW [dbo].[TargetTables]
AS
SELECT 1 AS Column1' 
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[DataFlows]'))
EXEC dbo.sp_executesql @statement = N'CREATE VIEW [dbo].[DataFlows]
AS
SELECT 1 AS Column1' 
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[WalkSources]'))
EXEC dbo.sp_executesql @statement = N'CREATE VIEW [dbo].[WalkSources]
AS
SELECT 1 AS Column1' 
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[Packages]'))
EXEC dbo.sp_executesql @statement = N'CREATE VIEW [dbo].[Packages]
AS
SELECT 1 AS Column1' 
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vAudit]'))
EXEC dbo.sp_executesql @statement = N'CREATE VIEW [dbo].[vAudit]
AS
SELECT 1 AS Column1' 
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[TableLineageMap]'))
EXEC dbo.sp_executesql @statement = N'CREATE VIEW [dbo].[TableLineageMap]
AS
SELECT 1 AS Column1' 
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[ConnectionsMapping]'))
EXEC dbo.sp_executesql @statement = N'CREATE VIEW [dbo].[ConnectionsMapping]
AS
SELECT 1 AS Column1' 
GO


/* Start View Definitions */

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


GO
ALTER VIEW [dbo].[TargetTables]
AS
SELECT
    Objects.RunKey,
    ObjectDependencies.DependencyType,
    Objects.ObjectKey,
    Objects.ObjectName,
    Objects.ObjectDesc,
    ObjectDependencies.SrcObjectKey AS TgtComponentKey,
    TargetObjects.ObjectName AS TargetComponentName,
    TargetObjects.ObjectDesc AS TargetComponentDesc,
    OD_DataFlow.SrcObjectKey AS DataFlowID,
    OD_DestConnection.SrcObjectKey AS DestinationConnectionID

FROM dbo.Objects

INNER JOIN dbo.ObjectDependencies AS ObjectDependencies
        ON Objects.ObjectKey = ObjectDependencies.TgtObjectKey
       AND Objects.RunKey = ObjectDependencies.RunKey

INNER JOIN dbo.Objects AS TargetObjects
        ON ObjectDependencies.SrcObjectKey = TargetObjects.ObjectKey
       AND Objects.RunKey = TargetObjects.RunKey
       AND ObjectDependencies.RunKey = TargetObjects.RunKey

INNER JOIN dbo.ObjectDependencies AS OD_DataFlow
        ON ObjectDependencies.SrcObjectKey = OD_DataFlow.TgtObjectKey
       AND ObjectDependencies.RunKey = OD_DataFlow.RunKey

INNER JOIN dbo.ObjectDependencies AS OD_DestConnection
        ON Objects.ObjectKey = OD_DestConnection.TgtObjectKey
       AND Objects.RunKey = OD_DestConnection.RunKey

WHERE ObjectDependencies.DependencyType = N'Map'
  AND Objects.ObjectTypeString = N'Table'
  AND OD_DataFlow.DependencyType = N'Containment'
  AND OD_DestConnection.DependencyType = N'Containment'
GO
ALTER VIEW [dbo].[SourceTables]
AS
SELECT
    dbo.Objects.RunKey,
    dbo.Objects.ObjectKey,
    dbo.Objects.ObjectName,
    dbo.Objects.ObjectTypeString,
    dbo.Objects.ObjectDesc,
    dbo.ObjectDependencies.TgtObjectKey AS SrcComponentKey,
    SourceObjects.ObjectName AS SourceObjectsName,
    SourceObjects.ObjectDesc AS SourceObjectsDesc,
    OD_DataFlow.SrcObjectKey AS DataFlowID,
    OD_DestConnection.SrcObjectKey AS SourceConnectionID

FROM dbo.Objects

INNER JOIN dbo.ObjectDependencies
        ON dbo.Objects.ObjectKey = dbo.ObjectDependencies.SrcObjectKey
        AND dbo.Objects.RunKey = dbo.ObjectDependencies.RunKey

INNER JOIN dbo.ObjectDependencies AS OD_DataFlow
        ON dbo.ObjectDependencies.TgtObjectKey = OD_DataFlow.TgtObjectKey
        AND dbo.ObjectDependencies.RunKey = OD_DataFlow.RunKey

INNER JOIN dbo.Objects AS SourceObjects
        ON dbo.ObjectDependencies.TgtObjectKey = SourceObjects.ObjectKey
        AND dbo.ObjectDependencies.RunKey = SourceObjects.RunKey

INNER JOIN dbo.ObjectDependencies AS OD_DestConnection
        ON dbo.Objects.ObjectKey = OD_DestConnection.TgtObjectKey
        AND dbo.Objects.RunKey = OD_DestConnection.RunKey

WHERE dbo.ObjectDependencies.DependencyType = N'Map'
  AND dbo.Objects.ObjectTypeString = N'Table'
  AND OD_DataFlow.DependencyType = N'Containment'
  AND OD_DataFlow.DependencyType = OD_DestConnection.DependencyType
GO

ALTER VIEW [dbo].[LineageMap]
AS
SELECT
    RunKey,
    SrcObjectKey,
    TgtObjectKey
    
FROM dbo.ObjectDependencies

WHERE DependencyType = N'Map'

GO

ALTER VIEW [dbo].[WalkSources]
AS
WITH f(RunKey, osrc, tgt, lvl, objecttype) 
AS 
(SELECT  Objects.RunKey,    dbo.SourceTables.ObjectKey
    , dbo.SourceTables.SrcComponentKey
    , 0 AS Expr1
    , dbo.Objects.ObjectTypeString
FROM dbo.SourceTables 
INNER JOIN dbo.Objects 
    ON dbo.SourceTables.ObjectKey = dbo.Objects.ObjectKey
    AND SourceTables.RunKey = Objects.RunKey
UNION ALL
SELECT  Objects_1.RunKey,    f_2.osrc
    , dbo.LineageMap.TgtObjectKey
    , f_2.lvl + 1 AS Expr1
    , Objects_1.ObjectTypeString
FROM         f AS f_2 
INNER JOIN dbo.LineageMap 
    ON f_2.tgt = dbo.LineageMap.SrcObjectKey 
INNER JOIN dbo.Objects AS Objects_1 
    ON dbo.LineageMap.TgtObjectKey = Objects_1.ObjectKey
    AND LineageMap.RunKey = Objects_1.RunKey
WHERE     (NOT (f_2.osrc = f_2.tgt)))

SELECT   RunKey,   osrc, tgt, lvl, objecttype
FROM         f AS f_1

GO

ALTER VIEW [dbo].[ObjectRelationships]
AS
SELECT
    RunKey,
    SrcObjectKey AS ParentObjectKey,
    TgtObjectKey AS ChildObjectKey

FROM dbo.ObjectDependencies

WHERE DependencyType = N'Containment'

GO

ALTER VIEW [dbo].[Packages]
AS
SELECT 
    Objects.RunKey,
    Objects.ObjectKey AS PackageID, 
    Objects.ObjectName AS PackageName,
    Objects.ObjectDesc AS PackageDesc,
    PackageProperties.PackageLocation,
    PackageProperties.PackageGUID
FROM [dbo].[Objects],
    (SELECT 
        RunKey,
        PackageProperties.ObjectKey,
        [PackageLocation],
        [PackageGUID]
    FROM dbo.ObjectAttributes 
    PIVOT (
        MIN (ObjectAttrValue) 
        FOR ObjectAttrName 
        IN ([PackageLocation], [PackageGUID])
        ) AS PackageProperties
    ) AS PackageProperties
WHERE [Objects].ObjectKey = PackageProperties.ObjectKey 
AND [Objects].RunKey = PackageProperties.RunKey
AND [Objects].ObjectTypeString = N'SSIS Package'

GO


ALTER VIEW [dbo].[Connections]
AS
SELECT 
    [Objects].[RunKey],
    [Objects].ObjectKey AS ConnectionID,
    [Objects].ObjectName AS ConnectionName,
    [Objects].ObjectDesc AS ConnectionDesc,
    ConnectionString,
    ConnectionProperties.[Server],
    ConnectionProperties.[Database]
    
FROM [dbo].[Objects] 
INNER JOIN
    (SELECT
        RunKey,
        ConnectionProperties.ObjectKey,
        ConnectionString,
        [Server],
        [Database]
        
        FROM [dbo].[ObjectAttributes] 
        PIVOT 
            (
                MIN(ObjectAttrValue) FOR ObjectAttrName 
                    IN (ConnectionString, [Server], [Database])
            ) AS ConnectionProperties
    ) AS ConnectionProperties
    ON [Objects].ObjectKey = ConnectionProperties.ObjectKey
    AND [Objects].RunKey = ConnectionProperties.RunKey
INNER JOIN dbo.LookupConnectionID
    ON ConnectionGUID = [Objects].ObjectTypeString

GO


ALTER VIEW [dbo].[TableLineageMap]
AS
SELECT
    dbo.WalkSources.RunKey,
    dbo.SourceTables.ObjectKey AS SourceTableObjectKey,
    dbo.SourceTables.ObjectName AS SourceTable,
    srel.ParentObjectKey AS SourceConnectionKey,
    sconn.ConnectionName AS SourceConnectionName,
    sconn.ConnectionString AS SourceConnectionString,
    sconn.[Server] AS SourceServer,
    sconn.[Database] AS SourceDatabase,
    dbo.SourceTables.SrcComponentKey AS SourceComponentKey,
    dbo.TargetTables.ObjectName AS TargetTable,
    dbo.TargetTables.TgtComponentKey AS TargetComponentKey,
    trel.ParentObjectKey AS TargetConnectionKey,
    tconn.ConnectionName AS TargetConnectionName,
    tconn.ConnectionString AS TargetConnectionString,
    tconn.[Server] AS TargetServer,
    tconn.[Database] AS TargetDatabase,
    dfrel.ParentObjectKey AS DataFlowKey,
    dbo.Packages.PackageName,
    dbo.Packages.PackageDesc,
    dbo.Packages.PackageLocation,
    dbo.Packages.PackageGUID

FROM dbo.WalkSources

INNER JOIN dbo.SourceTables
        ON dbo.WalkSources.osrc = dbo.SourceTables.ObjectKey
        AND dbo.WalkSources.RunKey = dbo.SourceTables.RunKey

INNER JOIN dbo.TargetTables
        ON dbo.WalkSources.tgt = dbo.TargetTables.ObjectKey
        AND dbo.WalkSources.RunKey = dbo.TargetTables.RunKey

INNER JOIN dbo.ObjectRelationships AS srel
        ON dbo.SourceTables.ObjectKey = srel.ChildObjectKey
        AND dbo.SourceTables.RunKey = srel.RunKey

INNER JOIN dbo.ObjectRelationships AS trel
        ON dbo.TargetTables.ObjectKey = trel.ChildObjectKey
        AND dbo.TargetTables.RunKey = trel.RunKey

INNER JOIN dbo.ObjectRelationships AS dfrel
        ON dbo.TargetTables.TgtComponentKey = dfrel.ChildObjectKey
        AND dbo.TargetTables.RunKey = dfrel.RunKey

INNER JOIN dbo.ObjectRelationships AS pkgrel
        ON dfrel.ParentObjectKey = pkgrel.ChildObjectKey
        AND dfrel.RunKey = pkgrel.RunKey

INNER JOIN dbo.Packages
        ON pkgrel.ParentObjectKey = dbo.Packages.PackageID
        AND pkgrel.RunKey = dbo.Packages.RunKey

INNER JOIN dbo.Connections AS sconn
        ON srel.ParentObjectKey = sconn.ConnectionID
        AND srel.RunKey = sconn.RunKey

INNER JOIN dbo.Connections AS tconn
        ON trel.ParentObjectKey = tconn.ConnectionID
        AND trel.RunKey = tconn.RunKey

GO





ALTER VIEW [dbo].[DataFlows]
AS
SELECT
    dbo.Objects.RunKey,
    dbo.Objects.ObjectKey,
    dbo.Objects.ObjectName,
    dbo.Objects.ObjectDesc,
    dbo.ObjectDependencies.SrcObjectKey AS PackageID

FROM dbo.Objects

INNER JOIN dbo.ObjectDependencies
        ON dbo.Objects.ObjectKey = dbo.ObjectDependencies.TgtObjectKey
        AND dbo.Objects.RunKey = dbo.ObjectDependencies.RunKey

WHERE dbo.Objects.ObjectTypeString IN (
    N'{C3BF9DC1-4715-4694-936F-D3CFDA9E42C5}', 
    N'{E3CFBEA8-1F48-40D8-91E1-2DEDC1EDDD56}'
)
  AND dbo.ObjectDependencies.DependencyType = N'Containment'

GO

ALTER VIEW [dbo].[ConnectionsMapping]
AS
SELECT DISTINCT 
    srel.ParentObjectKey AS SourceConnectionID,
    trel.ParentObjectKey AS TargetConnectionID

FROM dbo.WalkSources

INNER JOIN dbo.SourceTables
        ON dbo.WalkSources.osrc = dbo.SourceTables.ObjectKey
        AND dbo.WalkSources.RunKey = dbo.SourceTables.RunKey

INNER JOIN dbo.TargetTables
        ON dbo.WalkSources.tgt = dbo.TargetTables.ObjectKey
        AND dbo.WalkSources.RunKey = dbo.TargetTables.RunKey

INNER JOIN dbo.ObjectRelationships AS srel
        ON dbo.SourceTables.ObjectKey = srel.ChildObjectKey
        AND srel.RunKey = dbo.SourceTables.RunKey

INNER JOIN dbo.ObjectRelationships AS trel
        ON dbo.TargetTables.ObjectKey = trel.ChildObjectKey
        AND dbo.TargetTables.RunKey = trel.RunKey

GO

/* End Views */

-- Alpha 4
INSERT INTO dbo.Version
(VersionID, InstallDate)
VALUES
(4, GETDATE())
GO

/****** Object:  View [dbo].[WalkSources]    Script Date: 04/05/2010 22:06:05 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER VIEW [dbo].[WalkSources]
AS
WITH WalkSourceCTE(RunKey, osrc, tgt, lvl, objecttype, ParentString) 
AS 
(
    SELECT  Objects.RunKey
        , dbo.SourceTables.ObjectKey
        , dbo.SourceTables.SrcComponentKey
        , 0 AS Expr1
        , dbo.Objects.ObjectTypeString
        , CAST(',' + CAST(dbo.SourceTables.ObjectKey as varchar(14)) + ',' AS VARCHAR(2000)) AS ParentString
    FROM dbo.SourceTables 
    INNER JOIN dbo.Objects 
        ON dbo.SourceTables.ObjectKey = dbo.Objects.ObjectKey
        AND SourceTables.RunKey = Objects.RunKey
    UNION ALL
    SELECT  Objects.RunKey
        , WalkSourceCTE.osrc
        , dbo.LineageMap.TgtObjectKey
        , WalkSourceCTE.lvl + 1 AS Expr1
        , Objects.ObjectTypeString
        , CAST(WalkSourceCTE.ParentString +  CAST(WalkSourceCTE.tgt as varchar(14)) + ',' AS VARCHAR(2000)) AS ParentString
    FROM         WalkSourceCTE
    INNER JOIN dbo.LineageMap 
        ON WalkSourceCTE.tgt = dbo.LineageMap.SrcObjectKey 
        AND WalkSourceCTE.RunKey = dbo.LineageMap.RunKey
    INNER JOIN dbo.Objects
        ON dbo.LineageMap.TgtObjectKey = Objects.ObjectKey
        AND LineageMap.RunKey = Objects.RunKey
    WHERE NOT ((WalkSourceCTE.osrc = WalkSourceCTE.tgt)
    OR CHARINDEX(',' + CAST(WalkSourceCTE.tgt AS VARCHAR(40)) + ',', WalkSourceCTE.ParentString) > 0)
)
    
SELECT   RunKey,   osrc, tgt, lvl, objecttype
FROM         WalkSourceCTE

GO

-- Alpha 5
INSERT INTO dbo.Version
(VersionID, InstallDate)
VALUES
(5, GETDATE())
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_RetrieveRunIDs]') AND type in (N'P', N'PC'))
EXEC ('CREATE PROCEDURE [dbo].[usp_RetrieveRunIDs] AS SELECT 1')
GO
-- =============================================
-- Author:		Keith Martin
-- Create date: 2011-11-16
-- Description:	Retrieves the list of Run's
-- =============================================
ALTER PROCEDURE [dbo].[usp_RetrieveRunIDs]
AS
BEGIN
SET NOCOUNT ON;
SELECT [RunKey] ,CONVERT(NVARCHAR(40), [RunDate], 120) + CHAR(9) + [RunCommand] FROM [dbo].[RunScan]
END
GO
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[fn_IntCSVSplit]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[fn_IntCSVSplit]
GO
-- =============================================
-- Author:		Keith Martin
-- Create date: 2011-11-16
-- Description:	Retrieves a table of integers from a csv string
-- =============================================
CREATE FUNCTION [dbo].[fn_IntCSVSplit]
( @RowData NVARCHAR(MAX) )
RETURNS @RtnValue TABLE 
( Data INT ) 
AS
BEGIN 
    DECLARE @Iterator INT
    DECLARE @WorkString NVARCHAR(MAX)
    SET @Iterator = 1
    DECLARE @FoundIndex INT
    SET @FoundIndex = CHARINDEX(',',@RowData)
    WHILE (@FoundIndex>0)
    BEGIN
        SET @WorkString = LTRIM(RTRIM(SUBSTRING(@RowData, 1, @FoundIndex - 1)))
        IF ISNUMERIC(@WorkString) = 1
        BEGIN
            INSERT INTO @RtnValue (data) VALUES (@WorkString)
        END
        ELSE
        BEGIN
            INSERT INTO @RtnValue (data) VALUES(NULL)
        END
        SET @RowData = SUBSTRING(@RowData, @FoundIndex + 1,LEN(@RowData))
        SET @Iterator = @Iterator + 1
        SET @FoundIndex = CHARINDEX(',', @RowData)
    END
    IF ISNUMERIC(LTRIM(RTRIM(@RowData))) = 1
    BEGIN
        INSERT INTO @RtnValue (Data) SELECT LTRIM(RTRIM(@RowData))
    END
    RETURN
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_RetrieveObjects]') AND type in (N'P', N'PC'))
EXEC ('CREATE PROCEDURE [dbo].[usp_RetrieveObjects] AS SELECT 1')
GO
-- =============================================
-- Author:		Keith Martin
-- Create date: 2011-11-16
-- Description:	Retrieves the list of Objects
-- =============================================
ALTER PROCEDURE [dbo].[usp_RetrieveObjects]
    @RunList nvarchar(max)
AS
BEGIN
    SET NOCOUNT ON;
        SELECT [ObjectKey], [ObjectName], [Objects].[ObjectTypeString], [ObjectTypes].[ObjectTypeName], [RunKey]
        FROM [dbo].[Objects] 
        LEFT OUTER JOIN [dbo].[ObjectTypes]
            ON [Objects].[ObjectTypeString] = [ObjectTypes].[ObjectTypeKey]
        INNER JOIN [dbo].[fn_IntCSVSplit](@RunList) AS Filter
            ON Filter.Data = [RunKey]
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_RetrieveLineageMap]') AND type in (N'P', N'PC'))
EXEC ('CREATE PROCEDURE [dbo].[usp_RetrieveLineageMap] AS SELECT 1')
GO
-- =============================================
-- Author:		Keith Martin
-- Create date: 2011-11-16
-- Description:	Retrieves the list of LineageMap
-- =============================================
ALTER PROCEDURE [dbo].[usp_RetrieveLineageMap]
    @RunList nvarchar(max)
AS
BEGIN
    SET NOCOUNT ON;
        SELECT [SrcObjectKey], [TgtObjectKey], [DependencyType] 
        FROM [dbo].[ObjectDependencies]
        INNER JOIN [dbo].[fn_IntCSVSplit](@RunList) AS Filter
            ON Filter.Data = [RunKey]
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_RetrieveObjectDetails]') AND type in (N'P', N'PC'))
EXEC ('CREATE PROCEDURE [dbo].[usp_RetrieveObjectDetails] AS SELECT 1')
GO
-- =============================================
-- Author:		Keith Martin
-- Create date: 2011-11-16
-- Description:	Retrieves the ObjectDetails
-- =============================================
ALTER PROCEDURE [dbo].[usp_RetrieveObjectDetails]
    @RunList nvarchar(max)
,	@ObjectKey INT
AS
BEGIN
    SET NOCOUNT ON;
        SELECT [ObjectTypeString], [ObjectDesc] 
        FROM [dbo].[Objects]
        INNER JOIN [dbo].[fn_IntCSVSplit](@RunList) AS Filter
            ON Filter.Data = [RunKey]
            AND ObjectKey = @ObjectKey
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_RetrieveObjectTypes]') AND type in (N'P', N'PC'))
EXEC ('CREATE PROCEDURE [dbo].[usp_RetrieveObjectTypes] AS SELECT 1')
GO
-- =============================================
-- Author:		Keith Martin
-- Create date: 2011-11-16
-- Description:	Retrieves the ObjectTypes
-- =============================================
ALTER PROCEDURE [dbo].[usp_RetrieveObjectTypes]
    @ObjectTypeKey NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
        SELECT [ObjectTypeName]
        FROM [dbo].[ObjectTypes] 
        WHERE ObjectTypeKey = @ObjectTypeKey
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_RetrieveObjectAttributes]') AND type in (N'P', N'PC'))
EXEC ('CREATE PROCEDURE [dbo].[usp_RetrieveObjectAttributes] AS SELECT 1')
GO
-- =============================================
-- Author:		Keith Martin
-- Create date: 2011-11-16
-- Description:	Retrieves the ObjectAttributes
-- =============================================
ALTER PROCEDURE [dbo].[usp_RetrieveObjectAttributes]
    @RunList nvarchar(max)
,	@ObjectKey INT
AS
BEGIN
    SET NOCOUNT ON;
        SELECT [ObjectAttrName], [ObjectAttrValue]
        FROM [dbo].[ObjectAttributes] 
        INNER JOIN [dbo].[fn_IntCSVSplit](@RunList) AS Filter
            ON Filter.Data = [RunKey]
            AND ObjectKey = @ObjectKey
        ORDER BY [ObjectAttrName];
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_RetrieveContainedTargetDependencies]') AND type in (N'P', N'PC'))
EXEC ('CREATE PROCEDURE [dbo].[usp_RetrieveContainedTargetDependencies] AS SELECT 1')
GO
-- =============================================
-- Author:		Keith Martin
-- Create date: 2011-11-16
-- Description:	Retrieves the Contained Target Dependencies
-- =============================================
ALTER PROCEDURE [dbo].[usp_RetrieveContainedTargetDependencies]
    @RunList nvarchar(max)
,	@TgtObjectKey INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT [SrcObjectKey] 
    FROM [dbo].[ObjectDependencies] 
    INNER JOIN [dbo].[fn_IntCSVSplit](@RunList) AS Filter
        ON Filter.Data = [RunKey]
        AND [DependencyType] = 'Containment' 
        AND [TgtObjectKey] = @TgtObjectKey
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_RetrieveSSASObjects]') AND type in (N'P', N'PC'))
EXEC ('CREATE PROCEDURE [dbo].[usp_RetrieveSSASObjects] AS SELECT 1')
GO
-- =============================================
-- Author:		Keith Martin
-- Create date: 2011-11-16
-- Description:	Retrieves the SSAS Objects
-- =============================================
ALTER PROCEDURE [dbo].[usp_RetrieveSSASObjects]
    @RunList nvarchar(max)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT [ObjectKey], [ObjectName]
    FROM [dbo].[Objects]
    INNER JOIN [dbo].[fn_IntCSVSplit](@RunList) AS Filter
        ON Filter.Data = [RunKey]
        AND [ObjectTypeString] = 'Ssas.Analysis Server'
    ORDER BY [ObjectName]
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_RetrieveSQLSObjects]') AND type in (N'P', N'PC'))
EXEC ('CREATE PROCEDURE [dbo].[usp_RetrieveSQLSObjects] AS SELECT 1')
GO
-- =============================================
-- Author:		Keith Martin
-- Create date: 2011-11-16
-- Description:	Retrieves the SQL Server Objects
-- =============================================
ALTER PROCEDURE [dbo].[usp_RetrieveSQLSObjects]
    @RunList nvarchar(max)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT  ConnectionID, ISNULL([Server], ConnectionName) + ISNULL('.' + [Database], '') as DisplayName
    FROM  [dbo].[Connections]
    INNER JOIN [dbo].[fn_IntCSVSplit](@RunList) AS Filter
        ON Filter.Data = [RunKey]
    ORDER BY DisplayName
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_RetrieveSSRSObjects]') AND type in (N'P', N'PC'))
EXEC ('CREATE PROCEDURE [dbo].[usp_RetrieveSSRSObjects] AS SELECT 1')
GO
-- =============================================
-- Author:		Keith Martin
-- Create date: 2011-11-16
-- Description:	Retrieves the SSRS Objects
-- =============================================
ALTER PROCEDURE [dbo].[usp_RetrieveSSRSObjects]
    @RunList nvarchar(max)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT [ObjectKey], [ObjectName]
    FROM [dbo].[Objects]
    INNER JOIN [dbo].[fn_IntCSVSplit](@RunList) AS Filter
        ON Filter.Data = [RunKey]
        AND [ObjectTypeString] = 'ReportServer'
    ORDER BY [ObjectName]
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_RetrieveFileObjects]') AND type in (N'P', N'PC'))
EXEC ('CREATE PROCEDURE [dbo].[usp_RetrieveFileObjects] AS SELECT 1')
GO
-- =============================================
-- Author:		Keith Martin
-- Create date: 2011-11-16
-- Description:	Retrieves the File Server Objects
-- =============================================
ALTER PROCEDURE [dbo].[usp_RetrieveFileObjects]
    @RunList nvarchar(max)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT [ObjectKey], [ObjectName]
    FROM [dbo].[Objects]
    INNER JOIN [dbo].[fn_IntCSVSplit](@RunList) AS Filter
        ON Filter.Data = [RunKey]
        AND [ObjectTypeString] = 'Machine'
    ORDER BY [ObjectName]
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_RetrieveSSISObjects]') AND type in (N'P', N'PC'))
EXEC ('CREATE PROCEDURE [dbo].[usp_RetrieveSSISObjects] AS SELECT 1')
GO
-- =============================================
-- Author:		Keith Martin
-- Create date: 2011-11-16
-- Description:	Retrieves the SSIS Objects
-- =============================================
ALTER PROCEDURE [dbo].[usp_RetrieveSSISObjects]
    @RunList nvarchar(max)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT [PackageID], [PackageLocation] 
    FROM [dbo].[Packages]
    INNER JOIN [dbo].[fn_IntCSVSplit](@RunList) AS Filter
        ON Filter.Data = [RunKey]
    ORDER BY [PackageID]
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_RetrieveContained]') AND type in (N'P', N'PC'))
EXEC ('CREATE PROCEDURE [dbo].[usp_RetrieveContained] AS SELECT 1')
GO
-- =============================================
-- Author:		Keith Martin
-- Create date: 2011-11-16
-- Description: Retrieves the Children of this Containment Object
-- =============================================
ALTER PROCEDURE [dbo].[usp_RetrieveContained]
    @SrcObjectKey INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT DISTINCT TgtObjectKey, ObjectName, ISNULL(ObjectTypes.ObjectTypeName, ObjectTypeString) as ObjectTypeString 
    FROM [dbo].[ObjectDependencies]
            INNER JOIN [dbo].[Objects] 
                ON ObjectKey = TgtObjectKey
                AND [DependencyType] = 'Containment' 
                AND SrcObjectKey = @SrcObjectKey
    LEFT OUTER JOIN [dbo].[ObjectTypes] 
        ON ObjectTypes.ObjectTypeKey = ObjectTypeString 
    ORDER BY ObjectTypeString, ObjectName
END
GO


INSERT INTO dbo.Version
(VersionID, InstallDate)
VALUES
(6, GETDATE())
GO
