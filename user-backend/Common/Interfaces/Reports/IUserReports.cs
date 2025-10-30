using Common.Enums.Documents;
using Common.Enums.Reports;
using Common.Models;
using Common.Models.Reports;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Common.Interfaces.Reports
{
    public interface IUserReports
    {
        Task<IEnumerable<UsageDataReport>> ReadUsageData(User user, DateTime? from, DateTime? to, List<DocumentStatus> documentStatuses,
            List<Guid> groupIds, bool includeDistributionDocs, int offset, int limit);
        byte[] ReportToCsv(IEnumerable<object> result);
    }
}
