

/*==========================================================================================
Author:			Rajesh Awate
Create date:	11-Apr-19
Description:	To save given offender email details to given automon database
---------------------------------------------------------------------------------
Test execution:-
EXEC	
	[dbo].[SaveOffenderEmailDetails]
		@AutomonDatabaseName = 'CX',
		@Pin = '5824',
		@EmailAddress = 'test1@edcgov.us',
		@UpdatedBy = 'rawate@xpanxion.com';
---------------------------------------------------------------------------------
History:-
Date			Author			Changes
11-Apr-19		Rajesh Awate	Created.
==========================================================================================*/
CREATE PROCEDURE [dbo].[SaveOffenderEmailDetails]
	@AutomonDatabaseName NVARCHAR(128),
	@Pin VARCHAR(20),
	@EmailAddress VARCHAR(80),
	@UpdatedBy VARCHAR(255)
AS
BEGIN
	DECLARE @SQLString NVARCHAR(MAX), @ParmDefinition NVARCHAR(1000);
	
		SET @SQLString = 
		'
		--declare required variables and assign it with values
		DECLARE 
			@EnteredByPId	INT				= ISNULL((SELECT [Id] FROM [$AutomonDatabaseName].[dbo].[Officer] WHERE [Email] = @UpdatedBy), 0),
			@PersonId		INT				= (SELECT [PersonId] FROM [$AutomonDatabaseName].[dbo].[OffenderInfo] WHERE [Pin] = @Pin),
			@EmailAddressId	INT				= 0;

		--check if given email address already set for person
		IF(NOT EXISTS(SELECT 1 FROM [$AutomonDatabaseName].[dbo].[Email] WHERE [PersonId] = @PersonId AND [ToTime] IS NULL AND [EmailAddress] = @EmailAddress))
		BEGIN
			EXEC 
				[$AutomonDatabaseName].[dbo].[UpdateEmail] 
					@EmailAddress, 
					@PersonId,
					@EnteredByPId,
					0,
					NULL;
		END
		';


	SET @SQLString = REPLACE(@SQLString, '$AutomonDatabaseName', @AutomonDatabaseName);

	SET @ParmDefinition = '
		@Pin VARCHAR(20),
		@EmailAddress VARCHAR(80),
		@UpdatedBy VARCHAR(255)';

PRINT @SQLString;

	EXECUTE sp_executesql 
				@SQLString, 
				@ParmDefinition,  
				@Pin = @Pin,
				@EmailAddress = @EmailAddress,
				@UpdatedBy = @UpdatedBy;
END