
namespace DAL.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Common.Enums.Users;
    using Common.Models.Configurations;
    using Common.Models.Documents;
    using DAL.DAOs.Companies;
    using DAL.DAOs.Configurations;

    public static class CompanyConfigurationExtensions
    {
        public static CompanyConfiguration ToCompanyConfiguration(this CompanyConfigurationDAO companyConfigurationDAO)
        {
            var result = companyConfigurationDAO == null ? null : new CompanyConfiguration()
            {
                CompanyId = companyConfigurationDAO.CompanyId,
                ShouldNotifyWhileSignerSigned = companyConfigurationDAO.ShouldNotifyWhileSignerSigned,
                ShouldEnableSignReminders = companyConfigurationDAO.ShouldEnableSignReminders,
                SignReminderFrequencyInDays = companyConfigurationDAO.SignReminderFrequencyInDays,
                CanUserControlReminderSettings = companyConfigurationDAO.CanUserControlReminderSettings,
                Language = companyConfigurationDAO.Language,
                ShouldSendSignedDocument = companyConfigurationDAO.ShouldSendSignedDocument,
                SignatureColor = companyConfigurationDAO.SignatureColor,
                MessageProviders = ToMessageProviders(companyConfigurationDAO.MessageProviders),
                CompanyMessages = ToCompanyMessages(companyConfigurationDAO.CompanyMessages),
                SignerLinkExpirationInHours = companyConfigurationDAO.SignerLinkExpirationInDays,
                EnableVisualIdentityFlow = companyConfigurationDAO.EnableVisualIdentityFlow,
                EnableDisplaySignerNameInSignature = companyConfigurationDAO.EnableDisplaySignerNameInSignature,
                ShouldSendWithOTPByDefault = companyConfigurationDAO.ShouldSendWithOTPByDefault,
                DefaultSigningType = companyConfigurationDAO.DefaultSigningType,
                IsPersonzliedPFX = companyConfigurationDAO.isPersonalizedPFX,
                RecentPasswordsAmount = companyConfigurationDAO.RecentPasswordsAmount,
                PasswordExpirationInDays = companyConfigurationDAO.PasswordExpirationInDays,                
                MinimumPasswordLength = companyConfigurationDAO.MinimumPasswordLength,
                ShouldSendDocumentNotifications = companyConfigurationDAO.ShouldSendDocumentNotifications,
                DocumentNotificationsEndpoint = companyConfigurationDAO.DocumentNotificationsEndpoint,
                ShouldForceOTPInLogin = companyConfigurationDAO.ShouldForceOTPInLogin,
                ShouldEnableMeaningOfSignatureOption = companyConfigurationDAO.ShouldEnableMeaningOfSignatureOption,
                ShouldEnableVideoConference = companyConfigurationDAO.ShouldEnableVideoConference,
                ShouldAddAppendicesAttachmentsToSendMail = companyConfigurationDAO.ShouldAddAppendicesAttachmentsToSendMail,
                EnableTabletsSupport = companyConfigurationDAO.EnableTabletsSupport,
                //ShouldEnableGovernmentSignatureFormat = companyConfigurationDAO.ShouldEnableGovernmentSignatureFormat,
                DocumentDeletionConfiguration = new DocumentDeletionConfiguration()
                {
                    DeleteSignedDocumentAfterXDays = companyConfigurationDAO.DeleteSignedDocumentAfterXDays,
                    DeleteUnsignedDocumentAfterXDays = companyConfigurationDAO.DeleteUnsignedDocumentAfterXDays
                }
            };
            if (result != null && !Enum.IsDefined(typeof(Common.Enums.PDF.SignatureFieldType), result.DefaultSigningType))
            {
                result.DefaultSigningType = Common.Enums.PDF.SignatureFieldType.Graphic;
            }

                return result;
        }

        private static IEnumerable<CompanyMessage> ToCompanyMessages(ICollection<CompanyMessageDAO> companyMessages)
        {
            var result = new List<CompanyMessage>();
            foreach (var item in companyMessages ?? Enumerable.Empty<CompanyMessageDAO>() )
            {
                result.Add(new CompanyMessage()
                {
                    Id = item.Id,
                    CompanyId = item.Id,
                    MessageType = item.MessageType,
                    SendingMethod = item.SendingMethod,
                    Content = item.Content,
                    Language = item?.Language ?? Language.en,
                });
            }
            return result;
        }

        private static IEnumerable<MessageProvider> ToMessageProviders(ICollection<MessageProviderDAO> messageProviders)
        {
            var result = new List<MessageProvider>();
            foreach (var item in messageProviders ?? Enumerable.Empty<MessageProviderDAO>())
            {
                result.Add(new MessageProvider()
                {
                    Id = item.Id,
                    CompanyId = item.Id,
                    From = item.From,
                    User = item.User,
                    Password = item.Password,
                    Server = item.Server,
                    Port = item.Port,
                    ProviderType = item.ProviderType,
                    SendingMethod = item.SendingMethod,
                    EnableSsl = item.EnableSsl
                });
            }
            return result;
        }
    }
}
