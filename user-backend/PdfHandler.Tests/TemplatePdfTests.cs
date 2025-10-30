namespace PdfHandler.Tests
{
    using BL.Tests;
    using Common.Enums;
    using Common.Handlers.Files;
    using Common.Interfaces;
    using Common.Interfaces.Files;
    using Common.Interfaces.PDF;
    using Common.Models.Settings;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Options;
    using Moq;
    using Org.BouncyCastle.Crypto.Paddings;
    using PdfHandler.Interfaces;
    using Serilog;
    using System;
    using System.Collections.Generic;
    using System.IO.Abstractions;
    using System.IO.Abstractions.TestingHelpers;
    using Xunit;

    public class TemplatePdfTests : IDisposable
    {
        private TemplatePdfHandler _templatePdfHandler;
        private IFileSystem _fileSystemMock;
        
        private Mock<ILogger> _loggerMock;
        private Mock<IDebenuPdfLibrary> _debenuMock;
        private IOptions<FolderSettings> _folderSettingsMock;
        private IOptions<GeneralSettings> _generalSettingsMock; 
        private readonly Mock<IMemoryCache> _memoryCacheMock;
        private readonly IFilesWrapper _fileWrapperMock;

        private readonly Mock<IDocumentFileWrapper> _documentFileWrapper;
        private readonly Mock<IContactFileWrapper> _contactFileWrapper;
        private readonly Mock<IUserFileWrapper> _userFileWrapper;
        private readonly Mock<ISignerFileWrapper> _signerFileWrapper;
        private readonly Mock<IConfigurationFileWrapper> _configurationFileWrapper;
        
        public TemplatePdfTests()
        {
            _generalSettingsMock = Options.Create(new GeneralSettings() { DPI = 1, CompressFilesOverSizeBytes = 150 });
            _folderSettingsMock = Options.Create(new FolderSettings() { Templates = @"c:\" });
            _debenuMock = new Mock<IDebenuPdfLibrary>();
            _loggerMock = new Mock<ILogger>();
            _memoryCacheMock = new Mock<IMemoryCache>();
            _fileSystemMock = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"c:\00000000-0000-0000-0000-000000000000\00000000-0000-0000-0000-000000000000.pdf", new MockFileData("") }
            });

            _documentFileWrapper = new Mock<IDocumentFileWrapper>();
            _contactFileWrapper = new Mock<IContactFileWrapper>();
            _userFileWrapper = new Mock<IUserFileWrapper>();
            _signerFileWrapper = new Mock<ISignerFileWrapper>();
            _configurationFileWrapper = new Mock<IConfigurationFileWrapper>();

            _fileWrapperMock = new FileWrapperStub(_documentFileWrapper.Object, _contactFileWrapper.Object, _userFileWrapper.Object, _signerFileWrapper.Object, _configurationFileWrapper.Object);



            
            _templatePdfHandler = new TemplatePdfHandler( _generalSettingsMock, 
                 _loggerMock.Object, _debenuMock.Object,
                null, _memoryCacheMock.Object, _fileWrapperMock, _fileSystemMock);
        }

        public void Dispose()
        {
            _debenuMock.Invocations.Clear();
            _loggerMock.Invocations.Clear();
        }

        [Fact]
        public void Load_ValidFile_Success()
        {
            _debenuMock.Setup(x => x.LoadFromString(It.IsAny<byte[]>(), It.IsAny<string>())).Returns(1);
            _documentFileWrapper.Setup(x => x.IsDocumentExist(DocumentType.Template, It.IsAny<Guid>())).Returns(true);
            var result = _templatePdfHandler.Load(new Guid());

            Assert.True(result);
        }

        [Fact]
        public void Load_InvalidFile_ReturnFalse()
        {
            _debenuMock.Setup(x => x.LoadFromString(It.IsAny<byte[]>(), It.IsAny<string>())).Throws(new Exception());

            var result = _templatePdfHandler.Load(new Guid());

            Assert.False(result);
        }

        [Fact]
        public void Create_ValidFile_Success()
        {
            var file = "MQ==";
            _debenuMock.Setup(x => x.LoadFromString(It.IsAny<byte[]>(), It.IsAny<string>())).Returns(1);
            _documentFileWrapper.Setup(x => x.SaveDocument(DocumentType.Template, It.IsAny<Guid>(), It.IsAny<byte[]>()));
            _documentFileWrapper.Setup(x => x.IsDocumentsImagesWasCreated(DocumentType.Template, It.IsAny<Guid>())).Returns(true);

            
            var result = _templatePdfHandler.Create(new Guid("00000000-0000-0000-0000-000000000001"), file);
            _documentFileWrapper.Verify(x => x.SaveDocument(DocumentType.Template, It.IsAny<Guid>(), It.IsAny<byte[]>()), Times.Once);
            _documentFileWrapper.Verify(x => x.IsDocumentsImagesWasCreated(DocumentType.Template, It.IsAny<Guid>()), Times.Once);
            _debenuMock.Verify(x => x.LoadFromString(It.IsAny<byte[]>(), It.IsAny<string>()), Times.Once);
            
        }

        [Fact]
        public void Create_InvalidFile_ReturnFalse()
        {
            var file = "MQ==";
            _debenuMock.Setup(x => x.LoadFromString(It.IsAny<byte[]>(), It.IsAny<string>())).Throws(new Exception());

            var result = _templatePdfHandler.Create(new Guid(), file);

            Assert.False(result);
        }


    }
}
