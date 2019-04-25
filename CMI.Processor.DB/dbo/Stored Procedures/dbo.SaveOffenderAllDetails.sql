
/*==========================================================================================
Author:			Rajesh Awate
Create date:	15-Apr-19
Description:	To save given offender details to given automon database
---------------------------------------------------------------------------------
Test execution:-
DECLARE @CurrentTimestamp DATETIME = GETDATE();
EXEC	
	[dbo].[SaveOffenderAllDetails]
		@AutomonDatabaseName = 'CX',
		@Pin = '5824',
		@FirstName = 'John',
		@MiddleName = 'Brent',
		@LastName = 'Aitkens',
		@Race = 'White',

		@DateOfBirth = '1976-02-10',
		@TimeZone  = 'Pacific',
		@OffenderType = 'Adult',
		@Gender = 'Male',
		@EmailAddress = 'test1@test.com',
		@Line1 = 'test line 1',
		@Line2 = 'test line 2',
		@AddressType  = 'Home',
		@Phone = '(888) 888-8877',
		@PhoneNumberType = 'MobilePhone',

		@UpdatedBy = 'rawate@xpanxion.com';
---------------------------------------------------------------------------------
History:-
Date			Author			Changes
15-Apr-19		Rajesh Awate	Created.
==========================================================================================*/
CREATE PROCEDURE [dbo].[SaveOffenderAllDetails]
	@AutomonDatabaseName NVARCHAR(128),
	@Pin VARCHAR(20),
	@FirstName VARCHAR(32) = NULL,
	@MiddleName VARCHAR(32) = NULL,
	@LastName VARCHAR(32),
	@Race VARCHAR(150),
	
	@DateOfBirth DATETIME,
	@TimeZone VARCHAR(150),
	@OffenderType VARCHAR(10),
	@Gender VARCHAR(150),
	@EmailAddress VARCHAR(150),
	@Line1 VARCHAR(75),
	@Line2 VARCHAR(75),
	@AddressType VARCHAR(100),
	@Phone VARCHAR(15),
	@PhoneNumberType VARCHAR(100),

	@UpdatedBy VARCHAR(255)
AS
BEGIN
	DECLARE @SQLString NVARCHAR(MAX), @ParmDefinition NVARCHAR(1000);
	
		SET @SQLString = 
		'
		--declare required variables and assign it with values
		DECLARE 
			@EnteredByPId	INT				= ISNULL((SELECT [PersonId] FROM [$AutomonDatabaseName].[dbo].[OfficerInfo] WHERE [Email] = @UpdatedBy), 0),
			@OffenderId		INT				= (SELECT [Id] FROM [$AutomonDatabaseName].[dbo].[OffenderInfo] WHERE [Pin] = @Pin),
			@AnyNameId		INT				= ISNULL((SELECT [Id] FROM [$AutomonDatabaseName].[dbo].[AnyName] WHERE [Firstname] = @FirstName AND [LastName] = @LastName AND [ToTime] IS NULL), 0),
			@PersonType		INT				= CASE WHEN @OffenderType = ''Adult'' THEN 1 WHEN @OffenderType = ''Juvenile'' THEN 16 WHEN @OffenderType = ''Officer'' THEN 4 WHEN @OffenderType = ''Associate'' THEN 2 ELSE 0 END,
			@PersonId		INT				= (SELECT [PersonId] FROM [$AutomonDatabaseName].[dbo].[OffenderInfo] WHERE [Pin] = @Pin),
			@Value			VARCHAR(255)	= (SELECT L.[Id] FROM [$AutomonDatabaseName].[dbo].[Lookup] L JOIN [$AutomonDatabaseName].[dbo].[LookupType] LT ON L.[LookupTypeId] = LT.[Id] WHERE L.[IsActive] = 1 AND L.[PermDesc] = @Race AND LT.[IsActive] = 1 AND LT.[Description] = ''Race''),
			@PermDesc		VARCHAR(50)		= ''Race'';


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

		
		IF(@Race IS NOT NULL)
		BEGIN
			EXEC 
				[$AutomonDatabaseName].[dbo].[UpdatePersonAttribute] 
					@PersonId, 
					@Value, 
					@EnteredByPId, 
					0,
					NULL,
					@PermDesc, 
					NULL,
					NULL,
					0;
		END

		';


	SET @SQLString = REPLACE(@SQLString, '$AutomonDatabaseName', @AutomonDatabaseName);

	SET @ParmDefinition = '
		@Pin VARCHAR(20),
		@FirstName VARCHAR(32),
		@MiddleName VARCHAR(32),
		@LastName VARCHAR(32),
		@Race VARCHAR(150),
		@UpdatedBy VARCHAR(255)';

PRINT @SQLString;

	--EXECUTE sp_executesql 
	--			@SQLString, 
	--			@ParmDefinition,  
	--			@Pin = @Pin,
	--			@FirstName = @FirstName,
	--			@MiddleName = @MiddleName,
	--			@LastName = @LastName,
	--			@Race = @Race,
	--			@UpdatedBy = @UpdatedBy;
END