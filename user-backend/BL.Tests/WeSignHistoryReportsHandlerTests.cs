using BL.Handlers;
using Common.Enums.Results;
using Common.Extensions;
using Common.Handlers;
using Common.Interfaces;
using Common.Interfaces.Reports;
using Common.Models.Configurations;
using Moq;
using Serilog;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace BL.Tests
{
    public class WeSignHistoryReportsHandlerTests : IDisposable
    {
        private readonly Mock<IHttpClientFactory> _httpClientFactory;
        private readonly Mock<IConfiguration> _configuration;
        private readonly Mock<ILogger> _logger;
        private readonly IWeSignHistoryReports _weSignHistoryReports;

        public WeSignHistoryReportsHandlerTests()
        {
            _httpClientFactory = new Mock<IHttpClientFactory>();
            _configuration = new Mock<IConfiguration>();
            _logger = new Mock<ILogger>();
            _weSignHistoryReports = new WeSignHistoryReportsHandler(_httpClientFactory.Object, _configuration.Object, _logger.Object);
        }

        public void Dispose()
        {
            _httpClientFactory.Invocations.Clear();
            _configuration.Invocations.Clear();
            _logger.Invocations.Clear();
        }

        #region ReadUsageDataReports

        [Fact]
        public async Task ReadUsageDataReports_MissingSettingsForHistoryIntegratorService_ThrowException()
        {
            // Arrange
            var config = new Configuration()
            {
                HistoryIntegratorServiceAPIKey = null
            };
            _configuration.Setup(_ => _.ReadAppConfiguration()).ReturnsAsync(config);

            // Action
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _weSignHistoryReports.ReadUsageDataReports(Guid.Empty, DateTime.Now.AddYears(-1), DateTime.Now,
                null, null, false));

            // Assert
            _configuration.Verify(_ => _.ReadAppConfiguration(), Times.Once);
            Assert.Equal(ResultCode.MissingSettingsForHistoryIntegratorService.GetNumericString(), actual.Message);
        }

        #endregion
    }
}
