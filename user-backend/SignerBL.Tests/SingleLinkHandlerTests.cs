using Common.Interfaces;
using Common.Interfaces.DB;
using Common.Interfaces.PDF;
using Common.Models;
using Common.Models.Configurations;
using Common.Models.Files.PDF;
using Moq;
using PdfHandler;
using SignerBL.Handlers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace SignerBL.Tests
{
    public class SingleLinkHandlerTests : IDisposable
    {
        
        private readonly Mock<IGenerateLinkHandler> _generateLinkHandlerMock;
        private readonly Mock<IDocumentPdf> _documentPdfMock;
        private readonly Mock<IDater> _daterMock;
        private readonly Mock<ITemplatePdf> _templateHandlerMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private ISingleLink _singleLinkHandler;
        private Guid _sampleGuid = Guid.Parse("00000000-0000-0000-0000-000000000001");
        private readonly Mock<IDocumentCollectionConnector> _documentCollectionConnectorMock;
        private readonly Mock<ICompanyConnector> _companyConnectorMock;
        private readonly Mock<IUserConnector> _userConnectorMock;
        private readonly Mock<ITemplateConnector> _templateConnectorMock;
        private readonly Mock<IContactConnector> _contactConnectorMock;

        public SingleLinkHandlerTests()
        {
            _documentCollectionConnectorMock = new Mock<IDocumentCollectionConnector>();
            _companyConnectorMock = new Mock<ICompanyConnector>();
            _userConnectorMock = new Mock<IUserConnector>();
            _templateConnectorMock = new Mock<ITemplateConnector>();
            _contactConnectorMock = new Mock<IContactConnector>();
               _generateLinkHandlerMock = new Mock<IGenerateLinkHandler>();
            _documentPdfMock = new Mock<IDocumentPdf>();
            _daterMock = new Mock<IDater>();
            _templateHandlerMock = new Mock<ITemplatePdf>();
            _configurationMock = new Mock<IConfiguration>();
            _singleLinkHandler = new SingleLinkHandler(_documentCollectionConnectorMock.Object,_companyConnectorMock.Object,
                _userConnectorMock.Object,_templateConnectorMock.Object,_contactConnectorMock.Object, _generateLinkHandlerMock.Object, _documentPdfMock.Object, _daterMock.Object, _templateHandlerMock.Object, _configurationMock.Object);
        }

        public void Dispose()
        {
            _documentCollectionConnectorMock.Invocations.Clear();
            _companyConnectorMock.Invocations.Clear();
            _userConnectorMock.Invocations.Clear();
            _templateConnectorMock.Invocations.Clear();
            _templateConnectorMock.Invocations.Clear();
            _contactConnectorMock.Invocations.Clear();
            _generateLinkHandlerMock.Invocations.Clear();
            _documentPdfMock.Invocations.Clear();
            _daterMock.Invocations.Clear();
            _templateHandlerMock.Invocations.Clear();
            _configurationMock.Invocations.Clear();
        }
        // כל מצב עם דאטא ריק נעצר בפונקציית גטדאטא, שאר הפונקציות תלויות בפונקצנליות של האינטרפייסים (בעיקר של הקונקטור) אל מול הדאטא, קריסה תגיע ככה"נ רק מהרכיבים האלה


        #region Create

        [Fact]
        public async Task Create_WhenSingleLinkIsEmpty_ShouldThrowInvalidOperationException()
        {
            // Arrange


            SingleLink singleLink = new SingleLink();
            Template template = new Template() { Id = _sampleGuid };
            _templateConnectorMock.Setup(x => x.Read(It.IsAny<Template>())).ReturnsAsync(new Template { Id = singleLink.TemplateId });

            // Assert
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _singleLinkHandler.Create(singleLink));
        }

        [Fact]
        public async Task Create_WhenSMSnotSupportedtempContactDefaultSendingMethodIsSMSandPhoneExtensionIsNotIsraeli_ShouldThrowInvalidOperationExceptionAsync()
        {
            // Arrange


            SingleLink singleLink = new SingleLink() { TemplateId = _sampleGuid, Contact = "", PhoneExtension="+000" };
            Template template = new Template() { Id = _sampleGuid };

            _templateConnectorMock.Setup(x => x.Read(It.IsAny<Template>())).ReturnsAsync(template);
            _userConnectorMock.Setup(x => x.Read(It.IsAny<User>())).ReturnsAsync(new User() { Id = _sampleGuid });

            _configurationMock.Setup(x => x.ReadAppConfiguration()).ReturnsAsync(new Configuration());

            CompanyConfiguration companyConfiguration = new CompanyConfiguration();
            Company company = new Company { CompanyConfiguration = companyConfiguration };

            _companyConnectorMock.Setup(x => x.Read(It.IsAny<Company>())).ReturnsAsync(company);

            _configurationMock.Setup(x => x.GetSmsConfiguration(It.IsAny<User>(), It.IsAny<Configuration>(), It.IsAny<CompanyConfiguration>())).ReturnsAsync(new SmsConfiguration() { IsProviderSupportGloballySend = false});


            // Assert

           var actual =   await Assert.ThrowsAsync<InvalidOperationException>(() => _singleLinkHandler.Create(singleLink));


        }

        [Fact]
        public async Task Create_WhenContactIsNullOrDeafultId_ShouldThrowInvalidOperationException()
        {
            // Arrange



            SingleLink singleLink = new SingleLink() { TemplateId = _sampleGuid, Contact = "", PhoneExtension = "+972" };
            Template template = new Template() { Id = _sampleGuid };

            _templateConnectorMock.Setup(x => x.Read(It.IsAny<Template>())).ReturnsAsync(template);
            _userConnectorMock.Setup(x => x.Read(It.IsAny<User>())).ReturnsAsync(new User() { Id = _sampleGuid });

            _configurationMock.Setup(x => x.ReadAppConfiguration()).ReturnsAsync(new Configuration());

            CompanyConfiguration companyConfiguration = new CompanyConfiguration();
            Company company = new Company { CompanyConfiguration = companyConfiguration };

            _companyConnectorMock.Setup(x => x.Read(It.IsAny<Company>())).ReturnsAsync(company);

            _configurationMock.Setup(x => x.GetSmsConfiguration(It.IsAny<User>(), It.IsAny<Configuration>(), It.IsAny<CompanyConfiguration>())).ReturnsAsync(new SmsConfiguration() { IsProviderSupportGloballySend = false });

            _contactConnectorMock.Setup(x => x.Read(It.IsAny<Contact>())).ReturnsAsync(new Contact());
         

            // Assert

           var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _singleLinkHandler.Create(singleLink));


        }


       
        // Cannot Test Success Case - Requiers ExtendedList Mock inside documentCollection Creation.
        // TODO - Create Interface from Pdf abstract class in order to mock an ExtendedList, Cannot be properly tested otherwise 
        // (unless we add a bunch of irrelevant (to the class) mocks.


        #endregion


        #region GetData

        [Fact]
        public async Task GetData_WhenUserIsNull_ShouldGetUserFromGroup()
        {
            // Arrange

            Guid userSampleId = Guid.Parse("00000000-0000-0000-0000-000000000002");

            SingleLink singleLink = new SingleLink() { TemplateId = _sampleGuid };
            Template template = new Template() { Id = _sampleGuid, UserId = _sampleGuid, GroupId = userSampleId };

            _templateConnectorMock.Setup(x => x.Read(It.IsAny<Template>())).ReturnsAsync(template);
            _userConnectorMock.Setup(x => x.Read(It.IsAny<User>())).ReturnsAsync((User)null);


            User user = new User() { Id = userSampleId };
            List<User> users = new List<User>() { user };
            Group group = new Group() { Id = template.GroupId, Users = users };
            _userConnectorMock.Setup(x => x.GetAllUsersInGroup(It.IsAny<Group>())).Returns(users);

            Configuration configuration = new Configuration() { EnableTabletsSupport = true };
           
            var companyConf = new CompanyConfiguration();
            _configurationMock.Setup(x => x.ReadAppConfiguration()).ReturnsAsync(configuration);
            _companyConnectorMock.Setup(x => x.ReadConfiguration(It.IsAny<Company>())).ReturnsAsync(companyConf);


            _configurationMock.Setup( x => x.GetSmsConfiguration(user, configuration, companyConf))
                .ReturnsAsync(new SmsConfiguration() { IsProviderSupportGloballySend = true });

            // Action
            var singleLinkresult = await _singleLinkHandler.GetData(singleLink);

            // Assert

            Assert.Equal(singleLinkresult.Template.UserId, userSampleId);

        }

        [Fact]
        public async Task GetData_WhenSingleLinkTemplateIdDoesntExist_ShouldThrowInvalidOperationException()
        {
            // Arrange
            SingleLink singleLink = new SingleLink();
            Template template = new Template();
            _templateConnectorMock.Setup(x => x.Read(It.IsAny<Template>())).ReturnsAsync(new Template());

            // Assert 
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _singleLinkHandler.GetData(singleLink));
        }

        [Fact]
        public async Task GetData_WhenSingleLinkIsValid_ShouldReturnBool()
        {
            // Arrange
            SingleLink singleLink = new SingleLink() { TemplateId = _sampleGuid };
            Template template = new Template() { Id = _sampleGuid };

            _templateConnectorMock.Setup(x => x.Read(It.IsAny<Template>())).ReturnsAsync(template);
            _userConnectorMock.Setup(x => x.Read(It.IsAny<User>())).ReturnsAsync(new User() { Id = _sampleGuid });

            _configurationMock.Setup(x => x.ReadAppConfiguration()).ReturnsAsync(new Configuration());

            CompanyConfiguration companyConfiguration = new CompanyConfiguration();
            Company company = new Company { CompanyConfiguration = companyConfiguration };

            _companyConnectorMock.Setup(x => x.Read(It.IsAny<Company>())).ReturnsAsync(company);

            _configurationMock.Setup(x => x.GetSmsConfiguration(It.IsAny<User>(), It.IsAny<Configuration>(), It.IsAny<CompanyConfiguration>())).ReturnsAsync(new SmsConfiguration());

            // Action

            var singleLinkresult = await _singleLinkHandler.GetData(singleLink);

            // Assert

            Assert.IsType<SignleLinkGetDataResult>(singleLinkresult);
        }



        #endregion
    }
}
