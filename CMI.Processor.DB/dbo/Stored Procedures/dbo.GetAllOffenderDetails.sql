
/*==========================================================================================
Author:			Rajesh Awate
Create date:	03-Oct-18
Description:	To get all offender details from given automon database
---------------------------------------------------------------------------------
Test execution:-
DECLARE @OfficerLogonsToFilterTbl [dbo].[Varchar50Tbl];
INSERT INTO @OfficerLogonsToFilterTbl
	([Item])
VALUES
	('mboyd'),('ryost'),('kpitts'),('khennings'),('ebellew'),('gromanko'),('acraven'),('rrussell'),('kplunkett'),('sclark'),('bvogt'),('jward'),('fblanco'),('plewis'),('jwyatt'),('calliguie'),('jwindham'),('eamorde'),('tsnyder'),('pespinosa'),('qwaterman'),('mdragony'),('bshreeve'),('ahastings'),('cmartinez')
EXEC	
	[dbo].[GetAllOffenderDetails]
		@AutomonDatabaseName = 'CX',
		@LastExecutionDateTime = NULL,
		@OfficerLogonsToFilterTbl = @OfficerLogonsToFilterTbl
---------------------------------------------------------------------------------
History:-
Date			Author			Changes
04-July-18		Rajesh Awate	Created.
26-July-19		Rajesh Awate	Changes to return values from columns [Description] & [PermDesc] for Race.
10-Sept-19		Rajesh Awate	Changes for integration by officer filter.
20-Sept-19		Rajesh Awate	Changes to select 1 client type for an offender based on order of PRCS > MS (MCS) > Formal
20-Sept-19		Rajesh Awate	Changes to exclude records having word "bench warrant" in its caseload name
31-Oct-19		Rajesh Awate	Changes for consideration of Mugshot Photo while fetching differential data
14-Nov-17		Rajesh Awate	Changes to return value for Dept Sup Level attribute
18-Nov-17		Rajesh Awate	Changes for implementation of incremental vs non-incremental mode execution
20-Nov-17		Rajesh Awate	Changes for US114859
==========================================================================================*/
CREATE PROCEDURE [dbo].[GetAllOffenderDetails]
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
		;WITH RaceData AS
		(
			SELECT
				L.[Id],
				L.[Description],
				L.[PermDesc]
			FROM
				[$AutomonDatabaseName].[dbo].[Lookup] L JOIN [$AutomonDatabaseName].[dbo].[LookupType] LT
					ON L.[LookupTypeId] = LT.[Id]
			WHERE
				LT.[Description] = ''Race''
		)
		SELECT DISTINCT
			O.[Pin],
			AN.[Firstname],
			AN.[MiddleName],
			AN.[LastName],

			P.[DOB] AS [DateOfBirth],
			P.[Gender],
	
			CASE
				WHEN EXISTS(SELECT 1 FROM [$AutomonDatabaseName].[dbo].[OffenderInfo] OI JOIN [$AutomonDatabaseName].[dbo].[CaseInfo] CI ON OI.[Id] = CI.[OffenderId] WHERE OI.[Id] = O.[Id] AND CI.[Status] = ''Active'' AND CI.[PermDesc] = ''PRCS'') THEN ''PRCS''
				WHEN EXISTS(SELECT 1 FROM [$AutomonDatabaseName].[dbo].[OffenderInfo] OI JOIN [$AutomonDatabaseName].[dbo].[CaseInfo] CI ON OI.[Id] = CI.[OffenderId] WHERE OI.[Id] = O.[Id] AND CI.[Status] = ''Active'' AND CI.[PermDesc] = ''MCS'') THEN ''MCS''
				WHEN EXISTS(SELECT 1 FROM [$AutomonDatabaseName].[dbo].[OffenderInfo] OI JOIN [$AutomonDatabaseName].[dbo].[CaseInfo] CI ON OI.[Id] = CI.[OffenderId] WHERE OI.[Id] = O.[Id] AND CI.[Status] = ''Active'' AND CI.[PermDesc] = ''MS'') THEN ''MS''
				WHEN EXISTS(SELECT 1 FROM [$AutomonDatabaseName].[dbo].[OffenderInfo] OI JOIN [$AutomonDatabaseName].[dbo].[CaseInfo] CI ON OI.[Id] = CI.[OffenderId] WHERE OI.[Id] = O.[Id] AND CI.[Status] = ''Active'' AND CI.[PermDesc] = ''Formal'') THEN ''Formal''
				ELSE CT.[PermDesc]
			END AS [ClientType],

			RD.[Description] AS [RaceDescription],
			RD.[PermDesc] AS [RacePermDesc],

			CL.[Name] As [CaseloadName],

			OFC.[Logon] AS [OfficerLogon],
			OFC.[Email] As [OfficerEmail],

			OFCNAME.[Firstname] As [OfficerFirstName],
			OFCNAME.[LastName] As [OfficerLastName],

			[$AutomonDatabaseName].[dbo].[GetPersonAttributeValue](P.[Id], NULL, ''DepartmentSupervisionLevel'') AS [DeptSupLevel]
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

															LEFT JOIN [$AutomonDatabaseName].[dbo].[DocumentInfo] DI
																ON P.[Id] = DI.[PersonId]

																LEFT JOIN [$AutomonDatabaseName].[dbo].[PersonAttribute] PA
																	ON P.[Id] = PA.[PersonId]
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
				OR DI.[EnteredDateTime] > @LastExecutionDateTime 
				OR PA.[FromTime] > @LastExecutionDateTime 
				OR @LastExecutionDateTime IS NULL
			)
			AND [$AutomonDatabaseName].[dbo].[GetCaseStatus](CC.[Id]) = ''Active''
			AND CSCT.[PermDesc] = ''Service''
			AND (CT.[PermDesc] = ''Formal'' OR CT.[PermDesc] = ''PRCS'' OR CT.[PermDesc] = ''MCS'' OR CT.[PermDesc] = ''Adult.Interstate'')
			AND DI.[DocumentTypeDescription] = ''Photo-Mugshot''
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
				L.[Description],
				L.[PermDesc]
			FROM
				[$AutomonDatabaseName].[dbo].[Lookup] L JOIN [$AutomonDatabaseName].[dbo].[LookupType] LT
					ON L.[LookupTypeId] = LT.[Id]
			WHERE
				LT.[Description] = ''Race''
		)
		SELECT DISTINCT
			O.[Pin],
			AN.[Firstname],
			AN.[MiddleName],
			AN.[LastName],

			P.[DOB] AS [DateOfBirth],
			P.[Gender],
	
			CASE
				WHEN EXISTS(SELECT 1 FROM [$AutomonDatabaseName].[dbo].[OffenderInfo] OI JOIN [$AutomonDatabaseName].[dbo].[CaseInfo] CI ON OI.[Id] = CI.[OffenderId] WHERE OI.[Id] = O.[Id] AND CI.[Status] = ''Active'' AND CI.[PermDesc] = ''PRCS'') THEN ''PRCS''
				WHEN EXISTS(SELECT 1 FROM [$AutomonDatabaseName].[dbo].[OffenderInfo] OI JOIN [$AutomonDatabaseName].[dbo].[CaseInfo] CI ON OI.[Id] = CI.[OffenderId] WHERE OI.[Id] = O.[Id] AND CI.[Status] = ''Active'' AND CI.[PermDesc] = ''MCS'') THEN ''MCS''
				WHEN EXISTS(SELECT 1 FROM [$AutomonDatabaseName].[dbo].[OffenderInfo] OI JOIN [$AutomonDatabaseName].[dbo].[CaseInfo] CI ON OI.[Id] = CI.[OffenderId] WHERE OI.[Id] = O.[Id] AND CI.[Status] = ''Active'' AND CI.[PermDesc] = ''MS'') THEN ''MS''
				WHEN EXISTS(SELECT 1 FROM [$AutomonDatabaseName].[dbo].[OffenderInfo] OI JOIN [$AutomonDatabaseName].[dbo].[CaseInfo] CI ON OI.[Id] = CI.[OffenderId] WHERE OI.[Id] = O.[Id] AND CI.[Status] = ''Active'' AND CI.[PermDesc] = ''Formal'') THEN ''Formal''
				ELSE CT.[PermDesc]
			END AS [ClientType],

			RD.[Description] AS [RaceDescription],
			RD.[PermDesc] AS [RacePermDesc],

			CL.[Name] As [CaseloadName],

			OFC.[Logon] AS [OfficerLogon],
			OFC.[Email] As [OfficerEmail],

			OFCNAME.[Firstname] As [OfficerFirstName],
			OFCNAME.[LastName] As [OfficerLastName],

			[$AutomonDatabaseName].[dbo].[GetPersonAttributeValue](P.[Id], NULL, ''DepartmentSupervisionLevel'') AS [DeptSupLevel]
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