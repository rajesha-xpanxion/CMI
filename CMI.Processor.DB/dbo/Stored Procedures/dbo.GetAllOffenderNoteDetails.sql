

/*==========================================================================================
Author:			Rajesh Awate
Create date:	03-Oct-18
Description:	To get all offender note details from given source database
---------------------------------------------------------------------------------
Test execution:-
EXEC	
	[dbo].[GetAllOffenderNoteDetails]
		@SourceDatabaseName = 'CX',
		@LastExecutionDateTime = NULL
---------------------------------------------------------------------------------
History:-
Date			Author			Changes
04-July-18		Rajesh Awate	Created.
==========================================================================================*/
CREATE PROCEDURE [dbo].[GetAllOffenderNoteDetails]
	@SourceDatabaseName NVARCHAR(128),
	@LastExecutionDateTime DATETIME = NULL
AS
BEGIN
	DECLARE @SQLString NVARCHAR(MAX), @ParmDefinition NVARCHAR(1000);
	
	IF(@LastExecutionDateTime IS NOT NULL)
	BEGIN
		SET @SQLString = 
		'
		;WITH ClientNameChangeNotesData AS
		(
			SELECT DISTINCT
				OFNDR.[Pin],
				N.[Id],
				N.[Value],
				OFCR.[Email],
				N.[FromTime]
			FROM
				[$SourceDatabaseName].[dbo].[Offender] OFNDR JOIN [$SourceDatabaseName].[dbo].[Person] OFNDRPER
					ON OFNDR.[PersonId] = OFNDRPER.[Id]
					JOIN [$SourceDatabaseName].[dbo].[AnyName] OFNDRAN
						ON OFNDRPER.[NameId] = OFNDRAN.[Id]
						JOIN [$SourceDatabaseName].[dbo].[Note] N
							ON OFNDRAN.[NoteId] = N.[Id]
							JOIN [$SourceDatabaseName].[dbo].[Officer] OFCR
								ON N.[EnteredByPId] = OFCR.[PersonId]
			WHERE
				N.[FromTime] > @LastExecutionDateTime
				OR @LastExecutionDateTime IS NULL

		), ClientAddressChangeNotesData AS
		(
			SELECT DISTINCT
				OFNDR.[Pin],
				N.[Id],
				N.[Value],
				OFCR.[Email],
				N.[FromTime]
			FROM
				[$SourceDatabaseName].[dbo].[Offender] OFNDR JOIN [$SourceDatabaseName].[dbo].[PersonAddress] OFNDRPA
					ON OFNDR.[PersonId] = OFNDRPA.[PersonId]
					JOIN [$SourceDatabaseName].[dbo].[Address] OFNDRA
						ON OFNDRPA.[AddressId] = OFNDRA.[Id]
						JOIN [$SourceDatabaseName].[dbo].[Note] N
							ON OFNDRA.[NoteId] = N.[Id]
							JOIN [$SourceDatabaseName].[dbo].[Officer] OFCR
									ON N.[EnteredByPId] = OFCR.[PersonId]
			WHERE
				N.[FromTime] > @LastExecutionDateTime
				OR @LastExecutionDateTime IS NULL
		), ClientPhoneNumberChangeNotesData AS
		(
			SELECT DISTINCT
				OFNDR.[Pin],
				N.[Id],
				N.[Value],
				OFCR.[Email],
				N.[FromTime]
			FROM
				[$SourceDatabaseName].[dbo].[Offender] OFNDR JOIN [$SourceDatabaseName].[dbo].[PersonPhone] OFNDRPP
					ON OFNDR.[PersonId] = OFNDRPP.[PersonId]
					JOIN [$SourceDatabaseName].[dbo].[PhoneNumber] OFNDRPN
						ON OFNDRPP.[PhoneNumberId] = OFNDRPN.[Id]
						JOIN [$SourceDatabaseName].[dbo].[Note] N
							ON OFNDRPN.[NoteId] = N.[Id]
							JOIN [$SourceDatabaseName].[dbo].[Officer] OFCR
								ON N.[EnteredByPId] = OFCR.[PersonId]
			WHERE
				N.[FromTime] > @LastExecutionDateTime
				OR @LastExecutionDateTime IS NULL
		)
		SELECT DISTINCT
			[Pin],
			[Id],
			[Value] AS [Text],
			[Email] AS [AuthorEmail],
			[FromTime] AS [Date],
			''Client'' As [NoteType]
		FROM
			ClientNameChangeNotesData
		UNION
		SELECT DISTINCT
			[Pin],
			[Id],
			[Value] AS [Text],
			[Email] AS [AuthorEmail],
			[FromTime] AS [Date],
			''Address'' As [NoteType]
		FROM
			ClientAddressChangeNotesData
		UNION
		SELECT DISTINCT
			[Pin],
			[Id],
			[Value] AS [Text],
			[Email] AS [AuthorEmail],
			[FromTime] AS [Date],
			''Phone'' As [NoteType]
		FROM
			ClientPhoneNumberChangeNotesData
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
		), ClientNameChangeNotesData AS
		(
			SELECT DISTINCT
				OFNDR.[Pin],
				N.[Id],
				N.[Value],
				OFCR.[Email],
				N.[FromTime]
			FROM
				[$SourceDatabaseName].[dbo].[Offender] OFNDR JOIN [$SourceDatabaseName].[dbo].[Person] OFNDRPER
					ON OFNDR.[PersonId] = OFNDRPER.[Id]
					JOIN [$SourceDatabaseName].[dbo].[AnyName] OFNDRAN
						ON OFNDRPER.[NameId] = OFNDRAN.[Id]
						JOIN [$SourceDatabaseName].[dbo].[Note] N
							ON OFNDRAN.[NoteId] = N.[Id]
							JOIN [$SourceDatabaseName].[dbo].[Officer] OFCR
								ON N.[EnteredByPId] = OFCR.[PersonId]
			WHERE
				(
					N.[FromTime] > @LastExecutionDateTime
					OR @LastExecutionDateTime IS NULL
				)
				AND EXISTS
				(
					SELECT
						1
					FROM
						ClientProfilesData CPD
					WHERE
						CPD.[Pin] = OFNDR.[Pin]
				)
		), ClientAddressChangeNotesData AS
		(
			SELECT DISTINCT
				OFNDR.[Pin],
				N.[Id],
				N.[Value],
				OFCR.[Email],
				N.[FromTime]
			FROM
				[$SourceDatabaseName].[dbo].[Offender] OFNDR JOIN [$SourceDatabaseName].[dbo].[PersonAddress] OFNDRPA
					ON OFNDR.[PersonId] = OFNDRPA.[PersonId]
					JOIN [$SourceDatabaseName].[dbo].[Address] OFNDRA
						ON OFNDRPA.[AddressId] = OFNDRA.[Id]
						JOIN [$SourceDatabaseName].[dbo].[Note] N
							ON OFNDRA.[NoteId] = N.[Id]
							JOIN [$SourceDatabaseName].[dbo].[Officer] OFCR
									ON N.[EnteredByPId] = OFCR.[PersonId]
			WHERE
				(
					N.[FromTime] > @LastExecutionDateTime
					OR @LastExecutionDateTime IS NULL
				)
				AND EXISTS
				(
					SELECT
						1
					FROM
						ClientProfilesData CPD
					WHERE
						CPD.[Pin] = OFNDR.[Pin]
				)
		), ClientPhoneNumberChangeNotesData AS
		(
			SELECT DISTINCT
				OFNDR.[Pin],
				N.[Id],
				N.[Value],
				OFCR.[Email],
				N.[FromTime]
			FROM
				[$SourceDatabaseName].[dbo].[Offender] OFNDR JOIN [$SourceDatabaseName].[dbo].[PersonPhone] OFNDRPP
					ON OFNDR.[PersonId] = OFNDRPP.[PersonId]
					JOIN [$SourceDatabaseName].[dbo].[PhoneNumber] OFNDRPN
						ON OFNDRPP.[PhoneNumberId] = OFNDRPN.[Id]
						JOIN [$SourceDatabaseName].[dbo].[Note] N
							ON OFNDRPN.[NoteId] = N.[Id]
							JOIN [$SourceDatabaseName].[dbo].[Officer] OFCR
								ON N.[EnteredByPId] = OFCR.[PersonId]
			WHERE
				(
					N.[FromTime] > @LastExecutionDateTime
					OR @LastExecutionDateTime IS NULL
				)
				AND EXISTS
				(
					SELECT
						1
					FROM
						ClientProfilesData CPD
					WHERE
						CPD.[Pin] = OFNDR.[Pin]
				)
		)
		SELECT DISTINCT
			[Pin],
			[Id],
			[Value] AS [Text],
			[Email] AS [AuthorEmail],
			[FromTime] AS [Date],
			''Client'' As [NoteType]
		FROM
			ClientNameChangeNotesData
		UNION
		SELECT DISTINCT
			[Pin],
			[Id],
			[Value] AS [Text],
			[Email] AS [AuthorEmail],
			[FromTime] AS [Date],
			''Address'' As [NoteType]
		FROM
			ClientAddressChangeNotesData
		UNION
		SELECT DISTINCT
			[Pin],
			[Id],
			[Value] AS [Text],
			[Email] AS [AuthorEmail],
			[FromTime] AS [Date],
			''Phone'' As [NoteType]
		FROM
			ClientPhoneNumberChangeNotesData
		';
	END


	SET @SQLString = REPLACE(@SQLString, '$SourceDatabaseName', @SourceDatabaseName);

	SET @ParmDefinition = '@LastExecutionDateTime DATETIME';

	EXECUTE sp_executesql 
				@SQLString, 
				@ParmDefinition,  
                @LastExecutionDateTime = @LastExecutionDateTime;
END