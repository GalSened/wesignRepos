using Common.Consts;
using Common.Enums;
using Common.Enums.Companies;
using Common.Enums.Contacts;
using Common.Enums.Documents;
using Common.Enums.Groups;
using Common.Enums.Program;
using Common.Enums.Users;
using Common.Handlers;
using Common.Interfaces;
using Common.Interfaces.DB;
using Common.Interfaces.License;
using Common.Interfaces.ManagementApp;
using Common.Interfaces.MessageSending;
using Common.Models;
using Common.Models.ActiveDirectory;
using Common.Models.Configurations;
using Common.Models.Documents.Signers;
using Common.Models.Programs;
using Common.Models.Settings;
using ManagementBL.CleanDb;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementBL.Handlers
{
    public class JobsHandler : IJobs
    {
        private const string NEW_LINE = "<br>";

        private readonly ILogger _logger;
        private readonly ISendingMessageHandler _sendingMessageHandler;
        private readonly IDater _dater;
        private IMessageSender _sender;
        private readonly ILicense _license;
        private readonly IActiveDirectory _activeDirectory;
        private readonly ICleanDBManager _cleanDBManager;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IPBKDF2 _pkbdf2Handler;
        private readonly ICertificate _certificate;

        private readonly GeneralSettings _generalSettings;
        private readonly ICompanies _companies;
        private readonly IDocumentCollection _documents;
        private readonly IConfiguration _configuration;
        private readonly IDocumentCollectionOperations _documentCollectionOperations;
        private readonly IUserPeriodicReports _userPeriodicReports;
        private readonly IManagementPeriodicReports _managementPeriodicReports;
        private readonly FolderSettings _folderSettings;

        public JobsHandler(ILogger logger, ISendingMessageHandler sendingMessageHandler,
            IDater dater, ILicense license, IActiveDirectory activeDirectory, IOptions<GeneralSettings> generalSettings,
            ICleanDBManager cleanDBManager, IServiceScopeFactory scopeFactory, ICertificate certificate,
            IPBKDF2 pkbdf2Handler, ICompanies companies, IDocumentCollection documents, IConfiguration configuration,
            IDocumentCollectionOperations documentCollectionOperations, IUserPeriodicReports userPeriodicReports,
            IManagementPeriodicReports managementPeriodicReports, IOptions<FolderSettings> folderSettings)
        {

            _logger = logger;
            _sendingMessageHandler = sendingMessageHandler;
            _sender = _sendingMessageHandler.ExecuteCreation(SendingMethod.Email);
            _dater = dater;
            _license = license;
            _certificate = certificate;
            _activeDirectory = activeDirectory;
            _cleanDBManager = cleanDBManager;
            _scopeFactory = scopeFactory;
            _pkbdf2Handler = pkbdf2Handler;
            _generalSettings = generalSettings.Value;
            _companies = companies;
            _documents = documents;
            _configuration = configuration;
            _documentCollectionOperations = documentCollectionOperations;
            _userPeriodicReports = userPeriodicReports;
            _managementPeriodicReports = managementPeriodicReports;
            _folderSettings = folderSettings.Value;
        }


        public async Task CleanDb()
        {
            try
            {
                _logger.Information("Start cleaning DB ...");
                await _cleanDBManager.StartCleanDBProcess();
            }
            catch (Exception ex)
            {

                _logger.Error(ex, "Error in cleaning DB");
                throw;
            }
        }

        public async Task SendProgramExpiredNotification()
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                ICompanyConnector companyConnector = scope.ServiceProvider.GetService<ICompanyConnector>();
                IUserConnector userConnector = scope.ServiceProvider.GetService<IUserConnector>();

                _logger.Information("Start send programs expired for all companies users and free-trail users ...");
                var appConfiguration = await _configuration.ReadAppConfiguration();

                await NotifyFreeTrailUsers(userConnector, appConfiguration);

                var companies = companyConnector.ReadAboutToBeExpired(_dater.UtcNow(), _dater.UtcNow().AddDays(32));
                foreach (var company in companies.Where(x => string.IsNullOrWhiteSpace(x.TransactionId)) ?? Enumerable.Empty<Company>())
                {
                    await NotifyCompanyUsers(userConnector, appConfiguration, company);
                }

                _logger.Information("Start send license is about to expired for all system users...");

                (var weSignLicense, var _) = await _license.GetLicenseInformationAndUsing(false);
                if (IsProgramAboutToExpired(weSignLicense.ExpirationTime))
                {


                    var systemAdminUsers = userConnector.ReadUsersByType(UserType.SystemAdmin);
                    foreach (var user in systemAdminUsers ?? Enumerable.Empty<User>())
                    {
                        await NotifySystemUsers(user, appConfiguration, weSignLicense);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in ResetProgramsUtilization");
            }
        }

        public async Task SendSignReminders()
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                ICompanyConnector companyConnector = scope.ServiceProvider.GetService<ICompanyConnector>();
                _logger.Information("Starting to send sign reminder notifications");
                var companies = companyConnector.ReadWithReminders(Consts.EMPTY, offset: 0, limit: Consts.UNLIMITED, CompanyStatus.Created, out _);
                if (!companies.Any())
                {
                    _logger.Information("No relevant companies in existance, Reminders were not sent");
                    return;
                }


                Dictionary<Guid, CompanyConfiguration> companyConfigurations = new Dictionary<Guid, CompanyConfiguration>();

                foreach (var company in companies)
                {
                    companyConfigurations.Add(company.Id, company.CompanyConfiguration);


                    IDocumentCollectionConnector documentCollectionConnector = scope.ServiceProvider.GetService<IDocumentCollectionConnector>();
                    var collectionFoCompany = await documentCollectionConnector.ReadDocumentsForRemainder(company);

                    if (!collectionFoCompany.Any())
                    {
                        _logger.Information("No relevant documents in existance, Reminders were not sent");
                        continue;
                    }

                    int reminderFrequency = 0;
                    foreach (var documentCollection in collectionFoCompany)
                    {

                        var userCompanyConfiguration = companyConfigurations[documentCollection.User.CompanyId];
                        if (userCompanyConfiguration.CanUserControlReminderSettings)
                        {
                            if (documentCollection.User.UserConfiguration.ShouldNotifySignReminder)
                            {
                                reminderFrequency = documentCollection.User.UserConfiguration.SignReminderFrequencyInDays;
                            }
                            else
                            {
                                continue;
                            }
                        }
                        else
                        {
                            reminderFrequency = userCompanyConfiguration.SignReminderFrequencyInDays;

                        }

                        if (documentCollection.Notifications.ShouldSendDocumentForSigning ?? true)
                        {
                            if (documentCollection.Mode == DocumentMode.GroupSign)
                            {
                                var validSigners = documentCollection.Signers.Where(x => x.Status != SignerStatus.Signed && x.Status != SignerStatus.Rejected);
                                foreach (var signer in validSigners)
                                {
                                    if (signer.TimeLastSent.AddHours(reminderFrequency * 24 - 1) <= _dater.UtcNow())
                                    {
                                        try
                                        {
                                            await SendSignReminderNotification(documentCollection, signer, documentCollection.User, userCompanyConfiguration);
                                        }
                                        catch (Exception ex)
                                        {
                                            _logger.Error(ex, "SendSignReminderNotification has failed, for document: {DocumentCollectionId}, signer: {SignerId}, User: {UserId}", documentCollection.Id, signer.Id, documentCollection.User);
                                        }

                                    }
                                }
                            }
                            else if (documentCollection.Mode == DocumentMode.OrderedGroupSign)
                            {
                                var signer = documentCollection.Signers.FirstOrDefault(x => x.Status != SignerStatus.Signed && x.Status != SignerStatus.Rejected);
                                if (signer == null)
                                {
                                    continue;
                                }
                                if (signer.TimeLastSent.AddHours(reminderFrequency * 24 - 1) <= _dater.UtcNow())
                                {
                                    try
                                    {
                                        await SendSignReminderNotification(documentCollection, signer, documentCollection.User, userCompanyConfiguration);
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.Error(ex, "SendSignReminderNotification has failed, for document: {DocumentCollectionId}, signer: {SignerId}, User: {UserId}", documentCollection.Id, signer.Id, documentCollection.User);
                                    }
                                }
                            }
                        }
                    }
                }
                _logger.Information("Finished sending sign reminder notifications");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in send sign reminders");
                throw;
            }
        }

        public async Task ResetProgramsUtilization()
        {
            try
            {
                _logger.Information("Start reset programsUtilization for all companies...");
                using var scope = _scopeFactory.CreateScope();
                ICompanyConnector companyConnector = scope.ServiceProvider.GetService<ICompanyConnector>();
                IProgramUtilizationConnector programUtilizationConnector = scope.ServiceProvider.GetService<IProgramUtilizationConnector>();
                // update all companies without program utilization 
                programUtilizationConnector.FixUnsetProgramLastResetDate();
                // read only companies that need to be reset

                IEnumerable<Company> companies = companyConnector.ReadForProgramUtilization();
                foreach (var company in companies)
                {
                    if (company.Id != Consts.FREE_ACCOUNTS_COMPANY_ID && company.Id != Consts.GHOST_USERS_COMPANY_ID)
                    {
                        try
                        {
                            var programUtilization = company.ProgramUtilization;

                            if (programUtilization.ProgramResetType == Common.Enums.Program.ProgramResetType.Monthly)
                            {
                                if ((_dater.UtcNow() - programUtilization?.LastResetDate).HasValue &&
                                   (Math.Round((_dater.UtcNow() - programUtilization?.LastResetDate).Value.TotalDays) >= 30)
                                   && Math.Round((_dater.UtcNow() - programUtilization?.LastResetDate).Value.TotalDays) != 0)
                                {
                                    await ResetCompanyProgramToZero(scope, company, programUtilization);
                                }
                            }
                            if (programUtilization.ProgramResetType == Common.Enums.Program.ProgramResetType.Yearly)
                            {
                                if ((_dater.UtcNow() - programUtilization?.LastResetDate).HasValue &&
                                    (Math.Round((_dater.UtcNow() - programUtilization?.LastResetDate).Value.TotalDays) >= 365)
                                    && Math.Round((_dater.UtcNow() - programUtilization?.LastResetDate).Value.TotalDays) != 0)
                                {
                                    await ResetCompanyProgramToZero(scope, company, programUtilization);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(ex, "Failed to Reset ProgramUtilization for company id [{CompanyId}]", company.Id);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in ResetProgramsUtilization");
            }
        }

        private async Task ResetCompanyProgramToZero(IServiceScope scope, Company company, ProgramUtilization programUtilization)
        {
            IProgramUtilizationConnector programUtilizationConnector = scope.ServiceProvider.GetService<IProgramUtilizationConnector>();
            await _companies.AddProgramUtilizationCompanyToHistory(company, ProgramUtilizationHistoryResourceMode.FromRestProgramUtilizationJob);
            programUtilization.DocumentsUsage = 0;
            programUtilization.DocumentsSentNotifyCount = 0;
            programUtilization.SMS = 0;
            programUtilization.SmsSentNotifyCount = 0;
            programUtilization.VisualIdentifications = 0;
            programUtilization.VisualIdentificationUsedNotifyCount = 0;
            programUtilization.VideoConference = 0;
            programUtilization.VideoConferenceUsedNotifyCount = 0;
            programUtilization.VisualIdentificationUsedNotifyCount = 0;
            programUtilization.LastResetDate = _dater.UtcNow();
            await programUtilizationConnector.Update(programUtilization);
            _logger.Debug("Update successfully programsUtilization for company [{CompanyId}]", company.Id);
        }

        public async Task CreateActiveDirectoryUsersAndContacts()
        {
            try
            {
                _logger.Information("Start Create Active Directory Users And Contacts ...");
                (var license, var _) = await _license.GetLicenseInformationAndUsing(false);
                _logger.Debug("license info - should use Active Directory = [{@License}]", license?.LicenseCounters);
                if (license?.LicenseCounters?.UseActiveDirectory ?? false)
                {
                    var activeDirectoryConfig = await _activeDirectory.Read();
                    if (string.IsNullOrWhiteSpace(activeDirectoryConfig.Domain) || string.IsNullOrWhiteSpace(activeDirectoryConfig.Host))
                    {
                        return;
                    }
                    using var scope = _scopeFactory.CreateScope();
                    ICompanyConnector companyConnector = scope.ServiceProvider.GetService<ICompanyConnector>();
                    IGroupConnector groupConnector = scope.ServiceProvider.GetService<IGroupConnector>();
                    IActiveDirectoryGroupsConnector activeDirectoryGroupsConnector = scope.ServiceProvider.GetService<IActiveDirectoryGroupsConnector>();
                    IContactConnector contactConnector = scope.ServiceProvider.GetService<IContactConnector>();
                    IUserConnector userConnector = scope.ServiceProvider.GetService<IUserConnector>();
                    IEnumerable<Company> companies = companyConnector.Read(null, 0, -1, CompanyStatus.Created, out int totalCount)
                    .Where(x => x.Id != Consts.FREE_ACCOUNTS_COMPANY_ID &&
                    x.Status != CompanyStatus.Deleted);

                    foreach (var company in companies)
                    {
                        var groups = groupConnector.Read(company).Where(g => g.GroupStatus != GroupStatus.Deleted);
                        var activeDerictoryMapping = await activeDirectoryGroupsConnector.GetAllGroupsForCompany(company);
                        foreach (var group in groups)
                        {

                            try
                            {
                                await HandleGroupsCreateRemoveADUsers(userConnector, company, group, activeDerictoryMapping);
                                await HandleGroupsCreateRemoveADContacts(contactConnector, userConnector, companyConnector, groupConnector, group, activeDerictoryMapping);
                            }
                            catch (Exception ex)
                            {
                                _logger.Error(ex, "Error in process AD synchronization for group {GroupName} at company {CompanyName}", group.Name, company.Name);
                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in Create Active Directory Users And Contacts");
                //throw;
            }
        }

        public async Task DeleteLogsFromDB()
        {
            try
            {
                var appConfiguration = await _configuration.ReadAppConfiguration();
                if (appConfiguration.LogArichveIntervalInDays != Consts.NEVER)
                {
                    DateTime from = _dater.UtcNow().AddDays(-appConfiguration.LogArichveIntervalInDays);
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        try
                        {
                            ILogConnector dependencyService = scope.ServiceProvider.GetService<ILogConnector>();
                            dependencyService.Delete(from, Common.Enums.Logs.LogApplicationSource.SignerApp);
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(ex, "Clear signer app logs from DB error ");
                        }
                        try
                        {
                            ILogConnector dependencyService = scope.ServiceProvider.GetService<ILogConnector>();
                            dependencyService.Delete(from, Common.Enums.Logs.LogApplicationSource.ManagementApp);
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(ex, "Clear Management app logs from DB error ");
                        }
                        try
                        {
                            ILogConnector dependencyService = scope.ServiceProvider.GetService<ILogConnector>();
                            dependencyService.Delete(from, Common.Enums.Logs.LogApplicationSource.UserApp);
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(ex, "Clear User app logs from DB error ");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in DeleteLogs from DB - ");
            }

        }


        public async Task CleanUnusedTemplatesAndContacts()
        {
            _logger.Debug("Start clean unused temples and contacts");
            using var scope = _scopeFactory.CreateScope();
            ITemplateConnector templateConnector = scope.ServiceProvider.GetService<ITemplateConnector>();
            IContactConnector contactConnector = scope.ServiceProvider.GetService<IContactConnector>();

            await templateConnector.DeleteUnusedTemplates(_dater.UtcNow().AddMonths(-13));

            List<Contact> contacts = contactConnector.GetUnusedContacts(_dater.UtcNow().AddMonths(-13));
            await contactConnector.DeleteRange(contacts);

            _logger.Debug("done process clean unused temples and contacts");


        }

        public async Task SendProgramCapacityIsAboutToExpiredNotification()
        {
            try
            {
                _logger.Information("Start send program capacity is about to expired notification for all companies...");
                var appConfiguration = await _configuration.ReadAppConfiguration();
                int skip = 0;
                int totalCount = 0;
                int batchSize = 250;
                using var scope = _scopeFactory.CreateScope();
                ICompanyConnector companyConnector = scope.ServiceProvider.GetService<ICompanyConnector>();
                IProgramConnector programConnector = scope.ServiceProvider.GetService<IProgramConnector>();
                IProgramUtilizationConnector programUtilizationConnector = scope.ServiceProvider.GetService<IProgramUtilizationConnector>();
                IUserConnector userConnector = scope.ServiceProvider.GetService<IUserConnector>();
                do
                {
                    var companies = companyConnector.Read(string.Empty, skip, batchSize, CompanyStatus.Created, out totalCount);



                    Dictionary<Guid, Program> programs = new Dictionary<Guid, Program>();
                    foreach (var company in companies)
                    {
                        if (company.Id != Consts.FREE_ACCOUNTS_COMPANY_ID && company.Id != Consts.GHOST_USERS_COMPANY_ID)
                        {
                            try
                            {

                                Program program = null;
                                if (programs.ContainsKey(company.ProgramId))
                                {
                                    program = programs[company.ProgramId];
                                }
                                else
                                {
                                    program = await programConnector.Read(new Program { Id = company.ProgramId });
                                    programs.Add(company.ProgramId, program);

                                }

                                await SendSmsCapacityIsAboutToExpiredNotification(programUtilizationConnector, userConnector, company, company.ProgramUtilization, program, appConfiguration);
                                await SendDocumentsCapacityIsAboutToExpiredNotification(programUtilizationConnector, userConnector, company, company.ProgramUtilization, program, appConfiguration);
                                if (company.CompanyConfiguration.EnableVisualIdentityFlow)
                                {
                                    await SendVisualIdentificationsCapacityIsAboutToExpireNotification(programUtilizationConnector, userConnector, company, company.ProgramUtilization, program, appConfiguration);
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.Error(ex, "Failed to send program capacity is about to expired notification for company id [{CompanyId}]", company.Id);
                            }
                        }
                    }
                    skip += batchSize;
                } while (skip <= totalCount);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in SendProgramCapacityIsAboutToExpiredNotification");
            }
        }

        public async Task<int> SendDocumentIsAboutToBeDeletedNotification()
        {
            try
            {
                int totalEmailsSent = 0;
                var appConfiguration = await _configuration.ReadAppConfiguration();
                int skip = 0;
                int totalCount = 0;
                int batchSize = 250;
                using var scope = _scopeFactory.CreateScope();
                ICompanyConnector companyConnector = scope.ServiceProvider.GetService<ICompanyConnector>();
                do
                {
                    var companies = companyConnector.Read(string.Empty, skip, batchSize, CompanyStatus.Created, out totalCount).ToList();

                    int configDeletionByDays = 0;
                    if (appConfiguration?.DocumentDeletionConfiguration?.DeleteUnsignedDocumentAfterXDays >= 3)
                    {
                        configDeletionByDays = appConfiguration.DocumentDeletionConfiguration.DeleteUnsignedDocumentAfterXDays;
                    }


                    foreach (var company in companies)
                    {
                        var notifyBefore = GetShouldByNotifySinceDataTime(company, configDeletionByDays, out int companyDeletionByDays, notifyWhenXDaysLeft: 2);
                        if (companyDeletionByDays < 3)
                        {
                            _logger.Information("Company: {CompanyName} doesn't want to delete its docuemnts", company.Name);
                            break;
                        }
                        var companyDocuments = await _documents.ReadByStatusAndDate(company, notifyBefore, readCompany: false);

                        if (companyDocuments == null || !companyDocuments.Any())
                        {
                            _logger.Information("no documents to the company: {CompanyName}", company.Name);
                        }
                        else
                        {
                            (var emailDocumentsDic, var users) = CreateEmailDocumentsDictionary(companyDocuments);

                            foreach (var email in emailDocumentsDic.Keys)
                            {
                                var user = users.FirstOrDefault(x => x.Email == email);
                                await SendDocumentIsAboutToBeDeletedMessage(appConfiguration, company, user, emailDocumentsDic[email], companyDeletionByDays);
                            }
                            totalEmailsSent += emailDocumentsDic.Keys.Count;
                            _logger.Information("Finish send document is about to deleted notification to users in the company: {CompanyName}", company.Name);
                        }
                    }
                    skip += batchSize;
                } while (skip <= totalCount);

                return totalEmailsSent;

            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in Send Document Is about To Be Deleted Notification");
                return -1;
                throw;
            }
        }
        public async Task SendUserPeriodicReports()
        {
            _logger.Information("Start send user periodic reports...");
            using var scope = _scopeFactory.CreateScope();
            IConfigurationConnector configurationConnector = scope.ServiceProvider.GetService<IConfigurationConnector>();
            IUserPeriodicReportConnector userPeriodicReportConnector = scope.ServiceProvider.GetService<IUserPeriodicReportConnector>();
            var appConfiguration = await configurationConnector.Read();
            var userReports = userPeriodicReportConnector.ReadReportsToSend().ToList();
            _logger.Information("Total user reports to send: {ReportsCount}", userReports.Count);
            if (userReports != null)
            {
                foreach (var report in userReports)
                {
                    await _userPeriodicReports.SendReportToUser(appConfiguration, report);
                    userPeriodicReportConnector.UpdateReportSendTime(report);
                }
            }
            _logger.Information("End send user periodic reports");
        }

        public async Task SendManagementPeriodicReports()
        {
            using var scope = _scopeFactory.CreateScope();
            IConfigurationConnector configurationConnector = scope.ServiceProvider.GetService<IConfigurationConnector>();
            IManagementPeriodicReportConnector managementPeriodicReportConnector = scope.ServiceProvider.GetService<IManagementPeriodicReportConnector>();
            IManagementPeriodicReportEmailConnector managementPeriodicReportEmailConnector = scope.ServiceProvider.GetService<IManagementPeriodicReportEmailConnector>();
            var appConfiguration = await configurationConnector.Read();
            var managementReports = managementPeriodicReportConnector.ReadReportsToSend().ToList();
            if (managementReports != null)
            {
                foreach (var report in managementReports)
                {
                    var emailManagementReports = managementPeriodicReportEmailConnector.ReadAllByReportId(report.Id);
                    if (emailManagementReports != null)
                    {
                        report.Emails = emailManagementReports.ToList();
                        await _managementPeriodicReports.SendManagementReportToUsers(appConfiguration, report, emailManagementReports);
                        managementPeriodicReportConnector.UpdateReportSendTime(report);
                    }
                }
            }
        }

        public async Task DeleteExpiredPeriodicReportFiles()
        {
            using var scope = _scopeFactory.CreateScope();
            IPeriodicReportFileConnector periodicReportFileConnector = scope.ServiceProvider.GetService<IPeriodicReportFileConnector>();
            var idsToDelete = periodicReportFileConnector.DeleteAllExpired().ToList();
            if (idsToDelete != null && idsToDelete.Any() && Directory.Exists(_folderSettings.PeriodicReports))
            {
                foreach (var id in idsToDelete)
                {
                    var subFolderRoute = $"{_folderSettings.PeriodicReports}\\{id}";
                    if (Directory.Exists(subFolderRoute))
                    {
                        Directory.Delete(subFolderRoute, true);
                    }
                }
            }
        }

        public async Task UpdateExpiredSignerTokens()
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                IDocumentCollectionConnector documentCollectionConnector = scope.ServiceProvider.GetService<IDocumentCollectionConnector>();
                var unsignedDocs = documentCollectionConnector.ReadUnsigned();
                if (unsignedDocs != null && unsignedDocs.Any())
                {
                    ISignersConnector signersConnector = scope.ServiceProvider.GetService<ISignersConnector>();
                    IJWT jwtHandler = scope.ServiceProvider.GetService<IJWT>();
                    IConfiguration configHandler = scope.ServiceProvider.GetService<IConfiguration>();
                    IUserConnector userConnector = scope.ServiceProvider.GetService<IUserConnector>();
                    ICompanyConnector companyConnector = scope.ServiceProvider.GetService<ICompanyConnector>();
                    ISignerTokenMappingConnector signerTokenMappingConnector = scope.ServiceProvider.GetService<ISignerTokenMappingConnector>();
                    foreach (var docCollection in unsignedDocs)
                    {
                        var pendingSigners = signersConnector.ReadPendingSigners(docCollection.Id);
                        if (pendingSigners != null && pendingSigners.Any())
                        {
                            foreach (var pendingSigner in pendingSigners)
                            {
                                var signerTokenMapping = new SignerTokenMapping()
                                {
                                    SignerId = pendingSigner.Id
                                };
                                var signerTokenMappingDb = await signerTokenMappingConnector.Read(signerTokenMapping);
                                if (signerTokenMappingDb != null)
                                {
                                    var isTokenExpired = jwtHandler.IsTokenExpired(signerTokenMappingDb.JwtToken);
                                    if (isTokenExpired)
                                    {
                                        _logger.Information("Signer token for signer {SignerId} is expired, updating the token", pendingSigner.Id);
                                        await GenerateNewToken(docCollection, pendingSigner, signerTokenMappingDb, userConnector, companyConnector,
                                            configHandler, jwtHandler, signerTokenMappingConnector);
                                    }
                                }
                                else
                                {
                                    _logger.Information("Signer token for signer {SignerId} is not exists, creating new token", pendingSigner.Id);
                                    signerTokenMappingDb = new SignerTokenMapping()
                                    {
                                        DocumentCollectionId = docCollection.Id,
                                        SignerId = pendingSigner.Id,
                                        GuidToken = Guid.NewGuid(),
                                    };
                                    await GenerateNewToken(docCollection, pendingSigner, signerTokenMappingDb, userConnector, companyConnector,
                                        configHandler, jwtHandler, signerTokenMappingConnector, true);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in  = JobsHandler_UpdateExpiredSignerTokens");
                throw;
            }
        }

        #region Private jobs

        private async Task GenerateNewToken(DocumentCollection documentCollection, Signer signer, SignerTokenMapping signerTokenMapping, IUserConnector userConnector, 
            ICompanyConnector companyConnector, IConfiguration configuration, IJWT jwt, ISignerTokenMappingConnector signerTokenMappingConnector, bool shouldCreateNewToken = false) 
        {
            var user = await userConnector.Read(new User { Id = documentCollection.UserId });
            var companyConfig = await companyConnector.ReadConfiguration(new Company() { Id = user.CompanyId });
            var expiredLinkInHours = configuration.GetSignerLinkExperationTimeInHours(user, companyConfig);
            var newToken = jwt.GenerateSignerToken(signer, expiredLinkInHours);
            signerTokenMapping.JwtToken = newToken;
            if (shouldCreateNewToken)
            {
                await signerTokenMappingConnector.Create(signerTokenMapping);
            }
            else
            {
                await signerTokenMappingConnector.Update(signerTokenMapping);
            }
        }

        /// <summary>
        /// Load/Create contacts from AD group, add new contact while there is new user in AD group.
        /// </summary>
        private async Task HandleGroupsCreateRemoveADContacts(IContactConnector contactConnector, IUserConnector userConnector,
            ICompanyConnector companyConnector, IGroupConnector groupConnector, Group group, IEnumerable<ActiveDirectoryGroup> activeDerictoryMappingList)
        {


            var activeDerictoryMapping = activeDerictoryMappingList.FirstOrDefault(x => x.GroupId == group.Id);
            if (activeDerictoryMapping == null || string.IsNullOrWhiteSpace(activeDerictoryMapping.ActiveDirectoryContactsGroupName))
            {
                await DeleteAllADContactsFromGroup(contactConnector, group);
            }
            else
            {
                (IEnumerable<Comda.Authentication.Models.User> adUsers, bool isSuccess) = await GetADUserByActiveDirecrotyGroupName(activeDerictoryMapping.ActiveDirectoryContactsGroupName);
                if (!isSuccess)
                {
                    Log.Warning("Failed to fetch data from AD - add/remove contacts process will not continue for now ");
                    return;
                }
                var weSignContacts = GetAllContactsCreatedFromADInGroup(contactConnector, group);

                var contactsToRemove = weSignContacts.Where(p => adUsers.All(p2 => p2.Email.ToLower() != p.Email.ToLower()));






                foreach (var contactToRemove in contactsToRemove)
                {
                    await contactConnector.Delete(contactToRemove);
                }


                var newContactsToCreate = adUsers.Where(p => weSignContacts.All(p2 => p2.Email.ToLower() != p.Email.ToLower()));
                foreach (var newContact in newContactsToCreate)
                {
                    await CreateNewWSNewContactsFromAD(userConnector, groupConnector, companyConnector, contactConnector, group, newContact);


                }

            }


        }

        private async Task CreateNewWSNewContactsFromAD(IUserConnector userConnector, IGroupConnector groupConnector,
            ICompanyConnector companyConnector, IContactConnector contactConnector, Group group, Comda.Authentication.Models.User newContact)
        {

            var users = userConnector.GetAllUsersInGroup(group);
            if (users.Any())
            {
                // need user To add Contact WHY???? what with group contacts?


                Contact contact = new Contact()
                {
                    UserId = users.First().Id,
                    Email = newContact.Email,
                    Name = $"{newContact.FirstName} {newContact.LastName}",
                    Status = ContactStatus.Activated,
                    DefaultSendingMethod = SendingMethod.Email,
                    GroupId = group.Id,
                    CreationSource = CreationSource.ActiveDirectory,
                    Phone = (newContact as Comda.Authentication.Models.ActiveDirectoryUser)?.PhoneNumber ?? "",

                };
                Group contactGroup = await groupConnector.Read(new Group() { Id = contact.GroupId });
                Company contactCompany = await companyConnector.Read(new Company() { Id = contactGroup.CompanyId });
                try
                {
                    await contactConnector.Create(contact);
                    _certificate.Create(contact, contactCompany.CompanyConfiguration);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Failed while trying to add new contact {NewContactEmail} In Active Directory auto add contacts ", newContact.Email);
                }
            }

        }

        private async Task DeleteAllADContactsFromGroup(IContactConnector contactConnector, Group group)
        {
            var contacts = GetAllContactsCreatedFromADInGroup(contactConnector, group);
            foreach (var contact in contacts)
            {
                await contactConnector.Delete(contact);
            }

        }

        private IEnumerable<Contact> GetAllContactsCreatedFromADInGroup(IContactConnector contactConnector, Group group)
        {
            return contactConnector.ReadAllContactInGroup(group).Where(x => x.CreationSource == CreationSource.ActiveDirectory);
        }

        private async Task HandleGroupsCreateRemoveADUsers(IUserConnector userConnector, Company company, Group group, IEnumerable<ActiveDirectoryGroup> activeDerictoryMappingList)
        {
            var activeDerictoryMapping = activeDerictoryMappingList.FirstOrDefault(x => x.GroupId == group.Id);
            if (activeDerictoryMapping == null || string.IsNullOrWhiteSpace(activeDerictoryMapping.ActiveDirectoryUsersGroupName))
            {
                _logger.Warning("Group {GroupId}: {GroupName} new found in Mapping list - remove all users from the group that created automatically using the AD synchronization process", group.Id, group.Name);
                await DeleteAllADUsersFromGroup(userConnector, group);
            }
            else
            {
                (IEnumerable<Comda.Authentication.Models.User> adUsers, bool isSuccess) = await GetADUserByActiveDirecrotyGroupName(activeDerictoryMapping.ActiveDirectoryUsersGroupName);
                if (!isSuccess)
                {
                    Log.Warning("Failed to fetch data from AD - add/remove users process will not continue for now ");
                    return;
                }


                var weSignUsers = GetAllUsersCreatedFromADInGroup(userConnector, group);

                var usersToRemove = weSignUsers.Where(p => adUsers.All(p2 => p2.Email.ToLower() != p.Email.ToLower())).ToList();

                using (var scope = _scopeFactory.CreateScope())
                {
                    IUserConnector dependencyService = scope.ServiceProvider.GetService<IUserConnector>();
                    foreach (var userToRemove in usersToRemove)
                    {
                        _logger.Warning("User {UserMail} not found at the AD group name {ADGroupName} and will be deleted from WS group {WSGroupName}",
                            userToRemove.Email, activeDerictoryMapping.ActiveDirectoryContactsGroupName, group.Name);
                        await dependencyService.Delete(userToRemove);
                    }
                }

                using (var scope = _scopeFactory.CreateScope())
                {
                    var newUsersToCreate = adUsers.Where(p => weSignUsers.All(p2 => p2.Email.ToLower() != p.Email.ToLower())).ToList();
                    foreach (var newUser in newUsersToCreate)
                    {
                        IUserConnector dependencyService = scope.ServiceProvider.GetService<IUserConnector>();
                        _logger.Warning("new User {UserMail} mot found at the AD group name {ADGroupName} and will be added to WS group {WSGroupName}",
                            newUser.Email, activeDerictoryMapping.ActiveDirectoryContactsGroupName, group.Name);
                        await CreateNewWSNewUserFromAD(dependencyService, company, group, newUser as Comda.Authentication.Models.ActiveDirectoryUser);

                    }
                }

            }

        }

        private async Task<(IEnumerable<Comda.Authentication.Models.User>, bool)> GetADUserByActiveDirecrotyGroupName(string adGroupName)
        {
            bool isSuccess;
            _logger.Debug("GetADUserByActiveDirecrotyGroupName for group name =[{ADGroupName}]", adGroupName);
            var activeDirectoryConfig = await _activeDirectory.Read();
            _logger.Debug("activeDirectoryConfig =[{@ActiveDirectoryConfig}]", activeDirectoryConfig);
            IEnumerable<Comda.Authentication.Models.User> adUsers = null;
            (adUsers, isSuccess) = await _activeDirectory.ReadAllUsersFromActiveDirectoryByGroupName(activeDirectoryConfig, adGroupName);

            _logger.Debug("Return {UserCount} users from AD for group {ADGroupName}", adUsers.Count(), adGroupName);
            return (adUsers, isSuccess);
        }

        private async Task CreateNewWSNewUserFromAD(IUserConnector userConnector, Company company, Group group, Comda.Authentication.Models.User newUser)
        {

            if (!string.IsNullOrWhiteSpace(newUser.Email) &&
                !await userConnector.Exists(new User() { Email = newUser.Email }))
            {

                User user = new User()
                {
                    CompanyId = company.Id,
                    CreationTime = _dater.UtcNow(),
                    Name = $"{newUser.FirstName} {newUser.LastName}",
                    GroupId = group.Id,
                    Email = newUser.Email,
                    Password = _pkbdf2Handler.Generate(Guid.NewGuid().ToString()),
                    CreationSource = CreationSource.ActiveDirectory,
                    Status = UserStatus.Activated,
                    Type = UserType.Editor,
                };
                await userConnector.Create(user);

            }
        }

        private IEnumerable<User> GetAllUsersCreatedFromADInGroup(IUserConnector userConnector, Group group)
        {
            return userConnector.GetAllUsersInGroup(group).Where(x => x.CreationSource == CreationSource.ActiveDirectory).ToList();
        }

        private async Task DeleteAllADUsersFromGroup(IUserConnector userConnector, Group group)
        {
            var users = GetAllUsersCreatedFromADInGroup(userConnector, group);


            if (!users.Any())
            {
                return;
            }
            _logger.Debug("found {UserCount} users in group {GroupName}", users.Count(), group.Name);

            using (var scope = _scopeFactory.CreateScope())
            {
                IUserConnector dependencyService = scope.ServiceProvider.GetService<IUserConnector>();
                foreach (var user in users)
                {
                    _logger.Warning("user {UserEmail} removed in ad sync groups", user.Email);
                    await dependencyService.Delete(user);
                }

            }
        }

        private async Task NotifySystemUsers(User user, Configuration appConfiguration, IWeSignLicense weSignLicense)
        {
            try
            {
                if (IsProgramAboutToExpired(weSignLicense.ExpirationTime))
                {
                    await SendProgramExpiredMail(appConfiguration, null, weSignLicense.ExpirationTime, user);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "SendProgramExpiredNotification | Failed to notify user [{UserEmail}]", user.Email);
            }
        }

        private async Task NotifyCompanyUsers(IUserConnector userConnector, Configuration appConfiguration, Company company)
        {

            if (string.IsNullOrWhiteSpace(company.TransactionId))
            {

                if (IsProgramAboutToExpired(company.ProgramUtilization.Expired))
                {
                    var users = userConnector.Read(Consts.EMPTY, 0, Consts.UNLIMITED, UserStatus.Activated, out _, company.Id).Where(x => x.Type == UserType.CompanyAdmin);
                    foreach (var user in users ?? Enumerable.Empty<Common.Models.User>())
                    {
                        try
                        {
                            await SendProgramExpiredMail(appConfiguration, company, company.ProgramUtilization.Expired, user);
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(ex, "SendProgramExpiredNotification | Failed to notify user [{UserEmail}]", user.Email);
                        }
                    }
                }
            }
        }

        private async Task NotifyFreeTrailUsers(IUserConnector userConnector, Configuration appConfiguration)
        {
            var users = userConnector.Read(Consts.EMPTY, 0, Consts.UNLIMITED, UserStatus.Activated, out _, Consts.FREE_ACCOUNTS_COMPANY_ID)
                                    .Where(x => x.Id != Consts.SYSTEM_ADMIN_ID && x.Id != Consts.PAYMENT_ADMIN_ID && x.Id != Consts.GHOST_USER_ID
                                    && x.Id != Consts.DEV_USER_ADMIN_ID);

            foreach (var user in users ?? Enumerable.Empty<User>())
            {
                try
                {
                    if (IsProgramAboutToExpired(user.ProgramUtilization.Expired))
                    {
                        await SendProgramExpiredMail(appConfiguration, null, user.ProgramUtilization.Expired, user);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "SendProgramExpiredNotification | Failed to notify user [{UserEmail}]", user.Email);
                }
            }


        }

        private async Task SendProgramExpiredMail(Configuration appConfiguration, Company company, DateTime expired, User user)
        {
            var messageInfo = new MessageInfo()
            {
                MessageType = MessageType.ProgramIsAboutToExipred,
                User = user,
                MessageContent = expired.Subtract(_dater.UtcNow()).Days.ToString()
            };
            await _sender.Send(appConfiguration, company?.CompanyConfiguration, messageInfo);
            _logger.Debug("Successfully SendProgramExpiredNotification to user [{UserEmail}]", user.Email);
        }

        private async Task SendDocumentIsAboutToBeDeletedMessage(Configuration appConfiguration, Company company, User user, IEnumerable<DocumentCollection> documentCollections, int daysTillDelete)
        {
            var messageContent = _configuration.GetDocumentIsAboutToBeDeletedMessage(user?.UserConfiguration?.Language ?? company.CompanyConfiguration.Language);
            var messageInfo = new MessageInfo()
            {
                MessageType = MessageType.UnsignedDocumentIsAboutToBeDeleted,
                User = user,
                Link = _generalSettings.UserFronendApplicationRoute,
                MessageContent = $"{messageContent} {NEW_LINE} {GetUnsingedDocumentCollectionList(documentCollections, daysTillDelete)}"

            };

            if (_sender == null)
                _sender = _sendingMessageHandler.ExecuteCreation(SendingMethod.Email);
            await _sender.Send(appConfiguration, company.CompanyConfiguration, messageInfo);
            _logger.Debug("Successfully send UnsignedDocumentIsAboutToBeDeleted to user [{UserEmail}]", user.Email);
        }
        private string GetUnsingedDocumentCollectionList(IEnumerable<DocumentCollection> documentCollections, int daysTillDelete)
        {
            StringBuilder result = new StringBuilder();
            var documentCollectionsList = documentCollections.ToList();
            for (int i = 0; i < documentCollectionsList.Count; i++)
            {
                var deletionDate = documentCollectionsList[i]?.CreationTime.AddDays(daysTillDelete);
                result.Append($"{i + 1}. {documentCollectionsList[i]?.Name} ({deletionDate}) {NEW_LINE}");
                result.AppendLine();
            }
            return result.ToString();
        }
        private (Dictionary<string, List<DocumentCollection>>, HashSet<User>) CreateEmailDocumentsDictionary(IEnumerable<DocumentCollection> documentCollections)
        {
            Dictionary<string, List<DocumentCollection>> emailDocumentsDic = new Dictionary<string, List<DocumentCollection>>();
            HashSet<User> users = new HashSet<User>();

            foreach (var dc in documentCollections)
            {
                if (!emailDocumentsDic.ContainsKey(dc.User.Email))
                {
                    emailDocumentsDic.Add(dc.User.Email, new List<DocumentCollection>());
                    users.Add(dc.User);
                }
                emailDocumentsDic[dc.User.Email].Add(dc);
            }
            return (emailDocumentsDic, users);
        }
        private DateTime GetShouldByNotifySinceDataTime(Company company, int configDeletionByDays, out int companyDeletionByDays, int notifyWhenXDaysLeft = 2)
        {
            companyDeletionByDays = configDeletionByDays;
            if (company?.CompanyConfiguration?.DocumentDeletionConfiguration?.DeleteUnsignedDocumentAfterXDays == Consts.UNLIMITED)
            {
                companyDeletionByDays = Consts.UNLIMITED;
                return DateTime.MaxValue;
            }
            else if (company?.CompanyConfiguration?.DocumentDeletionConfiguration?.DeleteUnsignedDocumentAfterXDays > 3)
            {
                companyDeletionByDays = company.CompanyConfiguration.DocumentDeletionConfiguration.DeleteUnsignedDocumentAfterXDays;

            }
            var now = _dater.UtcNow();
            int totalHoursToNotify = notifyWhenXDaysLeft * 24;
            int companyDeletionByHours = companyDeletionByDays * 24;
            int hoursToAddOrRemove = totalHoursToNotify - companyDeletionByHours;

            DateTime notifyEachDocumentBeforeThisDateTime;
            if (hoursToAddOrRemove > 0)
                notifyEachDocumentBeforeThisDateTime = now.AddHours(hoursToAddOrRemove);
            else
                notifyEachDocumentBeforeThisDateTime = now.Subtract(new TimeSpan(Math.Abs(hoursToAddOrRemove), 0, 0));

            return notifyEachDocumentBeforeThisDateTime;
        }

        private async Task SendProgramCapacityIsAboutToExpiredNotificationMail(IUserConnector userConnector, Configuration appConfiguration, Company company, int overXPercentage, ProgramCapacityType programCapacityType)
        {
            var companyAdminUsers = userConnector.ReadAdminUsersInCompany(company);

            foreach (var companyAdminUser in companyAdminUsers)
            {
                var messageInfo = new MessageInfo()
                {
                    MessageType = MessageType.ProgramCapacityIsAboutToExpired,
                    User = companyAdminUser,
                    MessageContent = $"{programCapacityType};{overXPercentage}",
                };
                await _sender.Send(appConfiguration, company?.CompanyConfiguration, messageInfo);
                _logger.Debug("Successfully SendProgramCapacityIsAboutToExpiredNotificationMail to user [{CompanyAdminEmail}]", companyAdminUser.Email);
            }
        }

        private bool IsProgramAboutToExpired(DateTime expired)
        {
            List<int> daysBeforeNotification = new List<int> { 30, 15, 7, 3, 1 };
            int daysBeforeProgramExipred = expired.Date.Subtract(_dater.UtcNow().Date).Days;

            return daysBeforeNotification.Contains(daysBeforeProgramExipred);
        }

        private async Task SendSmsCapacityIsAboutToExpiredNotification(IProgramUtilizationConnector programUtilizationConnector, IUserConnector userConnector, Company company, ProgramUtilization programUtilization, Program program, Configuration appConfiguration)
        {
            if (program.SmsPerMonth != Consts.UNLIMITED)
            {
                double div = (double)programUtilization.SMS / (double)program.SmsPerMonth;
                var percentages = _generalSettings.IsAboutToExpiredNotificationInPercentage.OrderBy(x => x.OverXPercentage).ToList();

                int notificationLimit = percentages.Count;
                for (int i = 0; i < notificationLimit; i++)
                {
                    bool smsCapacityIsAboutToExipred = div > ((double)percentages.ElementAtOrDefault(i)?.OverXPercentage / (double)100);
                    if (smsCapacityIsAboutToExipred && i == programUtilization.SmsSentNotifyCount)
                    {
                        await SendProgramCapacityIsAboutToExpiredNotificationMail(userConnector, appConfiguration, company, percentages.ElementAt(i).OverXPercentage, ProgramCapacityType.SMS);
                        programUtilization.SmsSentNotifyCount++;
                        await programUtilizationConnector.Update(programUtilization);

                    }
                }
            }
        }

        private async Task SendDocumentsCapacityIsAboutToExpiredNotification(IProgramUtilizationConnector programUtilizationConnector, IUserConnector userConnector, Company company, ProgramUtilization programUtilization, Program program, Configuration appConfiguration)
        {
            if (program.DocumentsPerMonth != Consts.UNLIMITED)
            {
                double div = (double)programUtilization.DocumentsUsage / (double)program.DocumentsPerMonth;
                var percentages = _generalSettings.IsAboutToExpiredNotificationInPercentage.OrderBy(x => x.OverXPercentage).ToList();
                int notificationLimit = percentages.Count;
                for (int i = 0; i < notificationLimit; i++)
                {
                    bool documentsCapacityIsAboutToExipred = div > ((double)percentages.ElementAtOrDefault(i)?.OverXPercentage / (double)100);
                    if (documentsCapacityIsAboutToExipred && i == programUtilization.DocumentsSentNotifyCount)
                    {
                        await SendProgramCapacityIsAboutToExpiredNotificationMail(userConnector, appConfiguration, company, percentages.ElementAt(i).OverXPercentage, ProgramCapacityType.Documents);
                        programUtilization.DocumentsSentNotifyCount++;
                        await programUtilizationConnector.Update(programUtilization);
                    }
                }
            }
        }

        private async Task SendVisualIdentificationsCapacityIsAboutToExpireNotification(IProgramUtilizationConnector programUtilizationConnector, IUserConnector userConnector,
            Company company, ProgramUtilization programUtilization, Program program, Configuration appConfiguration)
        {
            if (program.VisualIdentificationsPerMonth != Consts.UNLIMITED)
            {
                double div = (double)programUtilization.VisualIdentifications / (double)program.VisualIdentificationsPerMonth;
                var percentages = _generalSettings.IsAboutToExpiredNotificationInPercentage.OrderBy(x => x.OverXPercentage).ToList();
                int notificationLimit = percentages.Count;
                for (int i = 0; i < notificationLimit; i++)
                {
                    bool visualIdentificationsCapacityIsAboutToExipred = div > ((double)percentages.ElementAtOrDefault(i)?.OverXPercentage / (double)100);
                    if (visualIdentificationsCapacityIsAboutToExipred && i == programUtilization.VisualIdentificationUsedNotifyCount)
                    {
                        await SendProgramCapacityIsAboutToExpiredNotificationMail(userConnector, appConfiguration, company, percentages.ElementAt(i).OverXPercentage, ProgramCapacityType.VisualIdentification);
                        programUtilization.VisualIdentificationUsedNotifyCount++;
                        await programUtilizationConnector.Update(programUtilization);
                    }
                }
            }
        }

        private async Task SendSignReminderNotification(DocumentCollection documentCollection, Signer signer, User user, CompanyConfiguration companyConfiguration)
        {
            await _documentCollectionOperations.SendLinkToSpecificSigner(documentCollection, signer, user, companyConfiguration, true, MessageType.SignReminder, false);
            _logger.Information("Sent Notification From {UserName} UserId: {UserId} to {SignerContactName} signerId {SignerId} via Sending Method: {SendingMethod}"
                , user.Name, user.Id, signer.Contact.Name, signer.Id, signer.SendingMethod);
        }
        #endregion
    }
}
