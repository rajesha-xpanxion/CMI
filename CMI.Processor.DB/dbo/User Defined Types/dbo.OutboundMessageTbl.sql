CREATE TYPE [dbo].[OutboundMessageTbl] AS TABLE (
    [Id]                  INT            NOT NULL,
    [ActivityTypeName]    NVARCHAR (200) NOT NULL,
    [ActivitySubTypeName] NVARCHAR (200) NULL,
    [ActionReasonName]    NVARCHAR (200) NOT NULL,
    [ClientIntegrationId] NVARCHAR (200) NOT NULL,
    [ActivityIdentifier]  NVARCHAR (200) NOT NULL,
    [ActionOccurredOn]    DATETIME       NOT NULL,
    [ActionUpdatedBy]     NVARCHAR (200) NULL,
    [Details]             NVARCHAR (MAX) NULL,
    [IsSuccessful]        BIT            NULL,
    [ErrorDetails]        NVARCHAR (MAX) NULL,
    [RawData]             NVARCHAR (MAX) NULL,
    [IsProcessed]         BIT            NULL,
    [ReceivedOn]          DATETIME       NOT NULL,
    [AutomonIdentifier]   NVARCHAR (200) NULL);













