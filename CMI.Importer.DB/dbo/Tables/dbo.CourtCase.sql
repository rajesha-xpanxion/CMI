CREATE TABLE [dbo].[CourtCase] (
    [Id]                 INT             IDENTITY (1, 1) NOT NULL,
    [IsImportSuccessful] BIT             CONSTRAINT [DF_CourtCase_IsImportSuccessful] DEFAULT ((0)) NOT NULL,
    [IntegrationId]      NVARCHAR (200)  NULL,
    [CaseNumber]         NVARCHAR (200)  NULL,
    [CaseDate]           NVARCHAR (200)  NULL,
    [Status]             NVARCHAR (10)   NULL,
    [EndDate]            NVARCHAR (200)  NULL,
    [EarlyReleaseDate]   NVARCHAR (200)  NULL,
    [EndReason]          NVARCHAR (2000) NULL,
    CONSTRAINT [PK_CourtCase_Id] PRIMARY KEY CLUSTERED ([Id] ASC)
);





