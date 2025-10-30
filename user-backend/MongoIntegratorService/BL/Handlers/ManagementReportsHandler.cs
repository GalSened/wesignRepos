using HistoryIntegratorService.Common.Enums;
using HistoryIntegratorService.Common.Extensions;
using HistoryIntegratorService.Common.Interfaces;
using HistoryIntegratorService.Common.Interfaces.Connectors;
using HistoryIntegratorService.Common.Models;
using HistoryIntegratorService.Common.Models.ManagementReports;
using HistoryIntegratorService.Requests;
using Microsoft.Extensions.Options;

namespace HistoryIntegratorService.BL.Handlers
{
    public class ManagementReportsHandler : IManagementReports
    {
        private readonly GeneralSettings _generalSettings;
        private IManagementDocumentCollectionConnector _documentCollectionConnector;
        private readonly IEncryptor _encryptor;

        public ManagementReportsHandler(IOptions<GeneralSettings> options, IManagementDocumentCollectionConnector documentCollectionConnector, IEncryptor encryptor)
        {
            _generalSettings = options.Value;
            _documentCollectionConnector = documentCollectionConnector;
            _encryptor = encryptor;
        }

        public IEnumerable<UsageByUserReport> ReadUsageByUserDetails(string appKey, UsageByUserDetailsRequest usageByUserDetailsRequest)
        {
            if (string.IsNullOrEmpty(appKey) || _encryptor.Decrypt(_generalSettings.AppKey) != appKey)
            {
                throw new InvalidOperationException(ResultCode.InvalidCredential.GetNumericString());
            }
            return _documentCollectionConnector.ReadUsageByUserDetails(usageByUserDetailsRequest);
        }

        public IEnumerable<UsageByCompanyReport> ReadUsageByCompanyAndGroups(string appKey, UsageByCompanyAndGroupsRequest usageByCompanyAndGroupsRequest)
        {
            if (string.IsNullOrEmpty(appKey) || _encryptor.Decrypt(_generalSettings.AppKey) != appKey)
            {
                throw new InvalidOperationException(ResultCode.InvalidCredential.GetNumericString());
            }
            return _documentCollectionConnector.ReadUsageByCompanyAndGroups(usageByCompanyAndGroupsRequest);
        }

        public UsageBySignatureTypeReport ReadUsageByCompanyAndSignatureTypes(string appKey, UsageByCompanyAndSignatureTypesRequest usageByCompanyAndSignatureTypesRequest)
        {
            if (string.IsNullOrEmpty(appKey) || _encryptor.Decrypt(_generalSettings.AppKey) != appKey)
            {
                throw new InvalidOperationException(ResultCode.InvalidCredential.GetNumericString());
            }
            return _documentCollectionConnector.ReadUsageByCompanyAndSignatureTypes(usageByCompanyAndSignatureTypesRequest);
        }
    }
}
