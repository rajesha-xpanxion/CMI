


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
		@Text = 'test comment 1',
		@AuthorEmail = 'rawate@xpanxion.com',
		@Date = @CurrentTimestamp;
---------------------------------------------------------------------------------
History:-
Date			Author			Changes
03-Apr-19		Rajesh Awate	Created.
==========================================================================================*/
CREATE PROCEDURE [dbo].[SaveOffenderNoteDetails]
	@AutomonDatabaseName NVARCHAR(128),
	@Pin VARCHAR(200),
	@Text VARCHAR(MAX),
	@AuthorEmail VARCHAR(200),
	@Date DATETIME
AS
BEGIN
	DECLARE @SQLString NVARCHAR(MAX), @ParmDefinition NVARCHAR(1000);
	
		SET @SQLString = 
		'
		DECLARE 
			@EventTypeId	INT				= (SELECT [Id] FROM [$AutomonDatabaseName].[dbo].[EventType] WHERE [PermDesc] = ''CaseNote''),
			@EventDateTime	DATETIME		= @Date,
			@EnteredByPId	INT				= 0,	--(SELECT [Id] FROM [$AutomonDatabaseName].[dbo].[PersonInfo] WHERE [EmailAddress] = @AuthorEmail),
			@Comment		VARCHAR(MAX)	= @Text,
			@EventId		INT				= 0,	--to retrieve based on passed identifier
			@OffenderId		INT				= (SELECT [Id] FROM [$AutomonDatabaseName].[dbo].[OffenderInfo] WHERE [Pin] = @Pin);

		EXEC 
			[$AutomonDatabaseName].[dbo].[UpdateEvent] 
				@EventTypeId, 
				@EventDateTime, 
				@EnteredByPId, 
				@Comment, 
				@Id = @EventId OUTPUT;

		EXEC 
			[$AutomonDatabaseName].[dbo].[UpdateOffenderEvent] 
				@OffenderId, 
				@EventId;
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