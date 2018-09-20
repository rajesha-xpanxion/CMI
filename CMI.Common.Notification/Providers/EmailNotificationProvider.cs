using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace CMI.Common.Notification
{
    public class EmailNotificationProvider : IEmailNotificationProvider
    {
        private EmailNotificationConfig emailNotificationConfig;
        public EmailNotificationProvider(Microsoft.Extensions.Options.IOptions<EmailNotificationConfig> emailNotificationConfig)
        {
            this.emailNotificationConfig = emailNotificationConfig.Value;
        }

        public ExecutionStatusReportEmailResponse SendExecutionStatusReportEmail(ExecutionStatusReportEmailRequest request)
        {
            var response = new ExecutionStatusReportEmailResponse() { IsSuccessful = true };
            try
            {
                if (emailNotificationConfig.IsEmailNotificationEnabled)
                {
                    string emailBody = GetEmailBodyForProcessorExecutionStatusReportEmail(request.TaskExecutionStatuses);

                    SendEmail(
                        emailNotificationConfig.SMTPServerHost,
                        emailNotificationConfig.MailServerUserName,
                        emailNotificationConfig.MailServerPassword,
                        emailNotificationConfig.IsEnableSsl,
                        emailNotificationConfig.FromEmailAddress,
                        request.ToEmailAddress,
                        string.Empty,
                        request.Subject,
                        emailBody,
                        MailPriority.Normal,
                        null
                        );

                }
            }
            catch(Exception ex)
            {
                response.IsSuccessful = false;
                response.Exception = ex;
            }

            return response;
        }

        void SendEmail(string smtpServerHostName, string mailServerUserName, string mailServerPassword, bool isEnableSsl, string fromEmailAddress, string toEmailAddresses, string ccEmailAddresses, string subject, string body, MailPriority priority, List<string> attachmentFiles)
        {
            if (
                !string.IsNullOrEmpty(smtpServerHostName)
                && !string.IsNullOrEmpty(fromEmailAddress)
                && !string.IsNullOrEmpty(toEmailAddresses)
            )
                using (SmtpClient smtpClient = new SmtpClient(smtpServerHostName))
                {
                    smtpClient.UseDefaultCredentials = true;
                    smtpClient.Credentials = (
                        (string.IsNullOrEmpty(mailServerUserName) || string.IsNullOrEmpty(mailServerPassword))
                        ? System.Net.CredentialCache.DefaultNetworkCredentials
                        : new NetworkCredential(mailServerUserName, mailServerPassword)
                        );
                    smtpClient.EnableSsl = isEnableSsl;

                    MailMessage mailMessage = new MailMessage();
                    mailMessage.From = new MailAddress(fromEmailAddress);
                    mailMessage.Priority = (MailPriority)priority;

                    foreach (string toEmailAddress in ConvertDelimitedEmailAddressToList(toEmailAddresses))
                    {
                        mailMessage.To.Add(toEmailAddress);
                    }

                    if (!string.IsNullOrEmpty(ccEmailAddresses))
                    {
                        foreach (string ccEmailAddress in ConvertDelimitedEmailAddressToList(ccEmailAddresses))
                        {
                            mailMessage.CC.Add(ccEmailAddress);
                        }
                    }


                    mailMessage.Subject = subject;

                    string mailBody = body;

                    AlternateView htmlView = AlternateView.CreateAlternateViewFromString(mailBody, System.Text.Encoding.Default, "text/html");
                    AlternateView textView = AlternateView.CreateAlternateViewFromString(mailBody, System.Text.Encoding.Default, "text/plain");

                    mailMessage.AlternateViews.Add(textView);
                    mailMessage.AlternateViews.Add(htmlView);

                    if (attachmentFiles != null)
                    {
                        foreach (var attachmentFile in attachmentFiles)
                        {
                            if (!string.IsNullOrEmpty(attachmentFile))
                            {
                                mailMessage.Attachments.Add(new Attachment(attachmentFile));
                            }
                        }
                    }

                    smtpClient.Send(mailMessage);
                }
        }

        IEnumerable<string> ConvertDelimitedEmailAddressToList(string emailAddresses)
        {
            return new List<string>(emailAddresses.Split(new string[] { ";", ",", "|" }, StringSplitOptions.RemoveEmptyEntries));
        }

        string GetEmailBodyForProcessorExecutionStatusReportEmail(IEnumerable<TaskExecutionStatus> taskExecutionStatuses)
        {
            string body = string.Empty, dataHtml = string.Empty;

            using (StreamReader reader = new StreamReader(Path.Combine(emailNotificationConfig.EmailAlertTemplatesPath, Constants.ProcessorExecutionStatusReportEmailTemplateFileName)))
            {
                body = reader.ReadToEnd();
            }

            foreach (var taskExecutionStatus in taskExecutionStatuses)
            {
                dataHtml += string.Format(
                    "<tr><td>{0}</td><td>{1}</td><td>{2}</td><td>{3}</td><td>{4}</td><td>{5}</td><td>{6}</td></tr>",
                    taskExecutionStatus.TaskName,
                    (taskExecutionStatus.IsSuccessful ? "Yes" : "No"),
                    taskExecutionStatus.SourceReceivedRecordCount,
                    taskExecutionStatus.DestAddRecordCount,
                    taskExecutionStatus.DestUpdateRecordCount,
                    taskExecutionStatus.DestDeleteRecordCount,
                    taskExecutionStatus.DestFailureRecordCount
                );
            }

            body = body.Replace(Constants.TemplateVariableExecutionStatusDetails, dataHtml);

            return body;
        }
    }
}
