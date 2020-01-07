﻿
/*==========================================================================================
Author:			Rajesh Awate
Create date:	19-Dec-19
Description:	To save offender TouchPoint Check-In details to given automon database
---------------------------------------------------------------------------------
Test execution:-
DECLARE @CurrentDate DATETIME = GETDATE();
EXEC	
	[dbo].[SaveOffenderTouchPointCheckInDetails]
		@AutomonDatabaseName = 'CX',
		@Pin = '5824',
		@Id = 0,
		@StartDate = @CurrentDate,
		@Comment = 'test TouchPoint Check-In',
		@EndDate = @CurrentDate,
		@Status = 0, --Pending = 0, Missed = 16, Cancelled = 10, Complete = 2
		@UpdatedBy = 'rawate@xpanxion.com';
---------------------------------------------------------------------------------
History:-
Date			Author			Changes
19-Dec-19		Rajesh Awate	Created.
==========================================================================================*/
CREATE PROCEDURE [dbo].[SaveOffenderTouchPointCheckInDetails]
	@AutomonDatabaseName NVARCHAR(128),
	@Pin VARCHAR(20),
	@Id INT = 0,
	@StartDate DATETIME,
	@Comment VARCHAR(MAX) = NULL,
	@EndDate DATETIME,
	@Status INT = 0, --Pending = 0, Missed = 16, Cancelled = 10, Complete = 2
	@UpdatedBy VARCHAR(255)
AS
BEGIN
	DECLARE @SQLString NVARCHAR(MAX), @ParmDefinition NVARCHAR(1000);
	
		SET @SQLString = 
		'
		--declare required variables and assign it with values
		DECLARE 
			@EnteredByPId		INT	= ISNULL((SELECT [PersonId] FROM [$AutomonDatabaseName].[dbo].[OfficerInfo] WHERE [Email] = @UpdatedBy), 0),
			@OffenderId			INT	= ISNULL((SELECT [Id] FROM [$AutomonDatabaseName].[dbo].[OffenderInfo] WHERE [Pin] = @Pin), 0),
			@EventTypeId		INT	= (SELECT [Id] FROM [$AutomonDatabaseName].[dbo].[EventType] WHERE [PermDesc] = ''NexusTouchPointCheckIn''),
			@EventId			INT = @Id,
			@Value				VARCHAR(255);

		
		--check if OffenderId could be found for given Pin
		IF(@OffenderId IS NOT NULL AND @OffenderId > 0)
		BEGIN
		
			--add/update event
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
					@Id = @EventId OUTPUT;

			--link event with offender
			EXEC 
				[$AutomonDatabaseName].[dbo].[UpdateOffenderEvent] 
					@OffenderId, 
					@EventId;

			
		END
		
		SELECT @EventId;
		';


	SET @SQLString = REPLACE(@SQLString, '$AutomonDatabaseName', @AutomonDatabaseName);

	SET @ParmDefinition = '
		@Pin VARCHAR(20),
		@StartDate DATETIME,
		@Comment VARCHAR(MAX),
		@EndDate DATETIME,
		@Status INT,
		@UpdatedBy VARCHAR(255)';

--PRINT @SQLString;

	EXECUTE 
		sp_executesql 
			@SQLString, 
			@ParmDefinition,  
			@Pin = @Pin,
			@StartDate = @StartDate,
			@Comment = @Comment,
			@EndDate = @EndDate,
			@Status = @Status,
			@UpdatedBy = @UpdatedBy;
END