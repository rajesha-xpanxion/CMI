﻿using System;
using System.Collections.Generic;
using System.Text;

namespace CMI.DAL.Source.AutoMon
{
    public class SQLQuery
    {
        public const string GET_TIME_ZONE = @"
SELECT
    [ItemValue]
FROM
    [dbo].[InstallationParam]
WHERE
    [Name] = 'Time Zone'
";
        public const string GET_ALL_OFFENDER_DETAILS = @"
;WITH RaceData AS
(
	SELECT
		L.[Id],
		L.[PermDesc]
	FROM
		[dbo].[Lookup] L JOIN [dbo].[LookupType] LT
			ON L.[LookupTypeId] = LT.[Id]
	WHERE
		LT.[Description] = 'Race'
), CaseStatusData AS
(
	SELECT
		CA.[CaseId],
		L.[PermDesc]
	FROM
		[dbo].[CaseAttribute] CA JOIN [dbo].[AttributeDef] AD
			ON CA.[AttributeId] = AD.[Id]
			JOIN [dbo].[Lookup] L
				ON CA.[Value] = L.[Id]
	WHERE
		AD.[PermDesc] = 'Case_CaseStatus'
		AND CA.[FromTime] IS NOT NULL AND CA.[ToTime] IS NULL
		AND CA.[FromTime] > @LastExecutionDateTime
)
SELECT DISTINCT
	O.[Pin],
	AN.[Firstname],
	AN.[MiddleName],
	AN.[LastName],

	P.[DOB],
	P.[Gender],
	
	CT.[PermDesc] AS [ClientType],

	RD.[PermDesc] AS [Race],

	CL.[Name] As [CaseloadName],

	OFC.[Logon] AS [OfficerLogon],
	OFC.[Email] As [OfficerEmail],

	OFCNAME.[Firstname] As [OfficerFirstName],
	OFCNAME.[LastName] As [OfficerLastName]
FROM
	[dbo].[AnyName] AN JOIN [dbo].[Person] P
		ON AN.[Id] = P.[NameId]
		JOIN [dbo].[Offender] O
			ON P.[Id] = O.[PersonId]

			LEFT JOIN RaceData RD
				ON P.[RaceLId] = RD.[Id]

				LEFT JOIN [dbo].[OffenderCaseload] OFCL
					ON O.[Id] = OFCL.[OffenderId]
					LEFT JOIN [dbo].[Caseload] CL
						ON OFCL.[CaseloadId] = CL.[Id]
						
						LEFT JOIN [dbo].[OfficerCaseload] OCL
							ON CL.[Id] = OCL.[CaseloadId]
							LEFT JOIN [dbo].[Officer] OFC
								ON OCL.[OfficerId] = OFC.[Id]
								LEFT JOIN [dbo].[Person] OFCPER
									ON OFC.[PersonId] = OFCPER.[Id]
									LEFT JOIN [dbo].[AnyName] OFCNAME
										ON OFCPER.[NameId] = OFCNAME.[Id]

										LEFT JOIN [dbo].[CourtCase] CC
											ON O.[Id] = CC.[OffenderId]
											LEFT JOIN [dbo].[CaseType] CT
												ON CC.[CaseTypeId] = CT.[Id]
												LEFT JOIN [dbo].[CaseCategory] CSCT
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
	AND (AN.[FromTime] > @LastExecutionDateTime OR OCL.[FromTime] > @LastExecutionDateTime OR P.[LastModified] > @LastExecutionDateTime OR OFCL.[FromTime] > @LastExecutionDateTime)
	AND CSD.[PermDesc] = 'Active'
	AND CSCT.[PermDesc] = 'Service'
	AND (CT.[PermDesc] = 'Formal' OR CT.[PermDesc] = 'PRCS' OR CT.[PermDesc] = 'MCS' OR CT.[PermDesc] = 'Adult.Interstate')
";


        public const string GET_ALL_OFFENDER_ADDRESS_DETAILS = @"
;WITH AddressTypeLookupData AS
(
	SELECT
		L.[Id],
		L.[Description]
	FROM
		[dbo].[Lookup] L JOIN [dbo].[LookupType] LT
			ON L.[LookupTypeId] = LT.[Id]
	WHERE
		LT.[Description] = 'AddressTypes'
)
SELECT DISTINCT
	O.[Pin],
	PA.[Id],
	ATLD.[Description] AS [AddressType],
	A.[Line1],
	A.[Line2],
	A.[City],
	A.[State],
	A.[Zip],
	N.[Value] AS [Comment],
	PA.[IsPrimary],
	CASE 
		WHEN A.[ToTime] IS NULL THEN 1
		ELSE 0
	END AS [IsActive]
FROM
	[dbo].[Person] P JOIN [dbo].[Offender] O
		ON P.[Id] = O.[PersonId]
			
			JOIN [dbo].[PersonAddress] PA
			ON P.[Id] = PA.[PersonId]
				JOIN [dbo].[Address] A
				ON PA.[AddressId] = A.[Id] 
					
				JOIN AddressTypeLookupData ATLD
					ON PA.[AddressTypeLId] = ATLD.[Id]
					
					LEFT JOIN [dbo].[Note] N
						ON A.[NoteId] = N.[Id]
WHERE
	A.[FromTime] > @LastExecutionDateTime
";

        public const string GET_ALL_OFFENDER_PHONE_DETAILS = @"
;WITH PhoneNumberTypeLookupData AS
(
	SELECT
		L.[Id],
		L.[Description]
	FROM
		[dbo].[Lookup] L JOIN [dbo].[LookupType] LT
			ON L.[LookupTypeId] = LT.[Id]
	WHERE
		LT.[Description] = 'PhoneNumberTypes'
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
	[dbo].[Person] P JOIN [dbo].[Offender] O
		ON P.[Id] = O.[PersonId]
		JOIN [dbo].[PersonPhone] PP
			ON P.[Id] = PP.[PersonId]
			JOIN [dbo].[PhoneNumber] PN
				ON PP.[PhoneNumberId] = PN.[Id] 

				JOIN PhoneNumberTypeLookupData PNTLD
					ON PP.[PhoneTypeLId] = PNTLD.[Id]

					LEFT JOIN [dbo].[Note] N
						ON PN.[NoteId] = N.[Id]
WHERE
	PN.[FromTime] > @LastExecutionDateTime
";

        public const string GET_ALL_OFFENDER_EMAIL_DETAILS = @"
SELECT DISTINCT
	O.[Pin],
	E.[Id],
	E.[EmailAddress],

	E.[IsPrimary],
	CASE 
		WHEN E.[ToTime] IS NULL THEN 1
		ELSE 0
	END AS [IsActive]
FROM
	[dbo].[Person] P JOIN [dbo].[Offender] O
		ON P.[Id] = O.[PersonId]
		JOIN [dbo].[Email] E
			ON P.[Id] = E.[PersonId]
WHERE
	E.[FromTime] > @LastExecutionDateTime
";

        public const string GET_ALL_OFFENDER_CASE_DETAILS = @"
;WITH CaseAttributeData AS
(
	SELECT
		CA.[CaseId],
		CA.[Value],
		AD.[PermDesc]
	FROM
		[dbo].[CaseAttribute] CA JOIN [dbo].[AttributeDef] AD
			ON CA.[AttributeId] = AD.[Id]
	WHERE
		CA.[FromTime] IS NOT NULL AND CA.[ToTime] IS NULL
		AND CA.[FromTime] > @LastExecutionDateTime 
), CaseStatusLookupData AS
(
	SELECT
		CAD.[CaseId],
		L.[Description]
	FROM
		CaseAttributeData CAD JOIN [dbo].[Lookup] L
			ON CAD.[Value] = L.[Id]
	WHERE
		CAD.[PermDesc] = 'Case_CaseStatus'
), CaseSupervisionStartDateData AS
(
	SELECT
		[CaseId],
		[Value]
	FROM
		CaseAttributeData
	WHERE
		[PermDesc] = 'Case_SupervisionStart'
), CaseSupervisionEndDateData AS
(
	SELECT
		[CaseId],
		[Value]
	FROM
		CaseAttributeData
	WHERE
		[PermDesc] = 'Case_SupervisionEnd'
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

	COALESCE(CSSD.[Value], CSED.[Value]) AS [CaseDate]
FROM
	[dbo].[Offender] O JOIN [dbo].[CourtCase] CC
		ON O.[Id] = CC.[OffenderId]

		LEFT JOIN CaseStatusLookupData CSLD
			ON CC.[Id] = CSLD.[CaseId]

			LEFT JOIN [dbo].[CaseCharge] CCRG
				ON CC.[Id] = CCRG.[CaseId]
				LEFT JOIN [dbo].[Charge] CRG
					ON CCRG.[ChargeId] = CRG.[Id]
					LEFT JOIN [dbo].[Statute] ST
						ON CRG.[StatuteId] = ST.[Id]

						LEFT JOIN CaseSupervisionStartDateData CSSD
							ON CC.[Id] = CSSD.[CaseId]

							LEFT JOIN CaseSupervisionEndDateData CSED
								ON CC.[Id] = CSED.[CaseId]

								LEFT JOIN [dbo].[CaseType] CT
									ON CC.[CaseTypeId] = CT.[Id]

WHERE
	CRG.[FromTime] IS NOT NULL AND CRG.[ToTime] IS NULL
	AND 
	(
		CC.[FromTime] > @LastExecutionDateTime 
		OR CRG.[FromTime] > @LastExecutionDateTime 
	)
	AND CSLD.[Description] IS NOT NULL
	AND (CT.[PermDesc] = 'Formal' OR CT.[PermDesc] = 'PRCS' OR CT.[PermDesc] = 'MCS' OR CT.[PermDesc] = 'Adult.Interstate')
";


        public const string GET_ALL_OFFENDER_NOTE_DETAILS = @"
;WITH ClientNameChangeNotesData AS
(
	SELECT DISTINCT
		OFNDR.[Pin],
		N.[Id],
		N.[Value],
		OFCR.[Email],
		N.[FromTime]
	FROM
		[dbo].[Offender] OFNDR JOIN [dbo].[Person] OFNDRPER
			ON OFNDR.[PersonId] = OFNDRPER.[Id]
			JOIN [dbo].[AnyName] OFNDRAN
				ON OFNDRPER.[NameId] = OFNDRAN.[Id]
				JOIN [dbo].[Note] N
					ON OFNDRAN.[NoteId] = N.[Id]
					JOIN [dbo].[Officer] OFCR
						ON N.[EnteredByPId] = OFCR.[PersonId]
	WHERE
		N.[FromTime] > @LastExecutionDateTime

), ClientAddressChangeNotesData AS
(
	SELECT DISTINCT
		OFNDR.[Pin],
		N.[Id],
		N.[Value],
		OFCR.[Email],
		N.[FromTime]
	FROM
		[dbo].[Offender] OFNDR JOIN [dbo].[PersonAddress] OFNDRPA
			ON OFNDR.[PersonId] = OFNDRPA.[PersonId]
			JOIN [dbo].[Address] OFNDRA
				ON OFNDRPA.[AddressId] = OFNDRA.[Id]
				JOIN [dbo].[Note] N
					ON OFNDRA.[NoteId] = N.[Id]
					JOIN [dbo].[Officer] OFCR
							ON N.[EnteredByPId] = OFCR.[PersonId]
	WHERE
		N.[FromTime] > @LastExecutionDateTime
), ClientPhoneNumberChangeNotesData AS
(
	SELECT DISTINCT
		OFNDR.[Pin],
		N.[Id],
		N.[Value],
		OFCR.[Email],
		N.[FromTime]
	FROM
		[dbo].[Offender] OFNDR JOIN [dbo].[PersonPhone] OFNDRPP
			ON OFNDR.[PersonId] = OFNDRPP.[PersonId]
			JOIN [dbo].[PhoneNumber] OFNDRPN
				ON OFNDRPP.[PhoneNumberId] = OFNDRPN.[Id]
				JOIN [dbo].[Note] N
					ON OFNDRPN.[NoteId] = N.[Id]
					JOIN [dbo].[Officer] OFCR
						ON N.[EnteredByPId] = OFCR.[PersonId]
	WHERE
		N.[FromTime] > @LastExecutionDateTime
)
SELECT DISTINCT
	[Pin],
	[Id],
	[Value] AS [Text],
	[Email] AS [AuthorEmail],
	[FromTime] AS [Date]
FROM
	ClientNameChangeNotesData
UNION
SELECT DISTINCT
	[Pin],
	[Id],
	[Value] AS [Text],
	[Email] AS [AuthorEmail],
	[FromTime] AS [Date]
FROM
	ClientAddressChangeNotesData
UNION
SELECT DISTINCT
	[Pin],
	[Id],
	[Value] AS [Text],
	[Email] AS [AuthorEmail],
	[FromTime] AS [Date]
FROM
	ClientPhoneNumberChangeNotesData
";
    }
}