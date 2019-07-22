






/*==========================================================================================
Author:			Rajesh Awate
Create date:	04-Apr-19
Description:	Save received outbound message details
---------------------------------------------------------------------------------
Test execution:-
DECLARE @OutboundMessageTbl [dbo].[OutboundMessageTbl];
DECLARE @ReceivedOn DATETIME = GETDATE();
INSERT INTO @OutboundMessageTbl
	([Id], [ActivityTypeName], [ActivitySubTypeName], [ActionReasonName], [ClientIntegrationId], [ActivityIdentifier], [ActionOccurredOn], [ActionUpdatedBy], [Details], [IsProcessed], [IsSuccessful], [ReceivedOn])
VALUES
	(0, 'Test Activity Type 1', 'Test Sub Activity Type 1', 'Test Action Reason 1', 'Test Client Integration Id 1', 'Test Activity Identifier 1', GETDATE(), 'Test Action Updated By 1', 'Test Details 1', 0, 0, @ReceivedOn),
	(0, 'Test Activity Type 2', NULL, 'Test Action Reason 2', 'Test Client Integration Id 2', 'Test Activity Identifier 2', GETDATE(), 'Test Action Updated By 2', 'Test Details 2', 0, 0, @ReceivedOn)
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
	DECLARE @OutboundMessageOutput TABLE
	(
	   [ActionType] VARCHAR(50),
	   [Id] INT,
	   [ActivityTypeId] INT,
	   [ActivitySubTypeId] INT,
	   [ActionReasonId] INT,
	   [ClientIntegrationId] NVARCHAR(200),
	   [ActivityIdentifier] NVARCHAR(200),
	   [ActionOccurredOn] DATETIME,
	   [ActionUpdatedBy] NVARCHAR(200),
	   [Details] NVARCHAR(MAX),
	   [ReceivedOn] DATETIME,
	   [IsSuccessful] BIT,
	   [ErrorDetails] NVARCHAR(MAX),
	   [RawData] NVARCHAR(MAX),
	   [IsProcessed] BIT,
	   [AutomonIdentifier] NVARCHAR(200)
	);


	--retrieve distinct activity type names and merge it with table
	MERGE [dbo].[ActivityType] AS Tgt
	USING
	(
		SELECT DISTINCT
			[ActivityTypeName]
		FROM
			@OutboundMessageTbl
		WHERE
			[ActivityTypeName] IS NOT NULL
			AND [ActivityTypeName] <> ''

	) AS Src
	ON (Tgt.[Name] = Src.[ActivityTypeName])
	WHEN NOT MATCHED THEN
		INSERT ([Name])
		VALUES (Src.[ActivityTypeName]);

	--retrieve distinct activity sub type names and merge it with table
	MERGE [dbo].[ActivitySubType] AS Tgt
	USING
	(
		SELECT DISTINCT
			[ActivitySubTypeName]
		FROM
			@OutboundMessageTbl
		WHERE
			[ActivitySubTypeName] IS NOT NULL
			AND [ActivitySubTypeName] <> ''

	) AS Src
	ON (Tgt.[Name] = Src.[ActivitySubTypeName])
	WHEN NOT MATCHED THEN
		INSERT ([Name])
		VALUES (Src.[ActivitySubTypeName]);

	--retrieve distinct action reason names and merge it with table
	MERGE [dbo].[ActionReason] AS Tgt
	USING
	(
		SELECT DISTINCT
			[ActionReasonName]
		FROM
			@OutboundMessageTbl
		WHERE
			[ActionReasonName] IS NOT NULL
			AND [ActionReasonName] <> ''

	) AS Src
	ON (Tgt.[Name] = Src.[ActionReasonName])
	WHEN NOT MATCHED THEN
		INSERT ([Name])
		VALUES (Src.[ActionReasonName]);

	--merge outbound messages
	MERGE [dbo].[OutboundMessage] AS Tgt
	USING
	(
		SELECT DISTINCT
			OMT.[Id],
			AT.[Id] AS [ActivityTypeId],
			AST.[Id] AS [ActivitySubTypeId],
			AR.[Id] AS [ActionReasonId],
			OMT.[ClientIntegrationId],
			OMT.[ActivityIdentifier],
			OMT.[ActionOccurredOn],
			OMT.[ActionUpdatedBy],
			OMT.[Details],
			OMT.[ReceivedOn],
			OMT.[IsSuccessful],
			OMT.[ErrorDetails],
			OMT.[RawData],
			OMT.[IsProcessed],
			OMT.[AutomonIdentifier]
		FROM
			@OutboundMessageTbl OMT JOIN [dbo].[ActivityType] AT
				ON OMT.[ActivityTypeName] = AT.[Name]
				JOIN [dbo].[ActionReason] AR
					ON OMT.[ActionReasonName] = AR.[Name]
					LEFT JOIN [dbo].[ActivitySubType] AST
						ON OMT.[ActivitySubTypeName] = AST.[Name]
	) AS Src
	ON (Tgt.[Id] = Src.[Id])
	WHEN NOT MATCHED THEN  
		INSERT 
		(
			[ActivityTypeId], [ActivitySubTypeId], [ActionReasonId], [ClientIntegrationId], [ActivityIdentifier], 
			[ActionOccurredOn], [ActionUpdatedBy], [Details], [ReceivedOn], [IsSuccessful], [RawData], [IsProcessed],
			[AutomonIdentifier]
		)
		VALUES 
		(
			Src.[ActivityTypeId], Src.[ActivitySubTypeId], Src.[ActionReasonId], Src.[ClientIntegrationId], Src.[ActivityIdentifier], 
			Src.[ActionOccurredOn], Src.[ActionUpdatedBy], Src.[Details], Src.[ReceivedOn], Src.[IsSuccessful], Src.[RawData], Src.[IsProcessed],
			Src.[AutomonIdentifier]
		)
	WHEN MATCHED THEN
		UPDATE SET
			Tgt.[ActivityTypeId] = Src.[ActivityTypeId],
			Tgt.[ActivitySubTypeId] = Src.[ActivitySubTypeId],
			Tgt.[ActionReasonId] = Src.[ActionReasonId],
			Tgt.[ClientIntegrationId] = Src.[ClientIntegrationId],
			Tgt.[ActivityIdentifier] = Src.[ActivityIdentifier],
			Tgt.[ActionOccurredOn] = Src.[ActionOccurredOn],
			Tgt.[ActionUpdatedBy] = Src.[ActionUpdatedBy],
			Tgt.[Details] = Src.[Details],
			Tgt.[ReceivedOn] = Src.[ReceivedOn],
			Tgt.[IsSuccessful] = Src.[IsSuccessful],
			Tgt.[ErrorDetails] = Src.[ErrorDetails],
			Tgt.[RawData] = Src.[RawData],
			Tgt.[IsProcessed] = Src.[IsProcessed],
			Tgt.[AutomonIdentifier] = Src.[AutomonIdentifier]
	OUTPUT
		$action, inserted.* INTO @OutboundMessageOutput;


	SELECT DISTINCT
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
		[IsProcessed],
		[AutomonIdentifier]
	FROM 
		[dbo].[vw_AllOutboundMessageDetails]
	WHERE
		[IsProcessed] = 1
		AND [IsSuccessful] = 0
	UNION
	SELECT DISTINCT
		OMO.[Id],
		AT.[Id],
		AT.[Name] AS [ActivityTypeName],
		AST.[Id],
		AST.[Name] AS [ActivitySubTypeName],
		AR.[Id],
		AR.[Name] AS [ActionReasonName],
		OMO.[ClientIntegrationId],
		OMO.[ActivityIdentifier],
		OMO.[ActionOccurredOn],
		OMO.[ActionUpdatedBy],
		OMO.[Details],
		OMO.[ReceivedOn],
		OMO.[IsSuccessful],
		OMO.[ErrorDetails],
		OMO.[RawData],
		OMO.[IsProcessed],
		ISNULL(
			OMO.[AutomonIdentifier], 
			(
				SELECT TOP 1 
					VWOM.[AutomonIdentifier] 
				FROM 
					[dbo].[vw_AllOutboundMessageDetails] VWOM 
				WHERE 
					VWOM.[ActivityIdentifier] = OMO.[ActivityIdentifier]
					AND VWOM.[AutomonIdentifier] IS NOT NULL
				ORDER BY 
					VWOM.[ReceivedOn] DESC
			)
		) AS [AutomonIdentifier]
	FROM
		@OutboundMessageOutput OMO JOIN [dbo].[ActivityType] AT
			ON OMO.[ActivityTypeId] = AT.[Id]
			JOIN [dbo].[ActionReason] AR
				ON OMO.[ActionReasonId] = AR.[Id]
				LEFT JOIN [dbo].[ActivitySubType] AST
					ON OMO.[ActivitySubTypeId] = AST.[Id]
					
	
END