CREATE TABLE [dbo].[ClientProfile] (
    [Id]                        INT            IDENTITY (1, 1) NOT NULL,
    [IsImportSuccessful]        BIT            CONSTRAINT [DF_ClientProfile_IsImportSuccessful] DEFAULT ((0)) NOT NULL,
    [IntegrationId]             NVARCHAR (200) NULL,
    [FirstName]                 NVARCHAR (200) NULL,
    [MiddleName]                NVARCHAR (200) NULL,
    [LastName]                  NVARCHAR (200) NULL,
    [ClientType]                NVARCHAR (200) NULL,
    [TimeZone]                  NVARCHAR (200) NULL,
    [Gender]                    NVARCHAR (50)  NULL,
    [Ethnicity]                 NVARCHAR (200) NULL,
    [DateOfBirth]               NVARCHAR (200) NULL,
    [SupervisingOfficerEmailId] NVARCHAR (200) NULL,
    CONSTRAINT [PK_ClientProfile_Id] PRIMARY KEY CLUSTERED ([Id] ASC)
);





