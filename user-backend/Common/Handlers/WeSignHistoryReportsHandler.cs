using Common.Enums.Documents;
using Common.Enums.Results;
using Common.Extensions;
using Common.Interfaces;
using Common.Interfaces.Reports;
using Common.Models.Reports;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Common.Handlers
{
    public class WeSignHistoryReportsHandler : IWeSignHistoryReports
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;
        private readonly string _weSignReportsRoute;

        public WeSignHistoryReportsHandler(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
            _weSignReportsRoute = "/wesignreports";
        }

        public async Task<IEnumerable<UsageDataReport>> ReadUsageDataReports(Guid userId, DateTime from, DateTime to, IEnumerable<DocumentStatus> documentStatuses, IEnumerable<Guid> groupIds, bool includeDistributionDocs)
        {
            if (IsDateRangesValid(from, to))
            {
                var configuration = await _configuration.ReadAppConfiguration();
                if (string.IsNullOrWhiteSpace(configuration.HistoryIntegratorServiceURL) || string.IsNullOrWhiteSpace(configuration.HistoryIntegratorServiceAPIKey))
                {
                    _logger.Warning(ResultCode.MissingSettingsForHistoryIntegratorService.GetDescription());
                    throw new InvalidOperationException(ResultCode.MissingSettingsForHistoryIntegratorService.GetNumericString());
                }

                try
                {
                    using (var client = _httpClientFactory.CreateClient())
                    {
                        var query = $"{string.Concat(configuration.HistoryIntegratorServiceURL, _weSignReportsRoute)}/UserUsageDataReports?" +
                            $"userId={userId}" +
                            $"&from={from.ToString("yyyy-MM-dd")}" +
                            $"&to={to.ToString("yyyy-MM-dd")}" +
                            $"&documentStatuses={(documentStatuses != null ? string.Join(',', documentStatuses.Select(_ => _.ToString())) : string.Empty)}" +
                            $"&groupIds={(groupIds != null ? string.Join(',', groupIds.Select(_ => _.ToString())) : string.Empty)}" +
                            $"&includeDistributionDocs={includeDistributionDocs.ToString().ToLower()}";
                        client.DefaultRequestHeaders.Add("AppKey", configuration.HistoryIntegratorServiceAPIKey);
                        var result = await client.GetAsync(query);

                        if (result.IsSuccessStatusCode)
                        {
                            string responseContent = await result.Content.ReadAsStringAsync();
                            var reports = JsonConvert.DeserializeObject<IEnumerable<UsageDataReport>>(responseContent);
                            return reports ?? Enumerable.Empty<UsageDataReport>();
                        }
                    }
                }
                catch (Exception)
                {
                    _logger.Warning(ResultCode.OldReportsFetchFailed.GetDescription());
                    throw new InvalidOperationException(ResultCode.OldReportsFetchFailed.GetNumericString());
                }
            }
            return Enumerable.Empty<UsageDataReport>();
        }

        private bool IsDateRangesValid(DateTime from, DateTime to)
        {
            bool isDatesRangeGreaterThanMonth = (to - from).TotalDays > 30;
            bool isFromOlderThanMonth = from < DateTime.Now.AddMonths(-1);
            return isDatesRangeGreaterThanMonth || isFromOlderThanMonth;
        }
    }
}
