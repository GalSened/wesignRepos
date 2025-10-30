using Common.Enums;
using Common.Models.Configurations;
using System.Collections.Generic;
using System.Linq;

namespace WeSignManagement.Models.Companies
{
    public class SmsConfigurationDTO
    {

        public string From { get; set; }
        public ProviderType Provider { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public SmsConfigurationDTO(IEnumerable<MessageProvider> messageProviders)
        {
            var messageProvider = messageProviders?.FirstOrDefault(x => x.User != null &&   x.User?.Trim() != "" && x.ProviderType != ProviderType.EmailSmtp);
            if (messageProvider == null)
            {
                From = "";
                Provider = ProviderType.SmsGoldman;
                User = "";
                Password = "";
            }
            else
            {
                From = messageProvider?.From;
                Provider = messageProvider?.ProviderType ?? ProviderType.SmsGoldman;
                User = messageProvider?.User;
                Password = messageProvider?.Password;
            }
        }

    }
}
