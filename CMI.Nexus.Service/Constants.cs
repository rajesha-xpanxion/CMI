
namespace CMI.Nexus.Service
{
    public static class Constants
    {
        /// <summary>
        /// Http header type of "Authorization"
        /// </summary>
        public static string HeaderTypeAuthorization { get { return "Authorization"; } }

        /// <summary>
        /// Represents content type format - JSON
        /// </summary>
        public static string ContentTypeFormatJson { get { return "application/json"; } }

        /// <summary>
        /// Represent expected minimum lenght of Id
        /// </summary>
        public static int ExpectedMinLenghOfId { get { return 3; } }

        /// <summary>
        /// Represent auth token format to passed in to API
        /// </summary>
        public static string AuthTokenFormat { get { return "{0} {1}"; } }
    }

    public static class Status
    {
        public static string Attended { get { return "Attended"; } }

        public static string Missed { get { return "Missed"; } }

        public static string Excused { get { return "Excused"; } }

        public static string Completed { get { return "Completed"; } }

        public static string Tampered { get { return "Tampered"; } }

        public static string Passed { get { return "Passed"; } }

        public static string Failed { get { return "Failed"; } }

        public static string Attempted { get { return "Attempted"; } }

        public static string Skipped { get { return "Skipped"; } }

        public static string Compliant { get { return "compliant"; } }

        public static string Removed { get { return "Removed"; } }
    }

    public static class DataElementType
    {
        public static string Client { get { return "Client"; } }
        public static string Address { get { return "Address"; } }
        public static string Contact { get { return "Contact"; } }
        public static string Case { get { return "Case"; } }
        public static string Employer { get { return "Employer"; } }
        public static string Vehicle { get { return "Vehicle"; } }
        public static string Note { get { return "Note"; } }
        public static string ProtectiveOrder { get { return "ProtectiveOrder"; } }

    }

    public static class StaticRiskRating
    {
        public static string Unspecified { get { return "Unspecified"; } }
        public static string Low { get { return "Low"; } }
        public static string Medium { get { return "Medium"; } }
        public static string HighDrug { get { return "High - Drug"; } }
        public static string HighProperty { get { return "High - Property"; } }
        public static string HighViolence { get { return "High - Violence"; } }
        public static string CMLow { get { return "CM-Low"; } }
        public static string CMMedium { get { return "CM-Medium"; } }
        public static string CMHigh { get { return "CM-High"; } }
        public static string High { get { return "High"; } }
    }
}
