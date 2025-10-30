using Common.Enums;
using Common.Enums.Results;
using Common.Extensions;
using Common.Interfaces.DB;
using Common.Interfaces.FileGateScanner;
using Common.Models.FileGateScanner;
using ManagementBL.Handlers;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace ManagementBL.Tests.Handlers
{
    public class ManagementValidatorHandlerTest : IDisposable
    {
        private const string BASE_64_STRING = "BASE_64_STRING";
        private readonly Mock<IDbConnector> _dbConnector;
        private readonly Mock<IConfigurationConnector> _configurationConnector;
        private readonly Mock<IFileGateScannerProvider> _fileGateScannerProvider;
        private readonly Mock<IFileGateScannerProviderFactory> _fileGateScannerProviderFactory;
        private readonly ManagementValidatorHandler _managementValidatorHandler;
        public ManagementValidatorHandlerTest()
        {
            _dbConnector = new Mock<IDbConnector>();
            _configurationConnector = new Mock<IConfigurationConnector>();
            _fileGateScannerProvider = new Mock<IFileGateScannerProvider>();
            _fileGateScannerProviderFactory = new Mock<IFileGateScannerProviderFactory>();
            _dbConnector.SetupGet(_ => _.Configurations).Returns(_configurationConnector.Object);
            _fileGateScannerProviderFactory.Setup(_ => _.ExecuteCreation(It.IsAny<FileGateScannerProviderType>()))
                .Returns(_fileGateScannerProvider.Object);
            _managementValidatorHandler = new ManagementValidatorHandler(_configurationConnector.Object, _fileGateScannerProviderFactory.Object);
        }

        public void Dispose()
        {
            _dbConnector.Invocations.Clear();
            _configurationConnector.Invocations.Clear();
            _fileGateScannerProvider.Invocations.Clear();
            _fileGateScannerProviderFactory.Invocations.Clear();
        }

        #region ValidateIsCleanFile

        [Fact]
        public async Task ValidateIsCleanFile_Invalid_ThrowInvalidOperationException()
        {
            // Arrange
            FileGateScanResult fileGateScanResult = new FileGateScanResult()
            { IsValid = false };
            _fileGateScannerProvider.Setup(_ => _.Scan(It.IsAny<FileGateScan>())).Returns(fileGateScanResult);

            // Action
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _managementValidatorHandler.ValidateIsCleanFile(BASE_64_STRING));

            // Assert
            _fileGateScannerProvider.Verify(_ => _.Scan(It.IsAny<FileGateScan>()), Times.Once);
            Assert.Equal(ResultCode.InvalidFileContent.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task ValidateIsCleanFile_Valid_ShouldSuccess()
        {
            // Arrange
            FileGateScanResult requiredFileGateScanResult = new FileGateScanResult()
            {
                IsValid = true,
                CleanFile = BASE_64_STRING
            };
            _fileGateScannerProvider.Setup(_ => _.Scan(It.IsAny<FileGateScan>())).Returns(requiredFileGateScanResult);

            // Action
            FileGateScanResult fileGateScanResult = await _managementValidatorHandler.ValidateIsCleanFile(BASE_64_STRING);

            // Assert
            _fileGateScannerProvider.Verify(_ => _.Scan(It.IsAny<FileGateScan>()), Times.Once);
            Assert.Equal(requiredFileGateScanResult, fileGateScanResult);
        }

        #endregion
    }
}
