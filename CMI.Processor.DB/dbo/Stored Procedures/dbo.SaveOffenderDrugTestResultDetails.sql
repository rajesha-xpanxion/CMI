


/*==========================================================================================
Author:			Rajesh Awate
Create date:	08-Apr-19
Description:	To save offender drug test result details to given automon database
---------------------------------------------------------------------------------
Test execution:-
DECLARE @CurrentDate DATETIME = GETDATE();
EXEC	
	[dbo].[SaveOffenderDrugTestResultDetails]
		@AutomonDatabaseName = 'CX',
		@Pin = '5824',
		@Id = 0,
		@StartDate = @CurrentDate,
		@Comment = 'test office visit',
		@EndDate = @CurrentDate,
		@Status = 0, --Pending = 0, Missed = 16, Cancelled = 10, Complete = 2
		@DeviceType = 'Test',
		@TestResult = 'Failed',
		@Validities = 'Diluted',
		@IsSaveFinalTestResult = 0,
		@SentToLab = 'No',
		@LabRequisitionNumber = '123456789',
		@UpdatedBy = 'rawate@xpanxion.com';
---------------------------------------------------------------------------------
History:-
Date			Author			Changes
08-Apr-19		Rajesh Awate	Created.
08-July-19		Rajesh Awate	Changes to handle update scenario.
19-Aug-19		Rajesh Awate	Changes to save Final Test Result based on condition
13-Dec-19		Rajesh Awate	Changes for US116315
18-Dec-19		Rajesh Awate	Changes to save Sent To Lab attribute
19-Dec-19		Rajesh Awate	Changes to save Lab Requisition #
==========================================================================================*/
CREATE PROCEDURE [dbo].[SaveOffenderDrugTestResultDetails]
	@AutomonDatabaseName NVARCHAR(128),
	@Pin VARCHAR(20),
	@Id INT = 0,
	@StartDate DATETIME,
	@Comment VARCHAR(MAX) = NULL,
	@EndDate DATETIME,
	@Status INT = 0, --Pending = 0, Missed = 16, Cancelled = 10, Complete = 2
	@DeviceType VARCHAR(255) = NULL,
	@TestResult VARCHAR(255) = 'Failed',
	@Validities VARCHAR(255) = NULL,
	@IsSaveFinalTestResult BIT = 0,
	@SentToLab VARCHAR(255) = NULL,
	@LabRequisitionNumber VARCHAR(255) = NULL,
	@UpdatedBy VARCHAR(255)
AS
BEGIN
	DECLARE @SQLString NVARCHAR(MAX), @ParmDefinition NVARCHAR(1000);
	
		SET @SQLString = 
		'
		--declare required variables and assign it with values
		DECLARE 
			@EnteredByPId		INT	= ISNULL((SELECT [PersonId] FROM [$AutomonDatabaseName].[dbo].[OfficerInfo] WHERE [Email] = @UpdatedBy), 0),
			@PersonId			INT	= ISNULL((SELECT [PersonId] FROM [$AutomonDatabaseName].[dbo].[OffenderInfo] WHERE [Pin] = @Pin), 0),
			@OffenderId			INT	= ISNULL((SELECT [Id] FROM [$AutomonDatabaseName].[dbo].[OffenderInfo] WHERE [Pin] = @Pin), 0),
			@EventTypeId		INT	= (SELECT [Id] FROM [$AutomonDatabaseName].[dbo].[EventType] WHERE [PermDesc] = ''NexusDrugTestingResults''),
			@EventId			INT = @Id,
			@Value				VARCHAR(255);

		--check if OffenderId could be found for given Pin
		IF(@OffenderId IS NOT NULL AND @OffenderId > 0)
		BEGIN
		
			EXEC [$AutomonDatabaseName].[dbo].[UpdateEvent] @EventTypeId, @StartDate, @EnteredByPId, @Comment, 0, NULL, NULL, 0, @EndDate, NULL, @Status, NULL, @Id = @EventId OUTPUT;

			EXEC [$AutomonDatabaseName].[dbo].[UpdateOffenderEvent] @OffenderId, @EventId;

			--Test Date/Time
			SET @Value = CAST(@StartDate AS VARCHAR(255));
			EXEC [$AutomonDatabaseName].[dbo].[UpdateEventAttribute] @EventId, @EnteredByPId, @Value, NULL, ''CeDrugTest.TestDateTime'', NULL, NULL, NULL;

			--Device
			IF(@DeviceType IS NOT NULL)
			BEGIN
				EXEC [$AutomonDatabaseName].[dbo].[UpdateEventAttribute] @EventId, @EnteredByPId, @DeviceType, NULL, ''CeDrugTest.DeviceType'', NULL, NULL, NULL;
			END

			--Collector
			SET @Value = (SELECT [FirstLastName] FROM [$AutomonDatabaseName].[dbo].[OfficerInfo] WHERE [Email] = @UpdatedBy);
			EXEC [$AutomonDatabaseName].[dbo].[UpdateEventAttribute] @EventId, @EnteredByPId, @Value, NULL, ''CeDrugTest.Collector'', NULL, NULL, NULL;

			--Initial Test Result
			EXEC [$AutomonDatabaseName].[dbo].[UpdateEventAttribute] @EventId, @EnteredByPId, @TestResult, NULL, ''CeDrugTest.InitialOutcome'', NULL, NULL, NULL;

			--Final Test Result
			IF(@IsSaveFinalTestResult = 1)
			BEGIN
				EXEC [$AutomonDatabaseName].[dbo].[UpdateEventAttribute] @EventId, @EnteredByPId, @TestResult, NULL, ''CeDrugTest.FinalTestResult'', NULL, NULL, NULL;
			END

			--Validities
			IF(@Validities IS NOT NULL)
			BEGIN
				EXEC [$AutomonDatabaseName].[dbo].[UpdateEventAttribute] @EventId, @EnteredByPId, @Validities, NULL, ''CeDrugTest.Validities'', NULL, NULL, NULL;
			END

			--Sent To Lab
			IF(@SentToLab IS NOT NULL AND EXISTS(SELECT 1 FROM [$AutomonDatabaseName].[dbo].[AttributeDef] WHERE [Module] = ''Event'' AND [PermDesc] = ''CeDrugTest.SentToLab''))
			BEGIN
				EXEC [$AutomonDatabaseName].[dbo].[UpdateEventAttribute] @EventId, @EnteredByPId, @SentToLab, NULL, ''CeDrugTest.SentToLab'', NULL, NULL, NULL;
			END

			--Lab Requisition #
			IF(@LabRequisitionNumber IS NOT NULL AND EXISTS(SELECT 1 FROM [$AutomonDatabaseName].[dbo].[AttributeDef] WHERE [Module] = ''Event'' AND [PermDesc] = ''NexusDrugTest.LabReqNo''))
			BEGIN
				EXEC [$AutomonDatabaseName].[dbo].[UpdateEventAttribute] @EventId, @EnteredByPId, @LabRequisitionNumber, NULL, ''NexusDrugTest.LabReqNo'', NULL, NULL, NULL;
			END
		END
		
		SELECT @EventId;
		';


	SET @SQLString = REPLACE(@SQLString, '$AutomonDatabaseName', @AutomonDatabaseName);

	SET @ParmDefinition = '
		@Pin VARCHAR(20),
		@Id INT,
		@StartDate DATETIME,
		@Comment VARCHAR(MAX),
		@EndDate DATETIME,
		@Status INT,
		@DeviceType VARCHAR(255),
		@TestResult VARCHAR(255),
		@Validities VARCHAR(255),
		@IsSaveFinalTestResult BIT,
		@SentToLab VARCHAR(255),
		@LabRequisitionNumber VARCHAR(255)
		@UpdatedBy VARCHAR(255)';

--PRINT @SQLString;

	EXECUTE 
		sp_executesql 
			@SQLString, 
			@ParmDefinition,  
			@Pin = @Pin,
			@Id = @Id,
			@StartDate = @StartDate,
			@Comment = @Comment,
			@EndDate = @EndDate,
			@Status = @Status,
			@DeviceType = @DeviceType,
			@TestResult = @TestResult,
			@Validities = @Validities,
			@IsSaveFinalTestResult = @IsSaveFinalTestResult,
			@SentToLab = @SentToLab,
			@LabRequisitionNumber = @LabRequisitionNumber,
			@UpdatedBy = @UpdatedBy;
END