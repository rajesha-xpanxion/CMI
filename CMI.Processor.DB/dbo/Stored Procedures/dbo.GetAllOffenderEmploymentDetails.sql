
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
	('mboyd'),('ryost'),('kpitts'),('khennings'),('ebellew'),('gromanko'),('acraven'),('rrussell'),('kplunkett'),('sclark'),('bvogt'),('jward'),('fblanco'),('plewis'),('jwyatt'),('calliguie'),('jwindham'),('eamorde'),('tsnyder'),('pespinosa'),('qwaterman'),('mdragony'),('bshreeve'),('ahastings'),('cmartinez')
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
18-Nov-17		Rajesh Awate	Changes for implementation of incremental vs non-incremental mode execution
20-Nov-17		Rajesh Awate	Changes for US114589
21-Nov-17		Rajesh Awate	Changes to handle updating of address & phone in diferential mode
==========================================================================================*/
CREATE PROCEDURE [dbo].[GetAllOffenderEmploymentDetails]
	@AutomonDatabaseName NVARCHAR(128),
	@LastExecutionDateTime DATETIME = NULL,
	@OfficerLogonsToFilterTbl [dbo].[Varchar50Tbl] READONLY
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
							LEFT JOIN [$AutomonDatabaseName].[dbo].[AddressInfo] AI
								ON ORGI.[AddressId] = AI.[Id]
								LEFT JOIN [$AutomonDatabaseName].[dbo].[PhoneNumberInfo] PNI
									ON ORGI.[PhoneId] = PNI.[Id]
		WHERE
			ARI.[PermDesc] = ''Employer''
			AND (PAI.[ToTime] IS NULL AND [PARI].[ToTime] IS NULL)
			AND 
			(
				PAI.[FromTime] >= @LastExecutionDateTime 
				OR PARI.[FromTime] >= @LastExecutionDateTime 
				OR ORGI.[EnteredDateTime] >= @LastExecutionDateTime 
				OR AI.[FromTime] >= @LastExecutionDateTime 
				OR PNI.[FromTime] >= @LastExecutionDateTime
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
							LEFT JOIN [$AutomonDatabaseName].[dbo].[AddressInfo] AI
								ON ORGI.[AddressId] = AI.[Id]
								LEFT JOIN [$AutomonDatabaseName].[dbo].[PhoneNumberInfo] PNI
									ON ORGI.[PhoneId] = PNI.[Id]
		WHERE
			ARI.[PermDesc] = ''Employer''
			AND 
			(
				(PAI.[ToTime] IS NOT NULL AND PAI.[DeletedByPId] IS NOT NULL)
				OR 
				([PARI].[ToTime] IS NOT NULL AND PARI.[DeletedByPId] IS NOT NULL)
			)
			AND 
			(
				PAI.[ToTime] >= @LastExecutionDateTime 
				OR PARI.[ToTime] >= @LastExecutionDateTime 
				OR ORGI.[EnteredDateTime] >= @LastExecutionDateTime 
				OR AI.[FromTime] >= @LastExecutionDateTime 
				OR PNI.[FromTime] >= @LastExecutionDateTime
			)
		';
	END
	ELSE
	BEGIN
		SET @SQLString = 
		'
		;WITH RaceData AS
		(
			SELECT
				L.[Id],
				L.[PermDesc]
			FROM
				[$AutomonDatabaseName].[dbo].[Lookup] L JOIN [$AutomonDatabaseName].[dbo].[LookupType] LT
					ON L.[LookupTypeId] = LT.[Id]
			WHERE
				LT.[Description] = ''Race''
		), ClientProfilesData AS
		(
			SELECT DISTINCT
				O.[Pin],
				AN.[Firstname],
				AN.[MiddleName],
				AN.[LastName],

				P.[DOB] AS [DateOfBirth],
				P.[Gender],
	
				CT.[PermDesc] AS [ClientType],

				RD.[PermDesc] AS [Race],

				CL.[Name] As [CaseloadName],

				OFC.[Logon] AS [OfficerLogon],
				OFC.[Email] As [OfficerEmail],

				OFCNAME.[Firstname] As [OfficerFirstName],
				OFCNAME.[LastName] As [OfficerLastName]
			FROM
				[$AutomonDatabaseName].[dbo].[AnyName] AN JOIN [$AutomonDatabaseName].[dbo].[Person] P
					ON AN.[Id] = P.[NameId]
					JOIN [$AutomonDatabaseName].[dbo].[Offender] O
						ON P.[Id] = O.[PersonId]

						LEFT JOIN RaceData RD
							ON P.[RaceLId] = RD.[Id]

							LEFT JOIN [$AutomonDatabaseName].[dbo].[OffenderCaseload] OFCL
								ON O.[Id] = OFCL.[OffenderId]
								LEFT JOIN [$AutomonDatabaseName].[dbo].[Caseload] CL
									ON OFCL.[CaseloadId] = CL.[Id]
						
									LEFT JOIN [$AutomonDatabaseName].[dbo].[OfficerCaseload] OCL
										ON CL.[Id] = OCL.[CaseloadId]
										LEFT JOIN [$AutomonDatabaseName].[dbo].[Officer] OFC
											ON OCL.[OfficerId] = OFC.[Id]
											LEFT JOIN [$AutomonDatabaseName].[dbo].[Person] OFCPER
												ON OFC.[PersonId] = OFCPER.[Id]
												LEFT JOIN [$AutomonDatabaseName].[dbo].[AnyName] OFCNAME
													ON OFCPER.[NameId] = OFCNAME.[Id]

													LEFT JOIN [$AutomonDatabaseName].[dbo].[CourtCase] CC
														ON O.[Id] = CC.[OffenderId]
														LEFT JOIN [$AutomonDatabaseName].[dbo].[CaseType] CT
															ON CC.[CaseTypeId] = CT.[Id]
															LEFT JOIN [$AutomonDatabaseName].[dbo].[CaseCategory] CSCT
																ON CT.[CaseCategoryId] = CSCT.[Id]
			WHERE
				AN.[Firstname] IS NOT NULL
				AND P.[DOB] IS NOT NULL
				AND AN.[FromTime] IS NOT NULL AND AN.[ToTime] IS NULL
				AND OCL.[FromTime] IS NOT NULL AND OCL.[ToTime] IS NULL
				AND P.[FromTime] IS NOT NULL AND P.[ToTime] IS NULL
				AND OFCL.[FromTime] IS NOT NULL AND OFCL.[ToTime] IS NULL AND OFCL.[IsPrimary] = 1
				AND CC.[FromTime] IS NOT NULL AND CC.[CloseDateTime] IS NULL AND CT.[IsActive] = 1
				AND 
				(
					AN.[FromTime] > @LastExecutionDateTime 
					OR OCL.[FromTime] > @LastExecutionDateTime 
					OR P.[LastModified] > @LastExecutionDateTime 
					OR OFCL.[FromTime] > @LastExecutionDateTime 
					OR @LastExecutionDateTime IS NULL
				)
				AND [$AutomonDatabaseName].[dbo].[GetCaseStatus](CC.[Id]) = ''Active''
				AND CSCT.[PermDesc] = ''Service''
				AND (CT.[PermDesc] = ''Formal'' OR CT.[PermDesc] = ''PRCS'' OR CT.[PermDesc] = ''MCS'' OR CT.[PermDesc] = ''Adult.Interstate'')
				AND CL.[Name] NOT LIKE ''%bench warrant%''
				AND EXISTS
				(
					SELECT
						1
					FROM 
						[$AutomonDatabaseName].[dbo].[OffenderInfo] OI JOIN [$AutomonDatabaseName].[dbo].[CaseInfo] CI
							ON OI.[Id] = CI.[OffenderId]
					WHERE
						OI.[Id] = O.[Id]
						AND CI.[Status] = ''Active''
						AND CI.[SupervisionStartDate] <= DATEADD(DAY, 30, GETDATE())
						AND CI.[SupervisionStartDate] < CI.[SupervisionEndDate]
						AND ISNULL(CAST(([$AutomonDatabaseName].[dbo].[GetCaseAttributeValue](CI.[Id], NULL, ''SentencingDate'')) AS DATE), CI.[StartDate]) <= DATEADD(DAY, 30, GETDATE())
				)

				--apply officer logon filter if any passed
				AND
				(
					NOT EXISTS(SELECT 1 FROM @OfficerLogonsToFilterTbl OLTF) 
					OR EXISTS(SELECT 1 FROM @OfficerLogonsToFilterTbl OLTF WHERE OLTF.[Item] = OFC.[Logon])
				)
		)
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
			AND EXISTS
			(
				SELECT
					1
				FROM
					ClientProfilesData CPD
				WHERE
					CPD.[Pin] = OI.[Pin]
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
			AND EXISTS
			(
				SELECT
					1
				FROM
					ClientProfilesData CPD
				WHERE
					CPD.[Pin] = OI.[Pin]
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