using Common.Enums.Logs;
using Common.Interfaces.DB;
using Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using Moq;
using Serilog;
using Common.Interfaces.SignerApp;
using SignerBL.Handlers;
using Xunit;
using Common.Models.Documents.Signers;
using Common.Models;
using Common.Enums.Results;
using Common.Extensions;
using System.Threading.Tasks;

namespace SignerBL.Tests
{
    public class LogsHandlerTests : IDisposable
    {
        private readonly Mock<ILogger> _loggerMock;
        private readonly Mock<ISignerTokenMappingConnector> _signerTokeMappingConectorMock;
        private readonly Mock<IJWT> _jwtMock;

        private readonly ILogs _logsHandler;

        public void Dispose()
        {
            _loggerMock.Invocations.Clear();
            _signerTokeMappingConectorMock.Invocations.Clear();
            _jwtMock.Invocations.Clear();
        }

        public LogsHandlerTests()
        {
            _loggerMock = new Mock<ILogger>();
            _signerTokeMappingConectorMock = new Mock<ISignerTokenMappingConnector>();
            _jwtMock = new Mock<IJWT>();

            _logsHandler = new LogsHandler(_loggerMock.Object, _signerTokeMappingConectorMock.Object, _jwtMock.Object);
        }

        #region Create

        [Fact]
        public async Task Create_SignerIsNull_ShouldThrowException()
        {
            SignerTokenMapping signerTokenMapping = new SignerTokenMapping();
            string token = "";
            LogMessage logMessage = new LogMessage();


            _signerTokeMappingConectorMock.Setup(x => x.Read(It.IsAny<SignerTokenMapping>())).ReturnsAsync(signerTokenMapping);

            var actual =await Assert.ThrowsAsync<InvalidOperationException>(() => _logsHandler.Create(token, logMessage));

            Assert.IsType<InvalidOperationException>(actual);
            Assert.Equal(ResultCode.InvalidToken.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task Create_LogLevelIsDebug()
        {
            SignerTokenMapping signerTokenMapping = new SignerTokenMapping();
            string token = "";
            LogMessage logMessage = new LogMessage()
            {
                LogLevel = LogLevel.Debug,
                Message = "message"
            };
            string logMessageText = "";
            Signer signer = new Signer()
            {
                Id = Guid.NewGuid(),
            };
            User user = new User();


            _signerTokeMappingConectorMock.Setup(x => x.Read(It.IsAny<SignerTokenMapping>())).ReturnsAsync(signerTokenMapping);
            _jwtMock.Setup(x => x.GetSigner(It.IsAny<string>())).Returns(signer);
            _jwtMock.Setup(x => x.GetUser(It.IsAny<string>())).Returns(user);
            _loggerMock.Setup(x => x.Information(It.IsAny<string>())).Callback((string a) => logMessageText = a);

            await _logsHandler.Create(token, logMessage);

            _loggerMock.Verify(x=>x.Debug(It.IsAny<string>()), Times.Once);
        }

        #endregion

    }
}
