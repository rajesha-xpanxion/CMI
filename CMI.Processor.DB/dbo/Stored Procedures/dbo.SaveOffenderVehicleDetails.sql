
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
==========================================================================================*/
CREATE PROCEDURE [dbo].[SaveOffenderVehicleDetails]
	@AutomonDatabaseName NVARCHAR(128),
	@Pin VARCHAR(20),
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
			@VehicleId			INT	= ISNULL((SELECT TOP 1 [Id] FROM [$AutomonDatabaseName].[dbo].[VehicleInfo] WHERE [ToTime] IS NULL AND [LicensePlate] = @LicensePlate AND [Vyear] = @VehicleYear AND [Make] = @Make AND [BodyStyle] = @BodyStyle AND [Color] = @Color ORDER BY [FromTime] DESC), 0);

		IF(
			NOT EXISTS
			(
				SELECT 
					1 
				FROM 
					[$AutomonDatabaseName].[dbo].[VehicleInfo] 
				WHERE 
					[ToTime] IS NULL 
					AND [LicensePlate] = @LicensePlate 
					AND [Vyear] = @VehicleYear 
					AND [Make] = @Make 
					AND [BodyStyle] = @BodyStyle 
					AND [Color] = @Color
			)
		)
		BEGIN
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
					@VehicleId OUTPUT;
		END
		';


	SET @SQLString = REPLACE(@SQLString, '$AutomonDatabaseName', @AutomonDatabaseName);

	SET @ParmDefinition = '
		@Pin VARCHAR(20),
		@VehicleYear INT,
		@Make VARCHAR(150),
		@BodyStyle VARCHAR(150),
		@Color VARCHAR(150),
		@LicensePlate VARCHAR(10),
		@UpdatedBy VARCHAR(255)';

PRINT @SQLString;

	EXECUTE sp_executesql 
				@SQLString, 
				@ParmDefinition,  
				@Pin = @Pin,
				@VehicleYear = @VehicleYear,
				@Make = @Make,
				@BodyStyle = @BodyStyle,
				@Color = @Color,
				@LicensePlate = @LicensePlate,
				@UpdatedBy = @UpdatedBy;
END