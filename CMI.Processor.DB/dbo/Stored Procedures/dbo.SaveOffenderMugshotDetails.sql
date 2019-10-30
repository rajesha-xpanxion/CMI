


/*==========================================================================================
Author:			Rajesh Awate
Create date:	30-Aug-19
Description:	To save given offender mugshot details to given automon database
---------------------------------------------------------------------------------
Test execution:-
EXEC	
	[dbo].[SaveOffenderMugshotDetails]
		@AutomonDatabaseName = 'CX',
		@Pin = '5115',
		@DocumentData = NULL,
		@DocumentId = NULL,
		@DocumentDate = NULL,
		@UpdatedBy = 'edcuser@scramtest.com';
---------------------------------------------------------------------------------
History:-
Date			Author			Changes
30-Aug-19		Rajesh Awate	Created.
==========================================================================================*/
CREATE PROCEDURE [dbo].[SaveOffenderMugshotDetails]
	@AutomonDatabaseName NVARCHAR(128),
	@Pin VARCHAR(20),
	@DocumentData IMAGE = NULL,
	@DocumentId INT = NULL,
	@DocumentDate DATETIME = NULL,
	@UpdatedBy VARCHAR(255)
AS
BEGIN
	DECLARE @SQLString NVARCHAR(MAX), @ParmDefinition NVARCHAR(1000);
	
		SET @SQLString = 
		'
		--declare required variables and assign it with values
		DECLARE 
			@EnteredByPId	INT	= ISNULL((SELECT [PersonId] FROM [$AutomonDatabaseName].[dbo].[OfficerInfo] WHERE [Email] = @UpdatedBy), 0),
			@PersonId		INT	= (SELECT [PersonId] FROM [$AutomonDatabaseName].[dbo].[OffenderInfo] WHERE [Pin] = @Pin),
			@DocumentDataId	INT = (SELECT [DocumentDataId] FROM [$AutomonDatabaseName].[dbo].[DocumentInfo] WHERE [Id] = @DocumentId),
			@DocumentTypeId INT;

		--retrieve document type id
		SELECT
			@DocumentTypeId = [Id]
		FROM
			[$AutomonDatabaseName].[dbo].[DocumentType]
		WHERE
			[Description] = ''Photo-Mugshot'';
		
		--update document
		EXEC 
			[$AutomonDatabaseName].[dbo].[UpdateDocument]
				@PersonId = @PersonId,
				@DocumentFormat = 0 ,
				@EnteredByPId = @EnteredByPId,
				@DocumentDataId = @DocumentDataId,
				@EnteredDateTime = NULL,
				@CaseId = NULL,
				@ViolationId = NULL,
				@DocumentTypeId = @DocumentTypeId,
				@LocalFileName = NULL,
				@Description = NULL,
				@DocumentTemplateId = NULL,
				@CompletedByPId = NULL,
				@HasThumbnail = 0,
				@ReadOnly = 1,
				@SignedByOfficerId = NULL,
				@SignedBySupervisorId = NULL,
				@SignedByBranchChiefId = NULL,
				@DocumentExecutionId = NULL,
				@DocumentDate = @DocumentDate,
				@Id = @DocumentId OUTPUT;
			
		--update document data
		EXEC 
			[$AutomonDatabaseName].[dbo].[UpdateDocumentData] 
				@FileLocation = NULL, 
				@FileName = NULL, 
				@DocumentData = @DocumentData, 
				@ThumbnailData = NULL, 
				@DocumentId = @DocumentId, 
				@Id = @DocumentDataId OUTPUT;

		--re-update document for correct document data id
		EXEC 
			[$AutomonDatabaseName].[dbo].[UpdateDocument]
				@PersonId = @PersonId,
				@DocumentFormat = 0 ,
				@EnteredByPId = @EnteredByPId,
				@DocumentDataId = @DocumentDataId,
				@EnteredDateTime = NULL,
				@CaseId = NULL,
				@ViolationId = NULL,
				@DocumentTypeId = @DocumentTypeId,
				@LocalFileName = NULL,
				@Description = NULL,
				@DocumentTemplateId = NULL,
				@CompletedByPId = NULL,
				@HasThumbnail = 0,
				@ReadOnly = 1,
				@SignedByOfficerId = NULL,
				@SignedBySupervisorId = NULL,
				@SignedByBranchChiefId = NULL,
				@DocumentExecutionId = NULL,
				@DocumentDate = @DocumentDate,
				@Id = @DocumentId OUTPUT;

		--return document id
		SELECT @DocumentId;
		';


	SET @SQLString = REPLACE(@SQLString, '$AutomonDatabaseName', @AutomonDatabaseName);

	SET @ParmDefinition = '
		@Pin VARCHAR(20),
		@DocumentData IMAGE,
		@DocumentId INT,
		@DocumentDate DATETIME,
		@UpdatedBy VARCHAR(255)';

--PRINT @SQLString;

	EXECUTE sp_executesql 
				@SQLString, 
				@ParmDefinition,  
				@Pin = @Pin,
				@DocumentData = @DocumentData,
				@DocumentId = @DocumentId,
				@DocumentDate = @DocumentDate,
				@UpdatedBy = @UpdatedBy;
END