using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;
using Microsoft.Extensions.Options;
using System.Linq;

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
                    string emailBody = GetEmailBodyForProcessorExecutionStatusReportEmail(request.TaskExecutionStatuses, request.ProcessorType);

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
                        request.TaskExecutionStatuses.Any(x => x.AutomonReceivedRecordCount > 0 || x.NexusReceivedMessageCount > 0) ? MailPriority.High : MailPriority.Normal,
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

        public ExecutionStatusReportEmailResponse SendCriticalErrorEmail(BaseEmailRequest request)
        {
            var response = new ExecutionStatusReportEmailResponse { IsSuccessful = true };
            try
            {
                if (emailNotificationConfig.IsEmailNotificationEnabled)
                {
                    string emailBody = GetEmailBodyForCriticalErrorEmail();

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
            catch (Exception ex)
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

        private string GetEmailBodyForProcessorExecutionStatusReportEmail(IEnumerable<TaskExecutionStatus> taskExecutionStatuses, ProcessorType processorType)
        {
            string body = string.Empty;

            using (StreamReader reader = new StreamReader(Path.Combine(emailNotificationConfig.EmailAlertTemplatesPath, Constants.ProcessorExecutionStatusReportEmailTemplateFileName)))
            {
                body = reader.ReadToEnd();
            }

            //inbound execution status data
            if (processorType == ProcessorType.Both || processorType == ProcessorType.Inbound)
            {
                StringBuilder inboundDataHtmlBuilder = new StringBuilder();
                foreach (var taskExecutionStatus in taskExecutionStatuses.Where(x => x.ProcessorType == ProcessorType.Inbound))
                {
                    inboundDataHtmlBuilder.Append(
                        string.Format(
                            Constants.ExecutionStatusReportEmailBodyTableFormat,
                            taskExecutionStatus.TaskName,
                            (taskExecutionStatus.IsSuccessful ? "Yes" : "No"),
                            taskExecutionStatus.AutomonReceivedRecordCount,
                            taskExecutionStatus.NexusAddRecordCount,
                            taskExecutionStatus.NexusUpdateRecordCount,
                            taskExecutionStatus.NexusDeleteRecordCount,
                            taskExecutionStatus.NexusFailureRecordCount
                        )
                    );
                }
                body = body.Replace(Constants.TemplateVariableInboundExecutionStatusDetails, inboundDataHtmlBuilder.ToString());
            }
            else
            {
                body = body.Replace(Constants.TemplateVariableInboundExecutionStatusDetails, Constants.NoInboundExecutionHtml);
            }

            //outbound execution status data
            if (processorType == ProcessorType.Both || processorType == ProcessorType.Outbound)
            {
                StringBuilder outboundDataHtmlBuilder = new StringBuilder();
                foreach (var taskExecutionStatus in taskExecutionStatuses.Where(x => x.ProcessorType == ProcessorType.Outbound))
                {
                    outboundDataHtmlBuilder.Append(
                        string.Format(
                            Constants.ExecutionStatusReportEmailBodyTableFormat,
                            taskExecutionStatus.TaskName,
                            (taskExecutionStatus.IsSuccessful ? "Yes" : "No"),
                            taskExecutionStatus.NexusReceivedMessageCount,
                            taskExecutionStatus.AutomonAddMessageCount,
                            taskExecutionStatus.AutomonUpdateMessageCount,
                            taskExecutionStatus.AutomonDeleteMessageCount,
                            taskExecutionStatus.AutomonFailureMessageCount
                        )
                    );
                }
                body = body.Replace(Constants.TemplateVariableOutboundExecutionStatusDetails, outboundDataHtmlBuilder.ToString());
            }
            else
            {
                body = body.Replace(Constants.TemplateVariableOutboundExecutionStatusDetails, Constants.NoOutboundExecutionHtml);
            }

            return body;
        }

        private string GetEmailBodyForCriticalErrorEmail()
        {
            string body = string.Empty;

            using (StreamReader reader = new StreamReader(Path.Combine(emailNotificationConfig.EmailAlertTemplatesPath, Constants.CriticalErrorEmailTemplateFileName)))
            {
                body = reader.ReadToEnd();
            }

            return body;
        }
        #endregion
    }
}
