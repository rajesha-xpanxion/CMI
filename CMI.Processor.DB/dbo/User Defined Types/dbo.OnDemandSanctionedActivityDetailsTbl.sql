CREATE TYPE [dbo].[OnDemandSanctionedActivityDetailsTbl] AS TABLE (
    [TermOfSupervision] NVARCHAR (200) NOT NULL,
    [Description]       NVARCHAR (200) NOT NULL,
    [EventDateTime]     DATETIME       NOT NULL);

