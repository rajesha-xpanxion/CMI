
/*==========================================================================================
Author:			Rajesh Awate
Create date:	03-Sept-19
Description:	To save given offender GPS violation details to given automon database
---------------------------------------------------------------------------------
Test execution:-
EXEC	
	[dbo].[SaveOffenderGPSViolationDetails]
		@AutomonDatabaseName = 'CX',
		@Pin = '5115',
		@Id = 0,
		@ViolationDateTime = '09/01/2019 12:00 AM',
		@UpdatedBy = 'edcuser@scramtest.com';
---------------------------------------------------------------------------------
History:-
Date			Author			Changes
03-Sept-19		Rajesh Awate	Created.
==========================================================================================*/
CREATE PROCEDURE [dbo].[SaveOffenderGPSViolationDetails]
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
		--declare required variables and assign it with values
		DECLARE 
			@EnteredByPId		INT				= ISNULL((SELECT [PersonId] FROM [$AutomonDatabaseName].[dbo].[OfficerInfo] WHERE [Email] = @UpdatedBy), 0),
			@PersonId			INT				= (SELECT [PersonId] FROM [$AutomonDatabaseName].[dbo].[Offender] WHERE [Pin] = @Pin),
			@PersonAttributeId	INT				= @Id,
			@PermDesc			VARCHAR(50)		= ''GPSViolation'',
			@Value				VARCHAR(255)	= CONVERT(VARCHAR(255), @ViolationDateTime, 22);

		

		--check if valid offender pin is passed
		IF(@PersonId IS NOT NULL)
		BEGIN
			--update GPS Violation attribute
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