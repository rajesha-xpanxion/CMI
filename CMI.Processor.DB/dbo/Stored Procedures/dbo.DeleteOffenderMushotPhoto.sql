


/*==========================================================================================
Author:			Rajesh Awate
Create date:	30-Aug-19
Description:	To delete given offender mugshot photo to given automon database
---------------------------------------------------------------------------------
Test execution:-
EXEC	
	[dbo].[DeleteOffenderMushotPhoto]
		@AutomonDatabaseName = 'CX',
		@Id = 0;
---------------------------------------------------------------------------------
History:-
Date			Author			Changes
30-Aug-19		Rajesh Awate	Created.
==========================================================================================*/
CREATE PROCEDURE [dbo].[DeleteOffenderMushotPhoto]
	@AutomonDatabaseName NVARCHAR(128),
	@Id INT
AS
BEGIN
	DECLARE @SQLString NVARCHAR(MAX), @ParmDefinition NVARCHAR(1000);
	
		SET @SQLString = 
		'
		--delete document for given id
		EXEC 
			[$AutomonDatabaseName].[dbo].[DeleteDocument]
				@Id = @id;

		';


	SET @SQLString = REPLACE(@SQLString, '$AutomonDatabaseName', @AutomonDatabaseName);

	SET @ParmDefinition = '
		@Id INT';

--PRINT @SQLString;

	EXECUTE sp_executesql 
				@SQLString, 
				@ParmDefinition,  
				@Id = @Id;
END