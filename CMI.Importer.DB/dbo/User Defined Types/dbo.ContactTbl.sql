CREATE TYPE [dbo].[ContactTbl] AS TABLE (
    [Id]                 INT            NOT NULL,
    [IsImportSuccessful] BIT            NULL,
    [IntegrationId]      NVARCHAR (200) NULL,
    [ContactId]          NVARCHAR (200) NULL,
    [ContactValue]       NVARCHAR (200) NULL,
    [ContactType]        NVARCHAR (50)  NULL);



