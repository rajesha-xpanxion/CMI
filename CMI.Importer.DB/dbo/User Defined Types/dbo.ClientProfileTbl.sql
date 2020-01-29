CREATE TYPE [dbo].[ClientProfileTbl] AS TABLE (
    [Id]                        INT            NOT NULL,
    [IsImportSuccessful]        BIT            NULL,
    [IntegrationId]             NVARCHAR (200) NULL,
    [FirstName]                 NVARCHAR (200) NULL,
    [MiddleName]                NVARCHAR (200) NULL,
    [LastName]                  NVARCHAR (200) NULL,
    [ClientType]                NVARCHAR (200) NULL,
    [TimeZone]                  NVARCHAR (200) NULL,
    [Gender]                    NVARCHAR (50)  NULL,
    [Ethnicity]                 NVARCHAR (200) NULL,
    [DateOfBirth]               NVARCHAR (200) NULL,
    [SupervisingOfficerEmailId] NVARCHAR (200) NULL);



