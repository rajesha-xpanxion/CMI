
/*==========================================================================================
Author:			Rajesh Awate
Create date:	18-Apr-19
Description:	To delete given offender vehicle details to given automon database
---------------------------------------------------------------------------------
Test execution:-
EXEC	
	[dbo].[DeleteOffenderVehicleDetails]
		@AutomonDatabaseName = 'CX',
		@Pin = '5824',
		@Id = 999,
		@UpdatedBy = 'rawate@xpanxion.com';
---------------------------------------------------------------------------------
History:-
Date			Author			Changes
18-Apr-19		Rajesh Awate	Created.
12-July-19		Rajesh Awate	Changes to delete vehicle using Id.
==========================================================================================*/
CREATE PROCEDURE [dbo].[DeleteOffenderVehicleDetails]
	@AutomonDatabaseName NVARCHAR(128),
	@Pin VARCHAR(20),
	@Id INT,
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
			@VehicleId			INT	= @Id;

		EXEC 
			[$AutomonDatabaseName].[dbo].[DeleteVehicle]
				@VehicleId;
		';


	SET @SQLString = REPLACE(@SQLString, '$AutomonDatabaseName', @AutomonDatabaseName);

	SET @ParmDefinition = '
		@Pin VARCHAR(20),
		@Id INT,
		@UpdatedBy VARCHAR(255)';

--PRINT @SQLString;

	EXECUTE sp_executesql 
				@SQLString, 
				@ParmDefinition,  
				@Pin = @Pin,
				@Id = @Id,
				@UpdatedBy = @UpdatedBy;
END