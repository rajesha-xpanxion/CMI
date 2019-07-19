
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
}
