



/*==========================================================================================
Author:			Rajesh Awate
Create date:	04-July-18
Description:	To get save execution status of processor application
---------------------------------------------------------------------------------
Test execution:-
DECLARE @CurrentDate DATETIME = GETDATE();
EXEC	
	[dbo].[SaveExecutionStatus]
		@ExecutedOn = @CurrentDate,
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
==========================================================================================*/
CREATE PROCEDURE [dbo].[SaveExecutionStatus]
	@ExecutedOn DATETIME,
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
		[ExecutedOn],
		[IsSuccessful],
		[NumTaskProcessed],
		[NumTaskSucceeded],
		[NumTaskFailed],
		[ExecutionStatusMessage],
		[ErrorDetails]
	)
	VALUES
	(
		@ExecutedOn,
		@IsSuccessful,
		@NumTaskProcessed,
		@NumTaskSucceeded,
		@NumTaskFailed,
		@Message,
		@ErrorDetails
	)

	SELECT IDENT_CURRENT('[dbo].[ProcessorExecutionHistory]') AS [ProcessorExecutionHistoryId];
	
END

