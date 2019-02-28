
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
}
