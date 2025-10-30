using Common.Enums.Results;
using Common.Extensions;
using Common.Handlers;
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
    public class HistoryDocumentCollectionHandlerTests : IDisposable
    {
        private readonly Mock<IHttpClientFactory> _httpClientFactory;
        private readonly Mock<IConfiguration> _configuration;
        private readonly Mock<ILogger> _logger;
        private readonly Mock<IEncryptor> _encryptor;
        private readonly IHistoryDocumentCollection _historyDocumentCollection;

        public HistoryDocumentCollectionHandlerTests()
        {
            _httpClientFactory = new Mock<IHttpClientFactory>();
            _configuration = new Mock<IConfiguration>();
            _logger = new Mock<ILogger>();
            _encryptor = new Mock<IEncryptor>();
            _historyDocumentCollection = new HistoryDocumentCollectionHandler(_httpClientFactory.Object, _configuration.Object, _logger.Object, _encryptor.Object);
        }

        public void Dispose()
        {
            _httpClientFactory.Invocations.Clear();
            _configuration.Invocations.Clear();
            _logger.Invocations.Clear();
            _encryptor.Invocations.Clear(); 
        }

        #region ReadOldDocuments

        [Fact]
        public async Task ReadOldDocuments_MissingSettingsForHistoryIntegratorService_ThrowException()
        {
            // Arrange
            var config = new Configuration()
            {
                HistoryIntegratorServiceAPIKey = null
            };
            _configuration.Setup(_ => _.ReadAppConfiguration()).ReturnsAsync(config);
            
            // Action
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _historyDocumentCollection.ReadOldDocuments(DateTime.Now.AddYears(-1), DateTime.Now,
                null));

            // Assert
            _configuration.Verify(_ => _.ReadAppConfiguration(), Times.Once);
            Assert.Equal(ResultCode.MissingSettingsForHistoryIntegratorService.GetNumericString(), actual.Message);
        }

        #endregion
    }
}