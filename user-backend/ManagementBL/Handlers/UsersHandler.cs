using Common.Consts;
using Common.Enums.Results;
using Common.Enums.Users;
using Common.Extensions;
using Common.Interfaces;
using Common.Interfaces.DB;
using Common.Interfaces.Emails;
using Common.Interfaces.Files;
using Common.Interfaces.PDF;
using Common.Models;
using Common.Models.ManagementApp;
using Common.Models.Programs;
using Common.Models.Users;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ManagementBL.Handlers
{
    public class UsersHandler : Common.Interfaces.ManagementApp.IUsers
    {
        private readonly ClaimsPrincipal _userClaims;
        
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IOneTimeTokens _oneTimeTokens;
        private readonly IJWT _jwt;
        private readonly IPBKDF2 _pkbdf2Handler;
        private readonly ILogger _logger;
        private readonly IFilesWrapper _filesWrapper;
        private readonly IDater _dater;
        private readonly IEmail _email;
        private readonly IDataUriScheme _dataUriScheme;
        private readonly ITemplatePdf _templateHandler;

        public UsersHandler(IServiceScopeFactory scopeFactory,
            IOneTimeTokens oneTimeTokens, IJWT jwt, 
            IPBKDF2 pkbdf2Handler, ClaimsPrincipal user, ILogger logger,
             IEmail email, IDater dater, IDataUriScheme dataUriScheme, 
             ITemplatePdf templateHandler,IFilesWrapper filesWrapper )
        {
            _scopeFactory = scopeFactory;
            _oneTimeTokens = oneTimeTokens;
            _jwt = jwt;
            _pkbdf2Handler = pkbdf2Handler;
            _userClaims = user;
            _logger = logger;
            _filesWrapper = filesWrapper;
            _dater = dater;
            _email = email;
            
            _dataUriScheme = dataUriScheme;
            _templateHandler = templateHandler;
            
        }

        public async Task<(bool, UserTokens userTokens)> TryLogin(User user)
        {
            using var scope = _scopeFactory.CreateScope();
            IUserConnector userConnector = scope.ServiceProvider.GetService<IUserConnector>();
            UserTokens userTokens;
            var password = user?.Password;
            user =await userConnector.Read(user);
            if (user == null || (user.Type != UserType.SystemAdmin && user.Type != UserType.Ghost && user.Type != UserType.PaymentAdmin && user.Type != UserType.Dev))
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
            await _oneTimeTokens.GenerateRefreshToken(user);

            userTokens = new UserTokens()
            {
                JwtToken = _jwt.GenerateToken(user),
                RefreshToken =await _oneTimeTokens.GetRefreshToken(user)
            };

            return (true, userTokens);
        }

        public async Task Delete(User user)
        {
            using var scope = _scopeFactory.CreateScope();
            IUserConnector userConnector = scope.ServiceProvider.GetService<IUserConnector>();
          
            var systemUser = await GetCurrentUser();
            if (!await userConnector.Exists(user))
            {
                throw new InvalidOperationException(ResultCode.UserNotExist.GetNumericString());
            }
            var userToDelete = await userConnector.Read(user);
            user = userToDelete;

            // check if the user is the last user in the company

            if (!userConnector.GetAllUsersInCompany(new Company { Id = userToDelete.CompanyId }).Any(x => x.Id != userToDelete.Id))
            {
                throw new InvalidOperationException(ResultCode.CantDeleteTheLastUserFromCompany.GetNumericString());
            }

            IProgramConnector programConnector = scope.ServiceProvider.GetService<IProgramConnector>();
            IProgramUtilizationConnector programUtilizationConnector = scope.ServiceProvider.GetService<IProgramUtilizationConnector>();
            Guid programUtilizationId = userToDelete?.ProgramUtilizationId?? Guid.Empty ;
              userToDelete.ProgramUtilizationId = null;
           

            await  userConnector.Delete(userToDelete);

            if (programConnector.IsFreeTrialUser(user))
            {
                    IGroupConnector groupConnector = scope.ServiceProvider.GetService<IGroupConnector>();
                await programUtilizationConnector.Delete(new ProgramUtilization { Id = programUtilizationId  });
                await groupConnector.Delete(new Group() { Id = user.GroupId, CompanyId = user.CompanyId });

            }
            else
            {
                await programUtilizationConnector.UpdateUsersAmount(userToDelete, Common.Enums.CalcOperation.Substruct);
            }
            _logger.Information("Successfully delete user : Name [{UserName}], Id [{UserId}] ---------- by SystemAdmin User [{SystemUserEmail}]", user.Name, user.Id, systemUser.Email);
        }

        public  Task Refresh(UserTokens tokens)
        {
            return _oneTimeTokens.Refresh(tokens);
        }
        public async Task<(IEnumerable<UserDetails>, int totalCount)> ReadAllUsersInCompany(Company company)
        {
            int totalCount;
            using var scope = _scopeFactory.CreateScope();
            IUserConnector userConnector = scope.ServiceProvider.GetService<IUserConnector>();

            var users = userConnector.Read("", 0, -1, null, out totalCount, company.Id);
            List<UserDetails> result = await ConvertToUserDetails(users);

            return (result, totalCount);
        }
        public async Task<(IEnumerable<UserDetails>, int totalCount)> Read(string key, int offset, int limit, UserStatus? status)
        {
            int totalCount;
            using var scope = _scopeFactory.CreateScope();
            IUserConnector userConnector = scope.ServiceProvider.GetService<IUserConnector>();
            var users = userConnector.Read(key, offset, limit, status, out totalCount);
            List<UserDetails> result = await ConvertToUserDetails(users);

            return (result, totalCount);
        }

        private async Task<List<UserDetails>> ConvertToUserDetails(IEnumerable<User> users)
        {
            var result = new List<UserDetails>();

            using var scope = _scopeFactory.CreateScope();
            ICompanyConnector  companyConnector = scope.ServiceProvider.GetService<ICompanyConnector>();
            IProgramConnector programConnector = scope.ServiceProvider.GetService<IProgramConnector>();
            IGroupConnector  groupConnector = scope.ServiceProvider.GetService<IGroupConnector>();  

            foreach (var user in users ?? Enumerable.Empty<User>())
            {
                var company = await companyConnector.Read(new Company { Id = user.CompanyId });
                if (company != null)
                {
                    string companyName = company?.Name;
                    string programName = (await programConnector.Read(new Program { Id = company.ProgramId }))?.Name;
                    string groupName = (await groupConnector.Read(user))?.Name;
                    var userDetails = new UserDetails
                    {
                        Id = user.Id,
                        Name = user.Name,
                        Email = user.Email,
                        Username = user.Username,
                        CreationTime = user.CreationTime,
                        Language = user.UserConfiguration?.Language ?? Language.en,
                        Type = user.Type,
                        CompanyName = companyName,
                        ProgramName = programName,
                        GroupName = groupName,
                        UserStatus = user.Status,
                        LastSeen = user.LastSeen,
                    };
                    result.Add(userDetails);
                }
            }
            
            
            return result;
        }

        public async Task<Dictionary<Guid, string>> ReadTemplates(User user)
        {
            using var scope = _scopeFactory.CreateScope();
            ITemplateConnector templateConnector= scope.ServiceProvider.GetService<ITemplateConnector>();
            IUserConnector userConnector = scope.ServiceProvider.GetService<IUserConnector>();

            if (user == null)
            {
                throw new Exception("Null input - user is null");
            }
            var dbUser = await userConnector.Read(user);
            var templates = templateConnector.Read(dbUser, "", "", "", 0, -1, true, true, out int totalCount);

            return templates.ToDictionary(x => x.Id, x => x.Name);
        }

        public async Task CreateHtmlTemplate(User user, Template template, string htmlFile, string jsFile)
        {
            using var scope = _scopeFactory.CreateScope();
            ITemplateConnector templateConnector = scope.ServiceProvider.GetService<ITemplateConnector>();
            IUserConnector userConnector = scope.ServiceProvider.GetService<IUserConnector>();

            var dbUser = await userConnector.Read(user);
            var dbTemplate = await templateConnector.Read(template);
            if (dbTemplate == null || (dbTemplate.UserId != dbUser.Id && dbTemplate.GroupId != dbUser.GroupId))
            {
                throw new InvalidOperationException(ResultCode.InvalidTemplateId.GetNumericString());
            }
            byte[] htmlBytes = ValidatePdfFieldsExistsInHtmlTemplate(htmlFile, dbTemplate);

            byte[] jsBytes = _dataUriScheme.GetBytes(jsFile);
            _filesWrapper.Documents.SaveTemplateCustomHtml(template, htmlBytes, jsBytes);
            
        }

        private byte[] ValidatePdfFieldsExistsInHtmlTemplate(string htmlFile, Template dbTemplate)
        {
            _templateHandler.Load(dbTemplate.Id);
            var allfields = _templateHandler.GetAllFields();
            var pdfFieldsNames = new List<string>();
            pdfFieldsNames.AddRange(allfields.TextFields.Select(x => x.Name));
            pdfFieldsNames.AddRange(allfields.SignatureFields.Select(x => x.Name));
            pdfFieldsNames.AddRange(allfields.ChoiceFields.Select(x => x.Name));
            pdfFieldsNames.AddRange(allfields.CheckBoxFields.Select(x => x.Name));
            pdfFieldsNames.AddRange(allfields.RadioGroupFields.Select(x => x.Name));
            if (!pdfFieldsNames.Any())
            {
                throw new InvalidOperationException(ResultCode.NotAllFieldsExistsInDocuments.GetNumericString());
            }

            byte[] htmlBytes = _dataUriScheme.GetBytes(htmlFile);            
            string htmlContent = Encoding.UTF8.GetString(htmlBytes);
            foreach (var pdfField in pdfFieldsNames)
            {
                bool isHtmlContainFieldId = htmlContent.Contains($"id=\"{pdfField}\"") || htmlContent.Contains($"name=\"{pdfField}\"");
                if (!isHtmlContainFieldId)
                {
                    throw new InvalidOperationException(ResultCode.FieldNameNotExist.GetNumericString(), new Exception($"pdf field name [{pdfField}] not exist in html file"));
                }
            }

            return htmlBytes;
        }

        public async Task ResetPassword(User user)
        {
            if (user == null)
            {
                throw new Exception("Null input - user is null");
            }
            
            string newPassword = user?.Password;
            var userId = _userClaims?.Claims?.FirstOrDefault(x => x.Type == ClaimTypes.Sid)?.Value;
            if (userId == null)
            {
                throw new InvalidOperationException(ResultCode.InvalidToken.GetNumericString());
            }
            using var scope = _scopeFactory.CreateScope();
            IUserConnector userConnector = scope.ServiceProvider.GetService<IUserConnector>();

            user.Id = new Guid(userId);
            user = await userConnector.Read(user);
            if (user?.Type != UserType.SystemAdmin)
            {
                throw new InvalidOperationException(ResultCode.InvalidUserType.GetNumericString());
            }
            await HandleUserPasswordsHistory(user, newPassword);
            user.Password = _pkbdf2Handler.Generate(newPassword);
            user.PasswordSetupTime = _dater.UtcNow();
            await userConnector.Update(user, false);
        }

        public async Task UpdateEmail(User user)
        {
            if (user == null)
            {
                throw new Exception("Null input - user is null");
            }
            string newEmail = user?.Email;
            var userId = _userClaims?.Claims?.FirstOrDefault(x => x.Type == ClaimTypes.Sid)?.Value;
            if (userId == null)
            {
                throw new InvalidOperationException(ResultCode.InvalidToken.GetNumericString());
            }
            using var scope = _scopeFactory.CreateScope();
            IUserConnector userConnector = scope.ServiceProvider.GetService<IUserConnector>();
            user.Id = new Guid(userId);
            user = await userConnector.Read(user);
            if (user?.Type != UserType.SystemAdmin)
            {
                throw new InvalidOperationException(ResultCode.InvalidUserType.GetNumericString());
            }
            user.Email = newEmail;
            await userConnector.Update(user, false);

        }

        public async Task CreateUserFromManagment(User user)
        {
            using var scope = _scopeFactory.CreateScope();
            IUserConnector userConnector = scope.ServiceProvider.GetService<IUserConnector>();            
            ICompanyConnector companyConnector = scope.ServiceProvider.GetService<ICompanyConnector>();
            IProgramConnector programConnector = scope.ServiceProvider.GetService<IProgramConnector>();
            IGroupConnector groupConnector = scope.ServiceProvider.GetService<IGroupConnector>();

            var systemUser = await GetCurrentUser();
            var dbUser = await userConnector.Read(new User() { Email = user.Email, Username = user.Username });
            if (await userConnector.Exists(dbUser))
            {
                throw new InvalidOperationException(ResultCode.EmailBelongToOtherCompany.GetNumericString());
            }
            if (user?.Type != UserType.SystemAdmin)
            {
                var company = await companyConnector.Read(new Company() { Id = user.CompanyId });
                if (company == null)
                {
                    throw new InvalidOperationException(ResultCode.CompanyNotExist.GetNumericString());
                }
                var group =await groupConnector.Read(new Group() { Id = user.GroupId, CompanyId = user.CompanyId });
                if (group == null)
                {
                    throw new InvalidOperationException(ResultCode.InvalidGroupId.GetNumericString());
                }

                Program program = await programConnector.Read(new Program() { Id = company.ProgramId });
                IEnumerable<User> users = userConnector.GetAllUsersInCompany(company);
                if (users.Count() >= program.Users && program.Users != Consts.UNLIMITED)
                {
                    throw new InvalidOperationException(ResultCode.UsersExceedLicenseLimit.GetNumericString());
                }
                user.CompanyId = company.Id;
                user.GroupId = group.Id;
            }
            else
            {
                user.CompanyId = new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1);
                user.Password = _pkbdf2Handler.Generate(user.Password);
                user.PasswordSetupTime = _dater.UtcNow();
            }
            user.CreationTime = _dater.UtcNow();
            user.CreationSource = Common.Enums.CreationSource.Application;
            user.Status = UserStatus.Activated;
            //if (!string.IsNullOrWhiteSpace(user.Username))
            //{
            //    user.Username = _symmetricEncryptor.EncryptString(Consts.symmetricKey, user.Username);
            //}

            await userConnector.Create(user);
            _logger.Information("Successfully create user : Name [{UserName}], Id [{UserId}] ---------- by SystemAdmin User [{SystemUserEmail}]", user.Name, user.Id, systemUser.Email);

            if (user?.Type != UserType.SystemAdmin)
            {
                await SendResetPasswordMail(user);
            }
        }

        public async Task ResendResetPasswordMail(User user)
        {
            if (user == null)
            {
                throw new Exception($"Null input - user is null");
            }

            using var scope = _scopeFactory.CreateScope();
            IUserConnector userConnector = scope.ServiceProvider.GetService<IUserConnector>();
           
            user = await userConnector.Read(user);
            if (user == null)
            {
                throw new InvalidOperationException(ResultCode.UserNotExist.GetNumericString());
            }
            if (user.Type == UserType.SystemAdmin || user.Type == UserType.Ghost || user.Type == UserType.PaymentAdmin)
            {
                throw new InvalidOperationException(ResultCode.InvalidUserType.GetNumericString());
            }

            await SendResetPasswordMail(user);
        }

        public async Task<User> Read(User user)
        {
            using var scope = _scopeFactory.CreateScope();
            IUserConnector userConnector = scope.ServiceProvider.GetService<IUserConnector>();
            
            return await userConnector.Read(user);

        }

        public async Task<User> GetCurrentUser()
        {
            var userId = _userClaims?.Claims?.FirstOrDefault(x => x.Type == ClaimTypes.Sid)?.Value;
            if (userId == null)
            {
                throw new InvalidOperationException(ResultCode.InvalidToken.GetNumericString());
            }
            using var scope = _scopeFactory.CreateScope();
            IUserConnector userConnector = scope.ServiceProvider.GetService<IUserConnector>();
            
            var user = await userConnector.Read(new User() { Id = new Guid(userId) });
            if (user == null)
            {
                throw new InvalidOperationException(ResultCode.InvalidToken.GetNumericString());
            }

            return user;
        }

        public async Task UpdateUser(User user)
        {
            using var scope = _scopeFactory.CreateScope();
            IUserConnector userConnector = scope.ServiceProvider.GetService<IUserConnector>();
            
            var systemUser = await userConnector.Read(user);
            if (systemUser == null)
            {
                throw new InvalidOperationException(ResultCode.UserNotExist.GetNumericString());
            }
            var dbUser = new User();
            if (systemUser.Email != user.Email)
            {
                dbUser.Email = user.Email;
            }
            if (systemUser.Username != user.Username)
            {
                dbUser.Username = user.Username;
            }
            dbUser = await userConnector.Read(dbUser);
            if (await userConnector.Exists(dbUser))
            {
                throw new InvalidOperationException(ResultCode.UserBelongToAnotherCompany.GetNumericString());
            }

            systemUser.Name = user.Name;
            systemUser.Username = user.Username;
            systemUser.Email = user.Email;
            systemUser.Username = user.Username;
            systemUser.Type = user.Type;

            await userConnector.Update(systemUser, false);
        }

        public async Task<List<Group>> GetUserGroups()
        {
            var user = await GetCurrentUser();
            using var scope = _scopeFactory.CreateScope();
            IGroupConnector groupConnector = scope.ServiceProvider.GetService<IGroupConnector>();
            var userGroups = await groupConnector.ReadAllUserGroups(user);
            return userGroups;

        }

        public async Task<List<Group>> GetUserGroups(User user)
        {
            using var scope = _scopeFactory.CreateScope();
            IGroupConnector groupConnector = scope.ServiceProvider.GetService<IGroupConnector>();
            var userGroups = await groupConnector.ReadAllUserGroups(user);
            return userGroups;

        }

        private async Task SendResetPasswordMail(User user)
        {
            await _oneTimeTokens.GenerateRefreshToken(user);
            string resetPasswordToken = await _oneTimeTokens.GenerateResetPasswordToken(user);
            await _email.ResetPassword(user, resetPasswordToken);
        }

        private async Task HandleUserPasswordsHistory(User user, string newPassword)
        {
            using var scope = _scopeFactory.CreateScope();
            ICompanyConnector companyConnector = scope.ServiceProvider.GetService<ICompanyConnector>();
            IUserPasswordHistoryConnector userPasswordHistoryConnector = scope.ServiceProvider.GetService<IUserPasswordHistoryConnector>();
            
            var userCompany = new Company() { Id = user.CompanyId };
            var dbCompany =await companyConnector.Read(userCompany);

            if (dbCompany == null)
            {
                throw new InvalidOperationException(ResultCode.CompanyNotExist.GetNumericString());
            }

            if (dbCompany.CompanyConfiguration.RecentPasswordsAmount == 0)
            {
                await userPasswordHistoryConnector.DeleteAllByUserId(user.Id);
                return;
            }

            var dbUserPasswordHistories = userPasswordHistoryConnector.ReadAllByUserId(user.Id);
            var numberOfPasswordsToDelete = dbUserPasswordHistories.Count() - dbCompany.CompanyConfiguration.RecentPasswordsAmount;

            if (numberOfPasswordsToDelete > 0)
            {
                await userPasswordHistoryConnector.DeleteOldestPasswordsByUserId(user.Id, numberOfPasswordsToDelete);
            }

            if (!IsPasswordNew(user, newPassword, userPasswordHistoryConnector))
            {
                throw new InvalidOperationException(ResultCode.PasswordAlreadyUsed.GetNumericString());
            }

            if (numberOfPasswordsToDelete >= 0)
            {
                await userPasswordHistoryConnector.DeleteOldestPasswordsByUserId(user.Id, 1);
            }

            if (!string.IsNullOrEmpty(user.Password))
            {
                CreateUserPasswordHistory(user, userPasswordHistoryConnector);
            }
        }

        private bool IsPasswordNew(User user, string password, IUserPasswordHistoryConnector userPasswordHistoryConnector)
        {
            var dbUserPasswordHistories = userPasswordHistoryConnector.ReadAllByUserId(user.Id);
            foreach (var item in dbUserPasswordHistories)
            {
                if (_pkbdf2Handler.Check(item.Password, password))
                {
                    return false;
                }
            }
            return true;
        }

        private void CreateUserPasswordHistory(User user, IUserPasswordHistoryConnector userPasswordHistoryConnector)
        {
            var userPasswordHistory = new UserPasswordHistory()
            {
                UserId = user.Id,
                Password = user.Password,
                CreationTime = _dater.UtcNow()
            };
            userPasswordHistoryConnector.Create(userPasswordHistory);
        }
    }
}
