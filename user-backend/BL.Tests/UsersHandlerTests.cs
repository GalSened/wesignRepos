namespace BL.Tests
{
    using BL.Handlers;
    using BL.Tests.Services;
    using Castle.DynamicProxy;
    using Comda.License.DAL;
    using Common.Consts;
    using Common.Enums.Documents;
    using Common.Enums.Program;
    using Common.Enums.Results;
    using Common.Enums.Users;
    using Common.Extensions;
    using Common.Handlers;
    using Common.Handlers.Files;
    using Common.Handlers.SendingMessages;
    using Common.Interfaces;
    using Common.Interfaces.DB;
    using Common.Interfaces.Emails;
    using Common.Interfaces.Files;
    using Common.Interfaces.License;
    using Common.Interfaces.MessageSending;
    using Common.Interfaces.UserApp;
    using Common.Models;
    using Common.Models.Configurations;
    using Common.Models.License;
    using Common.Models.Programs;
    using Common.Models.Settings;
    using Common.Models.Users;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Options;
    using Moq;
    using Serilog;
    using System;
    using System.Collections.Generic;
    using System.IO.Abstractions;
    using System.IO.Abstractions.TestingHelpers;
    using System.Net.Http;
    using System.Runtime.Caching;
    using System.Security.Claims;
    using System.Threading.Tasks;
    
    using Xunit;

    public class UsersHandlerTests : IDisposable
    {
        private const string GUID = "C14E4B8B-5970-4B50-A1F1-090B8F99D3B2";
        private const string GROUP_GUID = "0472EE37-3C50-47CE-8ECE-B5455A5CC468";
        private Guid sampleGuid = Guid.Parse("00000000-0000-0000-0000-000000000001");
        private Guid differentSampleGuid = Guid.Parse("00000000-0000-0000-0000-000000000002");



        private readonly ClaimsPrincipal _user;
        private readonly IOptions<ReCaptchaSettings> _reCaptchaSettings;
        private readonly IOptions<GeneralSettings> _generalSetting;
        private readonly Mock<IUserPasswordHistoryConnector> _userPasswordHistoryConnector;
        private readonly Mock<IUserConnector> _userConnector;
        private readonly Mock<IUserTokenConnector> _userTokenConnector;
        
        private readonly Mock<IPBKDF2> _pkbdf2HandlerMock;
        private readonly Mock<IEmail> _emailMock;
        private readonly Mock<IJWT> _jwtMock;
        private readonly Mock<ILogger> _loggerMock;
        private readonly Mock<IOneTimeTokens> _oneTimeTokensMock;
        private readonly Mock<IConfiguration> _configurationMock;
        //private IFileSystem _fileSystemMock;
    
        private readonly Mock<IDater> _dater;
        private readonly Mock<ILicense> _licenseMock;
        private readonly Mock<IHttpClientFactory> _httpClientMock;
        private readonly IUsers _userHandler;
        private readonly Mock<IMemoryCache> _memoryCache;
        private readonly IMemoryCache _memoryCacheMock;
        private readonly Mock<ILMSWrapperConnectorService> _lmsWrapperConnectorService;
        private readonly Mock<ICertificate> _certificate;
        private readonly IFilesWrapper _filesWrapper;
        private readonly Mock<IWeSignLicense> _weSignLicense;
        private readonly Mock<IDocumentFileWrapper> _documentFileWrapper;
        private readonly Mock<IContactFileWrapper> _contactFileWrapper;
        private readonly Mock<IUserFileWrapper> _userFileWrapper;
        private readonly Mock<ISignerFileWrapper> _signerFileWrapper;
        private readonly Mock<IConfigurationFileWrapper> _configurationFileWrapper;
        private readonly Mock<ISendingMessageHandler> _sendingMessageHandler;
        private readonly Mock<IEncryptor> _encryptor;
        private readonly Mock<IProgramConnector> _programConnectorMock;
        private readonly Mock<IProgramUtilizationConnector> _programUtilizationConnectorMock;
        private readonly Mock<ICompanyConnector> _companyConnectorMock;
        private readonly Mock<IGroupConnector> _groupConnectorMock;
        public void Dispose()
        {
            _userPasswordHistoryConnector.Invocations.Clear();
            _userConnector.Invocations.Clear();
            _programConnectorMock.Invocations.Clear();
            _programUtilizationConnectorMock.Invocations.Clear();
            _companyConnectorMock.Invocations.Clear();
            _groupConnectorMock.Invocations.Clear();
            _userTokenConnector.Invocations.Clear();
            
            _pkbdf2HandlerMock.Invocations.Clear();
            _emailMock.Invocations.Clear();
            _jwtMock.Invocations.Clear();
            _loggerMock.Invocations.Clear();
            _oneTimeTokensMock.Invocations.Clear();
            _configurationMock.Invocations.Clear();
            _dater.Invocations.Clear();
            _memoryCache.Invocations.Clear();
            _lmsWrapperConnectorService.Invocations.Clear();
            _certificate.Invocations.Clear();
            _encryptor.Invocations.Clear();
            _sendingMessageHandler.Invocations.Clear();

        }

        public UsersHandlerTests()
        {
            _userPasswordHistoryConnector = new Mock<IUserPasswordHistoryConnector>();
            _userConnector = new Mock<IUserConnector>();
            _userTokenConnector = new Mock<IUserTokenConnector>();  
            _pkbdf2HandlerMock = new Mock<IPBKDF2>();
            _emailMock = new Mock<IEmail>();
            _jwtMock = new Mock<IJWT>();
            _loggerMock = new Mock<ILogger>();
            _oneTimeTokensMock = new Mock<IOneTimeTokens>();
            _configurationMock = new Mock<IConfiguration>();
            _dater = new Mock<IDater>();
            _licenseMock = new Mock<ILicense>();
            _httpClientMock = new Mock<IHttpClientFactory>();
            _memoryCache = new Mock<IMemoryCache>();
            _memoryCacheMock = new MemoryCacheMock();
            _weSignLicense = new Mock<IWeSignLicense>();
            _certificate = new Mock<ICertificate>();
            _sendingMessageHandler = new Mock<ISendingMessageHandler>();
            _encryptor = new Mock<IEncryptor>();
            _documentFileWrapper = new Mock<IDocumentFileWrapper>();
            _contactFileWrapper = new Mock<IContactFileWrapper>();
            _userFileWrapper = new Mock<IUserFileWrapper>();
            _signerFileWrapper = new Mock<ISignerFileWrapper>();
            _configurationFileWrapper = new Mock<IConfigurationFileWrapper>();
            _programConnectorMock = new Mock<IProgramConnector>();
            _programUtilizationConnectorMock = new Mock<IProgramUtilizationConnector>();
            _companyConnectorMock = new Mock<ICompanyConnector>();
            _groupConnectorMock = new Mock<IGroupConnector>();
            _filesWrapper = new FileWrapperStub(_documentFileWrapper.Object, _contactFileWrapper.Object, _userFileWrapper.Object, _signerFileWrapper.Object, _configurationFileWrapper.Object);

            _lmsWrapperConnectorService = new Mock<ILMSWrapperConnectorService>();
            //_fileSystemMock = new MockFileSystem();
            var identity = new ClaimsIdentity(new List<Claim>() { new Claim(ClaimTypes.Sid, GUID), new Claim(ClaimTypes.PrimaryGroupSid , GROUP_GUID) });
            _user = new ClaimsPrincipal(identity);
            _reCaptchaSettings = Options.Create(new ReCaptchaSettings() { });
            _generalSetting = Options.Create(new GeneralSettings() { });
            _userHandler = new UsersHandler(_user, _userConnector.Object,_userTokenConnector.Object,_groupConnectorMock.Object,
                _programUtilizationConnectorMock.Object,_programConnectorMock.Object,_companyConnectorMock.Object,_userPasswordHistoryConnector.Object,
                _emailMock.Object, _dater.Object,
                _pkbdf2HandlerMock.Object, _jwtMock.Object, _loggerMock.Object, _oneTimeTokensMock.Object, _generalSetting,
                _configurationMock.Object, 
                _licenseMock.Object, _memoryCacheMock,  _reCaptchaSettings,
                _httpClientMock.Object, _lmsWrapperConnectorService.Object,
                _certificate.Object, _filesWrapper,
                _sendingMessageHandler.Object, _encryptor.Object); 
        }

        #region SignUp

        [Fact]
        public async Task SignUp_NullInput_ThrowException()
        {
            // Arrange
            User user = null;

            // Action
            var actual = await Assert.ThrowsAsync<Exception>(() => _userHandler.SignUp(user));

            // Assert
            Assert.Equal("Null input - user is null", actual.Message);
        }

        [Fact]
        public async Task SignUp_EmailAlreadyExist_returnEmptyString()
        {
            // Arrange
            var user = new User
            {
                Email = "existEmail@comda.co.il"
            };
            _configurationMock.Setup(x => x.ReadAppConfiguration()).ReturnsAsync(new Common.Models.Configurations.Configuration { EnableFreeTrailUsers = true });

            _userConnector.Setup(x => x.Exists(It.IsAny<User>())).ReturnsAsync(true);

            // Action
            var link = await _userHandler.SignUp(user);

            // Assert
            Assert.Empty(link);
        }

        [Fact]
        public async Task SignUp_ProgramUtilizationDbCreationFailed_ThrowException()
        {
            // Arrange
            var user = new User
            {
                Email = "newUser@comda.co.il"
            };
            _userConnector.Setup(x => x.Exists(It.IsAny<User>())).ReturnsAsync(false);
            _programConnectorMock.Setup(x => x.Exists(It.IsAny<Program>())).ReturnsAsync(true);

            // Action
            var actual =await Assert.ThrowsAsync<NullReferenceException>(() => _userHandler.SignUp(user));

            // Assert
            Assert.Equal("Object reference not set to an instance of an object.", actual.Message);
        }

        [Fact]
        public async Task SignUp_ForbiddenToCreateFreeTrailUser_ThrowException()
        {
            // Arrange
            var user = new User
            {
                Email = "existEmail@comda.co.il"
            };

            _configurationMock.Setup(x => x.ReadAppConfiguration()).ReturnsAsync(new Configuration { EnableFreeTrailUsers = false });
            _userConnector.Setup(x => x.Exists(It.IsAny<User>())).ReturnsAsync(true);

            // Action
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _userHandler.SignUp(user));

            // Assert
            Assert.Equal(ResultCode.ForbiddenToCreateFreeTrailUser.GetNumericString(), actual.Message);
        }


        [Fact]
        public async Task SignUp_ValidInput_Success()
        {
            // Arrange
            var user = new User
            {
                Email = "newUser@comda.co.il"
            };
            _configurationMock.Setup(x => x.ReadAppConfiguration()).ReturnsAsync(new Configuration { EnableFreeTrailUsers = true,
                ShouldReturnActivationLinkInAPIResponse = true
            });
            
            _userConnector.Setup(x => x.Exists(It.IsAny<User>())).ReturnsAsync(false);
            _programConnectorMock.Setup(x => x.Exists(It.IsAny<Program>())).ReturnsAsync(true);
            _programUtilizationConnectorMock.Setup(x => x.Create(It.IsAny<ProgramUtilization>()));
            _groupConnectorMock.Setup(x => x.Create(It.IsAny<Group>()));
            _userConnector.Setup(x => x.Create(It.IsAny<User>()));
            _emailMock.Setup(x => x.ResetPassword(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync("validLink");

            // Action
            string link =await _userHandler.SignUp(user);

            // Assert
            Assert.Equal("validLink", link);
        }

        #endregion

        #region ExternalLogin 
        [Fact]
        public async Task ExteranlLogin_WhenTokenGeneratesNullUser_ShouldThrowInvalidOperationException()
        {
            // Arrange
            string token = "token";
            _oneTimeTokensMock.Setup(x => x.CheckRemoteLoginToken(It.IsAny<UserTokens>())).ReturnsAsync((User)null);

            // Assert
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _userHandler.ExternalLogin(token));

        }

        [Fact]
        public async Task ExternalLogin_WhenTokenGeneratesValidUser_ShouldReturnUserTokens()
        {
            // Arrange
            string token = "token";
            _oneTimeTokensMock.Setup(x => x.CheckRemoteLoginToken(It.IsAny<UserTokens>())).ReturnsAsync(new User());

            // Action
            var action = await _userHandler.ExternalLogin(token);

            // Assert
            Assert.IsType<UserTokens>(action);
        }

        #endregion

        #region TryLogin

        [Fact]
        public async Task TryLogin_InvalidLicense_ThrowException()
        {
            // Arrange
            User user = null;
            _licenseMock.Setup(x => x.GetLicenseInformation()).Throws(new Exception());

            // Action
            var actual = await Assert.ThrowsAsync<Exception>(() => _userHandler.TryLogin(user));

            // Assert
            Assert.True(actual is Exception);
        }

        [Fact]
        public async Task TryLogin_UserNull_ThrowException()
        {
            // Arrange
            User user = null;
            _userConnector.Setup(x => x.Read(It.IsAny<User>())).ReturnsAsync(user);

            // Action
            var actual =await Assert.ThrowsAsync<InvalidOperationException>(() => _userHandler.TryLogin(user));

            // Assert
            Assert.Equal(ResultCode.InvalidCredential.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task TryLogin_InactiveUser_ThrowException()
        {
            // Arrange
            var user = new User
            {
                Status = UserStatus.Created
            };
            _userConnector.Setup(x => x.ReadWithUserToken(It.IsAny<User>())).ReturnsAsync(user);

            // Action
            var actual =await Assert.ThrowsAsync<InvalidOperationException>(() => _userHandler.TryLogin(user));

            // Assert
            Assert.Equal(ResultCode.ActivationRequired.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task TryLogin_InvalidPassword_ThrowException()
        {
            // Arrange
            var user = new User
            {
                Status = UserStatus.Activated
            };
            _userConnector.Setup(x => x.Read(It.IsAny<User>())).ReturnsAsync(user);
            _pkbdf2HandlerMock.Setup(x => x.Check(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

            // Action
            var actual =await Assert.ThrowsAsync<InvalidOperationException>(() => _userHandler.TryLogin(user));

            // Assert
            Assert.Equal(ResultCode.InvalidCredential.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task TryLogin_ExpiredPassword_Valid()
        {
            // Arrange
            var user = new User
            {
                Status = UserStatus.Activated,
                PasswordSetupTime = DateTime.UtcNow.AddDays(-30)
            };
            var company = new Company()
            {
                CompanyConfiguration = new CompanyConfiguration()
                {
                    PasswordExpirationInDays = 5
                }
            };
            _userConnector.Setup(x => x.ReadWithUserToken(It.IsAny<User>())).ReturnsAsync(user);
            _pkbdf2HandlerMock.Setup(x => x.Check(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
            _companyConnectorMock.Setup(x => x.Read(It.IsAny<Company>())).ReturnsAsync(company);
            _dater.Setup(_ => _.UtcNow()).Returns(DateTime.UtcNow);
           

            // Action
            (var _ , UserTokens userTokens) =  await _userHandler.TryLogin(user);

            // Assert
            Assert.Equal("EXPIRED_PASSWORD", userTokens.AuthToken);
        }

        [Fact]
        public async Task TryLogin_ValidPassword_Success()
        {
            // Arrange
            var user = new User
            {
                Status = UserStatus.Activated
            };
            _userConnector.Setup(x => x.ReadWithUserToken(It.IsAny<User>())).ReturnsAsync(user);
            _pkbdf2HandlerMock.Setup(x => x.Check(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
            _configurationMock.Setup(x => x.ReadAppConfiguration()).ReturnsAsync(new Configuration());
            _companyConnectorMock.Setup(x => x.Read(It.IsAny<Company>())).ReturnsAsync(new Company());
            
            // Action
            (bool isSuccess,var _ ) = await _userHandler.TryLogin(user);

            // Assert
            Assert.True(isSuccess);
        }

        [Fact]
        public async Task TryLogin_ValidPassword_OTPFlow_Succcess()
        {
            // Arrange
            var user = new User
            {
                Status = UserStatus.Activated
            };
            var messageSender = new Mock<IMessageSender>();
            messageSender.Setup(x => x.Send(It.IsAny<Configuration>(), It.IsAny<CompanyConfiguration>(), It.IsAny<MessageInfo>()));
            _userConnector.Setup(x => x.ReadWithUserToken(It.IsAny<User>())).ReturnsAsync(user);
            _pkbdf2HandlerMock.Setup(x => x.Check(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
            _configurationMock.Setup(x => x.ReadAppConfiguration()).ReturnsAsync(new Configuration());
            _companyConnectorMock.Setup(x => x.Read(It.IsAny<Company>())).ReturnsAsync(new Company() { CompanyConfiguration = new CompanyConfiguration() { ShouldForceOTPInLogin = true} });
            _dater.Setup(x => x.UtcNow()).Returns(DateTime.UtcNow);
            _userTokenConnector.Setup(x => x.UpdateRefreshToken(It.IsAny<UserTokens>()));
            _configurationMock.Setup(x => x.GetOtpMessgae(It.IsAny<User>(), It.IsAny<Configuration>(), It.IsAny<CompanyConfiguration>())).Returns("WAAAAT");
            _sendingMessageHandler.Setup(x => x.ExecuteCreation(It.IsAny<SendingMethod>())).Returns(messageSender.Object);
            
            // Action
            (bool isSuccess, var _) =await _userHandler.TryLogin(user);

            // Assert
            Assert.True(isSuccess);
        }
        #endregion

        #region Update

        //[Fact]
        //public void Update_NullInput_ThrowException()
        //{
        //    User user = null;

        //    var actual = Assert.Throws<InvalidOperationException>(() => _userHandler.Update(user));

        //    Assert.Equal(ResultCode.NullInput.GetNumericString(), actual.Message);
        //}
        #endregion

        #region Activation

        [Fact]
        public async Task Activation_WhenUserIsntInDb_ShouldReturnNewEmptyUserTokens()
        {
            // Arrange
            User user = new User();
            _userConnector.Setup(x => x.Read(It.IsAny<User>())).ReturnsAsync((User)null);

            // Action
            var action = await _userHandler.Activation(user);

            // Assert
            Assert.IsType<UserTokens>(action);
            Assert.Null(action.JwtToken);
        }

        [Fact]
        public async Task Activation_WhenUserExistsInDb_ShouldReturnNonEmptyUserTokensAsync()
        {
            // Arrange
            User user = new User();
            _userConnector.Setup(x => x.Read(It.IsAny<User>())).ReturnsAsync(user);
            var company = new Company()
            {
                TransactionId = "not important"
        };
            _companyConnectorMock.Setup(x => x.Read(It.IsAny<Company>())).ReturnsAsync(company);

            _jwtMock.Setup(x => x.GenerateToken(It.IsAny<User>())).Returns("expectedValue");

            // Action
            var action = await _userHandler.Activation(user);

            // Assert
            Assert.IsType<UserTokens>(action);
            Assert.Equal("expectedValue", action.JwtToken);
        }

        #endregion

        #region ResendActivation

        [Fact]
        public async Task ResendActivation_WhenUserExistsInDb_ShouldCallActivation()
        {
            // Arrange
            User user = new User();
            _userConnector.Setup(x => x.Read(It.IsAny<User>())).ReturnsAsync(user);
            // Action
            await _userHandler.ReSendActivation(user);
            // Assert
            _emailMock.Verify(x => x.Activation(It.IsAny<User>(), true));

        }

        [Fact]
        public async Task ResendActivation_WhenUserDoesntExistInDb_ShouldntCallActivation()
        {
            // Arrange
            User user = new User();
            _userConnector.Setup(x => x.Read(It.IsAny<User>())).ReturnsAsync((User)null);
            // Action
            await _userHandler.ReSendActivation(user);
            // Assert
            _emailMock.Verify(x => x.Activation(It.IsAny<User>(), true), Times.Never);
        }

        #endregion

        #region ResetPassword

        [Fact]
        public async Task ResetPassword_WhenUserExistsInDb_ShouldReturnlink()
        {
            // Arrange
            User user = new User();
            _userConnector.Setup(x => x.Read(It.IsAny<User>())).ReturnsAsync(user);

            _oneTimeTokensMock.Setup(x => x.GenerateResetPasswordToken(It.IsAny<User>())).ReturnsAsync("value");
            _emailMock.Setup(x => x.ResetPassword(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync("testVal");
            // Action

            var action =await _userHandler.ResetPassword(user);
            // Assert
            Assert.Equal("testVal", action);
        }

        [Fact]
        public async Task ResetPassword_WhenUserDoesntExistInDb_ShouldReturnEmptyString()
        {
            // Arrange
            User user = new User();
            _userConnector.Setup(x => x.Read(It.IsAny<User>())).ReturnsAsync((User)null);
            // Action
            var action = await _userHandler.ResetPassword(user);
            // Assert
            Assert.Equal(String.Empty, action);
        }

        #endregion

        #region UpdatePassword

        [Fact]
        public async Task UpdatePassword_WhenOtpTokenNotValid_ShouldThrowInvalidOperationException()
        {
            // Arrange
            Guid sampleGuid = Guid.Parse("00000000-0000-0000-0000-000000000001");
            User user = new User();
            string token = "";

            _userTokenConnector.Setup(x => x.Read(It.IsAny<User>())).ReturnsAsync(new UserTokens() { UserId = sampleGuid });
            _oneTimeTokensMock.Setup(x => x.CheckPasswordToken(It.IsAny<User>(), It.IsAny<UserTokens>())).ReturnsAsync(false);

            // Assert
            await  Assert.ThrowsAsync<InvalidOperationException>(async () =>await _userHandler.UpdatePassword(user, token));


        }

        [Fact]
        public async Task UpdatePassword_WhenUserDoesntExistInDb_ShouldThrowInvalidOperationException()
        {
            // Arrange 

            Guid sampleGuid = Guid.Parse("00000000-0000-0000-0000-000000000001");
            User user = new User();
            string token = "";

            _userTokenConnector.Setup(x => x.Read(It.IsAny<User>())).ReturnsAsync(new UserTokens() { UserId = sampleGuid });
            _oneTimeTokensMock.Setup(x => x.CheckPasswordToken(It.IsAny<User>(), It.IsAny<UserTokens>())).ReturnsAsync(true);

            _userConnector.Setup(x => x.Read(It.IsAny<User>())).ReturnsAsync((User)null);

            // Assert
           var result  =  await Assert.ThrowsAsync<InvalidOperationException>(() => _userHandler.UpdatePassword(user, token));
          
        }


        [Fact]
        public async Task UpdatePassword_WhenOtpValidAndUserExists_ShouldReturnUserTokens()
        {
            // Arrange
            Guid sampleGuid = Guid.Parse("00000000-0000-0000-0000-000000000001");
            User user = new User();
            string token = "";
            _companyConnectorMock.Setup(x => x.Read(It.IsAny<Company>())).ReturnsAsync(new Company { CompanyConfiguration = new CompanyConfiguration()});
            _userPasswordHistoryConnector.Setup(x => x.DeleteAllByUserId(It.IsAny<Guid>()));
            
            _userTokenConnector.Setup(x => x.Read(It.IsAny<User>())).ReturnsAsync(new UserTokens() { UserId = sampleGuid });
            _oneTimeTokensMock.Setup(x => x.CheckPasswordToken(It.IsAny<User>(), It.IsAny<UserTokens>())).ReturnsAsync(true);

            _userConnector.Setup(x => x.Read(It.IsAny<User>())).ReturnsAsync(user);

            // Action 
            var action = await _userHandler.UpdatePassword(user, token);

            // Assert
            Assert.IsType<UserTokens>(action);

        }

        #endregion  

        #region UpdatePasswordByDevAdminUser

        [Fact]
        public async Task UpdatePasswordByDevAdminUser_WhenUserDoesntExistInDb_ShouldThrowInvalidOperationException()
        {
            // Arrange
            User user = new User();
            _companyConnectorMock.Setup(x => x.Read(It.IsAny<Company>())).ReturnsAsync(new Company { CompanyConfiguration = new CompanyConfiguration() });
            _userPasswordHistoryConnector.Setup(x => x.DeleteAllByUserId(It.IsAny<Guid>()));
            _userConnector.Setup(x => x.Read(It.IsAny<User>())).ReturnsAsync((User)null);

            // Assert
           var actual =  await Assert.ThrowsAsync<InvalidOperationException>(() => _userHandler.UpdatePasswordByDevAdminUser(user));
        }

        [Fact]
        public async Task UpdatePasswordByDevAdminUser_WhenUserExistsInDb_ShouldReturnUserTokens()
        {
            // Arrange
            Guid sampleGuid = Guid.Parse("00000000-0000-0000-0000-000000000001");
            User user = new User() { Id = sampleGuid };
            _userConnector.Setup(x => x.Read(It.IsAny<User>())).ReturnsAsync(user);
            _companyConnectorMock.Setup(x => x.Read(It.IsAny<Company>())).ReturnsAsync(new Company { CompanyConfiguration = new CompanyConfiguration() });
            _userPasswordHistoryConnector.Setup(x => x.DeleteAllByUserId(It.IsAny<Guid>()));
            _userTokenConnector.Setup(x => x.DeleteResetPasswordToken(It.IsAny<UserTokens>()));

            // Action 
            var action = await _userHandler.UpdatePasswordByDevAdminUser(user);

            // Assert
            Assert.IsType<UserTokens>(action);
        }

        [Fact]
        public async Task UpdatePasswordByDevAdminUser_NewUserShouldNotCreatePasswordHistory_Success()
        {
            // Arrange
            var dbUser = new User() { Password = null };
            var company = new Company()
            {
                CompanyConfiguration = new CompanyConfiguration()
                {
                    RecentPasswordsAmount = 1
                }
            };
            _userConnector.Setup(_ => _.Read(It.IsAny<User>())).ReturnsAsync(dbUser);
            _companyConnectorMock.Setup(_ => _.Read(It.IsAny<Company>())).ReturnsAsync(company);
            _userPasswordHistoryConnector.Setup(_ => _.ReadAllByUserId(It.IsAny<Guid>())).Returns(new List<UserPasswordHistory>());
            _userTokenConnector.Setup(_ => _.DeleteResetPasswordToken(It.IsAny<UserTokens>()));

            // Action
           await _userHandler.UpdatePasswordByDevAdminUser(new User());

            // Assert
            _userPasswordHistoryConnector.Verify(_ => _.Create(It.IsAny<UserPasswordHistory>()), Times.Never);

        }

        #endregion

        #region GetUser
        [Theory]
        [InlineData("logo", true, ProgramResetType.DocumentsLimitOnly)]
        [InlineData("data:image/png;base64", true, ProgramResetType.DocumentsLimitOnly)]
        [InlineData("logo", false, ProgramResetType.DocumentsLimitOnly)]
        [InlineData("data:image/png;base64", false, ProgramResetType.DocumentsLimitOnly)]
        [InlineData("logo", true, ProgramResetType.Monthly)]
        [InlineData("data:image/png;base64", true, ProgramResetType.Monthly)]
        [InlineData("logo", false, ProgramResetType.Monthly)]
        [InlineData("data:image/png;base64", false, ProgramResetType.Monthly)]

        public async Task GetUser_WhenUserInMemoryNullAccountCompanyIsRegularProgUtilNotExpired_ShouldChangeStateAccordinglyAndReturnUser
            (string logoPath, bool isSigner1DetailsInMemory, ProgramResetType programResetType)
        {

            // Arrange 
            string signer1CertId = "test";
            string Changedsigner1CertId = "Changed";
            long docsLimit = 1000;
            long docsUseage = 500;
            long documentsPerMonth = 1000;
            long brokenDocumentsPerMonth = 0;


            CompanySigner1Details companySigner1Details = new CompanySigner1Details() { CertId = signer1CertId };

            User user = new User() { Id = sampleGuid, CompanyLogo = logoPath, CompanyId = differentSampleGuid , GroupId = Guid.Parse(GROUP_GUID) };
            _userConnector.Setup(x => x.Read(It.IsAny<User>())).ReturnsAsync(user);
            ProgramUtilization programUtilization = new ProgramUtilization() { ProgramResetType = programResetType, Expired = DateTime.MaxValue, DocumentsLimit = docsLimit, DocumentsUsage = docsUseage };
            CompanyConfiguration companyConfiguration = new CompanyConfiguration();
            Company regularAccountCompany = new Company() { Id = Guid.Parse(GUID), ProgramUtilization = programUtilization, CompanyConfiguration = companyConfiguration };
            Program program = new Program() { DocumentsPerMonth = documentsPerMonth };
            if (programResetType != ProgramResetType.Monthly)
                program.DocumentsPerMonth = brokenDocumentsPerMonth;
            _programConnectorMock.Setup(x => x.Read(It.IsAny<Program>())).ReturnsAsync(program);

            if (!isSigner1DetailsInMemory)
            {
                user.CompanyId = sampleGuid;
                regularAccountCompany.CompanySigner1Details = companySigner1Details;
                companySigner1Details.CertId = Changedsigner1CertId;
            }
            _companyConnectorMock.Setup(x => x.Read(It.IsAny<Company>())).ReturnsAsync((Company)null);
            SetupsForGetUser(regularAccountCompany, user, isSigner1DetailsInMemory);

            _userFileWrapper.Setup(x => x.SetCompanyLogo(It.IsAny<User>())).Callback(() => user.CompanyLogo = logoPath);

         
            // Action
            (var action, companySigner1Details) = await _userHandler.GetUser();
            // Assert
            Assert.Equal(500, action.ProfileProgram.RemainingDocuments);
            Assert.Equal(1000, action.ProfileProgram.DocumentsLimits);
            Assert.Equal(logoPath, action.CompanyLogo);
            Assert.Equal(Changedsigner1CertId, companySigner1Details.CertId);
        }

        [Theory]
        [InlineData("logo", true, ProgramResetType.DocumentsLimitOnly, true)]
        [InlineData("data:image/png;base64", true, ProgramResetType.DocumentsLimitOnly, true)]
        [InlineData("logo", false, ProgramResetType.DocumentsLimitOnly, true)]
        [InlineData("data:image/png;base64", false, ProgramResetType.DocumentsLimitOnly, true)]
        [InlineData("logo", true, ProgramResetType.Monthly, true)]
        [InlineData("data:image/png;base64", true, ProgramResetType.Monthly, true)]
        [InlineData("logo", false, ProgramResetType.Monthly, true)]
        [InlineData("data:image/png;base64", false, ProgramResetType.Monthly, true)]
        [InlineData("logo", true, ProgramResetType.DocumentsLimitOnly, false)]
        [InlineData("data:image/png;base64", true, ProgramResetType.DocumentsLimitOnly, false)]
        [InlineData("logo", false, ProgramResetType.DocumentsLimitOnly, false)]
        [InlineData("data:image/png;base64", false, ProgramResetType.DocumentsLimitOnly, false)]
        [InlineData("logo", true, ProgramResetType.Monthly, false)]
        [InlineData("data:image/png;base64", true, ProgramResetType.Monthly, false)]
        [InlineData("logo", false, ProgramResetType.Monthly, false)]
        [InlineData("data:image/png;base64", false, ProgramResetType.Monthly, false)]
        public async Task GetUser_WhenUserInMemoryNullAccountCompanyIsFreeProgUtilNotExpired_ShouldChangeStateAccordinglyAndReturnUser
          (string logoPath, bool isSigner1DetailsInMemory, ProgramResetType programResetType, bool shouldGetUiViewCatch)
        {

            // Arrange 
            string signer1CertId = "test";
            string Changedsigner1CertId = "Changed";
            long docsLimit = 1000;
            long docsUseage = 500;
            long documentsPerMonth = 1000;
            long brokenDocumentsPerMonth = 0;


            CompanySigner1Details companySigner1Details = new CompanySigner1Details() { CertId = signer1CertId };
            UIViewLicense uiViewLicense = new UIViewLicense() { ShouldShowSelfSign = false };
            UIViewLicense uiViewProgramLicense = new UIViewLicense() { ShouldShowSelfSign = false };
            ProfileProgram profile = new ProfileProgram() { ViewLicense = uiViewLicense };
            User user = new User() { Id = sampleGuid, ProfileProgram = profile, CompanyLogo = logoPath, CompanyId = sampleGuid,GroupId = Guid.Parse(GROUP_GUID) };
            _userConnector.Setup(x => x.Read(It.IsAny<User>())).ReturnsAsync(user);
            ProgramUtilization programUtilization = new ProgramUtilization() { ProgramResetType = programResetType, Expired = DateTime.MaxValue, DocumentsLimit = docsLimit, DocumentsUsage = docsUseage };
            CompanyConfiguration companyConfiguration = new CompanyConfiguration();
            Company freeAccountCompany = new Company() { Id = sampleGuid, ProgramUtilization = programUtilization, CompanyConfiguration = companyConfiguration };
            Program program = new Program() { DocumentsPerMonth = documentsPerMonth, UIViewLicense = uiViewProgramLicense };
            if (programResetType != ProgramResetType.Monthly)
                program.DocumentsPerMonth = brokenDocumentsPerMonth;
            _programConnectorMock.Setup(x => x.Read(It.IsAny<Program>())).ReturnsAsync(program);
            _programUtilizationConnectorMock.Setup(x => x.Read(It.IsAny<ProgramUtilization>())).ReturnsAsync(programUtilization);

            if (!isSigner1DetailsInMemory)
            {
                user.CompanyId = sampleGuid;
                freeAccountCompany.CompanySigner1Details = companySigner1Details;
                companySigner1Details.CertId = Changedsigner1CertId;
            }
            _companyConnectorMock.Setup(x => x.Read(It.IsAny<Company>())).ReturnsAsync(freeAccountCompany);
            SetupsForGetUser(freeAccountCompany, user, isSigner1DetailsInMemory);
            _userFileWrapper.Setup(x => x.SetCompanyLogo(It.IsAny<User>())).Callback(() => user.CompanyLogo = logoPath);

          

            _weSignLicense.SetupGet(x => x.UIViewLicense).Returns(uiViewLicense);
            _licenseMock.Setup(x => x.GetLicenseInformation()).Returns(_weSignLicense.Object);


            if (shouldGetUiViewCatch)
            {
                _licenseMock.Setup(x => x.GetLicenseInformation()).Throws(new Exception());
                _weSignLicense.Object.UIViewLicense.ShouldShowSelfSign = true;
            }

            // Action
            (var action, companySigner1Details) = await _userHandler.GetUser();
            // Assert
            Assert.Equal(500, action.ProfileProgram.RemainingDocuments);
            Assert.Equal(1000, action.ProfileProgram.DocumentsLimits);
            Assert.Equal(logoPath, action.CompanyLogo);
            Assert.Equal(Changedsigner1CertId, companySigner1Details.CertId);
            Assert.False(user.ProfileProgram.ViewLicense.ShouldShowSelfSign);
        }

        [Theory]
        [InlineData("logo", true, ProgramResetType.DocumentsLimitOnly)]
        [InlineData("data:image/png;base64", true, ProgramResetType.DocumentsLimitOnly)]
        [InlineData("logo", false, ProgramResetType.DocumentsLimitOnly)]
        [InlineData("data:image/png;base64", false, ProgramResetType.DocumentsLimitOnly)]
        [InlineData("logo", true, ProgramResetType.Monthly)]
        [InlineData("data:image/png;base64", true, ProgramResetType.Monthly)]
        [InlineData("logo", false, ProgramResetType.Monthly)]
        [InlineData("data:image/png;base64", false, ProgramResetType.Monthly)]
        public async Task GetUser_WhenUserInMemoryNullAccountCompanyIsRegularProgUtilExpired_ShouldChangeStateAccordinglyAndReturnUser
           (string logoPath, bool isSigner1DetailsInMemory, ProgramResetType programResetType)
        {

            // Arrange 
            string signer1CertId = "test";
            string Changedsigner1CertId = "Changed";
            long docsLimit = 1000;
            long docsUseage = 500;
            long documentsPerMonth = 1000;
            long brokenDocumentsPerMonth = 0;


            CompanySigner1Details companySigner1Details = new CompanySigner1Details() { CertId = signer1CertId };

            User user = new User() { Id = sampleGuid, CompanyLogo = logoPath, CompanyId = differentSampleGuid,  GroupId = Guid.Parse(GROUP_GUID) };
            _userConnector.Setup(x => x.Read(It.IsAny<User>())).ReturnsAsync(user);
            ProgramUtilization programUtilization = new ProgramUtilization() { ProgramResetType = programResetType, Expired = DateTime.UtcNow, DocumentsLimit = docsLimit, DocumentsUsage = docsUseage };
            CompanyConfiguration companyConfiguration = new CompanyConfiguration();
            Company regularAccountCompany = new Company() { Id = Guid.Parse(GUID), ProgramUtilization = programUtilization, CompanyConfiguration = companyConfiguration };
            Program program = new Program() { DocumentsPerMonth = documentsPerMonth };
            if (programResetType != ProgramResetType.Monthly)
                program.DocumentsPerMonth = brokenDocumentsPerMonth;
            _programConnectorMock.Setup(x => x.Read(It.IsAny<Program>())).ReturnsAsync(program);
            _dater.Setup(x => x.UtcNow()).Returns(DateTime.UtcNow);

            if (!isSigner1DetailsInMemory)
            {
                user.CompanyId = sampleGuid;
                regularAccountCompany.CompanySigner1Details = companySigner1Details;
                companySigner1Details.CertId = Changedsigner1CertId;
            }
            _companyConnectorMock.Setup(x => x.Read(It.IsAny<Company>())).ReturnsAsync((Company)null);
            SetupsForGetUser(regularAccountCompany, user, isSigner1DetailsInMemory);

            _userFileWrapper.Setup(x => x.SetCompanyLogo(It.IsAny<User>())).Callback(() => user.CompanyLogo = logoPath);
           
            // Action
            (var action, companySigner1Details) = await _userHandler.GetUser();
            // Assert
            Assert.Equal(0, action.ProfileProgram.RemainingDocuments);
            Assert.Equal(1000, action.ProfileProgram.DocumentsLimits);
            Assert.Equal(logoPath, action.CompanyLogo);
            Assert.Equal(Changedsigner1CertId, companySigner1Details.CertId);
        }


        //

        [Theory]
        [InlineData("logo", true, ProgramResetType.DocumentsLimitOnly, true)]
        [InlineData("data:image/png;base64", true, ProgramResetType.DocumentsLimitOnly, true)]
        [InlineData("logo", false, ProgramResetType.DocumentsLimitOnly, true)]
        [InlineData("data:image/png;base64", false, ProgramResetType.DocumentsLimitOnly, true)]
        [InlineData("logo", true, ProgramResetType.Monthly, true)]
        [InlineData("data:image/png;base64", true, ProgramResetType.Monthly, true)]
        [InlineData("logo", false, ProgramResetType.Monthly, true)]
        [InlineData("data:image/png;base64", false, ProgramResetType.Monthly, true)]
        [InlineData("logo", true, ProgramResetType.DocumentsLimitOnly, false)]
        [InlineData("data:image/png;base64", true, ProgramResetType.DocumentsLimitOnly, false)]
        [InlineData("logo", false, ProgramResetType.DocumentsLimitOnly, false)]
        [InlineData("data:image/png;base64", false, ProgramResetType.DocumentsLimitOnly, false)]
        [InlineData("logo", true, ProgramResetType.Monthly, false)]
        [InlineData("data:image/png;base64", true, ProgramResetType.Monthly, false)]
        [InlineData("logo", false, ProgramResetType.Monthly, false)]
        [InlineData("data:image/png;base64", false, ProgramResetType.Monthly, false)]
        public async Task GetUser_WhenUserInMemoryNullAccountCompanyIsFreeProgUtilExpired_ShouldChangeStateAccordinglyAndReturnUser
          (string logoPath, bool isSigner1DetailsInMemory, ProgramResetType programResetType, bool shouldGetUiViewCatch)
        {

            // Arrange 
            string signer1CertId = "test";
            string Changedsigner1CertId = "Changed";
            long docsLimit = 1000;
            long docsUseage = 500;
            long documentsPerMonth = 1000;
            long brokenDocumentsPerMonth = 0;


            CompanySigner1Details companySigner1Details = new CompanySigner1Details() { CertId = signer1CertId };
            UIViewLicense uiViewLicense = new UIViewLicense() { ShouldShowSelfSign = false };
            UIViewLicense uiViewLicenseProgram = new UIViewLicense() { ShouldShowSelfSign = false };
            ProfileProgram profile = new ProfileProgram() { ViewLicense = uiViewLicense };
            User user = new User() { Id = sampleGuid, ProfileProgram = profile, CompanyLogo = logoPath, CompanyId = sampleGuid,GroupId = Guid.Parse( GROUP_GUID) };
            _userConnector.Setup(x => x.Read(It.IsAny<User>())).ReturnsAsync(user);
            ProgramUtilization programUtilization = new ProgramUtilization() { ProgramResetType = programResetType, Expired = DateTime.UtcNow, DocumentsLimit = docsLimit, DocumentsUsage = docsUseage };
            CompanyConfiguration companyConfiguration = new CompanyConfiguration();
            Company freeAccountCompany = new Company() { Id = sampleGuid, ProgramUtilization = programUtilization, CompanyConfiguration = companyConfiguration };
            Program program = new Program() { DocumentsPerMonth = documentsPerMonth, UIViewLicense = uiViewLicenseProgram };
            if (programResetType != ProgramResetType.Monthly)
                program.DocumentsPerMonth = brokenDocumentsPerMonth;
            _programConnectorMock.Setup(x => x.Read(It.IsAny<Program>())).ReturnsAsync(program);
            _programUtilizationConnectorMock.Setup(x => x.Read(It.IsAny<ProgramUtilization>())).ReturnsAsync(programUtilization);
            _dater.Setup(x => x.UtcNow()).Returns(DateTime.UtcNow);

            if (!isSigner1DetailsInMemory)  
            {
                user.CompanyId = sampleGuid;
                freeAccountCompany.CompanySigner1Details = companySigner1Details;
                companySigner1Details.CertId = Changedsigner1CertId;
            }
            _companyConnectorMock.Setup(x => x.Read(It.IsAny<Company>())).ReturnsAsync(freeAccountCompany);
            SetupsForGetUser(freeAccountCompany, user, isSigner1DetailsInMemory);
            _userFileWrapper.Setup(x => x.SetCompanyLogo(It.IsAny<User>())).Callback(() => user.CompanyLogo = logoPath);

         

            _weSignLicense.SetupGet(x => x.UIViewLicense).Returns(uiViewLicense);
            _licenseMock.Setup(x => x.GetLicenseInformation()).Returns(_weSignLicense.Object);


            if (shouldGetUiViewCatch)
            {
                _licenseMock.Setup(x => x.GetLicenseInformation()).Throws(new Exception());
                _weSignLicense.Object.UIViewLicense.ShouldShowSelfSign = true;
            }

            // Action
            (var action, companySigner1Details) = await _userHandler.GetUser();
            // Assert
            Assert.Equal(0, action.ProfileProgram.RemainingDocuments);
            Assert.Equal(1000, action.ProfileProgram.DocumentsLimits);
            Assert.Equal(logoPath, action.CompanyLogo);
            Assert.Equal(Changedsigner1CertId, companySigner1Details.CertId);
            Assert.False(user.ProfileProgram.ViewLicense.ShouldShowSelfSign);
        }


        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task GetUser_WhenUserIsInMemory_ShouldReturnUserAndChangeSigner1Details(bool isSigner1DetailsInMemory)
        {
            // Arrange
            var identity = new ClaimsIdentity();
            if (isSigner1DetailsInMemory)

                identity = new ClaimsIdentity(new List<Claim>() { new Claim(ClaimTypes.Sid, GetGuidForUserFromMemory(isSigner1DetailsInMemory).ToString()),
                                                                  new Claim( ClaimTypes.PrimaryGroupSid, GROUP_GUID) });
            else
                identity = new ClaimsIdentity(new List<Claim>() { new Claim(ClaimTypes.Sid, GetGuidForUserFromMemory(isSigner1DetailsInMemory).ToString()),
                 new Claim( ClaimTypes.PrimaryGroupSid, GROUP_GUID) });
            ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(identity);
            UsersHandler userHandlerWhenUserInMemory = new
                 UsersHandler(claimsPrincipal, _userConnector.Object, _userTokenConnector.Object, _groupConnectorMock.Object,
                _programUtilizationConnectorMock.Object, _programConnectorMock.Object, _companyConnectorMock.Object, _userPasswordHistoryConnector.Object,
                _emailMock.Object, _dater.Object,
                _pkbdf2HandlerMock.Object, _jwtMock.Object, _loggerMock.Object, _oneTimeTokensMock.Object, _generalSetting,
                _configurationMock.Object,
                _licenseMock.Object, _memoryCacheMock, _reCaptchaSettings,
                _httpClientMock.Object, _lmsWrapperConnectorService.Object,
                _certificate.Object, _filesWrapper,
                _sendingMessageHandler.Object, _encryptor.Object);


           
            _userFileWrapper.Setup(x => x.SetCompanyLogo(It.IsAny<User>()));
            string signer1CertId = "test";
            string Changedsigner1CertId = "Changed";
            Company company = new Company();

            User user = new User() { Id = GetGuidForUserFromMemory(isSigner1DetailsInMemory), CompanyId = sampleGuid , GroupId = Guid.Parse(GROUP_GUID)};
            if (isSigner1DetailsInMemory)
                user.CompanyId = differentSampleGuid;

            CompanySigner1Details companySigner1Details = new CompanySigner1Details() { CertId = signer1CertId };
            if (!isSigner1DetailsInMemory)
            {
                companySigner1Details.CertId = Changedsigner1CertId;
                company.CompanySigner1Details = companySigner1Details;    
            }
            _companyConnectorMock.Setup(x => x.Read(It.IsAny<Company>())).ReturnsAsync(company);
            
            

            // Action
            (var action , companySigner1Details) = await userHandlerWhenUserInMemory.GetUser();

            // Assert
            Assert.Equal(Changedsigner1CertId, companySigner1Details.CertId);
            Assert.IsType<User>(action);

        }

        [Fact]
        public async Task GetUser_WhenUserInMemoryNullAndUserInDbNull_ShouldThrowInvalidOperationException()
        {
            // Arrange
            CompanySigner1Details companySigner1Details = new CompanySigner1Details();
            _userConnector.Setup(x => x.Read(It.IsAny<User>())).ReturnsAsync((User)null);

            // Assert
         var actual =   await Assert.ThrowsAsync<InvalidOperationException>(() => _userHandler.GetUser());

        }
        #region private methods for GetUser

        private void SetupsForGetUser(Company company, User user, bool isUserSigner1detailsInMemory)
        {
            SmsConfiguration smsConfiguration = new SmsConfiguration();
            Configuration configuration = new Configuration();
            // Changes User.CompanyID in cases that simulate a situtation where CompanySigner1Details are in memory
            if (isUserSigner1detailsInMemory)
                _configurationMock.Setup(x => x.ReadAppConfiguration()).ReturnsAsync(configuration).Callback(() => user.CompanyId = differentSampleGuid);
            else
                _configurationMock.Setup(x => x.ReadAppConfiguration()).ReturnsAsync(configuration);
            _companyConnectorMock.Setup(x => x.Read(It.IsAny<Company>())).ReturnsAsync(company);
            _configurationMock.Setup(x => x.GetSmsConfiguration(It.IsAny<User>(), It.IsAny<Configuration>(), It.IsAny<CompanyConfiguration>())).ReturnsAsync(smsConfiguration);

        }

        private Guid GetGuidForUserFromMemory(bool isUserSigner1detailsInMemory=true)
        {
            if (isUserSigner1detailsInMemory)
                return Guid.Parse("00000000-0000-0000-0000-000000000003");
            return Guid.Parse("00000000-0000-0000-0000-000000000004");
        }
        #endregion
        #endregion

        #region ChangeExpiredPasswordFlow


   
        [Fact]
        public async Task ChangeExpiredPasswordFlow_oldPasswordIsWrong_ThrowInvalidOperationException()
        {
            UserTokens userTokens = new UserTokens()
            {
                AuthToken = Consts.EXPIRED_PASSWORD,
                RefreshTokenExpiredTime = DateTime.UtcNow.AddMinutes(3),
            };
            User user = new User() ;
            Company company = new Company()
            {
                CompanyConfiguration = new CompanyConfiguration()
                {
                    MinimumPasswordLength = 1
                }
            };
            string oldPassword = "oldPassword";
            string newPassword = "newPassword";
            _dater.Setup(x => x.UtcNow()).Returns(DateTime.UtcNow);
            _companyConnectorMock.Setup(x => x.Read(It.IsAny<Company>())).ReturnsAsync(company);
            _userTokenConnector.Setup(x => x.ReadTokenByRefreshToken(It.IsAny<UserTokens>(), It.IsAny<bool>())).ReturnsAsync(userTokens);
            _userConnector.Setup(x => x.Read(It.IsAny<User>())).ReturnsAsync(user);
            _pkbdf2HandlerMock.Setup(x => x.Check(It.IsAny<string>(), It.IsAny<string>())).Returns(false);
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _userHandler.ChangeExpiredPasswordFlow(oldPassword, newPassword, userTokens));
            // Assert
            Assert.Equal(ResultCode.InvalidCredential.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task ChangeExpiredPasswordFlow_newPasswordShorterThenMinimumPasswordLength_ThrowInvalidOperationException()
        {
            UserTokens userTokens = new UserTokens()
            {
                AuthToken = Consts.EXPIRED_PASSWORD,
                RefreshTokenExpiredTime = DateTime.UtcNow.AddMinutes(3),
            };
            User user = new User();
            Company company = new Company()
            {
                CompanyConfiguration = new CompanyConfiguration()
                {
                    MinimumPasswordLength = 100
                }
            };
            string oldPassword = "oldPassword";
            string newPassword = "newPassword";
            _dater.Setup(x => x.UtcNow()).Returns(DateTime.UtcNow);
            _companyConnectorMock.Setup(x => x.Read(It.IsAny<Company>())).ReturnsAsync(company);
            _userTokenConnector.Setup(x => x.ReadTokenByRefreshToken(It.IsAny<UserTokens>(), It.IsAny<bool>())).ReturnsAsync(userTokens);
            _userConnector.Setup(x => x.Read(It.IsAny<User>())).ReturnsAsync(user);
            _pkbdf2HandlerMock.Setup(x => x.Check(It.IsAny<string>(), It.IsAny<string>())).Returns(false);
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _userHandler.ChangeExpiredPasswordFlow(oldPassword, newPassword, userTokens));
            // Assert
            Assert.Equal(ResultCode.PasswordIsTooShortToCompanyPolicy.GetNumericString(), actual.Message);
        }
        [Fact]
        public async Task ChangeExpiredPasswordFlow_TokenNotExist_ThrowInvalidOperationException()
        {
            UserTokens userTokens = null;
            string oldPassword = "oldPassword";
            string newPassword = "newPassword";
            _userTokenConnector.Setup(x => x.ReadTokenByRefreshToken(It.IsAny<UserTokens>(), It.IsAny<bool>())).ReturnsAsync(userTokens);
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _userHandler.ChangeExpiredPasswordFlow(oldPassword, newPassword, userTokens));

            // Assert
            Assert.Equal(ResultCode.InvalidRefreshToken.GetNumericString(), actual.Message);
        }
        [Fact]
        public async Task ChangeExpiredPasswordFlow_UserNotExist_ThrowInvalidOperationException()
        {
            UserTokens userTokens = new UserTokens()
            {
                AuthToken = Consts.EXPIRED_PASSWORD,
                RefreshTokenExpiredTime = DateTime.UtcNow.AddMinutes(3),
            };
            User nullUser = null;
            string oldPassword = "oldPassword";
            string newPassword = "newPassword";
            _dater.Setup(x => x.UtcNow()).Returns(DateTime.UtcNow);
            _userTokenConnector.Setup(x => x.ReadTokenByRefreshToken(It.IsAny<UserTokens>(), It.IsAny<bool>())).ReturnsAsync(userTokens);
            _userConnector.Setup(x => x.Read(It.IsAny<User>())).ReturnsAsync(nullUser);
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _userHandler.ChangeExpiredPasswordFlow(oldPassword, newPassword, userTokens));

            // Assert
            Assert.Equal(ResultCode.InvalidRefreshToken.GetNumericString(), actual.Message);
        }
        [Fact]
        public async Task ChangeExpiredPasswordFlow_TokenExistButNotInEXPIRED_PASSWORDSession_ThrowInvalidOperationException()
        {
            UserTokens userTokens = new UserTokens()
            {
                AuthToken = "BLABLA"
            };
            string oldPassword = "oldPassword";
            string newPassword = "newPassword";
            _userTokenConnector.Setup(x => x.ReadTokenByRefreshToken(It.IsAny<UserTokens>(), It.IsAny<bool>())).ReturnsAsync(userTokens);
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _userHandler.ChangeExpiredPasswordFlow(oldPassword, newPassword, userTokens));

            // Assert
            Assert.Equal(ResultCode.InvalidRefreshToken.GetNumericString(), actual.Message);
        }
    
        [Fact]
        public async Task ChangeExpiredPasswordFlow_TokenExistTimeOut_ThrowInvalidOperationException()
        {
            UserTokens userTokens = new UserTokens()
            {
                AuthToken = Consts.EXPIRED_PASSWORD,
                RefreshTokenExpiredTime = DateTime.UtcNow.AddMinutes(-1),
            };
            User nullUser = null;
            string oldPassword = "oldPassword";
            string newPassword = "newPassword";
            _dater.Setup(x => x.UtcNow()).Returns(DateTime.UtcNow);
            _userTokenConnector.Setup(x => x.ReadTokenByRefreshToken(It.IsAny<UserTokens>(), It.IsAny<bool>())).ReturnsAsync(userTokens);
            _userConnector.Setup(x => x.Read(It.IsAny<User>())).ReturnsAsync(nullUser);
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _userHandler.ChangeExpiredPasswordFlow(oldPassword, newPassword, userTokens));

            // Assert
            Assert.Equal(ResultCode.PasswordSessionExpired.GetNumericString(), actual.Message);
        }
    

        [Fact]
        public void ChangeExpiredPasswordFlow_EmptyPasswordHistory_Valid()
        {
            // Arrange 
            User user = new User();
            Company company = new Company() 
            { 
                CompanyConfiguration = new CompanyConfiguration()
                { 
                    RecentPasswordsAmount = 0
                }
            };
            string oldPassword = "oldPassword";
            string newPassword = "newPassword";
            UserTokens userTokens = new UserTokens()
            {
                AuthToken = Consts.EXPIRED_PASSWORD,
                RefreshTokenExpiredTime = DateTime.UtcNow.AddMinutes(1),
            };
            _dater.Setup(x => x.UtcNow()).Returns(DateTime.UtcNow);
            _userTokenConnector.Setup(x => x.ReadTokenByRefreshToken(It.IsAny<UserTokens>(), It.IsAny<bool>())).ReturnsAsync(userTokens);
            

            _pkbdf2HandlerMock.Setup(_ => _.Check(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
            _companyConnectorMock.Setup(_ => _.Read(It.IsAny<Company>())).ReturnsAsync(company);
            
            _userConnector.Setup(_ => _.Read(It.IsAny<User>())).ReturnsAsync(user);
            

            // Action
            _userHandler.ChangeExpiredPasswordFlow(oldPassword, newPassword, userTokens);

            // Assert
            _userPasswordHistoryConnector.Verify(_ => _.DeleteAllByUserId(It.IsAny<Guid>()), Times.Once);
        }

        [Fact]
        public async Task ChangeExpiredPasswordFlow_PasswordAlreadyUsed_ThrowInvalidOperationException()
        {

            // Arrange 
            User user = new User();
            Company company = new Company()
            {
                CompanyConfiguration = new CompanyConfiguration()
                {
                    RecentPasswordsAmount = 1
                }
            };
            string oldPassword = "oldPassword";
            string newPassword = "newPassword";
            UserTokens userTokens = new UserTokens()
            {
                AuthToken = Consts.EXPIRED_PASSWORD,
                RefreshTokenExpiredTime = DateTime.UtcNow.AddMinutes(1),
            };
            IEnumerable<UserPasswordHistory> history = new List<UserPasswordHistory>()
            {
                new UserPasswordHistory(), new UserPasswordHistory()
            };
            _dater.Setup(x => x.UtcNow()).Returns(DateTime.UtcNow);
            _oneTimeTokensMock.Setup(_ => _.CheckRemoteLoginToken(It.IsAny<UserTokens>())).ReturnsAsync(user);
            _pkbdf2HandlerMock.Setup(_ => _.Check(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
            _companyConnectorMock.Setup(_ => _.Read(It.IsAny<Company>())).ReturnsAsync(company);
            
            _userConnector.Setup(_ => _.Read(It.IsAny<User>())).ReturnsAsync(user);
            _userTokenConnector.Setup(x => x.ReadTokenByRefreshToken(It.IsAny<UserTokens>(), It.IsAny<bool>())).ReturnsAsync(userTokens);
            _userPasswordHistoryConnector.Setup(_ => _.ReadAllByUserId(It.IsAny<Guid>())).Returns(history);

            // Action
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _userHandler.ChangeExpiredPasswordFlow(oldPassword, newPassword, userTokens));

            // Assert
            Assert.Equal(ResultCode.PasswordAlreadyUsed.GetNumericString(), actual.Message);
        }

        #endregion

    }
}
