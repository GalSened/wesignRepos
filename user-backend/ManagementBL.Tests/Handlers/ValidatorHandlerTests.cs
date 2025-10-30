using Common.Enums;
using Common.Enums.Results;
using Common.Extensions;
using Common.Handlers.FileGateScanner.Providers;
using Common.Interfaces;
using Common.Interfaces.DB;
using Common.Interfaces.FileGateScanner;
using Common.Models.Configurations;
using Common.Models.FileGateScanner;
using ManagementBL.Handlers;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ManagementBL.Tests.Handlers
{
    public class ValidatorHandlerTests
    {
        private readonly Mock<IConfigurationConnector> _confourationConnectorMock;
        private readonly Mock<IFileGateScannerProviderFactory> _fileGateScannerProviderHandlerMock;

        private readonly IValidator _validatorHandler;

        public ValidatorHandlerTests()
        {
            _confourationConnectorMock  = new Mock<IConfigurationConnector>();
               _fileGateScannerProviderHandlerMock = new Mock<IFileGateScannerProviderFactory>();

            _validatorHandler = new ManagementValidatorHandler(_confourationConnectorMock.Object, _fileGateScannerProviderHandlerMock.Object);
        }

        #region ValidateIsCleanFile

        [Theory]
        [InlineData("any file")]
        [InlineData("")]
        [InlineData(null)]
        public async Task ValidateIsCleanFile_Base64FileWithNoneScanGateProvider_ReturnSameBase64File_Success(string base64string)
        {
            _confourationConnectorMock.Setup(x => x.Read()).ReturnsAsync(new Configuration());
            _fileGateScannerProviderHandlerMock.Setup(x => x.ExecuteCreation(It.IsAny<FileGateScannerProviderType>())).Returns(new NoneGateScannerHandler(null,null));

            var actual =await _validatorHandler.ValidateIsCleanFile(base64string);

            Assert.Equal(base64string, actual.CleanFile);
        }

        #endregion
    }
}
