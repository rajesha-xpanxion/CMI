CREATE TYPE [dbo].[CourtCaseTbl] AS TABLE (
    [Id]                 INT             NOT NULL,
    [IsImportSuccessful] BIT             NULL,
    [IntegrationId]      NVARCHAR (200)  NULL,
    [CaseNumber]         NVARCHAR (200)  NULL,
    [CaseDate]           NVARCHAR (200)  NULL,
    [Status]             NVARCHAR (10)   NULL,
    [EndDate]            NVARCHAR (200)  NULL,
    [EarlyReleaseDate]   NVARCHAR (200)  NULL,
    [EndReason]          NVARCHAR (2000) NULL);



