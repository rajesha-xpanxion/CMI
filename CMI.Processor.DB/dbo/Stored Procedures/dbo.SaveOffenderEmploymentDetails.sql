

/*==========================================================================================
Author:			Rajesh Awate
Create date:	06-Apr-19
Description:	To save given offender employment details to given automon database
---------------------------------------------------------------------------------
Test execution:-
EXEC	
	[dbo].[SaveOffenderEmploymentDetails]
		@AutomonDatabaseName = 'CX',
		@Pin = '5115',
		@Id = 0,
		@OrganizationName = 'new org 111',
		@OrganizationAddress = 'Test Org Address 2',
		@OrganizationPhone = '(911)007-4444',
		@PayFrequency = 'Hourly',
		@PayRate = '68.00',
		@WorkType = 'Observation123',
		@JobTitle = 'Supervisor',
		@UpdatedBy = 'rawate@xpanxion.com';
---------------------------------------------------------------------------------
History:-
Date			Author			Changes
06-Apr-19		Rajesh Awate	Created.
08-July-19		Rajesh Awate	Changes to handle update scenario.
==========================================================================================*/
CREATE PROCEDURE [dbo].[SaveOffenderEmploymentDetails]
	@AutomonDatabaseName NVARCHAR(128),
	@Pin VARCHAR(20),
	@Id INT = 0,
	@OrganizationName VARCHAR(100),
	@OrganizationAddress VARCHAR(75) = NULL,
	@OrganizationPhone VARCHAR(15) = NULL,
	@PayFrequency varchar(255) = NULL,
	@PayRate varchar(255) = NULL,
	@WorkType varchar(255) = NULL,
	@JobTitle varchar(255) = NULL,
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
			@OrganizationTypeId			INT	= (SELECT [Id] FROM [$AutomonDatabaseName].[dbo].[OrganizationType] WHERE [Description] = ''Other'' AND [IsActive] = 1),
			@OrganizationId				INT	= @Id,
			@PersonAssociationId		INT	= 0,
			@PersonAssociationRoleId	INT	= 0,
			@AddressId					INT = ISNULL((SELECT TOP 1 [Id] FROM [$AutomonDatabaseName].[dbo].[AddressInfo] WHERE [Line1] = @OrganizationAddress ORDER BY [FromTime] DESC), 0),
			@PhoneNumberId				INT	= ISNULL((SELECT TOP 1 [Id] FROM [$AutomonDatabaseName].[dbo].[PhoneNumberInfo] WHERE [Phone] = @OrganizationPhone ORDER BY [FromTime] DESC), 0);

		SET @PersonAssociationId = ISNULL((SELECT TOP 1 [Id] FROM [$AutomonDatabaseName].[dbo].[PersonAssociationInfo] WHERE [PersonId] = @PersonId AND [OrganizationId] = @OrganizationId ORDER BY [FromTime] DESC), 0);

		--organization address
		IF(@OrganizationAddress IS NOT NULL)
		BEGIN
			EXEC 
				[$AutomonDatabaseName].[dbo].[UpdateAddress] 
					@OrganizationAddress, 
					NULL, 
					NULL, 
					NULL, 
					NULL, 
					NULL, 
					NULL, 
					@EnteredByPId = @EnteredByPId, 
					@Id = @AddressId OUTPUT;
		END
		
		--organization phone
		IF(@OrganizationPhone IS NOT NULL)
		BEGIN
			EXEC 
				[$AutomonDatabaseName].[dbo].[UpdatePhoneNumber] 
					@EnteredByPId, 
					@OrganizationPhone, 
					NULL, 
					NULL, 
					NULL, 
					@PhoneNumberId OUTPUT;
		END
		
		--organization
		EXEC 
			[$AutomonDatabaseName].[dbo].[UpdateOrganization]
				@OrganizationName,
				@OrganizationTypeId,
				@EnteredByPId,
				1, --@IsActive
				NULL, --@Description
				@AddressId, --@AddressId - need to retrieve separately
				@PhoneNumberId, --@PhoneId - need to retrieve separately
				NULL, --@FaxId - can be ignored
				NULL, --@MailingAddressId - can be ignored
				NULL, --@ForeignKeyCode - can be ignored
				NULL, --@ParentId - can be ignored
				@OrganizationId OUTPUT;

		--person associattion
		EXEC 
			[$AutomonDatabaseName].[dbo].[UpdatePersonAssociation] 
				@PersonId,
				@EnteredByPId,
				NULL, --@RelatedPersonId
				@OrganizationId,
				NULL, --@FromTime
				NULL, --@ToTime
				NULL, --@DeletedByPId
				0, --@LivesWith
				0, --@LegalCustody           BIT      = 0,
				0, --@PhysicalCustody        BIT      = 0,
				0, --@FinancialSupport       BIT      = 0,
				NULL, --@LegalDesignationLId    INT      = NULL,
				NULL, --@NoteId                 INT      = NULL,
				@PersonAssociationId OUTPUT;


		--person association role
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

		
		--pay frequency
		IF(@PayFrequency IS NOT NULL)
		BEGIN
			EXEC
				[$AutomonDatabaseName].[dbo].[UpdatePersonAssociationAttribute]
					@PersonAssociationId, 
					@EnteredByPId,
					@PayFrequency,
					NULL,
					''PayFrequency'',
					NULL,
					NULL,
					NULL;
		END

		--pay rate
		IF(@PayRate IS NOT NULL)
		BEGIN
			EXEC
				[$AutomonDatabaseName].[dbo].[UpdatePersonAssociationAttribute]
					@PersonAssociationId, 
					@EnteredByPId,
					@PayRate,
					NULL,
					''PayRate'',
					NULL,
					NULL,
					NULL;
		END

		--work type
		IF(@WorkType IS NOT NULL)
		BEGIN
			EXEC
				[$AutomonDatabaseName].[dbo].[UpdatePersonAssociationAttribute]
					@PersonAssociationId, 
					@EnteredByPId,
					@WorkType,
					NULL,
					''WorkType'',
					NULL,
					NULL,
					NULL;
		END

		--job title
		IF(@WorkType IS NOT NULL)
		BEGIN
			EXEC
				[$AutomonDatabaseName].[dbo].[UpdatePersonAssociationAttribute]
					@PersonAssociationId, 
					@EnteredByPId,
					@JobTitle,
					NULL,
					''JobTitle'',
					NULL,
					NULL,
					NULL;
		END
		
		SELECT @OrganizationId;
		';


	SET @SQLString = REPLACE(@SQLString, '$AutomonDatabaseName', @AutomonDatabaseName);

	SET @ParmDefinition = '
		@Pin VARCHAR(20),
		@Id INT,
		@OrganizationName VARCHAR(100),
		@OrganizationAddress VARCHAR(75),
		@OrganizationPhone VARCHAR(15),
		@PayFrequency varchar(255),
		@PayRate varchar(255),
		@WorkType varchar(255),
		@JobTitle varchar(255),
		@UpdatedBy VARCHAR(255)';

PRINT @SQLString;

	EXECUTE sp_executesql 
				@SQLString, 
				@ParmDefinition,  
				@Pin = @Pin,
				@Id = @Id,
				@OrganizationName = @OrganizationName,
				@OrganizationAddress = @OrganizationAddress,
				@OrganizationPhone = @OrganizationPhone,
				@PayFrequency = @PayFrequency,
				@PayRate = @PayRate,
				@WorkType = @WorkType,
				@JobTitle = @JobTitle,
				@UpdatedBy = @UpdatedBy;
END