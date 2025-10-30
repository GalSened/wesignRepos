namespace DAL.DAOs.Companies
{
    using Common.Consts;
    using Common.Enums.PDF;
    using Common.Enums.Users;
    using Common.Models.Configurations;
    using DAL.DAOs.Configurations;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    [Table("CompanyConfigurations")]
    public class CompanyConfigurationDAO
    {
        [Key]
        public Guid CompanyId { get; set; }
        public string SignatureColor { get; set; }
        public bool ShouldSendSignedDocument { get; set; }
        public bool ShouldNotifyWhileSignerSigned { get; set; }
        public bool ShouldEnableSignReminders { get; set; }
        public int SignReminderFrequencyInDays { get; set; }
        public bool CanUserControlReminderSettings { get; set; }
        public Language Language { get; set; }
        public int DeleteSignedDocumentAfterXDays { get; set; }
        public int DeleteUnsignedDocumentAfterXDays { get; set; }
        public int SignerLinkExpirationInDays { get; set; }
        public virtual ICollection<MessageProviderDAO> MessageProviders { get; set; }
        public virtual ICollection<CompanyMessageDAO> CompanyMessages { get; set; }
        public bool ShouldSendWithOTPByDefault { get; set; }
        public bool EnableVisualIdentityFlow { get; set; }
        public bool? EnableDisplaySignerNameInSignature { get; set; } = true;
        public bool isPersonalizedPFX { get; set; }
        public bool ShouldSendDocumentNotifications { get; set; }
        public string DocumentNotificationsEndpoint { get; set; }
        public bool ShouldForceOTPInLogin { get; set; }
        public bool ShouldEnableMeaningOfSignatureOption { get; set; }
        public bool ShouldEnableVideoConference { get; set; }
        public bool ShouldAddAppendicesAttachmentsToSendMail { get; set; }
        public SignatureFieldType DefaultSigningType { get; set; } = SignatureFieldType.Graphic;
        public int RecentPasswordsAmount { get; set; }
        public int PasswordExpirationInDays { get; set; }
        public int MinimumPasswordLength { get; set; }
        public bool EnableTabletsSupport { get; set; }
        //public bool ShouldEnableGovernmentSignatureFormat { get; set; }
        public virtual CompanyDAO Company { get; set; }

        public CompanyConfigurationDAO() { }

        public CompanyConfigurationDAO(CompanyConfiguration companyConfiguration)
        {
            SignatureColor = companyConfiguration.SignatureColor;
            ShouldSendSignedDocument = companyConfiguration.ShouldSendSignedDocument;
            ShouldNotifyWhileSignerSigned = companyConfiguration.ShouldNotifyWhileSignerSigned;
            ShouldEnableSignReminders = companyConfiguration.ShouldEnableSignReminders;
            SignReminderFrequencyInDays = companyConfiguration.SignReminderFrequencyInDays;
            CanUserControlReminderSettings = companyConfiguration.CanUserControlReminderSettings;
            SignerLinkExpirationInDays = companyConfiguration.SignerLinkExpirationInHours;
            Language = companyConfiguration.Language;
            DeleteSignedDocumentAfterXDays = companyConfiguration.DocumentDeletionConfiguration == null ? Consts.NEVER :
            companyConfiguration.DocumentDeletionConfiguration.DeleteSignedDocumentAfterXDays;
            DeleteUnsignedDocumentAfterXDays = companyConfiguration.DocumentDeletionConfiguration == null ? Consts.NEVER : companyConfiguration.DocumentDeletionConfiguration.DeleteUnsignedDocumentAfterXDays;
            MessageProviders = GetMessageProviders(companyConfiguration.MessageProviders);
            CompanyMessages = GetCompanyMessages(companyConfiguration.CompanyMessages);
            ShouldSendWithOTPByDefault = companyConfiguration.ShouldSendWithOTPByDefault;
            EnableVisualIdentityFlow = companyConfiguration.EnableVisualIdentityFlow;
            EnableDisplaySignerNameInSignature = companyConfiguration.EnableDisplaySignerNameInSignature;
            DefaultSigningType = companyConfiguration.DefaultSigningType;
            isPersonalizedPFX = companyConfiguration.IsPersonzliedPFX;
            ShouldSendDocumentNotifications = companyConfiguration.ShouldSendDocumentNotifications;
            DocumentNotificationsEndpoint = companyConfiguration.DocumentNotificationsEndpoint;
            ShouldForceOTPInLogin = companyConfiguration.ShouldForceOTPInLogin;
            ShouldEnableMeaningOfSignatureOption = companyConfiguration.ShouldEnableMeaningOfSignatureOption;
            ShouldEnableVideoConference = companyConfiguration.ShouldEnableVideoConference;
            RecentPasswordsAmount = companyConfiguration.RecentPasswordsAmount;
            PasswordExpirationInDays = companyConfiguration.PasswordExpirationInDays;
            MinimumPasswordLength = companyConfiguration.MinimumPasswordLength;
            ShouldAddAppendicesAttachmentsToSendMail = companyConfiguration.ShouldAddAppendicesAttachmentsToSendMail;
            EnableTabletsSupport = companyConfiguration.EnableTabletsSupport;
            //ShouldEnableGovernmentSignatureFormat = companyConfiguration.ShouldEnableGovernmentSignatureFormat;
        }

    private ICollection<CompanyMessageDAO> GetCompanyMessages(IEnumerable<CompanyMessage> companyMessages)
        {
            var result = new List<CompanyMessageDAO>();
            foreach (var item in companyMessages)
            {
                result.Add(new CompanyMessageDAO(item));
            }
            return result;
        }

        private ICollection<MessageProviderDAO> GetMessageProviders(IEnumerable<MessageProvider> messageProviders)
        {
            var result = new List<MessageProviderDAO>();
            foreach (var item in messageProviders ?? Enumerable.Empty<MessageProvider>())
            {
                result.Add(new MessageProviderDAO(item));
            }
            return result;
        }
    }
}