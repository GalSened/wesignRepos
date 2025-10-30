using Common.Enums.PDF;
using Common.Models.Configurations;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Models
{
    public class ExtendedUserInfo: User
    {
        public ExtendedUserInfo(User user)
        {
            CompanyId = user.CompanyId;
            Id = user.Id;
            CompanyId = user.CompanyId;
            Name = user.Name;
            CompanyName = user.CompanyName;
            GroupName = user.GroupName;
            Email = user.Email;
            Username = user.Username;
            Password = user.Password;
            Type = user.Type;
            Status = user.Status;
            CreationSource = user.CreationSource;
            CreationTime = user.CreationTime;
            GroupId = user.GroupId;
            ProgramUtilizationId = user.ProgramUtilizationId;
            ProgramUtilization = user.ProgramUtilization;
            UserConfiguration = user.UserConfiguration;
            UserTokens = user.UserTokens;
            ProfileProgram  = user.ProfileProgram;
            CompanyLogo = user.CompanyLogo;
            AdditionalGroupsMapper =  user.AdditionalGroupsMapper;
            Phone = user.Phone;

    }
      
        public bool ShouldSignEidasSignatureFlow { get; set; }
        public bool EnableMeaningOfSignature { get; set; }
        public bool ShouldSendWithOTPByDefault { get; set; }
        public bool EnableVisualIdentityFlow { get; set; }
        public bool EnableVideoConferenceFlow { get; set; }
        
        public SignatureFieldType DefaultSigningType { get; set; }
        public string TransactionId { get; set; }

        public bool EnableSignReminderSettings { get; set; }
        public bool EnableDisplaySignerNameInSignature { get; set; }
        public bool EnableTabletsSupport { get; set; }
        //public bool ShouldEnableGovernmentSignatureFormat { get; set; }
    }
}
