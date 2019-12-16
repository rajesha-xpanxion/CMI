

/*==========================================================================================
Author:			Rajesh Awate
Create date:	18-Apr-19
Description:	To delete given offender employment details to given automon database
---------------------------------------------------------------------------------
Test execution:-
EXEC	
	[dbo].[DeleteOffenderEmploymentDetails]
		@AutomonDatabaseName = 'CX',
		@Pin = '5824',
		@Id = 0,
		@UpdatedBy = 'rawate@xpanxion.com';
---------------------------------------------------------------------------------
History:-
Date			Author			Changes
18-Apr-19		Rajesh Awate	Created.
12-July-19		Rajesh Awate	Changes to delete employment details using Id.
27-Nov-19		Rajesh Awate	Fix for issue in delete offender employer association
16-Dec-19		Rajesh Awate	Changes for US116315
==========================================================================================*/
CREATE PROCEDURE [dbo].[DeleteOffenderEmploymentDetails]
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
			@EnteredByPId				INT	= ISNULL((SELECT [PersonId] FROM [$AutomonDatabaseName].[dbo].[OfficerInfo] WHERE [Email] = @UpdatedBy), 0),
			@PersonId					INT	= (SELECT [PersonId] FROM [$AutomonDatabaseName].[dbo].[OffenderInfo] WHERE [Pin] = @Pin),
			@PersonAssociationId		INT	= @Id;

		--check if PersonId could be found for given Pin
		IF(@PersonId IS NOT NULL AND @PersonId > 0)
		BEGIN
		
			EXEC 
				[$AutomonDatabaseName].[dbo].[DeactivatePersonAssociation]
					@PersonAssociationId,
					@EnteredByPId;
		END

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