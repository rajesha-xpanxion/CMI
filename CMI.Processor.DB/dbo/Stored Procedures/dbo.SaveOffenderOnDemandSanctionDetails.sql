﻿


/*==========================================================================================
Author:			Rajesh Awate
Create date:	06-Nov-19
Description:	To save offender on demand sanction details to given automon database
---------------------------------------------------------------------------------
Test execution:-
DECLARE @SanctionedActivityDetailsTbl [dbo].[SanctionedActivityDetailsTbl];
INSERT INTO @SanctionedActivityDetailsTbl
	([ActivityTypeName], [ActivityIdentifier])
VALUES
	('Drug Test Appointment', 'c948d769-c364-4d81-a3f7-1db0e7f52f27'),
	('Office Visit', '3bdcf474-69fc-4eab-9931-e9898dd14462'),
	('Field Visit', '51710aea-5e24-4a0f-8414-a7b9d4b34f24')
EXEC	
	[dbo].[SaveOffenderOnDemandSanctionDetails]
		@AutomonDatabaseName = 'CX',
		@Pin = '5824',
		@Id = 0,
		@EventDateTime = '2019-09-24'
		@Magnitude  = 'Low',
		@Response = 'Fish Bowl Drawing',
		@DateIssued = '2019-08-03',
		@IsBundled = 1,
		@IsSkipped = 0,
		@SanctionedActivityDetailsTbl = @SanctionedActivityDetailsTbl,
		@UpdatedBy = 'rawate@xpanxion.com';
---------------------------------------------------------------------------------
History:-
Date			Author			Changes
06-Nov-19		Rajesh Awate	Created.
==========================================================================================*/
CREATE PROCEDURE [dbo].[SaveOffenderOnDemandSanctionDetails]
	@AutomonDatabaseName NVARCHAR(128),
	@Pin VARCHAR(20),
	@Id INT = 0,
	@EventDateTime DATETIME,
	@Magnitude VARCHAR(255) = NULL,
	@Response VARCHAR(255) = NULL,
	@DateIssued VARCHAR(255) = NULL,
	@IsBundled BIT,
	@IsSkipped BIT,
	@SanctionedActivityDetailsTbl [dbo].[SanctionedActivityDetailsTbl] READONLY,
	@UpdatedBy VARCHAR(255)
AS
BEGIN
	DECLARE @SQLString NVARCHAR(MAX), @ParmDefinition NVARCHAR(1000);
	
		SET @SQLString = 
		'
		--declare required variables and assign it with values
		DECLARE 
			@EnteredByPId		INT			= ISNULL((SELECT [PersonId] FROM [$AutomonDatabaseName].[dbo].[OfficerInfo] WHERE [Email] = @UpdatedBy), 0),
			@OffenderId			INT			= ISNULL((SELECT [Id] FROM [$AutomonDatabaseName].[dbo].[OffenderInfo] WHERE [Pin] = @Pin), 0),
			@EventTypeId		INT			= (SELECT [Id] FROM [$AutomonDatabaseName].[dbo].[EventType] WHERE [PermDesc] = ''NexusContact''),
			@EventId			INT			= @Id,
			@CurrentId			INT			= 1,
			@MaxId				INT,
			@Value				VARCHAR(255);

		--add or update event for sanction
		EXEC 
			[$AutomonDatabaseName].[dbo].[UpdateEvent] 
				@EventTypeId, 
				@EventDateTime, 
				@EnteredByPId, 
				NULL, 
				0, 
				NULL, 
				NULL, 
				0, 
				@EventDateTime, 
				NULL, 
				2, --mark event status as completed
				NULL, 
				@Id = @EventId OUTPUT;

		--Contact Type
		SET @Value = 
			(
				SELECT TOP 1 
					CAST(L.[Id] AS VARCHAR(255)) 
				FROM 
					[$AutomonDatabaseName].[dbo].[Lookup] L JOIN [$AutomonDatabaseName].[dbo].[LookupType] LT 
						ON L.[LookupTypeId] = LT.[Id] 
				WHERE 
					LT.[IsActive] = 1 
					AND LT.[Description] = ''Contact Type'' 
					AND L.[IsActive] = 1 
					AND L.[PermDesc] = ''InpersonNexusSanction''
			);
		EXEC 
			[$AutomonDatabaseName].[dbo].[UpdateEventAttribute] 
				@EventId, 
				@EnteredByPId, 
				@Value, 
				NULL, 
				''CaseEventInv_ContactType'', 
				NULL, 
				NULL, 
				NULL;

		--link event to offender
		IF(@OffenderId IS NOT NULL AND @OffenderId > 0)
		BEGIN
			EXEC 
				[$AutomonDatabaseName].[dbo].[UpdateOffenderEvent] 
					@OffenderId, 
					@EventId;
		END

		--retrieve records for setting sanction attributes
		DECLARE @SanctionedEventDetails AS TABLE
		(
			[Id] INT NOT NULL IDENTITY(1, 1),
			[EventId] INT NOT NULL
		);

		INSERT INTO @SanctionedEventDetails 
		SELECT DISTINCT
			CONVERT(INT, VAOMD.[AutomonIdentifier])
		FROM
			@SanctionedActivityDetailsTbl ICDT JOIN [dbo].[vw_AllOutboundMessageDetails] VAOMD
				ON ICDT.[ActivityIdentifier] = VAOMD.[ActivityIdentifier]
		WHERE
			VAOMD.[AutomonIdentifier] IS NOT NULL
			
		-- check if there are any records to process
		IF(EXISTS(SELECT 1 FROM @SanctionedEventDetails))
		BEGIN
			--retrieve max id
			SET @MaxId = (SELECT MAX([Id]) FROM @SanctionedEventDetails);

			--loop through each record and update its attribute
			WHILE(@CurrentId <= @MaxId)
			BEGIN
				--retrieve current event id to process
				DECLARE @CurrentEventId INT;
				SET @CurrentEventId = (SELECT [EventId] FROM @SanctionedEventDetails WHERE [Id] = @CurrentId);

				--set each attribute or clear it based on @IsSkipped flag
				--Nexus Sanction Magnitude
				IF(@IsSkipped = 0 AND @Magnitude IS NOT NULL)
				BEGIN
					EXEC [$AutomonDatabaseName].[dbo].[UpdateEventAttribute] @CurrentEventId, @EnteredByPId, @Magnitude, NULL, ''NexusSanctionMagnitude'', NULL, NULL, NULL;
				END
				ELSE
				BEGIN
					EXEC [$AutomonDatabaseName].[dbo].[UpdateEventAttribute] @CurrentEventId, @EnteredByPId, NULL, NULL, ''NexusSanctionMagnitude'', NULL, NULL, NULL;
				END

				--Nexus Sanction Response
				IF(@IsSkipped = 0 AND @Response IS NOT NULL)
				BEGIN
					EXEC [$AutomonDatabaseName].[dbo].[UpdateEventAttribute] @CurrentEventId, @EnteredByPId, @Response, NULL, ''NexusSanctionResponse'', NULL, NULL, NULL;
				END
				ELSE
				BEGIN
					EXEC [$AutomonDatabaseName].[dbo].[UpdateEventAttribute] @CurrentEventId, @EnteredByPId, NULL, NULL, ''NexusSanctionResponse'', NULL, NULL, NULL;
				END

				--Nexus Sanction Date Issued
				IF(@IsSkipped = 0 AND @DateIssued IS NOT NULL)
				BEGIN
					EXEC [$AutomonDatabaseName].[dbo].[UpdateEventAttribute] @CurrentEventId, @EnteredByPId, @DateIssued, NULL, ''NexusSanctionDateIssued'', NULL, NULL, NULL;
				END
				ELSE
				BEGIN
					EXEC [$AutomonDatabaseName].[dbo].[UpdateEventAttribute] @CurrentEventId, @EnteredByPId, NULL, NULL, ''NexusSanctionDateIssued'', NULL, NULL, NULL;
				END

				--Nexus Sanction Bundled
				IF(@IsSkipped = 0 AND @IsBundled IS NOT NULL)
				BEGIN
					--convert into varchar(255) format
					DECLARE @IsBundledText VARCHAR(255);
					SET @IsBundledText = (CASE WHEN @IsBundled = 1 THEN ''True'' ELSE ''False'' END);

					EXEC [$AutomonDatabaseName].[dbo].[UpdateEventAttribute] @CurrentEventId, @EnteredByPId, @IsBundledText, NULL, ''SanctionBundled'', NULL, NULL, NULL;
				END
				ELSE
				BEGIN
					EXEC [$AutomonDatabaseName].[dbo].[UpdateEventAttribute] @CurrentEventId, @EnteredByPId, NULL, NULL, ''SanctionBundled'', NULL, NULL, NULL;
				END

				SET @CurrentId = @CurrentId + 1;
			END
		END

		SELECT @EventId;
		';


	SET @SQLString = REPLACE(@SQLString, '$AutomonDatabaseName', @AutomonDatabaseName);

	SET @ParmDefinition = '
		@Pin VARCHAR(20),
		@Id INT,
		@EventDateTime DATETIME,
		@Magnitude VARCHAR(255),
		@Response VARCHAR(255),
		@DateIssued VARCHAR(255),
		@IsBundled BIT,
		@IsSkipped BIT,
		@SanctionedActivityDetailsTbl [dbo].[SanctionedActivityDetailsTbl] READONLY,
		@UpdatedBy VARCHAR(255)';

--PRINT @SQLString;

	EXECUTE 
		sp_executesql 
			@SQLString, 
			@ParmDefinition,  
			@Pin = @Pin,
			@Id = @Id,
			@EventDateTime = @EventDateTime,
			@Magnitude = @Magnitude,
			@Response = @Response,
			@DateIssued = @DateIssued,
			@IsBundled = @IsBundled,
			@IsSkipped = @IsSkipped,
			@SanctionedActivityDetailsTbl = @SanctionedActivityDetailsTbl,
			@UpdatedBy = @UpdatedBy;
END