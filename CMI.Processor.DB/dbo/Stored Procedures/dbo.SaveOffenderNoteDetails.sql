

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
		@Id = 0,
		@Text = 'test comment 2',
		@AuthorEmail = 'rawate@xpanxion.com',
		@Date = @CurrentTimestamp;
---------------------------------------------------------------------------------
History:-
Date			Author			Changes
03-Apr-19		Rajesh Awate	Created.
10-May-19		Rajesh Awate	Changes to use new event type for notes.
05-July-19		Rajesh Awate	Changes to handle update scenario.
13-Dec-19		Rajesh Awate	Changes for US116315
==========================================================================================*/
CREATE PROCEDURE [dbo].[SaveOffenderNoteDetails]
	@AutomonDatabaseName NVARCHAR(128),
	@Pin VARCHAR(20),
	@Id INT = 0,
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
			@EventTypeId	INT				= (SELECT [Id] FROM [$AutomonDatabaseName].[dbo].[EventType] WHERE [PermDesc] = ''NexusCaseNote''),
			@EventDateTime	DATETIME		= @Date,
			@EnteredByPId	INT				= ISNULL((SELECT [PersonId] FROM [$AutomonDatabaseName].[dbo].[OfficerInfo] WHERE [Email] = @AuthorEmail), 0),
			@Comment		VARCHAR(MAX)	= @Text,
			@EventId		INT				= @Id,
			@OffenderId		INT				= (SELECT [Id] FROM [$AutomonDatabaseName].[dbo].[OffenderInfo] WHERE [Pin] = @Pin);

		--SELECT @EventTypeId, @EventDateTime, @EnteredByPId, @Comment, @EventId, @OffenderId;

		--check if OffenderId could be found for given Pin
		IF(@OffenderId IS NOT NULL AND @OffenderId > 0)
		BEGIN

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

		
			--associate newly added event with given offender
			IF(@EventId IS NOT NULL)
			BEGIN
				EXEC 
					[$AutomonDatabaseName].[dbo].[UpdateOffenderEvent] 
						@OffenderId, 
						@EventId;
			END
		END

		SELECT @EventId;
		';

	SET @SQLString = REPLACE(@SQLString, '$AutomonDatabaseName', @AutomonDatabaseName);

	SET @ParmDefinition = '
		@Pin VARCHAR(200),
		@Id INT,
		@Text VARCHAR(MAX),
		@AuthorEmail VARCHAR(200),
		@Date DATETIME';

--PRINT @SQLString;

	EXECUTE sp_executesql 
				@SQLString, 
				@ParmDefinition,  
				@Pin = @Pin,
				@Id = @Id,
				@Text = @Text,
				@AuthorEmail = @AuthorEmail,
				@Date = @Date;
END