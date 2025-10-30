using Common.Models.Configurations;
using System.Collections.Generic;

namespace WeSignManagement.Models.Configurations
{
    public class AppConfigurationDTO
    {
        public string MessageAfter { get; set; }
        public string MessageBefore { get; set; }
        public string MessageAfterHebrew { get; set; }
        public string MessageBeforeHebrew { get; set; }

        public int DeleteSignedDocumentAfterXDays { get; set; }
        public int DeleteUnsignedDocumentAfterXDays { get; set; }

        public string SmsFrom { get; set; }
        public string SmsPassword { get; set; }
        public int SmsProvider { get; set; }
        public int SmsLanguage { get; set; }
        public string SmsUser { get; set; }

        public int SmtpAttachmentMaxSize { get; set; }
        public bool SmtpEnableSsl { get; set; }
        public string SmtpFrom { get; set; }
        public string SmtpPassword { get; set; }
        public int SmtpPort { get; set; }
        public string SmtpServer { get; set; }
        public string SmtpUser { get; set; }

        public int LogArichveIntervalInDays { get; set; }

        public bool UseManagementOtpAuth { get; set; }
        public bool EnableFreeTrailUsers { get; set; }
        public bool EnableTabletsSupport { get; set; }
        public bool EnableSigner1ExtraSigningTypes { get; set; }
        public string Signer1Endpoint { get; set; }
        public string Signer1User { get; set; }
        public string Signer1Password { get; set; }

        public ActiveDirecrotyConfigurationDTO ActiveDirecrotyConfiguration { get; set; }
        public bool ShouldUseReCaptchaInRegistration { get; set; }
        public bool ShouldUseSignerAuth { get; set; }
        public bool ShouldUseSignerAuthDefault { get; set; }
        public bool EnableShowSSOOnlyInUserUI { get; set; }
        public bool EnableVisualIdentityFlow { get; set; }
        public string VisualIdentityURL { get; set; }
        public string VisualIdentityUser { get; set; }
        public string VisualIdentityPassword { get; set; }
        public bool ShouldSendWithOTPByDefault { get; set; }
        public string ExternalPdfServiceAPIKey { get; set; }
        public string ExternalPdfServiceURL { get; set; }
        public string HistoryIntegratorServiceAPIKey { get; set; }
        public string HistoryIntegratorServiceURL { get; set; }

        public bool UseExternalGraphicSignature { get; set; }
        public string ExternalGraphicSignatureSigner1Url { get; set; }        
        public string ExternalGraphicSignatureCert { get; set; }
        public string ExternalGraphicSignaturePin { get; set; }


        public bool ReturnActivationLinkInAPIResponse { get; set; }
        public bool ShouldReturnActivationLinkInAPIResponse { get;  set; }

        public int RecentPasswordsAmount { get; set; }

        public AppConfigurationDTO(Configuration config)
        {
            MessageBefore = config?.MessageBefore;
            MessageAfter = config?.MessageAfter;
            MessageBeforeHebrew = config?.MessageBeforeHebrew;
            MessageAfterHebrew = config?.MessageAfterHebrew;
            DeleteSignedDocumentAfterXDays = config?.DocumentDeletionConfiguration.DeleteSignedDocumentAfterXDays ?? 0;
            DeleteUnsignedDocumentAfterXDays = config?.DocumentDeletionConfiguration.DeleteUnsignedDocumentAfterXDays ?? 0;
            SmtpServer = config?.SmtpConfiguration.Server;
            SmtpPort = config?.SmtpConfiguration.Port ?? 0;
            SmtpFrom = config?.SmtpConfiguration.From;
            SmtpUser = config?.SmtpConfiguration.User;
            SmtpPassword = config?.SmtpConfiguration.Password;
            SmtpEnableSsl = config?.SmtpConfiguration.EnableSsl ?? false;
            SmtpAttachmentMaxSize = config?.SmtpConfiguration?.AttachmentMaxSize ?? 0;
            SmsFrom = config?.SmsConfiguration.From;
            SmsLanguage = config?.SmsConfiguration?.Language != null ? (int)config?.SmsConfiguration?.Language : 0;
            SmsProvider = config?.SmsConfiguration?.Provider != null ? (int)config?.SmsConfiguration?.Provider : 0;
            SmsUser = config?.SmsConfiguration.User;
            SmsPassword = config?.SmsConfiguration.Password;
            LogArichveIntervalInDays = config?.LogArichveIntervalInDays ?? 0;
            UseManagementOtpAuth = config?.UseManagementOtpAuth ?? false;
            EnableShowSSOOnlyInUserUI = config?.EnableShowSSOOnlyInUserUI ?? false;
            EnableFreeTrailUsers = config?.EnableFreeTrailUsers ?? false;
            EnableTabletsSupport = config?.EnableTabletsSupport ?? false;
            EnableSigner1ExtraSigningTypes = config?.EnableSigner1ExtraSigningTypes ?? false;
            ShouldUseReCaptchaInRegistration = config?.ShouldUseReCaptchaInRegistration ?? false;
            ShouldUseSignerAuth = config?.ShouldUseSignerAuth ?? false;
            ShouldUseSignerAuthDefault = config?.ShouldUseSignerAuthDefault ?? false;
            Signer1Endpoint = config?.Signer1Configuration?.Endpoint;
            Signer1User = config?.Signer1Configuration?.User;
            Signer1Password = config?.Signer1Configuration?.Password;
            EnableVisualIdentityFlow = config?.EnableVisualIdentityFlow ?? false;
            ShouldSendWithOTPByDefault = config?.SendWithOTPByDefault ?? false;
            VisualIdentityURL = config?.VisualIdentityURL;
            VisualIdentityUser = config?.VisualIdentityUser;
            VisualIdentityPassword = config?.VisualIdentityPassword;
            ExternalPdfServiceURL = config?.ExternalPdfServiceURL;
            ExternalPdfServiceAPIKey = config?.ExternalPdfServiceAPIKey;
            HistoryIntegratorServiceURL = config?.HistoryIntegratorServiceURL;
            HistoryIntegratorServiceAPIKey = config?.HistoryIntegratorServiceAPIKey;
            ShouldReturnActivationLinkInAPIResponse  = config?.ShouldReturnActivationLinkInAPIResponse ?? false;
            UseExternalGraphicSignature = config?.UseExternalGraphicSignature ?? false;
            ExternalGraphicSignatureSigner1Url = config?.ExternalGraphicSignatureSigner1Url;
            ExternalGraphicSignatureCert = config?.ExternalGraphicSignatureCert;
            ExternalGraphicSignaturePin = config?.ExternalGraphicSignaturePin;
            RecentPasswordsAmount = config?.RecentPasswordsAmount ?? 3;
            ActiveDirecrotyConfiguration = new ActiveDirecrotyConfigurationDTO
            {
                Container = config?.ActiveDirectoryConfiguration?.Container,
                Domain = config?.ActiveDirectoryConfiguration?.Domain,
                Host = config?.ActiveDirectoryConfiguration?.Host,
                Password = config?.ActiveDirectoryConfiguration?.Password,
                Port = config?.ActiveDirectoryConfiguration?.Port ?? 0,
                User = config?.ActiveDirectoryConfiguration?.User
            };

        }


    }
}
