using Common.Enums.PDF;
using Common.Enums.Results;
using Common.Extensions;
using Common.Interfaces;
using Common.Interfaces.Reports;
using Common.Models;
using Common.Models.ManagementApp.Reports;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace ManagementBL.Handlers
{
    public class ManagementHistoryReportsHandler : IManagementHistoryReports
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;
        private readonly string _managementReportsRoute;
        public ManagementHistoryReportsHandler(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
            _managementReportsRoute = "/managementreports";

        }

        public async Task<IEnumerable<UsageByUserReport>> ReadUsageByUserDetails(string email, Company company, DateTime from, DateTime to, IEnumerable<Guid> groupIds = null)
        {
            if (IsDateRangesValid(from, to))
            {
                var configuration = await _configuration.ReadAppConfiguration();
                if (string.IsNullOrWhiteSpace(configuration.HistoryIntegratorServiceURL) || string.IsNullOrWhiteSpace(configuration.HistoryIntegratorServiceAPIKey))
                {
                    _logger.Warning("Missing configuration for history integrator Service");
                    throw new InvalidOperationException(ResultCode.MissingSettingsForHistoryIntegratorService.GetNumericString());
                }

                try
                {
                    using (var client = _httpClientFactory.CreateClient())
                    {
                        var query = $"{string.Concat(configuration.HistoryIntegratorServiceURL, _managementReportsRoute)}/UsageByUserDetails?" +
                            $"from={from.ToString("yyyy-MM-dd")}" +
                            $"&to={to.ToString("yyyy-MM-dd")}" +
                            $"&email={email}" +
                            $"&companyId={(company != null ? company.Id : Guid.Empty)}" +
                            $"&groupIds={(groupIds != null ? string.Join(',', groupIds.Select(_ => _.ToString())) : string.Empty)}";
                        client.DefaultRequestHeaders.Add("AppKey", configuration.HistoryIntegratorServiceAPIKey);
                        var result = await client.GetAsync(query);
                        if (result.IsSuccessStatusCode)
                        {
                            string responseContent = await result.Content.ReadAsStringAsync();
                            var reports = JsonConvert.DeserializeObject<IEnumerable<UsageByUserReport>>(responseContent);
                            return reports ?? Enumerable.Empty<UsageByUserReport>();
                        }
                    }
                }
                catch (Exception)
                {
                    _logger.Warning(ResultCode.OldReportsFetchFailed.GetDescription());
                    throw new InvalidOperationException(ResultCode.OldReportsFetchFailed.GetNumericString());
                }
            }
            return Enumerable.Empty<UsageByUserReport>();
        }

        public async Task<IEnumerable<UsageByCompanyReport>> ReadUsageByCompanyAndGroups(Company company, IEnumerable<Guid> groupIds, DateTime from, DateTime to)
        {
            if (IsDateRangesValid(from, to))
            {
                var configuration = await _configuration.ReadAppConfiguration();
                if (string.IsNullOrWhiteSpace(configuration.HistoryIntegratorServiceURL) || string.IsNullOrWhiteSpace(configuration.HistoryIntegratorServiceAPIKey))
                {
                    _logger.Warning("Missing configuration for history integrator Service");
                    throw new InvalidOperationException(ResultCode.MissingSettingsForHistoryIntegratorService.GetNumericString());
                }

                try
                {
                    using (var client = new HttpClient())
                    {
                        var query = $"{string.Concat(configuration.HistoryIntegratorServiceURL, _managementReportsRoute)}/UsageByCompanyAndGroups?" +
                            $"companyId={company.Id}" +
                            $"&groupIds={(groupIds != null ? string.Join(',', groupIds.Select(_ => _.ToString())) : string.Empty)}" +
                            $"&from={from.ToString("yyyy-MM-dd")}" +
                            $"&to={to.ToString("yyyy-MM-dd")}";
                        client.DefaultRequestHeaders.Add("AppKey", configuration.HistoryIntegratorServiceAPIKey);
                        var result = await client.GetAsync(query);
                        if (result.IsSuccessStatusCode)
                        {
                            string responseContent = await result.Content.ReadAsStringAsync();
                            var reports = JsonConvert.DeserializeObject<IEnumerable<UsageByCompanyReport>>(responseContent);
                            return reports ?? Enumerable.Empty<UsageByCompanyReport>();
                        }
                    }
                }
                catch (Exception)
                {
                    _logger.Warning(ResultCode.OldReportsFetchFailed.GetDescription());
                    throw new InvalidOperationException(ResultCode.OldReportsFetchFailed.GetNumericString());
                }
            }
            return Enumerable.Empty<UsageByCompanyReport>();
        }

        public async Task<UsageBySignatureTypeReport> ReadUsageByCompanyAndSignatureTypes(Company company, IEnumerable<SignatureFieldType> signatureFieldTypes, DateTime from, DateTime to)
        {
            if (IsDateRangesValid(from, to))
            {
                var configuration = await _configuration.ReadAppConfiguration();
                if (string.IsNullOrWhiteSpace(configuration.HistoryIntegratorServiceURL) || string.IsNullOrWhiteSpace(configuration.HistoryIntegratorServiceAPIKey))
                {
                    _logger.Warning("Missing configuration for history integrator Service");
                    throw new InvalidOperationException(ResultCode.MissingSettingsForHistoryIntegratorService.GetNumericString());
                }

                try
                {
                    using (var client = new HttpClient())
                    {
                        var query = $"{string.Concat(configuration.HistoryIntegratorServiceURL, _managementReportsRoute)}/UsageByCompanyAndSignatureTypes?" +
                            $"companyId={company.Id}" +
                            $"&signatureFieldTypes={(signatureFieldTypes != null ? string.Join(',', signatureFieldTypes.Select(_ => _.ToString())) : string.Empty)}" +
                            $"&from={from.ToString("yyyy-MM-dd")}" +
                            $"&to={to.ToString("yyyy-MM-dd")}";
                        client.DefaultRequestHeaders.Add("AppKey", configuration.HistoryIntegratorServiceAPIKey);
                        var result = await client.GetAsync(query);
                        if (result.IsSuccessStatusCode)
                        {
                            string responseContent = await result.Content.ReadAsStringAsync();
                            var report = JsonConvert.DeserializeObject<UsageBySignatureTypeReport>(responseContent);
                            return report ?? null;
                        }
                    }
                }
                catch (Exception)
                {
                    _logger.Warning(ResultCode.OldReportsFetchFailed.GetDescription());
                    throw new InvalidOperationException(ResultCode.OldReportsFetchFailed.GetNumericString());
                }
            }
            return null;
        }

        private bool IsDateRangesValid(DateTime from, DateTime to)
        {
            bool isDatesRangeGreaterThanMonth = (to - from).TotalDays > 30;
            bool isFromOlderThanMonth = from < DateTime.Now.AddMonths(-1);
            return isDatesRangeGreaterThanMonth || isFromOlderThanMonth;
        }
    }
}
