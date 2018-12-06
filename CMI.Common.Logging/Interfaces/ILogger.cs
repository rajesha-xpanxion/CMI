
namespace CMI.Common.Logging
{
    public interface ILogger
    {
        void LogDebug(LogRequest logRequest);

        void LogInfo(LogRequest logRequest);

        void LogWarning(LogRequest logRequest);

        void LogError(LogRequest logRequest);
    }
}
