
/*==========================================================================================
Author:			Rajesh Awate
Create date:	23-Jan-20
Description:	To save contact records received into repository table
---------------------------------------------------------------------------------
Test execution:-
DECLARE @ContactTbl [dbo].[ContactTbl];
INSERT INTO @ContactTbl
	(
		[Id], [IsImportSuccessful], [IntegrationId], [ContactId], [ContactValue], [ContactType]
	)
VALUES
	(
		0, 0, '0208AE2A-01DC-E911-B999-00505691041F', 'F5B55ECE-DDDB-E911-B999-00505691041F', '(570) 341-8036', 'Home Phone'
	),
	(
		0, 0, '0B28A55F-02DC-E911-B999-00505691041F', 'DD156EF2-DDDB-E911-B999-00505691041F', '(570) 689-9867', 'Home Phone'
	),
	(
		326, 0, '072E36C4-07DC-E911-B999-00505691041F', '279F7ABE-DEDB-E911-B999-00505691041F', '(570) 982-2918', 'Home Phone'
	)

EXEC	
	[dbo].[SaveContacts]
		@ContactTbl = @ContactTbl
---------------------------------------------------------------------------------
History:-
Date			Author			Changes
23-Jan-20		Rajesh Awate	Created.
==========================================================================================*/
CREATE PROCEDURE [dbo].[SaveContacts]
	@ContactTbl [dbo].[ContactTbl] READONLY
AS
BEGIN
	--merge address
	MERGE [dbo].[Contact] AS Tgt
	USING @ContactTbl AS Src
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
			AND Tgt.[ContactId] = Src.[ContactId]
		)
	)
	WHEN NOT MATCHED THEN  
		INSERT 
		(
			[IsImportSuccessful], [IntegrationId], [ContactId], [ContactValue], [ContactType]
		)
		VALUES 
		(
			Src.[IsImportSuccessful], Src.[IntegrationId], Src.[ContactId], Src.[ContactValue], Src.[ContactType]
		)
	WHEN MATCHED AND Tgt.[IsImportSuccessful] = 0 THEN
		UPDATE SET
			Tgt.[IsImportSuccessful] = Src.[IsImportSuccessful],
			Tgt.[IntegrationId] = Src.[IntegrationId],
			Tgt.[ContactId] = Src.[ContactId],
			Tgt.[ContactValue] = Src.[ContactValue],
			Tgt.[ContactType] = Src.[ContactType]
	;

	SELECT 
		[Id],
		[IsImportSuccessful],
		[IntegrationId],
		[ContactId],
		[ContactValue],
		[ContactType]
	FROM 
		[dbo].[Contact]
					
END