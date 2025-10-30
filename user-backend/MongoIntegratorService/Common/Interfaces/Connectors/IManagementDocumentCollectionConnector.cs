using HistoryIntegratorService.Common.Models.ManagementReports;
using HistoryIntegratorService.Requests;

namespace HistoryIntegratorService.Common.Interfaces.Connectors
{
    public interface IManagementDocumentCollectionConnector : IDocumentCollectionConnector
    {
        IEnumerable<UsageByUserReport> ReadUsageByUserDetails(UsageByUserDetailsRequest usageByUserDetailsRequest);
        IEnumerable<UsageByCompanyReport> ReadUsageByCompanyAndGroups(UsageByCompanyAndGroupsRequest usageByCompanyAndGroupsRequest);
        UsageBySignatureTypeReport ReadUsageByCompanyAndSignatureTypes(UsageByCompanyAndSignatureTypesRequest usageByCompanyAndSignatureTypesRequest);
    }
}
