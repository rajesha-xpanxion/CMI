
/*==========================================================================================
Author:			Rajesh Awate
Create date:	20-Aug-18
Description:	To get all offender vehicle details from given automon database
---------------------------------------------------------------------------------
Test execution:-
DECLARE @OfficerLogonsToFilterTbl [dbo].[Varchar50Tbl];
INSERT INTO @OfficerLogonsToFilterTbl
	([Item])
VALUES
	('mboyd'),('ryost'),('kpitts'),('khennings'),('ebellew'),('gromanko'),('acraven'),('rrussell'),('kplunkett'),('sclark'),('bvogt'),('jward'),('fblanco'),('plewis'),('jwyatt')
EXEC	
	[dbo].[GetAllOffenderVehicleDetails]
		@AutomonDatabaseName = 'CX',
		@LastExecutionDateTime = NULL,
		@OfficerLogonsToFilterTbl = @OfficerLogonsToFilterTbl
---------------------------------------------------------------------------------
History:-
Date			Author			Changes
20-Aug-18		Rajesh Awate	Created.
27-Aug-18		Rajesh Awate	Changes to skip records having Vyear as NULL and return Unknown for NULL for columns Make, BodyStyle & Color
10-Sept-19		Rajesh Awate	Changes for integration by officer filter.
==========================================================================================*/
CREATE PROCEDURE [dbo].[GetAllOffenderVehicleDetails]
	@AutomonDatabaseName NVARCHAR(128),
	@LastExecutionDateTime DATETIME = NULL,
	@OfficerLogonsToFilterTbl [dbo].[Varchar50Tbl] READONLY
AS
BEGIN
	DECLARE @SQLString NVARCHAR(MAX), @ParmDefinition NVARCHAR(1000);

	--check if any ooficer logon filter passed
	IF(EXISTS(SELECT 1 FROM @OfficerLogonsToFilterTbl))
	BEGIN
		SET @LastExecutionDateTime = NULL;
	END
	
	IF(@LastExecutionDateTime IS NOT NULL)
	BEGIN
		SET @SQLString = 
		'
		SELECT DISTINCT
			OI.[Pin],
			VI.[Id],
			ISNULL(VI.[Make], ''Unknown'') AS [Make],
			ISNULL(VI.[BodyStyle], ''Unknown'') AS [BodyStyle],
			VI.[Vyear],
			VI.[LicensePlate],
			ISNULL(VI.[Color], ''Unknown'') AS [Color],
			1 AS [IsActive]
		FROM
			[$AutomonDatabaseName].[dbo].[OffenderInfo] OI JOIN [$AutomonDatabaseName].[dbo].[VehicleInfo] VI
				ON OI.[PersonId] = VI.[PersonId]
		WHERE
			VI.[Vyear] IS NOT NULL
			AND VI.[FromTime] >= @LastExecutionDateTime
			AND VI.[ToTime] IS NULL
		UNION
		SELECT DISTINCT
			OI.[Pin],
			VI.[Id],
			ISNULL(VI.[Make], ''Unknown'') AS [Make],
			ISNULL(VI.[BodyStyle], ''Unknown'') AS [BodyStyle],
			VI.[Vyear],
			VI.[LicensePlate],
			ISNULL(VI.[Color], ''Unknown'') AS [Color],
			0 AS [IsActive]
		FROM
			[$AutomonDatabaseName].[dbo].[OffenderInfo] OI JOIN [$AutomonDatabaseName].[dbo].[VehicleInfo] VI
				ON OI.[PersonId] = VI.[PersonId]
		WHERE
			VI.[Vyear] IS NOT NULL
			AND VI.[ChainId] IS NULL
			AND VI.[ToTime] IS NOT NULL
			AND VI.[ToTime] >= @LastExecutionDateTime
		';
	END
	ELSE
	BEGIN
		SET @SQLString = 
		'
		SELECT DISTINCT
			OI.[Pin],
			VI.[Id],
			ISNULL(VI.[Make], ''Unknown'') AS [Make],
			ISNULL(VI.[BodyStyle], ''Unknown'') AS [BodyStyle],
			VI.[Vyear],
			VI.[LicensePlate],
			ISNULL(VI.[Color], ''Unknown'') AS [Color],
			1 AS [IsActive]
		FROM
			[$AutomonDatabaseName].[dbo].[OffenderInfo] OI JOIN [$AutomonDatabaseName].[dbo].[VehicleInfo] VI
				ON OI.[PersonId] = VI.[PersonId]
				--officer wise data
				LEFT JOIN [$AutomonDatabaseName].[dbo].[OffenderCaseloadInfo] OCLI
					ON OI.[Id] = OCLI.[OffenderId]
					LEFT JOIN [$AutomonDatabaseName].[dbo].[CaseloadInfo] CLI
						ON OCLI.[CaseloadId] = CLI.[Id]
						LEFT JOIN [$AutomonDatabaseName].[dbo].[OfficerCaseloadInfo] OFCCLI
							ON CLI.[Id] = OFCCLI.[CaseloadId]
							LEFT JOIN [$AutomonDatabaseName].[dbo].[OfficerInfo] OFCI
								ON OFCCLI.[OfficerId] = OFCI.[Id]
		WHERE
			VI.[Vyear] IS NOT NULL
			AND VI.[ToTime] IS NULL

			--apply officer logon filter if any passed
			AND
			(
				NOT EXISTS(SELECT 1 FROM @OfficerLogonsToFilterTbl OLTF) 
				OR EXISTS(SELECT 1 FROM @OfficerLogonsToFilterTbl OLTF WHERE OLTF.[Item] = OFCI.[Logon])
			)
		UNION
		SELECT DISTINCT
			OI.[Pin],
			VI.[Id],
			ISNULL(VI.[Make], ''Unknown'') AS [Make],
			ISNULL(VI.[BodyStyle], ''Unknown'') AS [BodyStyle],
			VI.[Vyear],
			VI.[LicensePlate],
			ISNULL(VI.[Color], ''Unknown'') AS [Color],
			0 AS [IsActive]
		FROM
			[$AutomonDatabaseName].[dbo].[OffenderInfo] OI JOIN [$AutomonDatabaseName].[dbo].[VehicleInfo] VI
				ON OI.[PersonId] = VI.[PersonId]
				--officer wise data
				LEFT JOIN [$AutomonDatabaseName].[dbo].[OffenderCaseloadInfo] OCLI
					ON OI.[Id] = OCLI.[OffenderId]
					LEFT JOIN [$AutomonDatabaseName].[dbo].[CaseloadInfo] CLI
						ON OCLI.[CaseloadId] = CLI.[Id]
						LEFT JOIN [$AutomonDatabaseName].[dbo].[OfficerCaseloadInfo] OFCCLI
							ON CLI.[Id] = OFCCLI.[CaseloadId]
							LEFT JOIN [$AutomonDatabaseName].[dbo].[OfficerInfo] OFCI
								ON OFCCLI.[OfficerId] = OFCI.[Id]
		WHERE
			VI.[Vyear] IS NOT NULL
			AND VI.[ChainId] IS NULL
			AND VI.[ToTime] IS NOT NULL

			--apply officer logon filter if any passed
			AND
			(
				NOT EXISTS(SELECT 1 FROM @OfficerLogonsToFilterTbl OLTF) 
				OR EXISTS(SELECT 1 FROM @OfficerLogonsToFilterTbl OLTF WHERE OLTF.[Item] = OFCI.[Logon])
			)
		';
	END


	SET @SQLString = REPLACE(@SQLString, '$AutomonDatabaseName', @AutomonDatabaseName);

	SET @ParmDefinition = '
		@LastExecutionDateTime DATETIME,
		@OfficerLogonsToFilterTbl [dbo].[Varchar50Tbl] READONLY';

--PRINT @SQLString;

	EXECUTE sp_executesql 
				@SQLString, 
				@ParmDefinition,  
                @LastExecutionDateTime = @LastExecutionDateTime,
				@OfficerLogonsToFilterTbl = @OfficerLogonsToFilterTbl;
END