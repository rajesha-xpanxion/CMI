
/*==========================================================================================
Author:			Rajesh Awate
Create date:	03-Sept-19
Description:	To save given offender CAM violation details to given automon database
---------------------------------------------------------------------------------
Test execution:-
EXEC	
	[dbo].[SaveOffenderCAMViolationDetails]
		@AutomonDatabaseName = 'CX',
		@Pin = '5115',
		@Id = 0,
		@ViolationDateTime = '2019-10-04',
		@UpdatedBy = '';
---------------------------------------------------------------------------------
History:-
Date			Author			Changes
03-Sept-19		Rajesh Awate	Created.
==========================================================================================*/
CREATE PROCEDURE [dbo].[SaveOffenderCAMViolationDetails]
	@AutomonDatabaseName NVARCHAR(128),
	@Pin VARCHAR(20),
	@Id INT,
	@ViolationDateTime DATETIME,
	@UpdatedBy VARCHAR(255)
AS
BEGIN
	DECLARE @SQLString NVARCHAR(MAX), @ParmDefinition NVARCHAR(1000);
	
		SET @SQLString = 
		'
		IF(@UpdatedBy = '''')
		BEGIN
			SET @UpdatedBy = NULL;
		END

		--declare required variables and assign it with values
		DECLARE 
			@EnteredByPId					INT				= ISNULL((SELECT [PersonId] FROM [$AutomonDatabaseName].[dbo].[OfficerInfo] WHERE [Email] = @UpdatedBy), 0),
			@PersonId						INT				= (SELECT [PersonId] FROM [$AutomonDatabaseName].[dbo].[Offender] WHERE [Pin] = @Pin),
			@PersonAttributeId				INT				= @Id,
			@PermDesc						VARCHAR(50)		= ''CAMViolation'',
			@Value							VARCHAR(255)	= CONVERT(VARCHAR(255), @ViolationDateTime, 22),
			@ExistingCAMViolationDateTime	DATETIME		= NULL;

		--check if any record exists for given person attribute id, YES = retrieve details, NO = try to get details by person id
		IF(EXISTS(SELECT 1 FROM [$AutomonDatabaseName].[dbo].[PersonAttributes](''Person'', @PermDesc) WHERE [Id] = @PersonAttributeId))
		BEGIN
			SELECT
				@PersonAttributeId = [Id],
				@ExistingCAMViolationDateTime = CONVERT(DATETIME, [Value])
			FROM
				[$AutomonDatabaseName].[dbo].[PersonAttributes](''Person'', @PermDesc)
			WHERE
				[Id] = @PersonAttributeId;
		END
		ELSE
		BEGIN
			SELECT
				@PersonAttributeId = [Id],
				@ExistingCAMViolationDateTime = CONVERT(DATETIME, [Value])
			FROM
				[$AutomonDatabaseName].[dbo].[PersonAttributes](''Person'', @PermDesc)
			WHERE
				[PersonId] = @PersonId;
		END

		--check if valid offender pin is passed
		IF(@PersonId IS NOT NULL AND (@ExistingCAMViolationDateTime IS NULL OR @ExistingCAMViolationDateTime < @ViolationDateTime))
		BEGIN
			--update CAM violation attribute
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

		SELECT @PersonAttributeId;
		';

	SET @SQLString = REPLACE(@SQLString, '$AutomonDatabaseName', @AutomonDatabaseName);

	SET @ParmDefinition = '
		@Pin VARCHAR(20),
		@Id INT,
		@ViolationDateTime DATETIME,
		@UpdatedBy VARCHAR(255)';

--PRINT @SQLString;

	EXECUTE 
		sp_executesql 
			@SQLString, 
			@ParmDefinition,  
			@Pin = @Pin,
			@Id = @Id,
			@ViolationDateTime = @ViolationDateTime,
			@UpdatedBy = @UpdatedBy;
END