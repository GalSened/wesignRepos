using Common.Enums;
using Common.Enums.Reports;
using Common.Enums.Results;
using Common.Extensions;
using Common.Interfaces;
using Common.Interfaces.DB;
using Common.Interfaces.ManagementApp;
using Common.Interfaces.MessageSending;
using Common.Interfaces.Reports;
using Common.Models;
using Common.Models.Configurations;
using Common.Models.Reports;
using Common.Models.Users;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ManagementBL.Handlers
{
    public class UserPeriodicReportsHandler : IUserPeriodicReports
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IUserReports _reports;
        private readonly IMessageSender _sender;
        private readonly ILogger _logger;
        private readonly ITableFormatter _tableFormatter;
        private readonly IDater _dater;
        private readonly Common.Interfaces.ManagementApp.IUsers _users;

        public UserPeriodicReportsHandler(IServiceScopeFactory serviceScopeFactory, IUserReports reports, IMessageSender messageSender,
            ILogger logger, ITableFormatter tableFormatter, IDater dater, Common.Interfaces.ManagementApp.IUsers users)
        {
            _scopeFactory = serviceScopeFactory;
            _reports = reports;
            _sender = messageSender;
            _logger = logger;
            _tableFormatter = tableFormatter;
            _dater = dater;
            _users = users;
        }

        public async Task SendReportToUser(Configuration configuration, UserPeriodicReport report)
        {
            using var scope = _scopeFactory.CreateScope();
            IUserConnector userConnector = scope.ServiceProvider.GetService<IUserConnector>();
            ICompanyConnector companiesConnector = scope.ServiceProvider.GetService<ICompanyConnector>();
            var user = await userConnector.Read(new User() { Id = report.UserId });
            if (user == null)
            {
                _logger.Error("User is not exists");
                throw new InvalidOperationException(ResultCode.UserNotExist.GetNumericString());
            }

            var company = await companiesConnector.Read(new Company() { Id = user.CompanyId });
            if (company == null)
            {
                _logger.Error("Company is not exists");
                throw new InvalidOperationException(ResultCode.CompanyNotExist.GetNumericString());
            }

            switch (report.ReportType)
            {
                case ReportType.UsageData:
                    await HandleUsageDataPeriodicReports(configuration, report, user, company);
                    break;
                default:
                    break;
            }
        }

        private async Task HandleUsageDataPeriodicReports(Configuration configuration, UserPeriodicReport report, User user, Company company)
        {
            List<string> headers = new List<string>() { "{Company}-{Group}", "{Sent}", "{Signed}", "{Declined}", "{Canceled}", "{Distribution}", "{Total}" };
            var colorizedHeaders = new Dictionary<string, string>() { { "Signed", "green" }, { "Declined", "red" }, { "Canceled", "red" } };
            var now = _dater.UtcNow();
            // TODO: Check about this limit
            _logger.Information($"Trying to fetch reports from {report.GetReportStartTime(now)} to  {now}");
            var reports = (await _reports.ReadUsageData(user, report.GetReportStartTime(now), now, null, null, true, 0, 100)).ToList();
            if (reports == null || !reports.Any())
            {
                _logger.Warning("No reports found");
                reports = await FillEmptyReports(user);
            }
            var rows = CastUsageDataReportsToTableRows(reports, company.Name);
            var contact = new Contact()
            {
                Name = user.Name, Email = user.Email,
            };
            var messageInfo = new UserReportMessageInfo()
            {
                MessageType = MessageType.UserPeriodicReport,
                User = user,
                MessageContent = _tableFormatter.CreateHtmlTableSyntax(headers, rows, colorizedHeaders),
                Contact = contact,
                Report = report
            };
            await _sender.Send(configuration, company?.CompanyConfiguration, messageInfo);
            _logger.Debug("Successfully send UsageDataPeriodicReports to user [{UserEmail}]", user.Email);
        }

        private List<List<string>> CastUsageDataReportsToTableRows(IEnumerable<UsageDataReport> reports, string companyName)
        {
            var response = new List<List<string>>();
            foreach (var report in reports)
            {
                var total = (report.PendingDocumentsCount >= 0 ? report.PendingDocumentsCount : 0)
                    + (report.SignedDocumentsCount >= 0 ? report.SignedDocumentsCount : 0)
                    + (report.DeclinedDocumentsCount >= 0 ? report.DeclinedDocumentsCount : 0)
                    + (report.CanceledDocumentsCount >= 0 ? report.CanceledDocumentsCount : 0)
                    + (report.DistributionDocumentsCount >= 0 ? report.DistributionDocumentsCount : 0);
                var row = new List<string>
                {
                    $"{companyName}-{report.GroupName}",
                    report.PendingDocumentsCount.ToString(),
                    report.SignedDocumentsCount.ToString(),
                    report.DeclinedDocumentsCount.ToString(),
                    report.CanceledDocumentsCount.ToString(),
                    report.DistributionDocumentsCount.ToString(),
                    total.ToString()
                };
                response.Add(row);
            }
            return response;
        }

        private async Task<List<UsageDataReport>> FillEmptyReports(User user)
        {
            var reports = new List<UsageDataReport>();
            var groups = await _users.GetUserGroups(user);
            foreach (var group in groups)
            {
                reports.Add(new UsageDataReport()
                {
                    GroupId = group.Id,
                    GroupName = group.Name,
                    PendingDocumentsCount = 0,
                    SignedDocumentsCount = 0,
                    CanceledDocumentsCount = 0,
                    DeclinedDocumentsCount = 0,
                    DistributionDocumentsCount = 0
                });
            }
            return reports;
        }
    }
}
