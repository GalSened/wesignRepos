using Common.Interfaces.DB;
using Common.Models.Settings;
using Common.Interfaces.SignerApp;
using Common.Interfaces;
using SignerBL.Handlers;

using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using Moq;
using Microsoft.Extensions.Options;
using Xunit;
using Common.Models.Documents.Signers;
using Common.Models;
using System.IO;
using System.Linq;
using Comda.Authentication.Models;
using Common.Models.Users;
using Common.Enums.Results;
using Common.Extensions;
using Common.Models.FileGateScanner;
using Common.Enums;
using Microsoft.Extensions.Caching.Memory;
using System.Runtime.Caching;
using System.Threading.Tasks;

namespace SignerBL.Tests
{
    public class ContactsHandlerTests : IDisposable
    {
        private const string GUID = "C14E4B8B-5970-4B50-A1F1-090B8F99D3B2";

        private readonly IOptions<FolderSettings> _options;

        
        private readonly Mock<IJWT> _jwtMock;
        private readonly Mock<IFileSystem> _fileSystemMock;
        private readonly Mock<IDataUriScheme> _dataUriSchemeMock;
        private readonly Mock<Common.Interfaces.SignerApp.ISignerValidator> _validatorMock;
        private readonly Mock<IContactSignatures> _contactSignatures;
        private readonly Common.Interfaces.SignerApp.IContacts _contactHandler;
        private readonly Mock<IMemoryCache> _memoryCache;

        private readonly Mock<ISignerTokenMappingConnector> _signerTokenMappingConnectorMock;
        private readonly Mock<IDocumentCollectionConnector> _documentCollectionConnectorMock;

        public void Dispose()
        {
            _signerTokenMappingConnectorMock.Invocations.Clear();
            _documentCollectionConnectorMock.Invocations.Clear();
            _jwtMock.Invocations.Clear();
            _fileSystemMock.Invocations.Clear();
            _dataUriSchemeMock.Invocations.Clear();
            _validatorMock.Invocations.Clear();
            _contactSignatures.Invocations.Clear();
            _memoryCache.Invocations.Clear();

        }

        public ContactsHandlerTests()
        {
            _signerTokenMappingConnectorMock = new Mock<ISignerTokenMappingConnector>();
            _documentCollectionConnectorMock = new Mock<IDocumentCollectionConnector>();
            _jwtMock = new Mock<IJWT>(MockBehavior.Loose);
            _fileSystemMock = new Mock<IFileSystem>();
            _dataUriSchemeMock = new Mock<IDataUriScheme>();
            _validatorMock = new Mock<Common.Interfaces.SignerApp.ISignerValidator>();
            _contactSignatures = new Mock<IContactSignatures>();
            _memoryCache = new Mock<IMemoryCache>();
            _options = Options.Create(new FolderSettings()
            {
                ContactSignatureImages = @"c:\",
            }
            );

            _contactHandler = new ContactsHandler(_signerTokenMappingConnectorMock.Object, _documentCollectionConnectorMock.Object,_jwtMock.Object,  _validatorMock.Object,
                _contactSignatures.Object/*, _memoryCache.Object*/);
        }


        #region Read Signatures Images

        [Fact]
        public async Task ReadSignaturesImages_SignerTokenMapperNotExistInDB_ThrowException()
        {
            // Arrange
            SignerTokenMapping signerTokenMapping = null;
            _signerTokenMappingConnectorMock.Setup(x => x.Read(It.IsAny<SignerTokenMapping>())).ReturnsAsync(signerTokenMapping);

            // Act
            var actual =await Assert.ThrowsAsync<InvalidOperationException>(() => _contactHandler.ReadSignaturesImages(signerTokenMapping));

            // Assert
            Assert.Equal(ResultCode.InvalidToken.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task ReadSignaturesImages_SignerIsNull_ThrowException()
        {
            // Arrange
            SignerTokenMapping signerTokenMapping = new SignerTokenMapping();
            Signer signer = null;
            _jwtMock.Setup(x => x.GetSigner(It.IsAny<string>())).Returns(signer);
            _signerTokenMappingConnectorMock.Setup(x => x.Read(It.IsAny<SignerTokenMapping>())).ReturnsAsync(signerTokenMapping);

            // Act
            var actual =await Assert.ThrowsAsync<InvalidOperationException>(() => _contactHandler.ReadSignaturesImages(signerTokenMapping));

            // Assert
            Assert.Equal(ResultCode.InvalidToken.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task ReadSignaturesImages_DocumentIsNull_ThrowException()
        {
            // Arrange
            _jwtMock.DefaultValue = DefaultValue.Mock;
            SignerTokenMapping signerTokenMapping = new SignerTokenMapping();
            Signer signer = new Signer();
            DocumentCollection documentCollection = null;
            _signerTokenMappingConnectorMock.Setup(x => x.Read(It.IsAny<SignerTokenMapping>())).ReturnsAsync(signerTokenMapping);
            _jwtMock.Setup(x => x.GetSigner(It.IsAny<string>())).Returns(signer);
            _documentCollectionConnectorMock.Setup(x => x.Read(It.IsAny<DocumentCollection>())).ReturnsAsync(documentCollection);
            

            _signerTokenMappingConnectorMock.Setup(x => x.Read(It.IsAny<SignerTokenMapping>())).ReturnsAsync(signerTokenMapping);
            //_dbConnectorMock.Setup(x => x.DocumentCollections.ReadWithDocumentsSignerAndContact(It.IsAny<DocumentCollection>())).Returns(documentCollection);
            // Act
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _contactHandler.ReadSignaturesImages(signerTokenMapping));

            // Assert
            Assert.Equal(ResultCode.InvalidDocumentCollectionId.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task ReadSignaturesImages_ContactIsNull_ThrowException()
        {
            // Arrange
            SignerTokenMapping signerTokenMapping = new SignerTokenMapping();
            _jwtMock.DefaultValue = DefaultValue.Mock;
            

            DocumentCollection documentCollection = new DocumentCollection();
            _documentCollectionConnectorMock.Setup(x => x.Read(It.IsAny<DocumentCollection>())).ReturnsAsync(documentCollection);

            // Act
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _contactHandler.ReadSignaturesImages(signerTokenMapping));

            // Assert
            Assert.Equal(ResultCode.InvalidToken.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task ReadSignaturesImages_FolderPathNotExist_ReturnEmptyList()
        {
            // Arrange
            _jwtMock.DefaultValue = DefaultValue.Mock;
            
            _fileSystemMock.DefaultValue = DefaultValue.Mock;

            SignerTokenMapping signerTokenMapping = new SignerTokenMapping();
            var signersList = new List<Signer>();

            Contact contact = new Contact()
            {
                Id = Guid.NewGuid()
            };

            var list = new List<string>();

            Signer signer = new Signer()
            {
                Id = Guid.Parse(GUID),
                Contact = contact
            };
            signersList.Add(signer);
            DocumentCollection documentCollection = new DocumentCollection()
            {
                Signers = signersList,
            };
            _signerTokenMappingConnectorMock.Setup(x => x.Read(It.IsAny<SignerTokenMapping>())).ReturnsAsync(signerTokenMapping);
            _contactSignatures.Setup(x => x.GetContactSavedSignatures(It.IsAny<Contact>())).Returns(list);
            _signerTokenMappingConnectorMock.Setup(x => x.Read(It.IsAny<SignerTokenMapping>())).ReturnsAsync(signerTokenMapping);
            _documentCollectionConnectorMock.Setup(x => x.Read(It.IsAny<DocumentCollection>())).ReturnsAsync(documentCollection);
            //_dbConnectorMock.Setup(x => x.DocumentCollections.ReadWithDocumentsSignerAndContact(It.IsAny<DocumentCollection>())).Returns(documentCollection);
            _jwtMock.Setup(x => x.GetSigner(It.IsAny<string>())).Returns(signer);
            _fileSystemMock.Setup(x => x.Directory.Exists(It.IsAny<string>())).Returns(false);


            // Act
            var actual = await _contactHandler.ReadSignaturesImages(signerTokenMapping);

            // Assert
            Assert.Equal(list, actual);
            Assert.Empty(actual);
        }

        [Fact]
        public async Task ReadSignaturesImages_FolderPathExist_Success()
        {
            // Arrange
            _jwtMock.DefaultValue = DefaultValue.Mock;
            
            _fileSystemMock.DefaultValue = DefaultValue.Mock;

            SignerTokenMapping signerTokenMapping = new SignerTokenMapping();
            var signersList = new List<Signer>();

            Contact contact = new Contact()
            {
                Id = Guid.NewGuid()
            };


            Signer signer = new Signer()
            {
                Id = Guid.Parse(GUID),
                Contact = contact
            };
            signersList.Add(signer);
            DocumentCollection documentCollection = new DocumentCollection()
            {
                Signers = signersList,
            };

            var returnedList = new List<string>();
            var path = @"D:\";
            var images = new string[]
            {
                "d:\\ASdcasdc.png",
                "d:\\349jmsdca.jpeg",
                "d:\\KOKO\\SHMOKO\\asdjc234r.png"
            };
            _signerTokenMappingConnectorMock.Setup(x => x.Read(It.IsAny<SignerTokenMapping>())).ReturnsAsync(signerTokenMapping);
            _documentCollectionConnectorMock.Setup(x => x.Read(It.IsAny<DocumentCollection>())).ReturnsAsync(documentCollection);
            //_dbConnectorMock.Setup(x => x.DocumentCollections.ReadWithDocumentsSignerAndContact(It.IsAny<DocumentCollection>())).Returns(documentCollection);
            _jwtMock.Setup(x => x.GetSigner(It.IsAny<string>())).Returns(signer);
            _signerTokenMappingConnectorMock.Setup(x => x.Read(It.IsAny<SignerTokenMapping>())).ReturnsAsync(signerTokenMapping);
            _fileSystemMock.Setup(x => x.Path.Combine(It.IsAny<string[]>())).Returns(path);
            _fileSystemMock.Setup(x => x.Directory.Exists(It.IsAny<string>())).Returns(true);
            _fileSystemMock.Setup(x => x.Directory.GetFiles(It.IsAny<string>())).Returns(images);
            _contactSignatures.Setup(x => x.GetContactSavedSignatures(It.IsAny<Contact>())).Returns(images.ToList());
            _fileSystemMock.Setup(x => x.Path.GetExtension(It.IsAny<string>())).Returns(string.Empty);
            _fileSystemMock.Setup(x => x.File.ReadAllBytes(It.IsAny<string>())).Returns(new byte[2]);
            // Act
            var actual = await _contactHandler.ReadSignaturesImages(signerTokenMapping);

            // Assert
            Assert.Equal(images.Count(), actual.Count());
            Assert.NotEmpty(actual);
        }

        #endregion

        #region Update Signatures Images

        //New Handler created need to move this function.
        //[Fact]
        //public void UpdateSignaturesImages_FileIsNotClean_ThrowException()
        //{
        //    // Arrange
        //    _jwtMock.DefaultValue = DefaultValue.Mock;
        //    _dbConnectorMock.DefaultValue = DefaultValue.Mock;
        //    _fileSystemMock.DefaultValue = DefaultValue.Mock;

        //    var signersList = new List<Signer>();
        //    Contact contact = new Contact()
        //    {
        //        Id = Guid.NewGuid()
        //    };


        //    Signer signer = new Signer()
        //    {
        //        Id = Guid.Parse(GUID),
        //        Contact = contact
        //    };
        //    signersList.Add(signer);
        //    DocumentCollection documentCollection = new DocumentCollection()
        //    {
        //        Signers = signersList,
        //    };


        //    SignerTokenMapping signerTokenMapping = new SignerTokenMapping();
        //    var signaturesImages = new List<string>()
        //    {
        //        "Sdckljsc",
        //        "sadlkfj234"
        //    };
        //    FileGateScanResult scanResult = new FileGateScanResult()
        //    {
        //        IsValid = true,
        //        CleanFile = ""
        //    };

        //    _dbConnectorMock.Setup(x => x.DocumentCollections.Read(It.IsAny<DocumentCollection>())).Returns(documentCollection);
        //    _jwtMock.Setup(x => x.GetSigner(It.IsAny<string>())).Returns(signer);
        //    _validatorMock.Setup(x => x.ValidateIsCleanFile(It.IsAny<string>())).Returns(scanResult);
        //    _fileSystemMock.Setup(x => x.Path.Combine(It.IsAny<string>())).Returns(It.IsAny<string>());
        //    _fileSystemMock.Setup(x => x.Directory.Exists(It.IsAny<string>())).Returns(false);

        //    ImageType imageType;

        //    _dataUriSchemeMock.Setup(x => x.IsValidImageType(It.IsAny<string>(), out imageType)).Returns(false);


        //    // Act
        //    var actual = Assert.Throws<InvalidOperationException>(() => _contactHandler.UpdateSignaturesImages(signerTokenMapping, signaturesImages));

        //    // Assert
        //    Assert.Equal(ResultCode.NotSupportedImageFormat.GetNumericString(), actual.Message);
        //}

        [Fact]
        public async Task UpdateSignaturesImages_ContactIsNull_ThrowException()
        {
            // Arrange
            _jwtMock.DefaultValue = DefaultValue.Mock;
            
            SignerTokenMapping signerTokenMapping = new SignerTokenMapping();
            Signer signer = new Signer();
            DocumentCollection documentCollection = new DocumentCollection();

            _signerTokenMappingConnectorMock.Setup(x => x.Read(It.IsAny<SignerTokenMapping>())).ReturnsAsync(signerTokenMapping);
            _jwtMock.Setup(x => x.GetSigner(It.IsAny<string>())).Returns(signer);
            _documentCollectionConnectorMock.Setup(x => x.Read(It.IsAny<DocumentCollection>())).ReturnsAsync(documentCollection);

            // Act
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _contactHandler.UpdateSignaturesImages(signerTokenMapping, new List<string>()));

            // Assert
            Assert.Equal(ResultCode.InvalidSignerId.GetNumericString(), actual.Message);
        }

        #endregion
    }
}
