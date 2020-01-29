
namespace CMI.Importer.DAL
{
    public static class InboundImporterStage
    {
        public static string ClientProfiles { get { return "Client Profiles"; } }
        public static string Addresses { get { return "Addresses"; } }
        public static string Contacts { get { return "Contacts"; } }
        public static string CourtCases { get { return "Court Cases"; } }
    }

    public static class ExcelSheetExtensionType
    {
        public static string Xls { get { return ".xls"; } }
    }

    public static class UserDefinedTableType
    {
        public static string ClientProfileTbl { get { return "ClientProfileTbl"; } }
        public static string AddressTbl { get { return "AddressTbl"; } }
        public static string ContactTbl { get { return "ContactTbl"; } }
        public static string CourtCaseTbl { get { return "CourtCaseTbl"; } }
    }

    public static class StoredProc
    {
        public static string SaveClientProfiles { get { return @"[dbo].[SaveClientProfiles]"; } }
        public static string SaveAddresses { get { return @"[dbo].[SaveAddresses]"; } }
        public static string SaveContacts { get { return @"[dbo].[SaveContacts]"; } }
        public static string SaveCourtCases { get { return @"[dbo].[SaveCourtCases]"; } }
    }

    public static class ConfigKeys
    {
        public static string ImporterConfig { get { return "ImporterConfig"; } }
        public static string NexusConfig { get { return "NexusConfig"; } }
        public static string LogConfig { get { return "LogConfig"; } }
        public static string ImporterTypesToExecute { get { return "ImporterConfig:ImporterTypesToExecute"; } }
    }

    public static class TableColumnName
    {
        public static string Id { get { return "Id"; } }
        public static string IsImportSuccessful { get { return "IsImportSuccessful"; } }
        public static string IntegrationId { get { return "IntegrationId"; } }
        public static string FirstName { get { return "FirstName"; } }
        public static string MiddleName { get { return "MiddleName"; } }
        public static string LastName { get { return "LastName"; } }
        public static string ClientType { get { return "ClientType"; } }
        public static string TimeZone { get { return "TimeZone"; } }
        public static string Gender { get { return "Gender"; } }
        public static string Ethnicity { get { return "Ethnicity"; } }
        public static string DateOfBirth { get { return "DateOfBirth"; } }
        public static string SupervisingOfficerEmailId { get { return "SupervisingOfficerEmailId"; } }
        public static string ContactId { get { return "ContactId"; } }
        public static string ContactValue { get { return "ContactValue"; } }
        public static string ContactType { get { return "ContactType"; } }
        public static string AddressId { get { return "AddressId"; } }
        public static string FullAddress { get { return "FullAddress"; } }
        public static string AddressType { get { return "AddressType"; } }
        public static string IsPrimary { get { return "IsPrimary"; } }
        public static string CaseNumber { get { return "CaseNumber"; } }
        public static string CaseDate { get { return "CaseDate"; } }
        public static string Status { get { return "Status"; } }
        public static string EndDate { get { return "EndDate"; } }
        public static string EarlyReleaseDate { get { return "EarlyReleaseDate"; } }
        public static string EndReason { get { return "EndReason"; } }
    }

    public static class SqlParamName
    {
        public static string ClientProfileTbl { get { return "@ClientProfileTbl"; } }
        public static string AddressTbl { get { return "@AddressTbl"; } }
        public static string ContactTbl { get { return "@ContactTbl"; } }
        public static string CourtCaseTbl { get { return "@CourtCaseTbl"; } }
    }
}
