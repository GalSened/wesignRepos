using Common.Handlers.FileGateScanner.Providers;
using Common.Interfaces;
using Common.Interfaces.DB;
using Common.Models.FileGateScanner;
using Moq;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Common.Tests.Handlers.FileGateScanner
{
    public class NoneGateScannerHandlerTests
    {


        private readonly Mock<IDataUriScheme> _dataUriScheme;
        private readonly Mock<ILogger> _logger;
        private readonly NoneGateScannerHandler _noneGateScannerHandler;

        public NoneGateScannerHandlerTests()
        {
         
            _dataUriScheme = new Mock<IDataUriScheme>();
            _logger = new Mock<ILogger>();
            _noneGateScannerHandler = new NoneGateScannerHandler(_logger.Object,  _dataUriScheme.Object);
        }

        [Fact]
        public void Scan_EmptyString_ReturnEmptyString()
        {
            FileGateScan fileGateScan = new FileGateScan
            {
                Base64 = string.Empty
            };

            var actual = _noneGateScannerHandler.Scan(fileGateScan);

            Assert.Empty(actual.CleanFile);
        }

        [Fact]
        public void Scan_ValidBase64File_ReturnSameBase64File()
        {
            string smallImage = "R0lGODlhAQABAIAAAP///////yH5BAEKAAEALAAAAAABAAEAAAICTAEAOw==";

            FileGateScan fileGateScan = new FileGateScan
            {
                Base64 = $"data:image/png;base64,{smallImage}"
            };

            var actual = _noneGateScannerHandler.Scan(fileGateScan);

            Assert.Equal($"data:image/png;base64,{smallImage}", actual.CleanFile);
        }
    }
}
