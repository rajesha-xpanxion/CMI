
/*==========================================================================================
Author:			Rajesh Awate
Create date:	24-Apr-19
Description:	To get outbound messages which was processed but got failed
---------------------------------------------------------------------------------
Test execution:-
EXEC	
	[dbo].[GetFailedOutboundMessages]
---------------------------------------------------------------------------------
History:-
Date			Author			Changes
24-Apr-19		Rajesh Awate	Created.
==========================================================================================*/
CREATE PROCEDURE [dbo].[GetFailedOutboundMessages]
AS
BEGIN
	SELECT 
		[OutboundMessageId] AS [Id],
		[ActivityTypeId],
		[ActivityTypeName],
		[ActivitySubTypeId],
		[ActivitySubTypeName],
		[ActionReasonId],
		[ActionReasonName],
		[ClientIntegrationId],
		[ActivityIdentifier],
		[ActionOccurredOn],
		[ActionUpdatedBy],
		[Details],
		[ReceivedOn],
		[IsSuccessful],
		[ErrorDetails],
		[RawData],
		[IsProcessed]
	FROM 
		[dbo].[vw_AllOutboundMessageDetails]
	WHERE
		[IsProcessed] = 1
		AND [IsSuccessful] = 0
	
END