﻿

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
		@UpdatedBy = 'rawate@xpanxion.com';
---------------------------------------------------------------------------------
History:-
Date			Author			Changes
08-Apr-19		Rajesh Awate	Created.
08-July-19		Rajesh Awate	Changes to handle update scenario.
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
			@EventTypeId		INT	= (SELECT [Id] FROM [$AutomonDatabaseName].[dbo].[EventType] WHERE [PermDesc] = ''CeDrugTestingResults''),
			@EventId			INT = @Id,
			@Value				VARCHAR(255);

		EXEC [$AutomonDatabaseName].[dbo].[UpdateEvent] @EventTypeId, @StartDate, @EnteredByPId, @Comment, 0, NULL, NULL, 0, @EndDate, NULL, @Status, NULL, @Id = @EventId OUTPUT;

		IF(@OffenderId IS NOT NULL AND @OffenderId > 0)
		BEGIN
			EXEC [$AutomonDatabaseName].[dbo].[UpdateOffenderEvent] @OffenderId, @EventId;
		END

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
		EXEC [$AutomonDatabaseName].[dbo].[UpdateEventAttribute] @EventId, @EnteredByPId, @TestResult, NULL, ''CeDrugTest.FinalTestResult'', NULL, NULL, NULL;

		--Validities
		IF(@Validities IS NOT NULL)
		BEGIN
			EXEC [$AutomonDatabaseName].[dbo].[UpdateEventAttribute] @EventId, @EnteredByPId, @Validities, NULL, ''CeDrugTest.Validities'', NULL, NULL, NULL;
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
		@UpdatedBy VARCHAR(255)';

PRINT @SQLString;

	EXECUTE sp_executesql 
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
				@UpdatedBy = @UpdatedBy;
END