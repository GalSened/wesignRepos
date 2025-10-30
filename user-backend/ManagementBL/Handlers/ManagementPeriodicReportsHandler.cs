using Common.Enums;
using Common.Enums.Reports;
using Common.Enums.Results;
using Common.Extensions;
using Common.Interfaces;
using Common.Interfaces.DB;
using Common.Interfaces.ManagementApp;
using Common.Interfaces.MessageSending;
using Common.Models;
using Common.Models.Configurations;
using Common.Models.ManagementApp;
using Common.Models.ManagementApp.Reports;
using Common.Models.Reports;
using Common.Models.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementBL.Handlers
{
    public class ManagementPeriodicReportsHandler : IManagementPeriodicReports
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IReport _reports;
        private readonly IMessageSender _sender;
        private readonly ILogger _logger;
        private readonly IDater _dater;
        private readonly FolderSettings _folderSettings;
        private readonly GeneralSettings _generalSettings;

        public ManagementPeriodicReportsHandler(IServiceScopeFactory serviceScopeFactory, IReport reports, IMessageSender sender, ILogger logger, IDater dater, IOptions<FolderSettings> folderOptions, IOptions<GeneralSettings> generalOptions)
        {
            _scopeFactory = serviceScopeFactory;
            _reports = reports;
            _sender = sender;
            _logger = logger;
            _dater = dater;
            _folderSettings = folderOptions.Value;
            _generalSettings = generalOptions.Value;
            InitBaseReportsFolder();
        }

        public async Task SendManagementReportToUsers(Configuration configuration, ManagementPeriodicReport report, IEnumerable<ManagementPeriodicReportEmail> reportEmails)
        {
            using var scope = _scopeFactory.CreateScope();
            IUserConnector userConnector = scope.ServiceProvider.GetService<IUserConnector>();
            ICompanyConnector companiesConnector = scope.ServiceProvider.GetService<ICompanyConnector>();
            IPeriodicReportFileConnector periodicReportFileConnector = scope.ServiceProvider.GetService<IPeriodicReportFileConnector>();
            IManagementPeriodicReportEmailConnector managementPeriodicReportEmailConnector = scope.ServiceProvider.GetService<IManagementPeriodicReportEmailConnector>();

            var user = await userConnector.Read(new User() { Id = report.UserId });
            if (user == null)
            {
                throw new InvalidOperationException(ResultCode.UserNotExist.GetNumericString());
            }

            var company = await companiesConnector.Read(new Company() { Id = user.CompanyId });
            if (company == null)
            {
                throw new InvalidOperationException(ResultCode.CompanyNotExist.GetNumericString());
            }

            var reportParams = JsonConvert.DeserializeObject<ReportParameters>(report.ReportParameters);
            if (reportParams == null)
            {
                throw new InvalidOperationException(ResultCode.FailedToDeserializeReportParameters.GetNumericString());
            }

            if (reportParams.GroupIds.Count == 0)
            {
                reportParams.GroupIds = null;
            }

            reportParams.Limit = int.MaxValue;

            var reports = await GetReports(report, reportParams);
            if (reports != null && reports.Any())
            {
                var fileBytes = _reports.ReportToCsv(reports);
                var periodicReportFile = new PeriodicReportFile()
                {
                    Token = "",
                    CreationTime = _dater.UtcNow()
                };
                var periodicReportFileId = periodicReportFileConnector.Create(periodicReportFile);
                string path = $"{_folderSettings.PeriodicReports}/{periodicReportFileId}/{periodicReportFileId}.csv";
                string filePath = Path.GetFullPath(path);
                string directory = Path.GetDirectoryName(path);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                string content = Encoding.UTF8.GetString(fileBytes);
                File.WriteAllText(filePath, content, Encoding.UTF8);

                MessageInfo messageInfo = new ManagementReportMessageInfo()
                {
                    MessageType = MessageType.ManagementPeriodicReport,
                    User = user,
                    Report = report,
                    ReportsToCSV = reports,
                    Link = $"{_generalSettings.UserBackendEndPoint}/reports/FrequencyReports/Download?id={periodicReportFileId}"
                };
                await _sender.Send(configuration, company?.CompanyConfiguration, messageInfo);
                string emails = string.Join(", ", reportEmails.Select(_ => _.Email));
                _logger.Debug("Successfully sent [{ReportType}]Reports to [{Emails}]", report.ReportType.ToString(), emails);
            }
        }

        private async Task<IEnumerable<object>> GetReports(ManagementPeriodicReport report, ReportParameters reportParams)
        {
            var now = _dater.UtcNow();
            IEnumerable<object> reports = null;
            Guid? programId = reportParams.ProgramId != Guid.Empty ? reportParams.ProgramId : null;
            switch (report.ReportType)
            {
                case ManagementReportType.ExpirationUtilization:
                    (reports, int _) = await _reports.GetUtilizationReports(reportParams.IsProgramUtilizationExpired, reportParams.MonthsForAvgUse,
                programId, now.AddYears(-1), now, reportParams.Offset, reportParams.Limit);
                    break;
                case ManagementReportType.ProgramUtilization:
                    (reports, int _) = await _reports.GetUtilizationReportsByProgram(reportParams.ProgramId, reportParams.MonthsForAvgUse,
                reportParams.MinDocs, reportParams.MinSms, reportParams.Offset, reportParams.Limit);
                    break;
                case ManagementReportType.CompanyUsers:
                    (reports, int _) = await _reports.GetUsersByCompany(reportParams.CompanyId, reportParams.Offset, reportParams.Limit);
                    break;
                case ManagementReportType.FreeTrialUsers:
                    reports = _reports.GetFreeTrialUsers(reportParams.Offset, reportParams.Limit, out _);
                    break;
                case ManagementReportType.UsageByUsers:
                    (reports, int _) = await _reports.GetUsageByUsers(reportParams.UserEmail, reportParams.CompanyId, reportParams.GroupIds, now.AddYears(-1), now,
                reportParams.Offset, reportParams.Limit);
                    break;
                case ManagementReportType.UsageByCompanies:
                    (reports, int _) = await _reports.GetUsageByCompanies(reportParams.CompanyId, reportParams.GroupIds, now.AddYears(-1), now,
                reportParams.Offset, reportParams.Limit);
                    break;
                case ManagementReportType.TemplatesByUsage:
                    (reports, int _) = await _reports.GetTemplatesByUsage(reportParams.CompanyId, reportParams.GroupIds, now.AddYears(-1), now,
                reportParams.Offset, reportParams.Limit);
                    break;
                case ManagementReportType.UsageBySignatureType:
                    reports = new List<object>() { await _reports.GetUsageBySignatureType(reportParams.CompanyId, null, now.AddYears(-1), now) };
                    break;
                case ManagementReportType.UsePercentageUtilization:
                    reports = _reports.GetUtilizationReportsByPercentage(reportParams.DocsUsagePercentage, reportParams.MonthsForAvgUse,
                reportParams.Offset, reportParams.Limit, out _);
                    break;
                case ManagementReportType.AllCompaniesUtilization:
                    reports = _reports.GetAllCompaniesUtilizations(reportParams.MonthsForAvgUse, reportParams.Offset, reportParams.Limit, out _);
                    break;
                case ManagementReportType.GroupUtilization:
                    (reports, int _) = await _reports.GetUtilizationReportPerGroup(reportParams.CompanyId, reportParams.DocsUsagePercentage, reportParams.MonthsForAvgUse,
                reportParams.Offset, reportParams.Limit);
                    break;
                case ManagementReportType.ProgramByUtilization:
                    reports = _reports.GetProgramsReport(reportParams.MinDocs, reportParams.MinSms, null, reportParams.Offset, reportParams.Limit, out _);
                    break;
                case ManagementReportType.ProgramsByUsage:
                    reports = _reports.GetProgramsReport(0, 0, reportParams.IsProgramUsed, reportParams.Offset, reportParams.Limit, out _);
                    break;
                case ManagementReportType.GroupDocumentStatuses:
                    (reports, int _) = await _reports.GetGroupDocumentReports(reportParams.CompanyId, reportParams.Offset, reportParams.Limit);
                    break;
                case ManagementReportType.DocsByUsers:
                    (reports, int _) = await _reports.GetUserDocumentReports(reportParams.CompanyId, reportParams.GroupIds, true, now.AddYears(-1), now,
                reportParams.Offset, reportParams.Limit);
                    break;
                case ManagementReportType.DocsBySigners:
                    (reports, int _) = await _reports.GetUserDocumentReports(reportParams.CompanyId, reportParams.GroupIds, false, now.AddYears(-1), now,
                reportParams.Offset, reportParams.Limit);
                    break;
                default:
                    break;
            }
            return reports;
        }

        private void InitBaseReportsFolder()
        {
            try
            {
                if (!Directory.Exists(_folderSettings.PeriodicReports))
                {
                    Directory.CreateDirectory(_folderSettings.PeriodicReports);
                }
            }
            catch (Exception)
            {
                _logger.Error("Failed to create periodic reports folder");
            }

        }
    }
}
