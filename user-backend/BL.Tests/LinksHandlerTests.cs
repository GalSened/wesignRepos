using Common.Interfaces.DB;
using Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using Serilog;
using Moq;
using BL.Handlers;
using Xunit;
using Common.Models.Configurations;
using Common.Models;
using Common.Models.Documents.Signers;
using System.Linq;
using System.Threading.Tasks;
using Common.Handlers.SendingMessages;
using Common.Handlers;
using Common.Interfaces.MessageSending;
using Common.Models.Links;
using Common.Enums.Results;
using Common.Extensions;


namespace BL.Tests
{
    public class LinksHandlerTests : IDisposable
    {

        
        private readonly Guid FREE_ACCOUNTS_COMPANY_ID = new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1);

        private readonly Mock<IUsers> _usersMock;
        private readonly Mock<IGenerateLinkHandler> _generateLinkHandlerMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<ILogger> _loggerMock;
             
        private readonly Mock<IProgramUtilizationConnector> _programUtilizationConnectorMock;
        private readonly Mock<IDocumentCollectionConnector> _documentCollectionConnector;
        private readonly Mock<ICompanyConnector> _companyConnectorMock;


        private readonly Mock<IProgramConnector> _programConnectorMock;
        private readonly Mock<ISendingMessageHandler> _sendingMessageHandlerMock;
        private readonly Mock<IVideoConfrence> _videoConfrenceMock;
        private readonly Mock<IConfigurationConnector> _configurationConnectorMock;


        private readonly Mock<IValidator> _validatorMock;
        private readonly Mock<ITemplateConnector> _templateConnectorMock;
        


        private readonly ILinks _linksHandler;



        public void Dispose()
        {
            _documentCollectionConnector.Invocations.Clear();
            _companyConnectorMock.Invocations.Clear();
            _usersMock.Invocations.Clear();
            _generateLinkHandlerMock.Invocations.Clear();
            _configurationMock.Invocations.Clear();
            _loggerMock.Invocations.Clear();
            _programUtilizationConnectorMock.Invocations.Clear();
            _programConnectorMock.Invocations.Clear();
            _sendingMessageHandlerMock.Invocations.Clear();
            _videoConfrenceMock.Invocations.Clear();
            _configurationConnectorMock.Invocations.Clear();
            _validatorMock.Invocations.Clear();
            _templateConnectorMock.Invocations.Clear();


        }

        public LinksHandlerTests()
        {
            _documentCollectionConnector = new Mock<IDocumentCollectionConnector>();
            _companyConnectorMock = new Mock<ICompanyConnector>();
            _usersMock = new Mock<IUsers>();
            _generateLinkHandlerMock = new Mock<IGenerateLinkHandler>();
            _configurationMock = new Mock<IConfiguration>();
            _loggerMock = new Mock<ILogger>();
            _programUtilizationConnectorMock = new Mock<IProgramUtilizationConnector>();


            _programConnectorMock = new Mock<IProgramConnector>();
            _sendingMessageHandlerMock= new  Mock<ISendingMessageHandler>();
            _videoConfrenceMock = new Mock<IVideoConfrence>();
            _configurationConnectorMock = new Mock<IConfigurationConnector>();
            _validatorMock= new Mock<IValidator>();
            _templateConnectorMock = new Mock<ITemplateConnector>();
            _linksHandler = new LinksHandler(_documentCollectionConnector.Object, _companyConnectorMock.Object, _usersMock.Object, _generateLinkHandlerMock.Object,
                _programConnectorMock.Object, _programUtilizationConnectorMock.Object, _loggerMock.Object, _configurationMock.Object, _videoConfrenceMock.Object, _sendingMessageHandlerMock.Object,
                _configurationConnectorMock.Object, _validatorMock.Object, _templateConnectorMock.Object);

        }

        #region Read

        [Fact]
        public void Read_NoDocs_ReturnEmpty()
        {
            User user = new User();
            CompanySigner1Details companySigner1Details = new CompanySigner1Details();
            Company company = new Company()
            {
                CompanyConfiguration = new CompanyConfiguration()
                {

                }
            };
            IEnumerable<DocumentCollection> documentCollections = new List<DocumentCollection>();
            int totalCount;
            Configuration configuration = new Configuration();


            _usersMock.Setup(x => x.GetUser()).ReturnsAsync((user, companySigner1Details));
            _companyConnectorMock.Setup(x => x.Read(It.IsAny<Company>())).ReturnsAsync(company);
            _documentCollectionConnector.Setup(x => x.ReadBySignerEmail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), out totalCount)).Returns(documentCollections);

            _configurationMock.Setup(x => x.GetSignerLinkExperationTimeInHours(user, company.CompanyConfiguration)).Returns(24);
            _configurationMock.Setup(x => x.ReadAppConfiguration()).ReturnsAsync(configuration);
        }

        [Fact]
        public async Task Read_ReturnDocsLinkTupleList_Success()
        {
            User user = new User()
            {
                Email = "email"
            };
            CompanySigner1Details companySigner1Details = new CompanySigner1Details();
            Company company = new Company()
            {
                CompanyConfiguration = new CompanyConfiguration()
                {

                }
            };
            IEnumerable<DocumentCollection> documentCollections = new List<DocumentCollection>()
            {
                new DocumentCollection()
                {
                    Mode = Common.Enums.Documents.DocumentMode.Online,
                    Signers = new List<Signer>()
                    {
                        new Signer()
                        {
                            Contact = new Contact()
                            {
                                Email = "email"
                            },
                            SendingMethod = Common.Enums.Documents.SendingMethod.Email
                        }
                    }
                }
            };
            int totalCount;
            Configuration configuration = new Configuration();

            List<(DocumentCollection DocumentCollection, string SigningLink)> expected = new List<(DocumentCollection DocumentCollection, string SigningLink)>();

            SignerLink signerLink = new SignerLink()
            {
                Link = "link"
            };

            _usersMock.Setup(x => x.GetUser()).ReturnsAsync((user, companySigner1Details));
            _companyConnectorMock.Setup(x => x.Read(It.IsAny<Company>())).ReturnsAsync(company);
            _documentCollectionConnector.Setup(x => x.ReadBySignerEmail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), 
                out totalCount)).Returns(documentCollections);

            _configurationMock.Setup(x => x.GetSignerLinkExperationTimeInHours(user, company.CompanyConfiguration)).Returns(24);
            _configurationMock.Setup(x => x.ReadAppConfiguration()).ReturnsAsync(configuration);

            _generateLinkHandlerMock.Setup(x => x.GenerateSigningLinkToSingleSigner(documentCollections.First(), It.IsAny<bool>(),
                It.IsAny<int>(), It.IsAny<Configuration>(), It.IsAny<Signer>())).ReturnsAsync(signerLink).Callback(() =>
                {
                    (DocumentCollection DocumentCollection, string SigningLink) a =  (documentCollections.First(), "link");
                    expected.Add(a);
                });

            (var actual, totalCount) =await _linksHandler.Read("", 0, 2);

            Assert.Equal(expected.Count(), actual.Count());
        }

        #endregion


        [Fact]
        public async Task CreateVideoConference_ShouldThrowException_WhenUserIsFreeAccount()
        {
            // Arrange
            var user = new User { CompanyId = FREE_ACCOUNTS_COMPANY_ID  };
            _usersMock.Setup(u => u.GetUser()).ReturnsAsync((user, null));

            // Act & Assert
          var actual =   await Assert.ThrowsAsync<InvalidOperationException>(() => _linksHandler.CreateVideoConference(new CreateVideoConference()));

            Assert.Equal(ResultCode.FreeAccountsCannotCreateVideoConference.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task CreateVideoConference_ShouldThrowException_WhenVideoConferenceIsNotEnabled()
        {
            // Arrange
            var user = new User { CompanyId = Guid.NewGuid() };
            var companyConfiguration = new CompanyConfiguration { ShouldEnableVideoConference = false };
            _usersMock.Setup(u => u.GetUser()).ReturnsAsync((user, null));
            _companyConnectorMock.Setup(c => c.ReadConfiguration(It.IsAny<Company>())).ReturnsAsync(companyConfiguration);

            // Act & Assert
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _linksHandler.CreateVideoConference(new CreateVideoConference()));
            Assert.Equal(ResultCode.VideoConfrenceIsNotEnabled.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task CreateVideoConference_ShouldThrowException_WhenVideoConferenceExceedsLicenseLimit()
        {
            // Arrange
            var user = new User { CompanyId = Guid.NewGuid() };
            var companyConfiguration = new CompanyConfiguration { ShouldEnableVideoConference = true };
            _usersMock.Setup(u => u.GetUser()).ReturnsAsync((user, null));
            _companyConnectorMock.Setup(c => c.ReadConfiguration(It.IsAny<Company>())).ReturnsAsync(companyConfiguration);
            _programConnectorMock.Setup(p => p.CanAddVideoConference(It.IsAny<User>(), It.IsAny<int>())).ReturnsAsync(false);

            // Act & Assert
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _linksHandler.CreateVideoConference(new CreateVideoConference()));
            Assert.Equal(ResultCode.VideoConfrenceExceedLicenseLimit.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task CreateVideoConference_ShouldReturnVideoConferenceResult_WhenSuccessful()
        {
            // Arrange
            var user = new User { CompanyId = Guid.NewGuid() };
            var companyConfiguration = new CompanyConfiguration { ShouldEnableVideoConference = true };
            var externalVideoConfrenceResult = new ExternalVideoConfrenceResult { HostURL = "http://example.com" };
            _usersMock.Setup(u => u.GetUser()).ReturnsAsync((user, null));
            _companyConnectorMock.Setup(c => c.ReadConfiguration(It.IsAny<Company>())).ReturnsAsync(companyConfiguration);
            _programConnectorMock.Setup(p => p.CanAddVideoConference(It.IsAny<User>(), It.IsAny<int>())).ReturnsAsync(true);
            _videoConfrenceMock.Setup(v => v.CreateVideoConference()).ReturnsAsync(externalVideoConfrenceResult);

            // Act
            var result = await _linksHandler.CreateVideoConference(new CreateVideoConference() { VideoConferenceUsers = new List<VideoConferenceUser>()});

            // Assert
            Assert.NotNull(result);
            Assert.Equal("http://example.com", result.ConferenceHostUrl);
        }

    }
}
