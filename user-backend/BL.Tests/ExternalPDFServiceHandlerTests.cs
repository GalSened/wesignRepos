using BL.Handlers;
using Common.Enums.Results;
using Common.Extensions;
using Common.Interfaces;
using Common.Interfaces.PDF;
using Common.Models.Configurations;
using Common.Models.Settings;
using Microsoft.Extensions.Options;
using Moq;
using Serilog;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace BL.Tests
{
    public class ExternalPDFServiceHandlerTests : IDisposable
    {
        private const string PDF_SERVICE_URL = "example-url";
        private readonly Mock<IOptions<GeneralSettings>> _options;
        private readonly Mock<GeneralSettings> _generalSettings;
        private readonly Mock<IHttpClientFactory> _httpClientFactory;
        private readonly Mock<ILogger> _logger;
        private readonly Mock<IEncryptor> _encryptor;
        private readonly Mock<IConfiguration> _configuration;
        private readonly IExternalPDFService _externalPDFServiceHandler;

        public ExternalPDFServiceHandlerTests()
        {
            _options = new Mock<IOptions<GeneralSettings>>();
            _generalSettings = new Mock<GeneralSettings>();
            _httpClientFactory = new Mock<IHttpClientFactory>();
            _logger = new Mock<ILogger>();
            _encryptor = new Mock<IEncryptor>();
            _configuration = new Mock<IConfiguration>();
            _options.SetupGet(_ => _.Value).Returns(_generalSettings.Object);
            _externalPDFServiceHandler = new ExternalPDFServiceHandler(_httpClientFactory.Object, _logger.Object, _encryptor.Object, 
                _configuration.Object);
        }

        public void Dispose()
        {
            _options.Invocations.Clear();
            _generalSettings.Invocations.Clear();
            _httpClientFactory.Invocations.Clear();
            _logger.Invocations.Clear();
            _encryptor.Invocations.Clear();
            _configuration.Invocations.Clear();
        }

        #region Merge

        [Fact]
        public async Task Merge_MissingExternalPdfServiceURL_ThrowInvalidOperationException()
        {
            // Arrange
            var config = new Configuration()
            {
                ExternalPdfServiceURL = null
            };
            _configuration.Setup(_ => _.ReadAppConfiguration()).ReturnsAsync(config);

            // Action
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _externalPDFServiceHandler.Merge(null));

            // Assert
            _configuration.Verify(_ => _.ReadAppConfiguration(), Times.Once);
            Assert.Equal(ResultCode.MissingSettingsForPDFExternalSettings.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task Merge_MissingExternalPdfServiceAPIKey_ThrowInvalidOperationException()
        {
            // Arrange
            var config = new Configuration()
            {
                ExternalPdfServiceURL = PDF_SERVICE_URL,
                ExternalPdfServiceAPIKey = null
            };
            _configuration.Setup(_ => _.ReadAppConfiguration()).ReturnsAsync(config);

            // Action
            var actual =await Assert.ThrowsAsync<InvalidOperationException>(() => _externalPDFServiceHandler.Merge(null));

            // Assert
            _configuration.Verify(_ => _.ReadAppConfiguration(), Times.Once);
            Assert.Equal(ResultCode.MissingSettingsForPDFExternalSettings.GetNumericString(), actual.Message);
        }

        #endregion
    }
}
