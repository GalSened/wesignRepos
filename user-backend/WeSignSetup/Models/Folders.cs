using System.Configuration;
using System.IO;

namespace WeSignSetup.Models
{
    public class Folders
    {
        public static string SignerBackendPath { get; private set; }
        public static string UserBackendPath { get; private set; }
        public static string UserSoapServicePath { get; private set; }        
        public static string ManagementBackendPath { get; private set; }
        public static string SignerFrontendPath { get; private set; }
        public static string UserFrontendPath { get; private set; }
        public static string ManagementFrontendPath { get; private set; }
        public static string BaseFolder { get; private set; }
        public static string WseAuthFolder { get; internal set; }
        public static string PdfExternalService { get; internal set; }
        public static string HistoryServiceApi { get; internal set; }
        public static void Init(string basePath)
        {
            BaseFolder = basePath;
            SignerBackendPath = Path.Combine(basePath, "SignerBackend");
            UserBackendPath = Path.Combine(basePath, "UserBackend");
            UserSoapServicePath = Path.Combine(basePath, "UserSoapService");
            ManagementBackendPath = Path.Combine(basePath, "ManagementBackend");
            SignerFrontendPath = Path.Combine(basePath, "SignerFrontend");
            UserFrontendPath = Path.Combine(basePath, "UserFrontend");
            ManagementFrontendPath = Path.Combine(basePath, "ManagementFrontend");
            WseAuthFolder = Path.Combine(basePath, "WSEAuth");
            PdfExternalService = Path.Combine(basePath, "PdfExternalService");
            HistoryServiceApi = Path.Combine(basePath, "HistoryServiceApi");
        }

    }
}


