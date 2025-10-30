using System.Collections.Generic;

namespace Common.Models.Settings
{
    public class GeneralSettings
    {
	    public string CA { get; set; }
	    public string CryptographicServiceEProvider { get; set; }
        public int DPI { get; set; }
        public string UserFronendApplicationRoute { get; set; }
        public string UserBackendEndPoint { get; set; }
        public string SignerFronendApplicationRoute { get; set; }
        public string AuthSignerFronendApplicationRoute { get; set; }        
        public string ConnectionString { get; set; }
        
        public string NotifySmsApiEndpoint { get;  set; }                
        public string PayCallSmsApiEndpoint { get; set; }
        public string LicenseDMZEndpoint { get; set; }
        public string RedirectUrl { get; set; }
        public string ProductId { get; set; }
        public string QRCodeSecret { get; set; }
        public string SmartCardDesktopClientInstallerPath { get; set; }
        public int OtpCodeExpirationMinuteTime { get; set; }
        public bool AllowSingleLink { get; set; }
        public bool DisableHangfire { get; set; }
        
        public string AgentHubEndpoint { get; set; }
        public string LibreOfficePath { get; set; }
        public int MaxUploadFileSize { get; set; }
        public string Signer1Endpoint { get; set; }
        public string ManagementAPIUrl { get; set; }
        public int MaxContentLengthBodyRequest { get; set; }
        public  int CompressFilesOverSizeBytes { get; set; }
        public bool ShouldDeleteFormFieldsBeforeCreateImage { get; set; }
        public bool SaveSignerDocTrace { get; set; }
        
        public List<Exp> IsAboutToExpiredNotificationInPercentage { get; set; }
        public bool ComsignIDPActive { get; set; }
        public string ComsignIDPURL { get; set; }
        public string ComsignIDPClientSecret { get; set; }
       public string ComsignIDPClientId{ get; set; }
        public string LogoutRoute { get; set; }
        public bool Signer1RestConnector { get; set; }
        public List<int> InvalidOperationExceptionToIgnoreInLog { get; set; } = new List<int>();
        public bool UseExternalConvertor { get; set; } = false;

        public string ExternalConvertorKey1 { get; set; }
        public bool UseMailKit { get; set; } = false;

        public bool AuthorizeHangfire { get; set; }
        public bool AddNotificationExtraInfo { get; set; } = false;
        public bool UseRabbitMQForHistoryDocuments { get; set; }
        public int PeriodicReportFileExpirationInHours { get; set; }
        public bool ShowSwaggerUI { get; set; }
    }

    public class Exp
    {
        public int OverXPercentage { get; set; }
    }
}
