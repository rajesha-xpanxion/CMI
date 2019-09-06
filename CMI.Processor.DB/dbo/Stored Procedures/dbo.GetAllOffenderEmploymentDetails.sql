
/*==========================================================================================
Author:			Rajesh Awate
Create date:	27-Aug-18
Description:	To get all offender employment details from given automon database
---------------------------------------------------------------------------------
Test execution:-
EXEC	
	[dbo].[GetAllOffenderEmploymentDetails]
		@AutomonDatabaseName = 'CX',
		@LastExecutionDateTime = NULL
---------------------------------------------------------------------------------
History:-
Date			Author			Changes
27-Aug-18		Rajesh Awate	Created.
==========================================================================================*/
CREATE PROCEDURE [dbo].[GetAllOffenderEmploymentDetails]
	@AutomonDatabaseName NVARCHAR(128),
	@LastExecutionDateTime DATETIME = NULL
AS
BEGIN
	DECLARE @SQLString NVARCHAR(MAX), @ParmDefinition NVARCHAR(1000);
	
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

			--AND OFCI.[Logon] IN (''kplunkett'')
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

			--AND OFCI.[Logon] IN (''kplunkett'')
		';
	END


	SET @SQLString = REPLACE(@SQLString, '$AutomonDatabaseName', @AutomonDatabaseName);

	SET @ParmDefinition = '@LastExecutionDateTime DATETIME';

--PRINT @SQLString;

	EXECUTE sp_executesql 
				@SQLString, 
				@ParmDefinition,  
                @LastExecutionDateTime = @LastExecutionDateTime;
END