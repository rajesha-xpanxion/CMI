using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;
using Microsoft.Extensions.Options;

namespace CMI.Common.Notification
{
    public class EmailNotificationProvider : IEmailNotificationProvider
    {
        #region Private Member Variables
        private readonly EmailNotificationConfig emailNotificationConfig;
        #endregion

        #region Constructor
        public EmailNotificationProvider(
            IOptions<EmailNotificationConfig> emailNotificationConfig
        )
        {
            this.emailNotificationConfig = emailNotificationConfig.Value;
        }
        #endregion

        #region Public Methods
        public ExecutionStatusReportEmailResponse SendExecutionStatusReportEmail(ExecutionStatusReportEmailRequest request)
        {
            var response = new ExecutionStatusReportEmailResponse { IsSuccessful = true };
            try
            {
                if (emailNotificationConfig.IsEmailNotificationEnabled)
                {
                    string emailBody = GetEmailBodyForProcessorExecutionStatusReportEmail(request.TaskExecutionStatuses);

                    SendEmail(
                        emailNotificationConfig.SmtpServerHost,
                        emailNotificationConfig.SmtpServerPort,
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
        #endregion

        #region Private Helper Methods
        private void SendEmail(
            string smtpServerHostName,
            int smtpServerPort,
            string mailServerUserName,
            string mailServerPassword,
            bool isEnableSsl,
            string fromEmailAddress,
            string toEmailAddresses,
            string ccEmailAddresses,
            string subject,
            string body,
            MailPriority priority,
            List<string> attachmentFiles
        )
        {
            if (
                !string.IsNullOrEmpty(smtpServerHostName)
                && !string.IsNullOrEmpty(fromEmailAddress)
                && !string.IsNullOrEmpty(toEmailAddresses)
            )
            {
                using (SmtpClient smtpClient = new SmtpClient(smtpServerHostName))
                {
                    if (smtpServerPort > 0)
                    {
                        smtpClient.Port = smtpServerPort;
                    }

                    smtpClient.UseDefaultCredentials = (string.IsNullOrEmpty(mailServerUserName) || string.IsNullOrEmpty(mailServerPassword));

                    smtpClient.Credentials = (
                        (string.IsNullOrEmpty(mailServerUserName) || string.IsNullOrEmpty(mailServerPassword))
                        ? CredentialCache.DefaultNetworkCredentials
                        : new NetworkCredential(mailServerUserName, mailServerPassword)
                    );

                    smtpClient.EnableSsl = isEnableSsl;

                    MailMessage mailMessage = new MailMessage();
                    mailMessage.From = new MailAddress(fromEmailAddress);
                    mailMessage.Priority = priority;

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

                    AlternateView htmlView = AlternateView.CreateAlternateViewFromString(mailBody, Encoding.Default, Constants.MediaTypeHtml);
                    AlternateView textView = AlternateView.CreateAlternateViewFromString(mailBody, Encoding.Default, Constants.MediaTypePlain);

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
        }

        private IEnumerable<string> ConvertDelimitedEmailAddressToList(string emailAddresses)
        {
            return new List<string>(emailAddresses.Split(Constants.EmailAddressSeparators, StringSplitOptions.RemoveEmptyEntries));
        }

        private string GetEmailBodyForProcessorExecutionStatusReportEmail(IEnumerable<TaskExecutionStatus> taskExecutionStatuses)
        {
            string body = string.Empty;

            using (StreamReader reader = new StreamReader(Path.Combine(emailNotificationConfig.EmailAlertTemplatesPath, Constants.ProcessorExecutionStatusReportEmailTemplateFileName)))
            {
                body = reader.ReadToEnd();
            }

            StringBuilder dataHtmlBuilder = new StringBuilder();
            foreach (var taskExecutionStatus in taskExecutionStatuses)
            {
                dataHtmlBuilder.Append(
                    string.Format(
                        Constants.ExecutionStatusReportEmailBodyTableFormat,
                        taskExecutionStatus.TaskName,
                        (taskExecutionStatus.IsSuccessful ? "Yes" : "No"),
                        taskExecutionStatus.SourceReceivedRecordCount,
                        taskExecutionStatus.DestAddRecordCount,
                        taskExecutionStatus.DestUpdateRecordCount,
                        taskExecutionStatus.DestDeleteRecordCount,
                        taskExecutionStatus.DestFailureRecordCount
                    )
                );
            }

            body = body.Replace(Constants.TemplateVariableExecutionStatusDetails, dataHtmlBuilder.ToString());

            return body;
        }
        #endregion
    }
}
