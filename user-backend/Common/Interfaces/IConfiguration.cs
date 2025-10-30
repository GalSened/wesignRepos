
namespace Common.Interfaces
{
    using Common.Enums;
    using Common.Enums.Documents;
    using Common.Enums.Users;
    using Common.Interfaces.MessageSending.Sms;
    using Common.Models;
    using Common.Models.Configurations;
    using Common.Models.Documents;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IConfiguration
    {
        int GetSignerLinkExperationTimeInHours(User user, CompanyConfiguration companyConfiguration);
        string GetBeforeMessage(User user, Configuration appConfiguration, CompanyConfiguration companyConfiguration);
        string GetAfterMessage(User user, Configuration appConfiguration, CompanyConfiguration companyConfiguration);
        string GetDocumentIsAboutToBeDeletedMessage(Language language);
        string GetShareDocument(Language language);
        string GetVideoConfrenceSmsMessage(Language language);
        string GetOtpMessgae(User user, Configuration appConfiguration, CompanyConfiguration companyConfiguration);
        Task<SmsConfiguration> GetSmsConfiguration(User user, Configuration appConfiguration, CompanyConfiguration companyConfiguration);
        Task<Configuration> ReadAppConfiguration();
        Task UpdateSyncTemplates(bool syncTemplates);
        Task<IEnumerable<Tablet>> ReadTablesConfiguration(string key);
        SmtpConfiguration GetSmtpConfiguration(User user, Configuration appConfiguration, CompanyConfiguration companyConfiguration);
        Task<ISmsProvider> GetSmsProviderHandler(User user, Configuration appConfiguration, CompanyConfiguration companyConfiguration);
        /// <summary>
        /// Check if should send by priority (there are 4 levels): 
        /// document level -> user level -> company level -> app level (which is true by default)
        /// </summary>
        /// <param name="user"></param>
        /// <param name="companyConfiguration"></param>
        /// <param name="documentNotifications"></param>
        /// <returns></returns>
        bool ShouldSendSignedDocument(User user, CompanyConfiguration companyConfiguration, DocumentNotifications documentNotifications);
        /// <summary>
        /// Check if should notify by priority (there are 4 levels): 
        /// document level -> user level -> company level -> app level (which is true by default)
        /// </summary>
        /// <param name="user"></param>
        /// <param name="companyConfiguration"></param>
        /// <param name="documentNotifications"></param>
        /// <returns></returns>
        bool ShouldNotifyWhileSignerSigned(User user, CompanyConfiguration companyConfiguration, DocumentNotifications documentNotifications);

     
        Task<Signer1Configuration> GetSigner1Configuration(CompanySigner1Details companySigner1Details);
 

        /// <summary>
        /// return company logo base64image if exist, if not return ""
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        string GetCompanyLogo(Guid companyId);
        /// <summary>
        /// Save company logo image in system
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        void SetCompanyLogo(User user, string base64Logo);
        void DeleteCompanyLogo(User user);

        /// <summary>
        /// return company email base64image if exist, if not return ""
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        string GetCompanyEmailHtml(Guid companyId, MessageType messageType);
        void UpdateCompanyEmailHtml(User user, EmailHtmlBodyTemplates emailHtmlBodyTemplates);
        void DeleteCompanyEmailHtml(User user);
        Task<Language> GetLanguage(User user);
        int GetDocumentsDeletionInterval(Configuration appConfiguration, Company company, DocumentStatus documentStatus);
    }
}
