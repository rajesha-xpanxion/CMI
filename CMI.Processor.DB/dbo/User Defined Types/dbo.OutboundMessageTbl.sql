CREATE TYPE [dbo].[OutboundMessageTbl] AS TABLE (
    [Id]                  INT            NOT NULL,
    [ActivityTypeName]    NVARCHAR (200) NOT NULL,
    [ActionReasonName]    NVARCHAR (200) NOT NULL,
    [ClientIntegrationId] NVARCHAR (200) NOT NULL,
    [ActivityIdentifier]  NVARCHAR (200) NOT NULL,
    [ActionOccurredOn]    DATETIME       NOT NULL,
    [ActionUpdatedBy]     NVARCHAR (200) NOT NULL,
    [Details]             NVARCHAR (MAX) NULL);



