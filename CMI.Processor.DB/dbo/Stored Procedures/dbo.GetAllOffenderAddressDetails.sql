
/*==========================================================================================
Author:			Rajesh Awate
Create date:	03-Oct-18
Description:	To get all offender address details from given automon database
---------------------------------------------------------------------------------
Test execution:-
DECLARE @OfficerLogonsToFilterTbl [dbo].[Varchar50Tbl];
INSERT INTO @OfficerLogonsToFilterTbl
	([Item])
VALUES
	('mboyd'),('ryost'),('kpitts'),('khennings'),('ebellew'),('gromanko'),('acraven'),('rrussell'),('kplunkett'),('sclark'),('bvogt'),('jward'),('fblanco'),('plewis'),('jwyatt')
EXEC	
	[dbo].[GetAllOffenderAddressDetails]
		@AutomonDatabaseName = 'CX',
		@LastExecutionDateTime = NULL,
		@OfficerLogonsToFilterTbl = @OfficerLogonsToFilterTbl
---------------------------------------------------------------------------------
History:-
Date			Author			Changes
04-July-18		Rajesh Awate	Created.
10-Sept-19		Rajesh Awate	Changes for integration by officer filter.
16-Sept-19		Rajesh Awate	Changes for issue in data type mismatch
==========================================================================================*/
CREATE PROCEDURE [dbo].[GetAllOffenderAddressDetails]
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
			OI.[Pin],
			PAI.[Id],
			PAI.[AddressTypeDescription] AS [AddressType],
			PAI.[Line1],
			PAI.[Line2],
			PAI.[City],
			PAI.[State],
			PAI.[Zip],
			PAI.[NoteValue] AS [Comment],
			PAI.[IsPrimary],
			CASE 
				WHEN PAI.[ToTime] IS NULL THEN 1
				ELSE 0
			END AS [IsActive]
		FROM
			[$AutomonDatabaseName].[dbo].[OffenderInfo] OI JOIN [$AutomonDatabaseName].[dbo].[PersonAddressInfo] PAI
				ON OI.[PersonId] = PAI.[PersonId]
		WHERE
			(PAI.[Line1] IS NOT NULL OR PAI.[Line2] IS NOT NULL OR PAI.[City] IS NOT NULL OR PAI.[State] IS NOT NULL OR PAI.[Zip] IS NOT NULL)
			AND
			(
				PAI.[FromTime] > @LastExecutionDateTime	OR @LastExecutionDateTime IS NULL
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
						AND CAST(([$AutomonDatabaseName].[dbo].[GetCaseAttributeValue](CI.[Id], NULL, ''SentencingDate'')) AS DATE) <= DATEADD(DAY, 30, GETDATE())
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
			PAI.[AddressTypeDescription] AS [AddressType],
			PAI.[Line1],
			PAI.[Line2],
			PAI.[City],
			PAI.[State],
			PAI.[Zip],
			PAI.[NoteValue] AS [Comment],
			PAI.[IsPrimary],
			CASE 
				WHEN PAI.[ToTime] IS NULL THEN 1
				ELSE 0
			END AS [IsActive]
		FROM
			[$AutomonDatabaseName].[dbo].[OffenderInfo] OI JOIN [$AutomonDatabaseName].[dbo].[PersonAddressInfo] PAI
				ON OI.[PersonId] = PAI.[PersonId]
		WHERE
			(PAI.[Line1] IS NOT NULL OR PAI.[Line2] IS NOT NULL OR PAI.[City] IS NOT NULL OR PAI.[State] IS NOT NULL OR PAI.[Zip] IS NOT NULL)
			AND
			(
				PAI.[FromTime] > @LastExecutionDateTime
				OR @LastExecutionDateTime IS NULL
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