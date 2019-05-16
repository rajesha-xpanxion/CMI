
namespace CMI.Common.Notification
{
    public interface IEmailNotificationProvider
    {
        ExecutionStatusReportEmailResponse SendExecutionStatusReportEmail(ExecutionStatusReportEmailRequest request);

        ExecutionStatusReportEmailResponse SendCriticalErrorEmail(BaseEmailRequest request);
    }
}
