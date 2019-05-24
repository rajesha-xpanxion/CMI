

/*==========================================================================================
Author:			Rajesh Awate
Create date:	06-Apr-19
Description:	To save offender office visit details to given automon database
---------------------------------------------------------------------------------
Test execution:-
DECLARE @CurrentDate DATETIME = GETDATE();
EXEC	
	[dbo].[SaveOffenderOfficeVisitDetails]
		@AutomonDatabaseName = 'CX',
		@Pin = '5824',
		@StartDate DATETIME = @CurrentDate,
		@Comment VARCHAR(MAX) = 'test office visit',
		@EndDate DATETIME = @CurrentDate,
		@Status INT = 0, --Pending = 0, Missed = 16, Cancelled = 10, Complete = 2
		@IsOffenderPresent BIT = 0,
		@UpdatedBy = 'rawate@xpanxion.com';
---------------------------------------------------------------------------------
History:-
Date			Author			Changes
06-Apr-19		Rajesh Awate	Created.
==========================================================================================*/
CREATE PROCEDURE [dbo].[SaveOffenderOfficeVisitDetails]
	@AutomonDatabaseName NVARCHAR(128),
	@Pin VARCHAR(20),
	@StartDate DATETIME,
	@Comment VARCHAR(MAX),
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
			@PersonId			INT	= (SELECT [PersonId] FROM [$AutomonDatabaseName].[dbo].[OffenderInfo] WHERE [Pin] = @Pin),
			@OffenderId			INT	= (SELECT [Id] FROM [$AutomonDatabaseName].[dbo].[OffenderInfo] WHERE [Pin] = @Pin),
			@EventTypeId		INT	= (SELECT [Id] FROM [$AutomonDatabaseName].[dbo].[EventType] WHERE [PermDesc] = ''NexusContact''),
			@EventId			INT = 0,
			@Value				VARCHAR(255);

		EXEC 
			[$AutomonDatabaseName].[dbo].[UpdateEvent] 
				@EventTypeId, 
				@StartDate, 
				@EnteredByPId, 
				@Comment, 
				0, 
				NULL, 
				NULL, 
				0, 
				@EndDate, 
				NULL, 
				@Status, 
				NULL, 
				@EventId OUTPUT;

		EXEC 
			[$AutomonDatabaseName].[dbo].[UpdateOffenderEvent] 
				@OffenderId, 
				@EventId;

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
					AND L.[PermDesc] = ''ContactType_InPersonOffice''
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
		';


	SET @SQLString = REPLACE(@SQLString, '$AutomonDatabaseName', @AutomonDatabaseName);

	SET @ParmDefinition = '
		@Pin VARCHAR(20),
		@StartDate DATETIME,
		@Comment VARCHAR(MAX),
		@EndDate DATETIME,
		@Status INT,
		@IsOffenderPresent BIT,
		@UpdatedBy VARCHAR(255)';

PRINT @SQLString;

	EXECUTE sp_executesql 
				@SQLString, 
				@ParmDefinition,  
				@Pin = @Pin,
				@StartDate = @StartDate,
				@Comment = @Comment,
				@EndDate = @EndDate,
				@Status = @Status,
				@IsOffenderPresent = @IsOffenderPresent,
				@UpdatedBy = @UpdatedBy;
END