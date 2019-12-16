
/*==========================================================================================
Author:			Rajesh Awate
Create date:	06-Apr-19
Description:	To save given offender personal details to given automon database
---------------------------------------------------------------------------------
Test execution:-
EXEC	
	[dbo].[SaveOffenderPersonalDetails]
		@AutomonDatabaseName = 'CX',
		@Pin = '5115',
		@FirstName = 'Sarah',
		@MiddleName = NULL,
		@LastName = 'Anderson',
		@DateOfBirth = '1989-07-24',
		@Gender = 'Female',
		@Race = 'Native American',
		@UpdatedBy = 'edcuser@scramtest.com';
---------------------------------------------------------------------------------
History:-
Date			Author			Changes
06-Apr-19		Rajesh Awate	Created.
16-Apr-19		Rajesh Awate	Check if given race matches, else set it as Unknown
16-Apr-19		Rajesh Awate	Changes to save DateOfBirth & Gender information
09-July-19		Rajesh Awate	Changes to handle update scenario.
16-Dec-19		Rajesh Awate	Changes for US116315
==========================================================================================*/
CREATE PROCEDURE [dbo].[SaveOffenderPersonalDetails]
	@AutomonDatabaseName NVARCHAR(128),
	@Pin VARCHAR(20),
	@FirstName VARCHAR(32) = NULL,
	@MiddleName VARCHAR(32) = NULL,
	@LastName VARCHAR(32),
	@DateOfBirth DATETIME = NULL,
	@Gender VARCHAR(255) = NULL,
	@Race VARCHAR(150) = NULL,
	@UpdatedBy VARCHAR(255)
AS
BEGIN
	DECLARE @SQLString NVARCHAR(MAX), @ParmDefinition NVARCHAR(1000);
	
		SET @SQLString = 
		'
		--declare required variables and assign it with values
		DECLARE 
			@EnteredByPId	INT				= ISNULL((SELECT [PersonId] FROM [$AutomonDatabaseName].[dbo].[OfficerInfo] WHERE [Email] = @UpdatedBy), 0),
			@OffenderId		INT				= 0,
			@AnyNameId		INT				= 0,
			@PersonId		INT				= 0;

		--retrieve other required Ids
		SELECT
			@OffenderId = [Id],
			@PersonId = [PersonId],
			@AnyNameId = [NameId]
		FROM
			[$AutomonDatabaseName].[dbo].[OffenderInfo]
		WHERE
			[Pin] = @Pin
			
		
		--check if OffenderId could be found for given Pin
		IF(@OffenderId IS NOT NULL AND @OffenderId > 0)
		BEGIN
		
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
					@Id = @AnyNameId OUTPUT;

			EXEC 
				[$AutomonDatabaseName].[dbo].[UpdatePerson]
					@EnteredByPId,
					@AnyNameId,
					1,
					@Id = @PersonId OUTPUT;

			/*******************************RACE****************************/
			DECLARE
				@PersonAttributeId	INT			= 0,
				@Value			VARCHAR(255)	= (SELECT L.[Id] FROM [$AutomonDatabaseName].[dbo].[Lookup] L JOIN [$AutomonDatabaseName].[dbo].[LookupType] LT ON L.[LookupTypeId] = LT.[Id] WHERE L.[IsActive] = 1 AND L.[Description] = @Race AND LT.[IsActive] = 1 AND LT.[Description] = ''Race''),
				@PermDesc		VARCHAR(50)		= ''Race'';

			--check if valid race matched, otherwise set it with unknown
			IF(@Value IS NULL)
			BEGIN
				SET @Value = (SELECT L.[Id] FROM [$AutomonDatabaseName].[dbo].[Lookup] L JOIN [$AutomonDatabaseName].[dbo].[LookupType] LT ON L.[LookupTypeId] = LT.[Id] WHERE L.[IsActive] = 1 AND L.[Description] = ''Unknown'' AND LT.[IsActive] = 1 AND LT.[Description] = ''Race'');
			END
		
			--update Race attribute
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
					@PersonAttributeId OUTPUT;

			/*******************************DOB****************************/
			SELECT
				@PersonAttributeId	= 0,
				@Value				= CONVERT(VARCHAR(255), @DateOfBirth, 101),
				@PermDesc			= ''DOB'';

			--check if value is not null
			IF(@Value IS NOT NULL)
			BEGIN
				--update DOB attribute
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
						@PersonAttributeId OUTPUT;
			END

			/*******************************Gender****************************/
			SELECT
				@PersonAttributeId	= 0,
				@Value				= ISNULL(@Gender, ''Unknown''),
				@PermDesc			= ''Sex'';

			--check if value is not null
			IF(@Value IS NOT NULL)
			BEGIN
				--update Gender attribute
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
						@PersonAttributeId OUTPUT;
			END
		END
		';


	SET @SQLString = REPLACE(@SQLString, '$AutomonDatabaseName', @AutomonDatabaseName);

	SET @ParmDefinition = '
		@Pin VARCHAR(20),
		@FirstName VARCHAR(32),
		@MiddleName VARCHAR(32),
		@LastName VARCHAR(32),
		@DateOfBirth DATETIME,
		@Gender VARCHAR(255),
		@Race VARCHAR(150),
		@UpdatedBy VARCHAR(255)';

--PRINT @SQLString;

	EXECUTE sp_executesql 
				@SQLString, 
				@ParmDefinition,  
				@Pin = @Pin,
				@FirstName = @FirstName,
				@MiddleName = @MiddleName,
				@LastName = @LastName,
				@DateOfBirth = @DateOfBirth,
				@Gender = @Gender,
				@Race = @Race,
				@UpdatedBy = @UpdatedBy;
END