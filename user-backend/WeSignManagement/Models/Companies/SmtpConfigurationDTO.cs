using Common.Models.Configurations;
using System.Collections.Generic;
using System.Linq;

namespace WeSignManagement.Models.Companies
{
    public class SmtpConfigurationDTO
    {
        public string BeforeSigningHtmlTemplateBase64String { get; set; }
        public string AfterSigningHtmlTemplateBase64String { get; set; }
        public string SmtpFrom { get; set; }
        public string SmtpServer { get; set; }
        public string SmtpPort { get; set; }
        public string SmtpUser { get; set; }
        public string SmtpPassword { get; set; }
        public bool SmtpEnableSsl { get; set; }

        public SmtpConfigurationDTO(IEnumerable<MessageProvider> messageProviders, EmailHtmlBodyTemplates emailHtmlBodyTemplates)
        {
            var messageProvider =  messageProviders?.FirstOrDefault(x => x.ProviderType == Common.Enums.ProviderType.EmailSmtp);
            SmtpFrom = messageProvider?.From;
            SmtpServer = messageProvider?.Server;
            SmtpPort = messageProvider?.Port.ToString();
            SmtpUser = messageProvider?.User;
            SmtpPassword = messageProvider?.Password;
            SmtpEnableSsl = messageProvider?.EnableSsl ?? false;
            BeforeSigningHtmlTemplateBase64String = emailHtmlBodyTemplates?.BeforeSigningBase64String;
            AfterSigningHtmlTemplateBase64String = emailHtmlBodyTemplates?.AfterSigningBase64String;
        }

    }
}
