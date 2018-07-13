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
SELECT DISTINCT
	O.[Pin],
	AN.[Firstname],
	AN.[MiddleName],
	AN.[LastName],
	P.[DOB],
	CT.[PermDesc] AS [ClientType],
	@TimeZone AS [TimeZone],
	P.[Gender],
	L1.[Description] AS [Ethinicity],
	CL.[Name] As [CaseloadName],
	L2.[Description] As [CaseloadType],
	OFC.[Logon] AS [OfficerLogon],
	OFC.[Email] As [OfficerEmail],
	OFCNAME.[Firstname] As [OfficerFirstName],
	OFCNAME.[LastName] As [OfficerLastName]
FROM
	[dbo].[AnyName] AN LEFT JOIN [dbo].[Person] P
		ON AN.[Id] = P.[NameId]
		LEFT JOIN [dbo].[Offender] O
			ON P.[Id] = O.[PersonId]
			LEFT JOIN [dbo].[Lookup] L1
				ON P.[RaceLId] = L1.[Id]
				LEFT JOIN [dbo].[LookupType] LT1
					ON L1.[LookupTypeId] = LT1.[Id] AND LT1.[Description] = 'Race'
					LEFT JOIN [dbo].[OffenderCaseload] OFCL
						ON O.[Id] = OFCL.[OffenderId]
						LEFT JOIN [dbo].[Caseload] CL
							ON OFCL.[CaseloadId] = CL.[Id]
							LEFT JOIN [dbo].[Lookup] L2
								ON CL.[CaseloadTypeLId] = L2.[Id]
								LEFT JOIN [dbo].[LookupType] LT2
									ON L2.[LookupTypeId] = LT2.[Id] AND LT2.[Description] = 'Caseloads'
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
	AN.[FromTime] IS NOT NULL AND AN.[ToTime] IS NULL
	AND OCL.[FromTime] IS NOT NULL AND OCL.[ToTime] IS NULL
	AND P.[FromTime] IS NOT NULL AND P.[ToTime] IS NULL
	AND OFCL.[FromTime] IS NOT NULL AND OFCL.[ToTime] IS NULL AND OFCL.[IsPrimary] = 1
	AND CC.[FromTime] IS NOT NULL AND CC.[CloseDateTime] IS NULL AND CT.[IsActive] = 1
	AND (AN.[FromTime] > @LastExecutionDateTime OR OCL.[FromTime] > @LastExecutionDateTime OR P.[LastModified] > @LastExecutionDateTime OR OFCL.[FromTime] > @LastExecutionDateTime)
ORDER BY
	AN.[FirstName],
	AN.[LastName]
";


        public const string GET_ALL_OFFENDER_ADDRESS_DETAILS = @"
SELECT DISTINCT
	O.[Pin],
	AN.[Firstname],
	AN.[MiddleName],
	AN.[LastName],
	PA.[Id],
	L3.[Description] AS [AddressType],
	A.[Line1],
	A.[Line2],
	A.[City],
	A.[State],
	A.[Zip],
	N.[Value] AS [Comment],
	PA.[IsPrimary]
FROM
	[dbo].[AnyName] AN JOIN [dbo].[Person] P
		ON AN.[Id] = P.[NameId]
		JOIN [dbo].[Offender] O
			ON P.[Id] = O.[PersonId]
			
			LEFT JOIN [dbo].[PersonAddress] PA
				ON P.[Id] = PA.[PersonId]
				LEFT JOIN [dbo].[Address] A
					ON PA.[AddressId] = A.[Id] 
					LEFT JOIN [dbo].[Lookup] L3
						ON PA.[AddressTypeLId] = L3.[Id]
						LEFT JOIN [dbo].[LookupType] LT3
							ON L3.[LookupTypeId] = LT3.[Id] AND LT3.[Description] = 'AddressTypes'
							LEFT JOIN [dbo].[Note] N
								ON A.[NoteId] = N.[Id]
WHERE
	A.[FromTime] IS NOT NULL AND A.[ToTime] IS NULL
	AND (A.[FromTime] > @LastExecutionDateTime)
ORDER BY
	AN.[FirstName],
	AN.[LastName]
";

        public const string GET_ALL_OFFENDER_PHONE_DETAILS = @"
SELECT DISTINCT
	O.[Pin],
	AN.[Firstname],
	AN.[MiddleName],
	AN.[LastName],
	PP.[Id],
	L3.[Description] AS [PhoneNumberType],
	PN.[Phone],
	N.[Value] AS [Comment],
	PP.[IsPrimary]
FROM
	[dbo].[AnyName] AN JOIN [dbo].[Person] P
		ON AN.[Id] = P.[NameId]
		JOIN [dbo].[Offender] O
			ON P.[Id] = O.[PersonId]
			LEFT JOIN [dbo].[PersonPhone] PP
				ON P.[Id] = PP.[PersonId]
				LEFT JOIN [dbo].[PhoneNumber] PN
					ON PP.[PhoneNumberId] = PN.[Id] 
					LEFT JOIN [dbo].[Lookup] L3
						ON PP.[PhoneTypeLId] = L3.[Id]
						LEFT JOIN [dbo].[LookupType] LT3
							ON L3.[LookupTypeId] = LT3.[Id] AND LT3.[Description] = 'PhoneTypes'
							LEFT JOIN [dbo].[Note] N
								ON PN.[NoteId] = N.[Id]
WHERE
	PN.[FromTime] IS NOT NULL AND PN.[ToTime] IS NULL
	AND (PN.[FromTime] > @LastExecutionDateTime)
ORDER BY
	AN.[FirstName],
	AN.[LastName]
";

        public const string GET_ALL_OFFENDER_EMAIL_DETAILS = @"
SELECT DISTINCT
	O.[Pin],
	AN.[Firstname],
	AN.[MiddleName],
	AN.[LastName],
	E.[EmailAddress],
	E.[IsPrimary]
FROM
	[dbo].[AnyName] AN JOIN [dbo].[Person] P
		ON AN.[Id] = P.[NameId]
		JOIN [dbo].[Offender] O
			ON P.[Id] = O.[PersonId]
			LEFT JOIN [dbo].[Email] E
				ON P.[Id] = E.[PersonId]
WHERE
	E.[FromTime] IS NOT NULL AND E.[ToTime] IS NULL
	AND (E.[FromTime] > @LastExecutionDateTime)
ORDER BY
	AN.[FirstName],
	AN.[LastName]
";

        public const string GET_ALL_OFFENDER_CASE_DETAILS = @"
SELECT DISTINCT
	O.[Pin],
	AN.[Firstname],
	AN.[MiddleName],
	AN.[LastName],

	CC.[CaseNumber],
	L1.[Description] AS [CaseStatus],

	ST.[DisplayCode] AS [OffenseLabel],
	ST.[OffenseCode] AS [OffenseStatute],
	ST.[OffenseLevel] AS [OffenseCategory],
	CRG.[MostSeriousCharge] AS [IsPrimary],
	CRG.[ViolationDate] AS [OffenseDate],

	COALESCE(CC.[CloseDateTime], CSSD.[Value], CSED.[Value]) AS [CaseDate]
FROM
	[dbo].[AnyName] AN LEFT JOIN [dbo].[Person] P
		ON AN.[Id] = P.[NameId]
		LEFT JOIN [dbo].[Offender] O
			ON P.[Id] = O.[PersonId]
			LEFT JOIN [dbo].[CourtCase] CC
				ON O.[Id] = CC.[OffenderId]
				LEFT JOIN 
				(	SELECT 
						CA1.[CaseId],
						CA1.[Value],
						CA1.[FromTime]
					FROM
						[dbo].[CaseAttribute] CA1 JOIN [dbo].[AttributeDef] AD1
							ON CA1.[AttributeId] = AD1.[Id]
								WHERE
									AD1.[PermDesc] = 'Case_CaseStatus'
									AND CA1.[FromTime] IS NOT NULL AND CA1.[ToTime] IS NULL
				) CCS
					ON CC.[Id] = CCS.[CaseId]

					LEFT JOIN [dbo].[Lookup] L1
						ON CCS.[Value] = L1.[Id]
						LEFT JOIN [dbo].[CaseCharge] CCRG
							ON CC.[Id] = CCRG.[CaseId]
							LEFT JOIN [dbo].[Charge] CRG
								ON CCRG.[ChargeId] = CRG.[Id]
								LEFT JOIN [dbo].[Statute] ST
									ON CRG.[StatuteId] = ST.[Id]

									LEFT JOIN 
									(
										SELECT 
											CA2.[CaseId],
											CA2.[Value],
											CA2.[FromTime]
										FROM
											[dbo].[CaseAttribute] CA2 JOIN [dbo].[AttributeDef] AD2
												ON CA2.[AttributeId] = AD2.[Id] 
										WHERE
											AD2.[PermDesc] = 'Case_SupervisionStart'
											AND CA2.[FromTime] IS NOT NULL AND CA2.[ToTime] IS NULL
									) CSSD
										ON CC.[Id] = CSSD.[CaseId]

										LEFT JOIN 
										(
											SELECT 
												CA3.[CaseId],
												CA3.[Value],
												CA3.[FromTime]
											FROM
												[dbo].[CaseAttribute] CA3 JOIN [dbo].[AttributeDef] AD3
													ON CA3.[AttributeId] = AD3.[Id] 
											WHERE
												AD3.[PermDesc] = 'Case_SupervisionEnd'
												AND CA3.[FromTime] IS NOT NULL AND CA3.[ToTime] IS NULL
										) CSED
											ON CC.[Id] = CSSD.[CaseId]


WHERE
	CRG.[FromTime] IS NOT NULL AND CRG.[ToTime] IS NULL
	AND 
	(
		CC.[FromTime] > @LastRunDateTime 
		OR CRG.[FromTime] > @LastExecutionDateTime 
		OR CCS.[FromTime] > @LastExecutionDateTime 
		OR CSSD.[FromTime] > @LastExecutionDateTime
		OR CSED.[FromTime] > @LastExecutionDateTime
	)
ORDER BY
	AN.[FirstName],
	AN.[LastName]
";


        public const string GET_ALL_OFFENDER_NOTE_DETAILS = @"
SELECT DISTINCT
	OFN.[Pin],
	AN.[NoteId],
	N.[FromTime] AS [NoteDate],
	N.[Value],
	OFC.[Logon],
	OFC.[Email]
FROM
	[dbo].[AnyName] AN JOIN [dbo].[Person] OFNP
		ON AN.[Id] = OFNP.[NameId]
		JOIN [dbo].[Offender] OFN
			ON OFNP.[Id] = OFN.[PersonId]
			LEFT JOIN [dbo].[Note] N
				ON AN.[NoteId] = N.[Id]
				LEFT JOIN [dbo].[Person] OFCP
					ON N.[EnteredByPId] = OFCP.[Id]
					JOIN [dbo].[Officer] OFC
						ON OFCP.[Id] = OFC.PersonId
WHERE
	N.[FromTime] IS NOT NULL AND N.[ToTime] IS NULL
	AND (N.[FromTime] > @LastExecutionDateTime)
";
    }
}
