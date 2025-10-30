using BL.Handlers;
using Common.Enums.Contacts;
using Common.Enums.Documents;
using Common.Handlers.Files;
using Common.Interfaces;
using Common.Interfaces.DB;
using Common.Interfaces.Files;
using Common.Models;
using Common.Models.Documents.Signers;
using Common.Models.Settings;
using iTextSharp.text;
using Microsoft.Extensions.Options;
using Moq;
using Org.BouncyCastle.Crypto;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace BL.Tests
{
    public class DoneDocumentsHandlerTests : IDisposable
    {
        private readonly Mock<IDater> _dater;
        
        private readonly Mock<IDocumentCollectionConnector> _documentCollectionConnector;
        private readonly Mock<IOptions<GeneralSettings>> _generalSettingsOptions;
        private readonly Mock<GeneralSettings> _generalSettings;
        
        private readonly IFilesWrapper _fileWrapper;
        private readonly Mock<Signer> _signer;
        private readonly string _installerPath;
        private readonly DoneDocumentsHandler _doneDocumentsHandler;




        private readonly Mock<IDocumentFileWrapper> _documentFileWrapper;
        private readonly Mock<IContactFileWrapper> _contactFileWrapper;
        private readonly Mock<IUserFileWrapper> _userFileWrapper;
        private readonly Mock<ISignerFileWrapper> _signerFileWrapper;

        private readonly Mock<IConfigurationFileWrapper> _configurationFileWrapper;
        public DoneDocumentsHandlerTests()
        {
            _dater = new Mock<IDater>();
            
            _documentCollectionConnector = new Mock<IDocumentCollectionConnector>();
            
            _generalSettings = new Mock<GeneralSettings>();
            _generalSettingsOptions = new Mock<IOptions<GeneralSettings>>();
            _generalSettingsOptions.Setup(x => x.Value).Returns(_generalSettings.Object);

            _documentFileWrapper = new Mock<IDocumentFileWrapper>();
            _contactFileWrapper = new Mock<IContactFileWrapper>();
            _userFileWrapper = new Mock<IUserFileWrapper>();
            _signerFileWrapper = new Mock<ISignerFileWrapper>();
            _configurationFileWrapper = new Mock<IConfigurationFileWrapper>();

            _fileWrapper = new FileWrapperStub(_documentFileWrapper.Object, _contactFileWrapper.Object, _userFileWrapper.Object, _signerFileWrapper.Object, _configurationFileWrapper.Object);
            _signer = new Mock<Signer>();
            _installerPath = _generalSettings.Object.SmartCardDesktopClientInstallerPath;
            _doneDocumentsHandler = new DoneDocumentsHandler(_dater.Object, _documentCollectionConnector.Object, _fileWrapper);
        }

        public void Dispose()
        {
            _dater.Invocations.Clear();
            
            _documentCollectionConnector.Invocations.Clear();
            _generalSettingsOptions.Invocations.Clear();
            _generalSettings.Invocations.Clear();
            
            _signer.Invocations.Clear();
        }

        #region DoneProcess

        [Fact]
        public async Task DoneProcess_NotAllSignersSigned_ShouldNotSign()
        {
            // Arrange
            Signer dbSigner = new Signer();
            Signer signer = new Signer()
            { Status = SignerStatus.Rejected };
            List<Signer> signers = new List<Signer>() { signer };
            DocumentCollection documentCollection = new DocumentCollection()
            { Signers = signers };

            // Action
            await _doneDocumentsHandler.DoneProcess(documentCollection, dbSigner);

            // Assert
            Assert.NotEqual(DocumentStatus.Signed, documentCollection.DocumentStatus);
        }

        [Fact]
        public async Task DoneProcess_AllSignersSigned_ShouldSign()
        {
            // Arrange
            Signer dbSigner = new Signer();
            Signer signer = new Signer()
            { Status = SignerStatus.Signed };
            List<Signer> signers = new List<Signer>() { signer };
            DocumentCollection documentCollection = new DocumentCollection()
            { Signers = signers };

            // Action
            await _doneDocumentsHandler.DoneProcess(documentCollection, dbSigner);

            // Assert
            Assert.Equal(DocumentStatus.Signed, documentCollection.DocumentStatus);
        }

        #endregion

        #region DownloadSmartCardDesktopClientInstaller

        [Fact]
        public void DownloadSmartCardDesktopClientInstaller_NotFoundInstaller_ThrowException()
        {
            // Arrange
            string exceptionMessage = "not exist";
            _signerFileWrapper.Setup(x => x.GetSmartCardDesktopClientInstaller()).Throws<Exception>(() => throw new Exception(exceptionMessage));
            

            // Action
            var actual = Assert.Throws<Exception>(() => _doneDocumentsHandler.DownloadSmartCardDesktopClientInstaller());

            // Assert
            _signerFileWrapper.Verify(x => x.GetSmartCardDesktopClientInstaller(), Times.Once);
            Assert.Equal(exceptionMessage, actual.Message);
        }

        [Fact]
        public void DownloadSmartCardDesktopClientInstaller_FoundInstaller_ShouldSuccessReadAllBytes()
        {
            // Arrange
            byte[] currentBytes = null;
            byte[] requiredBytes = new byte[5];
            Random random = new Random();
            random.NextBytes(requiredBytes);
            _signerFileWrapper.Setup(x => x.GetSmartCardDesktopClientInstaller()).Returns(requiredBytes);
          

            // Action
            currentBytes =  _doneDocumentsHandler.DownloadSmartCardDesktopClientInstaller();

            // Assert
            _signerFileWrapper.Verify(x => x.GetSmartCardDesktopClientInstaller(), Times.Once);
            
            Assert.Equal(currentBytes, requiredBytes);
        }

        #endregion
    }
}
