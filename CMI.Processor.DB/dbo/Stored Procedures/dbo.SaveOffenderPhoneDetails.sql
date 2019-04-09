﻿
/*==========================================================================================
Author:			Rajesh Awate
Create date:	06-Apr-19
Description:	To save given offender phone details to given automon database
---------------------------------------------------------------------------------
Test execution:-
DECLARE @CurrentTimestamp DATETIME = GETDATE();
EXEC	
	[dbo].[SaveOffenderPhoneDetails]
		@AutomonDatabaseName = 'CX',
		@Pin = '5824',
		@Phone = '(876)673-0388',
		@PhoneNumberType = 'Message',
		@UpdatedBy = 'rawate@xpanxion.com';
---------------------------------------------------------------------------------
History:-
Date			Author			Changes
06-Apr-19		Rajesh Awate	Created.
==========================================================================================*/
CREATE PROCEDURE [dbo].[SaveOffenderPhoneDetails]
	@AutomonDatabaseName NVARCHAR(128),
	@Pin VARCHAR(20),
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
			@EnteredByPId	INT				= ISNULL((SELECT [Id] FROM [$AutomonDatabaseName].[dbo].[Officer] WHERE [Email] = @UpdatedBy), 0),
			@OffenderId		INT				= (SELECT [Id] FROM [$AutomonDatabaseName].[dbo].[OffenderInfo] WHERE [Pin] = @Pin),
			@PersonId		INT				= (SELECT [PersonId] FROM [$AutomonDatabaseName].[dbo].[OffenderInfo] WHERE [Pin] = @Pin),
			@PhoneNumberId	INT				= 0,
			@PersonPhoneId	INT				= 0;

		EXEC 
			[$AutomonDatabaseName].[dbo].[UpdatePhoneNumber] 
				@EnteredByPId, 
				@Phone, 
				NULL, 
				NULL, 
				NULL, 
				@PhoneNumberId OUTPUT;
		

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
		';


	SET @SQLString = REPLACE(@SQLString, '$AutomonDatabaseName', @AutomonDatabaseName);

	SET @ParmDefinition = '
		@Pin VARCHAR(20),
		@Phone VARCHAR(15),
		@PhoneNumberType VARCHAR(100),
		@UpdatedBy VARCHAR(255)';

PRINT @SQLString;

	EXECUTE sp_executesql 
				@SQLString, 
				@ParmDefinition,  
				@Pin = @Pin,
				@Phone = @Phone,
				@PhoneNumberType = @PhoneNumberType,
				@UpdatedBy = @UpdatedBy;
END