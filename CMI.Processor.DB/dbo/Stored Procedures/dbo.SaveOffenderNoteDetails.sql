

/*==========================================================================================
Author:			Rajesh Awate
Create date:	03-Apr-19
Description:	To save given offender note details to given automon database
---------------------------------------------------------------------------------
Test execution:-
DECLARE @CurrentTimestamp DATETIME = GETDATE();
EXEC	
	[dbo].[SaveOffenderNoteDetails]
		@AutomonDatabaseName = 'CX',
		@Pin = '5824',
		@Text = 'test comment 2',
		@AuthorEmail = 'rawate@xpanxion.com',
		@Date = @CurrentTimestamp;
---------------------------------------------------------------------------------
History:-
Date			Author			Changes
03-Apr-19		Rajesh Awate	Created.
==========================================================================================*/
CREATE PROCEDURE [dbo].[SaveOffenderNoteDetails]
	@AutomonDatabaseName NVARCHAR(128),
	@Pin VARCHAR(20),
	@Text VARCHAR(MAX),
	@AuthorEmail VARCHAR(255),
	@Date DATETIME
AS
BEGIN
	DECLARE @SQLString NVARCHAR(MAX), @ParmDefinition NVARCHAR(1000);
	
		SET @SQLString = 
		'
		--declare required variables and assign it with values
		DECLARE 
			@EventTypeId	INT				= (SELECT [Id] FROM [$AutomonDatabaseName].[dbo].[EventType] WHERE [PermDesc] = ''CaseNote''),
			@EventDateTime	DATETIME		= @Date,
			@EnteredByPId	INT				= ISNULL((SELECT [PersonId] FROM [$AutomonDatabaseName].[dbo].[OfficerInfo] WHERE [Email] = @AuthorEmail), 0),
			@Comment		VARCHAR(MAX)	= @Text,
			@EventId		INT				= 0,
			@OffenderId		INT				= (SELECT [Id] FROM [$AutomonDatabaseName].[dbo].[OffenderInfo] WHERE [Pin] = @Pin);

		--SELECT @EventTypeId, @EventDateTime, @EnteredByPId, @Comment, @EventId, @OffenderId;

		--add new event
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
				NULL,
				NULL,
				2, --status as Complete
				NULL,
				@Id = @EventId OUTPUT;

		--SELECT * FROM [$AutomonDatabaseName].[dbo].[Event] WHERE [Id] = @EventId;

		--associate newly added event with given offender
		IF(@EventId IS NOT NULL)
		BEGIN
			EXEC 
				[$AutomonDatabaseName].[dbo].[UpdateOffenderEvent] 
					@OffenderId, 
					@EventId;
		END

		--SELECT * FROM [$AutomonDatabaseName].[dbo].[OffenderEvent] WHERE [OffenderId] = @OffenderId AND [EventId] = @EventId;
		';

	SET @SQLString = REPLACE(@SQLString, '$AutomonDatabaseName', @AutomonDatabaseName);

	SET @ParmDefinition = '
		@Pin VARCHAR(200),
		@Text VARCHAR(MAX),
		@AuthorEmail VARCHAR(200),
		@Date DATETIME';

PRINT @SQLString;

	EXECUTE sp_executesql 
				@SQLString, 
				@ParmDefinition,  
				@Pin = @Pin,
				@Text = @Text,
				@AuthorEmail = @AuthorEmail,
				@Date = @Date;
END