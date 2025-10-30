using Common.Consts;
using Common.Enums;
using Common.Enums.Companies;
using Common.Enums.Groups;
using Common.Enums.Program;
using Common.Enums.Results;
using Common.Enums.Users;
using Common.Extensions;
using Common.Interfaces;
using Common.Interfaces.DB;
using Common.Interfaces.Emails;
using Common.Interfaces.ManagementApp;
using Common.Models;
using Common.Models.Configurations;
using Common.Models.Documents;
using Common.Models.License;
using Common.Models.ManagementApp;
using Common.Models.Programs;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace ManagementBL.Handlers
{
    public class CompaniesHandler : ICompanies
    {
        
        private readonly IConfiguration _configuration;
        private readonly IEmail _email;
        private readonly IOneTimeTokens _oneTimeTokens;
        private readonly ILogger _logger;
        private readonly IEncryptor _encryptor;
        private readonly IDater _dater;
        private readonly ILicense _license;
        private readonly IActiveDirectoryGroupsConnector _activeDirectoryGroupsConnector;
        private readonly Common.Interfaces.ManagementApp.IUsers _users;
        private readonly IValidator _validator;
        private readonly ICompanyConnector _companyConnector;
        private readonly IUserConnector _userConnector;
        private readonly IProgramConnector _programConnector;
        private readonly IContactConnector _contactConnector;
        private readonly IGroupConnector _groupConnector;
        private readonly ITemplateConnector _templateConnector;
        private readonly IDocumentCollectionConnector _documentCollectionConnector;
        private readonly IProgramUtilizationConnector _programUtilizationConnector;
        private readonly IProgramUtilizationHistoryConnector _programUtilizationHistoryConnector;

        public CompaniesHandler(ICompanyConnector companyConnector, IUserConnector userConnector,IProgramConnector programConnector, IGroupConnector groupConnector, 
            IProgramUtilizationConnector programUtilizationConnector, IProgramUtilizationHistoryConnector programUtilizationHistoryConnector, 
             ITemplateConnector templateConnector,IContactConnector contactConnector, IDocumentCollectionConnector documentCollectionConnector, IConfiguration configuration,
                                IEmail email, IOneTimeTokens oneTimeTokens, ILogger logger, IEncryptor encryptor,
                                IDater dater, ILicense license, IActiveDirectoryGroupsConnector activeDirectoryGroupsConnector,
                                Common.Interfaces.ManagementApp.IUsers users, IValidator validator)
        {
            _companyConnector = companyConnector;
            _userConnector = userConnector;
            _programConnector = programConnector;
            _contactConnector = contactConnector;
            _groupConnector = groupConnector;
            _templateConnector = templateConnector;
            _documentCollectionConnector = documentCollectionConnector;
            _programUtilizationConnector = programUtilizationConnector;
            _programUtilizationHistoryConnector = programUtilizationHistoryConnector;
            _configuration = configuration;
            _oneTimeTokens = oneTimeTokens;
            _email = email;
            _logger = logger;
            _encryptor = encryptor;
            _dater = dater;
            _license = license;
            _activeDirectoryGroupsConnector = activeDirectoryGroupsConnector;
            _users = users;
            _validator = validator;
           
        }

        public async Task<(IEnumerable<CompanyBaseDetails>, int totalCount)> Read(string key, int offset, int limit)
        {
            
            var result = new List<CompanyBaseDetails>();
            IEnumerable<Company> companies = _companyConnector.Read(key, offset, limit, CompanyStatus.Created, out int totalCount)
                .Where(x => x.Id != Consts.FREE_ACCOUNTS_COMPANY_ID &&
                x.Status != CompanyStatus.Deleted);
            foreach (var company in companies)
            {
                if (company.ProgramUtilization != null)
                {
                    company.ProgramUtilization =await _programUtilizationConnector.Read(new ProgramUtilization { Id = company.ProgramUtilization.Id });
                }
                string programName = (await _programConnector.Read(new Program { Id = company.ProgramId }))?.Name;
                var users = _userConnector.Read(Consts.EMPTY, 0, Consts.UNLIMITED, null, out _, company.Id);
                var adminUsers = users.Where(x => x.Type == UserType.CompanyAdmin || x.Type == UserType.SystemAdmin)
                                        .Select(x => (x.Id, x.Email)).ToList();
                var companyDetails = new CompanyBaseDetails(company)
                {
                    UsersEmails = adminUsers,
                    ProgramName = programName
                };
                result.Add(companyDetails);
            }

            return (result, totalCount);
        }

        public async Task<CompanyExpandedDetails> Read(Company company, User user)
        {
            if (company == null || user == null)
            {
                throw new Exception($"Null input - user or company is null");
            }
            company = await _companyConnector.Read(company);

            if (company == null)
            {
                return null;
            }
            var groups = _groupConnector.Read(company).Where(g => g.GroupStatus != GroupStatus.Deleted).Select(x => (x.Id, x.Name)).ToList();
            user = await _userConnector.Read(user);
            GetCompanyLogo(company);
            GetCompanyEmailTemplate(company);
            if (company.CompanySigner1Details != null)
            {
                company.CompanySigner1Details.CertId = _encryptor.Decrypt(company.CompanySigner1Details.CertId);
            }
            var result = new CompanyExpandedDetails
            {
                Id = company.Id,
                Name = company.Name,
                CompanyConfiguration = company.CompanyConfiguration,
                ExpirationTime = company.ProgramUtilization?.Expired ?? DateTime.MinValue,
                ProgramId = company.ProgramId,
                User = user?.Status != UserStatus.Deleted ? user : null,
                Groups = groups,
                CompanySigner1Details = company.CompanySigner1Details,
                TransactionId = company.TransactionId,


            };
            
            (var license, var _) =await _license.GetLicenseInformationAndUsing( false);
            if (license.LicenseCounters.UseActiveDirectory)
            {
                result.ActiveDirectoryGroups = await _activeDirectoryGroupsConnector.GetAllGroupsForCompany(company);
            }

            return result;
        }

        public async Task<DocumentDeletionConfiguration> ReadCompanyDeletionConfiguration(Company company)
        {
            if (company == null)
            {
                return null;
            }
            company = await _companyConnector.Read(company);
            if (company == null)
            {
                return null;
            }
            var deletionConfiguration = new DocumentDeletionConfiguration()
            {
                DeleteSignedDocumentAfterXDays = company.CompanyConfiguration.DocumentDeletionConfiguration.DeleteSignedDocumentAfterXDays,
                DeleteUnsignedDocumentAfterXDays = company.CompanyConfiguration.DocumentDeletionConfiguration.DeleteUnsignedDocumentAfterXDays
            };
            return deletionConfiguration;
        }
        public async Task Create(Company company, Group group, User user)
        {
            var systemUser = await _users.GetCurrentUser();
            if (company == null || user == null || group == null)
            {
                throw new Exception($"Null input - user or company or group is null");
            }

            company.CompanyConfiguration.Base64Logo = (await _validator.ValidateIsCleanFile(company.CompanyConfiguration?.Base64Logo))?.CleanFile;
            company.CompanyConfiguration.EmailTemplates.BeforeSigningBase64String = (await _validator.ValidateIsCleanFile(company.CompanyConfiguration?.EmailTemplates?.BeforeSigningBase64String))?.CleanFile;
            company.CompanyConfiguration.EmailTemplates.AfterSigningBase64String = (await _validator.ValidateIsCleanFile(company.CompanyConfiguration?.EmailTemplates?.AfterSigningBase64String))?.CleanFile;

            var companies = _companyConnector.Read(company.Name, 0, Consts.UNLIMITED, null, out _);

            var existCompany = companies.FirstOrDefault(x => x.Name == company.Name && x.Status != CompanyStatus.Deleted);
            if (existCompany != null)
            {
                throw new InvalidOperationException(ResultCode.CompanyAlreadyExists.GetNumericString());
            }

            var dbUser = await _userConnector.Read(user);
            if (await _userConnector.Exists(user))
            {
                if (!_programConnector.IsFreeTrialUser(dbUser) && dbUser.CompanyId != company.Id)
                {
                    throw new InvalidOperationException(ResultCode.EmailBelongToOtherCompany.GetNumericString());
                }
            }

            await _license.ValidateProgramAddition(new Program { Id = company.ProgramId }, companies);
            company.CompanySigner1Details.CertId = _encryptor.Encrypt(company.CompanySigner1Details.CertId);
            company.CompanySigner1Details.CertPassword = _encryptor.Encrypt(company.CompanySigner1Details.CertPassword);
            company.CompanyConfiguration?.MessageProviders.ForEach(x => x.Password = _encryptor.Encrypt(x.Password));
            
            await _companyConnector.Create(company);
            _logger.Information("Successfully create company : Name [{CompanyName}], Id [{CompanyId}] ---------- by SystemAdmin User [{SystemUserEmail}]", company.Name, company.Id, systemUser.Email);

            try
            {
                group.CompanyId = company.Id;
                await _groupConnector.Create(group);
                _logger.Information("Successfully create group : Name [{GroupName}], Id [{GroupId}], companyId [{GroupCompanyId}]", group.Name, group.Id, group.CompanyId);

                await CreateCompanyAdminUser(company, group, user);
            }
            catch
            {
                await _programUtilizationConnector.Delete(company.ProgramUtilization);
                await _companyConnector.Delete(company);
                throw;
            }

            _configuration.SetCompanyLogo(user, company.CompanyConfiguration?.Base64Logo);
            _configuration.UpdateCompanyEmailHtml(user, company.CompanyConfiguration?.EmailTemplates);

            bool isFreeTrailUser = _programConnector.IsFreeTrialUser(dbUser);
            await SendResetPasswordMail(user, isFreeTrailUser);
        }

        public async Task Update(Company company, Group group, User user)
        {
            if (company == null)
            {
                throw new Exception($"Null input - user or company or group is null");
            }
            await UpdateCompany(company);
            await UpdateGroup(group);
            await UpdateUser(company, group, user);
            UpdateCompanyFiles(company, user);
        }

        public async Task UpdateCompanyTransactionAndExpirationTime(Company company)
        {
            var dbCompany = await _companyConnector.Read(company);
            if (dbCompany == null || dbCompany?.Id != company.Id)
            {
                throw new InvalidOperationException(ResultCode.CompanyNotExist.GetNumericString());
            }
            await _companyConnector.Update(company);
        }

        public async Task UpdateTransactionId(Company company)
        {
            
            var dbCompany = await _companyConnector.Read(company);
            if (dbCompany == null || dbCompany?.Id != company.Id)
            {
                throw new InvalidOperationException(ResultCode.CompanyNotExist.GetNumericString());
            }
            dbCompany.TransactionId = company.TransactionId;
           await _companyConnector.Update(dbCompany);
        }
        public async Task Delete(Company company)
        {
            var systemUser = await _users.GetCurrentUser();
            var dbCompany = await _companyConnector.Read(company);
            await _companyConnector.Delete(company);
            _logger.Information("Successfully delete company : Name [{DbCompanyName}], Id [{CompanyId}] ---------- by SystemAdmin User [{SystemUserEmail}]", dbCompany?.Name, company.Id, systemUser.Email);
        }

        public async Task ResendResetPasswordMail(User user)
        {
            if (user == null)
            {
                throw new Exception($"Null input - user is null");
            }

            user = await _userConnector.Read(user);
            if (user == null)
            {
                throw new InvalidOperationException(ResultCode.UserNotExist.GetNumericString());
            }
            if (user.Type != UserType.CompanyAdmin)
            {
                throw new InvalidOperationException(ResultCode.InvalidUserType.GetNumericString());
            }

            await SendResetPasswordMail(user, false);
        }

        public Task AddProgramUtilizationCompanyToHistory(Company company, ProgramUtilizationHistoryResourceMode mode)
        {
            ProgramUtilizationHistory programUtilizationHistory = new ProgramUtilizationHistory
            {
                DocumentsUsage = company.ProgramUtilization.DocumentsUsage,
                SmsUsage = company.ProgramUtilization.SMS,
                VisualIdentificationsUsage = company.ProgramUtilization.VisualIdentifications,
                UpdateDate = _dater.UtcNow(),
                CompanyId = company.Id,
                CompanyName = company.Name,
                ResourceMode = mode,
                UsersUsage = mode == ProgramUtilizationHistoryResourceMode.FromUpdateComapny ? company.ProgramUtilization.Users : 0,
                TemplatesUsage = mode == ProgramUtilizationHistoryResourceMode.FromUpdateComapny ? company.ProgramUtilization.Templates : 0,
                Expired = company.ProgramUtilization.Expired
            };
           return _programUtilizationHistoryConnector.Create(programUtilizationHistory);
        }

        #region Private Functions
        private async Task UpdateGroup(Group group)
        {
            var dbGroup = await _groupConnector.Read(group);
            if (dbGroup == null || dbGroup.CompanyId != group.CompanyId)
            {
                await _groupConnector.Create(group);
            }
            else
            {
                group.Id = dbGroup.Id;
            }
        }

        private void GetCompanyEmailTemplate(Company company)
        {
            var beforeSigningBase64file = _configuration.GetCompanyEmailHtml(company.Id, MessageType.BeforeSigning);
            var afterSigningBase64file = _configuration.GetCompanyEmailHtml(company.Id, MessageType.AfterSigning);
            if (company.CompanyConfiguration != null)
            {
                company.CompanyConfiguration.EmailTemplates.BeforeSigningBase64String = beforeSigningBase64file;
                company.CompanyConfiguration.EmailTemplates.AfterSigningBase64String = afterSigningBase64file;
            }
            else
            {
                company.CompanyConfiguration = new CompanyConfiguration
                {
                    EmailTemplates = new EmailHtmlBodyTemplates
                    {
                        BeforeSigningBase64String = beforeSigningBase64file,
                        AfterSigningBase64String = afterSigningBase64file
                    }
                };
            }
        }

        private void GetCompanyLogo(Company company)
        {
            var base64image =  _configuration.GetCompanyLogo(company.Id);
            if (!string.IsNullOrWhiteSpace(base64image))
            {
                if (company.CompanyConfiguration != null)
                {
                    company.CompanyConfiguration.Base64Logo = base64image;
                }
                else
                {
                    company.CompanyConfiguration = new CompanyConfiguration
                    {
                        Base64Logo = base64image
                    };
                }
            }
        }

        private async Task UpdateUser(Company company, Group group, User user)
        {
            var dbUser = await _userConnector.Read(user);

            await CreateCompanyAdminUser(company, group, dbUser ?? user);
            if (dbUser == null)
            {
                await IncreaseCompanyUsersByOne(company);
                await SendResetPasswordMail(user, false);
            }
        }

        private async Task IncreaseCompanyUsersByOne(Company company)
        {
            var users = _userConnector.Read(Consts.EMPTY, 0, 1, UserStatus.Activated, out int totalCount, company.Id); // TODO - check? 
            var companyUser = users.FirstOrDefault();
            if (companyUser != null && await _programConnector.CanAddUser(companyUser))
            {
                await _programUtilizationConnector.UpdateUsersAmount(companyUser, CalcOperation.Add);
            }
        }

        private async Task UpdateCompany(Company company)
        {
            var dbCompany = await _companyConnector.Read(company);
            if (dbCompany == null || dbCompany?.Id != company.Id)
            {
                throw new InvalidOperationException(ResultCode.CompanyNotExist.GetNumericString());
            }
            dbCompany.Name = company.Name;
            //In case program changed
            if (dbCompany.ProgramId != company.ProgramId)
            {
                bool isProgramExist =await  _programConnector.Exists(new Program { Id = company.ProgramId });
                if (!isProgramExist)
                {
                    throw new InvalidOperationException(ResultCode.ProgramNotExist.GetNumericString());
                }
                var companies = _companyConnector.Read(company.Name, 0, Consts.UNLIMITED, null, out _).Where(x => x.Id != company.Id);
                await _license.ValidateProgramAddition(new Program { Id = company.ProgramId }, companies);

                await ResetProgramUtilization(dbCompany);
            }
            dbCompany.ProgramId = company.ProgramId;
            if (company.ProgramUtilization?.Expired != null)
            {
                dbCompany.ProgramUtilization.Expired = company.ProgramUtilization.Expired;
            }

            company.CompanyConfiguration?.MessageProviders.ForEach(x => x.Password = GetPassword(x, dbCompany.CompanyConfiguration.MessageProviders));
            dbCompany.CompanyConfiguration = company.CompanyConfiguration;
            company.CompanySigner1Details.CertId = _encryptor.Encrypt(company.CompanySigner1Details.CertId);
            if (dbCompany.CompanySigner1Details != null && dbCompany.CompanySigner1Details.CertPassword != company.CompanySigner1Details.CertPassword)
            {
                company.CompanySigner1Details.CertPassword = _encryptor.Encrypt(company.CompanySigner1Details.CertPassword);
            }
            dbCompany.TransactionId = company.TransactionId;
            dbCompany.CompanySigner1Details = company.CompanySigner1Details;
            await _companyConnector.Update(dbCompany);
            company.ProgramUtilization = dbCompany.ProgramUtilization;
        }

        private async Task ResetProgramUtilization(Company dbCompany)
        {
            await AddProgramUtilizationCompanyToHistory(dbCompany, ProgramUtilizationHistoryResourceMode.FromUpdateComapny);
            dbCompany.ProgramUtilization.DocumentsUsage = 0;
            dbCompany.ProgramUtilization.DocumentsSentNotifyCount = 0;
            dbCompany.ProgramUtilization.SMS = 0;
            dbCompany.ProgramUtilization.SmsSentNotifyCount = 0;
            dbCompany.ProgramUtilization.VisualIdentifications = 0;
            dbCompany.ProgramUtilization.VisualIdentificationUsedNotifyCount = 0;
            dbCompany.ProgramUtilization.Users = 0;
            dbCompany.ProgramUtilization.Templates = 0;
            dbCompany.ProgramUtilization.StartDate = DateTime.UtcNow;
        }

        private string GetPassword(MessageProvider messageProvider, IEnumerable<MessageProvider> dbMessageProviders)
        {
            var exist = dbMessageProviders.FirstOrDefault(y => y.ProviderType == messageProvider.ProviderType &&
                                                            y.Password == messageProvider.Password);
            if (exist == null)
            {
                return _encryptor.Encrypt(messageProvider.Password);
            }
            return exist.Password;
        }

        /// <summary>
        /// In case user email is free trial user we will transfer him to created company.
        /// </summary>
        /// <param name="company"></param>
        /// <param name="group"></param>
        /// <param name="user"></param>
        private async Task CreateCompanyAdminUser(Company company, Group group, User user)
        {

            if (await _userConnector.Exists(user))
            {
                var dbUser = await _userConnector.Read(user);
                if (!_programConnector.IsFreeTrialUser(dbUser) && dbUser.CompanyId != company.Id)
                {
                    throw new InvalidOperationException(ResultCode.EmailBelongToOtherCompany.GetNumericString());
                }

                await UserDbUpdate(company, group, dbUser);
            }
            else
            {
                user.GroupId = group.Id;
                user.Type = UserType.CompanyAdmin;
                user.CompanyId = company.Id;
                user.CreationTime = _dater.UtcNow();
                await _userConnector.Create(user);
                _logger.Information("Successfully create user : Id [{UserId}], Email [{UserEmail}]", user.Id, user.Email);

            }
        }

        private async Task UserDbUpdate(Company company, Group group, User dbUser)
        {
            bool isFreeTrailUser = _programConnector.IsFreeTrialUser(dbUser);
            if (isFreeTrailUser)
            {
                await _groupConnector.Delete(new Group { Id = dbUser.GroupId, CompanyId = Consts.FREE_ACCOUNTS_COMPANY_ID });

                dbUser.ProgramUtilizationId = null;
                await _userConnector.Update(dbUser, false);
                await _programUtilizationConnector.Delete(new ProgramUtilization { Id = (Guid)dbUser.ProgramUtilization.Id });
                await UpdateFreeTrailTemplates(group, dbUser);
                await UpdateFreeTrailContacts(group, dbUser);
                await UpdateFreeTrailDocumentCollections(group, dbUser);
            }

            dbUser = await _userConnector.Read(dbUser);
            dbUser.GroupId = group.Id;
            dbUser.Type = UserType.CompanyAdmin;
            dbUser.CompanyId = company.Id;

           await _userConnector.Update(dbUser, true);
            _logger.Information("Successfully update user : Id [{DbUserId}], Email [{DbUserEmail}]", dbUser.Id, dbUser.Email);
        }

        private async Task UpdateFreeTrailTemplates(Group newGroup, User dbUser)
        {
            var templates = _templateConnector.Read(dbUser, Consts.EMPTY, Consts.EMPTY, Consts.EMPTY, 0, Consts.UNLIMITED, true, true, out _).ToList();
            foreach(Template x in  templates?? Enumerable.Empty<Template>())
            {
                x.GroupId = newGroup.Id;
                await _templateConnector.Update(x);
            }
        }

        private async Task UpdateFreeTrailContacts(Group newGroup, User dbUser)
        {
            var contacts = _contactConnector.Read(dbUser, "", 0, Consts.UNLIMITED, true, true, out _, true);
            foreach(Contact contact in contacts ?? Enumerable.Empty<Contact>())
            {
                contact.GroupId = newGroup.Id;
                await _contactConnector.Update(contact);
            }
        }

        private async Task UpdateFreeTrailDocumentCollections(Group newGroup, User dbUser)
        {
            var documentCollections = _documentCollectionConnector.Read(dbUser, "", true, true, true, true, true, true, "", "", 0, Consts.UNLIMITED, out _);
           foreach(var documentCollection in  documentCollections ?? Enumerable.Empty<DocumentCollection>())
            {
                documentCollection.GroupId = newGroup.Id;
                await _documentCollectionConnector.Update(documentCollection);
            }
        }

        private void UpdateCompanyFiles(Company company, User user)
        {
            if (!string.IsNullOrWhiteSpace(company.CompanyConfiguration?.Base64Logo))
            {
                _configuration.SetCompanyLogo(user, company.CompanyConfiguration?.Base64Logo);
            }
            else
            {
                _configuration.DeleteCompanyLogo(user);
            }
            if (company.CompanyConfiguration?.EmailTemplates != null)
            {
                _configuration.UpdateCompanyEmailHtml(user, company.CompanyConfiguration?.EmailTemplates);
            }
        }

        private async Task SendResetPasswordMail(User user, bool isFreeTrailUser)
        {
            if (isFreeTrailUser)
            {
                return;
            }
            await _oneTimeTokens.GenerateRefreshToken(user);
            string resetPasswordToken = await _oneTimeTokens.GenerateResetPasswordToken(user);
            await _email.ResetPassword(user, resetPasswordToken);
        }

        #endregion
    }
}
