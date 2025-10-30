
using Common.Consts;
using Common.Enums;
using Common.Enums.License;
using Common.Enums.Program;
using Common.Enums.Results;
using Common.Enums.Users;
using Common.Extensions;
using Common.Handlers.Files;
using Common.Interfaces;
using Common.Interfaces.DB;
using Common.Interfaces.Emails;
using Common.Interfaces.Files;
using Common.Interfaces.MessageSending;
using Common.Interfaces.UserApp;
using Common.Models;
using Common.Models.Configurations;
using Common.Models.Programs;
using Common.Models.Settings;
using Common.Models.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Serilog;
using Serilog.Core;
using ServerSignatureService;
using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;

using System.Threading.Tasks;
using Twilio.Jwt.Taskrouter;

namespace BL.Handlers
{
    public class UsersHandler : IUsers
    {
        private readonly string FREE_ACCOUNTS_COMPANY_NAME = "Free Accounts";
        private readonly IEncryptor _encryptor;
        private readonly ClaimsPrincipal _userClaims;
        private readonly IUserPasswordHistoryConnector _userPasswordHistoryConnector;
        private readonly IUserConnector _userConnector;
        private readonly IUserTokenConnector _userTokenConnector;
        private readonly IProgramConnector _programConnector;
        private readonly IProgramUtilizationConnector _programUtilizationConnector;
        private readonly ICompanyConnector _companyConnector;
        private readonly IGroupConnector _groupConnector;
        private readonly IPBKDF2 _pkbdf2Handler;
        private readonly IEmail _email;
        private readonly ILogger _logger;
        private readonly IJWT _jwt;
        private readonly IOneTimeTokens _oneTimeTokens;
        private readonly IConfiguration _configuration;
        private readonly IFilesWrapper _filesWrapper;
        private readonly ISendingMessageHandler _sendingMessageHandler;
        private readonly IDater _dater;
        private readonly ILicense _license;
        private readonly ReCaptchaSettings _reCaptchaSettings;
        private readonly IHttpClientFactory _clientFactory;
        private readonly IMemoryCache _memoryCache;
        private readonly ILMSWrapperConnectorService _lmsWrapperConnectorService;
        private readonly ICertificate _certificate;
        private readonly GeneralSettings _generalSettings;
        private readonly string OTP_CODE_PLACEHOLDER = "[OTP_CODE]";

        public UsersHandler(ClaimsPrincipal user,
            IUserConnector userConnector, IUserTokenConnector userTokenConnector,IGroupConnector groupConnector,
            IProgramUtilizationConnector programUtilizationConnector, IProgramConnector programConnector,
            ICompanyConnector companyConnector, IUserPasswordHistoryConnector userPasswordHistoryConnector,
            IEmail email, IDater dater,
                            IPBKDF2 pkbdf2Handler, IJWT jwt, ILogger logger, IOneTimeTokens oneTimeTokens, IOptions<GeneralSettings> generalSettings,
                            IConfiguration configuration, ILicense license, IMemoryCache memoryCache,
                            IOptions<ReCaptchaSettings> reCaptchaSettings, IHttpClientFactory clientFactory,
                             ILMSWrapperConnectorService lmsWrapperConnectorService, ICertificate certificate,
                             IFilesWrapper filesWrapper,
                              ISendingMessageHandler sendingMessageHandler, IEncryptor encryptor)
        {
            _encryptor = encryptor;
            _userClaims = user;
            _userPasswordHistoryConnector = userPasswordHistoryConnector;
            _userConnector = userConnector;
            _userTokenConnector = userTokenConnector;
            _programConnector = programConnector;
            _programUtilizationConnector = programUtilizationConnector;
            _companyConnector = companyConnector;
            _groupConnector = groupConnector;
            _pkbdf2Handler = pkbdf2Handler;
            _email = email;
            _logger = logger;
            _jwt = jwt;
            _oneTimeTokens = oneTimeTokens;
            _configuration = configuration;
            _filesWrapper = filesWrapper;
            _sendingMessageHandler = sendingMessageHandler;
            _dater = dater;
            _license = license;
            _reCaptchaSettings = reCaptchaSettings.Value;
            _clientFactory = clientFactory;
            _memoryCache = memoryCache;
            _lmsWrapperConnectorService = lmsWrapperConnectorService;
            _certificate = certificate;
            _generalSettings = generalSettings.Value;
        }

        public async Task<string> SignUp(User user, bool sendActivationLink = true)
        {
            if (user == null)
            {
                throw new Exception($"Null input - user is null");
            }
            Configuration configuration = await _configuration.ReadAppConfiguration();
            if (!configuration.EnableFreeTrailUsers)
            {
                throw new InvalidOperationException(ResultCode.ForbiddenToCreateFreeTrailUser.GetNumericString());
            }
            user.CompanyId = Consts.FREE_ACCOUNTS_COMPANY_ID;
            user.Type = UserType.Editor;
            user.CreationTime = _dater.UtcNow();
            bool passwordSent = false;
            if (!string.IsNullOrWhiteSpace(user.Password))
            {
                user.Password = _pkbdf2Handler.Generate(user.Password);
                user.PasswordSetupTime = _dater.UtcNow();
                passwordSent = true;
            }
            if (await _userConnector.Exists(user))
            {
                _logger.Warning($"User {user.Email} try to sign up Again");
                return "";

            }
            try
            {
                await InitProgramUtilization(user);
                await CreateGroup(user);
                await _userConnector.Create(user);
            }
            catch
            {
                await _programUtilizationConnector.Delete(new ProgramUtilization { Id = user.ProgramUtilization?.Id ?? Guid.Empty });
                await _groupConnector.Delete(new Group { Id = user.GroupId });
                throw ;
            }
            _logger.Information("Successfully create user [{UserId}] with email {UserEmail}", user.Id, user.Email);
            string link = "";
            if (passwordSent)
            {
                link = await _email.Activation(user, sendActivationLink);
                if (!(await _configuration.ReadAppConfiguration()).ShouldReturnActivationLinkInAPIResponse)
                {
                    link = "";
                }
            }
            else
            {
                string resetPasswordToken = await _oneTimeTokens.GenerateResetPasswordToken(user);
                link =  await _email.ResetPassword(user, resetPasswordToken);
                if (!(await _configuration.ReadAppConfiguration()).ShouldReturnActivationLinkInAPIResponse)
                {
                    link = "";
                }
            }
            
            return link;
        }


        public async Task<UserTokens> ExternalLogin(string token)
        {
            _license.GetLicenseInformation();

            UserTokens userTokens = new UserTokens()
            {
                RefreshToken = token
            };
            var user = await _oneTimeTokens.CheckRemoteLoginToken(userTokens);
            if (user == null)
            {
                throw new InvalidOperationException(ResultCode.InvalidToken.GetNumericString());
            }
            await _oneTimeTokens.GenerateRefreshToken(user);

            return new UserTokens()
            {
                JwtToken = _jwt.GenerateToken(user),
                RefreshToken = await _oneTimeTokens.GetRefreshToken(user),
                AuthToken = user.UserTokens?.AuthToken ?? ""
            };
        }


        public async Task<(bool, UserTokens)> TryLogin(User user)
        {
            UserTokens userTokens;
            _license.GetLicenseInformation();
            var password = user?.Password;
            user = await _userConnector.ReadWithUserToken(user);
            if (user == null || (user.Type == UserType.SystemAdmin && user.CompanyId == new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1)))
            {
                throw new InvalidOperationException(ResultCode.InvalidCredential.GetNumericString());
            }
            if (user.Status != UserStatus.Activated)
            {
                throw new InvalidOperationException(ResultCode.ActivationRequired.GetNumericString());
            }
            if (!_pkbdf2Handler.Check(user.Password, password))
            {
                throw new InvalidOperationException(ResultCode.InvalidCredential.GetNumericString());
            }

            var company = await _companyConnector.Read(new Company() { Id = user.CompanyId });

            if (company == null)
            {
                throw new InvalidOperationException(ResultCode.CompanyNotExist.GetNumericString());
            }

            if (IsUserPasswordExpired(user, company.CompanyConfiguration.PasswordExpirationInDays))
            {
                userTokens = await ExpiredPasswordFlow(user);
                return (true, userTokens);
            }

            if (!string.IsNullOrWhiteSpace(company.TransactionId))
            {
                await _lmsWrapperConnectorService.CheckUser(new Common.Models.License.LmsUserAction
                {
                    CompanyID = company.Id,
                    UserID = user.Id,
                    TransactionId = company.TransactionId
                }
                    );

            }
            if (user.UserTokens != null)
            {
                user.UserTokens.AuthToken = "";
            }
            if (!company.CompanyConfiguration?.ShouldForceOTPInLogin ?? false)
            {

                userTokens = await LoginFlow(user);
            }
            else
            {
                userTokens = await OtpFlow(user, company);
            }

            return (true, userTokens);
        }

        private bool IsUserPasswordExpired(User user, int passwordExpirationInDays)
        {
            if (passwordExpirationInDays == 0)
                return false;

            return _dater.UtcNow() > user.PasswordSetupTime.AddDays(passwordExpirationInDays);
        }

        public async Task<bool> ChangeExpiredPasswordFlow(string oldPassword, string newPassword, UserTokens userToken)
        {
            UserTokens dbUserTokens =await _userTokenConnector.ReadTokenByRefreshToken(userToken);

            if(dbUserTokens == null || dbUserTokens.AuthToken !=  Consts.EXPIRED_PASSWORD)
            {
                throw new InvalidOperationException(ResultCode.InvalidRefreshToken.GetNumericString());
            }
            if (dbUserTokens.RefreshTokenExpiredTime < _dater.UtcNow())
            {
                throw new InvalidOperationException(ResultCode.PasswordSessionExpired.GetNumericString());
            }


            User user =  await _userConnector.Read(new User { Id = dbUserTokens.UserId });
            if (user == null)
            {
                throw new InvalidOperationException(ResultCode.InvalidRefreshToken.GetNumericString());
            }


            var company = await _companyConnector.Read(new Company { Id = user.CompanyId });
            if (company == null)
            {
                throw new InvalidOperationException(ResultCode.InvalidRefreshToken.GetNumericString());
            }

            if(newPassword.Trim().Length < company.CompanyConfiguration.MinimumPasswordLength)
            {
                throw new InvalidOperationException(ResultCode.PasswordIsTooShortToCompanyPolicy.GetNumericString());
            }
           

            if (!_pkbdf2Handler.Check(user.Password, oldPassword))
            {
                throw new InvalidOperationException(ResultCode.InvalidCredential.GetNumericString());
            }

            await HandleUserPasswordsHistory(user, newPassword);

            user.Password = _pkbdf2Handler.Generate(newPassword);
            user.PasswordSetupTime = _dater.UtcNow();
            user.Status = user.Status == UserStatus.Created ? UserStatus.Activated : user.Status;
            await _userConnector.Update(user, false);

            await _oneTimeTokens.GenerateRefreshToken(user);
            await _userTokenConnector.DeleteResetPasswordToken(userToken);
            return true;
        }

        public async Task<(bool, UserTokens userTokens)> LoginOtpFlow(UserTokens inputUserToken)
        {
            UserTokens userTokens;
            var user = await _oneTimeTokens.CheckRemoteLoginToken(inputUserToken);
            if (user == null)
            {
                throw new InvalidOperationException(ResultCode.OTPTokenWrongOrExpired.GetNumericString());
            }
            if (inputUserToken.AuthToken == _encryptor.Decrypt(user.UserTokens.AuthToken))
            {
                userTokens = await LoginFlow(user);
                return (true, userTokens);
            }
            throw new InvalidOperationException(ResultCode.WrongOTPCode.GetNumericString());
        }

        public async Task<UserTokens> ResendOtp(UserTokens userTokens)
        {
            UserTokens userToken = await _userTokenConnector.ReadTokenByRefreshToken(userTokens, true);

            if (userToken == null)
            {
                throw new InvalidOperationException(ResultCode.OTPTokenWrongOrExpired.GetNumericString());
            }

            User user = await _userConnector.Read(new User { Id = userToken.UserId });
            Company company = await _companyConnector.Read(new Company() { Id = user.CompanyId });
            return await OtpFlow(user, company);

        }

        public async Task Update(User user)
        {
            (var currentUser, var _) = await GetUser();
            currentUser.Name = user.Name;

            if (currentUser.Email != user.Email)
            {
                if (await _userConnector.ExistsByEmail(user))
                {
                    throw new InvalidOperationException(ResultCode.EmailAlreadyExist.GetNumericString());
                }

                currentUser.Email = user.Email;
            }

            if (currentUser.Username != user.Username)
            {
                if (await _userConnector.ExistsByUsername(user))
                {
                    throw new InvalidOperationException(ResultCode.UsernameAlreadyExist.GetNumericString());
                }
                currentUser.Username = user.Username;
            }

            currentUser.Email = user.Email;
            currentUser.Username = user.Username;
            currentUser.UserConfiguration.ShouldSendSignedDocument = user.UserConfiguration.ShouldSendSignedDocument;
            currentUser.UserConfiguration.ShouldNotifyWhileSignerSigned = user.UserConfiguration.ShouldNotifyWhileSignerSigned;
            currentUser.UserConfiguration.ShouldNotifyWhileSignerViewed = user.UserConfiguration.ShouldNotifyWhileSignerViewed;
            currentUser.UserConfiguration.ShouldDisplayNameInSignature = user.UserConfiguration.ShouldDisplayNameInSignature;
            currentUser.UserConfiguration.ShouldNotifySignReminder = user.UserConfiguration.ShouldNotifySignReminder;
            currentUser.UserConfiguration.SignReminderFrequencyInDays = user.UserConfiguration.SignReminderFrequencyInDays;
            currentUser.UserConfiguration.Language = user.UserConfiguration.Language;
            currentUser.UserConfiguration.SignatureColor = user.UserConfiguration.SignatureColor;



            await _userConnector.Update(currentUser, false);
            Company userCompany = await _companyConnector.Read(new Company() { Id = currentUser.CompanyId });
            _certificate.Create(currentUser, userCompany.CompanyConfiguration);
        }

        public async Task<UserTokens> Activation(User user)
        {
            string userEmail = user.Email;
            user = await _userConnector.Read(user);
            if (user == null)
            {
                
                _logger.Error("Activation - User not exist [{UserEmail}]", userEmail);
                return new UserTokens();
            }
            user.Status = UserStatus.Activated;
            if (user.CompanyId != Consts.FREE_ACCOUNTS_COMPANY_ID)
            {
                var comp =await _companyConnector.Read(new Company { Id = user.CompanyId });
                if (!string.IsNullOrWhiteSpace(comp.TransactionId))
                {
                    // case user created from LMS need fo set a new password and the activation
                    await ResetPassword(user);
                }
            }
           await _userConnector.Update(user, false);
            await _oneTimeTokens.GenerateRefreshToken(user);

            return new UserTokens()
            {
                JwtToken = _jwt.GenerateToken(user),
                RefreshToken = await _oneTimeTokens.GetRefreshToken(user),
                AuthToken = user.UserTokens?.AuthToken ?? ""
            };
        }

        public async Task ReSendActivation(User user)
        {
            string userEmail = user.Email;
            user = await _userConnector.Read(user);
            if (user == null)
            {
                
                _logger.Error("ReSendActivation - User not exist [{UserEmail}]", userEmail);
                return;
            }
            await _email.Activation(user);

        }

        public async Task<string> ResetPassword(User user)
        {
            string userEmail = user.Email;
            user = await _userConnector.Read(user);
            if (user == null)
            {
                
                _logger.Error("ResetPassword - User not exist [{UserEmail}]", userEmail);
                return string.Empty;
            }
            string resetPasswordToken = await _oneTimeTokens.GenerateResetPasswordToken(user);
            string link = await _email.ResetPassword(user, resetPasswordToken);

            return link;
        }

        public async Task<UserTokens> UpdatePassword(User user, string token)
        {
            var userTokens = new UserTokens()
            {
                ResetPasswordToken = token
            };
            userTokens =await _userTokenConnector.Read(userTokens);
            user.Id = userTokens != null ? userTokens.UserId : Guid.Empty;
            bool isValidToken = await _oneTimeTokens.CheckPasswordToken(user, userTokens);
            if (!isValidToken)
            {
                throw new InvalidOperationException(ResultCode.InvalidToken.GetNumericString());
            }
            string newPassword = user.Password;
            string userEmail = user.Email;
            user = await _userConnector.Read(user);
            if (user == null)
            {
                _logger.Error("UpdatePassword - User not exist [{UserEmail}]", userEmail);
                throw new InvalidOperationException(ResultCode.InvalidToken.GetNumericString());
            }

            await HandleUserPasswordsHistory(user, newPassword);

            user.Password = _pkbdf2Handler.Generate(newPassword);
            user.PasswordSetupTime = _dater.UtcNow();
            user.Status = user.Status == UserStatus.Created ? UserStatus.Activated : user.Status;
            await _userConnector.Update(user, false);

            await _oneTimeTokens.GenerateRefreshToken(user);
            await _userTokenConnector.DeleteResetPasswordToken(userTokens);

            return new UserTokens()
            {
                JwtToken = _jwt.GenerateToken(user),
                RefreshToken = await _oneTimeTokens.GetRefreshToken(user),
                AuthToken = user.UserTokens?.AuthToken ?? ""
            };
        }

        public async Task<UserTokens> UpdatePasswordByDevAdminUser(User user)
        {
            var dbuser = await _userConnector.Read(user);
            if (dbuser == null)
            {
                _logger.Error("UpdatePassword - User not exist [{UserId} {UserEmail}]", user.Id, user.Email);
                throw new InvalidOperationException(ResultCode.InvalidToken.GetNumericString());
            }

            await HandleUserPasswordsHistory(dbuser, user.Password);

            dbuser.Password = _pkbdf2Handler.Generate(user.Password);
            dbuser.PasswordSetupTime = _dater.UtcNow();
            dbuser.Status = dbuser.Status == UserStatus.Created ? dbuser.Status = UserStatus.Activated : dbuser.Status;
           await _userConnector.Update(dbuser, false);

            await _oneTimeTokens.GenerateRefreshToken(dbuser);
           await _userTokenConnector.DeleteResetPasswordToken(new UserTokens { UserId = dbuser.Id });

            return new UserTokens()
            {
                JwtToken = _jwt.GenerateToken(dbuser),
                RefreshToken = await _oneTimeTokens.GetRefreshToken(dbuser),
                AuthToken = dbuser.UserTokens?.AuthToken ?? ""
            };
        }



        public  Task<(User, CompanySigner1Details)> GetUser()
        {            
            var userId = _userClaims?.Claims?.FirstOrDefault(x => x.Type == ClaimTypes.Sid)?.Value;
            if (userId == null)
            {
                throw new InvalidOperationException(ResultCode.InvalidToken.GetNumericString());
            }
            var groupId = _userClaims?.Claims?.FirstOrDefault(x => x.Type == ClaimTypes.PrimaryGroupSid)?.Value;
            if (groupId== Guid.Empty.ToString() || groupId == null)
            {
                throw new InvalidOperationException(ResultCode.InvalidToken.GetNumericString());
            }



            return GetUserInfo(userId, groupId);
           
        }

        private async Task<(User, CompanySigner1Details)> GetUserInfo(string userId, string groupId)
        {
            CompanySigner1Details companySigner1Details = null;
            User user = _memoryCache.Get<User>($"{userId}_{groupId}" );

            if (user == null)
            {
                user =await  _userConnector.Read(new User() { Id = new Guid(userId) });
                if (user == null)
                {
                    throw new InvalidOperationException(ResultCode.InvalidToken.GetNumericString());
                }
                
                if(user.GroupId != new Guid(groupId) && !user.AdditionalGroupsMapper.Exists(x => x.GroupId == new Guid(groupId)) )
                {

                    throw new InvalidOperationException(ResultCode.UserNotInTokenGroup.GetNumericString());
                }
               await GetProfileProgram(user);
                GetCompanyLogo(user);              
                
                Configuration appConfiguration = await _configuration.ReadAppConfiguration();
                Company company = await _companyConnector.Read(new Company() { Id = user.CompanyId });

                await GetSmsGloballySendSupport(user, appConfiguration, company);
                companySigner1Details = GetCompanySigner1Details(user, company);
                user.GroupId = new Guid(groupId);
                _memoryCache.Set<User>($"{userId}_{groupId}", user, TimeSpan.FromMinutes(3));
            }
            else
            {
                Company company = await _companyConnector.Read(new Company() { Id = user.CompanyId });
                companySigner1Details = GetCompanySigner1Details( user, company);
            }

            return (user, companySigner1Details);
        }
        public async Task<UserTokens> SwitchGroup(Guid groupId)
        {
            (var user, var _) = await GetUser();
            List<Group> userGroups = await GetUserGroups();
            Guid prevGroup = user.GroupId;
            if (user.GroupId == groupId)
            {
                throw new InvalidOperationException(ResultCode.UserAlreadyConnectedToTheSelectedGroup.GetNumericString());
            }
            if(!userGroups.Exists(x => x.Id == groupId))
            {
                throw new InvalidOperationException(ResultCode.InvalidGroupId.GetNumericString());
            }
            user.GroupId = groupId;

            _memoryCache.Remove($"{user.Id}_{prevGroup}");
            return await LoginFlow(user);

        }
        public async Task<List<Group>> GetUserGroups()
        {           
            (User user, var _) = await GetUser();
            List<Group> userGroups = _memoryCache.Get<List<Group>>($"{user.Id}_All_Groups");
            if (userGroups == null)
            {
                
                userGroups = await _groupConnector.ReadAllUserGroups(user);
                _memoryCache.Set($"{user.Id}_All_Groups", userGroups, TimeSpan.FromMinutes(2));
            }
            return userGroups;

        }

        public async Task<(ExtendedUserInfo, CompanySigner1Details)> GetExtendedUserInfo( )
        {
            (User user, CompanySigner1Details companySigner1Details) = await GetUser();
            
            ExtendedUserInfo result = new ExtendedUserInfo(user);
            var appConfiguration = await _configuration.ReadAppConfiguration();
            Company company =await _companyConnector.Read(new Company() { Id = result.CompanyId });
            Program program =await _programConnector.Read(new Program
            {
                Id = company.ProgramId
            });
            Group group = await _groupConnector.Read(new Group { Id = result.GroupId });
            result.CompanyName = company.Name;
            result.GroupName = group.Name;
            result.DefaultSigningType = company.CompanyConfiguration?.DefaultSigningType ?? Common.Enums.PDF.SignatureFieldType.Graphic;
            if ((program != null) && ((!program.ServerSignature && result.DefaultSigningType == Common.Enums.PDF.SignatureFieldType.Server) ||
                (!program.SmartCard && result.DefaultSigningType == Common.Enums.PDF.SignatureFieldType.SmartCard)))
            {
                result.DefaultSigningType = Common.Enums.PDF.SignatureFieldType.Graphic;
            }

            result.TransactionId = company.TransactionId;
            result.EnableVisualIdentityFlow = appConfiguration.EnableVisualIdentityFlow && (company.CompanyConfiguration?.EnableVisualIdentityFlow ?? false);
            result.ShouldSendWithOTPByDefault = company.CompanyConfiguration?.ShouldSendWithOTPByDefault ?? false;
            result.EnableDisplaySignerNameInSignature = company.CompanyConfiguration?.EnableDisplaySignerNameInSignature ?? true;
            result.EnableSignReminderSettings = (company.CompanyConfiguration?.CanUserControlReminderSettings ?? false) && (company.CompanyConfiguration?.ShouldEnableSignReminders ?? false);
            result.EnableMeaningOfSignature = (company.CompanyConfiguration?.ShouldEnableMeaningOfSignatureOption ?? false);
            result.ShouldSignEidasSignatureFlow = _generalSettings.ComsignIDPActive;
            result.EnableVideoConferenceFlow = company.CompanyConfiguration?.ShouldEnableVideoConference ?? false; 
            result.EnableTabletsSupport = company.CompanyConfiguration?.EnableTabletsSupport ?? false && appConfiguration.EnableTabletsSupport;
            //result.ShouldEnableGovernmentSignatureFormat = company.CompanyConfiguration?.ShouldEnableGovernmentSignatureFormat ?? false;

            return (result, companySigner1Details);
        }

        public async  Task<string> Logout()
        {
            (var dbUser, var _) = await GetUser();
            if (dbUser == null)
            {
                throw new InvalidOperationException(ResultCode.ActivationRequired.GetNumericString());
            }

            await _oneTimeTokens.ClearTokens(dbUser);

            return _generalSettings.LogoutRoute;
        }
        #region Async

        public Task<User> GetUserAsync()
        {
            var user = new User() { Id = new Guid(_userClaims?.Claims?.FirstOrDefault(x => x.Type == ClaimTypes.Sid)?.Value) };
            return _userConnector.ReadAsync(user);
        }

        public async Task<bool> TryChangePasswordAsync(User user, string newPassword)
        {
           
            (var dbUser, var _) = await GetUser();

            if (dbUser == null) // TODO - move two following check to seperate function
            {
                _logger.Error("TryChangePasswordAsync - User not exist [{UserEmail}]", user.Email);
                throw new InvalidOperationException(ResultCode.ActivationRequired.GetNumericString());
            }

            if (dbUser.Status != UserStatus.Activated)
            {
                throw new InvalidOperationException(ResultCode.ActivationRequired.GetNumericString());
            }

            if (!_pkbdf2Handler.Check(dbUser.Password, user.Password))
            {
                throw new InvalidOperationException(ResultCode.InvalidCredential.GetNumericString());
            }

            await HandleUserPasswordsHistory(dbUser, newPassword);

            dbUser.Password = _pkbdf2Handler.Generate(newPassword);
            dbUser.PasswordSetupTime = _dater.UtcNow();
            await _userConnector.UpdateAsync(dbUser);
            return true;
        }

        private async Task HandleUserPasswordsHistory(User user, string newPassword)
        {
            Company userCompany = new Company() { Id = user.CompanyId };
            Company dbCompany =await _companyConnector.Read(userCompany);

            if (dbCompany == null)
            {
                throw new InvalidOperationException(ResultCode.CompanyNotExist.GetNumericString());
            }

            if (dbCompany.CompanyConfiguration.RecentPasswordsAmount == 0)
            {
                await _userPasswordHistoryConnector.DeleteAllByUserId(user.Id);
                return;
            }

            IEnumerable<UserPasswordHistory> dbUserPasswordHistories = _userPasswordHistoryConnector.ReadAllByUserId(user.Id);
            var numberOfPasswordsToDelete = dbUserPasswordHistories.Count() - dbCompany.CompanyConfiguration.RecentPasswordsAmount;

            if (numberOfPasswordsToDelete > 0)
            {
                await _userPasswordHistoryConnector.DeleteOldestPasswordsByUserId(user.Id, numberOfPasswordsToDelete);
            }

            if (!IsPasswordNew(user, newPassword))
            {
                throw new InvalidOperationException(ResultCode.PasswordAlreadyUsed.GetNumericString());
            }

            if (numberOfPasswordsToDelete >= 0)
            {
                await _userPasswordHistoryConnector.DeleteOldestPasswordsByUserId(user.Id, 1);
            }
            if (!string.IsNullOrEmpty(user.Password))
            {
                CreateUserPasswordHistory(user);
            }
        }

        private bool IsPasswordNew(User user, string password)
        {
            IEnumerable<UserPasswordHistory> dbUserPasswordHistories = _userPasswordHistoryConnector.ReadAllByUserId(user.Id);
            foreach (var item in dbUserPasswordHistories)
            {
                if (_pkbdf2Handler.Check(item.Password, password))
                {
                    return false;
                }
            }
            return true;
        }

        private void CreateUserPasswordHistory(User user)
        {
            var userPasswordHistory = new UserPasswordHistory()
            {
                UserId = user.Id,
                Password = user.Password,
                CreationTime = _dater.UtcNow()
            };
            _userPasswordHistoryConnector.Create(userPasswordHistory);
        }

        public async Task StartUpdatePhoneProcess(UserOtpDetails userOtpDetails)
        {
            (User dbUser, _) = await GetUser();
            if(string.IsNullOrWhiteSpace(userOtpDetails.AdditionalInfo) || !ContactsExtenstions.IsValidPhone(userOtpDetails.AdditionalInfo))
            {
                throw new InvalidOperationException(ResultCode.InvalidPhone.GetNumericString());
            }

            var company = await _companyConnector.Read(new Company() { Id = dbUser.CompanyId });

            if (company == null)
            {
                throw new InvalidOperationException(ResultCode.CompanyNotExist.GetNumericString());
            }

            if(dbUser.Phone == userOtpDetails.AdditionalInfo)
            {
                throw new InvalidOperationException(ResultCode.SamePhoneExistAtTheSystem.GetNumericString());
            }

            Random generator = new Random(GenerateRandomSeed());
            string code = generator.Next(0, 999999).ToString("D6");
            userOtpDetails.Code = _encryptor.Encrypt( code);
            userOtpDetails.ExpirationTime = _dater.UtcNow().AddMinutes(5);
            userOtpDetails.UserId = dbUser.Id;
            userOtpDetails.OtpMode = UserOtpMode.PhoneUpdate;
            userOtpDetails.AdditionalInfo = userOtpDetails.AdditionalInfo;
            await _userConnector.SetUserOtpDitails(dbUser, userOtpDetails);
            dbUser.Phone = userOtpDetails.AdditionalInfo;
            await SendOTPForUser(dbUser, company, code, Common.Enums.Documents.SendingMethod.SMS);
        }
        public async Task ValidateUpdatePhoneInfo(UserOtpDetails userOtpDetails)
        {
            (User dbUser, _) = await GetUser();
            
            if (string.IsNullOrWhiteSpace(userOtpDetails.Code) && System.Text.RegularExpressions.Regex.IsMatch(userOtpDetails.Code, @"^\d{6}$"))
            {
                throw new InvalidOperationException(ResultCode.WrongOTPCode.GetNumericString());
            }
            var dbuserOtpDetails =  _userConnector.ReadOtpDitails(dbUser);
            if(dbuserOtpDetails == null || dbuserOtpDetails.OtpMode != UserOtpMode.PhoneUpdate || dbuserOtpDetails.ExpirationTime < _dater.UtcNow())
            {
                throw new InvalidOperationException(ResultCode.OTPTokenWrongOrExpired.GetNumericString());
            }
            if( _encryptor.Decrypt( dbuserOtpDetails.Code) != userOtpDetails.Code)
            {
                throw new InvalidOperationException(ResultCode.WrongOTPCode.GetNumericString());
            }

            dbUser.Phone = dbuserOtpDetails.AdditionalInfo;
            await _userConnector.UpdateUserPhone(dbUser);
            dbuserOtpDetails.OtpMode = UserOtpMode.None;
            dbuserOtpDetails.Code = "";
            dbuserOtpDetails.ExpirationTime = _dater.UtcNow();
            dbuserOtpDetails.AdditionalInfo = "";
            await _userConnector.SetUserOtpDitails(dbUser, dbuserOtpDetails);
           
        }

        public async Task<string> GetGetChangePaymentRuleURL()
        {
            (User dbUser, _) = await GetUser();
            if (dbUser == null)
            {
                throw new InvalidOperationException(ResultCode.ActivationRequired.GetNumericString());
            }
            if (dbUser.Type != UserType.SystemAdmin && dbUser.Type != UserType.CompanyAdmin)
            {
                throw new InvalidOperationException(ResultCode.ActivationRequired.GetNumericString());
            }

            Company company =await _companyConnector.Read(new Company { Id = dbUser.CompanyId });
            if (string.IsNullOrWhiteSpace(company.TransactionId))
            {
                throw new InvalidOperationException(ResultCode.ActivationRequired.GetNumericString());
            }

            string url = await _lmsWrapperConnectorService.GetURLForChangePaymentRule(new Common.Models.License.LmsUserAction
            {
                CompanyID = company.Id,
                TransactionId = company.TransactionId,
                UserID = dbUser.Id
            });
            return url;
        }

        public async Task UnSubscribeUser()
        {
            (User dbUser,  _) = await GetUser();
            if (dbUser == null)
            {
                throw new InvalidOperationException(ResultCode.ActivationRequired.GetNumericString());
            }
            if (dbUser.Type != UserType.SystemAdmin && dbUser.Type != UserType.CompanyAdmin)
            {
                throw new InvalidOperationException(ResultCode.ActivationRequired.GetNumericString());
            }

            Company company = await _companyConnector.Read(new Company { Id = dbUser.CompanyId });
            if (string.IsNullOrWhiteSpace(company.TransactionId))
            {
                throw new InvalidOperationException(ResultCode.ActivationRequired.GetNumericString());
            }

            if (!await _lmsWrapperConnectorService.UnsubscribeUser(new Common.Models.License.LmsUserAction
            {
                CompanyID = company.Id,
                TransactionId = company.TransactionId,
                UserID = dbUser.Id
            }))
            {
                throw new InvalidOperationException(ResultCode.ActivationRequired.GetNumericString());
            }

            _memoryCache.Remove(company.Id);
            company = await _companyConnector.Read(new Company { Id = dbUser.CompanyId });
            //TODO: need to update TransactionId only and not all model
            company.TransactionId = "";
            await _companyConnector.Update(company);

        }
        public async Task ValidateReCAPCHAAsync(string reCAPCHA)
        {
            Configuration configuration = await _configuration.ReadAppConfiguration();
            if (configuration.ShouldUseReCaptchaInRegistration)
            {
                HttpClient httpClient = _clientFactory.CreateClient();
                var response = await httpClient.PostAsync($"{_reCaptchaSettings.GoogleVerificationUrl}?secret={_reCaptchaSettings.ServerSecretKey}&response={reCAPCHA}", null);
                string jsonString = await response.Content.ReadAsStringAsync();
                CaptchaVerificationResponse captchaVerfication = JsonConvert.DeserializeObject<CaptchaVerificationResponse>(jsonString);

                if (!captchaVerfication.Success)
                {
                    throw new InvalidOperationException(ResultCode.InvalidCaptchaToken.GetNumericString());
                }
            }
        }

        #endregion

        #region Private

        private int GenerateRandomSeed()
        {
            Guid guid = Guid.NewGuid();
            long timestamp = _dater.UtcNow().Ticks;

            // Mix the bytes of the Guid and timestamp
            byte[] guidBytes = guid.ToByteArray();
            byte[] timestampBytes = BitConverter.GetBytes(timestamp);

            // XOR the two arrays to create a seed
            int seed = 0;
            for (int i = 0; i < guidBytes.Length && i < timestampBytes.Length; i++)
            {
                seed ^= guidBytes[i] ^ timestampBytes[i];
            }

            // Ensure the seed is positive
            return Math.Abs(seed);
        }
        private CompanySigner1Details GetCompanySigner1Details( User user, Company company)
        {
            CompanySigner1Details companySigner1Details;
            var cacheCompanySigner1Details = _memoryCache.Get<CompanySigner1Details>(user.CompanyId + "_Signer1Details");
            if (cacheCompanySigner1Details == null)
            {
                companySigner1Details = company.CompanySigner1Details;
                _memoryCache.Set<CompanySigner1Details>(user.CompanyId + "_Signer1Details", companySigner1Details, TimeSpan.FromMinutes(3));
            }
            else
            {
                companySigner1Details = cacheCompanySigner1Details;
            }

            return companySigner1Details;
        }


        private async Task<UserTokens> OtpFlow(User user, Company company)
        {
            Random generator = new Random(GenerateRandomSeed());
            string otpCode = generator.Next(0, 999999).ToString("D6");
            Guid guid = Guid.NewGuid();
            var userTokens = new UserTokens()
            {
                JwtToken = "",
                RefreshToken = guid.ToString(),
                RefreshTokenExpiredTime = _dater.UtcNow().AddMinutes(5),
                AuthToken = _encryptor.Encrypt(otpCode),
                UserId = user.Id,

            };

            await _userTokenConnector.UpdateRefreshToken(userTokens);
            userTokens.AuthToken = "OTP";
            await SendOTPForUser(user, company, otpCode, Common.Enums.Documents.SendingMethod.Email);

            return userTokens;
        }

        private async Task<UserTokens> ExpiredPasswordFlow(User user)
        {
            Guid guid = Guid.NewGuid();
            var userTokens = new UserTokens()
            {
                JwtToken = "",
                RefreshToken = guid.ToString(),
                RefreshTokenExpiredTime = _dater.UtcNow().AddMinutes(5),
                AuthToken = Consts.EXPIRED_PASSWORD,
                UserId = user.Id,
            };

           await _userTokenConnector.UpdateRefreshToken(userTokens);
            return userTokens;
        }

        private async Task SendOTPForUser(User user, Company company, string otpCode, Common.Enums.Documents.SendingMethod sendingMethod)
        {
            IMessageSender messageSender = _sendingMessageHandler.ExecuteCreation(sendingMethod);
            Configuration appConfiguration = await _configuration.ReadAppConfiguration();
            string otpMessage = _configuration.GetOtpMessgae(user, appConfiguration, null);
            otpMessage = otpMessage.Replace(OTP_CODE_PLACEHOLDER, otpCode);
            MessageInfo info = new MessageInfo()
            {
                MessageType = MessageType.OtpCode,
                MessageContent = otpMessage,
                User = user,

            };
            if(sendingMethod == Common.Enums.Documents.SendingMethod.SMS)
            {
                info.Contact = new Contact() { Phone = user.Phone, PhoneExtension = "+972" };
            }

            await messageSender.Send(appConfiguration, company.CompanyConfiguration, info);
        }

        private async Task<UserTokens> LoginFlow(User user)
        {
            UserTokens userTokens;
            await _oneTimeTokens.GenerateRefreshToken(user);

            userTokens = new UserTokens()
            {
                JwtToken = _jwt.GenerateToken(user),
                RefreshToken =await _oneTimeTokens.GetRefreshToken(user),
                AuthToken = user.UserTokens?.AuthToken ?? ""
            };

            user.LastSeen = _dater.UtcNow();
            await _userConnector.UpdateLastSeen(user);
            return userTokens;
        }


        private async Task InitProgramUtilization(User user)
        {
            Program trialProgram = new Program()
            {
                Id = new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1),
                Name = "Trial",
                Templates = 2,
                DocumentsPerMonth = 5
            };
            if (!await _programConnector.Exists(trialProgram))
            {
                await _programConnector.Create(trialProgram);
            }
            ProgramUtilization programUtilization = new ProgramUtilization() { Expired = _dater.UtcNow().AddDays(14) };
            await _programUtilizationConnector.Create(programUtilization);
            user.ProgramUtilizationId = programUtilization.Id;
          
        }

        private async Task CreateGroup(User user)
        {
            var group = new Group()
            {
                CompanyId = user.CompanyId,
                Name = $"FreeTrial-{user.Email}"
            };
            await _groupConnector.Create(group);
            user.GroupId = group.Id;
        }

        private void GetCompanyLogo(User user)
        {
            _filesWrapper.Users.SetCompanyLogo(user);

        }
        private async Task GetSmsGloballySendSupport(User user, Configuration appConfiguration, Company company)
        {

            var companyConfiguration = company?.CompanyConfiguration;
            var smsConfiguration =await _configuration.GetSmsConfiguration(user, appConfiguration, companyConfiguration);
            user.ProfileProgram.IsSmsProviderSupportGloballySend = smsConfiguration.IsProviderSupportGloballySend;
        }

        private async Task GetProfileProgram(User user)
        {
            Company freeAccountCompany =await _companyConnector.Read(
                            new Company()
                            {
                                Id = Consts.FREE_ACCOUNTS_COMPANY_ID,
                                Name = FREE_ACCOUNTS_COMPANY_NAME
                            });

            if (freeAccountCompany != null && user.CompanyId == freeAccountCompany.Id)
            {
                await GetFreeTailUserProfileProgram(user, freeAccountCompany);
            }
            else
            {
                await GetRegularCompanyUserProfileProgram(user);
            }
        }


        private async Task GetFreeTailUserProfileProgram(User user, Company freeAccountCompany)
        {
            ProgramUtilization programUtilization = new ProgramUtilization() { Id = (Guid)user.ProgramUtilization.Id };
            Program program = await _programConnector.Read(new Program() { Id = freeAccountCompany.ProgramId });
            programUtilization = await _programUtilizationConnector.Read(programUtilization);

            user.ProfileProgram.Expired = programUtilization?.Expired ?? DateTime.MinValue;
            user.ProfileProgram.Name = program.Name;
            user.ProfileProgram.Note = program.Note;
            user.ProfileProgram.RemainingDocuments = GetRemainingDocumentsForMonth(program, programUtilization);
            user.ProfileProgram.DocumentsLimits = GetDocumentLimits(program, programUtilization);
            user.ProfileProgram.ServerSignature = program.ServerSignature;
            user.ProfileProgram.SmartCard = program.SmartCard;
            user.ProfileProgram.ViewLicense = program.UIViewLicense;

            user.ProfileProgram.SMSLimit = program.SmsPerMonth;
            user.ProfileProgram.RemainingTemplates = program.Templates == Consts.UNLIMITED ? Consts.UNLIMITED : program.Templates - programUtilization.Templates;
            user.ProfileProgram.TemplatesLimit = program.Templates;
            user.ProfileProgram.RemainingUsers = program.Users == Consts.UNLIMITED ? Consts.UNLIMITED : program.Users - programUtilization.Users;
            user.ProfileProgram.UsersLimit = program.Users;
            user.ProfileProgram.RemainingSMS = program.SmsPerMonth == Consts.UNLIMITED ? Consts.UNLIMITED : program.SmsPerMonth - programUtilization.SMS;

            //            GetUIViewLicense(user, program);
        }

        private async Task GetRegularCompanyUserProfileProgram(User user)
        {
            Company company = await _companyConnector.Read(new Company() { Id = user.CompanyId });
            Program program = await _programConnector.Read(new Program() { Id = company.ProgramId });
            user.ProfileProgram.LastResetDate = company.ProgramUtilization?.LastResetDate ?? DateTime.MinValue;
            user.ProfileProgram.Expired = company.ProgramUtilization?.Expired ?? DateTime.MinValue;
            user.ProfileProgram.ProgramResetType = company.ProgramUtilization?.ProgramResetType ?? ProgramResetType.Monthly;
            user.ProfileProgram.Name = program.Name;
            user.ProfileProgram.Note = program.Note;
            user.ProfileProgram.RemainingDocuments = GetRemainingDocumentsForMonth(program, company.ProgramUtilization);
            user.ProfileProgram.DocumentsLimits = GetDocumentLimits(program, company.ProgramUtilization);
            user.ProfileProgram.RemainingSMS = program.SmsPerMonth == Consts.UNLIMITED ? Consts.UNLIMITED : program.SmsPerMonth - company.ProgramUtilization.SMS;
            user.ProfileProgram.SMSLimit = program.SmsPerMonth;
            user.ProfileProgram.RemainingVisualIdentifications = program.VisualIdentificationsPerMonth == Consts.UNLIMITED ? Consts.UNLIMITED : program.VisualIdentificationsPerMonth - company.ProgramUtilization.VisualIdentifications;
            user.ProfileProgram.RemainingVideoConference = program.VideoConferencePerMonth == Consts.UNLIMITED ? Consts.UNLIMITED : program.VideoConferencePerMonth - company.ProgramUtilization.VideoConference;
            user.ProfileProgram.VisualIdentificationsLimit = program.VisualIdentificationsPerMonth;
            user.ProfileProgram.VideoConferenceLimit = program.VideoConferencePerMonth;
            user.ProfileProgram.RemainingTemplates = program.Templates == Consts.UNLIMITED ? Consts.UNLIMITED : program.Templates - company.ProgramUtilization.Templates;
            user.ProfileProgram.TemplatesLimit = program.Templates;
            user.ProfileProgram.RemainingUsers = program.Users == Consts.UNLIMITED ? Consts.UNLIMITED : program.Users - company.ProgramUtilization.Users - 1;
            user.ProfileProgram.UsersLimit = program.Users;
            user.ProfileProgram.ServerSignature = program.ServerSignature;
            user.ProfileProgram.SmartCard = program.SmartCard;
            user.ProfileProgram.ViewLicense = program.UIViewLicense;
            //GetUIViewLicense(user, program);
        }

       




        private long GetDocumentLimits(Program program, ProgramUtilization programUtilization)
        {
            if (programUtilization.ProgramResetType == ProgramResetType.DocumentsLimitOnly || programUtilization.ProgramResetType == ProgramResetType.TimeAndDocumentsLimit)
            {
                return programUtilization.DocumentsLimit;
            }
            return program.DocumentsPerMonth;

        }

        private long GetRemainingDocumentsForMonth(Program program, ProgramUtilization programUtilization)
        {
            long Remaining = 0;
            if ((programUtilization?.Expired ?? DateTime.MinValue) >= _dater.UtcNow())
            {

                if (programUtilization.ProgramResetType == ProgramResetType.DocumentsLimitOnly || programUtilization.ProgramResetType == ProgramResetType.TimeAndDocumentsLimit)
                {
                    Remaining = programUtilization.DocumentsLimit - programUtilization.DocumentsUsage;
                }
                else
                {

                    Remaining = program.DocumentsPerMonth == Consts.UNLIMITED ? Consts.UNLIMITED : program.DocumentsPerMonth - programUtilization.DocumentsUsage;
                }
            }
            return Remaining;
        }

        #endregion
    }
}

