CREATE TABLE [dbo].[Address] (
    [Id]                 INT             IDENTITY (1, 1) NOT NULL,
    [IsImportSuccessful] BIT             CONSTRAINT [DF_Address_IsImportSuccessful] DEFAULT ((0)) NOT NULL,
    [IntegrationId]      NVARCHAR (200)  NULL,
    [AddressId]          NVARCHAR (200)  NULL,
    [FullAddress]        NVARCHAR (2000) NULL,
    [AddressType]        NVARCHAR (200)  NULL,
    [IsPrimary]          NVARCHAR (50)   NULL,
    CONSTRAINT [PK_Address_Id] PRIMARY KEY CLUSTERED ([Id] ASC)
);





