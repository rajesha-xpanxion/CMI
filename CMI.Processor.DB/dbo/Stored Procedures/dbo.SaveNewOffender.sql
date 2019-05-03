
/*==========================================================================================
Author:			Rajesh Awate
Create date:	02-May-19
Description:	To save new offender to given automon database
---------------------------------------------------------------------------------
Test execution:-
EXEC	
	[dbo].[SaveNewOffender]
		@AutomonDatabaseName = 'CX',
		@FirstName = 'First Name 4',
		@MiddleName = 'Middle Name 4',
		@LastName = 'Last Name 4',
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
			@AnyNameId		INT				= ISNULL((SELECT TOP 1 [Id] FROM [$AutomonDatabaseName].[dbo].[AnyNameInfo] WHERE [FirstName] = @FirstName AND [MiddleName] = @MiddleName AND [LastName] = @LastName AND [ToTime] IS NULL ORDER BY [FromTime] DESC), 0),
			@PersonId		INT				= (SELECT TOP 1 [Id] FROM [$AutomonDatabaseName].[dbo].[PersonInfo] WHERE [FirstName] = @FirstName AND [MiddleName] = @MiddleName AND [LastName] = @LastName ORDER BY [FromTime] DESC),
			@PersonType		INT				= CASE WHEN @OffenderType = ''Adult'' THEN 1 WHEN @OffenderType = ''Juvenile'' THEN 16 WHEN @OffenderType = ''Officer'' THEN 4 WHEN @OffenderType = ''Associate'' THEN 2 ELSE 0 END,
			@OffenderId		INT				= ISNULL((SELECT TOP 1 [Id] FROM [$AutomonDatabaseName].[dbo].[OffenderInfo] WHERE [FirstName] = @FirstName AND [MiddleName] = @MiddleName AND [LastName] = @LastName ORDER BY [FromTime] DESC), 0),
			@Pin			VARCHAR(20)		= (SELECT TOP 1 [Pin] FROM [$AutomonDatabaseName].[dbo].[OffenderInfo] WHERE [FirstName] = @FirstName AND [MiddleName] = @MiddleName AND [LastName] = @LastName ORDER BY [FromTime] DESC);


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

		--update person
		EXEC
			[$AutomonDatabaseName].[dbo].[UpdatePerson]
				@EnteredByPId,
				@AnyNameId,
				@PersonType, --PersonType: 1 = Adult, 16 = Juvenile, 4 = Officer, 2 = Associate
				@PersonId OUTPUT;

		--update offender
		EXEC
			[$AutomonDatabaseName].[dbo].[UpdateOffender] 
				@PersonId, 
				@OffenderType, 
				0, 
				@Pin OUTPUT, 
				@OffenderId OUTPUT;

		--return pin
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