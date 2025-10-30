using Common.Enums.Documents;
using Common.Models.Reports;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Common.Interfaces.Reports
{
    public interface IWeSignHistoryReports
    {
        Task<IEnumerable<UsageDataReport>> ReadUsageDataReports(Guid userId, DateTime from, DateTime to, IEnumerable<DocumentStatus> documentStatuses, IEnumerable<Guid> groupIds, bool includeDistributionDocs);
    }
}
