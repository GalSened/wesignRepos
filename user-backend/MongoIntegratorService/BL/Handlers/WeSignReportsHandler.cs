using HistoryIntegratorService.Common.Enums;
using HistoryIntegratorService.Common.Extensions;
using HistoryIntegratorService.Common.Interfaces;
using HistoryIntegratorService.Common.Interfaces.Connectors;
using HistoryIntegratorService.Common.Models;
using HistoryIntegratorService.Common.Models.UserReports;
using HistoryIntegratorService.Requests;
using Microsoft.Extensions.Options;

namespace HistoryIntegratorService.BL.Handlers
{
    public class WeSignReportsHandler : IWeSignReports
    {
        private readonly GeneralSettings _generalSettings;
        private IWeSignDocumentCollectionConnector _documentCollectionConnector;
        private readonly IEncryptor _encryptor;

        public WeSignReportsHandler(IOptions<GeneralSettings> options, IWeSignDocumentCollectionConnector documentCollectionConnector, IEncryptor encryptor)
        {
            _generalSettings = options.Value;
            _documentCollectionConnector = documentCollectionConnector;
            _encryptor = encryptor;
        }

        public IEnumerable<UsageDataReport> ReadUserUsageDataReports(string appKey, UserUsageDataRequest userUsageDataRequest)
        {
            if (string.IsNullOrEmpty(appKey) || _encryptor.Decrypt(_generalSettings.AppKey) != appKey)
            {
                throw new InvalidOperationException(ResultCode.InvalidCredential.GetNumericString());
            }
            if (userUsageDataRequest.DocumentStatuses != null && userUsageDataRequest.DocumentStatuses.Contains(DocumentStatus.Sent))
            {  
                userUsageDataRequest.DocumentStatuses.Add(DocumentStatus.Viewed);
            }

            return _documentCollectionConnector.ReadUserUsageDataReports(userUsageDataRequest);
        }
    }
}
