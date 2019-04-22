

/*==========================================================================================
Author:			Rajesh Awate
Create date:	18-Apr-19
Description:	To delete given offender employment details to given automon database
---------------------------------------------------------------------------------
Test execution:-
EXEC	
	[dbo].[DeleteOffenderEmploymntDetails]
		@AutomonDatabaseName = 'CX',
		@Pin = '5824',
		@OrganizationName = 'New Org 1',
		@OrganizationAddress = 'Test Org Address 2',
		@OrganizationPhone = '(911)007-4444',
		@UpdatedBy = 'rawate@xpanxion.com';
---------------------------------------------------------------------------------
History:-
Date			Author			Changes
18-Apr-19		Rajesh Awate	Created.
==========================================================================================*/
CREATE PROCEDURE [dbo].[DeleteOffenderEmploymentDetails]
	@AutomonDatabaseName NVARCHAR(128),
	@Pin VARCHAR(20),
	@OrganizationName VARCHAR(100),
	@OrganizationAddress VARCHAR(75),
	@OrganizationPhone VARCHAR(15),
	@UpdatedBy VARCHAR(255)
AS
BEGIN
	DECLARE @SQLString NVARCHAR(MAX), @ParmDefinition NVARCHAR(1000);
	
		SET @SQLString = 
		'
		--declare required variables and assign it with values
		DECLARE 
			@EnteredByPId				INT	= ISNULL((SELECT [PersonId] FROM [$AutomonDatabaseName].[dbo].[OfficerInfo] WHERE [Email] = @UpdatedBy), 0),
			@PersonId					INT	= (SELECT [PersonId] FROM [$AutomonDatabaseName].[dbo].[OffenderInfo] WHERE [Pin] = @Pin),
			@OrganizationId				INT	= ISNULL
				(
					(
						SELECT TOP 1 
							OI.[Id] 
						FROM
							[$AutomonDatabaseName].[dbo].[OrganizationInfo] OI LEFT JOIN [$AutomonDatabaseName].[dbo].[AddressInfo] AI
								ON OI.[FullAddress] = AI.[FullAddress]
								LEFT JOIN [$AutomonDatabaseName].[dbo].[PhoneNumberInfo] PNI
									ON OI.[FullPhoneNumber] = PNI.[FullPhone]
						WHERE
							OI.[Name] = @OrganizationName
							AND OI.[IsActive] = 1
							AND AI.[Line1] = @OrganizationAddress
							AND PNI.[Phone] = @OrganizationPhone
						ORDER BY
							OI.[EnteredDateTime] DESC
					), 
				0),
			@PersonAssociationId		INT	= 0;

		SET @PersonAssociationId = ISNULL((SELECT TOP 1 [Id] FROM [$AutomonDatabaseName].[dbo].[PersonAssociationInfo] WHERE [PersonId] = @PersonId AND [OrganizationId] = @OrganizationId ORDER BY [FromTime] DESC), 0);
		
		
		EXEC 
			[$AutomonDatabaseName].[dbo].[DeletePersonAssociation]
				@PersonAssociationId,
				@EnteredByPId;

		EXEC 
			[$AutomonDatabaseName].[dbo].[DeleteOrganization]
				@OrganizationId,
				@EnteredByPId;
		
		';


	SET @SQLString = REPLACE(@SQLString, '$AutomonDatabaseName', @AutomonDatabaseName);

	SET @ParmDefinition = '
		@Pin VARCHAR(20),
		@OrganizationName VARCHAR(100),
		@OrganizationAddress VARCHAR(75),
		@OrganizationPhone VARCHAR(15),
		@UpdatedBy VARCHAR(255)';

PRINT @SQLString;

	EXECUTE sp_executesql 
				@SQLString, 
				@ParmDefinition,  
				@Pin = @Pin,
				@OrganizationName = @OrganizationName,
				@OrganizationAddress = @OrganizationAddress,
				@OrganizationPhone = @OrganizationPhone,
				@UpdatedBy = @UpdatedBy;
END