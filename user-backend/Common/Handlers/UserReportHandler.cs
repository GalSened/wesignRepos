using Common.Enums.Documents;
using Common.Enums.Reports;
using Common.Interfaces.DB;
using Common.Interfaces;
using Common.Interfaces.Reports;
using Common.Models;
using Common.Models.Reports;
using Common.Models.Users;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Enums.Results;
using Common.Extensions;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace Common.Handlers
{
    public class UserReportHandler : IUserReports
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IDater _dater;
        private readonly Interfaces.ManagementApp.IUsers _users;
        private readonly IWeSignHistoryReports _historyReports;

        public UserReportHandler(IServiceScopeFactory scopeFactory, IDater dater, Interfaces.ManagementApp.IUsers users, IWeSignHistoryReports historyReports)
        {
            _scopeFactory = scopeFactory;
            _dater = dater;
            _users = users;
            _historyReports = historyReports;
        }

        public async Task<IEnumerable<UsageDataReport>> ReadUsageData(User user, DateTime? from, DateTime? to, List<DocumentStatus> documentStatuses, List<Guid> groupIds, bool includeDistributionDocs, int offset, int limit)
        {
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
            var userGroups = await _users.GetUserGroups(user);
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

        public byte[] ReportToCsv(IEnumerable<object> result)
        {
            throw new NotImplementedException();
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
