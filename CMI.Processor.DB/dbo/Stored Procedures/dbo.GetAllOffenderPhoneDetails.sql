

/*==========================================================================================
Author:			Rajesh Awate
Create date:	03-Oct-18
Description:	To get all offender phone details from given automon database
---------------------------------------------------------------------------------
Test execution:-
DECLARE @OfficerLogonsToFilterTbl [dbo].[Varchar50Tbl];
INSERT INTO @OfficerLogonsToFilterTbl
	([Item])
VALUES
	('kplunkett')
EXEC	
	[dbo].[GetAllOffenderPhoneDetails]
		@AutomonDatabaseName = 'CX',
		@LastExecutionDateTime = NULL,
		@OfficerLogonsToFilterTbl = @OfficerLogonsToFilterTbl
---------------------------------------------------------------------------------
History:-
Date			Author			Changes
04-July-18		Rajesh Awate	Created.
10-Sept-19		Rajesh Awate	Changes for integration by officer filter.
==========================================================================================*/
CREATE PROCEDURE [dbo].[GetAllOffenderPhoneDetails]
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
		;WITH PhoneNumberTypeLookupData AS
		(
			SELECT
				L.[Id],
				L.[Description]
			FROM
				[$AutomonDatabaseName].[dbo].[Lookup] L JOIN [$AutomonDatabaseName].[dbo].[LookupType] LT
					ON L.[LookupTypeId] = LT.[Id]
			WHERE
				LT.[Description] = ''PhoneNumberTypes''
		)
		SELECT DISTINCT
			O.[Pin],

			PP.[Id],
			PNTLD.[Description] AS [PhoneNumberType],

			PN.[Phone],

			N.[Value] AS [Comment],

			PP.[IsPrimary],
			CASE 
				WHEN PN.[ToTime] IS NULL THEN 1
				ELSE 0
			END AS [IsActive]
		FROM
			[$AutomonDatabaseName].[dbo].[Person] P JOIN [$AutomonDatabaseName].[dbo].[Offender] O
				ON P.[Id] = O.[PersonId]
				JOIN [$AutomonDatabaseName].[dbo].[PersonPhone] PP
					ON P.[Id] = PP.[PersonId]
					JOIN [$AutomonDatabaseName].[dbo].[PhoneNumber] PN
						ON PP.[PhoneNumberId] = PN.[Id] 

						JOIN PhoneNumberTypeLookupData PNTLD
							ON PP.[PhoneTypeLId] = PNTLD.[Id]

							LEFT JOIN [$AutomonDatabaseName].[dbo].[Note] N
								ON PN.[NoteId] = N.[Id]
		WHERE
			PN.[FromTime] > @LastExecutionDateTime
			OR @LastExecutionDateTime IS NULL
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

				--apply officer logon filter if any passed
				AND
				(
					NOT EXISTS(SELECT 1 FROM @OfficerLogonsToFilterTbl OLTF) 
					OR EXISTS(SELECT 1 FROM @OfficerLogonsToFilterTbl OLTF WHERE OLTF.[Item] = OFC.[Logon])
				)
		), PhoneNumberTypeLookupData AS
		(
			SELECT
				L.[Id],
				L.[Description]
			FROM
				[$AutomonDatabaseName].[dbo].[Lookup] L JOIN [$AutomonDatabaseName].[dbo].[LookupType] LT
					ON L.[LookupTypeId] = LT.[Id]
			WHERE
				LT.[Description] = ''PhoneNumberTypes''
		)
		SELECT DISTINCT
			O.[Pin],

			PP.[Id],
			PNTLD.[Description] AS [PhoneNumberType],

			PN.[Phone],

			N.[Value] AS [Comment],

			PP.[IsPrimary],
			CASE 
				WHEN PN.[ToTime] IS NULL THEN 1
				ELSE 0
			END AS [IsActive]
		FROM
			[$AutomonDatabaseName].[dbo].[Person] P JOIN [$AutomonDatabaseName].[dbo].[Offender] O
				ON P.[Id] = O.[PersonId]
				JOIN [$AutomonDatabaseName].[dbo].[PersonPhone] PP
					ON P.[Id] = PP.[PersonId]
					JOIN [$AutomonDatabaseName].[dbo].[PhoneNumber] PN
						ON PP.[PhoneNumberId] = PN.[Id] 

						JOIN PhoneNumberTypeLookupData PNTLD
							ON PP.[PhoneTypeLId] = PNTLD.[Id]

							LEFT JOIN [$AutomonDatabaseName].[dbo].[Note] N
								ON PN.[NoteId] = N.[Id]
		WHERE
			(
				PN.[FromTime] > @LastExecutionDateTime
				OR @LastExecutionDateTime IS NULL
			)
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