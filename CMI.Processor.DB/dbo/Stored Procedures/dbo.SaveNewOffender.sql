﻿
/*==========================================================================================
Author:			Rajesh Awate
Create date:	02-May-19
Description:	To save new offender to given automon database
---------------------------------------------------------------------------------
Test execution:-
EXEC	
	[dbo].[SaveNewOffender]
		@AutomonDatabaseName = 'CX',
		@FirstName = 'John',
		@MiddleName = 'Brent',
		@LastName = 'Aitkens',
		@OffenderType = 'Adult',
		@UpdatedBy = 'rawate@xpanxion.com';
---------------------------------------------------------------------------------
History:-
Date			Author			Changes
02-May-19		Rajesh Awate	Created.
==========================================================================================*/
CREATE PROCEDURE [dbo].[SaveNewOffender]
	@AutomonDatabaseName NVARCHAR(128),
	@FirstName VARCHAR(32) = NULL,
	@MiddleName VARCHAR(32) = NULL,
	@LastName VARCHAR(32),
	@OffenderType VARCHAR(10),
	@UpdatedBy VARCHAR(255)
AS
BEGIN
	DECLARE @SQLString NVARCHAR(MAX), @ParmDefinition NVARCHAR(1000);
	
		SET @SQLString = 
		'
		--declare required variables and assign it with values
		DECLARE 
			@EnteredByPId	INT				= ISNULL((SELECT [PersonId] FROM [$AutomonDatabaseName].[dbo].[OfficerInfo] WHERE [Email] = @UpdatedBy), 0),
			@AnyNameId		INT				= 0,
			@PersonId		INT				= 0,
			@PersonType		INT				= CASE WHEN @OffenderType = ''Adult'' THEN 1 WHEN @OffenderType = ''Juvenile'' THEN 16 WHEN @OffenderType = ''Officer'' THEN 4 WHEN @OffenderType = ''Associate'' THEN 2 ELSE 0 END,
			@OffenderId		INT				= 0,
			@Pin			VARCHAR(20)		= NULL;


		--update name based on given info
		EXEC 
			[$AutomonDatabaseName].[dbo].[UpdateAnyName] 
				@LastName, 
				@EnteredByPId, 
				@FirstName, 
				@MiddleName, 
				NULL,
				NULL,
				0,
				NULL,
				@AnyNameId OUTPUT;

		EXEC
			[$AutomonDatabaseName].[dbo].[UpdatePerson]
				@EnteredByPId,
				@AnyNameId,
				@PersonType, --PersonType: 1 = Adult, 16 = Juvenile, 4 = Officer, 2 = Associate
				@PersonId OUTPUT;

		EXEC
			[$AutomonDatabaseName].[dbo].[UpdateOffender] 
				@PersonId, 
				@OffenderType, 
				0, 
				@Pin OUTPUT, 
				@OffenderId OUTPUT;

		
		SELECT @Pin;

		';


	SET @SQLString = REPLACE(@SQLString, '$AutomonDatabaseName', @AutomonDatabaseName);

	SET @ParmDefinition = '
		@FirstName VARCHAR(32),
		@MiddleName VARCHAR(32),
		@LastName VARCHAR(32),
		@OffenderType VARCHAR(10),
		@UpdatedBy VARCHAR(255)';

--PRINT @SQLString;

	EXECUTE sp_executesql 
				@SQLString, 
				@ParmDefinition,  
				@FirstName = @FirstName,
				@MiddleName = @MiddleName,
				@LastName = @LastName,
				@OffenderType = @OffenderType,
				@UpdatedBy = @UpdatedBy;
END