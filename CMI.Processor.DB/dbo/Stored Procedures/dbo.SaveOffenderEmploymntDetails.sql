
/*==========================================================================================
Author:			Rajesh Awate
Create date:	06-Apr-19
Description:	To save given offender employment details to given automon database
---------------------------------------------------------------------------------
Test execution:-
EXEC	
	[dbo].[SaveOffenderEmploymntDetails]
		@AutomonDatabaseName = 'CX',
		@Pin = '5824',
		@OrganizationName = 'New Org 1',
		@UpdatedBy = 'rawate@xpanxion.com';
---------------------------------------------------------------------------------
History:-
Date			Author			Changes
06-Apr-19		Rajesh Awate	Created.
==========================================================================================*/
CREATE PROCEDURE [dbo].[SaveOffenderEmploymntDetails]
	@AutomonDatabaseName NVARCHAR(128),
	@Pin VARCHAR(20),
	@OrganizationName VARCHAR(100),
	@UpdatedBy VARCHAR(255)
AS
BEGIN
	DECLARE @SQLString NVARCHAR(MAX), @ParmDefinition NVARCHAR(1000);
	
		SET @SQLString = 
		'
		--declare required variables and assign it with values
		DECLARE 
			@EnteredByPId				INT	= ISNULL((SELECT [Id] FROM [$AutomonDatabaseName].[dbo].[Officer] WHERE [Email] = @UpdatedBy), 0),
			@PersonId					INT	= (SELECT [PersonId] FROM [$AutomonDatabaseName].[dbo].[OffenderInfo] WHERE [Pin] = @Pin),
			@OrganizationTypeId			INT	= (SELECT [Id] FROM [$AutomonDatabaseName].[dbo].[OrganizationType] WHERE [Description] = ''Other'' AND [IsActive] = 1),
			@OrganizationId				INT	= 0,
			@PersonAssociationId		INT	= 0, 
			@PersonAssociationRoleId	INT	= 0

		EXEC 
			[$AutomonDatabaseName].[dbo].[UpdateOrganization]
				@OrganizationName,
				@OrganizationTypeId,
				@EnteredByPId,
				1, --@IsActive
				NULL, --@Description
				NULL, --@AddressId - need to retrieve separately
				NULL, --@PhoneId - need to retrieve separately
				NULL, --@FaxId - can be ignored
				NULL, --@MailingAddressId - can be ignored
				NULL, --@ForeignKeyCode - can be ignored
				NULL, --@ParentId - can be ignored
				@OrganizationId OUTPUT;

		EXEC 
			[$AutomonDatabaseName].[dbo].[UpdatePersonAssociation] 
				@PersonId,
				@EnteredByPId,
				NULL, --@RelatedPersonId
				@OrganizationId,
				NULL, --@FromTime
				NULL, --@ToTime
				NULL, --@DeletedByPId
				NULL, --@LivesWith
				NULL, --@LegalCustody           BIT      = 0,
				NULL, --@PhysicalCustody        BIT      = 0,
				NULL, --@FinancialSupport       BIT      = 0,
				NULL, --@LegalDesignationLId    INT      = NULL,
				NULL, --@NoteId                 INT      = NULL,
				@PersonAssociationId OUTPUT;


		EXEC 
			[$AutomonDatabaseName].[dbo].[UpdatePersonAssociationRole] 
				@PersonAssociationId, 
				@EnteredByPId, 
				1, 
				''Employer'', 
				NULL, 
				NULL, 
				NULL, 
				NULL, 
				NULL, 
				@PersonAssociationRoleId OUTPUT;
		';


	SET @SQLString = REPLACE(@SQLString, '$AutomonDatabaseName', @AutomonDatabaseName);

	SET @ParmDefinition = '
		@Pin VARCHAR(20),
		@OrganizationName,
		@UpdatedBy VARCHAR(255)';

PRINT @SQLString;

	EXECUTE sp_executesql 
				@SQLString, 
				@ParmDefinition,  
				@Pin = @Pin,
				@OrganizationName = @OrganizationName,
				@UpdatedBy = @UpdatedBy;
END