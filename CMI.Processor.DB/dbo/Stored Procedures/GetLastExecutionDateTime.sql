
/*==========================================================================================
Author:			Rajesh Awate
Create date:	04-July-18
Description:	To get last execution date time for incremental & non-incremental mode of processor application
---------------------------------------------------------------------------------
Test execution:-
EXEC	
	[dbo].[GetLastExecutionDateTime]
		@ProcessorTypeId = 1
---------------------------------------------------------------------------------
History:-
Date			Author			Changes
04-July-18		Rajesh Awate	Created.
05-Mar-19		Rajesh Awate	Changes to accomodate Processor Type
14-Nov-19		Rajesh Awate	Changes to return last successful execution date time for incremental & non-incremental mode.
==========================================================================================*/
CREATE PROCEDURE [dbo].[GetLastExecutionDateTime]
	@ProcessorTypeId INT
AS
BEGIN
	DECLARE @LastIncrementalModeExecutionDateTime DATETIME, @LastNonIncrementalModeExecutionDateTime DATETIME;

	--retrieve last successful execution date time in incremental mode
	SELECT
		@LastIncrementalModeExecutionDateTime = MAX([ExecutedOn])
	FROM
		[dbo].[ProcessorExecutionHistory]
	WHERE
		[ProcessorTypeId] = @ProcessorTypeId
		AND [IsSuccessful] = 1
		AND [IsExecutedInIncrementalMode] = 1;

	--retrieve last successful execution date time in non-incremental mode
	SELECT
		@LastNonIncrementalModeExecutionDateTime = MAX([ExecutedOn])
	FROM
		[dbo].[ProcessorExecutionHistory]
	WHERE
		[ProcessorTypeId] = @ProcessorTypeId
		AND [IsSuccessful] = 1
		AND [IsExecutedInIncrementalMode] = 0;
	
	SELECT
		@LastIncrementalModeExecutionDateTime AS [LastIncrementalModeExecutionDateTime],
		@LastNonIncrementalModeExecutionDateTime AS [LastNonIncrementalModeExecutionDateTime]

END

