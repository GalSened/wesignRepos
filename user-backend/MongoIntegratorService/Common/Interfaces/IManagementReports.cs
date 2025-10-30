using HistoryIntegratorService.Common.Models.ManagementReports;
using HistoryIntegratorService.Requests;

namespace HistoryIntegratorService.Common.Interfaces
{
    public interface IManagementReports
    {
        IEnumerable<UsageByUserReport> ReadUsageByUserDetails(string appKey, UsageByUserDetailsRequest usageByUserDetailsRequest);
        IEnumerable<UsageByCompanyReport> ReadUsageByCompanyAndGroups(string appKey, UsageByCompanyAndGroupsRequest usageByCompanyAndGroupsRequest);
        UsageBySignatureTypeReport ReadUsageByCompanyAndSignatureTypes(string appKey, UsageByCompanyAndSignatureTypesRequest usageByCompanyAndSignatureTypesRequest);
    }
}
