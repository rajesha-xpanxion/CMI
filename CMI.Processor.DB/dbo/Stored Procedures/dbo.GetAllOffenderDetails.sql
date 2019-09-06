/*==========================================================================================
Author:			Rajesh Awate
Create date:	03-Oct-18
Description:	To get all offender details from given automon database
---------------------------------------------------------------------------------
Test execution:-
EXEC	
	[dbo].[GetAllOffenderDetails]
		@AutomonDatabaseName = 'CX',
		@LastExecutionDateTime = NULL
---------------------------------------------------------------------------------
History:-
Date			Author			Changes
04-July-18		Rajesh Awate	Created.
26-July-19		Rajesh Awate	Changes to return values from columns [Description] & [PermDesc] for Race.
==========================================================================================*/
CREATE PROCEDURE [dbo].[GetAllOffenderDetails]
	@AutomonDatabaseName NVARCHAR(128),
	@LastExecutionDateTime DATETIME = NULL
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
		), CaseStatusData AS
		(
			SELECT
				CA.[CaseId],
				L.[PermDesc],
				CA.[FromTime],
				CA.[ToTime]
			FROM
				[$AutomonDatabaseName].[dbo].[CaseAttribute] CA JOIN [$AutomonDatabaseName].[dbo].[AttributeDef] AD
					ON CA.[AttributeId] = AD.[Id]
					JOIN [$AutomonDatabaseName].[dbo].[Lookup] L
						ON CA.[Value] = L.[Id]
			WHERE
				AD.[PermDesc] = ''Case_CaseStatus''
		)
		SELECT DISTINCT
			O.[Pin],
			AN.[Firstname],
			AN.[MiddleName],
			AN.[LastName],

			P.[DOB] AS [DateOfBirth],
			P.[Gender],
	
			CT.[PermDesc] AS [ClientType],

			RD.[Description] AS [RaceDescription],
			RD.[PermDesc] AS [RacePermDesc],

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
		), CaseStatusData AS
		(
			SELECT
				CA.[CaseId],
				L.[PermDesc],
				CA.[FromTime],
				CA.[ToTime]
			FROM
				[$AutomonDatabaseName].[dbo].[CaseAttribute] CA JOIN [$AutomonDatabaseName].[dbo].[AttributeDef] AD
					ON CA.[AttributeId] = AD.[Id]
					JOIN [$AutomonDatabaseName].[dbo].[Lookup] L
						ON CA.[Value] = L.[Id]
			WHERE
				AD.[PermDesc] = ''Case_CaseStatus''
		)
		SELECT DISTINCT
			O.[Pin],
			AN.[Firstname],
			AN.[MiddleName],
			AN.[LastName],

			P.[DOB] AS [DateOfBirth],
			P.[Gender],
	
			CT.[PermDesc] AS [ClientType],

			RD.[Description] AS [RaceDescription],
			RD.[PermDesc] AS [RacePermDesc],

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

			--AND OFC.[Logon] IN (''kplunkett'')
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