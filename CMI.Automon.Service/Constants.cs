
namespace CMI.Automon.Service
{
    public static class Constants
    {
        public static string TestDataJsonFileNameAllOffenderDetails { get { return "AllOffenderDetails.json"; } }
        public static string TestDataJsonFileNameAllOffenderAddressDetails { get { return "AllOffenderAddressDetails.json"; } }
        public static string TestDataJsonFileNameAllOffenderPhoneContactDetails { get { return "AllOffenderPhoneContactDetails.json"; } }
        public static string TestDataJsonFileNameAllOffenderEmailContactDetails { get { return "AllOffenderEmailContactDetails.json"; } }
        public static string TestDataJsonFileNameAllOffenderCaseDetails { get { return "AllOffenderCaseDetails.json"; } }
        public static string TestDataJsonFileNameAllOffenderNoteDetails { get { return "AllOffenderNoteDetails.json"; } }
        public static string TestDataJsonFileNameAllOffenderDrugTestAppointmentDetails { get { return "AllOffenderDrugTestAppointmentDetails.json"; } }
        public static string TestDataJsonFileNameAllOffenderDrugTestResultDetails { get { return "AllOffenderDrugTestResultDetails.json"; } }
        public static string TestDataJsonFileNameAllOffenderEmploymentDetails { get { return "AllOffenderEmploymentDetails.json"; } }
        public static string TestDataJsonFileNameAllOffenderFieldVisitDetails { get { return "AllOffenderFieldVisitDetails.json"; } }
        public static string TestDataJsonFileNameAllOffenderOfficeVisitDetails { get { return "AllOffenderOfficeVisitDetails.json"; } }
        public static string TestDataJsonFileNameAllOffenderPersonalDetails { get { return "AllOffenderPersonalDetails.json"; } }
        public static string TestDataJsonFileNameAllOffenderPhoneDetails { get { return "AllOffenderPhoneDetails.json"; } }
        public static string TestDataJsonFileNameAllOffenderTreatmentAppointmentDetails { get { return "AllOffenderTreatmentAppointmentDetails.json"; } }
        public static string TestDataJsonFileNameAllOffenderVehicleDetails { get { return "AllOffenderVehicleDetails.json"; } }
    }

    public static class SqlParamName
    {
        public static string AutomonDatabaseName { get { return "@AutomonDatabaseName"; } }
        public static string LastExecutionDateTime { get { return "@LastExecutionDateTime"; } }
        public static string Pin { get { return "@Pin"; } }
        public static string UpdatedBy { get { return "@UpdatedBy"; } }
        public static string FirstName { get { return "@FirstName"; } }
        public static string MiddleName { get { return "@MiddleName"; } }
        public static string LastName { get { return "@LastName"; } }
        public static string Race { get { return "@Race"; } }
        public static string DateOfBirth { get { return "@DateOfBirth"; } }
        public static string TimeZone { get { return "@TimeZone"; } }
        public static string OffenderType { get { return "@OffenderType"; } }
        public static string Gender { get { return "@Gender"; } }
        public static string EmailAddress { get { return "@EmailAddress"; } }
        public static string Line1 { get { return "@Line1"; } }
        public static string Line2 { get { return "@Line2"; } }
        public static string AddressType { get { return "@AddressType"; } }
        public static string Phone { get { return "@Phone"; } }
        public static string PhoneNumberType { get { return "@PhoneNumberType"; } }
        public static string VehicleYear { get { return "@VehicleYear"; } }
        public static string Make { get { return "@Make"; } }
        public static string BodyStyle { get { return "@BodyStyle"; } }
        public static string Color { get { return "@Color"; } }
        public static string LicensePlate { get { return "@LicensePlate"; } }
        public static string OrganizationName { get { return "@OrganizationName"; } }
        public static string OrganizationAddress { get { return "@OrganizationAddress"; } }
        public static string OrganizationPhone { get { return "@OrganizationPhone"; } }
        public static string PayFrequency { get { return "@PayFrequency"; } }
        public static string PayRate { get { return "@PayRate"; } }
        public static string WorkType { get { return "@WorkType"; } }
        public static string JobTitle { get { return "@JobTitle"; } }
        public static string Text { get { return "@Text"; } }
        public static string AuthorEmail { get { return "@AuthorEmail"; } }
        public static string Date { get { return "@Date"; } }
        public static string StartDate { get { return "@StartDate"; } }
        public static string Comment { get { return "@Comment"; } }
        public static string EndDate { get { return "@EndDate"; } }
        public static string Status { get { return "@Status"; } }
        public static string IsOffenderPresent { get { return "@IsOffenderPresent"; } }
        public static string IsSearchConducted { get { return "@IsSearchConducted"; } }
        public static string SearchLocations { get { return "@SearchLocations"; } }
        public static string SearchResults { get { return "@SearchResults"; } }
        public static string DeviceType { get { return "@DeviceType"; } }
        public static string TestResult { get { return "@TestResult"; } }
        public static string Validities { get { return "@Validities"; } }
        public static string Id { get { return "@Id"; } }
        public static string IsSaveFinalTestResult { get { return "@IsSaveFinalTestResult"; } }
    }

    public static class DbColumnName
    {
        public static string Pin { get { return "Pin"; } }
        public static string Id { get { return "Id"; } }
        public static string AddressType { get { return "AddressType"; } }
        public static string Line1 { get { return "Line1"; } }
        public static string Line2 { get { return "Line2"; } }
        public static string City { get { return "City"; } }
        public static string State { get { return "State"; } }
        public static string Zip { get { return "Zip"; } }
        public static string FirstName { get { return "FirstName"; } }
        public static string MiddleName { get { return "MiddleName"; } }
        public static string LastName { get { return "LastName"; } }
        public static string DateOfBirth { get { return "DateOfBirth"; } }
        public static string ClientType { get { return "ClientType"; } }
        public static string Gender { get { return "Gender"; } }
        public static string Race { get { return "Race"; } }
        public static string RaceDescription { get { return "RaceDescription"; } }
        public static string RacePermDesc { get { return "RacePermDesc"; } }
        public static string CaseloadName { get { return "CaseloadName"; } }
        public static string CaseloadType { get { return "CaseloadType"; } }
        public static string OfficerLogon { get { return "OfficerLogon"; } }
        public static string OfficerEmail { get { return "OfficerEmail"; } }
        public static string OfficerFirstName { get { return "OfficerFirstName"; } }
        public static string OfficerLastName { get { return "OfficerLastName"; } }
        public static string PhoneNumberType { get { return "PhoneNumberType"; } }
        public static string Phone { get { return "Phone"; } }
        public static string IsPrimary { get { return "IsPrimary"; } }
        public static string Comment { get { return "Comment"; } }
        public static string CaseNumber { get { return "CaseNumber"; } }
        public static string CaseStatus { get { return "CaseStatus"; } }
        public static string ClosureReason { get { return "ClosureReason"; } }
        public static string OffenseLabel { get { return "OffenseLabel"; } }
        public static string OffenseStatute { get { return "OffenseStatute"; } }
        public static string OffenseCategory { get { return "OffenseCategory"; } }
        public static string OffenseDate { get { return "OffenseDate"; } }
        public static string CaseDate { get { return "CaseDate"; } }
        public static string SupervisionStartDate { get { return "SupervisionStartDate"; } }
        public static string SupervisionEndDate { get { return "SupervisionEndDate"; } }
        public static string EmailAddress { get { return "EmailAddress"; } }
        public static string Date { get { return "Date"; } }
        public static string Text { get { return "Text"; } }
        public static string AuthorEmail { get { return "AuthorEmail"; } }
        public static string IsActive { get { return "IsActive"; } }
        public static string NoteType { get { return "NoteType"; } }
        public static string Make { get { return "Make"; } }
        public static string BodyStyle { get { return "BodyStyle"; } }
        public static string Vyear { get { return "Vyear"; } }
        public static string LicensePlate { get { return "LicensePlate"; } }
        public static string Color { get { return "Color"; } }
        public static string Name { get { return "Name"; } }
        public static string FullAddress { get { return "FullAddress"; } }
        public static string FullPhoneNumber { get { return "FullPhoneNumber"; } }
        public static string PayFrequency { get { return "PayFrequency"; } }
        public static string PayRate { get { return "PayRate"; } }
        public static string WorkType { get { return "WorkType"; } }
        public static string JobTitle { get { return "JobTitle"; } }
    }

    public static class TestResult
    {
        public static string Positive { get { return "Positive"; } }
        public static string Negative { get { return "Negative"; } }
        public static string Tampered { get { return "Tampered"; } }
    }
}
