

/*==========================================================================================
Author:			Rajesh Awate
Create date:	03-Oct-18
Description:	To get all offender case details from given source database
---------------------------------------------------------------------------------
Test execution:-
EXEC	
	[dbo].[GetAllOffenderCaseDetails]
		@SourceDatabaseName = 'CX',
		@LastExecutionDateTime = NULL
---------------------------------------------------------------------------------
History:-
Date			Author			Changes
04-July-18		Rajesh Awate	Created.
==========================================================================================*/
CREATE PROCEDURE [dbo].[GetAllOffenderCaseDetails]
	@SourceDatabaseName NVARCHAR(128),
	@LastExecutionDateTime DATETIME = NULL
AS
BEGIN
	DECLARE @SQLString NVARCHAR(MAX), @ParmDefinition NVARCHAR(1000);
	
	IF(@LastExecutionDateTime IS NOT NULL)
	BEGIN
		SET @SQLString = 
		'
		;WITH CaseAttributeData AS
		(
			SELECT
				CA.[CaseId],
				CA.[Value],
				AD.[PermDesc]
			FROM
				[$SourceDatabaseName].[dbo].[CaseAttribute] CA JOIN [$SourceDatabaseName].[dbo].[AttributeDef] AD
					ON CA.[AttributeId] = AD.[Id]
			WHERE
				CA.[ToTime] IS NULL
				AND
				(
					CA.[FromTime] > @LastExecutionDateTime
					OR @LastExecutionDateTime IS NULL
				)

		), CaseStatusLookupData AS
		(
			SELECT
				CAD.[CaseId],
				L.[Description]
			FROM
				CaseAttributeData CAD JOIN [$SourceDatabaseName].[dbo].[Lookup] L
					ON CAD.[Value] = L.[Id]
			WHERE
				CAD.[PermDesc] = ''Case_CaseStatus''
		), CaseSupervisionStartDateData AS
		(
			SELECT
				[CaseId],
				[Value]
			FROM
				CaseAttributeData
			WHERE
				[PermDesc] = ''Case_SupervisionStart''
		), CaseSupervisionEndDateData AS
		(
			SELECT
				[CaseId],
				[Value]
			FROM
				CaseAttributeData
			WHERE
				[PermDesc] = ''Case_SupervisionEnd''
		), CaseClosureReasonData AS
		(
			SELECT
				CAD.[CaseId],
				L.[Description]
			FROM
				CaseAttributeData CAD JOIN [$SourceDatabaseName].[dbo].[Lookup] L
					ON CAD.[Value] = L.[Id]
			WHERE
				CAD.[PermDesc] = ''Case_TerminationType''
		)
		SELECT DISTINCT
			O.[Pin],

			CC.[CaseNumber],
			CSLD.[Description] AS [CaseStatus],

			ST.[DisplayCode] AS [OffenseLabel],
			ST.[OffenseCode] AS [OffenseStatute],
			ST.[OffenseLevel] AS [OffenseCategory],
			CRG.[MostSeriousCharge] AS [IsPrimary],
			CRG.[ViolationDate] AS [OffenseDate],

			COALESCE(CSSD.[Value], CSED.[Value]) AS [CaseDate],
			CSSD.[Value] AS [SupervisionStartDate],
			CSED.[Value] AS [SupervisionEndDate],
			CCRD.[Description] AS [ClosureReason]
		FROM
			[$SourceDatabaseName].[dbo].[Offender] O JOIN [$SourceDatabaseName].[dbo].[CourtCase] CC
				ON O.[Id] = CC.[OffenderId]

				LEFT JOIN CaseStatusLookupData CSLD
					ON CC.[Id] = CSLD.[CaseId]

					LEFT JOIN [$SourceDatabaseName].[dbo].[CaseCharge] CCRG
						ON CC.[Id] = CCRG.[CaseId]
						LEFT JOIN [$SourceDatabaseName].[dbo].[Charge] CRG
							ON CCRG.[ChargeId] = CRG.[Id]
							LEFT JOIN [$SourceDatabaseName].[dbo].[Statute] ST
								ON CRG.[StatuteId] = ST.[Id]

								LEFT JOIN CaseSupervisionStartDateData CSSD
									ON CC.[Id] = CSSD.[CaseId]

									LEFT JOIN CaseSupervisionEndDateData CSED
										ON CC.[Id] = CSED.[CaseId]

										LEFT JOIN [$SourceDatabaseName].[dbo].[CaseType] CT
											ON CC.[CaseTypeId] = CT.[Id]

											LEFT JOIN CaseClosureReasonData CCRD
												ON CC.[Id] = CCRD.[CaseId]

		WHERE
			CRG.[FromTime] IS NOT NULL AND CRG.[ToTime] IS NULL
			AND 
			(
				CC.[FromTime] > @LastExecutionDateTime 
				OR CRG.[FromTime] > @LastExecutionDateTime
				OR @LastExecutionDateTime IS NULL
			)
			AND CSLD.[Description] IS NOT NULL
			AND (CT.[PermDesc] = ''Formal'' OR CT.[PermDesc] = ''PRCS'' OR CT.[PermDesc] = ''MCS'' OR CT.[PermDesc] = ''Adult.Interstate'')
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
				[$SourceDatabaseName].[dbo].[Lookup] L JOIN [$SourceDatabaseName].[dbo].[LookupType] LT
					ON L.[LookupTypeId] = LT.[Id]
			WHERE
				LT.[Description] = ''Race''
		), CaseStatusData AS
		(
			SELECT
				CA.[CaseId],
				L.[PermDesc],
				CA.[FromTime],
				CA.[ToTime]
			FROM
				[$SourceDatabaseName].[dbo].[CaseAttribute] CA JOIN [$SourceDatabaseName].[dbo].[AttributeDef] AD
					ON CA.[AttributeId] = AD.[Id]
					JOIN [$SourceDatabaseName].[dbo].[Lookup] L
						ON CA.[Value] = L.[Id]
			WHERE
				AD.[PermDesc] = ''Case_CaseStatus''
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
				[$SourceDatabaseName].[dbo].[AnyName] AN JOIN [$SourceDatabaseName].[dbo].[Person] P
					ON AN.[Id] = P.[NameId]
					JOIN [$SourceDatabaseName].[dbo].[Offender] O
						ON P.[Id] = O.[PersonId]

						LEFT JOIN RaceData RD
							ON P.[RaceLId] = RD.[Id]

							LEFT JOIN [$SourceDatabaseName].[dbo].[OffenderCaseload] OFCL
								ON O.[Id] = OFCL.[OffenderId]
								LEFT JOIN [$SourceDatabaseName].[dbo].[Caseload] CL
									ON OFCL.[CaseloadId] = CL.[Id]
						
									LEFT JOIN [$SourceDatabaseName].[dbo].[OfficerCaseload] OCL
										ON CL.[Id] = OCL.[CaseloadId]
										LEFT JOIN [$SourceDatabaseName].[dbo].[Officer] OFC
											ON OCL.[OfficerId] = OFC.[Id]
											LEFT JOIN [$SourceDatabaseName].[dbo].[Person] OFCPER
												ON OFC.[PersonId] = OFCPER.[Id]
												LEFT JOIN [$SourceDatabaseName].[dbo].[AnyName] OFCNAME
													ON OFCPER.[NameId] = OFCNAME.[Id]

													LEFT JOIN [$SourceDatabaseName].[dbo].[CourtCase] CC
														ON O.[Id] = CC.[OffenderId]
														LEFT JOIN [$SourceDatabaseName].[dbo].[CaseType] CT
															ON CC.[CaseTypeId] = CT.[Id]
															LEFT JOIN [$SourceDatabaseName].[dbo].[CaseCategory] CSCT
																ON CT.[CaseCategoryId] = CSCT.[Id]

																LEFT JOIN CaseStatusData CSD
																	ON CC.[Id] = CSD.[CaseId]
			WHERE
				AN.[Firstname] IS NOT NULL
				AND P.[DOB] IS NOT NULL
				AND AN.[FromTime] IS NOT NULL AND AN.[ToTime] IS NULL
				AND OCL.[FromTime] IS NOT NULL AND OCL.[ToTime] IS NULL
				AND P.[FromTime] IS NOT NULL AND P.[ToTime] IS NULL
				AND OFCL.[FromTime] IS NOT NULL AND OFCL.[ToTime] IS NULL AND OFCL.[IsPrimary] = 1
				AND CC.[FromTime] IS NOT NULL AND CC.[CloseDateTime] IS NULL AND CT.[IsActive] = 1
				AND CSD.[FromTime] IS NOT NULL AND CSD.[ToTime] IS NULL
				AND 
				(
					AN.[FromTime] > @LastExecutionDateTime 
					OR OCL.[FromTime] > @LastExecutionDateTime 
					OR P.[LastModified] > @LastExecutionDateTime 
					OR OFCL.[FromTime] > @LastExecutionDateTime 
					OR CSD.[FromTime] > @LastExecutionDateTime
					OR @LastExecutionDateTime IS NULL
				)
				AND CSD.[PermDesc] = ''Active''
				AND CSCT.[PermDesc] = ''Service''
				AND (CT.[PermDesc] = ''Formal'' OR CT.[PermDesc] = ''PRCS'' OR CT.[PermDesc] = ''MCS'' OR CT.[PermDesc] = ''Adult.Interstate'')
		), CaseAttributeData AS
		(
			SELECT
				CA.[CaseId],
				CA.[Value],
				AD.[PermDesc]
			FROM
				[$SourceDatabaseName].[dbo].[CaseAttribute] CA JOIN [$SourceDatabaseName].[dbo].[AttributeDef] AD
					ON CA.[AttributeId] = AD.[Id]
			WHERE
				CA.[ToTime] IS NULL
				AND
				(
					CA.[FromTime] > @LastExecutionDateTime
					OR @LastExecutionDateTime IS NULL
				)

		), CaseStatusLookupData AS
		(
			SELECT
				CAD.[CaseId],
				L.[Description]
			FROM
				CaseAttributeData CAD JOIN [$SourceDatabaseName].[dbo].[Lookup] L
					ON CAD.[Value] = L.[Id]
			WHERE
				CAD.[PermDesc] = ''Case_CaseStatus''
		), CaseSupervisionStartDateData AS
		(
			SELECT
				[CaseId],
				[Value]
			FROM
				CaseAttributeData
			WHERE
				[PermDesc] = ''Case_SupervisionStart''
		), CaseSupervisionEndDateData AS
		(
			SELECT
				[CaseId],
				[Value]
			FROM
				CaseAttributeData
			WHERE
				[PermDesc] = ''Case_SupervisionEnd''
		), CaseClosureReasonData AS
		(
			SELECT
				CAD.[CaseId],
				L.[Description]
			FROM
				CaseAttributeData CAD JOIN [$SourceDatabaseName].[dbo].[Lookup] L
					ON CAD.[Value] = L.[Id]
			WHERE
				CAD.[PermDesc] = ''Case_TerminationType''
		)
		SELECT DISTINCT
			O.[Pin],

			CC.[CaseNumber],
			CSLD.[Description] AS [CaseStatus],

			ST.[DisplayCode] AS [OffenseLabel],
			ST.[OffenseCode] AS [OffenseStatute],
			ST.[OffenseLevel] AS [OffenseCategory],
			CRG.[MostSeriousCharge] AS [IsPrimary],
			CRG.[ViolationDate] AS [OffenseDate],

			COALESCE(CSSD.[Value], CSED.[Value]) AS [CaseDate],
			CSSD.[Value] AS [SupervisionStartDate],
			CSED.[Value] AS [SupervisionEndDate],
			CCRD.[Description] AS [ClosureReason]
		FROM
			[$SourceDatabaseName].[dbo].[Offender] O JOIN [$SourceDatabaseName].[dbo].[CourtCase] CC
				ON O.[Id] = CC.[OffenderId]

				LEFT JOIN CaseStatusLookupData CSLD
					ON CC.[Id] = CSLD.[CaseId]

					LEFT JOIN [$SourceDatabaseName].[dbo].[CaseCharge] CCRG
						ON CC.[Id] = CCRG.[CaseId]
						LEFT JOIN [$SourceDatabaseName].[dbo].[Charge] CRG
							ON CCRG.[ChargeId] = CRG.[Id]
							LEFT JOIN [$SourceDatabaseName].[dbo].[Statute] ST
								ON CRG.[StatuteId] = ST.[Id]

								LEFT JOIN CaseSupervisionStartDateData CSSD
									ON CC.[Id] = CSSD.[CaseId]

									LEFT JOIN CaseSupervisionEndDateData CSED
										ON CC.[Id] = CSED.[CaseId]

										LEFT JOIN [$SourceDatabaseName].[dbo].[CaseType] CT
											ON CC.[CaseTypeId] = CT.[Id]

											LEFT JOIN CaseClosureReasonData CCRD
												ON CC.[Id] = CCRD.[CaseId]

		WHERE
			CRG.[FromTime] IS NOT NULL AND CRG.[ToTime] IS NULL
			AND 
			(
				CC.[FromTime] > @LastExecutionDateTime 
				OR CRG.[FromTime] > @LastExecutionDateTime
				OR @LastExecutionDateTime IS NULL
			)
			AND CSLD.[Description] IS NOT NULL
			AND (CT.[PermDesc] = ''Formal'' OR CT.[PermDesc] = ''PRCS'' OR CT.[PermDesc] = ''MCS'' OR CT.[PermDesc] = ''Adult.Interstate'')
			AND EXISTS
			(
				SELECT
					1
				FROM
					ClientProfilesData CPD
				WHERE
					CPD.[Pin] = O.[Pin]
			)
		';
	END


	SET @SQLString = REPLACE(@SQLString, '$SourceDatabaseName', @SourceDatabaseName);

	SET @ParmDefinition = '@LastExecutionDateTime DATETIME';

	EXECUTE sp_executesql 
				@SQLString, 
				@ParmDefinition,  
                @LastExecutionDateTime = @LastExecutionDateTime;
END