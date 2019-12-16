
/*==========================================================================================
Author:			Rajesh Awate
Create date:	06-Apr-19
Description:	To save given offender vehicle details to given automon database
---------------------------------------------------------------------------------
Test execution:-
EXEC	
	[dbo].[SaveOffenderVehicleDetails]
		@AutomonDatabaseName = 'CX',
		@Pin = '5824',
		@Id = 0,
		@VehicleYear = 2012,
		@Make = 'Suzuki',
		@Color = 'Grey',
		@LicensePlate = 'MH14DF5029',
		@UpdatedBy = 'rawate@xpanxion.com';
---------------------------------------------------------------------------------
History:-
Date			Author			Changes
06-Apr-19		Rajesh Awate	Created.
08-July-19		Rajesh Awate	Changes to handle update scenario.
22-Nov-19		Rajesh Awate	Fix for Bug 108315
11-Dec-19		Rajesh Awate	Changes for US116002
16-Dec-19		Rajesh Awate	Changes for US116315
==========================================================================================*/
CREATE PROCEDURE [dbo].[SaveOffenderVehicleDetails]
	@AutomonDatabaseName NVARCHAR(128),
	@Pin VARCHAR(20),
	@Id INT = 0,
	@VehicleYear INT = NULL,
	@Make VARCHAR(150),
	@Color VARCHAR(150),
	@LicensePlate VARCHAR(10),
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
			@MakeLId			INT	= 
				ISNULL
				(
					(SELECT [Id] FROM [$AutomonDatabaseName].[dbo].[LookupInfo] WHERE [LookupType] = ''Vehicle Make'' AND [Description] = @Make),
					(SELECT [Id] FROM [$AutomonDatabaseName].[dbo].[LookupInfo] WHERE [LookupType] = ''Vehicle Make'' AND [Description] = ''Other'')
				),
			@ColorLId			INT	= 
				ISNULL
				(
					(SELECT [Id] FROM [$AutomonDatabaseName].[dbo].[LookupInfo] WHERE [LookupType] = ''Vehicle Color'' AND [Description] = @Color),
					(SELECT [Id] FROM [$AutomonDatabaseName].[dbo].[LookupInfo] WHERE [LookupType] = ''Vehicle Color'' AND [Description] = ''Other'')
				),
			@AssociationLId		INT	= (SELECT [Id] FROM [$AutomonDatabaseName].[dbo].[LookupInfo] WHERE [LookupType] = ''Vehicle Association'' AND [PermDesc] = ''Offender''),
			@VehicleId			INT	= @Id,
			@BodyStyleLId       INT,
			@LicenseState       VARCHAR(3),
			@LicenseTypeLId     INT,
			@CountryLId         INT,
			@VehicleIdNumber    VARCHAR(30),
			@Description        VARCHAR(225),
			@InsuranceCompany   VARCHAR(50),
			@InsuranceCoAddress VARCHAR(255),
			@PolicyNumber       VARCHAR(30),
			@ExpirationDate     DATETIME;

		
		--check if PersonId could be found for given Pin
		IF(@PersonId IS NOT NULL AND @PersonId > 0)
		BEGIN
		
			--retrieve other details which has already been saved to avoid data loss
			SELECT
				@BodyStyleLId = [BodyStyleLId],
				@LicenseState = [LicenseState],
				@LicenseTypeLId = [LicenseTypeLId],
				@CountryLId = [CountryLId],
				@VehicleIdNumber = [VehicleIdNumber],
				@Description = [Description],
				@InsuranceCompany = [InsuranceCompany],
				@InsuranceCoAddress = [InsuranceCoAddress],
				@PolicyNumber = [PolicyNumber],
				@ExpirationDate = [ExpirationDate]
			FROM
				[dbo].[VehicleInfo]
			WHERE
				[Id] = @VehicleId;
		
			EXEC 
				[$AutomonDatabaseName].[dbo].[UpdateVehicle] 
					@PersonId, 
					@VehicleYear, 
					@MakeLId, 
					@BodyStyleLId, 
					@ColorLId, 
					@LicensePlate, 
					@LicenseState, 
					@LicenseTypeLId, 
					@CountryLId, 
					@VehicleIdNumber, 
					@AssociationLId, 
					@Description, 
					@InsuranceCompany, 
					@InsuranceCoAddress, 
					@PolicyNumber, 
					@ExpirationDate, 
					@EnteredByPId, 
					@Id = @VehicleId OUTPUT;
		END

		SELECT @VehicleId;
		';


	SET @SQLString = REPLACE(@SQLString, '$AutomonDatabaseName', @AutomonDatabaseName);

	SET @ParmDefinition = '
		@Pin VARCHAR(20),
		@Id INT,
		@VehicleYear INT,
		@Make VARCHAR(150),
		@Color VARCHAR(150),
		@LicensePlate VARCHAR(10),
		@UpdatedBy VARCHAR(255)';

--PRINT @SQLString;

	EXECUTE sp_executesql 
				@SQLString, 
				@ParmDefinition,  
				@Pin = @Pin,
				@Id = @Id,
				@VehicleYear = @VehicleYear,
				@Make = @Make,
				@Color = @Color,
				@LicensePlate = @LicensePlate,
				@UpdatedBy = @UpdatedBy;
END