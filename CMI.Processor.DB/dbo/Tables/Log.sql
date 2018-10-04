CREATE TABLE [dbo].[Log] (
    [Id]            INT            IDENTITY (1, 1) NOT NULL,
    [LogLevel]      NVARCHAR (50)  NOT NULL,
    [OperationName] NVARCHAR (256) NULL,
    [MethodName]    NVARCHAR (256) NULL,
    [ErrorType]     INT            NULL,
    [Message]       NVARCHAR (MAX) NULL,
    [StackTrace]    NVARCHAR (MAX) NULL,
    [CustomParams]  NVARCHAR (MAX) NULL,
    [CreateDate]    DATETIME       CONSTRAINT [DF_Log_CreateDate] DEFAULT (getdate()) NOT NULL,
    [SourceData]    NVARCHAR (MAX) NULL,
    [DestData]      NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_Log_Id] PRIMARY KEY CLUSTERED ([Id] ASC) WITH (FILLFACTOR = 90)
);



