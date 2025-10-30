
using Common.Enums.PDF;
using Common.Enums.Users;
using Common.Models;
using Common.Models.Configurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WeSign.Models.Users.Responses
{
    public class ExtendedUserResponseDTO
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public Guid GroupId { get; set; }
        public string Name { get; set; }
        public string CompanyName { get; set; }
        public string GroupName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }   
        public UserType Type { get; set; }
        public DateTime CreationTime { get; set; }
        public UserConfiguration UserConfiguration { get; set; }
        public Guid? ProgramUtilizationId { get; set; }
        public ProgramResponseDTO Program { get; set; }
        public string CompanyLogo { get; set; }
        public CompanySigner1Details CompanySigner1Details { get; set; }
        public bool ShouldSendWithOTPByDefault { get; set; }
        public bool EnableVisualIdentityFlow { get; set; }
        public bool EnableVideoConferenceFlow { get; set; }
        public SignatureFieldType DefaultSigningType { get; set; }
        public string TransactionId { get; set; }
        public DateTime LastSeen { get; set; }
        public string Username { get; set;}
        public bool EnableSignReminderSettings { get; set; }
        public bool EnableMeaningOfSignature { get; set; }
        public bool EnableDisplaySignerNameInSignature { get; set; }
        public List<Guid> additionalGroupsIds { get; set; }
        public bool ShouldSignEidasSignatureFlow { get; set; }
        public bool EnableTabletsSupport { get; set; }
        //public bool ShouldEnableGovernmentSignatureFormat { get; set; }
        public ExtendedUserResponseDTO() { }

        public ExtendedUserResponseDTO(ExtendedUserInfo user, CompanySigner1Details companySigner1Details)
        {
            Id = user.Id;
            CompanyId = user.CompanyId;
            GroupId = user.GroupId;
            Name = user.Name;
            CompanyName = user.CompanyName;
            GroupName = user.GroupName;
            Email = user.Email;
            Type = user.Type;
            CreationTime = user.CreationTime;
            ProgramUtilizationId = user.ProgramUtilization?.Id;
            UserConfiguration = user.UserConfiguration;
            CompanyLogo = user.CompanyLogo;
            Phone = user.Phone;
            Program = new ProgramResponseDTO(user.ProfileProgram);
            CompanySigner1Details = companySigner1Details;
            ShouldSendWithOTPByDefault = user.ShouldSendWithOTPByDefault;
            EnableVisualIdentityFlow = user.EnableVisualIdentityFlow;
            EnableVideoConferenceFlow = user.EnableVideoConferenceFlow;
            DefaultSigningType = user.DefaultSigningType;
            TransactionId = user.TransactionId;
            LastSeen = user?.LastSeen ?? DateTime.MinValue;
            Username = user.Username;
            EnableSignReminderSettings = user.EnableSignReminderSettings;
            EnableDisplaySignerNameInSignature = user.EnableDisplaySignerNameInSignature;
            additionalGroupsIds = user?.AdditionalGroupsMapper?.Select(x => x.GroupId).ToList();
            EnableMeaningOfSignature = user.EnableMeaningOfSignature;
            ShouldSignEidasSignatureFlow = user.ShouldSignEidasSignatureFlow;
            EnableTabletsSupport = user.EnableTabletsSupport;
            //ShouldEnableGovernmentSignatureFormat = user.ShouldEnableGovernmentSignatureFormat;
        }
    }
}

