namespace CMI.Processor.DAL
{
    public static class Constants
    {
        public static string ContactTypeEmailNexus { get { return "E-mail"; } }

        public static string EthnicityUnknown { get { return "Unknown"; } }
    }

    public static class SqlParamName
    {
        public static string ProcessorTypeId { get { return "@ProcessorTypeId"; } }

        public static string ExecutedOn { get { return "@ExecutedOn"; } }

        public static string IsSuccessful { get { return "@IsSuccessful"; } }

        public static string NumTaskProcessed { get { return "@NumTaskProcessed"; } }

        public static string NumTaskSucceeded { get { return "@NumTaskSucceeded"; } }

        public static string NumTaskFailed { get { return "@NumTaskFailed"; } }

        public static string Message { get { return "@Message"; } }

        public static string ErrorDetails { get { return "@ErrorDetails"; } }

        public static string OutboundMessageTbl { get { return "@OutboundMessageTbl"; } }

        public static string ReceivedOn { get { return "@ReceivedOn"; } }

    }

    public static class StoredProc
    {
        public static string GetLastExecutionDateTime { get { return @"[dbo].[GetLastExecutionDateTime]"; } }
        public static string SaveExecutionStatus { get { return @"[dbo].[SaveExecutionStatus]"; } }
        public static string SaveOutboundMessages { get { return @"[dbo].[SaveOutboundMessages]"; } }
    }

    public static class UserDefinedTableType
    {
        public static string OutboundMessageTbl { get { return "OutboundMessageTbl"; } }
        public static string Varchar50Tbl { get { return "Varchar50Tbl"; } }
    }

    public static class TableColumnName
    {
        public static string Id { get { return "Id"; } }
        public static string ActivityTypeId { get { return "ActivityTypeId"; } }
        public static string ActivityTypeName { get { return "ActivityTypeName"; } }
        public static string ActivitySubTypeId { get { return "ActivitySubTypeId"; } }
        public static string ActivitySubTypeName { get { return "ActivitySubTypeName"; } }
        public static string ActionReasonId { get { return "ActionReasonId"; } }
        public static string ActionReasonName { get { return "ActionReasonName"; } }
        public static string ClientIntegrationId { get { return "ClientIntegrationId"; } }
        public static string ActivityIdentifier { get { return "ActivityIdentifier"; } }
        public static string ActionOccurredOn { get { return "ActionOccurredOn"; } }
        public static string ActionUpdatedBy { get { return "ActionUpdatedBy"; } }
        public static string Details { get { return "Details"; } }
        public static string IsSuccessful { get { return "IsSuccessful"; } }
        public static string ErrorDetails { get { return "ErrorDetails"; } }
        public static string RawData { get { return "RawData"; } }
        public static string IsProcessed { get { return "IsProcessed"; } }
        public static string ReceivedOn { get { return "ReceivedOn"; } }
        public static string AutomonIdentifier { get { return "AutomonIdentifier"; } }
        public static string Item { get { return "Item"; } }
    }

    public static class InboundProcessorStage
    {
        public static string ClientProfiles { get { return "ClientProfiles"; } }
        public static string Addresses { get { return "Addresses"; } }
        public static string PhoneContacts { get { return "PhoneContacts"; } }
        public static string EmailContacts { get { return "EmailContacts"; } }
        public static string Cases { get { return "Cases"; } }
        public static string Notes { get { return "Notes"; } }
        public static string Vehicles { get { return "Vehicles"; } }
        public static string Employments { get { return "Employments"; } }
    }

    public static class OutboundProcessorActivityType
    {
        public static string Client { get { return "Client"; } }
        public static string ClientProfile { get { return "ClientProfile"; } }
        public static string Note { get { return "Note"; } }
        public static string OfficeVisit { get { return "Office Visit"; } }
        public static string DrugTestAppointment { get { return "Drug Test Appointment"; } }
        public static string DrugTestResult { get { return "Drug Test Result"; } }
        public static string FieldVisit { get { return "FieldVisit"; } }
        public static string TreatmentAppointment { get { return "Treatment Appointment"; } }
        public static string CAMAlert { get { return "CAM Alert"; } }
        public static string CAMSupervision { get { return "CAM Supervision"; } }
        public static string GPSAlert { get { return "GPS Alert"; } }
        public static string GPSSupervision { get { return "GPS Supervision"; } }
        public static string Incentive { get { return "Incentive"; } }
        public static string Sanction { get { return "Sanction"; } }
    }

    public static class OutboundProcessorClientProfileActivitySubType
    {
        public static string PersonalDetails { get { return "Personal Details"; } }
        public static string EmailDetails { get { return "Email Details"; } }
        public static string AddressDetails { get { return "Address Details"; } }
        public static string ContactDetails { get { return "Contact Details"; } }
        public static string VehicleDetails { get { return "Vehicle Details"; } }
        public static string EmploymentDetails { get { return "Employment Details"; } }
        public static string ProfilePicture { get { return "Profile Picture"; } }
    }

    public static class OutboundProcessorActionReason
    {
        public static string Created { get { return "Created"; } }
        public static string Updated { get { return "Updated"; } }
        public static string Removed { get { return "Removed"; } }
    }

    public static class ConfigKeys
    {
        public static string ProcessorConfig { get { return "ProcessorConfig"; } }
        public static string NexusConfig { get { return "NexusConfig"; } }
        public static string AutomonConfig { get { return "AutomonConfig"; } }
        public static string LogConfig { get { return "LogConfig"; } }
        public static string MessageRetrieverConfig { get { return "MessageRetrieverConfig"; } }
        public static string EmailNotificationConfig { get { return "NotificationConfig:EmailNotificationConfig"; } }
        public static string MessageRetrieverTypeToExecute { get { return "ProcessorConfig:OutboundProcessorConfig:MessageRetrieverConfig:RetrieverTypeToExecute"; } }
        public static string ProcessorTypesToExecute { get { return "ProcessorConfig:ProcessorTypesToExecute"; } }
        public static string ExecutionStatusReportReceiverEmailAddresses { get { return "ProcessorConfig:ExecutionStatusReportReceiverEmailAddresses"; } }
        public static string ExecutionStatusReportEmailSubject { get { return "ProcessorConfig:ExecutionStatusReportEmailSubject"; } }
    }
    public static class MessageRetrieverTypeToExecute
    {
        public static string Rest { get { return "REST"; } }
        public static string Amqp { get { return "AMQP"; } }
    }
}
