
/*==========================================================================================
Author:			Rajesh Awate
Create date:	04-July-18
Description:	To get save execution status of processor application
---------------------------------------------------------------------------------
Test execution:-
DECLARE @CurrentDate DATETIME = GETDATE();
EXEC	
	[dbo].[SaveExecutionStatus]
		@ProcessorTypeId = 1,
		@ExecutedOn = @CurrentDate,
		@IsExecutedInIncrementalMode = 1,
		@IsSuccessful = 1,
		@NumTaskProcessed = 0,
		@NumTaskSucceeded = 0,
		@NumTaskFailed = 0,
		@Message = 'test message',
		@ErrorDetails = NULL
---------------------------------------------------------------------------------
History:-
Date			Author			Changes
04-July-18		Rajesh Awate	Created.
05-Mar-19		Rajesh Awate	Changes to accomodate Processor Type
18-Nov-19		Rajesh Awate	Changes to save flag as per execution mode (incremental or non-incremental).
==========================================================================================*/
CREATE PROCEDURE [dbo].[SaveExecutionStatus]
	@ProcessorTypeId INT,
	@ExecutedOn DATETIME,
	@IsExecutedInIncrementalMode BIT,
	@IsSuccessful BIT,
	@NumTaskProcessed INT,
    @NumTaskSucceeded INT,
    @NumTaskFailed INT,
	@Message NVARCHAR(200) = NULL,
	@ErrorDetails NVARCHAR(MAX) = NULL
AS
BEGIN
	
	INSERT INTO [dbo].[ProcessorExecutionHistory]
	(
		[ProcessorTypeId],
		[ExecutedOn],
		[IsExecutedInIncrementalMode],
		[IsSuccessful],
		[NumTaskProcessed],
		[NumTaskSucceeded],
		[NumTaskFailed],
		[ExecutionStatusMessage],
		[ErrorDetails]
	)
	VALUES
	(
		@ProcessorTypeId,
		@ExecutedOn,
		@IsExecutedInIncrementalMode,
		@IsSuccessful,
		@NumTaskProcessed,
		@NumTaskSucceeded,
		@NumTaskFailed,
		@Message,
		@ErrorDetails
	)

	SELECT IDENT_CURRENT('[dbo].[ProcessorExecutionHistory]') AS [ProcessorExecutionHistoryId];
	
END

