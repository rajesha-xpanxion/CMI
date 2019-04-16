
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
		@Line1 = '12357 Old Follows Lane, St. Helens, Oregon 97409',
		@Line2 = NULL,
		@AddressType = 'Residential',
		@UpdatedBy = 'rawate@xpanxion.com';
---------------------------------------------------------------------------------
History:-
Date			Author			Changes
06-Apr-19		Rajesh Awate	Created.
==========================================================================================*/
CREATE PROCEDURE [dbo].[SaveOffenderAddressDetails]
	@AutomonDatabaseName NVARCHAR(128),
	@Pin VARCHAR(20),
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
			@EnteredByPId		INT	= ISNULL((SELECT [Id] FROM [$AutomonDatabaseName].[dbo].[Officer] WHERE [Email] = @UpdatedBy), 0),
			@PersonId			INT	= (SELECT [PersonId] FROM [$AutomonDatabaseName].[dbo].[OffenderInfo] WHERE [Pin] = @Pin),
			@AddressId			INT = 0,
			@PersonAddressId	INT = 0;

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
		';


	SET @SQLString = REPLACE(@SQLString, '$AutomonDatabaseName', @AutomonDatabaseName);

	SET @ParmDefinition = '
		@Pin VARCHAR(20),
		@Line1 VARCHAR(75),
		@Line2 VARCHAR(75),
		@AddressType VARCHAR(100),
		@UpdatedBy VARCHAR(255)';

PRINT @SQLString;

	EXECUTE sp_executesql 
				@SQLString, 
				@ParmDefinition,  
				@Pin = @Pin,
				@Line1 = @Line1,
				@Line2 = @Line2,
				@AddressType = @AddressType,
				@UpdatedBy = @UpdatedBy;
END