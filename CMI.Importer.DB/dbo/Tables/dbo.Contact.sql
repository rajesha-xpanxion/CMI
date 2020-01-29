CREATE TABLE [dbo].[Contact] (
    [Id]                 INT            IDENTITY (1, 1) NOT NULL,
    [IsImportSuccessful] BIT            CONSTRAINT [DF_Contact_IsImportSuccessful] DEFAULT ((0)) NOT NULL,
    [IntegrationId]      NVARCHAR (200) NULL,
    [ContactId]          NVARCHAR (200) NULL,
    [ContactValue]       NVARCHAR (200) NULL,
    [ContactType]        NVARCHAR (50)  NULL,
    CONSTRAINT [PK_Contact_Id] PRIMARY KEY CLUSTERED ([Id] ASC)
);





