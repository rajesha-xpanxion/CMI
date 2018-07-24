



/*==========================================================================================
Author:			Rajesh Awate
Create date:	04-July-18
Description:	To get last execution date time of processor application
---------------------------------------------------------------------------------
Test execution:-
EXEC	
	[dbo].[GetLastExecutionDateTime]
---------------------------------------------------------------------------------
History:-
Date			Author			Changes
04-July-18		Rajesh Awate	Created.
==========================================================================================*/
CREATE PROCEDURE [dbo].[GetLastExecutionDateTime]
AS
BEGIN
	
	SELECT
		MAX([ExecutedOn])
	FROM
		[dbo].[ProcessorExecutionHistory]
	WHERE
		[IsSuccessful] = 1
	
END

