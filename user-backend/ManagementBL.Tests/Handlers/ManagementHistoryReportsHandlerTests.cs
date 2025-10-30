using Common.Enums.Results;
using Common.Extensions;
using Common.Interfaces;
using Common.Interfaces.Reports;
using Common.Models.Configurations;
using ManagementBL.Handlers;
using Moq;
using Serilog;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace ManagementBL.Tests.Handlers
{
    public class ManagementHistoryReportsHandlerTests : IDisposable
    {
        private readonly Mock<IHttpClientFactory> _httpClientFactory;
        private readonly Mock<IConfiguration> _configuration;
        private readonly Mock<ILogger> _logger;
        private readonly IManagementHistoryReports _managementHistoryReports;

        public ManagementHistoryReportsHandlerTests()
        {
            _httpClientFactory = new Mock<IHttpClientFactory>();
            _configuration = new Mock<IConfiguration>();
            _logger = new Mock<ILogger>();
            _managementHistoryReports = new ManagementHistoryReportsHandler(_httpClientFactory.Object, _configuration.Object, _logger.Object);
        }

        public void Dispose()
        {
            _httpClientFactory.Invocations.Clear();
            _configuration.Invocations.Clear();
            _logger.Invocations.Clear();
        }

        #region ReadUsageByUserDetails

        [Fact]
        public async Task ReadUsageByUserDetails_MissingSettingsForHistoryIntegratorService_ThrowException()
        {
            // Arrange
            var config = new Configuration()
            {
                HistoryIntegratorServiceAPIKey = null
            };
            _configuration.Setup(_ => _.ReadAppConfiguration()).ReturnsAsync(config);

            // Action
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _managementHistoryReports.ReadUsageByUserDetails(string.Empty, null, 
                DateTime.Now.AddYears(-1), DateTime.Now));

            // Assert
            _configuration.Verify(_ => _.ReadAppConfiguration(), Times.Once);
            Assert.Equal(ResultCode.MissingSettingsForHistoryIntegratorService.GetNumericString(), actual.Message);
        }

        #endregion

        #region ReadUsageByCompanyAndGroups

        [Fact]
        public async Task ReadUsageByCompanyAndGroups_MissingSettingsForHistoryIntegratorService_ThrowException()
        {
            // Arrange
            var config = new Configuration()
            {
                HistoryIntegratorServiceAPIKey = null
            };
            _configuration.Setup(_ => _.ReadAppConfiguration()).ReturnsAsync(config);

            // Action
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _managementHistoryReports.ReadUsageByCompanyAndGroups(null, null, 
                DateTime.Now.AddYears(-1), DateTime.Now));

            // Assert
            _configuration.Verify(_ => _.ReadAppConfiguration(), Times.Once);
            Assert.Equal(ResultCode.MissingSettingsForHistoryIntegratorService.GetNumericString(), actual.Message);
        }

        #endregion

        #region ReadUsageByCompanyAndSignatureTypes

        [Fact]
        public async Task ReadUsageByCompanyAndSignatureTypes_MissingSettingsForHistoryIntegratorService_ThrowException()
        {
            // Arrange
            var config = new Configuration()
            {
                HistoryIntegratorServiceAPIKey = null
            };
            _configuration.Setup(_ => _.ReadAppConfiguration()).ReturnsAsync(config);

            // Action
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _managementHistoryReports.ReadUsageByCompanyAndSignatureTypes(null, null,
                DateTime.Now.AddYears(-1), DateTime.Now));

            // Assert
            _configuration.Verify(_ => _.ReadAppConfiguration(), Times.Once);
            Assert.Equal(ResultCode.MissingSettingsForHistoryIntegratorService.GetNumericString(), actual.Message);
        }

        #endregion
    }
}
