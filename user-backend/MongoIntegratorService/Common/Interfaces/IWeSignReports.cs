using HistoryIntegratorService.Common.Models.UserReports;
using HistoryIntegratorService.Requests;

namespace HistoryIntegratorService.Common.Interfaces
{
    public interface IWeSignReports
    {
        IEnumerable<UsageDataReport> ReadUserUsageDataReports(string appKey, UserUsageDataRequest userUsageDataRequest);
    }
}
