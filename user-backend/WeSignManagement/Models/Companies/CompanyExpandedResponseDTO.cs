using Common.Enums;
using Common.Enums.PDF;
using Common.Enums.Users;
using Common.Models.ActiveDirectory;
using Common.Models.Configurations;
using Common.Models.ManagementApp;
using System;
using System.Collections.Generic;
using System.Linq;
using WeSignManagement.Models.Users;

namespace WeSignManagement.Models.Companies
{
    public class CompanyExpandedResponseDTO
    {
        public Guid Id { get; set; }
        public string CompanyName { get; set; }
        public IEnumerable<(Guid, string)> Groups { get; set; }
        public string LogoBase64String { get; set; }
        public string SignatureColor { get; set; }
        public Language Language { get; set; }
        public BaseUserManagementDTO User { get; set; }
        public Guid ProgramId { get; set; }
        public DateTime ExpirationTime { get; set; }
        public string MessageBefore { get; set; }
        public string MessageAfter { get; set; }
        public string MessageBeforeHebrew { get; set; }
        public string MessageAfterHebrew { get; set; }
        public bool ShouldSendWithOTPByDefault { get; set; }
        public bool ShouldForceOTPInLogin { get; set; }
        public bool ShouldEnableMeaningOfSignatureOption { get; set; }
        public bool ShouldEnableVideoConference { get; set; }
        public bool ShouldAddAppendicesAttachmentsToSendMail { get; set; }        
        public bool EnableVisualIdentityFlow { get; set; }
        public bool EnableDisplaySignerNameInSignature { get; set; }
        public bool IsPersonalizedPFX { get; set; }
        public int RecentPasswordsAmount { get; set; }
        public int PasswordExpirationInDays { get; set; }
        public int MinimumPasswordLength { get; set; }
        public SignatureFieldType DefaultSigningType { get; set; }
        public SmtpConfigurationDTO SmtpConfiguration { get; set; }
        public SmsConfigurationDTO SmsConfiguration { get; set; }
        public NotificationsDTO Notifications { get; set; }
        public DeletionDTO DeletionDetails { get; set; }
        public ICollection<ActiveDirectoryGroupDTO> ActiveDirectoryGroups { get; set; }
        public CompanySigner1Details CompanySigner1Details { get; set; }
        public string TransactionId { get; set; }
        public bool EnableTabletsSupport { get; set; }
        //public bool ShouldEnableGovernmentSignatureFormat { get; set; }
        public CompanyExpandedResponseDTO(CompanyExpandedDetails companyExpandedDetails)
        {
            Id = companyExpandedDetails.Id;

            CompanyName = companyExpandedDetails.Name;
            Groups = companyExpandedDetails.Groups;
            LogoBase64String = companyExpandedDetails.CompanyConfiguration?.Base64Logo;
            SignatureColor = companyExpandedDetails.CompanyConfiguration?.SignatureColor;
            Language = companyExpandedDetails.CompanyConfiguration?.Language ?? Language.en;
            ShouldSendWithOTPByDefault = companyExpandedDetails.CompanyConfiguration?.ShouldSendWithOTPByDefault ?? false;
            ShouldForceOTPInLogin = companyExpandedDetails.CompanyConfiguration?.ShouldForceOTPInLogin ?? false;
            ShouldEnableMeaningOfSignatureOption = companyExpandedDetails.CompanyConfiguration?.ShouldEnableMeaningOfSignatureOption ?? false;
            ShouldEnableVideoConference = companyExpandedDetails.CompanyConfiguration?.ShouldEnableVideoConference ?? false;
            EnableVisualIdentityFlow = companyExpandedDetails.CompanyConfiguration?.EnableVisualIdentityFlow ?? false;
            EnableDisplaySignerNameInSignature = companyExpandedDetails.CompanyConfiguration?.EnableDisplaySignerNameInSignature ?? false;
            IsPersonalizedPFX = companyExpandedDetails.CompanyConfiguration?.IsPersonzliedPFX ?? false;
            RecentPasswordsAmount = companyExpandedDetails.CompanyConfiguration?.RecentPasswordsAmount ?? 0;
            PasswordExpirationInDays = companyExpandedDetails.CompanyConfiguration?.PasswordExpirationInDays ?? 0;
            MinimumPasswordLength = companyExpandedDetails.CompanyConfiguration?.MinimumPasswordLength ?? 8;
            DefaultSigningType = companyExpandedDetails.CompanyConfiguration.DefaultSigningType;
            EnableTabletsSupport = companyExpandedDetails.CompanyConfiguration?.EnableTabletsSupport ?? false;
            //ShouldEnableGovernmentSignatureFormat = companyExpandedDetails.CompanyConfiguration?.ShouldEnableGovernmentSignatureFormat ?? false;
            ShouldAddAppendicesAttachmentsToSendMail = companyExpandedDetails.CompanyConfiguration?.ShouldAddAppendicesAttachmentsToSendMail?? false;
            User = new BaseUserManagementDTO
            {
                Email = companyExpandedDetails.User?.Email,
                UserUsername = companyExpandedDetails.User?.Username,
                GroupName = companyExpandedDetails.Groups.FirstOrDefault(x => x.Item1 == companyExpandedDetails.User?.GroupId).Item2,
                UserName = companyExpandedDetails.User?.Name,
            };
            ProgramId = companyExpandedDetails.ProgramId;
            ExpirationTime = companyExpandedDetails.ExpirationTime;
            var companyMessages = companyExpandedDetails.CompanyConfiguration.CompanyMessages.ToList();
            MessageAfter = companyMessages.Find(x => x.MessageType == MessageType.AfterSigning && (x.Language == 0 || x.Language == Language.en))?.Content;
            MessageBefore = companyMessages.Find(x => x.MessageType == MessageType.BeforeSigning && (x.Language == 0 || x.Language == Language.en))?.Content;
            MessageAfterHebrew = companyMessages.Find(x => x.MessageType == MessageType.AfterSigning && x.Language == Language.he)?.Content;
            MessageBeforeHebrew = companyMessages.Find(x => x.MessageType == MessageType.BeforeSigning && x.Language == Language.he)?.Content;
            SmtpConfiguration = new SmtpConfigurationDTO(companyExpandedDetails.CompanyConfiguration?.MessageProviders, companyExpandedDetails.CompanyConfiguration?.EmailTemplates);
            SmsConfiguration = new SmsConfigurationDTO(companyExpandedDetails.CompanyConfiguration?.MessageProviders);
            Notifications = new NotificationsDTO(companyExpandedDetails.CompanyConfiguration);
            DeletionDetails = new DeletionDTO(companyExpandedDetails.CompanyConfiguration?.DocumentDeletionConfiguration);
            ActiveDirectoryGroups = new List<ActiveDirectoryGroupDTO>();
            foreach (var companyExpandedDetail in companyExpandedDetails.ActiveDirectoryGroups ?? new List<ActiveDirectoryGroup>())
            {
                ActiveDirectoryGroups.Add(new ActiveDirectoryGroupDTO()
                {
                    Id = companyExpandedDetail.Id,
                    ActiveDirectoryContactsGroupName = companyExpandedDetail.ActiveDirectoryContactsGroupName,
                    ActiveDirectoryUsersGroupName = companyExpandedDetail.ActiveDirectoryUsersGroupName,
                    GroupId = companyExpandedDetail.GroupId,
                    GroupName = companyExpandedDetail.GroupName
                });
            }
            CompanySigner1Details = companyExpandedDetails?.CompanySigner1Details;

            TransactionId = companyExpandedDetails?.TransactionId;


        }
    }
}