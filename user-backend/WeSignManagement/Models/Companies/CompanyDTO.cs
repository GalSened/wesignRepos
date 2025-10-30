using Common.Enums.PDF;
using Common.Enums.Users;
using System;

using System.Collections.Generic;
using WeSignManagement.Models.Companies.Responses;
using WeSignManagement.Models.Users;

namespace WeSignManagement.Models.Companies
{

    public class CompanyDTO
    {
        public string CompanyName { get; set; }
        public string LogoBase64String { get; set; }
        public string SignatureColor { get; set; }
        public Language Language { get; set; }
        public BaseUserManagementDTO User { get; set; }
        public string ProgramId { get; set; }
        public DateTime ExpirationTime { get; set; }
        public string MessageBefore { get; set; }
        public string MessageAfter { get; set; }
        public string MessageBeforeHebrew { get; set; }
        public string MessageAfterHebrew { get; set; }
        public bool ShouldSendWithOTPByDefault { get; set; }
        public bool EnableVisualIdentityFlow { get; set; }
        public bool EnableDisplaySignerNameInSignature { get; set; }
        public bool ShouldForceOTPInLogin { get; set; }
        public bool ShouldEnableMeaningOfSignatureOption { get; set; }
        public bool ShouldAddAppendicesAttachmentsToSendMail { get; set; }
        public bool ShouldEnableVideoConference { get; set; }
        
        public SignatureFieldType DefaultSigningType { get; set; }
        public SmtpConfigurationDTO SmtpConfiguration { get; set; }
        public SmsConfigurationDTO SmsConfiguration { get; set; }
        public NotificationsDTO Notifications { get; set; }
        public DeletionDTO DeletionDetails { get; set; }
        public IEnumerable< GroupsADMapperDTO> GroupsADMapper { get; set; }
        public CompanySigner1DetailsDTO CompanySigner1Details { get; set; }  
        public bool IsPersonalizedPFX { get; set; }
        public string TransactionId { get; set; }
        public int RecentPasswordsAmount { get; set; }
        public int PasswordExpirationInDays { get; set; }
        public int MinimumPasswordLength { get; set; }
        public bool EnableTabletsSupport { get; set; }
        //public bool ShouldEnableGovernmentSignatureFormat { get; set; }
    }
}
