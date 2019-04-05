
CREATE VIEW [dbo].[vw_AllOutboundMessageDetails]
AS
	SELECT
		OM.[Id] AS [OutboundMessageId],
		AT.[Id] AS [ActivityTypeId],
		AT.[Name] AS [ActivityTypeName],
		AR.[Id] AS [ActionReasonId],
		AR.[Name] AS [ActionReasonName],
		OM.[ClientIntegrationId],
		OM.[ActivityIdentifier],
		OM.[ActionOccurredOn],
		OM.[ActionUpdatedBy],
		OM.[Details]
	FROM
		[dbo].[OutboundMessage] OM JOIN [dbo].[ActivityType] AT
			ON OM.[ActivityTypeId] = AT.[Id]
			JOIN [dbo].[ActionReason] AR
				ON OM.[ActionReasonId] = AR.[Id]