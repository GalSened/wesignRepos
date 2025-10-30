using Common.Enums.Results;
using Common.Extensions;
using Common.Interfaces;
using Common.Interfaces.Reports;
using Common.Models;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Twilio.Http;

namespace ManagementBL.Handlers
{
    public class HistoryDocumentCollectionHandler : IHistoryDocumentCollection
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;
        private readonly IEncryptor _encryptor;
        private readonly string _historyDocumentsRoute;

        public HistoryDocumentCollectionHandler(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger logger, IEncryptor encryptor)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
            _encryptor = encryptor;
            _historyDocumentsRoute = "/documentcollections";
        }

        public async Task<IEnumerable<DocumentCollection>> ReadOldDocuments(DateTime from, DateTime to, Guid? userId, IEnumerable<Guid> groupIds = null, int offset = 0, int limit = int.MaxValue)
        {
            if (IsDateRangesValid(from, to))
            {
                var configuration = await _configuration.ReadAppConfiguration();
                if (string.IsNullOrWhiteSpace(configuration.HistoryIntegratorServiceURL) || string.IsNullOrWhiteSpace(configuration.HistoryIntegratorServiceAPIKey))
                {
                    _logger.Warning("Missing configuration for history integrator Service");
                    throw new InvalidOperationException(ResultCode.MissingSettingsForHistoryIntegratorService.GetNumericString());
                }

                var decryptedAppKey = _encryptor.Decrypt(configuration.HistoryIntegratorServiceAPIKey);

                try
                {
                    using (var client = _httpClientFactory.CreateClient())
                    {
                        var query = $"{string.Concat(configuration.HistoryIntegratorServiceURL, _historyDocumentsRoute)}?" +
                            $"from={from.ToString("yyyy-MM-dd")}" +
                            $"&to={to.ToString("yyyy-MM-dd")}" +
                            $"&userId={(userId.HasValue ? userId.Value : null)}" +
                            $"&offset={offset}" +
                            $"&limit={limit}";
                        if (groupIds != null)
                        {
                            query += $"&groupIds={string.Join(',', groupIds.Select(_ => _.ToString()))}";
                        }
                        _logger.Information("Trying to fetch old reports from - " + query);
                        client.DefaultRequestHeaders.Add("AppKey", decryptedAppKey);
                        var result = await client.GetAsync(query);
                        if (result.IsSuccessStatusCode)
                        {
                            string responseContent = await result.Content.ReadAsStringAsync();
                            var reports = JsonConvert.DeserializeObject<IEnumerable<DocumentCollection>>(responseContent);
                            return reports ?? Enumerable.Empty<DocumentCollection>();
                        }
                        else
                        {
                            var errorContent = await result.Content.ReadAsStringAsync();
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException(ResultCode.OldReportsFetchFailed.GetNumericString(), ex);
                }
            }
            return Enumerable.Empty<DocumentCollection>();
        }

        private bool IsDateRangesValid(DateTime from, DateTime to)
        {
            bool isDatesRangeGreaterThanMonth = (to - from).TotalDays > 30;
            bool isFromOlderThanMonth = from < DateTime.Now.AddMonths(-1);
            return isDatesRangeGreaterThanMonth || isFromOlderThanMonth;
        }
    }
}
