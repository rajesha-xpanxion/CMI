using System;
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
		L.[Description]
	FROM
		[dbo].[Lookup] L JOIN [dbo].[LookupType] LT
			ON L.[LookupTypeId] = LT.[Id]
	WHERE
		LT.[Description] = 'Race'
)
SELECT DISTINCT
	O.[Pin],
	AN.[Firstname],
	AN.[MiddleName],
	AN.[LastName],

	P.[DOB],
	P.[Gender],
	
	CT.[PermDesc] AS [ClientType],

	RD.[Description] AS [Race],

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
WHERE
	AN.[Firstname] IS NOT NULL
	AND P.[DOB] IS NOT NULL
	AND AN.[FromTime] IS NOT NULL AND AN.[ToTime] IS NULL
	AND OCL.[FromTime] IS NOT NULL AND OCL.[ToTime] IS NULL
	AND P.[FromTime] IS NOT NULL AND P.[ToTime] IS NULL
	AND OFCL.[FromTime] IS NOT NULL AND OFCL.[ToTime] IS NULL AND OFCL.[IsPrimary] = 1
	AND CC.[FromTime] IS NOT NULL AND CC.[CloseDateTime] IS NULL AND CT.[IsActive] = 1
	AND (AN.[FromTime] > @LastExecutionDateTime OR OCL.[FromTime] > @LastExecutionDateTime OR P.[LastModified] > @LastExecutionDateTime OR OFCL.[FromTime] > @LastExecutionDateTime)
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
	
	PA.[IsPrimary]
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
	A.[FromTime] IS NOT NULL AND A.[ToTime] IS NULL
	AND (A.[FromTime] > @LastExecutionDateTime)
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

	PP.[IsPrimary]
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
	PN.[FromTime] IS NOT NULL AND PN.[ToTime] IS NULL
	AND (PN.[FromTime] > @LastExecutionDateTime)
";

        public const string GET_ALL_OFFENDER_EMAIL_DETAILS = @"
SELECT DISTINCT
	O.[Pin],

	E.[EmailAddress],

	E.[IsPrimary]
FROM
	[dbo].[Person] P JOIN [dbo].[Offender] O
		ON P.[Id] = O.[PersonId]
		JOIN [dbo].[Email] E
			ON P.[Id] = E.[PersonId]
WHERE
	E.[FromTime] IS NOT NULL AND E.[ToTime] IS NULL
	AND (E.[FromTime] > @LastExecutionDateTime)
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

	COALESCE(CSSD.[Value], CSED.[Value], CC.[CloseDateTime]) AS [CaseDate]
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

WHERE
	CRG.[FromTime] IS NOT NULL AND CRG.[ToTime] IS NULL
	AND 
	(
		CC.[FromTime] > @LastExecutionDateTime 
		OR CRG.[FromTime] > @LastExecutionDateTime 
	)
	AND CSLD.[Description] IS NOT NULL
";


        public const string GET_ALL_OFFENDER_NOTE_DETAILS = @"
SELECT DISTINCT
	OFN.[Pin],
	AN.[NoteId],
	N.[FromTime],
	N.[Value],
	OFC.[Logon],
	OFC.[Email]
FROM
	[dbo].[AnyName] AN JOIN [dbo].[Person] OFNP
		ON AN.[Id] = OFNP.[NameId]
		JOIN [dbo].[Offender] OFN
			ON OFNP.[Id] = OFN.[PersonId]
			JOIN [dbo].[Note] N
				ON AN.[NoteId] = N.[Id]
				JOIN [dbo].[Person] OFCP
					ON N.[EnteredByPId] = OFCP.[Id]
					JOIN [dbo].[Officer] OFC
						ON OFCP.[Id] = OFC.PersonId
WHERE
	N.[FromTime] IS NOT NULL AND N.[ToTime] IS NULL
	AND (N.[FromTime] > @LastExecutionDateTime)
";
    }
}
