using Common.Enums;
using Common.Enums.Reports;
using Common.Enums.Users;
using Common.Interfaces.Emails;
using Common.Interfaces.MessageSending.Mail;
using Common.Models;
using Common.Models.Configurations;
using Common.Models.Emails;
using Common.Models.Users;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using Common.Interfaces;

namespace Common.Handlers.SendingMessages.Mail
{
    public class UserPeriodicReportHandler : IEmailType
    {
        private IEmailProvider _emailProvider;
        private IShared _shared;
        public UserPeriodicReportHandler(IEmailProvider emailProvider, IShared shared)
        {
            _emailProvider = emailProvider;
            _shared = shared;
        }

        public async Task SendAsync(SmtpConfiguration config, CompanyConfiguration companyConfiguration, MessageInfo messageInfo)
        {
            if (messageInfo == null || !(messageInfo is UserReportMessageInfo))
            {
                return;
            }
            var reportMessageInfo = messageInfo as UserReportMessageInfo;
            var email = new Email() { };
            email.To = messageInfo?.Contact.Email;
            Resource resource = await _shared.InitEmail(email, messageInfo?.User, MessageType.UserPeriodicReport);
            var frequency = GetReportFrequency(resource, reportMessageInfo.Report.ReportFrequency);
            var datesInStringFormat = GetDatesRangeStringFormatted(reportMessageInfo.Report);
            resource.UserReportText = resource.UserReportText.Replace("{DD/MM/YY-DD/MM/YY}", datesInStringFormat);
            messageInfo.MessageContent = resource.UserReportText.Replace("{username}", messageInfo?.Contact.Name) +
                    "<br/>" + "<br/>" + resource.UserReportTableSubject.Replace("{frequency}", frequency) + "<br/>" + "<br/>" + messageInfo.MessageContent;
            messageInfo.MessageContent = ConvertMessageContentLanguage(messageInfo.MessageContent, resource);
            email.Subject = resource.UserReportSubject.Replace("{frequency}", frequency);
            email.HtmlBody.EmailText += $"<div style='direction:{email.HtmlBody.TextDirection.ToString()};font-size: smaller;'>{messageInfo?.MessageContent}</div><br/>";
            _shared.RemoveButton(email);
            await _emailProvider.Send(email, config);
        }

        private string ConvertMessageContentLanguage(string messageContent, Resource resource)
        {
            messageContent = messageContent.Replace("{Company}", resource.Company);
            messageContent = messageContent.Replace("{Group}", resource.Group);
            messageContent = messageContent.Replace("{Sent}", resource.Sent);
            messageContent = messageContent.Replace("{Signed}", resource.Signed);
            messageContent = messageContent.Replace("{Declined}", resource.Declined);
            messageContent = messageContent.Replace("{Canceled}", resource.Canceled);
            messageContent = messageContent.Replace("{Distribution}", resource.Distribution);
            messageContent = messageContent.Replace("{Total}", resource.Total);
            return messageContent;
        }

        private string GetDatesRangeStringFormatted(UserPeriodicReport report)
        {
            var now = DateTime.Now;
            string startDate = report.GetReportStartTime(now).ToString("dd/MM/yy");
            string endDate = now.ToString("dd/MM/yy");
            return $"{startDate}-{endDate}";
        }

        private string GetReportFrequency(Resource resource, ReportFrequency frequency)
        {
            switch (frequency)
            {
                case ReportFrequency.None:
                    return resource.NoneUserReport.ToString();
                case ReportFrequency.Daily:
                    return resource.DailyReport.ToString();
                case ReportFrequency.Weekly:
                    return resource.WeeklyReport.ToString();
                case ReportFrequency.Monthly:
                    return resource.MonthlyReport.ToString();
                default:
                    return resource.NoneUserReport.ToString();
            }
        }
    }
}
