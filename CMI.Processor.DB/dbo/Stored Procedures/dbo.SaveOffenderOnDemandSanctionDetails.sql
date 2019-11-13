
/*==========================================================================================
Author:			Rajesh Awate
Create date:	06-Nov-19
Description:	To save offender on demand sanction details to given automon database
---------------------------------------------------------------------------------
Test execution:-
DECLARE @OnDemandSanctionedActivityDetailsTbl [dbo].[OnDemandSanctionedActivityDetailsTbl];
INSERT INTO @OnDemandSanctionedActivityDetailsTbl
	([TermOfSupervision], [EventDateTime], [Description])
VALUES
	('Batterer''s Program', '2019-08-13 18:00:00', '52-week Batterer’s Program; ENROLL BY ____________ with proof to Court.'),
	('100 Yards Perimeter', '2019-08-21 16:00:00', 'Defendant shall not be within 100 yards of the perimeter of places where children congregate (schools, parks, playgrounds, video arcades, swimming pools, etc.).')
EXEC	
	[dbo].[SaveOffenderOnDemandSanctionDetails]
		@AutomonDatabaseName = 'CX',
		@Pin = '5824',
		@Magnitude  = 'Moderate',
		@Response = 'Phase Demotion',
		@IsSkipped = 0,
		@Comment = 'test on demand sanction',
		@OnDemandSanctionedActivityDetailsTbl = @OnDemandSanctionedActivityDetailsTbl,
		@UpdatedBy = 'rawate@xpanxion.com';
---------------------------------------------------------------------------------
History:-
Date			Author			Changes
06-Nov-19		Rajesh Awate	Created.
==========================================================================================*/
CREATE PROCEDURE [dbo].[SaveOffenderOnDemandSanctionDetails]
	@AutomonDatabaseName NVARCHAR(128),
	@Pin VARCHAR(20),
	@Magnitude VARCHAR(255) = NULL,
	@Response VARCHAR(255) = NULL,
	@IsSkipped BIT,
	@Comment VARCHAR(MAX) = NULL,
	@OnDemandSanctionedActivityDetailsTbl [dbo].[OnDemandSanctionedActivityDetailsTbl] READONLY,
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
			@CurrentId			INT			= 1,
			@MaxId				INT,
			@Value				VARCHAR(255);

		
		DECLARE @OnDemandSanctionedEventDetails AS TABLE
		(
			[Id] INT NOT NULL IDENTITY(1, 1),
			[TermOfSupervision] NVARCHAR(200) NOT NULL,
			[Description] NVARCHAR(200) NOT NULL,
			[EventDateTime] DATETIME NOT NULL
		);

		INSERT INTO @OnDemandSanctionedEventDetails
		SELECT DISTINCT
			[TermOfSupervision],
			[Description],
			[EventDateTime]
		FROM
			@OnDemandSanctionedActivityDetailsTbl
		
		
		-- check if there are any records to process
		IF(EXISTS(SELECT 1 FROM @OnDemandSanctionedEventDetails))
		BEGIN
			--retrieve max id
			SET @MaxId = (SELECT MAX([Id]) FROM @OnDemandSanctionedEventDetails);

			--loop through each record and update its attribute
			WHILE(@CurrentId <= @MaxId)
			BEGIN
				DECLARE @EventId INT = 0;
				DECLARE @TermOfSupervision NVARCHAR(200);
				DECLARE @Description NVARCHAR(200);
				DECLARE @EventDateTime DATETIME;

				SELECT
					@TermOfSupervision = [TermOfSupervision],
					@Description = [Description],
					@EventDateTime = [EventDateTime]
				FROM
					@OnDemandSanctionedEventDetails
				WHERE
					[Id] = @CurrentId;
				
				--add event for each of on demand sanction
				EXEC 
					[$AutomonDatabaseName].[dbo].[UpdateEvent] 
						@EventTypeId, 
						@EventDateTime, 
						@EnteredByPId, 
						@Comment, 
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
				
				
				--set each attribute or clear it based on @IsSkipped flag
				--Nexus Sanction Magnitude
				IF(@IsSkipped = 0 AND @Magnitude IS NOT NULL)
				BEGIN
					EXEC [$AutomonDatabaseName].[dbo].[UpdateEventAttribute] @EventId, @EnteredByPId, @Magnitude, NULL, ''NexusSanctionMagnitude'', NULL, NULL, NULL;
				END
				ELSE
				BEGIN
					EXEC [$AutomonDatabaseName].[dbo].[UpdateEventAttribute] @EventId, @EnteredByPId, NULL, NULL, ''NexusSanctionMagnitude'', NULL, NULL, NULL;
				END

				--Nexus Sanction Response
				IF(@IsSkipped = 0 AND @Response IS NOT NULL)
				BEGIN
					EXEC [$AutomonDatabaseName].[dbo].[UpdateEventAttribute] @EventId, @EnteredByPId, @Response, NULL, ''NexusSanctionResponse'', NULL, NULL, NULL;
				END
				ELSE
				BEGIN
					EXEC [$AutomonDatabaseName].[dbo].[UpdateEventAttribute] @EventId, @EnteredByPId, NULL, NULL, ''NexusSanctionResponse'', NULL, NULL, NULL;
				END

				--Nexus Sanction Terms Of Supervision
				IF(@IsSkipped = 0 AND @TermOfSupervision IS NOT NULL)
				BEGIN
					EXEC [$AutomonDatabaseName].[dbo].[UpdateEventAttribute] @EventId, @EnteredByPId, @TermOfSupervision, NULL, ''NexusODTC'', NULL, NULL, NULL;
				END
				ELSE
				BEGIN
					EXEC [$AutomonDatabaseName].[dbo].[UpdateEventAttribute] @EventId, @EnteredByPId, NULL, NULL, ''NexusODTC'', NULL, NULL, NULL;
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
		@IsSkipped BIT,
		@Comment VARCHAR(MAX),
		@OnDemandSanctionedActivityDetailsTbl [dbo].[OnDemandSanctionedActivityDetailsTbl] READONLY,
		@UpdatedBy VARCHAR(255)';

--PRINT @SQLString;

	EXECUTE 
		sp_executesql 
			@SQLString, 
			@ParmDefinition,  
			@Pin = @Pin,
			@Magnitude = @Magnitude,
			@Response = @Response,
			@IsSkipped = @IsSkipped,
			@Comment = @Comment,
			@OnDemandSanctionedActivityDetailsTbl = @OnDemandSanctionedActivityDetailsTbl,
			@UpdatedBy = @UpdatedBy;
END