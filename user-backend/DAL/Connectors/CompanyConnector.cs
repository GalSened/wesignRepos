using Common.Consts;
using Common.Enums.Companies;
using Common.Enums.Groups;
using Common.Enums.Users;
using Common.Extensions;
using Common.Interfaces.DB;
using Common.Models;
using Common.Models.Configurations;
using DAL.DAOs.Companies;
using DAL.DAOs.Programs;
using DAL.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DAL.Connectors
{
    public class CompanyConnector : ICompanyConnector
    {
        private readonly IWeSignEntities _dbContext;
        private readonly IProgramUtilizationConnector _programUtilizationConnector;
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger _logger;

        public CompanyConnector(IWeSignEntities dbContext, IProgramUtilizationConnector programUtilizationConnector,
             IMemoryCache memoryCache, ILogger logger)
        {
            _dbContext = dbContext;
            _programUtilizationConnector = programUtilizationConnector;
            _memoryCache = memoryCache;
            _logger = logger;
        }

        public async Task Create(Company company)
        {
            try
            {
                var companyDAO = new CompanyDAO(company);
                await _dbContext.Companies.AddAsync(companyDAO);
                await _dbContext.SaveChangesAsync();
                company.Id = companyDAO.Id;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in CompanyConnector_Create = ");
                throw;
            }
        }

        public async Task Delete(Company company)
        {
            try
            {
                _memoryCache.Remove(company.Id);
                var existComapny = _dbContext.Companies.Local.FirstOrDefault(x => x.Id == company.Id) ??
                                        await _dbContext.Companies
                                        .Include(x => x.Users)
                                        .Include(x => x.Groups).
                                        FirstOrDefaultAsync(x => x.Id == company.Id);
                if (existComapny != null)
                {
                    existComapny.Users.ForEach(user => user.Status = UserStatus.Deleted);
                    existComapny.Groups.ForEach(group => group.GroupStatus = GroupStatus.Deleted);
                    existComapny.Status = CompanyStatus.Deleted;
                    _dbContext.Companies.Update(existComapny);

                    await _dbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in CompanyConnector_DeleteByCompany = ");
                throw;
            }
        }

        public Task DeleteRange(IEnumerable<Company> companies)
        {
            try
            {
                _dbContext.Companies.RemoveRange(companies.Select(C => new CompanyDAO(C)));
                return _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in CompanyConnector_DeleteRange = ");
                throw;
            }
        }


        public Task Delete(Company company, Action<Company> cleanCompanyLogoAndEmailsFromFS)
        {
            try
            {
                var strategy = _dbContext.Database.CreateExecutionStrategy();

                return strategy.ExecuteAsync(
                      async () =>
                      {
                          try
                          {

                              _dbContext.Database.SetCommandTimeout(100);
                              _memoryCache.Remove(company.Id);

                              using (var transaction = await _dbContext.Database.BeginTransactionAsync())
                              {
                                  try
                                  {
                                      var companyDAO = await _dbContext.Companies.FirstOrDefaultAsync(x => x.Id == company.Id);
                                      _dbContext.Companies.Remove(companyDAO);
                                      await _dbContext.SaveChangesAsync();
                                      cleanCompanyLogoAndEmailsFromFS(company);
                                      await transaction.CommitAsync();

                                  }
                                  catch
                                  {
                                      await transaction.RollbackAsync();
                                      throw;
                                  }
                              }
                          }
                          finally
                          {
                              _dbContext.Database.SetCommandTimeout(30);
                          }
                      });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in CompanyConnector_DeleteByCompanyAndCleanCompanyLogoAndEmailsFromFS = ");
                throw;
            }
        }

        public IEnumerable<Company> ReadCompaniesByProgram(Program program)
        {
            try
            {
                return _dbContext.Companies.Where(x => x.ProgramId == program.Id).Select(x => x.ToCompany()).AsEnumerable();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in CompanyConnector_ReadCompaniesByProgram = ");
                throw;
            }
        }

        public async Task<CompanyConfiguration> ReadConfiguration(Company company)
        {
            try
            {
                var memCompanyConfiguration = _memoryCache.Get<CompanyConfiguration>($"{company.Id}_ReadConfiguration");
                if (memCompanyConfiguration != null)
                {
                    return memCompanyConfiguration;
                }
                var companyConfiguration = await _dbContext.CompanyConfiguration.Include(x => x.CompanyMessages).Include(x => x.MessageProviders).FirstOrDefaultAsync(x => x.CompanyId == company.Id);
                var result = companyConfiguration.ToCompanyConfiguration();
                if (result != null)
                {
                    _memoryCache.Set<CompanyConfiguration>($"{company.Id}_ReadConfiguration", result, TimeSpan.FromMinutes(6));
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in CompanyConnector_ReadConfiguration = ");
                throw;
            }
        }

        public async Task<Company> Read(Company company)
        {
            try
            {
                var memCompany = _memoryCache.Get<Company>(company.Id);
                if (memCompany != null)
                {
                    return memCompany;
                }
                var companyDAO = await _dbContext.Companies.Include(c => c.CompanyConfiguration)
                                                     .ThenInclude(x => x.MessageProviders)
                                                     .Include(c => c.CompanyConfiguration)
                                                     .ThenInclude(x => x.CompanyMessages)
                                                     .Include(c => c.CompanySigner1Details)
                                                     .Include(x => x.ProgramUtilization)
                                                     .FirstOrDefaultAsync(x => (x.Id == company.Id || x.Name == company.Name) && x.Status != CompanyStatus.Deleted);

                company = companyDAO.ToCompany();
                if (company != null)
                {
                    company.ProgramUtilization = await _programUtilizationConnector.Read(company.ProgramUtilization);
                    _memoryCache.Set<Company>(company.Id, company, TimeSpan.FromMinutes(3));
                }


                return company;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in CompanyConnector_ReadByCompany = ");
                throw;
            }
        }

        public IEnumerable<Company> ReadForProgramUtilization()
        {
            try
            {
                var query = _dbContext.Companies
                .Include(c => c.ProgramUtilization)
            .Where(c => c.Id != Consts.GHOST_USERS_COMPANY_ID &&
                c.Id != Consts.FREE_ACCOUNTS_COMPANY_ID &&
                ((c.ProgramUtilization.ProgramResetType == Common.Enums.Program.ProgramResetType.Monthly &&
                c.ProgramUtilization.LastResetDate.AddDays(30) < DateTime.UtcNow) || (
                c.ProgramUtilization.ProgramResetType == Common.Enums.Program.ProgramResetType.Yearly &&
                c.ProgramUtilization.LastResetDate.AddDays(365) < DateTime.UtcNow)
                ));

                return query.Select(x => x.ToCompany());
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in CompanyConnector_ReadForProgramUtilization = ");
                throw;
            }
        }

        public IEnumerable<Company> ReadAboutToBeExpired(DateTime from, DateTime to)
        {
            try
            {
                var query = _dbContext.Companies
                .Include(c => c.ProgramUtilization)
                .Include(t => t.CompanyConfiguration).ThenInclude(x => x.MessageProviders)
                .Where(c => c.Id != Consts.GHOST_USERS_COMPANY_ID &&
                c.ProgramUtilization.Expired.Date >= from.Date && c.ProgramUtilization.Expired.Date <= to.Date
                );
                return query.Select(x => x.ToCompany());
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in CompanyConnector_ReadAboutToBeExpired = ");
                throw;
            }
        }


        public IEnumerable<Company> ReadWithReminders(string key, int offset, int limit, CompanyStatus? status, out int totalCount)
        {
            try
            {
                var query = _dbContext.Companies
               .Include(c => c.ProgramUtilization)
               .Include(t => t.CompanyConfiguration).ThenInclude(x => x.MessageProviders)
               .Where(c => c.Id != Consts.GHOST_USERS_COMPANY_ID); //skip(offset)

                query = query.Where(c => c.CompanyConfiguration != null && c.CompanyConfiguration.ShouldEnableSignReminders == true);

                if (!string.IsNullOrWhiteSpace(key))
                {
                    query = query.Where(c => c.Name.Contains(key) || c.Users.FirstOrDefault(u => u.Email.Contains(key)) != null);
                }
                if (status.HasValue)
                {
                    query = query.Where(c => c.Status == status);
                }
                totalCount = query.Count();
                query = query.Skip(offset);
                if (limit != Consts.UNLIMITED)
                {
                    query = query.Take(limit);
                }

                return query.Select(q => q.ToCompany());
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in CompanyConnector_ReadWithReminders = ");
                throw;
            }
        }

        // TODO - add filter by company id 
        public IEnumerable<Company> Read(string key, int offset, int limit, CompanyStatus? status, out int totalCount)
        {
            try
            {
                var query = _dbContext.Companies
                                .Include(c => c.ProgramUtilization)
                                .Include(t => t.CompanyConfiguration).ThenInclude(x => x.MessageProviders)
                                .Where(c => c.Id != Consts.GHOST_USERS_COMPANY_ID); //skip(offset)

                if (!string.IsNullOrWhiteSpace(key))
                {
                    query = query.Where(c => c.Name.Contains(key) || c.Users.FirstOrDefault(u => u.Email.Contains(key)) != null);
                }

                if (status.HasValue)
                {
                    query = query.Where(c => c.Status == status);
                }
                totalCount = query.Count();
                query = query.Skip(offset);
                if (limit != Consts.UNLIMITED)
                {
                    query = query.Take(limit);
                }

                return query.Select(q => q.ToCompany());
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in CompanyConnector_ReadByKey&Offset&Limit = ");
                throw;
            }
        }

        public IEnumerable<Company> Read(Guid programID, int offset, int limit, out int totalCount)
        {
            try
            {
                var query = _dbContext.Companies
                .Include(c => c.ProgramUtilization)
                .Where(c => c.ProgramId == programID && c.Status != CompanyStatus.Deleted);

                totalCount = query.Count();
                query = limit != Consts.UNLIMITED ? query.Skip(offset).Take(limit) : query.Skip(offset);
                return query.Select(c => c.ToCompany());
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in CompanyConnector_ReadByProgramId&Offset&Limit = ");
                throw;
            }
        }

        public IEnumerable<Company> Read(int docsUsagePercentage, int offset, int limit, out int totalCount)
        {
            try
            {
                var query = _dbContext.Companies
                .Include(c => c.ProgramUtilization)
                .Where(c => (c.ProgramUtilization.DocumentsLimit != 0) &&
                (c.ProgramUtilization.DocumentsUsage / c.ProgramUtilization.DocumentsLimit) * 100 >= docsUsagePercentage);

                totalCount = query.Count();
                query = limit != Consts.UNLIMITED ? query.Skip(offset).Take(limit) : query.Skip(offset);
                return query.Select(c => c.ToCompany());
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in CompanyConnector_ReadByDocsUsagePercentage&Offset&Limit = ");
                throw;
            }
        }

        public async Task Update(Company company)
        {
            try
            {
                var companyDAO = await _dbContext.Companies.Include(x => x.CompanyConfiguration)
                                                    .ThenInclude(x => x.MessageProviders)
                                                .Include(c => c.CompanyConfiguration)
                                                    .ThenInclude(x => x.CompanyMessages)
                                                .Include(c => c.CompanySigner1Details)
                                                .FirstOrDefaultAsync(x => x.Id == company.Id);
                if (companyDAO != null)
                {
                    companyDAO.Name = company.Name;
                    companyDAO.ProgramId = company.ProgramId;
                    companyDAO.TransactionId = company.TransactionId;
                    companyDAO.ProgramUtilization = new ProgramUtilizationDAO(company.ProgramUtilization);
                    await _programUtilizationConnector.Update(company.ProgramUtilization);
                    var inputCompanySigner1DetailDAO = new CompanySigner1DetailDAO(company.CompanySigner1Details);
                    if (companyDAO.CompanySigner1Details == null)
                    {
                        companyDAO.CompanySigner1Details = inputCompanySigner1DetailDAO;
                    }
                    else
                    {
                        companyDAO.CompanySigner1Details.Key1 = inputCompanySigner1DetailDAO?.Key1;
                        companyDAO.CompanySigner1Details.Key2 = inputCompanySigner1DetailDAO?.Key2;
                        companyDAO.CompanySigner1Details.ShouldShowInUI = inputCompanySigner1DetailDAO?.ShouldShowInUI ?? false;
                        companyDAO.CompanySigner1Details.ShouldSignAsDefaultValue = inputCompanySigner1DetailDAO?.ShouldSignAsDefaultValue ?? false;
                        companyDAO.CompanySigner1Details.Signer1Endpoint = inputCompanySigner1DetailDAO?.Signer1Endpoint;
                        companyDAO.CompanySigner1Details.Signer1User = inputCompanySigner1DetailDAO?.Signer1User;
                        companyDAO.CompanySigner1Details.Signer1Password = inputCompanySigner1DetailDAO?.Signer1Password;
                    }
                    var inputConfigurationDAO = new CompanyConfigurationDAO(company.CompanyConfiguration);
                    if (companyDAO.CompanyConfiguration == null)
                    {
                        companyDAO.CompanyConfiguration = inputConfigurationDAO;
                    }
                    else
                    {
                        UpdateCompanyMessages(companyDAO, inputConfigurationDAO);
                        UpdateMessageProviders(companyDAO, inputConfigurationDAO);

                        companyDAO.CompanyConfiguration.DeleteSignedDocumentAfterXDays = inputConfigurationDAO.DeleteSignedDocumentAfterXDays;
                        companyDAO.CompanyConfiguration.DeleteUnsignedDocumentAfterXDays = inputConfigurationDAO.DeleteUnsignedDocumentAfterXDays;
                        companyDAO.CompanyConfiguration.ShouldNotifyWhileSignerSigned = inputConfigurationDAO.ShouldNotifyWhileSignerSigned;
                        companyDAO.CompanyConfiguration.ShouldEnableSignReminders = inputConfigurationDAO.ShouldEnableSignReminders;
                        companyDAO.CompanyConfiguration.SignReminderFrequencyInDays = inputConfigurationDAO.SignReminderFrequencyInDays;
                        companyDAO.CompanyConfiguration.CanUserControlReminderSettings = inputConfigurationDAO.CanUserControlReminderSettings;
                        companyDAO.CompanyConfiguration.ShouldSendSignedDocument = inputConfigurationDAO.ShouldSendSignedDocument;
                        companyDAO.CompanyConfiguration.Language = inputConfigurationDAO.Language;
                        companyDAO.CompanyConfiguration.SignatureColor = inputConfigurationDAO.SignatureColor;
                        companyDAO.CompanyConfiguration.SignerLinkExpirationInDays = inputConfigurationDAO.SignerLinkExpirationInDays;
                        companyDAO.CompanyConfiguration.EnableVisualIdentityFlow = inputConfigurationDAO.EnableVisualIdentityFlow;
                        companyDAO.CompanyConfiguration.EnableDisplaySignerNameInSignature = inputConfigurationDAO.EnableDisplaySignerNameInSignature;
                        companyDAO.CompanyConfiguration.ShouldSendWithOTPByDefault = inputConfigurationDAO.ShouldSendWithOTPByDefault;
                        companyDAO.CompanyConfiguration.DefaultSigningType = inputConfigurationDAO.DefaultSigningType;
                        companyDAO.CompanyConfiguration.isPersonalizedPFX = inputConfigurationDAO.isPersonalizedPFX;
                        companyDAO.CompanyConfiguration.ShouldSendDocumentNotifications = inputConfigurationDAO.ShouldSendDocumentNotifications;
                        companyDAO.CompanyConfiguration.DocumentNotificationsEndpoint = inputConfigurationDAO.DocumentNotificationsEndpoint;
                        companyDAO.CompanyConfiguration.ShouldForceOTPInLogin = inputConfigurationDAO.ShouldForceOTPInLogin;
                        companyDAO.CompanyConfiguration.ShouldEnableMeaningOfSignatureOption = inputConfigurationDAO.ShouldEnableMeaningOfSignatureOption;
                        companyDAO.CompanyConfiguration.ShouldEnableVideoConference = inputConfigurationDAO.ShouldEnableVideoConference;
                        companyDAO.CompanyConfiguration.ShouldAddAppendicesAttachmentsToSendMail = inputConfigurationDAO.ShouldAddAppendicesAttachmentsToSendMail;
                        companyDAO.CompanyConfiguration.RecentPasswordsAmount = inputConfigurationDAO.RecentPasswordsAmount;
                        companyDAO.CompanyConfiguration.PasswordExpirationInDays = inputConfigurationDAO.PasswordExpirationInDays;
                        companyDAO.CompanyConfiguration.MinimumPasswordLength = inputConfigurationDAO.MinimumPasswordLength;
                        companyDAO.CompanyConfiguration.EnableTabletsSupport = inputConfigurationDAO.EnableTabletsSupport;
                        //companyDAO.CompanyConfiguration.ShouldEnableGovernmentSignatureFormat = inputConfigurationDAO.ShouldEnableGovernmentSignatureFormat;
                    }
                    _memoryCache.Remove(company.Id);
                }
                _dbContext.Companies.Update(companyDAO);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in CompanyConnector_Update = ");
                throw;
            }
        }
        #region Private Functions
        private void UpdateMessageProviders(CompanyDAO companyDAO, CompanyConfigurationDAO inputConfigurationDAO)
        {
            foreach (var dbMessageProvider in companyDAO.CompanyConfiguration.MessageProviders)
            {

                var input = inputConfigurationDAO.MessageProviders?.FirstOrDefault(x => x.ProviderType == dbMessageProvider.ProviderType);
                if (input != null)
                {
                    dbMessageProvider.From = input.From;
                    dbMessageProvider.Password = input.Password;
                    dbMessageProvider.Port = input.Port;
                    dbMessageProvider.SendingMethod = input.SendingMethod;
                    dbMessageProvider.Server = input.Server;
                    dbMessageProvider.User = input.User;
                    dbMessageProvider.EnableSsl = input.EnableSsl;
                }
                else
                {
                    dbMessageProvider.From = "";
                    dbMessageProvider.Password = "";
                    dbMessageProvider.Port = 0;
                    dbMessageProvider.Server = "";
                    dbMessageProvider.User = "";
                }
            }
            foreach (var provider in inputConfigurationDAO.MessageProviders)
            {
                var savedProvider = companyDAO.CompanyConfiguration.MessageProviders?.FirstOrDefault(x => x.ProviderType == provider.ProviderType);
                if (savedProvider == null)
                {
                    companyDAO.CompanyConfiguration.MessageProviders.Add(provider);
                }
            }
        }

        private void UpdateCompanyMessages(CompanyDAO companyDAO, CompanyConfigurationDAO inputConfigurationDAO)
        {
            foreach (var companyMessage in inputConfigurationDAO.CompanyMessages)
            {
                var dbMessageProvider = companyDAO.CompanyConfiguration.CompanyMessages?.FirstOrDefault(x => x.MessageType == companyMessage.MessageType &&
                (x.Language == companyMessage.Language || (x.Language == 0 && companyMessage.Language == Language.en)));
                if (dbMessageProvider == null)
                {
                    companyDAO.CompanyConfiguration.CompanyMessages.Add(companyMessage);
                    continue;
                }
                dbMessageProvider.SendingMethod = companyMessage.SendingMethod;
                dbMessageProvider.Content = companyMessage.Content;
                dbMessageProvider.Language = companyMessage.Language;
            }
        }
        #endregion
    }
}