
/*==========================================================================================
Author:			Rajesh Awate
Create date:	23-Jan-20
Description:	To save client profile records received into repository table
---------------------------------------------------------------------------------
Test execution:-
DECLARE @ClientProfileTbl [dbo].[ClientProfileTbl];
INSERT INTO @ClientProfileTbl
	(
		[Id], [IsImportSuccessful], [IntegrationId], [FirstName], [MiddleName], [LastName], [ClientType], 
		[TimeZone], [Gender], [Ethnicity], [DateOfBirth], [SupervisingOfficerEmailId]
	)
VALUES
	(
		0, 0, '0208AE2A-01DC-E911-B999-00505691041F', 'Julio', 'Antonio', 'Cruz-Hernandez', 'Offender',
		'Eastern Standard Time', 'Male', 'Hispanic', '7/7/1980', 'DavisM@lackawannacounty.org'
	),
	(
		0, 0, '03727FA5-DD0A-EA11-B823-005056919E4A', 'Joshua', 'Lee', 'Norman', 'Offender',
		'Eastern Standard Time', 'Male', 'Non-Hispanic', '12/15/1975', 'GianacopoulosJ@lackawannacounty.org'
	),
	(
		62, 0, '072E36C4-07DC-E911-B999-00505691041F', 'Xavier', 'E.', 'Rodriguez', 'Offender',
		'Eastern Standard Time', 'Male', 'Hispanic', '7/31/1994', 'GianacopoulosJ@lackawannacounty.org'
	)

EXEC	
	[dbo].[SaveClientProfiles]
		@ClientProfileTbl = @ClientProfileTbl
---------------------------------------------------------------------------------
History:-
Date			Author			Changes
23-Jan-20		Rajesh Awate	Created.
==========================================================================================*/
CREATE PROCEDURE [dbo].[SaveClientProfiles]
	@ClientProfileTbl [dbo].[ClientProfileTbl] READONLY
AS
BEGIN
	--merge client profiles
	MERGE [dbo].[ClientProfile] AS Tgt
	USING @ClientProfileTbl AS Src
	ON 
	(
		(
			Src.[Id] > 0 
			AND Tgt.[Id] = Src.[Id]
		) 
		OR 
		(
			Src.[Id] = 0 
			AND Tgt.[IntegrationId] = Src.[IntegrationId]
		)
	)
	WHEN NOT MATCHED THEN  
		INSERT 
		(
			[IsImportSuccessful], [IntegrationId], [FirstName], [MiddleName], [LastName], [ClientType], 
			[TimeZone], [Gender], [Ethnicity], [DateOfBirth], [SupervisingOfficerEmailId]
		)
		VALUES 
		(
			Src.[IsImportSuccessful], Src.[IntegrationId], Src.[FirstName], Src.[MiddleName], Src.[LastName], Src.[ClientType], 
			Src.[TimeZone], Src.[Gender], Src.[Ethnicity], Src.[DateOfBirth], Src.[SupervisingOfficerEmailId]
		)
	WHEN MATCHED AND Tgt.[IsImportSuccessful] = 0 THEN
		UPDATE SET
			Tgt.[IsImportSuccessful] = Src.[IsImportSuccessful],
			Tgt.[IntegrationId] = Src.[IntegrationId],
			Tgt.[FirstName] = Src.[FirstName],
			Tgt.[MiddleName] = Src.[MiddleName],
			Tgt.[LastName] = Src.[LastName],
			Tgt.[ClientType] = Src.[ClientType],
			Tgt.[TimeZone] = Src.[TimeZone],
			Tgt.[Gender] = Src.[Gender],
			Tgt.[Ethnicity] = Src.[Ethnicity],
			Tgt.[DateOfBirth] = Src.[DateOfBirth],
			Tgt.[SupervisingOfficerEmailId] = Src.[SupervisingOfficerEmailId]
	;

	SELECT 
		[Id],
		[IsImportSuccessful],
		[IntegrationId],
		[FirstName],
		[MiddleName],
		[LastName],
		[ClientType],
		[TimeZone],
		[Gender],
		[Ethnicity],
		[DateOfBirth],
		[SupervisingOfficerEmailId]
	FROM 
		[dbo].[ClientProfile]
					
END