using Common.Handlers;
using Common.Interfaces;
using Common.Interfaces.DB;
using Common.Models.Configurations;
using Common.Models.Emails;
using Common.Models.Settings;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Serilog;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Common.Tests.Handlers
{
    public class SmtpMailkitProviderHandlerTests : IDisposable
    {
        private const string MISSING_CONFIGURATION_MESSAGE = "Smtp configuration missing (server, port or from address)";
        private const string EXAMPLE_SERVER = "server.example";
        private const int INVALID_PORT = 0;
        private const int VALID_PORT = 1;
        private readonly Mock<ILogger> _logger;
        private readonly Mock<IDbConnector> _dbConnector;
        private readonly Mock<IServiceScopeFactory> _serviceScopeFactory;
        private readonly Mock<IEncryptor> _encryptor;
        private readonly Mock<IMemoryCache> _memoryCache;
        private readonly Mock<IOptions<GeneralSettings>> _options;
        private readonly Mock<GeneralSettings> _generalSettings;
        private readonly SmtpMailkitProviderHandler _smtpMailkitProviderHandler;

        public SmtpMailkitProviderHandlerTests()
        {
            _logger = new Mock<ILogger>();
            _dbConnector = new Mock<IDbConnector>();
            _serviceScopeFactory = new Mock<IServiceScopeFactory>();
            _encryptor = new Mock<IEncryptor>();
            _memoryCache = new Mock<IMemoryCache>();
            _options = new Mock<IOptions<GeneralSettings>>();
            _generalSettings = new Mock<GeneralSettings>();
            _options.SetupGet(_ => _.Value).Returns(_generalSettings.Object);
            _smtpMailkitProviderHandler = new SmtpMailkitProviderHandler(_logger.Object,  _serviceScopeFactory.Object, _encryptor.Object, _memoryCache.Object, _options.Object);
        }

        public void Dispose()
        {
            _logger.Invocations.Clear();
            _dbConnector.Invocations.Clear();
            _serviceScopeFactory.Invocations.Clear();
            _encryptor.Invocations.Clear();
            _memoryCache.Invocations.Clear();
            _options.Invocations.Clear();
            _generalSettings.Invocations.Clear();
        }

        #region Send

        [Fact]
        public async Task Send_MissingConfiguration_ThrowException()
        {
            // Arrange
            Email email = null;

            // Action
            var actual = await Assert.ThrowsAsync<Exception>(() => _smtpMailkitProviderHandler.Send(email, null));

            // Assert
            Assert.Equal(MISSING_CONFIGURATION_MESSAGE, actual.Message);
        }

        [Fact]
        public async Task Send_MissingServerConfiguration_ThrowException()
        {
            // Arrange
            Email email = null;
            SmtpConfiguration smtpConfiguration = new SmtpConfiguration()
            {
                Server = null
            };

            // Action
            var actual = await Assert.ThrowsAsync<Exception>(() => _smtpMailkitProviderHandler.Send(email, null));

            // Assert
            Assert.Equal(MISSING_CONFIGURATION_MESSAGE, actual.Message);
        }

        [Fact]
        public async Task Send_InvalidPortConfiguration_ThrowException()
        {
            // Arrange
            Email email = null;
            SmtpConfiguration smtpConfiguration = new SmtpConfiguration()
            {
                Server = EXAMPLE_SERVER,
                Port = INVALID_PORT
            };
            // Action
            var actual = await Assert.ThrowsAsync<Exception>(() => _smtpMailkitProviderHandler.Send(email, null));

            // Assert
            Assert.Equal(MISSING_CONFIGURATION_MESSAGE, actual.Message);
        }

        [Fact]
        public async Task Send_MissingFromConfiguration_ThrowException()
        {
            // Arrange
            Email email = null;
            SmtpConfiguration emailConfiguration = new SmtpConfiguration()
            {
                Server = EXAMPLE_SERVER,
                Port = VALID_PORT,
                From = null
            };
            
            // Action
            var actual =await Assert.ThrowsAsync<Exception>(() => _smtpMailkitProviderHandler.Send(email, null));

            // Assert
            Assert.Equal(MISSING_CONFIGURATION_MESSAGE, actual.Message);
        }

        #endregion
    }
}
