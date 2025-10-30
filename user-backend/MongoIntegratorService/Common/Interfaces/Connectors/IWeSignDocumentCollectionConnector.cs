using HistoryIntegratorService.Common.Models.UserReports;
using HistoryIntegratorService.Requests;

namespace HistoryIntegratorService.Common.Interfaces.Connectors
{
    public interface IWeSignDocumentCollectionConnector : IDocumentCollectionConnector
    {
        IEnumerable<UsageDataReport> ReadUserUsageDataReports(UserUsageDataRequest userUsageDataRequest);
    }
}
