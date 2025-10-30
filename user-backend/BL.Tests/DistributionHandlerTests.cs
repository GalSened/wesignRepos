using BL.Handlers;
using BL.Tests.Services;
using Common.Enums.Documents;
using Common.Enums.Results;
using Common.Extensions;
using Common.Interfaces;
using Common.Interfaces.DB;
using Common.Interfaces.MessageSending;
using Common.Interfaces.PDF;
using Common.Models;
using Common.Models.Configurations;
using Common.Models.Documents;
using Common.Models.Documents.Signers;
using Common.Models.Files.PDF;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace BL.Tests
{
    public class DistributionHandlerTests : IDisposable
    {
        private const string GUID = "C14E4B8B-5970-4B50-A1F1-090B8F99D3B2";
        private const string TEMPLATE_GUID = "C80AC1E7-7654-4EF1-BC19-C6CA19C3F8E7";
        private const string Group_GUID = "6BAC7D6D-2391-4162-A9F2-E1F1084AC4B6";

        private readonly Mock<IUsers> _usersMock;
        
        private readonly Mock<IDater> _daterMock;
        private readonly Mock<ITemplatePdf> _templateHandlerMock;
        private readonly Mock<IDocumentPdf> _documentPdfMock;
        private readonly Mock<IDocumentCollections> _documentCollectionsMock;
        private readonly IDistribution _distributionHandler;
        private readonly Mock<ISendingMessageHandler> _sendingMessageHandler;
        private readonly Mock<ILogger> _loggerMock;
        private readonly Mock<IMemoryCache> _memoryCacheMock;

        private readonly Mock<IDocumentCollectionConnector> _documentCollectionConnector;
        private readonly Mock<IProgramConnector> _programConnector;
        private readonly Mock<ICompanyConnector> _companyConnector;
        private readonly Mock<ITemplateConnector> _templateConnector;
        private readonly Mock<IContactConnector> _contactConnector;
        private readonly Mock<IProgramUtilizationConnector> _programUtilizationConnector;
        private readonly Mock<IConfigurationConnector> _configurationConnector;

        public void Dispose()
        {
            _usersMock.Invocations.Clear();
            
            _daterMock.Invocations.Clear();
            _templateHandlerMock.Invocations.Clear();
            _documentPdfMock.Invocations.Clear();
            _documentCollectionsMock.Invocations.Clear();
            _sendingMessageHandler.Invocations.Clear();
            _memoryCacheMock.Invocations.Clear();
            _documentCollectionConnector.Invocations.Clear();
            _programConnector.Invocations.Clear();
            _companyConnector.Invocations.Clear();
            _templateConnector.Invocations.Clear();
            _contactConnector.Invocations.Clear();
            _programUtilizationConnector.Invocations.Clear();
            _configurationConnector.Invocations.Clear();
        }

        public DistributionHandlerTests()
        {
            _documentCollectionConnector = new Mock<IDocumentCollectionConnector>();
            _programConnector = new Mock<IProgramConnector>();
            _companyConnector = new Mock<ICompanyConnector>();
            _templateConnector = new Mock<ITemplateConnector>();
            _contactConnector = new Mock<IContactConnector>();
            _programUtilizationConnector = new Mock<IProgramUtilizationConnector>();
            _configurationConnector = new Mock<IConfigurationConnector>();
            _usersMock = new Mock<IUsers>();            
            _daterMock = new Mock<IDater>();
            _templateHandlerMock = new Mock<ITemplatePdf>();
            _documentPdfMock = new Mock<IDocumentPdf>();
            _documentCollectionsMock = new Mock<IDocumentCollections>();
            _sendingMessageHandler = new Mock<ISendingMessageHandler>();
            _memoryCacheMock = new Mock<IMemoryCache>();
            _loggerMock = new Mock<ILogger>();

            _distributionHandler = new DistributionHandler(_documentCollectionConnector.Object,_companyConnector.Object,_templateConnector.Object,_contactConnector.Object
                , _programUtilizationConnector.Object, _configurationConnector.Object, _programConnector.Object,_usersMock.Object, _daterMock.Object, _templateHandlerMock.Object, _documentPdfMock.Object, _documentCollectionsMock.Object, null, null, _loggerMock.Object, _sendingMessageHandler.Object,
                _memoryCacheMock.Object);
        }

        #region SendDocumentsUsingDistributionMechanism

        [Fact]
        public async Task SendDocumentsUsingDistributionMechanism_NullInput_ReturnException()
        {
            IEnumerable<DocumentCollection> documentCollections = null;

            var actual = await Assert.ThrowsAsync<ArgumentException>(() => _distributionHandler.SendDocumentsUsingDistributionMechanism(documentCollections));

            Assert.Contains("Null input", actual.Message);
        }


        [Fact]
        public async Task SendDocumentsUsingDistributionMechanism_UserProgramExpired_ReturnException()
        {
            IEnumerable<DocumentCollection> documentCollections = new List<DocumentCollection>();
            _programConnector.Setup(x => x.IsProgramExpired(It.IsAny<User>())).ReturnsAsync(true);

            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _distributionHandler.SendDocumentsUsingDistributionMechanism(documentCollections));

            Assert.Equal(ResultCode.UserProgramExpired.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task SendDocumentsUsingDistributionMechanism_UserProgramUtilizationGetToMax_ReturnException()
        {
            IEnumerable<DocumentCollection> documentCollections = new List<DocumentCollection>();
            _programConnector.Setup(x => x.IsProgramExpired(It.IsAny<User>())).ReturnsAsync(false);
            _programConnector.Setup(x => x.CanAddDocument(It.IsAny<User>(), It.IsAny<int>())).ReturnsAsync(false);

            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _distributionHandler.SendDocumentsUsingDistributionMechanism(documentCollections));

            Assert.Equal(ResultCode.ProgramUtilizationGetToMax.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task SendDocumentsUsingDistributionMechanism_InvalidTemplateId_ReturnException()
        {
            IEnumerable<DocumentCollection> documentCollections = new List<DocumentCollection>();
            _programConnector.Setup(x => x.IsProgramExpired(It.IsAny<User>())).ReturnsAsync(false);
            _programConnector.Setup(x => x.CanAddDocument(It.IsAny<User>(), It.IsAny<int>())).ReturnsAsync(true);
            _programConnector.Setup(x => x.CanAddSms(It.IsAny<User>(), It.IsAny<int>())).ReturnsAsync(true);
            _companyConnector.Setup(x => x.Read(It.IsAny<Company>())).ReturnsAsync(new Company());
            _configurationConnector.Setup(x => x.Read()).ReturnsAsync(new Configuration());
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _distributionHandler.SendDocumentsUsingDistributionMechanism(documentCollections));

            Assert.Equal(ResultCode.InvalidTemplateId.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task SendDocumentsUsingDistributionMechanism_NoDocumentInput_ReturnException()
        {
            IEnumerable<DocumentCollection> documentCollections = new List<DocumentCollection> 
            {
                { new DocumentCollection {Documents = new List<Document>{} } }
            };
            _programConnector.Setup(x => x.IsProgramExpired(It.IsAny<User>())).ReturnsAsync(false);
            _programConnector.Setup(x => x.CanAddDocument(It.IsAny<User>(), It.IsAny<int>())).ReturnsAsync(true);
            _templateConnector.Setup(x => x.Exists(It.IsAny<Template>())).ReturnsAsync(false);
            _programConnector.Setup(x => x.CanAddSms(It.IsAny<User>(), It.IsAny<int>())).ReturnsAsync(true);
            _companyConnector.Setup(x => x.Read(It.IsAny<Company>())).ReturnsAsync(new Company());
            _configurationConnector.Setup(x => x.Read()).ReturnsAsync(new Configuration());

            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _distributionHandler.SendDocumentsUsingDistributionMechanism(documentCollections));

            Assert.Equal(ResultCode.InvalidTemplateId.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task SendDocumentsUsingDistributionMechanism_NotUserDocument_ReturnException()
        {
            IEnumerable<DocumentCollection> documentCollections = new List<DocumentCollection>
            {
                { new DocumentCollection {Documents = new List<Document>{ new Document { TemplateId = new Guid(TEMPLATE_GUID) } } } }
            };
            _programConnector.Setup(x => x.IsProgramExpired(It.IsAny<User>())).ReturnsAsync(false);
            _programConnector.Setup(x => x.CanAddDocument(It.IsAny<User>(), It.IsAny<int>())).ReturnsAsync(true);
            _templateConnector.Setup(x => x.Exists(It.IsAny<Template>())).ReturnsAsync(true);
            _templateConnector.Setup(x => x.Read(It.IsAny<Template>())).ReturnsAsync(new Template { GroupId = new Guid(Group_GUID) });
            CompanySigner1Details companySigner1Details = null;
            _usersMock.Setup(x => x.GetUser()).ReturnsAsync((new User { GroupId = new Guid(GUID) }, companySigner1Details));
            _programConnector.Setup(x => x.CanAddSms(It.IsAny<User>(), It.IsAny<int>())).ReturnsAsync(true);
            _companyConnector.Setup(x => x.Read(It.IsAny<Company>())).ReturnsAsync(new Company());
            _configurationConnector.Setup(x => x.Read()).ReturnsAsync(new Configuration());
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _distributionHandler.SendDocumentsUsingDistributionMechanism(documentCollections));

            Assert.Equal(ResultCode.TemplateNotBelongToUserGroup.GetNumericString(), actual.Message);
        }

        //[Fact]
        //public void SendDocumentsUsingDistributionMechanism_DocumentCreationInDBFailed_ReturnException()
        //{
        //    Contact contact = new Contact { Name = "signerTest", Email = "test@comda.co.il", DefaultSendingMethod = SendingMethod.Email };
        //    IEnumerable<DocumentCollection> documentCollections = new List<DocumentCollection>
        //    {
        //        {
        //            new DocumentCollection {
        //                Id = Guid.Empty,
        //                Documents = new List<Document>{ new Document { TemplateId = new Guid(TEMPLATE_GUID) }},
        //                Signers = new List<Signer>{ new Signer { Contact = contact } } 
        //            } 
        //        }
        //    };
        //    _dbConnectorMock.Setup(x => x.Programs.IsProgramExpired(It.IsAny<User>())).Returns(false);
        //    _dbConnectorMock.Setup(x => x.Programs.CanAddDocument(It.IsAny<User>(), It.IsAny<int>())).Returns(true);
        //    _dbConnectorMock.Setup(x => x.Programs.CanAddSms(It.IsAny<User>(), It.IsAny<int>())).Returns(true);
        //    _dbConnectorMock.Setup(x => x.Templates.Exists(It.IsAny<Template>())).Returns(true);
        //    _dbConnectorMock.Setup(x => x.Templates.Read(It.IsAny<Template>())).Returns(new Template { GroupId = new Guid(Group_GUID), Name ="Template Test" });
        //    _dbConnectorMock.Setup(x => x.Contacts.Read(It.IsAny<Contact>())).Returns(contact);
        //    _dbConnectorMock.Setup(x => x.Programs.CanAddSms(It.IsAny<User>(), It.IsAny<int>())).Returns(true);
        //    _dbConnectorMock.Setup(x => x.Companies.Read(It.IsAny<Company>())).Returns(new Company());
        //    _dbConnectorMock.Setup(x => x.Configurations.Read()).Returns(new Configuration());
            
        //   CompanySigner1Details companySigner1Details;
        //    _usersMock.Setup(x => x.GetUser(out companySigner1Details)).Returns(new User { GroupId = new Guid(Group_GUID) });
        //    _templateHandlerMock.Setup(x => x.GetAllFields()).Returns(new PDFFields());
        //    _dbConnectorMock.Setup(x => x.DocumentCollections.Create(It.IsAny<DocumentCollection>()));
       




        //    var actual = Assert.Throws<InvalidOperationException>(() => _distributionHandler.SendDocumentsUsingDistributionMechanism(documentCollections));

        //    Assert.Equal(ResultCode.InvalidDocumentCollectionId.GetNumericString(), actual.Message);
        //}

        [Fact]
        public void SendDocumentsUsingDistributionMechanism_SignerExist_Success()
        {
            Contact contact = new Contact { Name = "signerTest", Email = "test@comda.co.il", DefaultSendingMethod = SendingMethod.Email };
            IEnumerable<DocumentCollection> documentCollections = new List<DocumentCollection>
            {
                {
                    new DocumentCollection {
                        Id = new Guid(GUID),
                        Documents = new List<Document>{ new Document { TemplateId = new Guid(TEMPLATE_GUID) }},
                        Signers = new List<Signer>{ new Signer { Contact = contact } }
                    }
                }
            };
            CompanySigner1Details companySigner1Details = null;
            _usersMock.Setup(x => x.GetUser()).ReturnsAsync((new User { GroupId = new Guid(Group_GUID) }, companySigner1Details));
            _programConnector.Setup(x => x.IsProgramExpired(It.IsAny<User>())).ReturnsAsync(false);
            _programConnector.Setup(x => x.CanAddDocument(It.IsAny<User>(), It.IsAny<int>())).ReturnsAsync(true);
            _templateConnector.Setup(x => x.Exists(It.IsAny<Template>())).ReturnsAsync(true);
            _templateConnector.Setup(x => x.Read(It.IsAny<Template>())).ReturnsAsync(new Template { GroupId = new Guid(Group_GUID), Name = "Template Test" });
            _contactConnector.Setup(x => x.Read(It.IsAny<Contact>())).ReturnsAsync(contact);
            _documentCollectionConnector.Setup(x => x.Create(It.IsAny<DocumentCollection>()));
            _programUtilizationConnector.Setup(x => x.AddDocument(It.IsAny<User>()));
            _programConnector.Setup(x => x.CanAddSms(It.IsAny<User>(), It.IsAny<int>())).ReturnsAsync(true);
            _companyConnector.Setup(x => x.Read(It.IsAny<Company>())).ReturnsAsync(new Company());
            _configurationConnector.Setup(x => x.Read()).ReturnsAsync(new Configuration());
            _templateHandlerMock.Setup(x => x.GetAllFields(It.IsAny<bool>())).Returns(new PDFFields());

            _distributionHandler.SendDocumentsUsingDistributionMechanism(documentCollections);

        }

        #endregion
    }
}
