CREATE TABLE [dbo].[OutboundMessage] (
    [Id]                  INT            IDENTITY (1, 1) NOT NULL,
    [ActivityTypeId]      INT            NOT NULL,
    [ActionReasonId]      INT            NOT NULL,
    [ClientIntegrationId] NVARCHAR (200) NOT NULL,
    [ActivityIdentifier]  NVARCHAR (200) NOT NULL,
    [ActionOccurredOn]    DATETIME       NOT NULL,
    [ActionUpdatedBy]     NVARCHAR (200) NOT NULL,
    [Details]             NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_OutboundMessage] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_OutboundMessage_ActionReason] FOREIGN KEY ([ActionReasonId]) REFERENCES [dbo].[ActionReason] ([Id]),
    CONSTRAINT [FK_OutboundMessage_ActivityType] FOREIGN KEY ([ActivityTypeId]) REFERENCES [dbo].[ActivityType] ([Id])
);



