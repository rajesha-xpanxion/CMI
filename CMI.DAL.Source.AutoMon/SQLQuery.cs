
namespace CMI.DAL.Source.AutoMon
{
    public static class SqlQuery
    {
        public static string GetTimeZone { get { return @"SELECT [ItemValue] FROM [dbo].[InstallationParam] WHERE [Name] = 'Time Zone'"; } }
    }

    public static class StoredProc
    {
        public static string GetAllOffenderDetails { get { return "[dbo].[GetAllOffenderDetails]"; } }
        public static string GetAllOffenderAddressDetails { get { return "[dbo].[GetAllOffenderAddressDetails]"; } }
        public static string GetAllOffenderPhoneDetails { get { return "[dbo].[GetAllOffenderPhoneDetails]"; } }
        public static string GetAllOffenderEmailDetails { get { return "[dbo].[GetAllOffenderEmailDetails]"; } }
        public static string GetAllOffenderCaseDetails { get { return "[dbo].[GetAllOffenderCaseDetails]"; } }
        public static string GetAllOffenderNoteDetails { get { return "[dbo].[GetAllOffenderNoteDetails]"; } }
    }
}
