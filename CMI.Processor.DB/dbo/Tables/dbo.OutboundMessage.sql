CREATE TABLE [dbo].[OutboundMessage] (
    [Id]                  INT            IDENTITY (1, 1) NOT NULL,
    [ActivityTypeId]      INT            NOT NULL,
    [ActivitySubTypeId]   INT            NULL,
    [ActionReasonId]      INT            NOT NULL,
    [ClientIntegrationId] NVARCHAR (200) NOT NULL,
    [ActivityIdentifier]  NVARCHAR (200) NOT NULL,
    [ActionOccurredOn]    DATETIME       NOT NULL,
    [ActionUpdatedBy]     NVARCHAR (200) NOT NULL,
    [Details]             NVARCHAR (MAX) NULL,
    [ReceivedOn]          DATETIME       CONSTRAINT [DF_OutboundMessage_ReceivedOn] DEFAULT (getdate()) NOT NULL,
    [IsSuccessful]        BIT            CONSTRAINT [DF_OutboundMessage_IsSuccessful] DEFAULT ((0)) NOT NULL,
    [ErrorDetails]        NVARCHAR (MAX) NULL,
    [RawData]             NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_OutboundMessage] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_OutboundMessage_ActionReasonId_ActionReason_Id] FOREIGN KEY ([ActionReasonId]) REFERENCES [dbo].[ActionReason] ([Id]),
    CONSTRAINT [FK_OutboundMessage_ActivitySubTypeId_ActivitySubType_Id] FOREIGN KEY ([ActivitySubTypeId]) REFERENCES [dbo].[ActivitySubType] ([Id]),
    CONSTRAINT [FK_OutboundMessage_ActivityTypeId_ActivityType_Id] FOREIGN KEY ([ActivityTypeId]) REFERENCES [dbo].[ActivityType] ([Id])
);







