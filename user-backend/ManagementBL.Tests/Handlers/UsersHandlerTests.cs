using Common.Interfaces;
using Common.Interfaces.DB;
using Common.Interfaces.ManagementApp;
using ManagementBL.Handlers;
using Moq;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using Common.Models;
using Common.Models.ManagementApp;
using Common.Models.Users;
using System.Linq;
using Xunit;
using Common.Enums.Results;
using Common.Extensions;
using Common.Enums.Users;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Serilog;
using Common.Consts;
using Common.Interfaces.Emails;
using Common.Interfaces.UserApp;
using Common.Models.Settings;
using Common.Interfaces.PDF;
using System.IO.Abstractions;
using Microsoft.Extensions.Options;
using Common.Enums;
using System.Security.Principal;
using Common.Models.Files.PDF;
using Common.Interfaces.Files;
using Common.Models.Configurations;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace ManagementBL.Tests.Handlers
{
    public class UsersHandlerTests
    {
        private readonly Guid ID = new Guid("00000000-0000-0000-0000-000000000001");
        private readonly Guid differentID = new Guid("00000000-0000-0000-0000-000000000002");

        private Mock<IServiceScopeFactory> _serviceScopeFactoryMock;

        private readonly Mock<ILogger> _logger;
        private readonly Mock<IJWT> _jwt;
        private readonly Mock<IPBKDF2> _pbkdf2;
        private readonly Mock<IEmail> _email;
        private readonly Mock<IDater> _dater;
        private readonly Mock<Common.Interfaces.ManagementApp.ILicense> _license;
        private Mock<IOneTimeTokens> _oneTimeTokens;
        private ClaimsPrincipal _userClaims;
        private Common.Interfaces.ManagementApp.IUsers _users;
        private Mock<IFileSystem> _fileSystem;
        private Mock<IDataUriScheme> _dataUriScheme;
        private Mock<ITemplatePdf> _templateHandler;
        private readonly IOptions<FolderSettings> _folderSettingsMock;
        private readonly Mock<ISymmetricEncryptor> _symmetricEncryptorMock;
        private readonly Mock<IFilesWrapper> _filesWrapperMock;



        private readonly Mock<ICompanyConnector> _companyConnectorMock;
        private readonly Mock<IUserConnector> _userConnectorMock;
        private readonly Mock<IGroupConnector> _groupsConnector;
        private readonly Mock<ITemplateConnector> _templateConnectorMock;
        private readonly Mock<IProgramUtilizationConnector> _programUtilizationConnector;
        private readonly Mock<IProgramConnector> _programConnectorMock;
        private readonly Mock<IUserPasswordHistoryConnector> _userPasswordHistoryConnectorMock;


        
        public UsersHandlerTests()
        {
            _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            _oneTimeTokens = new Mock<IOneTimeTokens>();
            _logger = new Mock<ILogger>();
            _jwt = new Mock<IJWT>();
            _pbkdf2 = new Mock<IPBKDF2>();
            _email = new Mock<IEmail>();
            _dater = new Mock<IDater>();
            _license = new Mock<Common.Interfaces.ManagementApp.ILicense>();
            _fileSystem = new Mock<IFileSystem>();
            _dataUriScheme = new Mock<IDataUriScheme>();
            _templateHandler = new Mock<ITemplatePdf>();
            _folderSettingsMock = Options.Create(new FolderSettings() { ContactSeals = @"c:\" });
            _symmetricEncryptorMock = new Mock<ISymmetricEncryptor>();
            _filesWrapperMock = new Mock<IFilesWrapper>();
            var identity = new ClaimsIdentity(new List<Claim>() { new Claim(ClaimTypes.Sid, ID.ToString()) });

            
            _userConnectorMock = new Mock<IUserConnector>();
            _groupsConnector = new Mock<IGroupConnector>();
                _programUtilizationConnector = new Mock<IProgramUtilizationConnector>();
                _programConnectorMock = new Mock<IProgramConnector>();
                _userPasswordHistoryConnectorMock = new Mock<IUserPasswordHistoryConnector>();
                _templateConnectorMock = new Mock<ITemplateConnector>();

            var serviceCollection = new ServiceCollection();
            _companyConnectorMock = new Mock<ICompanyConnector>();
            // Add any DI stuff here:
            
            serviceCollection.AddSingleton<IUserConnector>(_userConnectorMock.Object);
            serviceCollection.AddSingleton<IGroupConnector>(_groupsConnector.Object);
            serviceCollection.AddSingleton<ITemplateConnector>(_templateConnectorMock.Object);
            serviceCollection.AddSingleton<IProgramUtilizationConnector>(_programUtilizationConnector.Object);
            serviceCollection.AddSingleton<IProgramConnector>(_programConnectorMock.Object);
            serviceCollection.AddSingleton<IUserPasswordHistoryConnector>(_userPasswordHistoryConnectorMock.Object);
            serviceCollection.AddSingleton<ICompanyConnector>(_companyConnectorMock.Object);


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


            _userClaims = new ClaimsPrincipal(identity);

            _users = new UsersHandler(_serviceScopeFactoryMock.Object, _oneTimeTokens.Object, _jwt.Object,
                _pbkdf2.Object, _userClaims, _logger.Object, _email.Object, _dater.Object,
               _dataUriScheme.Object, _templateHandler.Object,  _filesWrapperMock.Object);
        }

        #region TryLogin

        [Fact]
        public async Task TryLogin_NullUser_ThrowException()
        {
            User user = null;
            UserTokens userTokens = new UserTokens();
            _userConnectorMock.Setup(x => x.Read(It.IsAny<User>())).ReturnsAsync(user);

            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _users.TryLogin(user));

            Assert.Equal(ResultCode.InvalidCredential.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task TryLogin_NotActivateUser_ThrowException()
        {
            User user = new User { Type = UserType.SystemAdmin, Status = UserStatus.Suspended };
            UserTokens userTokens = new UserTokens();
            _userConnectorMock.Setup(x => x.Read(It.IsAny<User>())).ReturnsAsync(user);

            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _users.TryLogin(user));

            Assert.Equal(ResultCode.ActivationRequired.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task TryLogin_InvalidCredential_ThrowException()
        {
            User user = new User { Type = UserType.SystemAdmin, Status = UserStatus.Activated };
            UserTokens userTokens = new UserTokens();
            _userConnectorMock.Setup(x => x.Read(It.IsAny<User>())).ReturnsAsync(user);
            _pbkdf2.Setup(x => x.Check(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _users.TryLogin(user));

            Assert.Equal(ResultCode.InvalidCredential.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task TryLogin_ValidCredential_Success()
        {
            User user = new User { Type = UserType.SystemAdmin, Status = UserStatus.Activated };
            UserTokens userTokens = new UserTokens();
            _userConnectorMock.Setup(x => x.Read(It.IsAny<User>())).ReturnsAsync(user);
            _pbkdf2.Setup(x => x.Check(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

            await _users.TryLogin(user);
        }

        #endregion

        #region Delete

        [Fact]
        public async Task Delete_NullInput_ThrowException()
        {
            User user = null;
            User currentAdminUser = new User();
            _userConnectorMock.Setup(x => x.Exists(It.IsAny<User>())).ReturnsAsync(false);
            _userConnectorMock.Setup(x => x.Read(It.IsAny<User>())).ReturnsAsync(currentAdminUser);
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _users.Delete(user));

            Assert.Equal(ResultCode.UserNotExist.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task Delete_UserDeletionFromDbFailed_ThrowException()
        {
            User user = new User() { Id = ID };
            User otherUser = new User() { Id = differentID };
            List<User> users = new List<User>() { user, otherUser };
            _userConnectorMock.Setup(x => x.Exists(It.IsAny<User>())).ReturnsAsync(true);
            _userConnectorMock.Setup(x => x.Delete(It.IsAny<User>())).Throws<Exception>();
            _userConnectorMock.Setup(x => x.Read(It.IsAny<User>())).ReturnsAsync(user);
            _userConnectorMock.Setup(x => x.GetAllUsersInCompany(It.IsAny<Company>())).Returns(users);
            var actual = await Assert.ThrowsAsync<Exception>(() => _users.Delete(user));

            _groupsConnector.Verify(x => x.Delete(It.IsAny<Group>()), Times.Never);
        }


        [Fact]
        public async Task Delete_ValidUser_Success()
        {
            User user = new User() { Id = ID };
            User otherUser = new User() { Id = differentID };
            List<User> users = new List<User>() { user, otherUser };
            _userConnectorMock.Setup(x => x.Exists(It.IsAny<User>())).ReturnsAsync(true);
            _userConnectorMock.Setup(x => x.Read(It.IsAny<User>())).ReturnsAsync(user);
            _groupsConnector.Setup(x => x.Delete(It.IsAny<Group>()));
            _programConnectorMock.Setup(x => x.IsFreeTrialUser(It.IsAny<User>())).Returns(false);
            _programUtilizationConnector.Setup(x => x.UpdateUsersAmount(It.IsAny<User>(), It.IsAny<CalcOperation>(), 1));
            _userConnectorMock.Setup(x => x.GetAllUsersInCompany(It.IsAny<Company>())).Returns(users);

            await _users.Delete(user);

            _userConnectorMock.Verify(x => x.Delete(It.IsAny<User>()), Times.Once);
        }

        #endregion

        #region ReadTemplates

        [Fact]
        public async Task ReadTemplates_UserIsNull_ShouldThrowException()
        {
            User user = null;

            var actual = await Assert.ThrowsAsync<Exception>(() => _users.ReadTemplates(user));

            Assert.Equal("Null input - user is null", actual.Message);
        }

        [Fact]
        public async Task ReadTemplates_ReturnTemplates_Success()
        {
            User user = new User();
            int totalCount = -1;
            IEnumerable<Template> templates = new List<Template>()
            {
                new Template()
                {
                    Id = Guid.NewGuid(),
                    Name = "template1"
                },
                new Template()
                {
                    Id = Guid.NewGuid(),
                    Name = "template2"
                },
            };

            _userConnectorMock.Setup(x => x.Read(It.IsAny<User>())).ReturnsAsync(user);
            _templateConnectorMock.Setup(x => x.Read(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<bool>(), out totalCount)).Returns(templates);

            var actual =await _users.ReadTemplates(user);

            Assert.IsType<Dictionary<Guid, string>>(actual);
            Assert.Equal(templates.Count(), actual.Count);


        }

        #endregion

        #region CreateHtmlTemplate

        [Fact]
        public async Task CreateHtmlTemplate_TemplateIsNull_ShouldThrowException()
        {
            User user = new User();

            Template template = null;

            _userConnectorMock.Setup(x => x.Read(It.IsAny<User>())).ReturnsAsync(user);
            _templateConnectorMock.Setup(x => x.Read(It.IsAny<Template>())).ReturnsAsync(template);

            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _users.CreateHtmlTemplate(new User(), new Template(), "", ""));

            Assert.Equal(ResultCode.InvalidTemplateId.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task CreateHtmlTemplate_EmptyPdfFields_ShouldThrowException()
        {
            User user = new User();

            Template template = new Template();
            PDFFields pdfFields = new PDFFields();

            _userConnectorMock.Setup(x => x.Read(It.IsAny<User>())).ReturnsAsync(user);
            _templateConnectorMock.Setup(x => x.Read(It.IsAny<Template>())).ReturnsAsync(template);

            // ValidatePdfFieldsExistsInHtmlTemplate
            _templateHandler.Setup(x => x.Load(template.Id, It.IsAny<bool>())).Returns(true);
            _templateHandler.Setup(x => x.GetAllFields(It.IsAny<bool>())).Returns(pdfFields);

            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _users.CreateHtmlTemplate(new User(), new Template(), "", ""));

            Assert.Equal(ResultCode.NotAllFieldsExistsInDocuments.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task CreateHtmlTemplate_FieldNameNotExist_ShouldThrowException()
        {
            User user = new User();

            Template template = new Template();
            PDFFields pdfFields = new PDFFields()
            {
                TextFields = new List<TextField>()
                {
                    new TextField()
                    {
                        Name=""
                    }
                }
            };

            _userConnectorMock.Setup(x => x.Read(It.IsAny<User>())).ReturnsAsync(user);
            _templateConnectorMock.Setup(x => x.Read(It.IsAny<Template>())).ReturnsAsync(template);

            // ValidatePdfFieldsExistsInHtmlTemplate
            _templateHandler.Setup(x => x.Load(template.Id, It.IsAny<bool>())).Returns(true);
            _templateHandler.Setup(x => x.GetAllFields(It.IsAny<bool>())).Returns(pdfFields);
            _dataUriScheme.Setup(x => x.GetBytes(It.IsAny<string>())).Returns(new byte[4]);
            _fileSystem.Setup(x => x.Path.GetTempPath());
            _fileSystem.Setup(x => x.Path.Combine(It.IsAny<string[]>()));
            _fileSystem.Setup(x => x.File.WriteAllBytes(It.IsAny<string>(), It.IsAny<byte[]>()));
            _fileSystem.Setup(x => x.File.ReadAllText(It.IsAny<string>())).Returns("");


            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _users.CreateHtmlTemplate(new User(), new Template(), "", ""));

            Assert.Equal(ResultCode.FieldNameNotExist.GetNumericString(), actual.Message);
        }

        #endregion

        #region ResetPassword

        [Fact]
        public async Task ResetPassword_UserNull_ThrowException()
        {
            User user = null;

            var actual = await Assert.ThrowsAsync<Exception>(() => _users.ResetPassword(user));

            Assert.Equal("Null input - user is null", actual.Message);
        }

        [Fact]
        public async Task ResetPassword_NoUserInClaims_ThrowException()
        {
            var user = new User();
            _userClaims = new ClaimsPrincipal();

            _users = new UsersHandler(_serviceScopeFactoryMock.Object, _oneTimeTokens.Object, _jwt.Object,
                _pbkdf2.Object, _userClaims, _logger.Object, _email.Object, _dater.Object,
               _dataUriScheme.Object, _templateHandler.Object, _filesWrapperMock.Object);

            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _users.ResetPassword(user));

            Assert.Equal(ResultCode.InvalidToken.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task ResetPassword_NonSystemAdminUser_ThrowException()
        {
            var user = new User { Type = UserType.CompanyAdmin };
            _userConnectorMock.Setup(x => x.Read(It.IsAny<User>())).ReturnsAsync(user);

            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _users.ResetPassword(user));

            Assert.Equal(ResultCode.InvalidUserType.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task    ResetPassword_ValidSystemAdminUser_Success()
        {
            var user = new User { Type = UserType.SystemAdmin };
            _userConnectorMock.Setup(x => x.Read(It.IsAny<User>())).ReturnsAsync(user);
            _companyConnectorMock.Setup(x => x.Read(It.IsAny<Company>())).ReturnsAsync(new Company { CompanyConfiguration = new CompanyConfiguration() });
            _userPasswordHistoryConnectorMock.Setup(x => x.DeleteAllByUserId(It.IsAny<Guid>()));
            await _users.ResetPassword(user);

            _pbkdf2.Verify(x => x.Generate(It.IsAny<string>()), Times.Once);
            _userConnectorMock.Verify(x => x.Update(It.IsAny<User>(), It.IsAny<bool>()), Times.Once);
        }

        [Fact]
        public void ResetPassword_FirstPasswordNotCreateHistoryPassword_Success()
        {
            // Arrange
            var user = new User() { Type = UserType.SystemAdmin, Password = null };
            var company = new Company { CompanyConfiguration = new CompanyConfiguration() 
            { 
                RecentPasswordsAmount = 1
            } };
            _userConnectorMock.Setup(x => x.Read(It.IsAny<User>())).ReturnsAsync(user);
            _companyConnectorMock.Setup(x => x.Read(It.IsAny<Company>())).ReturnsAsync(company);
            _userPasswordHistoryConnectorMock.Setup(x => x.ReadAllByUserId(It.IsAny<Guid>())).Returns(new List<UserPasswordHistory>());
            
            // Action
            _users.ResetPassword(user);

            // Assert
            _userPasswordHistoryConnectorMock.Verify(_ => _.Create(It.IsAny<UserPasswordHistory>()), Times.Never);
        }

        #endregion

        #region UpdateEmail

        [Fact]
        public async Task UpdateEmail_UserIsNull_ShouldThrowException()
        {
            var actual = await Assert.ThrowsAsync<Exception>(() => _users.UpdateEmail(null));

            Assert.Equal("Null input - user is null", actual.Message);
        }

        [Fact]
        public async  Task UpdateEmail_InvalidUserType_ShouldThrowException()
        {
            User user = new User()
            {

            };

            _userConnectorMock.Setup(x => x.Read(It.IsAny<User>())).ReturnsAsync(user);

            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _users.UpdateEmail(user));

            Assert.Equal(ResultCode.InvalidUserType.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task UpdateEmail_Success()
        {
            User user = new User()
            {
                Email = "gal@comda.co.il"
            };
            User dbUser = new User()
            {
                Email = "eran@comda.co.uk",
                Type = UserType.SystemAdmin
            };

            _userConnectorMock.Setup(x => x.Read(user)).ReturnsAsync(dbUser);

            await _users.UpdateEmail(user);

            Assert.Equal(dbUser.Email, user.Email);
        }

        #endregion

        #region CreateUserFromManagment

        [Fact]
        public async Task CreateUserFromManagment_InvalidToken_UserIsNull_ShouldThrowException()
        {
            User user = null;

            _userConnectorMock.Setup(x => x.Read(It.IsAny<User>())).ReturnsAsync(user);

            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _users.CreateUserFromManagment(new User()));

            Assert.Equal(ResultCode.InvalidToken.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task CreateUserFromManagment_EmailBelongToOtherCompany_ShouldThrowException()
        {
            User user = new User();

            _userConnectorMock.Setup(x => x.Exists(It.IsAny<User>())).ReturnsAsync(true);
            _userConnectorMock.Setup(x => x.Read(It.IsAny<User>())).ReturnsAsync(user);

            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _users.CreateUserFromManagment(new User()));

            Assert.Equal(ResultCode.EmailBelongToOtherCompany.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task CreateUserFromManagment_CompanyNotExist_ShouldThrowException()
        {
            User user = new User();
            Company company = null;

            _userConnectorMock.Setup(x => x.Exists(It.IsAny<User>())).ReturnsAsync(false);
            _userConnectorMock.Setup(x => x.Read(It.IsAny<User>())).ReturnsAsync(user);
            _companyConnectorMock.Setup(x => x.Read(It.IsAny<Company>())).ReturnsAsync(company);

            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _users.CreateUserFromManagment(new User()));

            Assert.Equal(ResultCode.CompanyNotExist.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task CreateUserFromManagment_InvalidGroupId_ShouldThrowException()
        {
            User user = new User();
            Company company = new Company();
            Group group = null;

            _userConnectorMock.Setup(x => x.Exists(It.IsAny<User>())).ReturnsAsync(false);
            _userConnectorMock.Setup(x => x.Read(It.IsAny<User>())).ReturnsAsync(user);
            _companyConnectorMock.Setup(x => x.Read(It.IsAny<Company>())).ReturnsAsync(company);
            _groupsConnector.Setup(x => x.Read(It.IsAny<Group>())).ReturnsAsync(group);

            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _users.CreateUserFromManagment(new User()));

            Assert.Equal(ResultCode.InvalidGroupId.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task CreateUserFromManagment_UsersExceedLicenseLimit_ShouldThrowException()
        {
            User user = new User();
            Company company = new Company();
            Group group = new Group();
            Program program = new Program()
            {
                Users = 5
            };

            List<User> users = new List<User>();
            for (int i = 0; i < 5; i++)
            {
                users.Add(new User() { Id = Guid.NewGuid() });
            }


            _userConnectorMock.Setup(x => x.Exists(It.IsAny<User>())).ReturnsAsync(false);
            _userConnectorMock.Setup(x => x.Read(It.IsAny<User>())).ReturnsAsync(user);
            _companyConnectorMock.Setup(x => x.Read(It.IsAny<Company>())).ReturnsAsync(company);
            _groupsConnector.Setup(x => x.Read(It.IsAny<Group>())).ReturnsAsync(group);
            _programConnectorMock.Setup(x => x.Read(It.IsAny<Program>())).ReturnsAsync(program);
            _userConnectorMock.Setup(x => x.GetAllUsersInCompany(It.IsAny<Company>())).Returns(users);

            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _users.CreateUserFromManagment(new User()));

            Assert.Equal(ResultCode.UsersExceedLicenseLimit.GetNumericString(), actual.Message);
        }
        [Fact]
        public async Task CreateUserFromManagment_Success()
        {
            User user = new User();
            Company company = new Company();
            Group group = new Group();
            Program program = new Program()
            {
                Users = 6
            };

            List<User> users = new List<User>();
            for (int i = 0; i < 5; i++)
            {
                users.Add(new User() { Id = Guid.NewGuid() });
            }


            _userConnectorMock.Setup(x => x.Exists(It.IsAny<User>())).ReturnsAsync(false);
            _userConnectorMock.Setup(x => x.Read(It.IsAny<User>())).ReturnsAsync(user);
            _companyConnectorMock.Setup(x => x.Read(It.IsAny<Company>())).ReturnsAsync(company);
            _groupsConnector.Setup(x => x.Read(It.IsAny<Group>())).ReturnsAsync(group);
            _programConnectorMock.Setup(x => x.Read(It.IsAny<Program>())).ReturnsAsync(program);
            _userConnectorMock.Setup(x => x.GetAllUsersInCompany(It.IsAny<Company>())).Returns(users);

            await _users.CreateUserFromManagment(new User());

            _userConnectorMock.Verify(x => x.Create(It.IsAny<User>()), Times.Once);

        }

        [Fact]
        public async Task CreateUserFromManagment_UserWithInadlidUsername_ShouldThrowException()
        {
            User user = new User() { Email = "sjdkc" };
            Company company = new Company();
            Group group = new Group();
            Program program = new Program()
            {
                Users = 6
            };

            List<User> users = new List<User>();
            for (int i = 0; i < 5; i++)
            {
                users.Add(new User() { Id = Guid.NewGuid() });
            }


            _userConnectorMock.Setup(x => x.Exists(It.IsAny<User>())).ReturnsAsync(false);
            _userConnectorMock.Setup(x => x.Read(It.IsAny<User>())).ReturnsAsync(user);
            _companyConnectorMock.Setup(x => x.Read(It.IsAny<Company>())).ReturnsAsync(company);
            _groupsConnector.Setup(x => x.Read(It.IsAny<Group>())).ReturnsAsync(group);
            _programConnectorMock.Setup(x => x.Read(It.IsAny<Program>())).ReturnsAsync(program);
            _userConnectorMock.Setup(x => x.GetAllUsersInCompany(It.IsAny<Company>())).Returns(users);

            await _users.CreateUserFromManagment(user);

            _userConnectorMock.Verify(x => x.Create(It.IsAny<User>()), Times.Once);

        }
        [Fact]
        public async Task CreateUserFromManagment_UsernameBelongToOtherCompany_ShouldThrowException()
        {
            User user = new User();

            _userConnectorMock.Setup(x => x.Exists(It.IsAny<User>())).ReturnsAsync(true);
            _userConnectorMock.Setup(x => x.Read(It.IsAny<User>())).ReturnsAsync(user);

            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _users.CreateUserFromManagment(new User()));

            Assert.Equal(ResultCode.EmailBelongToOtherCompany.GetNumericString(), actual.Message);
        }
        #endregion

        #region ResendResetPasswordMail

        [Fact]
        public async Task ResendResetPasswordMail_UserIsNull_ShouldThrowException()
        {
            User user = null;

            var actual = await Assert.ThrowsAsync<Exception>(() => _users.ResendResetPasswordMail(user));

            Assert.Equal("Null input - user is null", actual.Message);
        }

        [Fact]
        public async Task ResendResetPasswordMail_UserNotExist_ShouldThrowException()
        {
            User user = new User();
            User dbUser = null;

            _userConnectorMock.Setup(x => x.Read(It.IsAny<User>())).ReturnsAsync(dbUser);

            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _users.ResendResetPasswordMail(user));

            Assert.Equal(ResultCode.UserNotExist.GetNumericString(), actual.Message);
        }

        [Fact]
        public async System.Threading.Tasks.Task ResendResetPasswordMail_InvalidUserType_ShouldThrowException()
        {
            User user = new User();
            User dbUser = new User()
            {
                Type = UserType.SystemAdmin
            };

            _userConnectorMock.Setup(x => x.Read(It.IsAny<User>())).ReturnsAsync(dbUser);

            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _users.ResendResetPasswordMail(user));

            Assert.Equal(ResultCode.InvalidUserType.GetNumericString(), actual.Message);
        }

        #endregion

        #region Read

        [Fact]
        public async Task Read_NoUsersExists_ReturnEmptyList()
        {
            int count = 0;
            _userConnectorMock.Setup(x => x.Read(Consts.EMPTY, 0, Consts.UNLIMITED, null, out count, default, null)).Returns(new List<User>());

            (var users, var _) = await _users.Read(Consts.EMPTY, 0, Consts.UNLIMITED, null);

            Assert.Empty(users);
        }

        [Fact]
        public async Task Read_UsersExists_ReturnNonEmptyList()
        {
            var company = new Company { Id = ID };
            int count = 0;
            _userConnectorMock.Setup(x => x.Read(Consts.EMPTY, 0, Consts.UNLIMITED, null, out count, default, null))
                .Returns(new List<User> { new User() });
            _companyConnectorMock.Setup(x => x.Read(It.IsAny<Company>())).ReturnsAsync(company);
            
            _programConnectorMock.Setup(x => x.Read(It.IsAny<Program>())).ReturnsAsync(new Program());
            _groupsConnector.Setup(x => x.Read(It.IsAny<Group>())).ReturnsAsync(new Group());

            (var users, int totalCount) = await _users.Read(Consts.EMPTY, 0, Consts.UNLIMITED, null);
          

            Assert.Single(users);
        }



        #endregion

        #region GetCurrentUser

        [Fact]
        public async Task GetCurrentUser_UserIsNull_ShouldThrowException()
        {
            User user = null;

            _userConnectorMock.Setup(x => x.Read(It.IsAny<User>())).ReturnsAsync(user);

            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _users.GetCurrentUser());

            Assert.Equal(ResultCode.InvalidToken.GetNumericString(), actual.Message);
        }

        #endregion

        #region UpdateUser

        [Fact]
        public async Task UpdateUser_UserIsNull_ShouldThrowException()
        {
            User user = null;

            _userConnectorMock.Setup(x => x.Read(It.IsAny<User>())).ReturnsAsync(user);

            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _users.UpdateUser(new User()));

            Assert.Equal(ResultCode.UserNotExist.GetNumericString(), actual.Message);
        }

        #endregion
    }
}
