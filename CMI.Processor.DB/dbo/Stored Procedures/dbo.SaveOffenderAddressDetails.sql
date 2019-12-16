
/*==========================================================================================
Author:			Rajesh Awate
Create date:	06-Apr-19
Description:	To save given offender address details to given automon database
---------------------------------------------------------------------------------
Test execution:-
EXEC	
	[dbo].[SaveOffenderAddressDetails]
		@AutomonDatabaseName = 'CX',
		@Pin = '10772',
		@Id = 0,
		@Line1 = '12357 Old Follows Lane, St. Helens, Oregon 97409',
		@Line2 = NULL,
		@AddressType = 'Residential',
		@UpdatedBy = 'rawate@xpanxion.com';
---------------------------------------------------------------------------------
History:-
Date			Author			Changes
06-Apr-19		Rajesh Awate	Created.
10-July-19		Rajesh Awate	Changes to handle update scenario.
13-Dec-19		Rajesh Awate	Changes for US116315
==========================================================================================*/
CREATE PROCEDURE [dbo].[SaveOffenderAddressDetails]
	@AutomonDatabaseName NVARCHAR(128),
	@Pin VARCHAR(20),
	@Id INT = 0,
	@Line1 VARCHAR(75),
	@Line2 VARCHAR(75) = NULL,
	@AddressType VARCHAR(100),
	@UpdatedBy VARCHAR(255)
AS
BEGIN
	DECLARE @SQLString NVARCHAR(MAX), @ParmDefinition NVARCHAR(1000);
	
		SET @SQLString = 
		'
		--declare required variables and assign it with values
		DECLARE 
			@EnteredByPId		INT	= ISNULL((SELECT [PersonId] FROM [$AutomonDatabaseName].[dbo].[OfficerInfo] WHERE [Email] = @UpdatedBy), 0),
			@PersonId			INT	= (SELECT [PersonId] FROM [$AutomonDatabaseName].[dbo].[OffenderInfo] WHERE [Pin] = @Pin),
			@AddressId			INT = @Id,
			@PersonAddressId	INT = 0;

		--check if PersonId could be found for given Pin
		IF(@PersonId IS NOT NULL AND @PersonId > 0)
		BEGIN
		
			EXEC 
				[$AutomonDatabaseName].[dbo].[UpdateAddress] 
					@Line1, 
					@Line2, 
					NULL, 
					NULL, 
					NULL, 
					NULL, 
					NULL, 
					@EnteredByPId = @EnteredByPId, 
					@Id = @AddressId OUTPUT;

			EXEC 
				[$AutomonDatabaseName].[dbo].[UpdatePersonAddress] 
					@PersonId, 
					NULL, 
					@AddressId, 
					0, 
					@PersonAddressId OUTPUT, 
					@AddressType;
		END
		
		SELECT @AddressId;
		';


	SET @SQLString = REPLACE(@SQLString, '$AutomonDatabaseName', @AutomonDatabaseName);

	SET @ParmDefinition = '
		@Pin VARCHAR(20),
		@Id INT,
		@Line1 VARCHAR(75),
		@Line2 VARCHAR(75),
		@AddressType VARCHAR(100),
		@UpdatedBy VARCHAR(255)';

--PRINT @SQLString;

	EXECUTE sp_executesql 
				@SQLString, 
				@ParmDefinition,  
				@Pin = @Pin,
				@Id = @Id,
				@Line1 = @Line1,
				@Line2 = @Line2,
				@AddressType = @AddressType,
				@UpdatedBy = @UpdatedBy;
END