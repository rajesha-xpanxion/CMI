
/*==========================================================================================
Author:			Rajesh Awate
Create date:	27-Aug-18
Description:	To get all offender employment details from given automon database
---------------------------------------------------------------------------------
Test execution:-
DECLARE @OfficerLogonsToFilterTbl [dbo].[Varchar50Tbl];
INSERT INTO @OfficerLogonsToFilterTbl
	([Item])
VALUES
	('mboyd'),('ryost'),('kpitts'),('khennings'),('ebellew'),('gromanko'),('acraven'),('rrussell'),('kplunkett'),('sclark'),('bvogt'),('jward'),('fblanco'),('plewis'),('jwyatt')
EXEC	
	[dbo].[GetAllOffenderEmploymentDetails]
		@AutomonDatabaseName = 'CX',
		@LastExecutionDateTime = NULL,
		@OfficerLogonsToFilterTbl = @OfficerLogonsToFilterTbl
---------------------------------------------------------------------------------
History:-
Date			Author			Changes
27-Aug-18		Rajesh Awate	Created.
10-Sept-19		Rajesh Awate	Changes for integration by officer filter.
==========================================================================================*/
CREATE PROCEDURE [dbo].[GetAllOffenderEmploymentDetails]
	@AutomonDatabaseName NVARCHAR(128),
	@LastExecutionDateTime DATETIME = NULL,
	@OfficerLogonsToFilterTbl [dbo].[Varchar50Tbl] READONLY
AS
BEGIN
	DECLARE @SQLString NVARCHAR(MAX), @ParmDefinition NVARCHAR(1000);

	--check if any ooficer logon filter passed
	IF(EXISTS(SELECT 1 FROM @OfficerLogonsToFilterTbl))
	BEGIN
		SET @LastExecutionDateTime = NULL;
	END
	
	IF(@LastExecutionDateTime IS NOT NULL)
	BEGIN
		SET @SQLString = 
		'
		SELECT DISTINCT
			OI.[Pin],
			PAI.[Id],
			ORGI.[Name],
			ORGI.[FullAddress],
			ORGI.[FullPhoneNumber],
			[$AutomonDatabaseName].[dbo].[GetPersonAssociationAttributeValue](PAI.[Id], NULL, ''PayFrequency'') AS [PayFrequency],
			[$AutomonDatabaseName].[dbo].[GetPersonAssociationAttributeValue](PAI.[Id], NULL, ''PayRate'') AS [PayRate],
			[$AutomonDatabaseName].[dbo].[GetPersonAssociationAttributeValue](PAI.[Id], NULL, ''WorkType'') AS [WorkType],
			[$AutomonDatabaseName].[dbo].[GetPersonAssociationAttributeValue](PAI.[Id], NULL, ''JobTitle'') AS [JobTitle],
			1 AS [IsActive]
		FROM
			[$AutomonDatabaseName].[dbo].[OffenderInfo] OI LEFT JOIN [$AutomonDatabaseName].[dbo].[PersonAssociationInfo] PAI
				ON OI.[PersonId] = PAI.[PersonId]
				LEFT JOIN [$AutomonDatabaseName].[dbo].[PersonAssociationRoleInfo] PARI
					ON PAI.[Id] = PARI.[PersonAssociationId]
					LEFT JOIN [$AutomonDatabaseName].[dbo].[AssociationRoleInfo] ARI
						ON PARI.[AssociationRoleId] = ARI.[Id]
						LEFT JOIN [$AutomonDatabaseName].[dbo].[OrganizationInfo] ORGI
							ON PAI.[OrganizationId] = ORGI.[Id]
		WHERE
			ARI.[PermDesc] = ''Employer''
			AND (PAI.[ToTime] IS NULL AND [PARI].[ToTime] IS NULL)
			AND (PAI.[FromTime] >= @LastExecutionDateTime OR PARI.[FromTime] >= @LastExecutionDateTime OR ORGI.[EnteredDateTime] >= @LastExecutionDateTime)
		UNION
		SELECT DISTINCT
			OI.[Pin],
			PAI.[Id],
			ORGI.[Name],
			ORGI.[FullAddress],
			ORGI.[FullPhoneNumber],
			[$AutomonDatabaseName].[dbo].[GetPersonAssociationAttributeValue](PAI.[Id], NULL, ''PayFrequency'') AS [PayFrequency],
			[$AutomonDatabaseName].[dbo].[GetPersonAssociationAttributeValue](PAI.[Id], NULL, ''PayRate'') AS [PayRate],
			[$AutomonDatabaseName].[dbo].[GetPersonAssociationAttributeValue](PAI.[Id], NULL, ''WorkType'') AS [WorkType],
			[$AutomonDatabaseName].[dbo].[GetPersonAssociationAttributeValue](PAI.[Id], NULL, ''JobTitle'') AS [JobTitle],
			0 AS [IsActive]
		FROM
			[$AutomonDatabaseName].[dbo].[OffenderInfo] OI LEFT JOIN [$AutomonDatabaseName].[dbo].[PersonAssociationInfo] PAI
				ON OI.[PersonId] = PAI.[PersonId]
				LEFT JOIN [$AutomonDatabaseName].[dbo].[PersonAssociationRoleInfo] PARI
					ON PAI.[Id] = PARI.[PersonAssociationId]
					LEFT JOIN [$AutomonDatabaseName].[dbo].[AssociationRoleInfo] ARI
						ON PARI.[AssociationRoleId] = ARI.[Id]
						LEFT JOIN [$AutomonDatabaseName].[dbo].[OrganizationInfo] ORGI
							ON PAI.[OrganizationId] = ORGI.[Id]
		WHERE
			ARI.[PermDesc] = ''Employer''
			AND 
			(
				(PAI.[ToTime] IS NOT NULL AND PAI.[DeletedByPId] IS NOT NULL)
				OR 
				([PARI].[ToTime] IS NOT NULL AND PARI.[DeletedByPId] IS NOT NULL)
			)
			AND (PAI.[ToTime] >= @LastExecutionDateTime OR PARI.[ToTime] >= @LastExecutionDateTime OR ORGI.[EnteredDateTime] >= @LastExecutionDateTime)
		';
	END
	ELSE
	BEGIN
		SET @SQLString = 
		'
		SELECT DISTINCT
			OI.[Pin],
			PAI.[Id],
			ORGI.[Name],
			ORGI.[FullAddress],
			ORGI.[FullPhoneNumber],
			[$AutomonDatabaseName].[dbo].[GetPersonAssociationAttributeValue](PAI.[Id], NULL, ''PayFrequency'') AS [PayFrequency],
			[$AutomonDatabaseName].[dbo].[GetPersonAssociationAttributeValue](PAI.[Id], NULL, ''PayRate'') AS [PayRate],
			[$AutomonDatabaseName].[dbo].[GetPersonAssociationAttributeValue](PAI.[Id], NULL, ''WorkType'') AS [WorkType],
			[$AutomonDatabaseName].[dbo].[GetPersonAssociationAttributeValue](PAI.[Id], NULL, ''JobTitle'') AS [JobTitle],
			1 AS [IsActive]
		FROM
			[$AutomonDatabaseName].[dbo].[OffenderInfo] OI LEFT JOIN [$AutomonDatabaseName].[dbo].[PersonAssociationInfo] PAI
				ON OI.[PersonId] = PAI.[PersonId]
				LEFT JOIN [$AutomonDatabaseName].[dbo].[PersonAssociationRoleInfo] PARI
					ON PAI.[Id] = PARI.[PersonAssociationId]
					LEFT JOIN [$AutomonDatabaseName].[dbo].[AssociationRoleInfo] ARI
						ON PARI.[AssociationRoleId] = ARI.[Id]
						LEFT JOIN [$AutomonDatabaseName].[dbo].[OrganizationInfo] ORGI
							ON PAI.[OrganizationId] = ORGI.[Id]
							--officer wise data
							LEFT JOIN [$AutomonDatabaseName].[dbo].[OffenderCaseloadInfo] OCLI
								ON OI.[Id] = OCLI.[OffenderId]
								LEFT JOIN [$AutomonDatabaseName].[dbo].[CaseloadInfo] CLI
									ON OCLI.[CaseloadId] = CLI.[Id]
									LEFT JOIN [$AutomonDatabaseName].[dbo].[OfficerCaseloadInfo] OFCCLI
										ON CLI.[Id] = OFCCLI.[CaseloadId]
										LEFT JOIN [$AutomonDatabaseName].[dbo].[OfficerInfo] OFCI
											ON OFCCLI.[OfficerId] = OFCI.[Id]
		WHERE
			ARI.[PermDesc] = ''Employer''
			AND (PAI.[ToTime] IS NULL AND [PARI].[ToTime] IS NULL)

			--apply officer logon filter if any passed
			AND
			(
				NOT EXISTS(SELECT 1 FROM @OfficerLogonsToFilterTbl OLTF) 
				OR EXISTS(SELECT 1 FROM @OfficerLogonsToFilterTbl OLTF WHERE OLTF.[Item] = OFCI.[Logon])
			)
		UNION
		SELECT DISTINCT
			OI.[Pin],
			PAI.[Id],
			ORGI.[Name],
			ORGI.[FullAddress],
			ORGI.[FullPhoneNumber],
			[$AutomonDatabaseName].[dbo].[GetPersonAssociationAttributeValue](PAI.[Id], NULL, ''PayFrequency'') AS [PayFrequency],
			[$AutomonDatabaseName].[dbo].[GetPersonAssociationAttributeValue](PAI.[Id], NULL, ''PayRate'') AS [PayRate],
			[$AutomonDatabaseName].[dbo].[GetPersonAssociationAttributeValue](PAI.[Id], NULL, ''WorkType'') AS [WorkType],
			[$AutomonDatabaseName].[dbo].[GetPersonAssociationAttributeValue](PAI.[Id], NULL, ''JobTitle'') AS [JobTitle],
			0 AS [IsActive]
		FROM
			[$AutomonDatabaseName].[dbo].[OffenderInfo] OI LEFT JOIN [$AutomonDatabaseName].[dbo].[PersonAssociationInfo] PAI
				ON OI.[PersonId] = PAI.[PersonId]
				LEFT JOIN [$AutomonDatabaseName].[dbo].[PersonAssociationRoleInfo] PARI
					ON PAI.[Id] = PARI.[PersonAssociationId]
					LEFT JOIN [$AutomonDatabaseName].[dbo].[AssociationRoleInfo] ARI
						ON PARI.[AssociationRoleId] = ARI.[Id]
						LEFT JOIN [$AutomonDatabaseName].[dbo].[OrganizationInfo] ORGI
							ON PAI.[OrganizationId] = ORGI.[Id]
							--officer wise data
							LEFT JOIN [$AutomonDatabaseName].[dbo].[OffenderCaseloadInfo] OCLI
								ON OI.[Id] = OCLI.[OffenderId]
								LEFT JOIN [$AutomonDatabaseName].[dbo].[CaseloadInfo] CLI
									ON OCLI.[CaseloadId] = CLI.[Id]
									LEFT JOIN [$AutomonDatabaseName].[dbo].[OfficerCaseloadInfo] OFCCLI
										ON CLI.[Id] = OFCCLI.[CaseloadId]
										LEFT JOIN [$AutomonDatabaseName].[dbo].[OfficerInfo] OFCI
											ON OFCCLI.[OfficerId] = OFCI.[Id]
		WHERE
			ARI.[PermDesc] = ''Employer''
			AND 
			(
				(PAI.[ToTime] IS NOT NULL AND PAI.[DeletedByPId] IS NOT NULL)
				OR 
				([PARI].[ToTime] IS NOT NULL AND PARI.[DeletedByPId] IS NOT NULL)
			)

			--apply officer logon filter if any passed
			AND
			(
				NOT EXISTS(SELECT 1 FROM @OfficerLogonsToFilterTbl OLTF) 
				OR EXISTS(SELECT 1 FROM @OfficerLogonsToFilterTbl OLTF WHERE OLTF.[Item] = OFCI.[Logon])
			)
		';
	END


	SET @SQLString = REPLACE(@SQLString, '$AutomonDatabaseName', @AutomonDatabaseName);

	SET @ParmDefinition = '
		@LastExecutionDateTime DATETIME,
		@OfficerLogonsToFilterTbl [dbo].[Varchar50Tbl] READONLY';

--PRINT @SQLString;

	EXECUTE sp_executesql 
				@SQLString, 
				@ParmDefinition,  
                @LastExecutionDateTime = @LastExecutionDateTime,
				@OfficerLogonsToFilterTbl = @OfficerLogonsToFilterTbl;
END