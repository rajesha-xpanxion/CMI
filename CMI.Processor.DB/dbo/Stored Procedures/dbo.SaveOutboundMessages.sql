
/*==========================================================================================
Author:			Rajesh Awate
Create date:	04-Apr-19
Description:	Save received outbound message details
---------------------------------------------------------------------------------
Test execution:-
DECLARE @OutboundMessageTbl [dbo].[OutboundMessageTbl];

EXEC	
	[dbo].[SaveOutboundMessages]
		@OutboundMessageTbl = @OutboundMessageTbl
---------------------------------------------------------------------------------
History:-
Date			Author			Changes
04-Apr-19		Rajesh Awate	Created.
==========================================================================================*/
CREATE PROCEDURE [dbo].[SaveOutboundMessages]
	@OutboundMessageTbl [dbo].[OutboundMessageTbl] READONLY
AS
BEGIN

	--retrieve distinct activity type names and merge it with table
	MERGE [dbo].[ActivityType] AS Tgt
	USING
	(
		SELECT DISTINCT
			[ActivityTypeName]
		FROM
			@OutboundMessageTbl

	) AS Src
	ON (Tgt.[Name] = Src.[ActivityTypeName])
	WHEN NOT MATCHED THEN
		INSERT ([Name])
		VALUES (Src.[ActivityTypeName]);

	--retrieve distinct action reason names and merge it with table
	MERGE [dbo].[ActionReason] AS Tgt
	USING
	(
		SELECT DISTINCT
			[ActionReasonName]
		FROM
			@OutboundMessageTbl

	) AS Src
	ON (Tgt.[Name] = Src.[ActionReasonName])
	WHEN NOT MATCHED THEN
		INSERT ([Name])
		VALUES (Src.[ActionReasonName]);

	
	--merge outbound messages
	MERGE [dbo].[OutboundMessage] AS Tgt
	USING
	(
		SELECT
			OMT.[Id],
			AT.[Id] AS [ActivityTypeId],
			AR.[Id] AS [ActionReasonId],
			OMT.[ClientIntegrationId],
			OMT.[ActivityIdentifier],
			OMT.[ActionOccurredOn],
			OMT.[ActionUpdatedBy],
			OMT.[Details]
		FROM
			@OutboundMessageTbl OMT JOIN [dbo].[ActivityType] AT
				ON OMT.[ActivityTypeName] = AT.[Name]
				JOIN [dbo].[ActionReason] AR
					ON OMT.[ActionReasonName] = AR.[Name]
	) AS Src
	ON (Tgt.[Id] = Src.[Id])
	WHEN NOT MATCHED THEN  
		INSERT ([ActivityTypeId], [ActionReasonId], [ClientIntegrationId], [ActivityIdentifier], [ActionOccurredOn], [ActionUpdatedBy], [Details])
		VALUES (Src.[ActivityTypeId], Src.[ActionReasonId], Src.[ClientIntegrationId], Src.[ActivityIdentifier], Src.[ActionOccurredOn], Src.[ActionUpdatedBy], Src.[Details])
	WHEN MATCHED THEN
		UPDATE SET
			Tgt.[ActivityTypeId] = Src.[ActivityTypeId],
			Tgt.[ActionReasonId] = Src.[ActionReasonId],
			Tgt.[ClientIntegrationId] = Src.[ClientIntegrationId],
			Tgt.[ActivityIdentifier] = Src.[ActivityIdentifier],
			Tgt.[ActionOccurredOn] = Src.[ActionOccurredOn],
			Tgt.[ActionUpdatedBy] = Src.[ActionUpdatedBy],
			Tgt.[Details] = Src.[Details];
	
END