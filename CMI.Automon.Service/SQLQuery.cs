
namespace CMI.Automon.Service
{
    public static class SqlQuery
    {
        public static string GetTimeZone { get { return @"SELECT [ItemValue] FROM [dbo].[InstallationParam] WHERE [Name] = 'Time Zone'"; } }
    }

    public static class StoredProc
    {
        public static string GetAllOffenderDetails { get { return "[dbo].[GetAllOffenderDetails]"; } }
        public static string GetOffenderMugshotPhoto { get { return "[dbo].[GetOffenderMugshotPhoto]"; } }
        public static string GetAllOffenderAddressDetails { get { return "[dbo].[GetAllOffenderAddressDetails]"; } }
        public static string GetAllOffenderPhoneDetails { get { return "[dbo].[GetAllOffenderPhoneDetails]"; } }
        public static string GetAllOffenderEmailDetails { get { return "[dbo].[GetAllOffenderEmailDetails]"; } }
        public static string GetAllOffenderCaseDetails { get { return "[dbo].[GetAllOffenderCaseDetails]"; } }
        public static string GetAllOffenderNoteDetails { get { return "[dbo].[GetAllOffenderNoteDetails]"; } }
        public static string GetAllOffenderVehicleDetails { get { return "[dbo].[GetAllOffenderVehicleDetails]"; } }
        public static string GetAllOffenderEmploymentDetails { get { return "[dbo].[GetAllOffenderEmploymentDetails]"; } }
        public static string SaveNewOffender { get { return "[dbo].[SaveNewOffender]"; } }
        public static string SaveOffenderPersonalDetails { get { return "[dbo].[SaveOffenderPersonalDetails]"; } }
        public static string SaveOffenderEmailDetails { get { return "[dbo].[SaveOffenderEmailDetails]"; } }
        public static string SaveOffenderAddressDetails { get { return "[dbo].[SaveOffenderAddressDetails]"; } }
        public static string SaveOffenderPhoneDetails { get { return "[dbo].[SaveOffenderPhoneDetails]"; } }
        public static string SaveOffenderVehicleDetails { get { return "[dbo].[SaveOffenderVehicleDetails]"; } }
        public static string DeleteOffenderVehicleDetails { get { return "[dbo].[DeleteOffenderVehicleDetails]"; } }
        public static string SaveOffenderEmploymentDetails { get { return "[dbo].[SaveOffenderEmploymentDetails]"; } }
        public static string DeleteOffenderEmploymentDetails { get { return "[dbo].[DeleteOffenderEmploymentDetails]"; } }
        public static string SaveOffenderNoteDetails { get { return "[dbo].[SaveOffenderNoteDetails]"; } }
        public static string SaveOffenderOfficeVisitDetails { get { return "[dbo].[SaveOffenderOfficeVisitDetails]"; } }
        public static string SaveOffenderDrugTestResultDetails { get { return "[dbo].[SaveOffenderDrugTestResultDetails]"; } }
        public static string SaveOffenderDrugTestAppointmentDetails { get { return "[dbo].[SaveOffenderDrugTestAppointmentDetails]"; } }
        public static string SaveOffenderFieldVisitDetails { get { return "[dbo].[SaveOffenderFieldVisitDetails]"; } }
        public static string SaveOffenderTreatmentAppointmentDetails { get { return "[dbo].[SaveOffenderTreatmentAppointmentDetails]"; } }
        public static string SaveOffenderMugshotDetails { get { return "[dbo].[SaveOffenderMugshotDetails]"; } }
        public static string DeleteOffenderMushotPhoto { get { return "[dbo].[DeleteOffenderMushotPhoto]"; } }
        public static string SaveOffenderCAMViolationDetails { get { return "[dbo].[SaveOffenderCAMViolationDetails]"; } }
        public static string SaveOffenderGPSViolationDetails { get { return "[dbo].[SaveOffenderGPSViolationDetails]"; } }
        public static string SaveOffenderIncentiveDetails { get { return "[dbo].[SaveOffenderIncentiveDetails]"; } }
    }
}
