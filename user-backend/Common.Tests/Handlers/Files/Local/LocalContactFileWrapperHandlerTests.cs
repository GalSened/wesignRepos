using Common.Handlers.Files.Local;
using Common.Interfaces;
using Common.Interfaces.Files;
using Common.Models.Settings;
using Microsoft.Extensions.Options;
using Moq;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Tests.Handlers.Files.Local
{
    public class LocalContactFileWrapperHandlerTests
    {




        private readonly Guid ID = new Guid("C32BCF3A-C273-4F98-B002-A724DE1479FE");
        private const string DEFAULT_EMAIL_TEMPLATE = "Resources/EmailBody.html";
        private const string DEFAULT_LOGO = "Resources/Logo.png";
        private readonly IFileSystem _fileSystemMock;
        private IOptions<FolderSettings> _folderSettingsMock;
        private readonly Mock<IDataUriScheme> _dataUriSchemeMock;
        private IContactFileWrapper _contactFileWrapper;
        private readonly Mock<ILogger> _logger;
        private int NUMBER_OF_SIGNATURES = 6;

        public LocalContactFileWrapperHandlerTests()
        {
            _fileSystemMock = new MockFileSystem();
            _logger = new Mock<ILogger>();
            _folderSettingsMock = Options.Create(new FolderSettings()
            {
                EmailTemplates = "c:\\comda\\wesign\\emailTemplates",
                CompaniesLogo = "c:\\comda\\wesign\\CompaniesLogo",
                ContactSignatureImages = "c:\\comda\\wesign\\ContactSignatureImages",
                ContactCertificates = "c:\\comda\\wesign\\ContactCertificates",
                ContactSeals = "c:\\comda\\wesign\\ContactSeals",
            });
            _dataUriSchemeMock = new Mock<IDataUriScheme>();
            _contactFileWrapper = new LocalContactFileWrapperHandler(_folderSettingsMock,_fileSystemMock, _logger.Object, _dataUriSchemeMock.Object);
        }


    }
}
