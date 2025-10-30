using Comda.License.Interfaces;
using Common.Enums.Results;
using Common.Extensions;
using Common.Interfaces;
using Common.Interfaces.DB;
using Common.Interfaces.ManagementApp;
using Common.Models.ManagementApp;
using Comda.License.Models;
using Common.Models.Settings;
using ManagementBL.Handlers;
using Microsoft.Extensions.Options;
using Moq;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Text;
using Xunit;
using Comda.License.SDK.Models.License;
using UserInfo = Common.Models.ManagementApp.UserInfo;
using Comda.License;

namespace ManagementBL.Tests.Handlers
{
    public class LicenceHandlerTests : IDisposable
    {

        private readonly Mock<ILicenseDMZ> _licenseDMZMock;
        
        private readonly Mock<ILicenseManager> _licenseManagerMock;
        private readonly Mock<IFileSystem> _fileSystemMock;
        private readonly Mock<ILogger> _loggerMock;
        private readonly Mock<ILicenseWrapper> _licenceWrapperMock;

        private readonly IOptions<FolderSettings> _folderSettings;
        private readonly IOptions<GeneralSettings> _generalSettings;


        private readonly Mock<IProgramConnector> _programConnectorMock;
        private readonly Mock<ICompanyConnector> _companyConnectorMock;
        private readonly LicenseHandler _licenseHandler;

        private Manager manager;

        public void Dispose()
        {
            _licenseDMZMock.Invocations.Clear();
            
            _licenseManagerMock.Invocations.Clear();
            _fileSystemMock.Invocations.Clear();
            _loggerMock.Invocations.Clear();
            _licenceWrapperMock.Invocations.Clear();
            _programConnectorMock.Invocations.Clear();
            _companyConnectorMock.Invocations.Clear();
        }

        public LicenceHandlerTests()
        {
            _licenseDMZMock = new Mock<ILicenseDMZ>();
            
            _licenseManagerMock = new Mock<ILicenseManager>();
            _fileSystemMock = new Mock<IFileSystem>();
            _loggerMock = new Mock<ILogger>();
            _licenceWrapperMock = new Mock<ILicenseWrapper>();
            _programConnectorMock = new Mock<IProgramConnector>();
            _folderSettings = Options.Create(new FolderSettings { });
            _generalSettings = Options.Create(new GeneralSettings { });
            _companyConnectorMock = new Mock<ICompanyConnector>();

            _fileSystemMock.Setup(x => x.Directory.CreateDirectory(It.IsAny<string>()));
            _licenceWrapperMock.Setup(x => x.GetLicenseManager());

            _licenseHandler = new LicenseHandler(_companyConnectorMock.Object, _programConnectorMock.Object, _loggerMock.Object, _licenseDMZMock.Object, _generalSettings, _folderSettings, _licenceWrapperMock.Object, _fileSystemMock.Object);
        }

        #region GenerateLicence

        //[Fact]
        //public void ValidateProgramAddition_TheSMSIsUnlimitedInLMSSMSIsLimited_ShouldThrowException()
        //{
        //    _dbConnectorMock.Setup(x => x.Programs.Read(It.IsAny<Grogram>))
        //}
        //[Fact]
        //public void ValidateProgramAddition_TheTemplatesIsUnlimitedInLMSTemplatesIsLimited_ShouldThrowException()
        //{

        //}
        //[Fact]
        //public void ValidateProgramAddition_TheUsersIsUnlimitedInLMSUsersIsLimited_ShouldThrowException()
        //{

        //}

        //[Fact]
        //public void GenerateLicence_FailedInitLicenseRequest_ShouldThrowException()
        //{
        //    // Arrange
        //    UserInfo userInfo = new UserInfo();
        //    LicenseResponse licenseResponse = new LicenseResponse();

        //    _licenseDMZMock.Setup(x => x.IsDMZReachable()).ReturnsAsync(true);
        //    _licenseManagerMock.Setup(x => x.Init(It.IsAny<Comda.License.Models.UserInfo>(), It.IsAny<bool>())).ReturnsAsync(licenseResponse);

        //    // Act
        //    var actual = Assert.ThrowsAsync<InvalidOperationException>(async () => await _licenseHandler.GenerateLicense(userInfo));


        //    // Assert
        //    Assert.Equal(ResultCode.FailedInitLicenseRequest.GetNumericString(), actual.Exception.Message);
        //}

        #endregion
    }
}
