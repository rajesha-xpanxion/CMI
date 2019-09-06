
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
		@BodyStyle = 'Sedan',
		@Color = 'Grey',
		@LicensePlate = 'MH14DF5029',
		@UpdatedBy = 'rawate@xpanxion.com';
---------------------------------------------------------------------------------
History:-
Date			Author			Changes
06-Apr-19		Rajesh Awate	Created.
08-July-19		Rajesh Awate	Changes to handle update scenario.
==========================================================================================*/
CREATE PROCEDURE [dbo].[SaveOffenderVehicleDetails]
	@AutomonDatabaseName NVARCHAR(128),
	@Pin VARCHAR(20),
	@Id INT = 0,
	@VehicleYear INT = NULL,
	@Make VARCHAR(150),
	@BodyStyle VARCHAR(150),
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
			@MakeLId			INT	= (SELECT L.[Id] FROM [$AutomonDatabaseName].[dbo].[Lookup] L JOIN [$AutomonDatabaseName].[dbo].[LookupType] LT ON L.[LookupTypeId] = LT.[Id] WHERE LT.[Description] = ''Vehicle Make'' AND L.[PermDesc] = @Make),
			@BodyStyleLId		INT	= (SELECT L.[Id] FROM [$AutomonDatabaseName].[dbo].[Lookup] L JOIN [$AutomonDatabaseName].[dbo].[LookupType] LT ON L.[LookupTypeId] = LT.[Id] WHERE LT.[Description] = ''Vehicle Body Style'' AND L.[PermDesc] = @BodyStyle),
			@ColorLId			INT	= (SELECT L.[Id] FROM [$AutomonDatabaseName].[dbo].[Lookup] L JOIN [$AutomonDatabaseName].[dbo].[LookupType] LT ON L.[LookupTypeId] = LT.[Id] WHERE LT.[Description] = ''Vehicle Color'' AND L.[PermDesc] = @Color),
			@AssociationLId		INT	= (SELECT L.[Id] FROM [$AutomonDatabaseName].[dbo].[Lookup] L JOIN [$AutomonDatabaseName].[dbo].[LookupType] LT ON L.[LookupTypeId] = LT.[Id] WHERE LT.[Description] = ''Vehicle Association'' AND L.[PermDesc] = ''Offender''),
			@VehicleId			INT	= @Id;

		EXEC 
			[$AutomonDatabaseName].[dbo].[UpdateVehicle] 
				@PersonId, 
				@VehicleYear, 
				@MakeLId, 
				@BodyStyleLId, 
				@ColorLId, 
				@LicensePlate, 
				NULL, 
				NULL, 
				NULL, 
				NULL, 
				@AssociationLId, 
				NULL, 
				NULL, 
				NULL, 
				NULL, 
				NULL, 
				@EnteredByPId, 
				@Id = @VehicleId OUTPUT;

		SELECT @VehicleId;
		';


	SET @SQLString = REPLACE(@SQLString, '$AutomonDatabaseName', @AutomonDatabaseName);

	SET @ParmDefinition = '
		@Pin VARCHAR(20),
		@Id INT,
		@VehicleYear INT,
		@Make VARCHAR(150),
		@BodyStyle VARCHAR(150),
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
				@BodyStyle = @BodyStyle,
				@Color = @Color,
				@LicensePlate = @LicensePlate,
				@UpdatedBy = @UpdatedBy;
END