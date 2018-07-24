CREATE TABLE [dbo].[ProcessorExecutionHistory] (
    [Id]                     INT            IDENTITY (1, 1) NOT NULL,
    [ExecutedOn]             DATETIME       CONSTRAINT [DF_ProcessorExecutionHistory_ExecutedOn] DEFAULT (getdate()) NOT NULL,
    [IsSuccessful]           BIT            CONSTRAINT [DF_ProcessorExecutionHistory_IsSuccessful] DEFAULT ((1)) NOT NULL,
    [ExecutionStatusMessage] NVARCHAR (200) NULL,
    [ErrorDetails]           NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_ProcessorExecutionHistory_Id] PRIMARY KEY CLUSTERED ([Id] ASC)
);

