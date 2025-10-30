using Common.Handlers;
using Common.Interfaces;
using Common.Interfaces.DB;
using Common.Models;
using Common.Models.Configurations;
using Common.Models.Documents.Signers;
using Common.Models.Settings;
using DAL.Connectors;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Common.Tests.Handlers
{
    public class GenerateLinkHandlerTests
    {

        
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly  Mock<IJWT> _jwtMock;
        private readonly IOptions<GeneralSettings> _generalSettings;
        private readonly IGenerateLinkHandler _generateLinkHandler;
        private readonly Mock<IConfigurationConnector> _configurationConnector;
        private readonly Mock<ISignerTokenMappingConnector> _signerTokenMappingConnector;
        public GenerateLinkHandlerTests()
        {
            _configurationConnector = new Mock<IConfigurationConnector>();
            _signerTokenMappingConnector = new Mock<ISignerTokenMappingConnector>();
            _configurationMock = new Mock<IConfiguration>();
            _jwtMock = new Mock<IJWT>();
            _generalSettings = Options.Create(new GeneralSettings() { SignerFronendApplicationRoute  = "https://wesign.comda.co.il/signer"});
            _generateLinkHandler = new GenerateLinkHandler(_configurationConnector.Object, _signerTokenMappingConnector.Object, _configurationMock.Object, _jwtMock.Object, _generalSettings);
        }

        #region GenerateDocumentDownloadLink

        [Fact]
        public async Task GenerateDocumentDownloadLink_NullSigner_ThrowException()
        {
            User user = null;
            CompanyConfiguration companyConfiguration = null;
            _configurationMock.Setup(x => x.GetSignerLinkExperationTimeInHours(It.IsAny<User>(), It.IsAny<CompanyConfiguration>())).Returns(24);
            DocumentCollection documentCollection = null;
            Signer signer = null;

            var actual = await Assert.ThrowsAsync<Exception>(() => _generateLinkHandler.GenerateDocumentDownloadLink(documentCollection, signer, user, companyConfiguration));

            Assert.Equal("Null input - signer is null", actual.Message);
        }

        [Fact]
        public async Task GenerateDocumentDownloadLink_NullDocumentCollection_ThrowException()
        {
            User user = null;
            CompanyConfiguration companyConfiguration = null;
            _configurationMock.Setup(x => x.GetSignerLinkExperationTimeInHours(It.IsAny<User>(), It.IsAny<CompanyConfiguration>())).Returns(24);
            Signer signer = new Signer
            {
                Id = Guid.NewGuid()
            };
            DocumentCollection documentCollection = null;

            var actual = await  Assert.ThrowsAsync<Exception>(() => _generateLinkHandler.GenerateDocumentDownloadLink(documentCollection, signer, user, companyConfiguration));

            Assert.Equal("Null input - documentCollection is null", actual.Message);
        }

        [Fact]
        public async Task GenerateDocumentDownloadLink_ValidDocumentCollection_Success()
        {
            User user = null;
            CompanyConfiguration companyConfiguration = null;
            _configurationMock.Setup(x => x.GetSignerLinkExperationTimeInHours(It.IsAny<User>(), It.IsAny<CompanyConfiguration>())).Returns(24);
            Signer signer = new Signer
            {
                Id = Guid.NewGuid()
            };
            DocumentCollection documentCollection = new DocumentCollection { };
            _signerTokenMappingConnector.Setup(x => x.Delete(It.IsAny<SignerTokenMapping>()));

            var actual =await _generateLinkHandler.GenerateDocumentDownloadLink(documentCollection, signer, user, companyConfiguration);

            Assert.StartsWith($"{_generalSettings.Value.SignerFronendApplicationRoute}/download/", actual.Link);
        }

        #endregion

        #region GenerateSigningLink

        [Fact]
        public async Task GenerateSigningLink_NullInput_ReturnEmptyList()
        {
            User user = null;
            CompanyConfiguration companyConfiguration = null;
            DocumentCollection documentCollection = null;
            _configurationConnector.Setup(x => x.Read()).ReturnsAsync(new Configuration());

            var actual = await _generateLinkHandler.GenerateSigningLink(documentCollection, user, companyConfiguration);

            Assert.NotNull(actual);
            Assert.True(!actual.Any());
        }

        [Fact]
        public async Task GenerateSigningLink_DocumentCollectionWithoutSigners_ReturnEmptyList()
        {
            User user = null;
            CompanyConfiguration companyConfiguration = null;
            DocumentCollection documentCollection = new DocumentCollection { };
            _configurationConnector.Setup(x => x.Read()).ReturnsAsync(new Configuration());

            var actual =await _generateLinkHandler.GenerateSigningLink(documentCollection, user, companyConfiguration);

            Assert.NotNull(actual);
            Assert.True(actual.Count() == 0);
        }

        [Fact]
        public async Task GenerateSigningLink_DocumentCollectionWithSigners_Success()
        {
            User user = null;
            CompanyConfiguration companyConfiguration = null;
            DocumentCollection documentCollection = new DocumentCollection 
            {
                Signers = new List<Signer> { new Signer { } }
            };
            _signerTokenMappingConnector.Setup(x => x.Delete(It.IsAny<SignerTokenMapping>()));
            _configurationConnector.Setup(x => x.Read()).ReturnsAsync(new Configuration());

            var actual = await _generateLinkHandler.GenerateSigningLink(documentCollection, user, companyConfiguration);

            Assert.True(actual.Count() == 1);
            Assert.StartsWith($"{_generalSettings.Value.SignerFronendApplicationRoute}/signature/", actual.First().Link);
        }
       
        #endregion


        //TODO add unit tests handling roll back scenario
    }
}
