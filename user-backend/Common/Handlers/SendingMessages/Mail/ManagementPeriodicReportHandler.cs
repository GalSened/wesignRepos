using Common.Enums;
using Common.Enums.Reports;
using Common.Interfaces.Emails;
using Common.Interfaces.MessageSending.Mail;
using Common.Models;
using Common.Models.Configurations;
using Common.Models.Emails;
using Common.Models.ManagementApp;
using Common.Models.ManagementApp.Reports;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Common.Handlers.SendingMessages.Mail
{
    public class ManagementPeriodicReportHandler : IEmailType
    {
        private IEmailProvider _emailProvider;
        private IShared _shared;
        public ManagementPeriodicReportHandler(IEmailProvider emailProvider, IShared shared)
        {
            _emailProvider = emailProvider;
            _shared = shared;
        }
        public async Task SendAsync(SmtpConfiguration config, CompanyConfiguration companyConfiguration, MessageInfo messageInfo)
        {
            if (messageInfo == null || !(messageInfo is ManagementReportMessageInfo))
            {
                return;
            }
            var reportMessageInfo = messageInfo as ManagementReportMessageInfo;

            foreach (var emailToSend in reportMessageInfo.Report.Emails)
            {
                await SendSingleMail(config, companyConfiguration, reportMessageInfo, emailToSend);
            }
        }

        private async Task SendSingleMail(SmtpConfiguration config, CompanyConfiguration companyConfiguration, ManagementReportMessageInfo messageInfo, ManagementPeriodicReportEmail reportEmail)
        {
            var email = new Email() { };
            email.To = reportEmail.Email;
            email.HtmlBody.ClientName = reportEmail.Email;
            Resource resource = await _shared.InitEmail(email, messageInfo?.User, MessageType.ManagementPeriodicReport);
            var frequency = GetReportFrequency(resource, messageInfo.Report.ReportFrequency);
            var reportTypeTitle = GetReportTypeTitle(messageInfo.Report.ReportType);
            var reportType = GetReportType(resource, messageInfo.Report.ReportType);
            resource.ManagementReportText = resource.ManagementReportText.Replace("{frequency}", frequency);
            messageInfo.MessageContent = resource.ManagementReportText.Replace("{reportType}", reportType);
            email.Subject = $"{reportTypeTitle} Report - {frequency}";
            email.HtmlBody.Link = messageInfo.Link;
            email.HtmlBody.LinkText = resource.DownloadReportLinkText;
            email.HtmlBody.EmailText = $"<div style='direction:{email.HtmlBody.TextDirection.ToString()};'>{messageInfo?.MessageContent}</div>";
            await _emailProvider.Send(email, config);
        }

        private string GetReportFrequency(Resource resource, ManagementReportFrequency frequency)
        {
            switch (frequency)
            {
                case ManagementReportFrequency.Weekly:
                    return resource.WeeklyReport.ToString();
                case ManagementReportFrequency.Monthly:
                    return resource.MonthlyReport.ToString();
                case ManagementReportFrequency.Yearly:
                    return resource.YearlyReport.ToString();
                default:
                    return resource.WeeklyReport.ToString();
            }
        }

        private string GetReportTypeTitle(ManagementReportType type)
        {
            var enumString = type.ToString();
            string formattedString = Regex.Replace(enumString, @"([a-z0-9])([A-Z])", "$1 $2");
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(formattedString.ToLower());
        }

        private string GetReportType(Resource resource, ManagementReportType type)
        {
            switch (type)
            {
                case ManagementReportType.ExpirationUtilization:
                    return resource.ExpirationUtilizationReport.ToString();
                case ManagementReportType.ProgramUtilization:
                    return resource.ProgramUtilizationReport.ToString();
                case ManagementReportType.UsePercentageUtilization:
                    return resource.UsePercentageUtilizationReport.ToString();
                case ManagementReportType.AllCompaniesUtilization:
                    return resource.AllCompaniesUtilizationReport.ToString();
                case ManagementReportType.GroupUtilization:
                    return resource.GroupUtilizationReport.ToString();
                case ManagementReportType.ProgramByUtilization:
                    return resource.ProgramUtilizationReport.ToString();
                case ManagementReportType.ProgramsByUsage:
                    return resource.ProgramsByUsageReport.ToString();
                case ManagementReportType.GroupDocumentStatuses:
                    return resource.GroupDocumentStatusesReport.ToString();
                case ManagementReportType.DocsByUsers:
                    return resource.DocsByUsersReport.ToString();
                case ManagementReportType.DocsBySigners:
                    return resource.DocsBySignersReport.ToString();
                case ManagementReportType.CompanyUsers:
                    return resource.CompanyUsersReport.ToString();
                case ManagementReportType.FreeTrialUsers:
                    return resource.FreeTrialUsersReport.ToString();
                case ManagementReportType.UsageByUsers:
                    return resource.UsageByUsersReport.ToString();
                case ManagementReportType.UsageByCompanies:
                    return resource.UsageByCompaniesReport.ToString();
                case ManagementReportType.TemplatesByUsage:
                    return resource.TemplatesByUsageReport.ToString();
                case ManagementReportType.UsageBySignatureType:
                    return resource.UsageBySignatureTypeReport.ToString();
                default:
                    return "Unknown type";
            }
        }
    }
}
