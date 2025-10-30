using Common.Consts;
using Common.Enums;
using Common.Enums.Companies;
using Common.Enums.Documents;
using Common.Handlers.SendingMessages;
using Common.Interfaces;
using Common.Interfaces.DB;
using Common.Interfaces.Emails;
using Common.Interfaces.ManagementApp;
using Common.Interfaces.MessageSending;
using Common.Interfaces.MessageSending.Mail;
using Common.Interfaces.PDF;
using Common.Models;
using Common.Models.Configurations;
using Common.Models.Documents.Signers;
using Common.Models.Settings;
using ManagementBL.CleanDb;
using ManagementBL.Handlers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Moq;
using Org.BouncyCastle.Utilities;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Times = Moq.Times;

namespace ManagementBL.Tests.Handlers
{
    public class JobsHandlerTests : IDisposable
    {
        private const string GUID = "C14E4B8B-5970-4B50-A1F1-090B8F99D3B2";
        private const string GUID2 = "C80AC1E7-7654-4EF1-BC19-C6CA19C3F8E7";
        private const string GUID3 = "6BAC7D6D-2391-4162-A9F2-E1F1084AC4B6";

        private const string EMAIL1 = "comda1@comda.co.il";
        private const string EMAIL2 = "comda2@comda.co.il";


        
        private readonly Mock<ILogger> _loggerMock;
        private readonly IFileSystem _fileSystemMocker;
        private readonly Mock<IDocumentPdf> _documentPdfMocker;
        private readonly Mock<IConfiguration> _configurationMocker;
        private readonly Mock<IJWT> _jwtMocker;
        private readonly Mock<IDater> _daterMocker;
        private readonly Mock<ILicense> _licenseMocker;
        private readonly Mock<IDeleter> _deleter;
        private readonly Mock<IActiveDirectory> _activeDirectory;
        private readonly Mock<IEmail> _email;
        private readonly Mock<IOneTimeTokens> _oneTimeTokens;
        private readonly Mock<IDocumentCollection> _documentsCollectionsMock;
        private readonly Mock<ICleanDBManager> _cleanDbMangaerMock;
        private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
        private readonly Mock<ICertificate> _certificatesMock;
        private readonly Mock<IPBKDF2> _pbkdf2Mock;
        private readonly Mock<ICompanies> _companiesMock;

        private readonly Mock<ISendingMessageHandler> _sendingMessageHandlerMocker;
        private readonly Mock<IEmailTypeHandler> _emailTypeHandlerMock;
        private readonly Mock<IMessageSender> _senderMocker;
        private readonly Mock<IDocumentCollectionOperations> _documentsOperationsMock;
        private readonly Mock<IUserPeriodicReports> _userPeriodicReportsMock;
        private readonly Mock<IManagementPeriodicReports> _managementPeriodicReportsMock;

        private IOptions<FolderSettings> _folderSettings;
        private IOptions<GeneralSettings> _generalSettings;
        private readonly IJobs _jobsHandler;

        private readonly Mock<ICompanyConnector> _companyConnectorMock;
        private readonly Mock<IUserConnector> _userConnectorMock;
        private readonly Mock<IDocumentCollectionConnector> _documentCollectionConnectorMock;


        public JobsHandlerTests()
        {
            
            _loggerMock = new Mock<ILogger>();
            _fileSystemMocker = new MockFileSystem();
            _documentPdfMocker = new Mock<IDocumentPdf>();
            _configurationMocker = new Mock<IConfiguration>();
            _sendingMessageHandlerMocker = new Mock<ISendingMessageHandler>();
            _jwtMocker = new Mock<IJWT>();
            _daterMocker = new Mock<IDater>();
            _senderMocker = new Mock<IMessageSender>();
            _licenseMocker = new Mock<ILicense>();
            _folderSettings = Options.Create(new FolderSettings { });
            _generalSettings = Options.Create(new GeneralSettings { });
            _deleter = new Mock<IDeleter>();
            _activeDirectory = new Mock<IActiveDirectory>();
            _documentsCollectionsMock = new Mock<ManagementBL.Handlers.IDocumentCollection>();
            _cleanDbMangaerMock = new Mock<ICleanDBManager>();
            _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            _certificatesMock = new Mock<ICertificate>();
            _pbkdf2Mock = new Mock<IPBKDF2>();
            _companiesMock = new Mock<ICompanies>();
            _emailTypeHandlerMock = new Mock<IEmailTypeHandler>();
            _documentsOperationsMock = new Mock<IDocumentCollectionOperations>();
            _userPeriodicReportsMock = new Mock<IUserPeriodicReports>();
            _managementPeriodicReportsMock = new Mock<IManagementPeriodicReports>();
            _companyConnectorMock = new Mock<ICompanyConnector>();
            _userConnectorMock = new Mock<IUserConnector>();
            _documentCollectionConnectorMock = new Mock<IDocumentCollectionConnector>();



            var serviceCollection = new ServiceCollection();

            // Add any DI stuff here:
            serviceCollection.AddSingleton<ICompanyConnector>(_companyConnectorMock.Object);
            serviceCollection.AddSingleton<IUserConnector>(_userConnectorMock.Object);
            serviceCollection.AddSingleton<IDocumentCollectionConnector>(_documentCollectionConnectorMock.Object);
            

            // Create the ServiceProvider
            var serviceProvider = serviceCollection.BuildServiceProvider();

            // serviceScopeMock will contain my ServiceProvider
            var serviceScopeMock = new Mock<IServiceScope>();
            serviceScopeMock.SetupGet<IServiceProvider>(s => s.ServiceProvider)
                .Returns(serviceProvider);

            // serviceScopeFactoryMock will contain my serviceScopeMock
           _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            _serviceScopeFactoryMock.Setup(s => s.CreateScope())
                .Returns(serviceScopeMock.Object);





            _email = new Mock<IEmail>();
            _oneTimeTokens = new Mock<IOneTimeTokens>();

            _jobsHandler = new JobsHandler( _loggerMock.Object, _sendingMessageHandlerMocker.Object,
              _daterMocker.Object,  _licenseMocker.Object, _activeDirectory.Object,
                _generalSettings, _cleanDbMangaerMock.Object, _serviceScopeFactoryMock.Object, _certificatesMock.Object,
                _pbkdf2Mock.Object, _companiesMock.Object, _documentsCollectionsMock.Object, _configurationMocker.Object, _documentsOperationsMock.Object, 
                _userPeriodicReportsMock.Object, _managementPeriodicReportsMock.Object, _folderSettings);
        }
        public void Dispose()
        {
            _loggerMock.Invocations.Clear();
            _documentPdfMocker.Invocations.Clear();
            _configurationMocker.Invocations.Clear();
            _sendingMessageHandlerMocker.Invocations.Clear();
            _jwtMocker.Invocations.Clear();
            _daterMocker.Invocations.Clear();
            _licenseMocker.Invocations.Clear();
            _deleter.Invocations.Clear();
            _documentsCollectionsMock.Invocations.Clear();
            _activeDirectory.Invocations.Clear();
            _cleanDbMangaerMock.Invocations.Clear();
            _serviceScopeFactoryMock.Invocations.Clear();
            _certificatesMock.Invocations.Clear();
            _pbkdf2Mock.Invocations.Clear();
            _companiesMock.Invocations.Clear();
            _documentsOperationsMock.Invocations.Clear();
            _userPeriodicReportsMock.Invocations.Clear();
            _managementPeriodicReportsMock.Invocations.Clear();
        }


        #region SendDocumentIsAboutToBeDeletedNotification

        [Fact]
        public async Task SendDocumentIsAboutToBeDeletedNotification_()
        {

            // Arrange
            var companies = new List<Company>()
            {
                new Company()
                {
                    CompanyConfiguration = new CompanyConfiguration()
                    {
                        DocumentDeletionConfiguration = new Common.Models.Documents.DocumentDeletionConfiguration()
                        {
                            DeleteUnsignedDocumentAfterXDays = 5,
                        }
                    }
                }
            };

            var documentCollections = new List<DocumentCollection>()
            {
                new DocumentCollection()
                {
                    DocumentStatus = Common.Enums.Documents.DocumentStatus.Created,
                    User = new User()
                    {
                        Email = EMAIL1,
                    },
                },
               new DocumentCollection()
                {
                    DocumentStatus = Common.Enums.Documents.DocumentStatus.Created,
                    User = new User()
                    {
                        Email = EMAIL2,
                    },
                }
            };

            var user = new User()
            {

            };
            int _;
            var now = DateTime.Now;
            _configurationMocker.Setup(x => x.ReadAppConfiguration()).ReturnsAsync(new Configuration());
            _companyConnectorMock.Setup(x => x.Read(It.IsAny<String>(), It.IsAny<int>() , It.IsAny<int>(), It.IsAny<CompanyStatus>(), out _)).Returns(companies);
            
            _daterMocker.Setup(x => x.UtcNow()).Returns(now);
            _documentsCollectionsMock.Setup(x => x.ReadByStatusAndDate(It.IsAny<Company>(), It.IsAny<DateTime>(), It.IsAny<bool>())).ReturnsAsync(documentCollections);
            _userConnectorMock.Setup(x => x.Read(It.IsAny<User>())).ReturnsAsync(user);


            _sendingMessageHandlerMocker.Setup(x => x.ExecuteCreation(It.IsAny<SendingMethod>())).Returns(new EmailHandler(_configurationMocker.Object, _emailTypeHandlerMock.Object));
            _sendingMessageHandlerMocker.Setup(x => x.ExecuteCreation(It.IsAny<SendingMethod>())).Returns(_senderMocker.Object);

            _senderMocker.Setup(x => x.Send(It.IsAny<Configuration>(), It.IsAny<CompanyConfiguration>(), It.IsAny<MessageInfo>()));

            _loggerMock.Setup(x => x.Information(It.IsAny<string>()));
            _loggerMock.Setup(x => x.Debug(It.IsAny<string>()));
            _loggerMock.Setup(x => x.Error(It.IsAny<string>()));

            // Act
            var actual = await _jobsHandler.SendDocumentIsAboutToBeDeletedNotification();

            // Assert
            Assert.Equal(2, actual);

        }


        #endregion

        #region SendSignReminders
        [Fact]
        public void SendSignReminders_WithNoRelevantCompanies_ShouldNotInvokeSendSignReminders()
        {
            // Arrange
            int _;
            _companyConnectorMock.Setup(x => x.Read(Consts.EMPTY, 0, Consts.UNLIMITED, CompanyStatus.Created, out _)).Returns(Enumerable.Empty<Company>());

            // Action 
            _jobsHandler.SendSignReminders();

            // Assert
            _documentsOperationsMock.Verify(x => x.SendLinkToSpecificSigner(It.IsAny<DocumentCollection>(), It.IsAny<Signer>(),
                                            It.IsAny<User>(), It.IsAny<CompanyConfiguration>(), It.IsAny<bool>(), It.IsAny<MessageType>(), true), Times.Never);
        }

        [Fact]
        public void SendSignReminders_WithNoWithNoRelevantDocuments_ShouldNotInvokeSendSignReminders()
        {
            // Arrange
            int _;
            CompanyConfiguration companyConfiguration = new CompanyConfiguration()
            {

                ShouldEnableSignReminders = true
            };
            Company company = new Company()
            {
                Id = Guid.Empty,
                CompanyConfiguration = companyConfiguration
            };
            List<Company> companies = new List<Company>(new Company[] { company });
            _companyConnectorMock.Setup(x => x.Read(Consts.EMPTY, 0, Consts.UNLIMITED, CompanyStatus.Created, out _)).Returns(companies);
            _documentCollectionConnectorMock.Setup(x => x.Read(It.IsAny<HashSet<Guid>>())).Returns(Enumerable.Empty<DocumentCollection>());
            // Action 
            _jobsHandler.SendSignReminders();

            // Assert
            _documentsOperationsMock.Verify(x => x.SendLinkToSpecificSigner(It.IsAny<DocumentCollection>(), It.IsAny<Signer>(),
                                            It.IsAny<User>(), It.IsAny<CompanyConfiguration>(), It.IsAny<bool>(), It.IsAny<MessageType>(), true), Times.Never);
        }
        [Fact]
        public void SendSignReminders_UserCantControlsSettings_ShouldTakeCompanyFrequencyAndInvoke()
        {
            // Arrange
            int _;
            CompanyConfiguration companyConfiguration = new CompanyConfiguration()
            {

                ShouldEnableSignReminders = true,
                SignReminderFrequencyInDays = 5,
                CanUserControlReminderSettings = false
            };
            Company company = new Company()
            {
                Id = Guid.Empty,
                CompanyConfiguration = companyConfiguration
            };
            List<Company> companies = new List<Company>(new Company[] { company });
            _companyConnectorMock.Setup(x => x.ReadWithReminders(Consts.EMPTY, 0, Consts.UNLIMITED, CompanyStatus.Created, out _)).Returns(companies);

            UserConfiguration userConfiguration = new UserConfiguration()
            {
                SignReminderFrequencyInDays = 11
            };
            User user = new User()
            {
                Name = "testUser",
                Id = Guid.NewGuid(),
                UserConfiguration = userConfiguration
            };
            Contact contact = new Contact()
            {
                Name = "testSigner"
            };
            Signer signer = new Signer()
            {
                Contact = contact,
                SendingMethod = SendingMethod.Email,
                TimeLastSent = DateTime.UtcNow.AddDays(-10),
                Status = Common.Enums.Contacts.SignerStatus.Viewed
            };
            DocumentCollection documentCollection = new DocumentCollection()
            {
                User = user,
                Mode = DocumentMode.GroupSign,
                Signers = new List<Signer>(new Signer[] { signer }),
                Notifications = new Common.Models.Documents.DocumentNotifications()
                {
                    ShouldSendDocumentForSigning = true
                },
            };
            _documentCollectionConnectorMock.Setup(x => x.ReadDocumentsForRemainder(It.IsAny<Company>())).ReturnsAsync(new List<DocumentCollection>(new DocumentCollection[] { documentCollection }));
            _daterMocker.Setup(x => x.UtcNow()).Returns(DateTime.UtcNow);



            // Action 
            _jobsHandler.SendSignReminders();

            // Assert
            _documentsOperationsMock.Verify(x => x.SendLinkToSpecificSigner(It.IsAny<DocumentCollection>(), It.IsAny<Signer>(),
                                            It.IsAny<User>(), It.IsAny<CompanyConfiguration>(), It.IsAny<bool>(), It.IsAny<MessageType>(), It.IsAny<bool>() ), Times.Once);
        }

        [Fact]
        public void SendSignReminders_UserCanControlsSettings_ShouldTakeUserFrequencyAndInvoke()
        {
            // Arrange
            int _;
            CompanyConfiguration companyConfiguration = new CompanyConfiguration()
            {

                ShouldEnableSignReminders = true,
                SignReminderFrequencyInDays = 11,
                CanUserControlReminderSettings = true
            };
            Company company = new Company()
            {
                Id = Guid.Empty,
                CompanyConfiguration = companyConfiguration
            };
            List<Company> companies = new List<Company>(new Company[] { company });
            _companyConnectorMock.Setup(x => x.ReadWithReminders(Consts.EMPTY, 0, Consts.UNLIMITED, CompanyStatus.Created, out _)).Returns(companies);

            UserConfiguration userConfiguration = new UserConfiguration()
            {
                SignReminderFrequencyInDays = 5,
                ShouldNotifySignReminder = true
            };
            User user = new User()
            {
                Name = "testUser",
                Id = Guid.NewGuid(),
                UserConfiguration = userConfiguration
            };
            Contact contact = new Contact()
            { 
                Name = "testSigner"
            };

            Signer signer = new Signer()
            {
                Contact = contact,
                SendingMethod = SendingMethod.Email,
                TimeLastSent = DateTime.UtcNow.AddDays(-10),
                Status = Common.Enums.Contacts.SignerStatus.Viewed
            };
            DocumentCollection documentCollection = new DocumentCollection()
            {
                User = user,
                Mode = DocumentMode.GroupSign,
                Signers = new List<Signer>(new Signer[] { signer }),
                Notifications = new Common.Models.Documents.DocumentNotifications()
                {
                    ShouldSendDocumentForSigning = true
                },
            };
            _documentCollectionConnectorMock.Setup(x => x.ReadDocumentsForRemainder(It.IsAny<Company>())).ReturnsAsync(new List<DocumentCollection>(new DocumentCollection[] { documentCollection }));
            _daterMocker.Setup(x => x.UtcNow()).Returns(DateTime.UtcNow);



            // Action 
            _jobsHandler.SendSignReminders();

            // Assert
            _documentsOperationsMock.Verify(x => x.SendLinkToSpecificSigner(It.IsAny<DocumentCollection>(), It.IsAny<Signer>(),
                                            It.IsAny<User>(), It.IsAny<CompanyConfiguration>(), It.IsAny<bool>(), It.IsAny<MessageType>(), It.IsAny<bool>()), Times.Once);
        }

        [Fact]
        public void SendSignReminders_UserControlsSettingsAndDisables_ShouldSkipUser()
        {
            // Arrange
            int _;
            CompanyConfiguration companyConfiguration = new CompanyConfiguration()
            {

                ShouldEnableSignReminders = true,
                SignReminderFrequencyInDays = 5,
                CanUserControlReminderSettings = true
            };
            Company company = new Company()
            {
                Id = Guid.Empty,
                CompanyConfiguration = companyConfiguration
            };
            List<Company> companies = new List<Company>(new Company[] { company });
            _companyConnectorMock.Setup(x => x.Read(Consts.EMPTY, 0, Consts.UNLIMITED, CompanyStatus.Created, out _)).Returns(companies);

            UserConfiguration userConfiguration = new UserConfiguration()
            {
                SignReminderFrequencyInDays = 5,
                ShouldNotifySignReminder = false
            };
            User user = new User()
            {
                Id = Guid.NewGuid(),
                UserConfiguration = userConfiguration
            };
            Signer signer = new Signer()
            {
                TimeLastSent = DateTime.UtcNow.AddDays(-10),
                Status = Common.Enums.Contacts.SignerStatus.Viewed
            };
            DocumentCollection documentCollection = new DocumentCollection()
            {
                User = user,
                Mode = DocumentMode.GroupSign,
                Signers = new List<Signer>(new Signer[] { signer }),
                Notifications = new Common.Models.Documents.DocumentNotifications()
                {
                    ShouldSendDocumentForSigning = true
                },
            };
            _documentCollectionConnectorMock.Setup(x => x.Read(It.IsAny<HashSet<Guid>>())).Returns(new List<DocumentCollection>(new DocumentCollection[] { documentCollection }));
            _daterMocker.Setup(x => x.UtcNow()).Returns(DateTime.UtcNow);



            // Action 
            _jobsHandler.SendSignReminders();

            // Assert
            _documentsOperationsMock.Verify(x => x.SendLinkToSpecificSigner(It.IsAny<DocumentCollection>(), It.IsAny<Signer>(),
                                            It.IsAny<User>(), It.IsAny<CompanyConfiguration>(), It.IsAny<bool>(), It.IsAny<MessageType>(), true), Times.Never);
        }
        [Fact]
        public void SendSignReminder_GroupSign_ShouldNotifyRelevantSigners()
        {

            // Arrange
            int _;
            CompanyConfiguration companyConfiguration = new CompanyConfiguration()
            {

                ShouldEnableSignReminders = true,
                SignReminderFrequencyInDays = 5,
                CanUserControlReminderSettings = false
            };
            Company company = new Company()
            {
                Id = Guid.Empty,
                CompanyConfiguration = companyConfiguration
            };
            List<Company> companies = new List<Company>(new Company[] { company });
            _companyConnectorMock.Setup(x => x.ReadWithReminders(Consts.EMPTY, 0, Consts.UNLIMITED, CompanyStatus.Created, out _)).Returns(companies);

            UserConfiguration userConfiguration = new UserConfiguration()
            {
                SignReminderFrequencyInDays = 11
            };
            User user = new User()
            {
                Name = "testUser",
                Id = Guid.NewGuid(),
                UserConfiguration = userConfiguration
            };
            Contact contact = new Contact()
            {
                Name = "testSigner"
            };
            Signer signer1 = new Signer()
            {

                Contact = contact,
                SendingMethod = SendingMethod.Email,
                TimeLastSent = DateTime.UtcNow.AddDays(-10),
                Status = Common.Enums.Contacts.SignerStatus.Viewed
            };
            Signer signer2 = new Signer()
            {
                Contact = contact,
                SendingMethod = SendingMethod.Email,
                TimeLastSent = DateTime.UtcNow.AddDays(-10),
                Status = Common.Enums.Contacts.SignerStatus.Viewed
            };
            DocumentCollection documentCollection = new DocumentCollection()
            {
                User = user,
                Mode = DocumentMode.GroupSign,
                Signers = new List<Signer>(new Signer[] { signer1, signer2 }),
                Notifications = new Common.Models.Documents.DocumentNotifications()
                {
                    ShouldSendDocumentForSigning = true
                },
            };
            _documentCollectionConnectorMock.Setup(x => x.ReadDocumentsForRemainder(It.IsAny<Company>())).ReturnsAsync(new List<DocumentCollection>(new DocumentCollection[] { documentCollection }));
            _daterMocker.Setup(x => x.UtcNow()).Returns(DateTime.UtcNow);



            // Action 
            _jobsHandler.SendSignReminders();

            // Assert
            _documentsOperationsMock.Verify(x => x.SendLinkToSpecificSigner(It.IsAny<DocumentCollection>(), It.IsAny<Signer>(),
                                            It.IsAny<User>(), It.IsAny<CompanyConfiguration>(), It.IsAny<bool>(), It.IsAny<MessageType>(), It.IsAny<bool>()), Times.AtLeast(2));

        }
        [Fact]
        public void SendSignReminder_OrderedGroupSign_ShouldNotifyFirstRelevantSigner()
        {

            // Arrange
            int _;
            CompanyConfiguration companyConfiguration = new CompanyConfiguration()
            {

                ShouldEnableSignReminders = true,
                SignReminderFrequencyInDays = 5,
                CanUserControlReminderSettings = false
            };
            Company company = new Company()
            {
                Id = Guid.Empty,
                CompanyConfiguration = companyConfiguration
            };
            List<Company> companies = new List<Company>(new Company[] { company });
            _companyConnectorMock.Setup(x => x.ReadWithReminders(Consts.EMPTY, 0, Consts.UNLIMITED, CompanyStatus.Created, out _)).Returns(companies);

            UserConfiguration userConfiguration = new UserConfiguration()
            {
                SignReminderFrequencyInDays = 11
            };
            User user = new User()
            {
                Name = "testUser",
                Id = Guid.NewGuid(),
                UserConfiguration = userConfiguration
            };
            Contact contact = new Contact()
            {
                Name = "testSigner"
            };

            Signer signer1 = new Signer()
            {
                Contact = contact,
                SendingMethod = SendingMethod.Email,
                TimeLastSent = DateTime.UtcNow.AddDays(-10),
                Status = Common.Enums.Contacts.SignerStatus.Signed
            };
            Signer signer2 = new Signer()
            {
                Contact = contact,
                SendingMethod = SendingMethod.Email,
                TimeLastSent = DateTime.UtcNow.AddDays(-10),
                Status = Common.Enums.Contacts.SignerStatus.Viewed
            };
            Signer signer3 = new Signer()
            {
                Contact = contact,
                SendingMethod = SendingMethod.Email,
                TimeLastSent = DateTime.UtcNow.AddDays(-10),
                Status = Common.Enums.Contacts.SignerStatus.Viewed
            };

            DocumentCollection documentCollection = new DocumentCollection()
            {
                User = user,
                Mode = DocumentMode.OrderedGroupSign,
                Signers = new List<Signer>(new Signer[] { signer1, signer2, signer3 }),
                Notifications = new Common.Models.Documents.DocumentNotifications()
                {
                    ShouldSendDocumentForSigning = true
                },
            };
            _documentCollectionConnectorMock.Setup(x => x.ReadDocumentsForRemainder(It.IsAny<Company>())).ReturnsAsync(new List<DocumentCollection>(new DocumentCollection[] { documentCollection }));
            _daterMocker.Setup(x => x.UtcNow()).Returns(DateTime.UtcNow);



            // Action 
            _jobsHandler.SendSignReminders();

            // Assert
            _documentsOperationsMock.Verify(x => x.SendLinkToSpecificSigner(It.IsAny<DocumentCollection>(), signer2,
                                            It.IsAny<User>(), It.IsAny<CompanyConfiguration>(), It.IsAny<bool>(), It.IsAny<MessageType>(), It.IsAny<bool>()), Times.Once);

            _documentsOperationsMock.Verify(x => x.SendLinkToSpecificSigner(It.IsAny<DocumentCollection>(), signer1,
                                           It.IsAny<User>(), It.IsAny<CompanyConfiguration>(), It.IsAny<bool>(), It.IsAny<MessageType>(), It.IsAny<bool>()), Times.Never);


            _documentsOperationsMock.Verify(x => x.SendLinkToSpecificSigner(It.IsAny<DocumentCollection>(), signer3,
                                           It.IsAny<User>(), It.IsAny<CompanyConfiguration>(), It.IsAny<bool>(), It.IsAny<MessageType>(), It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        public void SendSignReminder_DocumentWithNoRelevantSigners_ShouldSkipDocument()
        {
            // Arrange
            int _;
            CompanyConfiguration companyConfiguration = new CompanyConfiguration()
            {

                ShouldEnableSignReminders = true,
                SignReminderFrequencyInDays = 5,
                CanUserControlReminderSettings = false
            };
            Company company = new Company()
            {
                Id = Guid.Empty,
                CompanyConfiguration = companyConfiguration
            };
            List<Company> companies = new List<Company>(new Company[] { company });
            _companyConnectorMock.Setup(x => x.ReadWithReminders(Consts.EMPTY, 0, Consts.UNLIMITED, CompanyStatus.Created, out _)).Returns(companies);

            UserConfiguration userConfiguration = new UserConfiguration()
            {
                SignReminderFrequencyInDays = 11
            };
            User user = new User()
            {
                Name = "testUser",
                Id = Guid.NewGuid(),
                UserConfiguration = userConfiguration
            };
            Contact contact = new Contact()
            {
                Name = "testSigner"
            };

            Signer signer1 = new Signer()
            {
                Contact = contact,
                SendingMethod = SendingMethod.Email,
                TimeLastSent = DateTime.UtcNow.AddDays(-10),
                Status = Common.Enums.Contacts.SignerStatus.Signed
            };
           

            DocumentCollection documentCollection = new DocumentCollection()
            {
                User = user,
                Mode = DocumentMode.OrderedGroupSign,
                Signers = new List<Signer>(new Signer[] { signer1}),
                Notifications = new Common.Models.Documents.DocumentNotifications()
                {
                    ShouldSendDocumentForSigning = true
                },
            };
            _documentCollectionConnectorMock.Setup(x => x.Read(It.IsAny<HashSet<Guid>>())).Returns(new List<DocumentCollection>(new DocumentCollection[] { documentCollection }));
            _daterMocker.Setup(x => x.UtcNow()).Returns(DateTime.UtcNow);



            // Action 
            _jobsHandler.SendSignReminders();

            // Assert
           
            _documentsOperationsMock.Verify(x => x.SendLinkToSpecificSigner(It.IsAny<DocumentCollection>(), signer1,
                                           It.IsAny<User>(), It.IsAny<CompanyConfiguration>(), It.IsAny<bool>(), It.IsAny<MessageType>(), true), Times.Never);


          
        }

        [Fact]
        public void SendSignReminder_DocumentWithNoSigners_ShouldSkipDocument()
        {
            // Arrange
            int _;
            CompanyConfiguration companyConfiguration = new CompanyConfiguration()
            {

                ShouldEnableSignReminders = true,
                SignReminderFrequencyInDays = 5,
                CanUserControlReminderSettings = false
            };
            Company company = new Company()
            {
                Id = Guid.Empty,
                CompanyConfiguration = companyConfiguration
            };
            List<Company> companies = new List<Company>(new Company[] { company });
            _companyConnectorMock.Setup(x => x.Read(Consts.EMPTY, 0, Consts.UNLIMITED, CompanyStatus.Created, out _)).Returns(companies);

            UserConfiguration userConfiguration = new UserConfiguration()
            {
                SignReminderFrequencyInDays = 11
            };
            User user = new User()
            {
                Name = "testUser",
                Id = Guid.NewGuid(),
                UserConfiguration = userConfiguration
            };
            Contact contact = new Contact()
            {
                Name = "testSigner"
            };




            DocumentCollection documentCollection = new DocumentCollection()
            {
                User = user,
                Mode = DocumentMode.OrderedGroupSign,
                Signers = new List<Signer>(),
                Notifications = new Common.Models.Documents.DocumentNotifications()
                {
                    ShouldSendDocumentForSigning = true
                },
            };
            _documentCollectionConnectorMock.Setup(x => x.Read(It.IsAny<HashSet<Guid>>())).Returns(new List<DocumentCollection>(new DocumentCollection[] { documentCollection }));
            _daterMocker.Setup(x => x.UtcNow()).Returns(DateTime.UtcNow);



            // Action 
            _jobsHandler.SendSignReminders();

            // Assert

            _documentsOperationsMock.Verify(x => x.SendLinkToSpecificSigner(It.IsAny<DocumentCollection>(), It.IsAny<Signer>(),
                                           It.IsAny<User>(), It.IsAny<CompanyConfiguration>(), It.IsAny<bool>(), It.IsAny<MessageType>(), true), Times.Never);



        }



        #endregion

        #region DeleteLogsFromDB

        //[Fact]
        //public void DeleteLogsFromDB_NeverDeleteLogs_Success()
        //{
        //    _dbConnectorMocker.Setup(x => x.Configurations.Read()).Returns(new Configuration { LogArichveIntervalInDays = -1 });
        //    _dbConnectorMocker.Setup(x => x.Logs.Delete(It.IsAny<DateTime>())).Verifiable();

        //    _jobsHandler.DeleteLogsFromDB();

        //    _dbConnectorMocker.Verify(x => x.Logs.Delete(It.IsAny<DateTime>()), Times.Never);
        //}

        //[Fact]
        //public void DeleteLogsFromDB_ShouldDeleteLogs_NothingShoudHappened()
        //{
        //    _dbConnectorMocker.Setup(x => x.Configurations.Read()).Returns(new Configuration { LogArichveIntervalInDays = 1 });
        //    _dbConnectorMocker.Setup(x => x.Logs.Delete(It.IsAny<DateTime>())).Verifiable();

        //    _jobsHandler.DeleteLogsFromDB();

        //    _dbConnectorMocker.Verify(x => x.Logs.Delete(It.IsAny<DateTime>()), Times.Once);
        //}

        #endregion
    }
}
