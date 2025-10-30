namespace Common.Models.Configurations
{
    using Common.Models.Documents;
    using System.Collections.Generic;

    public class Configuration
    {
        public SmtpConfiguration SmtpConfiguration { get; set; }
        public SmsConfiguration SmsConfiguration { get; set; }
        public DocumentDeletionConfiguration DocumentDeletionConfiguration { get; set; }
        public ActiveDirectoryConfiguration ActiveDirectoryConfiguration { get; set; }
        public Signer1Configuration Signer1Configuration { get; set; }
        public string MessageBefore { get; set; }
        public string MessageAfter { get; set; }
        public string MessageBeforeHebrew { get; set; }
        public string MessageAfterHebrew { get; set; }
        public int LogArichveIntervalInDays { get; set; }
        public bool UseManagementOtpAuth { get; set; }
        public bool EnableFreeTrailUsers { get; set; }
        public bool EnableTabletsSupport { get; set; }
        public bool EnableSigner1ExtraSigningTypes { get; set; }
        public IEnumerable<Tablet> Tablets { get; set; }
        public bool ShouldUseReCaptchaInRegistration { get; set; }
        public bool ShouldUseSignerAuth { get; set; }
        public bool ShouldUseSignerAuthDefault { get; set; }
        public FileGateScannerConfiguration FileGateScannerConfiguration { get; set; }
        public bool SendWithOTPByDefault { get; set; }
        public bool EnableVisualIdentityFlow { get; set; }        
        public bool EnableRenewalPayingUserLogic { get; set; }
        public string VisualIdentityURL { get; set; }
        public string VisualIdentityUser { get; set; }
        public string VisualIdentityPassword { get; set; }
        public string ExternalPdfServiceURL { get; set; }
        public string ExternalPdfServiceAPIKey { get; set; }
        public string HistoryIntegratorServiceURL { get; set; }
        public string HistoryIntegratorServiceAPIKey { get; set; }
        public bool UseExternalGraphicSignature { get; set; }
        public string ExternalGraphicSignatureSigner1Url { get; set; }
        public string ExternalGraphicSignatureCert { get; set; }
        public string ExternalGraphicSignaturePin { get; set; }
        public bool ShouldReturnActivationLinkInAPIResponse { get; set; }
        public bool EnableShowSSOOnlyInUserUI { get; set; }
        public int RecentPasswordsAmount { get; set; }
        public bool IsTemplatesSyncedMandatoryFields { get; set; }

        public Configuration()
        {
            SmtpConfiguration = new SmtpConfiguration();
            SmsConfiguration = new SmsConfiguration();
            DocumentDeletionConfiguration = new DocumentDeletionConfiguration();
            ActiveDirectoryConfiguration = new ActiveDirectoryConfiguration();
            Signer1Configuration = new Signer1Configuration();
            FileGateScannerConfiguration = new FileGateScannerConfiguration();
        }
    }
}
