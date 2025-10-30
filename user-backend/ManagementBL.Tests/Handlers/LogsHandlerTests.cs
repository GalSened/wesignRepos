using Common.Enums.Logs;
using Common.Interfaces.DB;
using Common.Interfaces.ManagementApp;
using Common.Models;
using ManagementBL.Handlers;
using Moq;
using Org.BouncyCastle.Crypto.Agreement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace ManagementBL.Tests.Handlers
{
    public class LogsHandlerTests
    {
        private readonly Mock<ILogConnector> _logConnectorMock;

        private readonly ILogs _logsHandler;

        public LogsHandlerTests()
        {
            _logConnectorMock = new Mock<ILogConnector>();

            _logsHandler = new LogsHandler(_logConnectorMock.Object);
        }

        [Fact]
        public void Read_ReturnEmptyList_Success()
        {
            IEnumerable<LogMessage> logMessages = new List<LogMessage>();

            int totalCount;
            _logConnectorMock.Setup(x=>x.Read(It.IsAny<LogApplicationSource>(), It.IsAny<string>(),It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny <LogLevel>(), It.IsAny<int>(), It.IsAny<int>(), out totalCount)).Returns(logMessages);

            var actual = _logsHandler.Read(new LogApplicationSource(), "", "", "", LogLevel.All, 0, int.MaxValue, out totalCount);

            Assert.Equal(logMessages.Count(), actual.Count());
        }


        [Fact]
        public void Read_ReturnList_Success()
        {
            IEnumerable<LogMessage> logMessages = new List<LogMessage>()
            {
                new LogMessage()
                {
                    
                },
                new LogMessage()
                {

                },
            };

            int totalCount;
            _logConnectorMock.Setup(x => x.Read(It.IsAny<LogApplicationSource>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<LogLevel>(), It.IsAny<int>(), It.IsAny<int>(), out totalCount)).Returns(logMessages);

            var actual = _logsHandler.Read(new LogApplicationSource(), "", "", "", LogLevel.All, 0, int.MaxValue, out totalCount);

            Assert.Equal(logMessages.Count(), actual.Count());
        }
    }
}
