CREATE TABLE [dbo].[ActivitySubType] (
    [Id]          INT             IDENTITY (1, 1) NOT NULL,
    [Name]        NVARCHAR (200)  NOT NULL,
    [Description] NVARCHAR (2000) NULL,
    CONSTRAINT [PK_ActivitySubType_Id] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [UK_ActivitySubType_Name] UNIQUE NONCLUSTERED ([Name] ASC)
);

