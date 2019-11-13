

/*==========================================================================================
Author:			Rajesh Awate
Create date:	18-Sept-19
Description:	To save offender incentive details to given automon database
---------------------------------------------------------------------------------
Test execution:-
DECLARE @IncentedActivityDetailsTbl [dbo].[IncentedActivityDetailsTbl];
INSERT INTO @IncentedActivityDetailsTbl
	([ActivityTypeName], [ActivityIdentifier])
VALUES
	('Drug Test Appointment', 'c948d769-c364-4d81-a3f7-1db0e7f52f27'),
	('Office Visit', '3bdcf474-69fc-4eab-9931-e9898dd14462'),
	('Field Visit', '51710aea-5e24-4a0f-8414-a7b9d4b34f24')
EXEC	
	[dbo].[SaveOffenderIncentiveDetails]
		@AutomonDatabaseName = 'CX',
		@Pin = '5824',
		@Magnitude  = 'Low',
		@Response = 'Fish Bowl Drawing',
		@DateIssued = '2019-08-03',
		@IsBundled = 1,
		@IsSkipped = 0,
		@IncentedActivityDetailsTbl = @IncentedActivityDetailsTbl,
		@UpdatedBy = 'rawate@xpanxion.com';
---------------------------------------------------------------------------------
History:-
Date			Author			Changes
18-Sept-19		Rajesh Awate	Created.
==========================================================================================*/
CREATE PROCEDURE [dbo].[SaveOffenderIncentiveDetails]
	@AutomonDatabaseName NVARCHAR(128),
	@Pin VARCHAR(20),
	@Magnitude VARCHAR(255) = NULL,
	@Response VARCHAR(255) = NULL,
	@DateIssued VARCHAR(255) = NULL,
	@IsBundled BIT,
	@IsSkipped BIT,
	@IncentedActivityDetailsTbl [dbo].[IncentedActivityDetailsTbl] READONLY,
	@UpdatedBy VARCHAR(255)
AS
BEGIN
	DECLARE @SQLString NVARCHAR(MAX), @ParmDefinition NVARCHAR(1000);
	
		SET @SQLString = 
		'
		--declare required variables and assign it with values
		DECLARE 
			@EnteredByPId		INT			= ISNULL((SELECT [PersonId] FROM [$AutomonDatabaseName].[dbo].[OfficerInfo] WHERE [Email] = @UpdatedBy), 0),
			@CurrentId			INT			= 1,
			@MaxId				INT;

		--retrieve records for setting incentive attributes
		DECLARE @IncentedEventDetails AS TABLE
		(
			[Id] INT NOT NULL IDENTITY(1, 1),
			[EventId] INT NOT NULL
		);

		INSERT INTO @IncentedEventDetails 
		SELECT DISTINCT
			CONVERT(INT, VAOMD.[AutomonIdentifier])
		FROM
			@IncentedActivityDetailsTbl ICDT JOIN [dbo].[vw_AllOutboundMessageDetails] VAOMD
				ON ICDT.[ActivityIdentifier] = VAOMD.[ActivityIdentifier]
		WHERE
			VAOMD.[AutomonIdentifier] IS NOT NULL
			
		-- check if there are any records to process
		IF(EXISTS(SELECT 1 FROM @IncentedEventDetails))
		BEGIN
			--retrieve max id
			SET @MaxId = (SELECT MAX([Id]) FROM @IncentedEventDetails);

			--loop through each record and update its attribute
			WHILE(@CurrentId <= @MaxId)
			BEGIN
				--retrieve current event id to process
				DECLARE @CurrentEventId INT;
				SET @CurrentEventId = (SELECT [EventId] FROM @IncentedEventDetails WHERE [Id] = @CurrentId);

				--set each attribute or clear it based on @IsSkipped flag
				--Nexus Incentive Magnitude
				IF(@IsSkipped = 0 AND @Magnitude IS NOT NULL)
				BEGIN
					EXEC [$AutomonDatabaseName].[dbo].[UpdateEventAttribute] @CurrentEventId, @EnteredByPId, @Magnitude, NULL, ''NexusResponseMagnitudeLevel'', NULL, NULL, NULL;
				END
				ELSE
				BEGIN
					EXEC [$AutomonDatabaseName].[dbo].[UpdateEventAttribute] @CurrentEventId, @EnteredByPId, NULL, NULL, ''NexusResponseMagnitudeLevel'', NULL, NULL, NULL;
				END

				--Nexus Incentive Response
				IF(@IsSkipped = 0 AND @Response IS NOT NULL)
				BEGIN
					EXEC [$AutomonDatabaseName].[dbo].[UpdateEventAttribute] @CurrentEventId, @EnteredByPId, @Response, NULL, ''NexusIncentiveResponseFinal'', NULL, NULL, NULL;
				END
				ELSE
				BEGIN
					EXEC [$AutomonDatabaseName].[dbo].[UpdateEventAttribute] @CurrentEventId, @EnteredByPId, NULL, NULL, ''NexusIncentiveResponseFinal'', NULL, NULL, NULL;
				END

				--Nexus Incentive Date Issued
				IF(@IsSkipped = 0 AND @DateIssued IS NOT NULL)
				BEGIN
					EXEC [$AutomonDatabaseName].[dbo].[UpdateEventAttribute] @CurrentEventId, @EnteredByPId, @DateIssued, NULL, ''NexusIncentiveDateIssues'', NULL, NULL, NULL;
				END
				ELSE
				BEGIN
					EXEC [$AutomonDatabaseName].[dbo].[UpdateEventAttribute] @CurrentEventId, @EnteredByPId, NULL, NULL, ''NexusIncentiveDateIssues'', NULL, NULL, NULL;
				END

				--Nexus Incentive Bundled
				IF(@IsSkipped = 0 AND @IsBundled IS NOT NULL)
				BEGIN
					--convert into varchar(255) format
					DECLARE @IsBundledText VARCHAR(255);
					SET @IsBundledText = (CASE WHEN @IsBundled = 1 THEN ''True'' ELSE ''False'' END);

					EXEC [$AutomonDatabaseName].[dbo].[UpdateEventAttribute] @CurrentEventId, @EnteredByPId, @IsBundledText, NULL, ''IncentiveBundled'', NULL, NULL, NULL;
				END
				ELSE
				BEGIN
					EXEC [$AutomonDatabaseName].[dbo].[UpdateEventAttribute] @CurrentEventId, @EnteredByPId, NULL, NULL, ''IncentiveBundled'', NULL, NULL, NULL;
				END

				SET @CurrentId = @CurrentId + 1;
			END
		END

		';


	SET @SQLString = REPLACE(@SQLString, '$AutomonDatabaseName', @AutomonDatabaseName);

	SET @ParmDefinition = '
		@Pin VARCHAR(20),
		@Magnitude VARCHAR(255),
		@Response VARCHAR(255),
		@DateIssued VARCHAR(255),
		@IsBundled BIT,
		@IsSkipped BIT,
		@IncentedActivityDetailsTbl [dbo].[IncentedActivityDetailsTbl] READONLY,
		@UpdatedBy VARCHAR(255)';

--PRINT @SQLString;

	EXECUTE 
		sp_executesql 
			@SQLString, 
			@ParmDefinition,  
			@Pin = @Pin,
			@Magnitude = @Magnitude,
			@Response = @Response,
			@DateIssued = @DateIssued,
			@IsBundled = @IsBundled,
			@IsSkipped = @IsSkipped,
			@IncentedActivityDetailsTbl = @IncentedActivityDetailsTbl,
			@UpdatedBy = @UpdatedBy;
END