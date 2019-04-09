



CREATE VIEW [dbo].[vw_AllOutboundMessageDetails]
AS
	SELECT
		OM.[Id] AS [OutboundMessageId],
		AT.[Id] AS [ActivityTypeId],
		AT.[Name] AS [ActivityTypeName],
		AST.[Id] AS [ActivitySubTypeId],
		AST.[Name] AS [ActivitySubTypeName],
		AR.[Id] AS [ActionReasonId],
		AR.[Name] AS [ActionReasonName],
		OM.[ClientIntegrationId],
		OM.[ActivityIdentifier],
		OM.[ActionOccurredOn],
		OM.[ActionUpdatedBy],
		OM.[Details],
		OM.[ReceivedOn],
		OM.[IsSuccessful],
		OM.[ErrorDetails]
	FROM
		[dbo].[OutboundMessage] OM JOIN [dbo].[ActivityType] AT
			ON OM.[ActivityTypeId] = AT.[Id]
			JOIN [dbo].[ActionReason] AR
				ON OM.[ActionReasonId] = AR.[Id]
				LEFT JOIN [dbo].[ActivitySubType] AST
					ON OM.[ActivitySubTypeId] = ASt.[Id]