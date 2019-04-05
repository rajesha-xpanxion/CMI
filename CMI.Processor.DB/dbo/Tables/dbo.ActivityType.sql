CREATE TABLE [dbo].[ActivityType] (
    [Id]          INT             IDENTITY (1, 1) NOT NULL,
    [Name]        NVARCHAR (200)  NOT NULL,
    [Description] NVARCHAR (2000) NULL,
    CONSTRAINT [PK_ActivityType_Id] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [UK_ActivityType_Name] UNIQUE NONCLUSTERED ([Name] ASC)
);

