
/*==========================================================================================
Author:			Rajesh Awate
Create date:	03-Oct-18
Description:	To get all offender case details from given automon database
---------------------------------------------------------------------------------
Test execution:-
DECLARE @OfficerLogonsToFilterTbl [dbo].[Varchar50Tbl];
INSERT INTO @OfficerLogonsToFilterTbl
	([Item])
VALUES
	('mboyd'),('ryost'),('kpitts'),('khennings'),('ebellew'),('gromanko'),('acraven'),('rrussell'),('kplunkett'),('sclark'),('bvogt'),('jward'),('fblanco'),('plewis'),('jwyatt')
EXEC	
	[dbo].[GetAllOffenderCaseDetails]
		@AutomonDatabaseName = 'CX',
		@LastExecutionDateTime = NULL,
		@OfficerLogonsToFilterTbl = @OfficerLogonsToFilterTbl
---------------------------------------------------------------------------------
History:-
Date			Author			Changes
03-Oct-18		Rajesh Awate	Created.
04-Oct-18		Rajesh Awate	Fix for type casting issue in application
12-Aug-19		Rajesh Awate	Fix for issue in getting case data
10-Sept-19		Rajesh Awate	Changes for integration by officer filter.
19-Sept-19		Rajesh Awate	Changes to map Conviction Date -> Sentencing Date to Offense Date
19-Sept-19		Rajesh Awate	Changes to map Sentencing Date to Case Date. If found NULL skip whole record.
==========================================================================================*/
CREATE PROCEDURE [dbo].[GetAllOffenderCaseDetails]
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
			CI.[Pin],
			CI.[CaseNumber],
			CI.[CaseType],
			CI.[Status] AS [CaseStatus],
			CCI.[StatuteCode] AS [OffenseLabel],
			SI.[OffenseCode] AS [OffenseStatute],
			[$AutomonDatabaseName].[dbo].[GetCaseOffenseLevel](CI.[Id]) AS [OffenseCategory],
			ISNULL(CCI.[MostSeriousCharge], 0) AS [IsPrimary],
			CAST(
				COALESCE(
					[$AutomonDatabaseName].[dbo].[GetCaseAttributeValue](CI.[Id], NULL, ''ConvictionDate''),
					[$AutomonDatabaseName].[dbo].[GetCaseAttributeValue](CI.[Id], NULL, ''SentencingDate'')
				)
			AS DATE) AS [OffenseDate],
			CAST(([$AutomonDatabaseName].[dbo].[GetCaseAttributeValue](CI.[Id], NULL, ''SentencingDate'')) AS DATE) AS [CaseDate],
			CAST(CI.[SupervisionStartDate] AS DATE) AS [SupervisionStartDate],
			CAST(CI.[SupervisionEndDate] AS DATE) AS [SupervisionEndDate],
			[$AutomonDatabaseName].[dbo].[GetCaseAttributeValue](CI.[Id], NULL, ''Case_TerminationType'') AS [ClosureReason]
		FROM
			[$AutomonDatabaseName].[dbo].[CaseInfo] CI LEFT JOIN [$AutomonDatabaseName].[dbo].[CaseChargeInfo] CCI
				ON CI.[Id] = CCI.[CaseId]
				LEFT JOIN [$AutomonDatabaseName].[dbo].[StatuteInfo] SI
					ON CCI.[StatuteId] = SI.[Id]
					LEFT JOIN [$AutomonDatabaseName].[dbo].[CaseAttribute] CA
						ON CI.[Id] = CA.[CaseId]
						LEFT JOIN [$AutomonDatabaseName].[dbo].[AttributeDef] AD
							ON CA.[AttributeId] = AD.[Id]
		WHERE
			(CI.[FromTime] >= @LastExecutionDateTime OR CA.[FromTime] >= @LastExecutionDateTime OR CCI.[FromTime] >= @LastExecutionDateTime)
			AND (CI.[PermDesc] = ''Formal'' OR CI.[PermDesc] = ''PRCS'' OR CI.[PermDesc] = ''MCS'' OR CI.[PermDesc] = ''Adult.Interstate'')
			AND 
			(
				AD.[PermDesc] = ''Case_SupervisionStart'' 
				OR AD.[PermDesc] = ''Case_SupervisionEnd'' 
				OR AD.[PermDesc] = ''Case_CaseStatus'' 
				OR AD.[PermDesc] = ''Case_TerminationType''
				OR AD.[PermDesc] = ''ConvictionDate''
				OR AD.[PermDesc] = ''SentencingDate''
			)
			AND [$AutomonDatabaseName].[dbo].[GetCaseAttributeValue](CI.[Id], NULL, ''SentencingDate'') IS NOT NULL
			AND CAST(CI.[SupervisionStartDate] AS DATE) <= CAST(CI.[SupervisionEndDate] AS DATE)
			AND CI.[SupervisionStartDate] <= DATEADD(DAY, 30, GETDATE())
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
				)

				--apply officer logon filter if any passed
				AND
				(
					NOT EXISTS(SELECT 1 FROM @OfficerLogonsToFilterTbl OLTF) 
					OR EXISTS(SELECT 1 FROM @OfficerLogonsToFilterTbl OLTF WHERE OLTF.[Item] = OFC.[Logon])
				)
		)
		SELECT DISTINCT
			CI.[Pin],
			CI.[CaseNumber],
			CI.[CaseType],
			CI.[Status] AS [CaseStatus],
			CCI.[StatuteCode] AS [OffenseLabel],
			SI.[OffenseCode] AS [OffenseStatute],
			[$AutomonDatabaseName].[dbo].[GetCaseOffenseLevel](CI.[Id]) AS [OffenseCategory],
			ISNULL(CCI.[MostSeriousCharge], 0) AS [IsPrimary],
			CAST(
				COALESCE(
					[$AutomonDatabaseName].[dbo].[GetCaseAttributeValue](CI.[Id], NULL, ''ConvictionDate''),
					[$AutomonDatabaseName].[dbo].[GetCaseAttributeValue](CI.[Id], NULL, ''SentencingDate'')
				)
			AS DATE) AS [OffenseDate],
			CAST(([$AutomonDatabaseName].[dbo].[GetCaseAttributeValue](CI.[Id], NULL, ''SentencingDate'')) AS DATE) AS [CaseDate],
			CAST(CI.[SupervisionStartDate] AS DATE) AS [SupervisionStartDate],
			CAST(CI.[SupervisionEndDate] AS DATE) AS [SupervisionEndDate],
			[$AutomonDatabaseName].[dbo].[GetCaseAttributeValue](CI.[Id], NULL, ''Case_TerminationType'') AS [ClosureReason]
		FROM
			[$AutomonDatabaseName].[dbo].[CaseInfo] CI LEFT JOIN [$AutomonDatabaseName].[dbo].[CaseChargeInfo] CCI
				ON CI.[Id] = CCI.[CaseId]
				LEFT JOIN [$AutomonDatabaseName].[dbo].[StatuteInfo] SI
					ON CCI.[StatuteId] = SI.[Id]
					LEFT JOIN [$AutomonDatabaseName].[dbo].[CaseAttribute] CA
						ON CI.[Id] = CA.[CaseId]
						LEFT JOIN [$AutomonDatabaseName].[dbo].[AttributeDef] AD
							ON CA.[AttributeId] = AD.[Id]

		WHERE
			(CI.[FromTime] >= @LastExecutionDateTime OR CA.[FromTime] >= @LastExecutionDateTime OR CCI.[FromTime] >= @LastExecutionDateTime OR @LastExecutionDateTime IS NULL)
			AND (CI.[PermDesc] = ''Formal'' OR CI.[PermDesc] = ''PRCS'' OR CI.[PermDesc] = ''MCS'' OR CI.[PermDesc] = ''Adult.Interstate'')
			AND 
			(
				AD.[PermDesc] = ''Case_SupervisionStart'' 
				OR AD.[PermDesc] = ''Case_SupervisionEnd'' 
				OR AD.[PermDesc] = ''Case_CaseStatus'' 
				OR AD.[PermDesc] = ''Case_TerminationType''
				OR AD.[PermDesc] = ''ConvictionDate''
				OR AD.[PermDesc] = ''SentencingDate''
			)
			AND [$AutomonDatabaseName].[dbo].[GetCaseAttributeValue](CI.[Id], NULL, ''SentencingDate'') IS NOT NULL
			AND CAST(CI.[SupervisionStartDate] AS DATE) <= CAST(CI.[SupervisionEndDate] AS DATE)
			AND CI.[SupervisionStartDate] <= DATEADD(DAY, 30, GETDATE())
			AND EXISTS
			(
				SELECT
					1
				FROM
					ClientProfilesData CPD
				WHERE
					CPD.[Pin] = CI.[Pin]
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