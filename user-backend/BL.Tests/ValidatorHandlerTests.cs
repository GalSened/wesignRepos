using BL.Handlers;
using Common.Enums;
using Common.Enums.Results;
using Common.Extensions;
using Common.Handlers;
using Common.Handlers.FileGateScanner.Providers;
using Common.Interfaces;
using Common.Interfaces.DB;
using Common.Interfaces.FileGateScanner;
using Common.Models.Configurations;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace BL.Tests
{
    public class ValidatorHandlerTests
    {
        private readonly Mock<IFileGateScannerProviderFactory> _fileGateScannerProviderHandler;
        private readonly Mock<IProgramConnector> _programConnectorMock;
        private readonly Mock<IConfigurationConnector> _configurationConnectorMock;
        private readonly IValidator _validator;

        public ValidatorHandlerTests()
        {
            _programConnectorMock = new Mock<IProgramConnector>();
            _configurationConnectorMock = new Mock<IConfigurationConnector>();
            _fileGateScannerProviderHandler = new Mock<IFileGateScannerProviderFactory>();
            _validator = new ValidatorHandler(_programConnectorMock.Object, _configurationConnectorMock.Object, _fileGateScannerProviderHandler.Object); 
        }

        #region HasDuplication

        [Fact]
        public void HasDuplication_WithoutDuplication_Success()
        {
            IEnumerable<string> collection = new List<string>() { "1", "2", "3" };

            var actual = _validator.HasDuplication(collection);

            Assert.False(actual);
        }

        [Fact]
        public void HasDuplication_WithDuplication_Success()
        {
            IEnumerable<string> collection = new List<string>() { "1", "2", "3", "2" };

            var actual = _validator.HasDuplication(collection);

            Assert.True(actual);
        }

        [Fact]
        public void HasDuplication_InvalidCollection_ReturnFalse()
        {
            IEnumerable<string> collection = null;

            var actual = _validator.HasDuplication(collection);

            Assert.False(actual);
        }

        #endregion

        #region ValidateIsCleanFile

        [Theory]
        [InlineData("data:image/png;base64,ndsvjndsvonvdnsdo")]
        [InlineData("any file")]
        [InlineData("")]
        [InlineData(null)]
        public async Task ValidateIsCleanFile_Base64FileWithNoneScanGateProvider_ReturnSameBase64File(string base64string)
        {
            _configurationConnectorMock.Setup(x => x.Read()).ReturnsAsync(new Configuration());
            _fileGateScannerProviderHandler.Setup(x => x.ExecuteCreation(It.IsAny<FileGateScannerProviderType>())).Returns(new NoneGateScannerHandler(null, null));

            var actual =await _validator.ValidateIsCleanFile(base64string);

            Assert.Equal(base64string, actual.CleanFile);
        }

        #endregion
    }
}