



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
	@Message NVARCHAR(200) = NULL,
	@ErrorDetails NVARCHAR(MAX) = NULL
AS
BEGIN
	
	INSERT INTO [dbo].[ProcessorExecutionHistory]
	(
		[ExecutedOn],
		[IsSuccessful],
		[ExecutionStatusMessage],
		[ErrorDetails]
	)
	VALUES
	(
		@ExecutedOn,
		@IsSuccessful,
		@Message,
		@ErrorDetails
	)

	SELECT IDENT_CURRENT('[dbo].[ProcessorExecutionHistory]') AS [HistoryId];
	
END

