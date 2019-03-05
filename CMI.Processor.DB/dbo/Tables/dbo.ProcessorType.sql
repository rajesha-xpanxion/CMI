CREATE TABLE [dbo].[ProcessorType] (
    [Id]       INT           IDENTITY (1, 1) NOT NULL,
    [Type]     NVARCHAR (50) NOT NULL,
    [IsActive] BIT           CONSTRAINT [DF_ProcessorType_IsActive] DEFAULT ((1)) NOT NULL,
    CONSTRAINT [PK_ProcessorType] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [UK_ProcessorType_Type] UNIQUE NONCLUSTERED ([Type] ASC)
);

