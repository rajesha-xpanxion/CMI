
/*==========================================================================================
Author:			Rajesh Awate
Create date:	23-Jan-20
Description:	To save court case records received into repository table
---------------------------------------------------------------------------------
Test execution:-
DECLARE @CourtCaseTbl [dbo].[CourtCaseTbl];
INSERT INTO @CourtCaseTbl
	(
		[Id], [IsImportSuccessful], [IntegrationId], [CaseNumber], [CaseDate], [Status], [EndDate], [EarlyReleaseDate], [EndReason]
	)
VALUES
	(
		0, 0, '0208AE2A-01DC-E911-B999-00505691041F', 'CP-35-CR-0002328-2011', '9/21/2019 9:43:28 PM', 'Active', '1/29/2014', '1/29/2014', NULL
	),
	(
		0, 0, '0B28A55F-02DC-E911-B999-00505691041F', 'CP-35-CR-0001020-2018', '9/22/2019 12:15:20 AM', 'Active', '1/22/2021', NULL, NULL
	),
	(
		2116, 0, '072E36C4-07DC-E911-B999-00505691041F', 'CP-35-CR-0001839-2018', '9/21/2019 10:20:16 PM', 'Active', '12/19/2019', NULL, NULL
	)

EXEC	
	[dbo].[SaveCourtCases]
		@CourtCaseTbl = @CourtCaseTbl
---------------------------------------------------------------------------------
History:-
Date			Author			Changes
23-Jan-20		Rajesh Awate	Created.
==========================================================================================*/
CREATE PROCEDURE [dbo].[SaveCourtCases]
	@CourtCaseTbl [dbo].[CourtCaseTbl] READONLY
AS
BEGIN
	--merge address
	MERGE [dbo].[CourtCase] AS Tgt
	USING @CourtCaseTbl AS Src
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
			AND Tgt.[CaseNumber] = Src.[CaseNumber]
		)
	)
	WHEN NOT MATCHED THEN  
		INSERT 
		(
			[IsImportSuccessful], [IntegrationId], [CaseNumber], [CaseDate], [Status], [EndDate], [EarlyReleaseDate], [EndReason]
		)
		VALUES 
		(
			Src.[IsImportSuccessful], Src.[IntegrationId], Src.[CaseNumber], Src.[CaseDate], Src.[Status], Src.[EndDate], Src.[EarlyReleaseDate], Src.[EndReason]
		)
	WHEN MATCHED AND Tgt.[IsImportSuccessful] = 0 THEN
		UPDATE SET
			Tgt.[IsImportSuccessful] = Src.[IsImportSuccessful],
			Tgt.[IntegrationId] = Src.[IntegrationId],
			Tgt.[CaseNumber] = Src.[CaseNumber],
			Tgt.[CaseDate] = Src.[CaseDate],
			Tgt.[Status] = Src.[Status],
			Tgt.[EndDate] = Src.[EndDate],
			Tgt.[EarlyReleaseDate] = Src.[EarlyReleaseDate],
			Tgt.[EndReason] = Src.[EndReason]
	;

	SELECT 
		[Id],
		[IsImportSuccessful],
		[IntegrationId],
		[CaseNumber],
		[CaseDate],
		[Status],
		[EndDate],
		[EarlyReleaseDate],
		[EndReason]
	FROM 
		[dbo].[CourtCase]
					
END