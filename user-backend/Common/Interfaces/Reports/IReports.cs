using Common.Enums.Documents;
using Common.Enums.Reports;
using Common.Models.Reports;
using Common.Models.Users;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Common.Interfaces.Reports
{
    public interface IReports
    {
        Task CreateFrequencyReport(ReportFrequency reportFrequency, ReportType reportType);
        Task<IEnumerable<UserPeriodicReport>> ReadFrequencyReports();
        Task DeleteAllFrequencyReports();
        Task<IEnumerable<UsageDataReport>> ReadUsageData(DateTime? from, DateTime? to, List<DocumentStatus> documentStatuses,
            List<Guid> groupIds, bool includeDistributionDocs, int offset, int limit);
        byte[] DownloadFrequencyReportsFile(Guid id);
        Task<byte[]> ReportToCsv(IEnumerable<object> result);
    }
}
