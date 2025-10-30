using BL.Tests;
using Common.Enums;
using Common.Enums.Documents;
using Common.Enums.Users;
using Common.Handlers;
using Common.Handlers.Files;
using Common.Interfaces;
using Common.Interfaces.DB;
using Common.Interfaces.Files;
using Common.Interfaces.MessageSending.Sms;
using Common.Models;
using Common.Models.Configurations;
using Common.Models.Documents;
using Common.Models.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace Common.Tests.Handlers
{
    public class ConfigurationHandlerTests
    {
        private readonly Guid ID = new Guid("C32BCF3A-C273-4F98-B002-A724DE1479FE");
        private const string DEFAULT_LOGO = "Resources/Logo.png";
        private readonly IFileSystem _fileSystemMock;
        private readonly Mock<ISmsProviderHandler> _smsProviderMock;
        private readonly Mock<IDataUriScheme> _dataUriSchemeMock;
        
        private IOptions<FolderSettings> _folderSettings;
        private IOptions<JwtSettings> _jwtSettings;
        private IConfiguration _configuration;
        private readonly IFilesWrapper _fileWrapperMock;

        private readonly Mock<IDocumentFileWrapper> _documentFileWrapper;
        private readonly Mock<IContactFileWrapper> _contactFileWrapper;
        private readonly Mock<IUserFileWrapper> _userFileWrapper;
        private readonly Mock<ISignerFileWrapper> _signerFileWrapper;
        private readonly Mock<IConfigurationFileWrapper> _configurationFileWrapper;
        private readonly Mock<IConfigurationConnector> _configurationConnectorMock;
        private readonly Mock<IProgramConnector> _programConnectorMock;
        private readonly Mock<ICompanyConnector> _companyConnectorMock;
        private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock;

        public ConfigurationHandlerTests()
        {
            _fileSystemMock = new MockFileSystem();
            _smsProviderMock = new Mock<ISmsProviderHandler>();
            _dataUriSchemeMock = new Mock<IDataUriScheme>();
            _configurationConnectorMock = new Mock<IConfigurationConnector>();
            _programConnectorMock = new Mock<IProgramConnector>();
            _companyConnectorMock = new Mock<ICompanyConnector>();
            _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();

            _folderSettings = Options.Create(new FolderSettings
            {
                CompaniesLogo = "c:\\comda\\wesign\\companiesLogo",
                EmailTemplates = "c:\\comda\\wesign\\emailTemplates",
            });
            _documentFileWrapper = new Mock<IDocumentFileWrapper>();
            _contactFileWrapper = new Mock<IContactFileWrapper>();
            _userFileWrapper = new Mock<IUserFileWrapper>();
            _signerFileWrapper = new Mock<ISignerFileWrapper>();
            _configurationFileWrapper = new Mock<IConfigurationFileWrapper>();

            _fileWrapperMock = new FileWrapperStub(_documentFileWrapper.Object, _contactFileWrapper.Object, _userFileWrapper.Object, _signerFileWrapper.Object, _configurationFileWrapper.Object);



            _jwtSettings = Options.Create(new JwtSettings { SignerLinkExpirationInHours = 25 });
            _configuration = new ConfigurationHandler(_configurationConnectorMock.Object,_programConnectorMock.Object, _companyConnectorMock.Object, _jwtSettings, 
                _smsProviderMock.Object, _fileWrapperMock, _serviceScopeFactoryMock.Object);

        }

        #region GetAfterMessage

        [Fact]
        public void GetAfterMessage_AppConfigurationNull_ThrowException()
        {
            User user = new User();
            Configuration configuration = null;
            CompanyConfiguration companyConfiguration = null;
            _programConnectorMock.Setup(x => x.IsFreeTrialUser(It.IsAny<User>())).Returns(false);
            string exceptedError = "App configuation is not set correctlly , please check your DB for missing configuration, and update site configuration";

            var actual = Assert.Throws<Exception>(() => _configuration.GetAfterMessage(user, configuration, companyConfiguration));

            Assert.Equal(exceptedError, actual.Message);
        }

        [Fact]
        public void GetAfterMessage_NullUser_ThrowException()
        {
            User user = null;
            Configuration configuration = new Configuration
            {
                MessageAfter = "valid message"
            };
            CompanyConfiguration companyConfiguration = null;
            _programConnectorMock.Setup(x => x.IsFreeTrialUser(It.IsAny<User>())).Throws(new Exception());

            var actual = Assert.Throws<Exception>(() => _configuration.GetAfterMessage(user, configuration, companyConfiguration));

            Assert.NotEmpty(actual.Message);
        }

        [Fact]
        public void GetAfterMessage_FreeTrailUserWithoutCompanyConfiguraion_Success()
        {
            User user = new User { Name = "FreeTrial User" };
            Configuration configuration = new Configuration
            {
                MessageAfter = "valid message"
            };
            CompanyConfiguration companyConfiguration = null;
            _programConnectorMock.Setup(x => x.IsFreeTrialUser(It.IsAny<User>())).Returns(true);

            var actual = _configuration.GetAfterMessage(user, configuration, companyConfiguration);

            Assert.Equal("valid message", actual);
        }

        [Fact]
        public void GetAfterMessage_RegularUserWithoutCompanyConfiguraion_Success()
        {
            User user = new User { Name = "Regular User" };
            Configuration configuration = new Configuration
            {
                MessageAfter = "valid message"
            };
            CompanyConfiguration companyConfiguration = null;
            _programConnectorMock.Setup(x => x.IsFreeTrialUser(It.IsAny<User>())).Returns(false);

            var actual = _configuration.GetAfterMessage(user, configuration, companyConfiguration);

            Assert.Equal("valid message", actual);
        }

        [Fact]
        public void GetAfterMessage_RegularUserWithCompanyConfiguraion_Success()
        {
            var user = new User { Name = "Regular User" };
            var configuration = new Configuration
            {
                MessageAfter = "valid message"
            };
            var companyConfiguration = new CompanyConfiguration
            {
                CompanyMessages = new List<CompanyMessage>()
                {
                    new CompanyMessage {  MessageType = MessageType.AfterSigning, Content = "valid company message", Language = Language.en }
                }
            };
            _programConnectorMock.Setup(x => x.IsFreeTrialUser(It.IsAny<User>())).Returns(false);

            var actual = _configuration.GetAfterMessage(user, configuration, companyConfiguration);

            Assert.Equal("valid company message", actual);
        }

        #endregion

        #region GetBeforeMessage

        [Fact]
        public void GetBeforeMessage_AppConfigurationNull_ThrowException()
        {
            User user = new User();
            Configuration configuration = null;
            CompanyConfiguration companyConfiguration = null;
            _programConnectorMock.Setup(x => x.IsFreeTrialUser(It.IsAny<User>())).Returns(false);
            string exceptedError = "App configuation is not set correctlly , please check your DB for missing configuration, and update site configuration";

            var actual = Assert.Throws<Exception>(() => _configuration.GetBeforeMessage(user, configuration, companyConfiguration));

            Assert.Equal(exceptedError, actual.Message);
        }

        [Fact]
        public void GetBeforeMessage_NullUser_ThrowException()
        {
            User user = null;
            Configuration configuration = new Configuration
            {
                MessageBefore = "valid message",
            };
            CompanyConfiguration companyConfiguration = null;
            _programConnectorMock.Setup(x => x.IsFreeTrialUser(It.IsAny<User>())).Throws(new Exception());

            var actual = Assert.Throws<Exception>(() => _configuration.GetBeforeMessage(user, configuration, companyConfiguration));

            Assert.NotEmpty(actual.Message);
        }

        [Fact]
        public void GetBeforeMessage_FreeTrailUserWithoutCompanyConfiguraion_Success()
        {
            User user = new User { Name = "FreeTrial User" };
            Configuration configuration = new Configuration
            {
                MessageBefore = "valid message"
            };
            CompanyConfiguration companyConfiguration = null;
            _programConnectorMock.Setup(x => x.IsFreeTrialUser(It.IsAny<User>())).Returns(true);

            var actual = _configuration.GetBeforeMessage(user, configuration, companyConfiguration);

            Assert.Equal("valid message", actual);
        }

        [Fact]
        public void GetBeforeMessage_RegularUserWithoutCompanyConfiguraion_Success()
        {
            User user = new User { Name = "Regular User" };
            Configuration configuration = new Configuration
            {
                MessageBefore = "valid message"
            };
            CompanyConfiguration companyConfiguration = null;
            _programConnectorMock.Setup(x => x.IsFreeTrialUser(It.IsAny<User>())).Returns(false);

            var actual = _configuration.GetBeforeMessage(user, configuration, companyConfiguration);

            Assert.Equal("valid message", actual);
        }

        [Fact]
        public void GetBeforeMessage_RegularUserWithCompanyConfiguraion_Success()
        {
            var user = new User { Name = "Regular User" };
            var configuration = new Configuration
            {
                MessageBefore = "valid message"
            };
            var companyConfiguration = new CompanyConfiguration
            {
                CompanyMessages = new List<CompanyMessage>()
                {
                    new CompanyMessage {  MessageType = MessageType.BeforeSigning, Content = "valid company message", Language = Language.en }
                }
            };
            _programConnectorMock.Setup(x => x.IsFreeTrialUser(It.IsAny<User>())).Returns(false);

            var actual = _configuration.GetBeforeMessage(user, configuration, companyConfiguration);

            Assert.Equal("valid company message", actual);
        }

        #endregion

        #region GetDeleteSignedDocumentAfterXDays

        [Fact]
        public void GetDeleteSignedDocumentAfterXDays_AppConfigurationNull_ThrowException()
        {
            Configuration configuration = null;
            Company company = null;
            string exceptedError = "App configuation is not set correctlly, please check your DB for missing configuration, and update site configuration";

            var actual = Assert.Throws<Exception>(() => _configuration.GetDocumentsDeletionInterval(configuration, company, Enums.Documents.DocumentStatus.Signed));

            Assert.Equal(exceptedError, actual.Message);
        }

        [Fact]
        public void GetDeleteSignedDocumentAfterXDays_ConpanyNull_ReturnsConfiguration()
        {
            Configuration configuration = new Configuration();
            configuration.DocumentDeletionConfiguration = new DocumentDeletionConfiguration();
            configuration.DocumentDeletionConfiguration.DeleteSignedDocumentAfterXDays = 10;
            configuration.DocumentDeletionConfiguration.DeleteUnsignedDocumentAfterXDays = 30;
            Company company = null;

            var signedInterval = _configuration.GetDocumentsDeletionInterval(configuration, company, Enums.Documents.DocumentStatus.Signed);
            var unsignedInterval = _configuration.GetDocumentsDeletionInterval(configuration, company, Enums.Documents.DocumentStatus.Viewed);

            Assert.Equal(configuration.DocumentDeletionConfiguration.DeleteSignedDocumentAfterXDays, signedInterval);
            Assert.Equal(configuration.DocumentDeletionConfiguration.DeleteUnsignedDocumentAfterXDays, unsignedInterval);
        }

        [Fact]
        public void GetDeleteSignedDocumentAfterXDays_FreeTrailUserWithoutCompanyConfiguraion_Success()
        {
            Configuration configuration = new Configuration
            {
                DocumentDeletionConfiguration = new DocumentDeletionConfiguration() { DeleteSignedDocumentAfterXDays = 10 }
            };
            Company company = null;

            var actual = _configuration.GetDocumentsDeletionInterval(configuration, company, DocumentStatus.Signed);

            Assert.Equal(10, actual);
        }

        [Fact]
        public void GetDeleteSignedDocumentAfterXDays_RegularUserWithoutCompanyConfiguraion_Success()
        {
            Configuration configuration = new Configuration
            {
                DocumentDeletionConfiguration = new DocumentDeletionConfiguration() { DeleteSignedDocumentAfterXDays = 10 }
            };
            Company company = null;

            var actual = _configuration.GetDocumentsDeletionInterval(configuration, company, DocumentStatus.Signed);

            Assert.Equal(10, actual);
        }

        [Fact]
        public void GetDeleteSignedDocumentAfterXDays_RegularUserWithCompanyConfiguraion_Success()
        {
            Configuration configuration = new Configuration
            {
                DocumentDeletionConfiguration = new DocumentDeletionConfiguration()
                {
                    DeleteSignedDocumentAfterXDays = 10
                }
            };
            var company = new Company
            {
                CompanyConfiguration = new CompanyConfiguration
                {
                    DocumentDeletionConfiguration = new DocumentDeletionConfiguration
                    {
                        DeleteSignedDocumentAfterXDays = 20
                    }
                }
            };

            var actual = _configuration.GetDocumentsDeletionInterval(configuration, company, DocumentStatus.Signed);

            Assert.Equal(20, actual);
        }

        #endregion

        #region GetDeleteUnsignedDocumentAfterXDays

        [Fact]
        public void GetDeleteUnsignedDocumentAfterXDays_AppConfigurationNull_ThrowException()
        {
            Configuration configuration = null;
            Company company = null;
            string exceptedError = "App configuation is not set correctlly, please check your DB for missing configuration, and update site configuration";

            var actual = Assert.Throws<Exception>(() => _configuration.GetDocumentsDeletionInterval(configuration, company, DocumentStatus.Signed));

            Assert.Equal(exceptedError, actual.Message);
        }



        [Fact]
        public void GetDeleteUnsignedDocumentAfterXDays_RegularUserWithoutCompanyConfiguraion_Success()
        {
            Configuration configuration = new Configuration
            {
                DocumentDeletionConfiguration = new DocumentDeletionConfiguration() { DeleteUnsignedDocumentAfterXDays = 10 }
            };
            Company companyConfiguration = new Company { Id = Consts.Consts.FREE_ACCOUNTS_COMPANY_ID };

            var actual = _configuration.GetDocumentsDeletionInterval(configuration, companyConfiguration, DocumentStatus.Viewed);

            Assert.Equal(configuration.DocumentDeletionConfiguration.DeleteUnsignedDocumentAfterXDays, actual);
        }

        [Fact]
        public void GetDeleteUnsignedDocumentAfterXDays_RegularUserWithCompanyConfiguraion_Success()
        {
            Configuration configuration = new Configuration
            {
                DocumentDeletionConfiguration = new DocumentDeletionConfiguration()
                {
                    DeleteUnsignedDocumentAfterXDays = 10
                }
            };
            var company = new Company()
            {
                CompanyConfiguration = new CompanyConfiguration
                {
                    DocumentDeletionConfiguration = new DocumentDeletionConfiguration
                    {
                        DeleteUnsignedDocumentAfterXDays = 20
                    }
                }
            };

            var actual = _configuration.GetDocumentsDeletionInterval(configuration, company, DocumentStatus.Viewed);

            Assert.Equal(company.CompanyConfiguration.DocumentDeletionConfiguration.DeleteUnsignedDocumentAfterXDays, actual);
        }

        #endregion

        #region GetSignerLinkExperationTimeInHours

        [Fact]
        public void GetSignerLinkExperationTimeInHours_RegularUserWithoutCompanyConfiguration_ReturnAppSignerLinkExperationTimeInHours()
        {
            User user = null;
            CompanyConfiguration companyConfiguration = null;
            _programConnectorMock.Setup(x => x.IsFreeTrialUser(It.IsAny<User>())).Returns(false);

            var actual = _configuration.GetSignerLinkExperationTimeInHours(user, companyConfiguration);

            Assert.Equal(25, actual);
        }

        [Fact]
        public void GetSignerLinkExperationTimeInHours_RegularUserWithEmptyCompanyConfiguration_ReturnAppSignerLinkExperationTimeInHours()
        {
            User user = null;
            CompanyConfiguration companyConfiguration = new CompanyConfiguration();
            _programConnectorMock.Setup(x => x.IsFreeTrialUser(It.IsAny<User>())).Returns(false);

            var actual = _configuration.GetSignerLinkExperationTimeInHours(user, companyConfiguration);

            Assert.Equal(25, actual);
        }

        [Fact]
        public void GetSignerLinkExperationTimeInHours_RegularUserWithValidCompanyConfiguration_ReturnValueFromCompanyConfig()
        {
            User user = null;
            CompanyConfiguration companyConfiguration = new CompanyConfiguration
            {
                SignerLinkExpirationInHours = 12
            };
            _programConnectorMock.Setup(x => x.IsFreeTrialUser(It.IsAny<User>())).Returns(false);

            var actual = _configuration.GetSignerLinkExperationTimeInHours(user, companyConfiguration);

            Assert.Equal(12, actual);
        }

        [Fact]
        public void GetSignerLinkExperationTimeInHours_FreeTrailUser_ReturnAppSignerLinkExperationTimeInHours()
        {
            User user = null;
            CompanyConfiguration companyConfiguration = null;
            _programConnectorMock.Setup(x => x.IsFreeTrialUser(It.IsAny<User>())).Returns(true);

            var actual = _configuration.GetSignerLinkExperationTimeInHours(user, companyConfiguration);

            Assert.Equal(25, actual);
        }

        #endregion

        #region GetSmsConfiguration

        [Fact]
        public async Task GetSmsConfiguration_AppConfigurationNull_ThrowException()
        {
            User user = null;
            Configuration configuration = null;
            CompanyConfiguration companyConfiguration = null;
            _programConnectorMock.Setup(x => x.IsFreeTrialUser(It.IsAny<User>())).Returns(false);
            string exceptedError = "App configuration is not set correctly , please check your DB for missing configuration, and update site configuration";

            var actual =await Assert.ThrowsAsync<Exception>(() => _configuration.GetSmsConfiguration(user, configuration, companyConfiguration));

            Assert.Equal(exceptedError, actual.Message);
        }

        [Fact]
        public async Task GetSmsConfiguration_NullUser_ThrowException()
        {
            User user = null;
            Configuration configuration = new Configuration();
            CompanyConfiguration companyConfiguration = null;
            _programConnectorMock.Setup(x => x.IsFreeTrialUser(It.IsAny<User>())).Throws(new Exception());

            var actual =await Assert.ThrowsAsync<Exception>(() => _configuration.GetSmsConfiguration(user, configuration, companyConfiguration));

            Assert.NotEmpty(actual.Message);
        }

        [Fact]
        public async Task GetSmsConfiguration_FreeTrailUserWithoutCompanyConfiguraion_ReturnAppSmsConfig()
        {
            User user = new User { Name = "FreeTrial User" };
            Configuration configuration = new Configuration
            {
                SmsConfiguration = new SmsConfiguration() { From = "unit test", User = "smsUser", Password = "smsPassword" }
            };
            CompanyConfiguration companyConfiguration = null;
            _programConnectorMock.Setup(x => x.IsFreeTrialUser(It.IsAny<User>())).Returns(true);

            var actual =await _configuration.GetSmsConfiguration(user, configuration, companyConfiguration);

            Assert.Equal(configuration.SmsConfiguration.From, actual.From);
        }

        [Fact]
        public async Task GetSmsConfiguration_RegularUserWithoutCompanyConfiguraion_ReturnAppSmsConfig()
        {
            User user = new User { Name = "Regular User" };
            Configuration configuration = new Configuration
            {
                SmsConfiguration = new SmsConfiguration() { From = "unit test", User = "smsUser", Password = "smsPassword" }
            };
            CompanyConfiguration companyConfiguration = null;
            _programConnectorMock.Setup(x => x.IsFreeTrialUser(It.IsAny<User>())).Returns(false);

            var actual = await _configuration.GetSmsConfiguration(user, configuration, companyConfiguration);

            Assert.Equal(configuration.SmsConfiguration.From, actual.From);
        }

        [Fact]
        public async Task GetSmsConfiguration_RegularUserWithOnlyDifferentFromParameter_ReturnCompanyFromParameterAndAppCredintial()
        {
            var user = new User { Name = "Regular User" };
            Configuration configuration = new Configuration
            {
                SmsConfiguration = new SmsConfiguration() { From = "unit test", User = "smsUser", Password = "smsPassword", }
            };
            var companyConfiguration = new CompanyConfiguration
            {
                MessageProviders = new List<MessageProvider>()
               {
                  new MessageProvider
                  {
                      ProviderType = ProviderType.SmsGoldman,
                      From = "unit test"
                  }
               }
            };
            _programConnectorMock.Setup(x => x.IsFreeTrialUser(It.IsAny<User>())).Returns(false);

            var actual = await _configuration.GetSmsConfiguration(user, configuration, companyConfiguration);

            Assert.Equal(companyConfiguration.MessageProviders.First().From, actual.From);
            Assert.Equal(configuration.SmsConfiguration.User, actual.User);
            Assert.Equal(configuration.SmsConfiguration.Password, actual.Password);
        }


        [Fact]
        public async Task GetSmsConfiguration_RegularUserWithValidCompanyConfiguraion_Success()
        {
            var user = new User { Name = "Regular User" };
            Configuration configuration = new Configuration
            {
                SmsConfiguration = new SmsConfiguration() { From = "unit test", User = "smsUser", Password = "smsPassword", Provider = ProviderType.SmsGoldman }
            };
            var companyConfiguration = new CompanyConfiguration
            {
                MessageProviders = new List<MessageProvider>()
               {
                  new MessageProvider
                  {
                      ProviderType = ProviderType.SmsGoldman,
                      From = "unit test",
                      Password ="pass",
                      SendingMethod = Enums.Documents.SendingMethod.SMS,
                      Server = "server",
                      User = "user"

                  }
               }
            };
            _programConnectorMock.Setup(x => x.IsFreeTrialUser(It.IsAny<User>())).Returns(false);

            var actual =await _configuration.GetSmsConfiguration(user, configuration, companyConfiguration);

            Assert.Equal(companyConfiguration.MessageProviders.First().From, actual.From);

        }

        #endregion

        #region GetSmtpConfiguration

        [Fact]
        public void GetSmtpConfiguration_NullInput_ThrowException()
        {
            User user = null;
            Configuration configuration = new Configuration();
            CompanyConfiguration companyConfiguration = null;
            _programConnectorMock.Setup(x => x.IsFreeTrialUser(It.IsAny<User>())).Throws(new Exception());

            var actual = Assert.Throws<Exception>(() => _configuration.GetSmtpConfiguration(user, configuration, companyConfiguration));

            Assert.NotEmpty(actual.Message);
        }

        [Fact]
        public void GetSmtpConfiguration_FreeTrailUserWithoutCompanyConfiguraion_ReturnAppSmtpConfig()
        {
            User user = new User { Name = "FreeTrial User" };
            Configuration configuration = new Configuration
            {
                SmtpConfiguration = new SmtpConfiguration() { From = "unitTest@comda.co.il" }
            };
            CompanyConfiguration companyConfiguration = null;
            _programConnectorMock.Setup(x => x.IsFreeTrialUser(It.IsAny<User>())).Returns(true);

            var actual = _configuration.GetSmtpConfiguration(user, configuration, companyConfiguration);

            Assert.Equal(configuration.SmtpConfiguration.From, actual.From);
        }

        [Fact]
        public void GetSmtpConfiguration_RegularUserWithoutCompanyConfiguraion_ReturnAppSmtpConfig()
        {
            User user = new User { Name = "Regular User" };
            Configuration configuration = new Configuration
            {
                SmtpConfiguration = new SmtpConfiguration() { From = "unitTest@comda.co.il" }
            };
            CompanyConfiguration companyConfiguration = null;
            _programConnectorMock.Setup(x => x.IsFreeTrialUser(It.IsAny<User>())).Returns(false);

            var actual = _configuration.GetSmtpConfiguration(user, configuration, companyConfiguration);

            Assert.Equal(configuration.SmtpConfiguration.From, actual.From);
        }

        [Fact]
        public void GetSmtpConfiguration_RegularUserWithInValidCompanyConfiguraion_Success()
        {
            var user = new User { Name = "Regular User" };
            Configuration configuration = new Configuration
            {
                SmtpConfiguration = new SmtpConfiguration() { From = "unitTest@comda.co.il" }
            };
            var companyConfiguration = new CompanyConfiguration
            {
                MessageProviders = new List<MessageProvider>()
               {
                  new MessageProvider
                  {
                      ProviderType = ProviderType.EmailSmtp,
                      From = "unit test"
                  }
               }
            };
            _programConnectorMock.Setup(x => x.IsFreeTrialUser(It.IsAny<User>())).Returns(false);

            var actual = _configuration.GetSmtpConfiguration(user, configuration, companyConfiguration);

            Assert.Equal(configuration.SmtpConfiguration.From, actual.From);
        }

        [Fact]
        public void GetSmtpConfiguration_RegularUserWithValidCompanyConfiguraion_Success()
        {
            var user = new User { Name = "Regular User" };
            Configuration configuration = new Configuration
            {
                SmtpConfiguration = new SmtpConfiguration() { From = "unitTest@comda.co.il" }
            };
            var companyConfiguration = new CompanyConfiguration
            {
                MessageProviders = new List<MessageProvider>()
               {
                  new MessageProvider
                  {
                      ProviderType = ProviderType.EmailSmtp,
                      From = "unitTest@comda.co.il",
                      SendingMethod = Enums.Documents.SendingMethod.Email,
                      Server = "server",
                      Port = 25
                  }
               }
            };
            _programConnectorMock.Setup(x => x.IsFreeTrialUser(It.IsAny<User>())).Returns(false);

            var actual = _configuration.GetSmtpConfiguration(user, configuration, companyConfiguration);

            Assert.Equal(companyConfiguration.MessageProviders.First().From, actual.From);

        }

        #endregion

        #region ShouldSendSignedDocument

        [Fact]
        public void ShouldSendSignedDocument_NullInput_ReturnTrue()
        {
            User user = null;
            CompanyConfiguration companyConfiguration = null;
            DocumentNotifications documentNotifications = null;

            var actual = _configuration.ShouldSendSignedDocument(user, companyConfiguration, documentNotifications);

            Assert.True(actual);
        }

        [Fact]
        public void ShouldSendSignedDocument_DocumentLevel_Success()
        {
            User user = null;
            CompanyConfiguration companyConfiguration = null;
            DocumentNotifications documentNotifications = new DocumentNotifications
            {
                ShouldSendSignedDocument = false
            };

            var actual = _configuration.ShouldSendSignedDocument(user, companyConfiguration, documentNotifications);

            Assert.False(actual);
        }

        [Fact]
        public void ShouldSendSignedDocument_UserLevel_Success()
        {
            User user = new User
            {
                UserConfiguration = new UserConfiguration
                {
                    ShouldSendSignedDocument = false
                }
            };
            CompanyConfiguration companyConfiguration = null;
            DocumentNotifications documentNotifications = new DocumentNotifications();

            var actual = _configuration.ShouldSendSignedDocument(user, companyConfiguration, documentNotifications);

            Assert.False(actual);
        }

        [Fact]
        public void ShouldSendSignedDocument_CompanyLevel_Success()
        {
            User user = null;
            CompanyConfiguration companyConfiguration = new CompanyConfiguration
            {
                ShouldSendSignedDocument = false
            };
            DocumentNotifications documentNotifications = new DocumentNotifications();

            var actual = _configuration.ShouldSendSignedDocument(user, companyConfiguration, documentNotifications);

            Assert.False(actual);
        }

        #endregion

        #region GetLogoPath

        //[Fact]
        //public void GetLogoPath_NullInput_ThrowException()
        //{
        //    User user = null;

        //    var actual = Assert.Throws<Exception>(() => _configuration.GetLogoPath(user));

        //    Assert.Equal("Null input - user is null", actual.Message);
        //}

        //[Fact]
        //public void GetLogoPath_NonCompanyLogo_ReturnDefaultAppLogoPath()
        //{
        //    User user = new User();
        //    string currentFolder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        //    string appLogoPath = Path.Combine(currentFolder, DEFAULT_LOGO);

        //    string actual = _configuration.GetLogoPath(user);

        //    Assert.Equal(appLogoPath, actual);
        //}

        //[Fact]
        //public void GetLogoPath_UserWithCompanyLogo_Success()
        //{
        //    User user = new User
        //    {
        //        CompanyId = ID
        //    };
        //    string companyLogoPath = Path.Combine(_folderSettings.Value.CompaniesLogo, $"{ID}.png");
        //    ((MockFileSystem)_fileSystemMock)?.AddFile(companyLogoPath, new MockFileData("textContent"));

        //    string actual = _configuration.GetLogoPath(user);

        //    Assert.Equal(companyLogoPath, actual);
        //}

        #endregion

        #region SetCompanyLogo

        [Fact]
        public void SetCompanyLogo_NullInput_ThrowException()
        {
            string base64Logo = null;
            User user = null;

            var actual = Assert.Throws<Exception>(() => _configuration.SetCompanyLogo(user, base64Logo));

            Assert.Equal("Null input - user is null", actual.Message);
        }

        [Fact]
        public void SetCompanyLogo_UserWithoutCompanyLogo_Success()
        {
            string base64Logo = null;
            User user = new User();

            _configuration.SetCompanyLogo(user, base64Logo);

            Assert.Empty(((MockFileSystem)_fileSystemMock).AllFiles);
        }

        [Fact]
        public void SetCompanyLogo_UserWithCompanyLogo_Success()
        {
            string base64Logo = "string ";
            User user = new User();
            _configurationFileWrapper.Setup(x => x.SaveCompanyLogo(It.IsAny<User>(), base64Logo));
            _configuration.SetCompanyLogo(user, base64Logo);
            _configurationFileWrapper.Verify(x => x.SaveCompanyLogo(It.IsAny<User>(), base64Logo), Times.Once);
            
        }

        #endregion

        #region GetCompanyLog 

        [Fact]
        public void GetCompanyLogo_NotExistCompanyId_Success()
        {
            User user = new User();
            _configurationFileWrapper.Setup(x => x.GetCompanyLogo(It.IsAny<Guid>())).Returns("");
            string actual = _configuration.GetCompanyLogo(user.CompanyId);

            Assert.Equal("", actual);
        }

      


        #endregion

        #region ShouldNotifyWhileSignerSigned

        [Fact]
        public void ShouldNotifyWhileSignerSigned_NullInput_ReturnTrue()
        {
            User user = null;
            CompanyConfiguration companyConfiguration = null;
            DocumentNotifications documentNotifications = null;

            var actual = _configuration.ShouldNotifyWhileSignerSigned(user, companyConfiguration, documentNotifications);

            Assert.True(actual);
        }

        //[Fact]
        //public void ShouldNotifyWhileSignerSigned_DocumentLevel_Success()
        //{
        //    User user = null;
        //    CompanyConfiguration companyConfiguration = null;
        //    DocumentNotifications documentNotifications = new DocumentNotifications
        //    {
        //        sho = false
        //    };

        //    var actual = _configuration.ShouldNotifyWhileSignerSigned(user, companyConfiguration, documentNotifications);

        //    Assert.False(actual);
        //}

        //[Fact]
        //public void ShouldNotifyWhileSignerSigned_UserLevel_Success()
        //{
        //    User user = new User
        //    {
        //        UserConfiguration = new UserConfiguration
        //        {
        //            ShouldSendSignedDocument = false
        //        }
        //    };
        //    CompanyConfiguration companyConfiguration = null;
        //    DocumentNotifications documentNotifications = new DocumentNotifications();

        //    var actual = _configuration.ShouldNotifyWhileSignerSigned(user, companyConfiguration, documentNotifications);

        //    Assert.False(actual);
        //}

        //[Fact]
        //public void ShouldNotifyWhileSignerSigned_CompanyLevel_Success()
        //{
        //    User user = null;
        //    CompanyConfiguration companyConfiguration = new CompanyConfiguration
        //    {
        //        ShouldSendSignedDocument = false
        //    };
        //    DocumentNotifications documentNotifications = new DocumentNotifications();

        //    var actual = _configuration.ShouldNotifyWhileSignerSigned(user, companyConfiguration, documentNotifications);

        //    Assert.False(actual);
        //}

        #endregion

        #region GetCompanyEmailHtml

        [Fact]
        public void GetCompanyEmailHtml_NotExistCompanyId_Success()
        {
            User user = new User();
            _configurationFileWrapper.Setup(x => x.GetCompanyEmailTemplate(It.IsAny<Company>(), MessageType.BeforeSigning)).Returns("");
            string actual = _configuration.GetCompanyEmailHtml(user.CompanyId, MessageType.BeforeSigning);

            Assert.Equal("", actual);
        }

        //[Fact]
        //public void GetCompanyEmailHtml_ValidCompanyIdWithHtmlBodyTemplate_Success()
        //{
        //    User user = new User
        //    {
        //        CompanyId = ID
        //    };
        //    string path = @"c:\comda\wesign\emailTemplates\c32bcf3a-c273-4f98-b002-a724de1479fe.html";
        //    ((MockFileSystem)_fileSystemMock).AddFile(path, new MockFileData("html body"));

        //    string actual = _configuration.GetCompanyEmailHtml(user.CompanyId);

        //    Assert.StartsWith("data:text/html;base64,", actual);
        //}

        //[Fact]
        //public void GetCompanyEmailHtml_ValidCompanyIdWithoutHtmlBodyTemplate_Success()
        //{
        //    User user = new User
        //    {
        //        CompanyId = ID
        //    };

        //    string actual = _configuration.GetCompanyEmailHtml(user.CompanyId);

        //    Assert.Equal("", actual);
        //}

        #endregion

        #region SetCompanyEmailHtml

        //[Fact]
        //public void SetCompanyEmailHtml_NullInput_ThrowException()
        //{
        //    string base64EmailTemplate = null;
        //    User user = null;

        //    var actual = Assert.Throws<Exception>(() => _configuration.SetCompanyEmailHtml(user, base64EmailTemplate));

        //    Assert.Equal("Null input - user is null", actual.Message);
        //}

        //[Fact]
        //public void SetCompanyEmailHtml_UserWithoutCompanyLogo_Success()
        //{
        //    string base64EmailTemplate = null;
        //    User user = new User();

        //    _configuration.SetCompanyEmailHtml(user, base64EmailTemplate);

        //    Assert.Empty(((MockFileSystem)_fileSystemMock).AllFiles);
        //}

        //[Fact]
        //public void SetCompanyEmailHtml_UserWithCompanyLogo_Success()
        //{
        //    string base64EmailTemplate = "string ";
        //    User user = new User();
        //    _fileSystemMock.Directory.CreateDirectory(_folderSettings.Value.EmailTemplates);
        //    _dataUriSchemeMock.Setup(x => x.GetBytes(It.IsAny<string>())).Returns(new byte[0]);

        //    _configuration.SetCompanyEmailHtml(user, base64EmailTemplate);

        //    Assert.NotEmpty(((MockFileSystem)_fileSystemMock).AllFiles);
        //}

        #endregion

        #region DeleteCompanyLogo

        [Fact]
        public void DeleteCompanyLogo_NullInput_ThrowException()
        {
            User user = null;

            var actual = Assert.Throws<Exception>(() => _configuration.DeleteCompanyLogo(user));

            Assert.Equal("Null input - user is null", actual.Message);
        }

        [Fact]
        public void DeleteCompanyLogo_UserWithCompanyLogo_Success()
        {
            var user = new User
            {
                CompanyId = ID
            };
            string path = Path.Combine(_folderSettings.Value.CompaniesLogo, $"{ID}.png");
            ((MockFileSystem)_fileSystemMock).AddFile(path, new MockFileData("file for delete"));
            _configurationFileWrapper.Setup(x => x.DeleteCompanyLogo(It.IsAny<User>()));
            _configuration.DeleteCompanyLogo(user);
            _configurationFileWrapper.Verify(x => x.DeleteCompanyLogo(It.IsAny<User>()), Times.Once());
            
        }

        [Fact]
        public void DeleteCompanyLogo_UserWithoutCompanyLogo_Success()
        {
            var user = new User
            {
                CompanyId = ID
            };
            _configuration.DeleteCompanyLogo(user);

            Assert.Empty(((MockFileSystem)_fileSystemMock).AllFiles);
        }

        #endregion

        #region DeleteCompanyEmailHtml

        [Fact]
        public void DeleteCompanyEmailHtml_NullInput_ThrowException()
        {
            User user = null;

            var actual = Assert.Throws<Exception>(() => _configuration.DeleteCompanyEmailHtml(user));

            Assert.Equal("Null input - user is null", actual.Message);
        }

        [Fact]
        public void DeleteCompanyEmailHtml_UserWithCompanyEmailHtml_Success()
        {
            var user = new User
            {
                CompanyId = ID
            };
            string path = Path.Combine(_folderSettings.Value.EmailTemplates, $"{ID}.html");
            ((MockFileSystem)_fileSystemMock).AddFile(path, new MockFileData("file for delete"));
            _configurationFileWrapper.Setup(x => x.DeleteCompanyEmailHtml(It.IsAny<User>()));
            _configuration.DeleteCompanyEmailHtml(user);
            _configurationFileWrapper.Verify(x => x.DeleteCompanyEmailHtml(It.IsAny<User>()), Times.Once());
            
        }

        [Fact]
        public void DeleteCompanyEmailHtml_UserWithoutCompanyEmailHtml_Success()
        {
            var user = new User
            {
                CompanyId = ID
            };
            _configuration.DeleteCompanyEmailHtml(user);

            Assert.Empty(((MockFileSystem)_fileSystemMock).AllFiles);
        }

        #endregion

        #region GetLanguage

        [Fact]
        public async Task GetLanguage_NullInput_ThrowException()
        {
            User user = null;

            var actual =await Assert.ThrowsAsync<Exception>(() => _configuration.GetLanguage(user));

            Assert.Equal("Null input - user is null", actual.Message);
        }

        [Fact]
        public async Task GetLanguage_ValidUser_LanguagFromUserConfig()
        {
            User user = new User
            {
                UserConfiguration = new UserConfiguration
                {
                    Language = Language.he
                }
            };

            var actual = await _configuration.GetLanguage(user);

            Assert.Equal(Language.he, actual);
        }

        [Fact]
        public async Task GetLanguage_ValidUser_LanguagFromCompanyConfig()
        {
            User user = new User
            {
                UserConfiguration = null,
                CompanyId = ID
            };
            var company = new Company()
            {
                Id = ID,
                CompanyConfiguration = new CompanyConfiguration
                {
                    Language = Language.he
                }
            };
            _companyConnectorMock.Setup(x => x.Read(It.IsAny<Company>())).ReturnsAsync(company);

            var actual = await _configuration.GetLanguage(user);

            Assert.Equal(Language.he, actual);
        }

        #endregion
    }
}
