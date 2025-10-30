/*
    ShouldSendSignedDocument = Should user need sent signed doc to customer
    ShouldNotifyWhileSignerSigned = Should user need get full mail info 
 */
namespace Common.Models.Configurations
{
    using Common.Enums.PDF;
    using Common.Enums.Users;
    using Common.Models.Documents;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class CompanyConfiguration
    {
        public Guid CompanyId { get; set; }
        public string SignatureColor { get; set; }
        public bool ShouldSendSignedDocument { get; set; }
        public bool ShouldNotifyWhileSignerSigned { get; set; }
        public bool ShouldEnableSignReminders { get; set; }
        public int SignReminderFrequencyInDays { get; set; }
        public bool CanUserControlReminderSettings { get; set; }
        public int SignerLinkExpirationInHours { get; set; }
        public Language Language { get; set; }
        public string Base64Logo { get; set; }
        public EmailHtmlBodyTemplates EmailTemplates { get; set; }
        public DocumentDeletionConfiguration DocumentDeletionConfiguration { get; set; }
        public IEnumerable<CompanyMessage> CompanyMessages { get; set; }
        public IEnumerable<MessageProvider> MessageProviders { get; set; }
        public bool ShouldSendWithOTPByDefault { get; set; }
        public bool EnableVisualIdentityFlow { get; set; }
        public bool? EnableDisplaySignerNameInSignature { get; set; }
        public SignatureFieldType DefaultSigningType { get; set; }
        public bool ShouldSendDocumentNotifications { get; set; }
        public string DocumentNotificationsEndpoint { get; set; }
        public bool ShouldForceOTPInLogin { get; set; }
        public bool ShouldEnableMeaningOfSignatureOption { get; set; }
        public bool ShouldEnableVideoConference { get; set; }
        public bool ShouldAddAppendicesAttachmentsToSendMail { get; set; }        
        public bool IsPersonzliedPFX { get; set; }
        public int RecentPasswordsAmount { get; set; }
        public int PasswordExpirationInDays { get; set; }
        public int MinimumPasswordLength { get; set; }
        public bool EnableTabletsSupport { get; set; }
        //public bool ShouldEnableGovernmentSignatureFormat { get; set; }

        public CompanyConfiguration()
        {
            CompanyMessages = Enumerable.Empty<CompanyMessage>();
            MessageProviders = Enumerable.Empty<MessageProvider>();
            DocumentDeletionConfiguration = new DocumentDeletionConfiguration();
            EmailTemplates = new EmailHtmlBodyTemplates();
        }
    }
}
