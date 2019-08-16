


/*==========================================================================================
Author:			Rajesh Awate
Create date:	08-May-19
Description:	To save offender drug test appointment details to given automon database
---------------------------------------------------------------------------------
Test execution:-
DECLARE @CurrentDate DATETIME = GETDATE();
EXEC	
	[dbo].[SaveOffenderDrugTestAppointmentDetails]
		@AutomonDatabaseName = 'CX',
		@Pin = '5824',
		@Id = 0,
		@StartDate = @CurrentDate,
		@EndDate = @CurrentDate,
		@Status = 2, --Pending = 0, Missed = 16, Cancelled = 10, Complete = 2
		@IsOffenderPresent BIT = 0,
		@UpdatedBy = 'rawate@xpanxion.com';
---------------------------------------------------------------------------------
History:-
Date			Author			Changes
08-May-19		Rajesh Awate	Created.
27-May-19		Rajesh Awate	Updated event type.
08-July-19		Rajesh Awate	Changes to handle update scenario.
14-Aug-19		Rajesh Awate	Changes for saving is offender present flag
==========================================================================================*/
CREATE PROCEDURE [dbo].[SaveOffenderDrugTestAppointmentDetails]
	@AutomonDatabaseName NVARCHAR(128),
	@Pin VARCHAR(20),
	@Id INT = 0,
	@StartDate DATETIME,
	@EndDate DATETIME,
	@Status INT = 0, --Pending = 0, Missed = 16, Cancelled = 10, Complete = 2
	@IsOffenderPresent BIT = 0,
	@UpdatedBy VARCHAR(255)
AS
BEGIN
	DECLARE @SQLString NVARCHAR(MAX), @ParmDefinition NVARCHAR(1000);
	
		SET @SQLString = 
		'
		--declare required variables and assign it with values
		DECLARE 
			@EnteredByPId		INT	= ISNULL((SELECT [PersonId] FROM [$AutomonDatabaseName].[dbo].[OfficerInfo] WHERE [Email] = @UpdatedBy), 0),
			@PersonId			INT	= ISNULL((SELECT [PersonId] FROM [$AutomonDatabaseName].[dbo].[OffenderInfo] WHERE [Pin] = @Pin), 0),
			@OffenderId			INT	= ISNULL((SELECT [Id] FROM [$AutomonDatabaseName].[dbo].[OffenderInfo] WHERE [Pin] = @Pin), 0),
			@EventTypeId		INT	= (SELECT [Id] FROM [$AutomonDatabaseName].[dbo].[EventType] WHERE [PermDesc] = ''NexusContact''),
			@EventId			INT = @Id,
			@Value				VARCHAR(255);

		EXEC 
			[$AutomonDatabaseName].[dbo].[UpdateEvent] 
				@EventTypeId, 
				@StartDate, 
				@EnteredByPId, 
				NULL, 
				0, 
				NULL, 
				NULL, 
				0, 
				@EndDate, 
				NULL, 
				@Status, 
				NULL, 
				@Id = @EventId OUTPUT;

		IF(@OffenderId IS NOT NULL AND @OffenderId > 0)
		BEGIN
			EXEC 
				[$AutomonDatabaseName].[dbo].[UpdateOffenderEvent] 
					@OffenderId, 
					@EventId;
		END

		--Offender Present
		SET @Value = CASE WHEN @IsOffenderPresent = 1 THEN ''True'' ELSE ''False'' END;
		EXEC 
			[$AutomonDatabaseName].[dbo].[UpdateEventAttribute] 
				@EventId, 
				@EnteredByPId, 
				@Value, 
				NULL, 
				''SupvContactOPresent'', 
				NULL, 
				NULL, 
				NULL;

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
					AND L.[PermDesc] = ''InPersonTreatmentFacility''
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
		
		SELECT @EventId;
		';


	SET @SQLString = REPLACE(@SQLString, '$AutomonDatabaseName', @AutomonDatabaseName);

	SET @ParmDefinition = '
		@Pin VARCHAR(20),
		@Id INT,
		@StartDate DATETIME,
		@EndDate DATETIME,
		@Status INT,
		@IsOffenderPresent BIT,
		@UpdatedBy VARCHAR(255)';

--PRINT @SQLString;

	EXECUTE sp_executesql 
				@SQLString, 
				@ParmDefinition,  
				@Pin = @Pin,
				@Id = @Id,
				@StartDate = @StartDate,
				@EndDate = @EndDate,
				@Status = @Status,
				@IsOffenderPresent = @IsOffenderPresent,
				@UpdatedBy = @UpdatedBy;
END