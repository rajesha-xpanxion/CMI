CREATE TABLE [dbo].[ActionReason] (
    [Id]          INT             IDENTITY (1, 1) NOT NULL,
    [Name]        NVARCHAR (200)  NOT NULL,
    [Description] NVARCHAR (2000) NULL,
    CONSTRAINT [PK_ActionReason_Id] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [UK_ActionReason_Name] UNIQUE NONCLUSTERED ([Name] ASC)
);

