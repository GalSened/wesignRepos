

// Ignore Spelling: app

namespace Common.Handlers
{
    using Common.Enums;
    using Common.Enums.Documents;
    using Common.Enums.Results;
    using Common.Enums.Users;
    using Common.Extensions;
    using Common.Interfaces;
    using Common.Interfaces.DB;
    using Common.Interfaces.MessageSending.Sms;
    using Common.Models;
    using Common.Models.Configurations;
    using Common.Models.Documents;
    using Common.Models.Settings;
    using Microsoft.Extensions.Options;
    using System;
    using System.Linq;    
    using Common.Consts;
    using System.Collections.Generic;    
    using Common.Interfaces.Files;    
    using System.Threading.Tasks;
    using CTInterfaces;
    using Microsoft.Extensions.DependencyInjection;
    using System.ComponentModel.Design;

    public class ConfigurationHandler : IConfiguration
    {
        
        

        private const string HEBREW_OTP_MESSAGE_PLACEHOLDER = "קוד הזיהוי שלך הוא [OTP_CODE] התקף ל-5 דקות";
        private const string ENGLISH_OTP_MESSAGE_PLACEHOLDER = "Your validation code is [OTP_CODE] and valid for the next 5 minutes";

        private readonly JwtSettings _jwtSettings;        
        private readonly IConfigurationConnector _configurationConnector;
        private readonly IProgramConnector _programConnector;
        private readonly ICompanyConnector _companyConnector;
        private readonly ISmsProviderHandler _smsProvider;        
        private readonly IFilesWrapper _filesWrapper;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public ConfigurationHandler(IConfigurationConnector configurationConnector,IProgramConnector programConnector,
            ICompanyConnector companyConnector, IOptions<JwtSettings> jwtSettings,
            ISmsProviderHandler smsProvider, IFilesWrapper filesWrapper, IServiceScopeFactory serviceScopeFactory)
        {
            _configurationConnector = configurationConnector;
            _programConnector = programConnector;
            _companyConnector = companyConnector;
            _jwtSettings = jwtSettings.Value;
            _smsProvider = smsProvider;                           
            _filesWrapper = filesWrapper;
            _serviceScopeFactory = serviceScopeFactory;


        }

        public string GetOtpMessgae(User user, Configuration appConfiguration, CompanyConfiguration companyConfiguration)
        {
            string message = ENGLISH_OTP_MESSAGE_PLACEHOLDER;

            if (user?.UserConfiguration?.Language == Language.he)
            {
                message =  HEBREW_OTP_MESSAGE_PLACEHOLDER;
            }
            return message;

        }

        public string GetAfterMessage(User user, Configuration appConfiguration, CompanyConfiguration companyConfiguration)
        {
            if (user == null)
            {
                throw new Exception("User is null");
            }
            if (user.UserConfiguration.Language == Language.he)
            {
                if (appConfiguration == null || string.IsNullOrWhiteSpace(appConfiguration.MessageAfterHebrew))
                {
                    throw new Exception("App configuation is not set correctlly , please check your DB for missing configuration, and update site configuration");
                }
            }
            else if (appConfiguration == null || string.IsNullOrWhiteSpace(appConfiguration.MessageAfter))
            {
                throw new Exception("App configuation is not set correctlly , please check your DB for missing configuration, and update site configuration");
            }

            string appConfigMessageAfter = user.UserConfiguration.Language == Language.he ? appConfiguration.MessageAfterHebrew : appConfiguration.MessageAfter;
            string companyConfigMessageAfter = companyConfiguration?.CompanyMessages?.FirstOrDefault(x => x.MessageType == MessageType.AfterSigning && x.Language == user.UserConfiguration.Language)?.Content;
            string message = string.IsNullOrWhiteSpace(companyConfigMessageAfter) ? appConfigMessageAfter : companyConfigMessageAfter;
            return _programConnector.IsFreeTrialUser(user) ? appConfigMessageAfter : message;
        }

        public string GetBeforeMessage(User user, Configuration appConfiguration, CompanyConfiguration companyConfiguration)
        {
            if (user == null)
            {
                throw new Exception("User is null");
            }
            if (user.UserConfiguration.Language == Language.he)
            {
                if (appConfiguration == null || string.IsNullOrWhiteSpace(appConfiguration.MessageBeforeHebrew))
                {
                    throw new Exception("App configuation is not set correctlly , please check your DB for missing configuration, and update site configuration");
                }
            }
            else if (appConfiguration == null || string.IsNullOrWhiteSpace(appConfiguration.MessageBefore))
            {
                throw new Exception("App configuation is not set correctlly , please check your DB for missing configuration, and update site configuration");
            }

            string appConfigMessageBefore = user.UserConfiguration.Language == Language.en ? appConfiguration.MessageBefore : appConfiguration.MessageBeforeHebrew;
            string companyConfigMessageBefore = companyConfiguration?.CompanyMessages?.FirstOrDefault(x => x.MessageType == MessageType.BeforeSigning && x.Language == user.UserConfiguration.Language)?.Content;
            string message = string.IsNullOrWhiteSpace(companyConfigMessageBefore) ? appConfigMessageBefore : companyConfigMessageBefore;


            return _programConnector.IsFreeTrialUser(user) ? appConfigMessageBefore : message;
        }

        public string GetDocumentIsAboutToBeDeletedMessage(Language language)
        {
            string hebrewMessage = "הקבצים הבאים עתידים להימחק בקרוב וטרם נחתמו סופית:";
            string englishMessage = "The following documents are about to be deleted soon and still haven't been signed:";

            if (language == Language.he)
                return hebrewMessage;
            return englishMessage;
        }

        public string GetVideoConfrenceSmsMessage(Language language)
        {
            string hebrewMessage = "להלן קישור לשיחת וידאו (היוועדות חזותית) - [DOCUMENT_NAME]: [LINK]";
            string englishMessage = "Link to the video conference – [DOCUMENT_NAME]: [LINK]";

            if (language == Language.he)
                return hebrewMessage;
            return englishMessage;
        }

        public string GetShareDocument(Language language)
        {
            string hebrewMessage = "שלום [CONTACT_NAME], [SENDER_NAME] שיתפ/ה מסמך איתך: [DOCUMENT_NAME]. [LINK]";
            string englishMessage = "Hello [CONTACT_NAME], [SENDER_NAME] Shared a document with you: [DOCUMENT_NAME]. [LINK]";

            if (language == Language.he)
                return hebrewMessage;
            return englishMessage;
        }

        public int GetDocumentsDeletionInterval(Configuration appConfiguration, Company company, DocumentStatus documentStatus)
        {
            if (appConfiguration == null)
            {
                throw new Exception("App configuation is not set correctlly, please check your DB for missing configuration, and update site configuration");
            }

            int _appConfInterval = (documentStatus == DocumentStatus.Signed
                ? appConfiguration?.DocumentDeletionConfiguration?.DeleteSignedDocumentAfterXDays
                : appConfiguration?.DocumentDeletionConfiguration?.DeleteUnsignedDocumentAfterXDays)
                ?? 0;

            int _companyInterval = (documentStatus == DocumentStatus.Signed
                ? company?.CompanyConfiguration?.DocumentDeletionConfiguration?.DeleteSignedDocumentAfterXDays
                : company?.CompanyConfiguration?.DocumentDeletionConfiguration?.DeleteUnsignedDocumentAfterXDays)
                ?? 0;

            return
                company?.ProgramId != Consts.FREE_ACCOUNTS_COMPANY_ID && (_companyInterval > 0 || _companyInterval == -1)
                ? _companyInterval
                : _appConfInterval;


        }

        public int GetSignerLinkExperationTimeInHours(User user, CompanyConfiguration companyConfiguration)
        {
            return !_programConnector.IsFreeTrialUser(user) && companyConfiguration?.SignerLinkExpirationInHours > 0 ?
                companyConfiguration.SignerLinkExpirationInHours :
                _jwtSettings.SignerLinkExpirationInHours;
        }

        public async Task<SmsConfiguration> GetSmsConfiguration(User user, Configuration appConfiguration, CompanyConfiguration companyConfiguration)
        {
            if (appConfiguration == null || !IsValidSmsConfiguration(appConfiguration.SmsConfiguration))
            {
                throw new Exception("App configuration is not set correctly , please check your DB for missing configuration, and update site configuration");
            }
            var companySmsConfiguration = GetCompanySmsConfiguration(companyConfiguration, appConfiguration.SmsConfiguration);
            var smsConfig = _programConnector.IsFreeTrialUser(user) ? appConfiguration.SmsConfiguration :
                             companySmsConfiguration != null && IsValidSmsCompanyConfiguration(companySmsConfiguration, appConfiguration.SmsConfiguration, user) ?
                             companySmsConfiguration : appConfiguration.SmsConfiguration;
            smsConfig.Language = await GetLanguage(user);
            smsConfig.IsProviderSupportGloballySend = smsConfig.Provider == ProviderType.SmsClickSend || smsConfig.Provider == ProviderType.SmsPayCall
                || smsConfig.Provider == ProviderType.SmsTwilio;

            return smsConfig;
        }

        public async Task<ISmsProvider> GetSmsProviderHandler(User user, Configuration appConfiguration, CompanyConfiguration companyConfiguration)
        {
            var smsConfiguration = await GetSmsConfiguration(user, appConfiguration, companyConfiguration);
            var smsProvider = _smsProvider.ExecuteCreation(smsConfiguration.Provider);
            return smsProvider;
        }

        public SmtpConfiguration GetSmtpConfiguration(User user, Configuration appConfiguration, CompanyConfiguration companyConfiguration)
        {
            var companySmtpConfiguration = GetSmtpConfiguration(companyConfiguration);
            var smtpConfig = _programConnector.IsFreeTrialUser(user) ? appConfiguration.SmtpConfiguration :
                             companySmtpConfiguration != null && IsValidSmtpConfiguration(companySmtpConfiguration) ? companySmtpConfiguration : appConfiguration.SmtpConfiguration;

            return smtpConfig;
        }

        /// <summary>
        /// Check if should send by priority (there are 4 levels): 
        /// document level -> user level -> company level -> app level (which is true by default)
        /// </summary>
        /// <param name="user"></param>
        /// <param name="companyConfiguration"></param>
        /// <param name="documentNotifications"></param>
        /// <returns></returns>
        public bool ShouldSendSignedDocument(User user, CompanyConfiguration companyConfiguration, DocumentNotifications documentNotifications)
        {
            return documentNotifications?.ShouldSendSignedDocument ??
                              user?.UserConfiguration?.ShouldSendSignedDocument ??
                              companyConfiguration?.ShouldSendSignedDocument ?? true;

        }
        public async Task<Signer1Configuration> GetSigner1Configuration(CompanySigner1Details companySigner1Details)
        {
            // company configuration
            if (companySigner1Details.Signer1Configuration != null && !string.IsNullOrWhiteSpace(companySigner1Details.Signer1Configuration.Endpoint))
                return companySigner1Details.Signer1Configuration;

            // app configuration
            return (await _configurationConnector.Read()).Signer1Configuration;
        }


        public void SetCompanyLogo(User user, string base64Logo)
        {
            if (user == null)
            {
                throw new Exception($"Null input - user is null");
            }
            _filesWrapper.Configurations.SaveCompanyLogo(user, base64Logo);
           
        }

        public string GetCompanyLogo(Guid companyId)
        {
            return _filesWrapper.Configurations.GetCompanyLogo(companyId);            
        }

        //TODO - chack all nameing in notification model is ok , 
        // if needed add should send , which responsvable to send document or not to signers 
        public bool ShouldNotifyWhileSignerSigned(User user, CompanyConfiguration companyConfiguration, DocumentNotifications documentNotifications)
        {
            return documentNotifications?.ShouldNotifyWhileSignerSigned ??
                               user?.UserConfiguration?.ShouldNotifyWhileSignerSigned ??
                               companyConfiguration?.ShouldNotifyWhileSignerSigned ?? true;
        }

        public string GetCompanyEmailHtml(Guid companyId, MessageType messageType)
        {
            return _filesWrapper.Configurations.GetCompanyEmailTemplate(new Company { Id = companyId }, messageType);
            
        }

        public void UpdateCompanyEmailHtml(User user, EmailHtmlBodyTemplates emailHtmlBodyTemplates)
        {
            if (user == null)
            {
                throw new Exception($"Null input - user is null");
            }
            _filesWrapper.Configurations.UpdateCompanyEmailHtml(user, emailHtmlBodyTemplates);
           
        }

        public void DeleteCompanyLogo(User user)
        {
            if (user == null)
            {
                throw new Exception($"Null input - user is null");
            }
            _filesWrapper.Configurations.DeleteCompanyLogo(user);

           
        }

        public void DeleteCompanyEmailHtml(User user)
        {
            if (user == null)
            {
                throw new Exception($"Null input - user is null");
            }
            _filesWrapper.Configurations.DeleteCompanyEmailHtml(user);
           
        }

        public async Task< Language> GetLanguage(User user)
        {
            if (user == null)
            {
                throw new Exception($"Null input - user is null");
            }
            if (user?.UserConfiguration != null)
            {
                return user.UserConfiguration.Language;
            }
            var company = await _companyConnector.Read(new Company { Id = user.CompanyId });
            return company?.CompanyConfiguration?.Language ?? Language.en;
        }

        public  Task<Configuration> ReadAppConfiguration()
        {
            return  _configurationConnector.Read();
        }

        public async Task UpdateSyncTemplates(bool syncTemplates)
        {
            var appConfig = await ReadAppConfiguration();
            appConfig.IsTemplatesSyncedMandatoryFields = syncTemplates;
            await _configurationConnector.Update(appConfig);
        }

        public async Task<IEnumerable<Tablet>> ReadTablesConfiguration(string key)
        {

            using var scope = _serviceScopeFactory.CreateScope();
            IUsers usersService = scope.ServiceProvider.GetService<IUsers>();
            (User user,  _) = await usersService.GetUser();            
            Guid companyId = user.CompanyId;
            var company = await _companyConnector.Read(new Company { Id = companyId });
            if(company == null)
            {
                return null;
            }
            var config = await ReadAppConfiguration();            
            if (config.EnableTabletsSupport && 
                company.CompanyConfiguration.EnableTabletsSupport)
            {
                return _configurationConnector.ReadTablets(key, companyId);
            }

            return null;
        }


        #region Private Function


        private SmtpConfiguration GetSmtpConfiguration(CompanyConfiguration companyConfiguration)
        {
            MessageProvider smtpMessageProvider = companyConfiguration?.MessageProviders?.FirstOrDefault(x => (int)x.ProviderType >= 10);
            return smtpMessageProvider == null ? null : new SmtpConfiguration()
            {
                From = smtpMessageProvider.From,
                Password = smtpMessageProvider.Password,
                User = smtpMessageProvider.User,
                Port = smtpMessageProvider.Port,
                Server = smtpMessageProvider.Server,
                EnableSsl = smtpMessageProvider.EnableSsl
            };
        }

        private SmsConfiguration GetCompanySmsConfiguration(CompanyConfiguration companyConfiguration, SmsConfiguration appSmsConfiguration)
        {
            var smsMessageProvider = companyConfiguration?.MessageProviders?.FirstOrDefault(x => x.ProviderType > 0 && ((int)x.ProviderType < (int)ProviderType.EmailSmtp || x.ProviderType == ProviderType.SmsCenter) && 
            !string.IsNullOrWhiteSpace(x.User));
            return smsMessageProvider == null ? null : new SmsConfiguration()
            {
                From = smsMessageProvider.From,
                Password = appSmsConfiguration.Provider == smsMessageProvider.ProviderType && string.IsNullOrWhiteSpace(smsMessageProvider.Password) ?
                appSmsConfiguration.Password : smsMessageProvider.Password,
                Provider = smsMessageProvider.ProviderType,
                User = appSmsConfiguration.Provider == smsMessageProvider.ProviderType && string.IsNullOrWhiteSpace(smsMessageProvider.User) ? appSmsConfiguration.User : smsMessageProvider.User
            };
        }

        private bool IsValidSmtpConfiguration(SmtpConfiguration smtpConfiguration)
        {
            if (string.IsNullOrWhiteSpace(smtpConfiguration.Server) ||
                string.IsNullOrWhiteSpace(smtpConfiguration.From) ||
                smtpConfiguration.Port <= 0)
            {
                return false;
            }
            return true;
        }

        private bool IsValidSmsConfiguration(SmsConfiguration smsConfiguration)
        {
            if (



                (string.IsNullOrWhiteSpace(smsConfiguration.Password) && smsConfiguration.Provider != ProviderType.SmsMicropay) ||
                string.IsNullOrWhiteSpace(smsConfiguration.User) ||
                string.IsNullOrWhiteSpace(smsConfiguration.From))
            {
                return false;
            }
            return true;
        }

        private bool IsValidSmsCompanyConfiguration(SmsConfiguration companySmsConfiguration, SmsConfiguration appSmsConfiguration, User user)
        {
            if (IsValidSmsConfiguration(appSmsConfiguration))
            {
                return IsValidSmsConfiguration(companySmsConfiguration);

            }
            throw new Exception($"Sms configuation is not set correctlly. Please check your DB for missing configuration, and update site configuration. In addition, check company [{user.CompanyId}] sms configutration.");

        }


        #endregion
    }
}