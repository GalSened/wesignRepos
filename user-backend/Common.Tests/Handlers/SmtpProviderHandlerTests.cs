using Common.Handlers.SendingMessages;
using Common.Interfaces;
using Common.Interfaces.DB;
using Common.Models.Configurations;
using Common.Models.Emails;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Serilog;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Common.Tests.Handlers
{
    public class SmtpProviderHandlerTests : IDisposable
    {
        private const string MISSING_CONFIGURATION_MESSAGE = "Smtp configuration missing (server, port or from address)";
        private const string EXAMPLE_SERVER = "server.example";
        private const int INVALID_PORT = 0;
        private const int VALID_PORT = 1;
        private readonly Mock<ILogger> _logger;
        
        private readonly Mock<IServiceScopeFactory> _serviceScopeFactory;
        private readonly Mock<IEncryptor> _encryptor;
        private readonly Mock<IMemoryCache> _memoryCache;
        private readonly SmtpProviderHandler _smtpProviderHandler;

        public SmtpProviderHandlerTests()
        {
            _logger = new Mock<ILogger>();
            
            _serviceScopeFactory = new Mock<IServiceScopeFactory>();
            _encryptor = new Mock<IEncryptor>();
            _memoryCache = new Mock<IMemoryCache>();
            _smtpProviderHandler = new SmtpProviderHandler(_logger.Object, _serviceScopeFactory.Object, _encryptor.Object, _memoryCache.Object);
        }
        public void Dispose()
        {
            _logger.Invocations.Clear();
            
            _serviceScopeFactory.Invocations.Clear();
            _encryptor.Invocations.Clear();
            _memoryCache.Invocations.Clear();
        }

        #region Send

        [Fact]
        public async Task Send_MissingConfiguration_ThrowException()
        {
            // Arrange
            Email email = null;

            // Action
            var actual = await Assert.ThrowsAsync<Exception>(() => _smtpProviderHandler.Send(email, null));

            //Assert
            Assert.Equal(MISSING_CONFIGURATION_MESSAGE, actual.Message);
        }

        [Fact]
        public async Task Send_MissingServerConfiguration_ThrowException()
        {
            // Arrange
            Email email = null;
            SmtpConfiguration smtpConfiguration = new SmtpConfiguration()
            {
                Server = null,
            };

            // Action
            var actual =await Assert.ThrowsAsync<Exception>(() => _smtpProviderHandler.Send(email, null));

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
            var actual =await Assert.ThrowsAsync<Exception>(() => _smtpProviderHandler.Send(email, null));

            // Assert
            Assert.Equal(MISSING_CONFIGURATION_MESSAGE, actual.Message);
        }

        [Fact]
        public async Task Send_MissingFromConfiguration_ThrowException()
        {
            // Arrange
            Email email = null;
            SmtpConfiguration smtpConfiguration = new SmtpConfiguration()
            {
                Server = EXAMPLE_SERVER,
                Port = VALID_PORT,
                From = null
            };

            // Action
            var actual =await Assert.ThrowsAsync<Exception>(() => _smtpProviderHandler.Send(email, null));

            // Assert
            Assert.Equal(MISSING_CONFIGURATION_MESSAGE, actual.Message);
        }

        #endregion
    }
}
