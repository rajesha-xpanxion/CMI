
/*==========================================================================================
Author:			Rajesh Awate
Create date:	20-Aug-18
Description:	To get all offender vehicle details from given automon database
---------------------------------------------------------------------------------
Test execution:-
EXEC	
	[dbo].[GetAllOffenderVehicleDetails]
		@AutomonDatabaseName = 'CX',
		@LastExecutionDateTime = NULL
---------------------------------------------------------------------------------
History:-
Date			Author			Changes
20-Aug-18		Rajesh Awate	Created.
27-Aug-18		Rajesh Awate	Changes to skip records having Vyear as NULL and return Unknown for NULL for columns Make, BodyStyle & Color
==========================================================================================*/
CREATE PROCEDURE [dbo].[GetAllOffenderVehicleDetails]
	@AutomonDatabaseName NVARCHAR(128),
	@LastExecutionDateTime DATETIME = NULL
AS
BEGIN
	DECLARE @SQLString NVARCHAR(MAX), @ParmDefinition NVARCHAR(1000);
	
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
			0 AS [IsDeleted]
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
			1 AS [IsDeleted]
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
			0 AS [IsDeleted]
		FROM
			[$AutomonDatabaseName].[dbo].[OffenderInfo] OI JOIN [$AutomonDatabaseName].[dbo].[VehicleInfo] VI
				ON OI.[PersonId] = VI.[PersonId]
		WHERE
			VI.[Vyear] IS NOT NULL
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
			1 AS [IsDeleted]
		FROM
			[$AutomonDatabaseName].[dbo].[OffenderInfo] OI JOIN [$AutomonDatabaseName].[dbo].[VehicleInfo] VI
				ON OI.[PersonId] = VI.[PersonId]
		WHERE
			VI.[Vyear] IS NOT NULL
			AND VI.[ChainId] IS NULL
			AND VI.[ToTime] IS NOT NULL
		';
	END


	SET @SQLString = REPLACE(@SQLString, '$AutomonDatabaseName', @AutomonDatabaseName);

	SET @ParmDefinition = '@LastExecutionDateTime DATETIME';

--PRINT @SQLString;

	EXECUTE sp_executesql 
				@SQLString, 
				@ParmDefinition,  
                @LastExecutionDateTime = @LastExecutionDateTime;
END