
/*==========================================================================================
Author:			Rajesh Awate
Create date:	23-Jan-20
Description:	To save address records received into repository table
---------------------------------------------------------------------------------
Test execution:-
DECLARE @AddressTbl [dbo].[AddressTbl];
INSERT INTO @AddressTbl
	(
		[Id], [IsImportSuccessful], [IntegrationId], [AddressId], [FullAddress], [AddressType], [IsPrimary]
	)
VALUES
	(
		0, 0, '0208AE2A-01DC-E911-B999-00505691041F', '528176D1-9E22-4DB1-88BE-FB5765A7E761', '20 Amity Ct  Apt 2 Scranton, PA 18509', 'Physical', 'Yes'
	),
	(
		0, 0, '03727FA5-DD0A-EA11-B823-005056919E4A', '44FD679B-27FB-4A5E-AA10-441932FFC04C', '241 MADISON ST WILKES-BARRE, PA 18705', 'Physical', 'Yes'
	),
	(
		625, 0, '072E36C4-07DC-E911-B999-00505691041F', 'EACEF23E-0C0C-4868-9D91-E56BF3E718FB', '1412 Schlager St. Scranton, PA 18504', 'Physical', 'Yes'
	)

EXEC	
	[dbo].[SaveAddresses]
		@AddressTbl = @AddressTbl
---------------------------------------------------------------------------------
History:-
Date			Author			Changes
23-Jan-20		Rajesh Awate	Created.
==========================================================================================*/
CREATE PROCEDURE [dbo].[SaveAddresses]
	@AddressTbl [dbo].[AddressTbl] READONLY
AS
BEGIN
	--merge address
	MERGE [dbo].[Address] AS Tgt
	USING @AddressTbl AS Src
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
			AND Tgt.[AddressId] = Src.[AddressId]
		)
	)
	WHEN NOT MATCHED THEN  
		INSERT 
		(
			[IsImportSuccessful], [IntegrationId], [AddressId], [FullAddress], [AddressType], [IsPrimary]
		)
		VALUES 
		(
			Src.[IsImportSuccessful], Src.[IntegrationId], Src.[AddressId], Src.[FullAddress], Src.[AddressType], Src.[IsPrimary]
		)
	WHEN MATCHED AND Tgt.[IsImportSuccessful] = 0 THEN
		UPDATE SET
			Tgt.[IsImportSuccessful] = Src.[IsImportSuccessful],
			Tgt.[IntegrationId] = Src.[IntegrationId],
			Tgt.[AddressId] = Src.[AddressId],
			Tgt.[FullAddress] = Src.[FullAddress],
			Tgt.[AddressType] = Src.[AddressType],
			Tgt.[IsPrimary] = Src.[IsPrimary]
	;

	SELECT 
		[Id],
		[IsImportSuccessful],
		[IntegrationId],
		[AddressId],
		[FullAddress],
		[AddressType],
		[IsPrimary]
	FROM 
		[dbo].[Address]
					
END