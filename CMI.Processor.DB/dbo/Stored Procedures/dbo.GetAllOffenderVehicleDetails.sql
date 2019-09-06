
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
			0 AS [IsDeleted]
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

			--AND OFCI.[Logon] IN (''kplunkett'')
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

			--AND OFCI.[Logon] IN (''kplunkett'')
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