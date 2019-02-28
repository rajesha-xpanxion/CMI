


/*==========================================================================================
Author:			Rajesh Awate
Create date:	03-July-18
Description:	Save log details
---------------------------------------------------------------------------------
Test execution:-
EXEC	
	[dbo].[SaveLogDetails]
		@LogLevel = 'Debug',
		@OperationName = 'Process Client Profiles',
		@MethodName = 'ProcessClientProfiles',
		@ErrorType = 1,
		@Message = 'test error message',
		@StackTrace = 'test error stack trace',
		@CustomParams = 'test custom params',
		@AutomonData = 'test automon data',
		@NexusData = 'test nexus data'
---------------------------------------------------------------------------------
History:-
Date			Author			Changes
03-July-18		Rajesh Awate	Created.
04-Oct-18		Rajesh Awate	Changes for 2 new parameters
==========================================================================================*/
CREATE PROCEDURE [dbo].[SaveLogDetails]
	@LogLevel [NVARCHAR](50),
	@OperationName [NVARCHAR](100) = NULL,
	@MethodName [NVARCHAR](100) = NULL,
	@ErrorType [INT] = NULL,
	@Message [NVARCHAR](MAX) = NULL,
	@StackTrace [NVARCHAR](MAX) = NULL,
	@CustomParams [NVARCHAR](MAX) = NULL,
	@AutomonData [NVARCHAR](MAX) = NULL,
	@NexusData [NVARCHAR](MAX) = NULL
AS
BEGIN
	
	INSERT INTO [dbo].[Log]
	(
		[LogLevel],
		[OperationName],
		[MethodName],
		[ErrorType],
		[Message],
		[StackTrace],
		[CustomParams],
		[AutomonData],
		[NexusData]
	)
	VALUES
	(
		@LogLevel,
		@OperationName,
		@MethodName,
		@ErrorType,
		@Message,
		@StackTrace,
		@CustomParams,
		@AutomonData,
		@NexusData
	)

	SELECT IDENT_CURRENT('[dbo].[Log]') AS [LogId];
END












