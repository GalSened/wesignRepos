using BL.Tests;
using BL.Tests.Services;
using Certificate.Interfaces;
using Common.Enums.Results;
using Common.Extensions;
using Common.Handlers;
using Common.Interfaces;
using Common.Interfaces.Files;
using Common.Models;
using Common.Models.Configurations;
using Common.Models.Settings;
using Common.Tests.Handlers;
using crypto;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Moq;
using Serilog;
using System;
using System.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Runtime.Caching;
using Xunit;


namespace Common.Tests.Handlers
{
    public class CertificatesHandlerTests
    {
        private readonly Guid ID = new Guid("C32BCF3A-C273-4F98-B002-A724DE1479FE");
        private readonly IFileSystem _fileSystemMock;
        private readonly Mock<ILogger> _logger;
        private IOptions<FolderSettings> _folderSettings;
        private IOptions<GeneralSettings> _generalSettings;
        private ICertificate _certificate;
        private readonly Mock<ICertificateCreator> _certificateCreator;
        private readonly IFilesWrapper _fileWrapperMock;

        private readonly Mock<IDocumentFileWrapper> _documentFileWrapper;
        private readonly Mock<IContactFileWrapper> _contactFileWrapper;
        private readonly Mock<IUserFileWrapper> _userFileWrapper;
        private readonly Mock<ISignerFileWrapper> _signerFileWrapper;
        private readonly Mock<IConfigurationFileWrapper> _configurationFileWrapper;
        private readonly Mock<IMemoryCache> _memoryCache;

        public CertificatesHandlerTests()
        {
            _logger = new Mock<ILogger>();
            _fileSystemMock = new MockFileSystem();
            _generalSettings = Options.Create(new GeneralSettings { CA = "" });
            _folderSettings = Options.Create(new FolderSettings { ContactCertificates = "c:\\comda\\contactsCerts" });
            _folderSettings = Options.Create(new FolderSettings { });
            _certificateCreator = new Mock<ICertificateCreator>();
            _documentFileWrapper = new Mock<IDocumentFileWrapper>();
            _contactFileWrapper = new Mock<IContactFileWrapper>();
            _userFileWrapper = new Mock<IUserFileWrapper>();
            _signerFileWrapper = new Mock<ISignerFileWrapper>();
            _configurationFileWrapper = new Mock<IConfigurationFileWrapper>();
            _memoryCache = new Mock<IMemoryCache>();
            _fileWrapperMock = new FileWrapperStub(_documentFileWrapper.Object, _contactFileWrapper.Object, _userFileWrapper.Object,
                _signerFileWrapper.Object, _configurationFileWrapper.Object);



        }

        #region Create_Contant_Input


       

        [Fact]
        public void Create_NullContact_ThrowException()
        {
            Contact contact = null;
            CompanyConfiguration companyConfiguration = new CompanyConfiguration()
            {
                IsPersonzliedPFX = true
            };
            _generalSettings = Options.Create(new GeneralSettings { CA = "192.168.0.99\\WESIGN-CA" });
            
             _certificate = new CertificatesHandler( _generalSettings, _logger.Object, _certificateCreator.Object,
                _fileWrapperMock, _memoryCache.Object);

            var actual = Assert.Throws<InvalidOperationException>(() => _certificate.Create(contact, companyConfiguration));

            Assert.Equal(ResultCode.InvalidContactId.GetNumericString(), actual.Message);
        }

        [Fact]
        public void Create_InvalidContactId_ThrowException()
        {
            CompanyConfiguration companyConfiguration = new CompanyConfiguration()
            {
                IsPersonzliedPFX = true
            };
            var contact = new Contact { Id = Guid.Empty };
            _generalSettings = Options.Create(new GeneralSettings { CA = "192.168.0.99\\WESIGN-CA" });
            _certificate = new CertificatesHandler(_generalSettings, _logger.Object, _certificateCreator.Object,
                _fileWrapperMock, _memoryCache.Object);

            var actual = Assert.Throws<InvalidOperationException>(() => _certificate.Create(contact, companyConfiguration));

            Assert.Equal(ResultCode.InvalidContactId.GetNumericString(), actual.Message);
        }

        [Fact]
        public void Create_ValidContactCertificateExist_NothingShouldHappened()
        {
            CompanyConfiguration companyConfiguration = new CompanyConfiguration();
            var contact = new Contact { Id = ID, Name = "ContactName" };
            _generalSettings = Options.Create(new GeneralSettings { CA = "192.168.0.99\\WESIGN-CA" });
            _folderSettings = Options.Create(new FolderSettings { ContactCertificates = "c:\\comda\\certificates\\contacts" });
            string filePath = Path.Combine(_folderSettings.Value.ContactCertificates, $"{ID}.pfx");
            ((MockFileSystem)_fileSystemMock)?.AddFile(filePath, new MockFileData("textContent"));
            _contactFileWrapper.Setup(x => x.IsCertificateExist(It.IsAny<Contact>())).Returns(true);
            _certificate = new CertificatesHandler(_generalSettings, _logger.Object, _certificateCreator.Object,
               _fileWrapperMock, _memoryCache.Object);

            _certificate.Create(contact, companyConfiguration);
        }

        //[Fact]
        //public void Create_ValidContactCertificateNotExist_NothingShouldHappened()
        //{
        //    //NEED TO FIX
        //    //
        //    //CompanyConfiguration companyConfiguration = new CompanyConfiguration();
        //    //var contact = new Contact { Id = ID, Name = "ContactName" };
        //    //_generalSettings = Options.Create(new GeneralSettings { CA = @"192.168.0.99\\WESIGN-CA", CryptographicServiceEProvider = "crypto" }); //CryptographicServiceEProvider is invalid
        //    //_folderSettings = Options.Create(new FolderSettings { ContactCertificates = @"c:\\comda\\certificates\\contacts" });
        //    //_certificate = new CertificatesHandler(_fileSystemMock, _folderSettings, _generalSettings, _logger.Object, _certificateCreator.Object);

        //    //_certificate.Create(contact, companyConfiguration);
        //}

        #endregion

        #region Delete_Contact

        [Fact]
        public void Delete_NullContact_NothingShouldHappened()
        {
            Contact contact = null;
            _folderSettings = Options.Create(new FolderSettings { ContactCertificates = "c:\\comda\\certificates\\contacts" });
            _certificate = new CertificatesHandler(_generalSettings, _logger.Object, _certificateCreator.Object,
               _fileWrapperMock, _memoryCache.Object);

            _certificate.Delete(contact);
        }

        [Fact]
        public void Delete_ValidContactFileNotExist_NothingShouldHappened()
        {
            var contact = new Contact { Id = ID, Name = "ContactName" };
            string path = $"c:\\comda\\contactsCerts\\{ID}.pfx";
            _folderSettings = Options.Create(new FolderSettings { ContactCertificates = "c:\\comda\\certificates\\contacts" });
            _certificate = new CertificatesHandler(_generalSettings, _logger.Object, _certificateCreator.Object,
               _fileWrapperMock, _memoryCache.Object);

            _certificate.Delete(contact);
        }

        [Fact]
        public void Delete_ValidContactFileExists_FileWillBeDeleted()
        {
            var contact = new Contact { Id = ID, Name = "ContactName" };
            
            bool certDeleted = false;
            _contactFileWrapper.Setup(x => x.IsCertificateExist(It.IsAny<Contact>())).Returns(true);
            _contactFileWrapper.Setup(x => x.DeleteCertificate(It.IsAny<Contact>())).
                Callback(() => certDeleted = true);
            

            _certificate = new CertificatesHandler(_generalSettings, _logger.Object, _certificateCreator.Object,
               _fileWrapperMock, _memoryCache.Object);

            _certificate.Delete(contact);

            Assert.True(certDeleted);
        }

        #endregion

        #region Get_Contact

        [Fact]
        public void Get_NullInputShouldUseGlobalCert_ReturnGlobalCert()
        {
            CompanyConfiguration companyConfiguration = new CompanyConfiguration();
            Contact contact = null;
            
            _memoryCache.Setup(x => x.CreateEntry(It.IsAny<string>())).Returns(Mock.Of<ICacheEntry>());
            _certificate = new CertificatesHandler(_generalSettings, _logger.Object, _certificateCreator.Object,
               _fileWrapperMock , _memoryCache.Object);

            var actucal = _certificate.Get(contact, companyConfiguration);

            Assert.NotNull(actucal);
        }

        [Fact]
        public void Get_NullInput_ThrowException()
        {
            CompanyConfiguration companyConfiguration = new CompanyConfiguration();
            Contact contact = null;
            _generalSettings = Options.Create(new GeneralSettings { CA = "192.168.0.99\\WESIGN-CA", CryptographicServiceEProvider = "crypto" });
            _folderSettings = Options.Create(new FolderSettings { ContactCertificates = "c:\\comda\\certificates\\contacts" });
            _certificate = new CertificatesHandler(_generalSettings, _logger.Object, _certificateCreator.Object,
               _fileWrapperMock  , _memoryCache.Object);

            var actual = Assert.Throws<InvalidOperationException>(() => _certificate.Get(contact, companyConfiguration));

            Assert.Equal(ResultCode.InvalidContactId.GetNumericString(), actual.Message);
        }

        // [Fact]
        //public void Get_ValidContactCreateCert_()
        //{
        //    var contact = new Contact { Id = ID, Name = "ContactName" };
        //    _generalSettings = Options.Create(new GeneralSettings { CA = "192.168.0.99\\WESIGN-CA", CryptographicServiceEProvider = "crypto" });
        //    _folderSettings = Options.Create(new FolderSettings { ContactCertificates = "c:\\comda\\certificates\\contacts" });
        //    ((MockFileSystem)_fileSystemMock).AddFile(@"c:\comda\certificates\contacts\c32bcf3a-c273-4f98-b002-a724de1479fe.pfx", new MockFileData("pfx"));
        //    _certificate = new CertificatesHandler(_fileSystemMock, _folderSettings, _generalSettings, _logger.Object);

        //    var actucal = _certificate.Get(contact);
        //    //TODO add to file system mock , valid pfx

        //    //Assert.Equal(ResultCode.InvalidContactId.GetNumericString(), actual.Message);
        //}




        #endregion

    }
}
