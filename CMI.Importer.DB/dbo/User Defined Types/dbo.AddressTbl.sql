CREATE TYPE [dbo].[AddressTbl] AS TABLE (
    [Id]                 INT             NOT NULL,
    [IsImportSuccessful] BIT             NULL,
    [IntegrationId]      NVARCHAR (200)  NULL,
    [AddressId]          NVARCHAR (200)  NULL,
    [FullAddress]        NVARCHAR (2000) NULL,
    [AddressType]        NVARCHAR (200)  NULL,
    [IsPrimary]          NVARCHAR (50)   NULL);



