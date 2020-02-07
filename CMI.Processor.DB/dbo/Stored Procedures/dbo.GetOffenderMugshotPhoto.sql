
/*==========================================================================================
Author:			Rajesh Awate
Create date:	28-Aug-19
Description:	To get mugshot photo of given offender
---------------------------------------------------------------------------------
Test execution:-
EXEC	
	[dbo].[GetOffenderMugshotPhoto]
		@AutomonDatabaseName = 'CX',
		@Pin = '5115'
---------------------------------------------------------------------------------
History:-
Date			Author			Changes
28-Aug-19		Rajesh Awate	Created.
09-Jan-20		Rajesh Awate	Changes to retrieve Document Id & Document Date
==========================================================================================*/
CREATE PROCEDURE [dbo].[GetOffenderMugshotPhoto]
	@AutomonDatabaseName NVARCHAR(128),
	@Pin VARCHAR(20)
AS
BEGIN
	DECLARE @SQLString NVARCHAR(MAX), @ParmDefinition NVARCHAR(1000);
	
	SET @SQLString = 
	'
	SELECT TOP 1
		DI.[Pin],
		DI.[Id] AS [DocumentId],
		DI.[DocumentDate],
		DD.[DocumentData]
	FROM
		[$AutomonDatabaseName].[dbo].[DocumentInfo] DI JOIN [$AutomonDatabaseName].[dbo].[DocumentData] DD
			ON DI.[DocumentDataId] = DD.[Id]
	WHERE
		DI.[Pin] = @Pin
		AND [DocumentTypeDescription] = ''Photo-Mugshot''
	ORDER BY
		DI.[EnteredDateTime] DESC
	';

	SET @SQLString = REPLACE(@SQLString, '$AutomonDatabaseName', @AutomonDatabaseName);

	SET @ParmDefinition = '@Pin VARCHAR(20)';

--PRINT @SQLString;

	EXECUTE sp_executesql 
				@SQLString, 
				@ParmDefinition,  
                @Pin = @Pin;
END