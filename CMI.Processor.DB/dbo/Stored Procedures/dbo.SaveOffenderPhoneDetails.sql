
/*==========================================================================================
Author:			Rajesh Awate
Create date:	06-Apr-19
Description:	To save given offender phone details to given automon database
---------------------------------------------------------------------------------
Test execution:-
EXEC	
	[dbo].[SaveOffenderPhoneDetails]
		@AutomonDatabaseName = 'CX',
		@Pin = '10772',
		@Id = 0,
		@Phone = '(541)555-8778',
		@PhoneNumberType = 'Residential',
		@UpdatedBy = 'rawate@xpanxion.com';
---------------------------------------------------------------------------------
History:-
Date			Author			Changes
06-Apr-19		Rajesh Awate	Created.
09-July-19		Rajesh Awate	Changes to handle update scenario.
16-Dec-19		Rajesh Awate	Changes for US116315
==========================================================================================*/
CREATE PROCEDURE [dbo].[SaveOffenderPhoneDetails]
	@AutomonDatabaseName NVARCHAR(128),
	@Pin VARCHAR(20),
	@Id INT = 0,
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
			@PersonId		INT				= (SELECT [PersonId] FROM [$AutomonDatabaseName].[dbo].[OffenderInfo] WHERE [Pin] = @Pin),
			@PhoneNumberId	INT				= @Id,
			@PersonPhoneId	INT				= 0;
		
		
		--check if PersonId could be found for given Pin
		IF(@PersonId IS NOT NULL AND @PersonId > 0)
		BEGIN
		
			EXEC 
				[$AutomonDatabaseName].[dbo].[UpdatePhoneNumber] 
					@EnteredByPId, 
					@Phone, 
					NULL, 
					NULL, 
					NULL, 
					@Id = @PhoneNumberId OUTPUT;

			EXEC 
				[$AutomonDatabaseName].[dbo].[UpdatePersonPhone] 
					@PersonId, 
					@PhoneNumberId, 
					NULL, 
					0, 
					@PersonPhoneId OUTPUT, 
					@PhoneNumberType, 
					NULL, 
					NULL;
		END
		
		SELECT @PhoneNumberId;
		';


	SET @SQLString = REPLACE(@SQLString, '$AutomonDatabaseName', @AutomonDatabaseName);

	SET @ParmDefinition = '
		@Pin VARCHAR(20),
		@Id INT,
		@Phone VARCHAR(15),
		@PhoneNumberType VARCHAR(100),
		@UpdatedBy VARCHAR(255)';

--PRINT @SQLString;

	EXECUTE sp_executesql 
				@SQLString, 
				@ParmDefinition,  
				@Pin = @Pin,
				@Id = @Id,
				@Phone = @Phone,
				@PhoneNumberType = @PhoneNumberType,
				@UpdatedBy = @UpdatedBy;
END