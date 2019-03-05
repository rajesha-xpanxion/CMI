



/*==========================================================================================
Author:			Rajesh Awate
Create date:	04-July-18
Description:	To get last execution date time of processor application
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
==========================================================================================*/
CREATE PROCEDURE [dbo].[GetLastExecutionDateTime]
	@ProcessorTypeId INT
AS
BEGIN
	
	SELECT
		MAX([ExecutedOn])
	FROM
		[dbo].[ProcessorExecutionHistory]
	WHERE
		[ProcessorTypeId] = @ProcessorTypeId
		AND [IsSuccessful] = 1
	
END

