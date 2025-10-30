using Common.Enums.Documents;
using Common.Enums.Reports;
using Common.Enums.Results;
using Common.Enums.Users;
using Common.Extensions;
using Common.Handlers;
using Common.Interfaces;
using Common.Interfaces.DB;
using Common.Interfaces.Reports;
using Common.Models;
using Common.Models.Reports;
using Common.Models.Settings;
using Common.Models.Users;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BL.Handlers
{
    public class ReportsHandler : IReports
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IDater _dater;
        private readonly IUsers _users;
        private readonly IWeSignHistoryReports _historyReports;
        private readonly GeneralSettings _generalSettings;
        private readonly FolderSettings _folderSettings;

        public ReportsHandler(IServiceScopeFactory scopeFactory,
            IDater dater, IUsers users, IWeSignHistoryReports reportsHttpClientWrapper, IOptions<GeneralSettings> generalSettings, IOptions<FolderSettings> folderSettings)
        {
            _scopeFactory = scopeFactory;
            _dater = dater;
            _users = users;
            _historyReports = reportsHttpClientWrapper;
            _generalSettings = generalSettings.Value;
            _folderSettings = folderSettings.Value;
        }

        public async Task<IEnumerable<UsageDataReport>> ReadUsageData(DateTime? from, DateTime? to, List<DocumentStatus> documentStatuses, List<Guid> groupIds, bool includeDistributionDocs, int offset, int limit)
        {
            (User user, _) = await _users.GetUser();
            if (user == null)
            {
                throw new InvalidOperationException(ResultCode.UserNotExist.GetNumericString());
            }
            if (!from.HasValue)
            {
                from = _dater.UtcNow().AddYears(-1);
            }
            if (!to.HasValue)
            {
                to = _dater.UtcNow();
            }
            if (offset < 0)
            {
                throw new InvalidOperationException(ResultCode.InvalidPositiveOffsetNumber.GetNumericString());
            }
            if (limit < 0)
            {
                throw new InvalidOperationException(ResultCode.InvalidLimitNumber.GetNumericString());
            }
            var userGroups = await _users.GetUserGroups();
            var userGroupsIds = userGroups.Select(_ => _.Id);
            if (groupIds != null && groupIds.Any(groupId => !userGroupsIds.Contains(groupId)))
            {
                throw new InvalidOperationException(ResultCode.GroupNotBelongToUser.GetNumericString());
            }
            var deletedDocsReports = await _historyReports.ReadUsageDataReports(user.Id, from.Value, to.Value, documentStatuses, groupIds, includeDistributionDocs);
            using var scope = _scopeFactory.CreateScope();
            IDocumentCollectionConnector documentCollectionConnector = scope.ServiceProvider.GetService<IDocumentCollectionConnector>();
            IGroupConnector groupConnector = scope.ServiceProvider.GetService<IGroupConnector>();
            var activeReports = documentCollectionConnector.ReadUserUsageDataReports(user.Id, from.Value, to.Value, documentStatuses, groupIds, includeDistributionDocs);
            var totalReports = MergeReports(activeReports, deletedDocsReports);
            totalReports = totalReports.Skip(offset).Take(limit).ToList();
            var reportsGroupIds = totalReports.Select(_ => _.GroupId).ToList();
            var groupIdNameDictionary = groupConnector.GetGroupIdNameDictionary(reportsGroupIds);
            if (groupIdNameDictionary == null)
            {
                return Enumerable.Empty<UsageDataReport>();
            }
            foreach (var report in totalReports)
            {
                if (groupIdNameDictionary.ContainsKey(report.GroupId))
                {
                    report.GroupName = groupIdNameDictionary[report.GroupId];
                }
            }
            return totalReports;
        }

        public async Task CreateFrequencyReport(ReportFrequency reportFrequency, ReportType reportType)
        {
            (var user, _) = await _users.GetUser();
            if (user == null)
            {
                throw new InvalidOperationException(ResultCode.UserNotExist.GetNumericString());
            }

            var userPeriodicReport = new UserPeriodicReport()
            {
                UserId = user.Id,
                ReportType = reportType,
                LastTimeSent = _dater.MinValue(),
                ReportFrequency = reportFrequency,
                User = user
            };
            using var scope = _scopeFactory.CreateScope();
            IUserPeriodicReportConnector userPeriodicReportConnector = scope.ServiceProvider.GetService<IUserPeriodicReportConnector>();
            await userPeriodicReportConnector.Create(userPeriodicReport);
        }

        public async Task<IEnumerable<UserPeriodicReport>> ReadFrequencyReports()
        {
            (var user, _) = await _users.GetUser();
            if (user == null)
            {
                throw new InvalidOperationException(ResultCode.UserNotExist.GetNumericString());
            }

            using var scope = _scopeFactory.CreateScope();
            IUserPeriodicReportConnector userPeriodicReportConnector = scope.ServiceProvider.GetService<IUserPeriodicReportConnector>();
            var reports = userPeriodicReportConnector.ReadByUser(user.Id);
            return reports;
        }

        public async Task DeleteAllFrequencyReports()
        {
            (var user, _) = await _users.GetUser();
            if (user == null)
            {
                throw new InvalidOperationException(ResultCode.UserNotExist.GetNumericString());
            }

            using var scope = _scopeFactory.CreateScope();
            IUserPeriodicReportConnector userPeriodicReportConnector = scope.ServiceProvider.GetService<IUserPeriodicReportConnector>();
            await userPeriodicReportConnector.DeleteAllByUserId(user.Id);
        }

        public byte[] DownloadFrequencyReportsFile(Guid id)
        {
            using var scope = _scopeFactory.CreateScope();
            IPeriodicReportFileConnector periodicReportFileConnector = scope.ServiceProvider.GetService<IPeriodicReportFileConnector>();
            var reportFile = periodicReportFileConnector.Read(id);
            string path = $"{_folderSettings.PeriodicReports}/{id}/{id}.csv";
            string filePath = Path.GetFullPath(path);
            if (reportFile != null && reportFile.CreationTime.AddHours(_generalSettings.PeriodicReportFileExpirationInHours) > _dater.UtcNow())
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
                periodicReportFileConnector.Delete(id);
                throw new InvalidOperationException(ResultCode.PeriodicReportFileIsExpired.GetNumericString());
            }
            if (File.Exists(filePath))
            {
                byte[] fileBytes = File.ReadAllBytes(filePath);
                return fileBytes;
            }
            throw new InvalidOperationException(ResultCode.PeriodicReportFileIsNotExists.GetNumericString());
        }

        public async Task<byte[]> ReportToCsv(IEnumerable<object> reportList)
        {
            (var user, _) = await _users.GetUser();
            return CsvHandler.ExportDocumentsCollection(reportList, user.UserConfiguration.Language);
        }

        private IEnumerable<UsageDataReport> MergeReports(IEnumerable<UsageDataReport> reports, IEnumerable<UsageDataReport> additionalReports)
        {
            var allReports = reports.Concat(additionalReports);
            var mergedReports = allReports
                .GroupBy(r => r.GroupId)
                .Select(g => new UsageDataReport
                {
                    GroupId = g.Key,
                    GroupName = g.First().GroupName,
                    PendingDocumentsCount = g.Sum(r => r.PendingDocumentsCount),
                    SignedDocumentsCount = g.Sum(r => r.SignedDocumentsCount),
                    DeclinedDocumentsCount = g.Sum(r => r.DeclinedDocumentsCount),
                    CanceledDocumentsCount = g.Sum(r => r.CanceledDocumentsCount),
                    DistributionDocumentsCount = g.Sum(r => r.DistributionDocumentsCount)
                });

            return mergedReports;
        }

        
    }
}
